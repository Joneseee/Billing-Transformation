using IntegrationLogic.Models;
using schemas.appliedsystems.com.epic.sdk._2009._07._common;
using schemas.appliedsystems.com.epic.sdk._2011._01._account;
using schemas.appliedsystems.com.epic.sdk._2011._01._account._transaction._detail;
using schemas.appliedsystems.com.epic.sdk._2011._01._generalledger;
using schemas.appliedsystems.com.epic.sdk._2011._01._generalledger._disbursement._action;
using schemas.appliedsystems.com.epic.sdk._2011._01._generalledger._disbursement._detail._detailitem;

namespace IntegrationLogic.Helpers;

public class DisbursementHelper
{
    public static Disbursement GenerateDisbursement(ProcessItem oProcessItem, Policy oPolicy, Transaction oTransaction)
    {
        DetailItem? detailItem = oTransaction.DetailValue.DetailItemsValue.FirstOrDefault(item => item.TransactionDetailNumber == 1);

        if (oProcessItem.Amount < 0)
        {
            oProcessItem.Amount *= -1;
        }

        // Get option type
        OptionType debitCreditOption;
        if (oProcessItem.CreditDebit is "DB")
        {
            debitCreditOption = new()
            {
                OptionName = "Debit",
                Value = 0
            };
        }
        else
        {
            debitCreditOption = new()
            {
                OptionName = "Credit",
                Value = 1
            };
        }

        return new()
        {
            ExtensionData = null,
            CheckLastPrintedDate = null,
            CheckMemo = null,
            CheckNumber = null,
            CheckPreviousReferNumber = null,
            CheckPrint = false,
            CheckRemittance = null,
            DetailValue = new()
            {
                ExtensionData = null,
                AddAsDefaultEntry = false,
                DefaultEntryValue = new()
                {
                    NewDefaultEntry = null,
                    NewDefaultEntryOption = null,
                    OverrideExistingDefaultEntryID = 0
                },
                DetailItemsValue = new()
                {
                    new()
                    {
                        AdvancePremium = false,
                        Amount = Convert.ToDecimal(oProcessItem.Amount),
                        ApplyTo = null,
                        ApplyToBrokerLookupCode = null,
                        ApplyToCashOnAccountPaidItemsReturnCommission = null,
                        ApplyToClientLookupCode = oProcessItem.EntityCode,
                        ApplyToFinanceCompanyLookupCode = null,
                        ApplyToOtherInterestLookupCode = null,
                        ApplyToPolicyClientLookupCode = null,
                        ApplyToPolicyIncludeHistory = false,
                        ApplyToPolicyLineID = 0,
                        ApplyToSelectedItemsPaidItemsAdvancePremium = null,
                        ApplyToSelectedItemsPaidItemsReturnPremium = new()
                        {
                            ItemsPaid = new()
                            {
                                new()
                                {
                                    AgencyCode = oPolicy.AgencyCode,
                                    AmountDisbursed = Convert.ToDecimal(oProcessItem.Amount),
                                    Balance = detailItem!.Amount + Convert.ToDecimal(oProcessItem.Amount),
                                    BilledOrReserved = null,
                                    Description = oProcessItem.ProcessTypeDescription,
                                    ItemNumber = oProcessItem.ItemNumber.ToString(),
                                    TransactionAmount = detailItem!.Amount,
                                    TransactionCode = detailItem.TransactionCode,
                                    TransactionEffectiveDate = detailItem.ARDueDate,
                                    TransactionID = oTransaction.TransactionID
                                }
                            },
                            TotalItemsPaid = 1
                        },
                        DebitCreditOption = debitCreditOption,
                        Description = oProcessItem.ProcessTypeDescription,
                        DetailItemID = 0,
                        Flag = Flags.Insert,
                        GeneralLedgerAccountNumberCode = oProcessItem.GlAccount.ToString(),
                        GeneralLedgerScheduleCode = null,
                        GeneralLedgerSubAccountNumberCode = null,
                        InvoiceDate = null,
                        InvoiceNumber = null,
                        IsBankAccount = false,
                        PurchaseOrderNumber = null,
                        ReturnPremium = true,
                        StructureAgencyCode = oPolicy.AgencyCode,
                        StructureBranchCode = oPolicy.BranchCode,
                        StructureDepartmentCode = null,
                        StructureProfitCenterCode = null
                    }
                },
                Total = 1
            },
            DisbursementAccountingMonth = oProcessItem.AccountingYearMonth,
            DisbursementBankAccountNumberCode = oProcessItem.BankAccount.ToString(),
            DisbursementBankSubAccountNumberCode = null,
            DisbursementDefaultEntryID = 0,
            DisbursementDescription = oProcessItem.ProcessTypeDescription,
            DisbursementEffectiveDate = Convert.ToDateTime(oProcessItem.EffectiveDate),
            DisbursementID = 0,
            DisbursementRecurringEntry = null,
            DisbursementReferNumber = null,
            IgnoreDuplicatePayToTheOrderOfInvoiceNumber = false,
            IsReadOnly = false,
            PayToTheOrderOfAccountLookupCode = oProcessItem.EntityCode,
            PayToTheOrderOfAccountNumber = null,
            PayToTheOrderOfAccountType = "Client",
            PayToTheOrderOfInvoiceDate = null,
            PayToTheOrderOfInvoiceNumber = null,
            PayToTheOrderOfMailingAddress = null,
            PayToTheOrderOfMailingAddressContact = null,
            PayToTheOrderOfMailingAddressContactID = 0,
            PayToTheOrderOfMailingAddressSiteID = null,
            PayToTheOrderOfPayee = null,
            PayToTheOrderOfPayeeContactID = 0,
            Timestamp = null,
            VoidDetails = null,
            VoidReason = null,
            VoidReferNumber = null,
            VoidVoided = null,
            CheckComments = null,
            CheckIncludeCheckStubDetail = false,
            CheckRouting = null,
            DirectDepositMethodID = null
        };
    }

    public static DisbursementVoid GenerateVoidDisbursement(ProcessItem oProcessItem)
    {
        return new()
        {
            DetailAccountingMonth = oProcessItem.AccountingYearMonth,
            DetailDescription = oProcessItem.ProcessTypeDescription,
            DisbursementID = oProcessItem.DisbursementId,
            Reason = string.Empty,
            ReasonDetails = string.Empty,
            RelatedVouchersOption = null
        };
    }
}