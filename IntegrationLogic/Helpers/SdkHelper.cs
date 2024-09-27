using System.ServiceModel;
using IntegrationLogic.Models;
using schemas.appliedsystems.com.epic.sdk._2009._07._common._lookup;
using schemas.appliedsystems.com.epic.sdk._2009._07._get;
using schemas.appliedsystems.com.epic.sdk._2011._01._account;
using schemas.appliedsystems.com.epic.sdk._2011._01._account._policy._action;
using schemas.appliedsystems.com.epic.sdk._2011._01._account._transaction._action;
using schemas.appliedsystems.com.epic.sdk._2011._01._get;
using schemas.appliedsystems.com.epic.sdk._2017._02._account._policy;
using SDK2009 = schemas.appliedsystems.com.epic.sdk._2009._07;
using schemas.appliedsystems.com.epic.sdk._2011._01._generalledger;
using schemas.appliedsystems.com.epic.sdk._2016._01._get;
using schemas.appliedsystems.com.epic.sdk._2017._02._account;
using schemas.appliedsystems.com.epic.sdk._2011._01._common;
using schemas.appliedsystems.com.epic.sdk._2011._01._generalledger._disbursement._action;
using schemas.appliedsystems.com.epic.sdk._2011._01._generalledger._receipt._detail;
using schemas.appliedsystems.com.epic.sdk._2011._01._get._policyfilter;
using schemas.appliedsystems.com.epic.sdk._2011._12._account;
using schemas.appliedsystems.com.epic.sdk._2011._12._account._common;
using schemas.appliedsystems.com.epic.sdk._2014._11._account;
using schemas.appliedsystems.com.epic.sdk._2019._01._account;
using schemas.appliedsystems.com.epic.sdk._2009._07._common;
using schemas.appliedsystems.com.epic.sdk._2011._12._configure._transaction;
using EpicTransaction = schemas.appliedsystems.com.epic.sdk._2011._01._account.Transaction;

namespace IntegrationLogic.Helpers;

public class SdkHelper : IDisposable
{
    #region Properties

    private EpicSDK_2021_02Client _service = null!;

    private SDK2009.MessageHeader _header = null!;

    private readonly string _url;

    private readonly string _databaseName;

    private readonly string _authenticationKey;

    #endregion

    #region Constructor

    public SdkHelper(string url, string databaseName, string authenticationKey, string user = "")
    {
        _url = url;
        _databaseName = databaseName;
        _authenticationKey = authenticationKey;
        SetupSdk(user);
    }

    #endregion

    #region Setup

    // Method for creating the service client
    public void SetupSdk(string user)
    {
        EndpointAddress endpointAddress = new(_url + "/v2021_02");
        BasicHttpBinding basicHttpBinding = new() { SendTimeout = new TimeSpan(0, 60, 0) };
        _header = new SDK2009.MessageHeader()
        {
            DatabaseName = _databaseName,
            AuthenticationKey = _authenticationKey,
            IntegrationKey = "06d1ca95-1dcf-4c8d-a5e7-665b8017253b"
        };

        if (!string.IsNullOrEmpty(user))
        {
            _header.UserCode = user;
        }

        basicHttpBinding.MaxBufferSize = int.MaxValue;
        basicHttpBinding.MaxReceivedMessageSize = int.MaxValue;
        basicHttpBinding.Name = "SDK";
        basicHttpBinding.SendTimeout = TimeSpan.MaxValue;

        basicHttpBinding.Security.Mode = endpointAddress.Uri.ToString().ToUpper().Contains("HTTPS") ? BasicHttpSecurityMode.Transport : BasicHttpSecurityMode.None;
        System.Net.ServicePointManager.SetTcpKeepAlive(true, 30000, 30000);
        _service = new EpicSDK_2021_02Client(basicHttpBinding, endpointAddress);

        try
        {
            _service.Get_Lookup(_header, LookupTypes.AccessLevel, new List<string>());
        }
        catch (Exception ex)
        {
            throw new Exception($"SDK creation failed on wakeup call: {ex.Message}");
        }
    }

    // Method for testing the SDK Connection
    public async Task<bool> TestSdk()
    {
        try
        {
            Get_LookupResponse list = await _service.Get_LookupAsync(_header, LookupTypes.Agency, new List<string>());
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion

    #region SDK Methods

    #region Actions

    // Method for getting a renew policy object
    public async Task<Renew> GetRenew(int iPolicyId)
    {
        try
        {
            Get_Policy_DefaultActionRenewResponse? response = await Retry
                .DoAsync(() => _service.Get_Policy_DefaultActionRenewAsync(_header, iPolicyId));

            return response.Get_Policy_DefaultActionRenewResult.FirstOrDefault()!;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting Epic renewal: {ex.Message}");
        }
    }

    // Method for renewing a policy
    public async Task<Action_Policy_RenewResponse> RenewPolicy(Renew oRenew)
    {
        try
        {
            Action_Policy_RenewResponse? response = await Retry
                .DoAsync(() => _service.Action_Policy_RenewAsync(_header, oRenew));

            return response;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error renewing Epic policy: {ex.Message}");
        }
    }

    #endregion

    #region Vendor

    // Method for getting vendor by lookup code
    public async Task<Lookup> GetVendorByLookupCode(string sLookupCode)
    {
        try
        {
            Get_LookupResponse? response = await _service.Get_LookupAsync(_header, LookupTypes.Vendor, new());

            if (response.Get_LookupResult.Count >= 1)
            {
                foreach (Lookup lookup in response.Get_LookupResult.Where(lookup => lookup.Code.Equals(sLookupCode)))
                {
                    return lookup;
                }
            }

            throw new Exception($"Vendor was not found with lookup code {sLookupCode}.");

        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting vendor:  {ex.Message}");
        }
    }

    #endregion

    #region Contact

    // Method for getting the contact
    public async Task<Contact> GetContact(int iContactId)
    {
        try
        {
            Get_ContactResponse? response = await Retry
                .DoAsync(() => _service.Get_ContactAsync(_header, new ContactFilter
                {
                    AccountTypeCode = "CUST",
                    ContactID = iContactId,
                }, 0));
            if (response.Get_ContactResult.Contacts.Count < 1)
            {
                throw new Exception($"No contacts were found in Epic with ID {iContactId}");
            }
            else
            {
                return response.Get_ContactResult.Contacts.First();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting Epic contact: {ex.Message}");
        }
    }

    // Method for getting the contact
    public async Task<Contact> GetVendor(string sIssuingLocation)
    {
        try
        {
            Get_ContactResponse? response = await Retry
                .DoAsync(() => _service.Get_ContactAsync(_header, new ContactFilter
                {
                    AccountTypeCode = "VEND",
                    State = sIssuingLocation
                }, 0));

            if (response.Get_ContactResult.Contacts.Count < 1)
            {
                throw new Exception($"No vendors were found in Epic for state code: {sIssuingLocation}");
            }

            return response.Get_ContactResult.Contacts.First();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting Epic vendor: {ex.Message}");
        }
    }

    public async Task<Update_ContactResponse> UpdateContact(Contact oContact)
    {
        try
        {
            Update_ContactResponse? response = await Retry
                .DoAsync(() => _service.Update_ContactAsync(_header, oContact));
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating Epic contact: {ex.Message}");
        }
    }

    #endregion

    #region Client

    // Method for getting client by lookup code
    public async Task<int> GetClientIdByLookupCode(string sLookupCode)
    {
        try
        {
            Get_ClientResponse response = await Retry
                .DoAsync(() => _service.Get_ClientAsync(_header, new ClientFilter() { LookupCode = sLookupCode }, 0));
            if (response.Get_ClientResult.Clients.Count < 1)
            {
                throw new Exception($"No clients were found in Epic with Lookup Code {sLookupCode}.");
            }
            else
            {
                return response.Get_ClientResult.Clients.First().ClientID;
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting Epic client: {ex.Message}");
        }
    }

    #endregion

    #region Policy

    // Method for getting the policy
    public async Task<Policy> GetPolicy(int clientId, int policyId)
    {
        try
        {
            Get_PolicyResponse response = await Retry
                .DoAsync(() => _service.Get_PolicyAsync(_header,
                        new PolicyFilter() { ClientID = clientId, PolicyID = policyId }, 0));

            if (response.Get_PolicyResult.Policies.Count < 1)
            {
                throw new Exception($"No policies found for client ID {clientId} and policy ID {policyId}.");
            }

            return response.Get_PolicyResult.Policies.First();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting Epic policy: {ex.Message}");
        }
    }

    // Method for getting the policy
    public async Task<Policy> GetPolicyById(int policyId)
    {
        try
        {
            Get_PolicyResponse response = await Retry
                .DoAsync(() => _service.Get_PolicyAsync(_header,
                        new PolicyFilter() { PolicyID = policyId }, 0));

            if (response.Get_PolicyResult.Policies.Count < 1)
            {
                throw new Exception($"No policies found for policy ID {policyId}.");
            }

            return response.Get_PolicyResult.Policies.First();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting Epic policy: {ex.Message}");
        }
    }

    // Method for getting the policy
    public async Task<Policy> GetPolicyByMajescoItem(ProcessItem oProcessItem)
    {
        try
        {
            Get_PolicyResponse response = await Retry
                .DoAsync(() => _service.Get_PolicyAsync(_header, new PolicyFilter
                {
                    ClientID = oProcessItem.ClientId,
                    EffectiveDateBegins = oProcessItem.EffectiveDate,
                    EffectiveDateEnds = oProcessItem.EffectiveDate,
                    PolicyNumber = oProcessItem.CarnetNumber,
                    PolicyNumberComparisonType = ComparisonType.EqualTo
                }, 0));

            if (response.Get_PolicyResult.Policies.Count < 1)
            {
                throw new Exception($"No policies found for carnet number {oProcessItem.CarnetNumber}.");
            }

            return response.Get_PolicyResult.Policies.First();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting Epic policy: {ex.Message}");
        }
    }

    public async Task<int> InsertPolicy(Policy policy)
    {
        try
        {
            Insert_PolicyResponse response = await Retry
                .DoAsync(() => _service.Insert_PolicyAsync(_header, policy));
            return response.Insert_PolicyResult;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inserting Epic policy: {ex.Message}");
        }
    }

    public async Task<Delete_PolicyResponse> DeletePolicy(int iPolicyId)
    {
        try
        {
            Delete_PolicyResponse? response = await Retry
                .DoAsync(() => _service.Delete_PolicyAsync(_header, iPolicyId));
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting Epic policy: {ex.Message}");
        }
    }

    #endregion

    #region Transaction

    public async Task<List<EpicTransaction>> GetTransactions(int iPolicyId)
    {
        try
        {
            Get_TransactionResponse? response = await Retry
                .DoAsync(() => _service.Get_TransactionAsync(_header, new TransactionFilter
            {
                PolicyID = iPolicyId
                }, 0));
            return response.Get_TransactionResult.Transactions;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting transactions:{Environment.NewLine}{ex.Message}");
        }
    }

    // 
    public async Task<EpicTransaction> GetTransaction(int iTransactionId)
    {
        try
        {
            Get_TransactionResponse? response = await Retry
                .DoAsync(() => _service.Get_TransactionAsync(_header, new TransactionFilter
                {
                    TransactionID = iTransactionId
                }, 0));
            return response.Get_TransactionResult.Transactions.FirstOrDefault()!;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting transactions:{Environment.NewLine}{ex.Message}");
        }
    }

    // Method for inserting transactions
    public async Task<int> InsertTransaction(EpicTransaction oTransaction)
    {
        try
        {
            Insert_TransactionResponse response = await Retry
                .DoAsync(() => _service.Insert_TransactionAsync(_header, oTransaction));
            if (response.Insert_TransactionResult.Count < 1)
            {
                throw new Exception($"Failed to insert installment transaction.");
            }

            return response.Insert_TransactionResult.FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inserting transaction: {ex.Message}");
        }
    }

    // Method for retrieving configured transaction codes
    public async Task<List<TransactionCode>> GetTransactionCodes()
    {
        try
        {
            Get_Transaction_TransactionCodeResponse? response = await Retry
                .DoAsync(() => _service.Get_Transaction_TransactionCodeAsync(_header, string.Empty, TransactionCodeGetType.All, false));

            return response.Get_Transaction_TransactionCodeResult;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting transaction codes: {ex.Message}");
        }
    }

    #endregion

    #region Receipt

    public async Task<int> InsertReceipt(Receipt oReceipt)
    {
        try
        {
            Insert_GeneralLedger_ReceiptResponse? response = await Retry
                .DoAsync(() => _service.Insert_GeneralLedger_ReceiptAsync(_header, oReceipt));
            return response.Insert_GeneralLedger_ReceiptResult;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inserting receipt:{Environment.NewLine}{ex.Message}");
        }
    }

    public async Task<bool> ApplyCreditsToDebits(ApplyCreditsToDebits oApplyCreditsToDebits)
    {
        try
        {
            Action_Transaction_ApplyCreditsToDebitsResponse? response = await Retry
                .DoAsync(() => _service.Action_Transaction_ApplyCreditsToDebitsAsync(_header, oApplyCreditsToDebits));
            return response != null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error applying credits to debits:{Environment.NewLine}{ex.Message}");
        }
    }

    public async Task<bool> FinalizeReceipt(int iReceiptId)
    {
        try
        {
            Action_GeneralLedger_ReceiptFinalizeReceiptResponse? response = await Retry
                .DoAsync(() => _service.Action_GeneralLedger_ReceiptFinalizeReceiptAsync(_header, iReceiptId));
            return response != null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error finalizing receipt:{Environment.NewLine}{ex.Message}");
        }
    }

    // Method for getting receipt
    public async Task<Receipt> GetReceipt(int iReceiptId)
    {
        try
        {
            Get_GeneralLedger_ReceiptResponse? response = await Retry
                .DoAsync(() => _service.Get_GeneralLedger_ReceiptAsync(_header, ReceiptGetType.ReceiptID, iReceiptId,
                ReceiptGetLimitType.None, ReceiptFilterType.None, string.Empty, ReceiptComparisonType.None,
                string.Empty, 0));
            return response.Get_GeneralLedger_ReceiptResult.Receipts.FirstOrDefault()!;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting receipt: {ex.Message}");
        }
    }

    // Method for updating receipt
    public async Task<Update_GeneralLedger_ReceiptResponse> UpdateReceipt(Receipt oReceipt)
    {
        try
        {
            Update_GeneralLedger_ReceiptResponse? response = await Retry
                .DoAsync(() => _service.Update_GeneralLedger_ReceiptAsync(_header, oReceipt));
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating receipt: {ex.Message}");
        }
    }

    // Method for deleting receipt
    public async Task<Delete_GeneralLedger_ReceiptResponse> DeleteReceipt(int iReceiptId)
    {
        try
        {
            Delete_GeneralLedger_ReceiptResponse? response = await Retry
                .DoAsync(() => _service.Delete_GeneralLedger_ReceiptAsync(_header, iReceiptId));
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting receipt: {ex.Message}");
        }
    }

    // Method for getting apply credits to debits
    public async Task<Receipt> GetApplyCreditsToDebits(int iReceiptId, DetailItem oDetailItem)
    {
        try
        {
            Get_GeneralLedger_ReceiptDefaultApplyCreditsToDebitsResponse? response = await Retry
                .DoAsync(() => _service.Get_GeneralLedger_ReceiptDefaultApplyCreditsToDebitsAsync(_header, iReceiptId, oDetailItem));
            return response.Get_GeneralLedger_ReceiptDefaultApplyCreditsToDebitsResult.Receipts.FirstOrDefault()!;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting receipt for ApplyCreditsToDebits: {ex.Message}");
        }
    }

    #endregion

    #region Line

    public async Task<Line> GetLine(int iLineId)
    {
        try
        {
            Get_LineResponse response = await Retry
                .DoAsync(() => _service.Get_LineAsync(_header, new LineFilter() { LineID = iLineId }, 0));
            return response.Get_LineResult.Lines.FirstOrDefault()!;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting Epic line: {ex.Message}");
        }
    }

    public async Task<Line> GetLineByPolicyId(int iPolicyId)
    {
        try
        {
            Get_LineResponse response = await Retry
                .DoAsync(() => _service.Get_LineAsync(_header, new LineFilter() { PolicyID = iPolicyId }, 0));
            return response.Get_LineResult.Lines.FirstOrDefault()!;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting Epic line: {ex.Message}");
        }
    }

    public async Task<List<Line>> GetLinesByPolicyId(int iPolicyId)
    {
        try
        {
            Get_LineResponse response = await Retry
                .DoAsync(() => _service.Get_LineAsync(_header, new LineFilter() { PolicyID = iPolicyId }, 0));
            return response.Get_LineResult.Lines;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting Epic lines: {ex.Message}");
        }
    }

    public async Task<bool> UpdateLine(Line oLine)
    {
        try
        {
            Update_LineResponse? response = await Retry
                .DoAsync(() => _service.Update_LineAsync(_header, oLine));
            return response != null;
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Commission Agreement ID not found. Refer to the LineCommissionAgreement lookup."))
            {
                throw new Exception($"Error updating Epic line: {ex.Message} Commission Agreement ID: {oLine.AgreementID}");
            }

            throw new Exception($"Error updating Epic line: {ex.Message}");
        }
    }

    public async Task<int> InsertLine(Line oLine)
    {
        try
        {
            Insert_LineResponse? response = await Retry
                .DoAsync(() => _service.Insert_LineAsync(_header, oLine));
            return response.Insert_LineResult;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inserting Epic line: {ex.Message}");
        }
    }

    #endregion

    #region Tax Fee

    // Method for getting generate tax fee
    public async Task<GenerateTaxFee> GetGenerateTaxFee(int iTransactionId)
    {
        try
        {
            Get_Transaction_DefaultActionGenerateTaxFeeResponse? response = await Retry
                .DoAsync(() => _service.Get_Transaction_DefaultActionGenerateTaxFeeAsync(_header, iTransactionId));

            return response.Get_Transaction_DefaultActionGenerateTaxFeeResult.FirstOrDefault()!;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating tax fee: {ex.Message}");
        }
    }

    // Method for inserting generate tax fee
    public async Task<int> InsertGenerateTaxFee(GenerateTaxFee oGenerateTaxFee)
    {
        try
        {
            Action_Transaction_GenerateTaxFeeResponse? response = await Retry
                .DoAsync(() => _service.Action_Transaction_GenerateTaxFeeAsync(_header, oGenerateTaxFee));

            return response.Action_Transaction_GenerateTaxFeeResult.FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inserting tax fee: {ex.Message}");
        }
    }

    #endregion

    #region Activity

    // Method for inserting activities
    public async Task<int> InsertActivity(Activity activity)
    {
        try
        {
            Insert_ActivityResponse response = await Retry
                .DoAsync(() => _service.Insert_ActivityAsync(_header, activity));
            return response.Insert_ActivityResult;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inserting activity: {ex.Message}");
        }
    }

    // Method for inserting activities
    public async Task<int> InsertGeneralLedgerActivity(Activity activity)
    {
        try
        {
            Insert_GeneralLedger_ActivityResponse response = await Retry
                .DoAsync(() => _service.Insert_GeneralLedger_ActivityAsync(_header, activity));
            return response.Insert_GeneralLedger_ActivityResult;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inserting general ledger activity: {ex.Message}");
        }
    }

    #endregion

    #region Disbursement

    // Method for inserting disbursements
    public async Task<int> InsertDisbursement(Disbursement oDisbursement)
    {
        try
        {
            Insert_GeneralLedger_DisbursementResponse? response = await Retry
                .DoAsync(() => _service.Insert_GeneralLedger_DisbursementAsync(_header, oDisbursement));
            return response.Insert_GeneralLedger_DisbursementResult;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inserting disbursement: {ex.Message}");
        }
    }

    // Method for voiding disbursements
    public async Task<Action_GeneralLedger_DisbursementVoidResponse> VoidDisbursement(DisbursementVoid oDisbursementVoid)
    {
        try
        {
            Action_GeneralLedger_DisbursementVoidResponse? response = await Retry
                .DoAsync(() => _service.Action_GeneralLedger_DisbursementVoidAsync(_header, oDisbursementVoid));
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error voiding disbursement: {ex.Message}");
        }
    }

    #endregion

    #region Writeoff

    // Method for inserting writeoff
    public async Task<Action_Transaction_AccountsReceivableWriteOffResponse> InsertWriteOff(AccountsReceivableWriteOff oWriteOff)
    {
        try
        {
            Action_Transaction_AccountsReceivableWriteOffResponse? response = await Retry
                .DoAsync(() => _service.Action_Transaction_AccountsReceivableWriteOffAsync(_header, oWriteOff));
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inserting writeoff: {ex.Message}");
        }
    }

    #endregion

    #region Company

    // Method for getting the first company payable contract listed
    public async Task<int> GetFirstCompanyPayableContractId(int iCompanyId, DateTime dtEffectiveDate)
    {
        try
        {
            Get_Company_PayableContractResponse? response = await Retry
                .DoAsync(() => _service.Get_Company_PayableContractAsync(_header, iCompanyId, 0, CompanyPayableContractGetType.CompanyID));

            if (response.Get_Company_PayableContractResult.Count < 1)
            {
                throw new Exception($"No payable contracts were found for company ID: {iCompanyId}.");
            }

            // Return first payable contract where effective and expiration date are open
            foreach (PayableContract? payableContract in response.Get_Company_PayableContractResult.Where(
                         payableContract => payableContract is { TermStartOpen: true, TermEndOpen: true, ContractType: "Account Current" }))
            {
                return payableContract.PayableContractID;
            }

            // Return first payable contract where effective date is open and
            // the Term End Date is greater than the line effective date
            foreach (PayableContract? payableContract in response.Get_Company_PayableContractResult.Where(
                         payableContract =>
                             payableContract is { TermStartOpen: true, ContractType: "Account Current" } && payableContract.TermEnd >= dtEffectiveDate))
            {
                return payableContract.PayableContractID;
            }

            // Return first payable contract where end date is open and
            // the Term Start Date is less than the line effective date
            foreach (PayableContract? payableContract in response.Get_Company_PayableContractResult.Where(
                         payableContract =>
                             payableContract is { TermEndOpen: true, ContractType: "Account Current" } && payableContract.TermStart <= dtEffectiveDate))
            {
                return payableContract.PayableContractID;
            }

            // Return first payable contract where term start date is less than the line effective
            // date the term end date is greater than the line effective date
            foreach (PayableContract? payableContract in response.Get_Company_PayableContractResult.Where(
                         payableContract => payableContract.TermStart <= dtEffectiveDate &&
                                            payableContract.TermEnd >= dtEffectiveDate && payableContract.ContractType.Equals("Account Current")))
            {
                return payableContract.PayableContractID;
            }

            throw new Exception("An account current company payable contract does not exist for the matched billing company.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting company payable contract: {ex.Message}");
        }
    }

    // Method for getting billing company from issuing company id
    public async Task<Company> GetBillingFromIssuingCompanyId(int iCompanyId)
    {
        try
        {
            Get_CompanyResponse? response = await Retry
                .DoAsync(() => _service.Get_CompanyAsync(_header, iCompanyId.ToString(), string.Empty, CompanyGetType.CompanyID, true,
                false, 0));

            if (response.Get_CompanyResult.Companies.Count < 1)
            {
                throw new Exception($"No issuing companies were found in with ID {iCompanyId}.");
            }
            
            Company? company = response.Get_CompanyResult.Companies.First();
            string issuingCompany = company.LookupCode;

            if (company.BillingValue.IsBillingCompany)
            {
                return company;
            }

            if (company.BillingValue.BillingCompanyID is null or -1 or 0)
            {
                throw new Exception($"The matched issuing company {issuingCompany} does not have a billing company.");
            }

            response = await Retry
                .DoAsync(() => _service.Get_CompanyAsync(_header, company.BillingValue.BillingCompanyID.ToString(), string.Empty, CompanyGetType.CompanyID, true,
                    false, 0));

            if (response.Get_CompanyResult.Companies.Count < 1)
            {
                throw new Exception($"No billing companies were found in with ID {company.BillingValue.BillingCompanyID} for issuing company {issuingCompany}.");
            }

            company = response.Get_CompanyResult.Companies.First();

            if (!company.BillingValue.IsBillingCompany)
            {
                throw new Exception($"The matched billing company {company.LookupCode} for issuing company {issuingCompany} is not configured for billing.");
            }

            return company;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting company: {ex.Message}");
        }
    }

    // Method for getting existing company commission agreement
    public async Task<List<CompanyCommission>> GetCompanyCommissionAgreement(int iAccountId)
    {
        try
        {
            Get_Commission_CompanyCommissionResponse? response = await Retry
                .DoAsync(() => _service.Get_Commission_CompanyCommissionAsync(_header, iAccountId, CommissionGetType.AccountID));

            return response.Get_Commission_CompanyCommissionResult.CompanyCommissions;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting company commission agreements: {ex.Message}");
        }
    }

    #endregion

    #region Broker

    // Method for getting broker
    public async Task<Broker> GetBroker(int iBrokerId)
    {
        try
        {
            Get_BrokerResponse? response = await Retry
                .DoAsync(() => _service.Get_BrokerAsync(_header, iBrokerId.ToString(), string.Empty, BrokerGetType.BrokerID, true,
                false, 0));

            if (response.Get_BrokerResult.Brokers.Count < 1)
            {
                throw new Exception($"No brokers were found in with ID {iBrokerId}.");
            }
            return response.Get_BrokerResult.Brokers.First();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting broker: {ex.Message}");
        }
    }

    // Method for getting broker
    public async Task<Broker> GetBrokerByLookup(string sLookupCode)
    {
        try
        {
            Get_BrokerResponse? response = await Retry
                .DoAsync(() => _service.Get_BrokerAsync(_header, sLookupCode, string.Empty, BrokerGetType.LookupCode, true,
                true, 0));

            if (response.Get_BrokerResult.Brokers.Count < 1)
            {
                throw new Exception($"Broker was not found with lookup code {sLookupCode}.");
            }
            return response.Get_BrokerResult.Brokers.First();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting broker: {ex.Message}");
        }
    }

    // Method for getting existing broker commission agreements
    public async Task<List<BrokerCommission>> GetBrokerCommissionAgreement(int iAccountId)
    {
        try
        {
            Get_Commission_BrokerCommissionResponse? response = await Retry
                .DoAsync(() => _service.Get_Commission_BrokerCommissionAsync(_header, iAccountId, CommissionGetType.AccountID));

            return response.Get_Commission_BrokerCommissionResult.BrokerCommissions;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting broker commission agreements: {ex.Message}");
        }
    }

    // Method for getting existing broker commission agreements
    public async Task<List<BrokerCommission>> GetBrokerCommissionAgreementByLookup(string sLookupCode)
    {
        try
        {
            Get_BrokerResponse? brokerResponse = await Retry.DoAsync(() =>
                    _service.Get_BrokerAsync(_header, sLookupCode, string.Empty, BrokerGetType.LookupCode,
                        true, true, 0));

            Get_Commission_BrokerCommissionResponse ? response = await Retry
                .DoAsync(() => _service.Get_Commission_BrokerCommissionAsync(_header,
                    brokerResponse.Get_BrokerResult.Brokers.FirstOrDefault()!.BrokerID, CommissionGetType.AccountID));

            return response.Get_Commission_BrokerCommissionResult.BrokerCommissions;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting broker commission agreements: {ex.Message}");
        }
    }

    #endregion

    #region Employee
    // Method for getting employee
    public async Task<Employee> GetEmployee(int iEmployeeId)
    {
        try
        {
            Get_EmployeeResponse? response = await Retry
                .DoAsync(() => _service.Get_EmployeeAsync(_header, iEmployeeId.ToString(), string.Empty, EmployeeGetType.EmployeeID, true,
                false, 0));

            if (response.Get_EmployeeResult.Employees.Count < 1)
            {
                throw new Exception($"No employees were found in with ID {iEmployeeId}.");
            }
            return response.Get_EmployeeResult.Employees.First();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting employee: {ex.Message}");
        }
    }

    // Method for getting employee
    public async Task<Employee> GetEmployeeByLookup(string sLookupCode)
    {
        try
        {
            Get_EmployeeResponse? response = await Retry
                .DoAsync(() => _service.Get_EmployeeAsync(_header, sLookupCode, string.Empty, EmployeeGetType.LookupCode, true,
                true, 0));

            if (response.Get_EmployeeResult.Employees.Count < 1)
            {
                throw new Exception($"Employee was not found with lookup code {sLookupCode}.");
            }
            return response.Get_EmployeeResult.Employees.First();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting employee: {ex.Message}");
        }
    }

    // Method for getting existing employee commission agreements
    public async Task<List<EmployeeCommission>> GetEmployeeCommissionAgreements(string sLookupCode)
    {
        try
        {
            Get_EmployeeResponse? employeeResponse = await Retry
                .DoAsync(() => _service.Get_EmployeeAsync(_header, sLookupCode, string.Empty,
                    EmployeeGetType.LookupCode, true, true, 0));

            Get_Commission_EmployeeCommissionResponse ? response = await Retry
                .DoAsync(() => _service.Get_Commission_EmployeeCommissionAsync(_header,
                    employeeResponse.Get_EmployeeResult.Employees.FirstOrDefault()!.EmployeeID,
                    CommissionGetType.AccountID));

            return response.Get_Commission_EmployeeCommissionResult.EmployeeCommissions;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting employee commission agreements: {ex.Message}");
        }
    }
    #endregion

    #endregion

    #region Methods

    // Method for dispose
    public void Dispose() => _service.Close();

    #endregion
}