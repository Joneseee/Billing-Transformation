using System.Data.SqlClient;
using IntegrationLogic.Extensions;

namespace IntegrationLogic.Models;

public class ProcessItem
{
    #region Properties

    public int UniqMajescoHeader { get; set; }
    public int UniqMajescoItem { get; set; }
    public string? RecordType { get; set; }
    public string? EntityCode { get; set; }
    public string? CarnetNumber { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int? MajescoItemNumber { get; set; }
    public string? SystemActivityNumber { get; set; }
    public string? TransactionType { get; set; }
    public string? ReceivableCode { get; set; }
    public decimal? Amount { get; set; }
    public string? CreditDebit { get; set; }
    public string? CarnetHolder { get; set; }
    public string? BankCode { get; set; }
    public string? AccountingYearMonth { get; set; }
    public DateTime? DepositDate { get; set; }
    public string? LocationId { get; set; }
    public int Processed { get; set; }
    public string Exception = string.Empty;
    public int ClientId { get; set; }
    public int MatchPolicyId { get; set; }
    public int MatchLineId { get; set; }
    public int TransactionId { get; set; }
    public int DeferredTransactionId { get; set; }
    public int PolicyId { get; set; }
    public int LineId { get; set; }
    public int ActivityId { get; set; }
    public int ReceiptId { get; set; }
    public int DisbursementId { get; set; }
    public string? LineType { get; set; }
    public string? TransactionCode { get; set; }
    public string Description { get; set; }
    public string ActivityCode { get; set; }
    public int ItemNumber { get; set; }
    public int BillNumber { get; set; }
    public int InvoiceNumber { get; set; }
    public int BankAccount { get; set; }
    public int GlAccount { get; set; }
    public int ReferNumber { get; set; }
    public int? ApplyToItemNumber { get; set;}
    public int? ApplyToTransactionId { get; set; }
    public int? ApplyToDeferredTransactionId { get; set; }
    public string? ApplyToTransactionCode { get; set; }
    public DateTime? ApplyToEffectiveDate { get; set; }
    public decimal? DeferredAmount { get; set; }
    public string? UnderwritingCompanyCode { get; set; }
    public int? PaymentSequence { get; set; }
    public int? PaymentItemSequence { get; set; }
    public string? ProcessType { get; set; }
    public string? FormNo { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyLookupCode { get; set; }
    public bool PartialProcess { get; set; } = false;
    public string ProcessTypeDescription { get; set; } = string.Empty;
    public string? CustomerEntityCode { get; set; }
    public DateTime? ProcessedDate { get; set; }

    #endregion

    #region Constructor

    public ProcessItem(SqlDataReader oReader)
    {
        UniqMajescoHeader = string.IsNullOrWhiteSpace(oReader["UniqMajescoHeader"].ToString())
            ? -1
            : Convert.ToInt32(oReader["UniqMajescoHeader"]);

        UniqMajescoItem = string.IsNullOrWhiteSpace(oReader["UniqMajescoItem"].ToString())
            ? -1
            : Convert.ToInt32(oReader["UniqMajescoItem"]);

        RecordType = string.IsNullOrWhiteSpace(oReader["RecordType"].ToString()) 
            ? string.Empty
            : oReader["RecordType"].ToString()!;

        EntityCode = string.IsNullOrWhiteSpace(oReader["EntityCode"].ToString())
            ? string.Empty
            : oReader["EntityCode"].ToString()!;

        CarnetNumber = string.IsNullOrWhiteSpace(oReader["CarnetNumber"].ToString()) 
            ? string.Empty
            : oReader["CarnetNumber"].ToString()!;

        EffectiveDate = string.IsNullOrWhiteSpace(oReader["EffectiveDate"].ToString()) 
            ? DateTime.MinValue 
            : Convert.ToDateTime(oReader["EffectiveDate"]);

        ExpirationDate = string.IsNullOrWhiteSpace(oReader["ExpirationDate"].ToString()) 
            ? DateTime.MinValue 
            : Convert.ToDateTime(oReader["ExpirationDate"]);

        MajescoItemNumber = string.IsNullOrWhiteSpace(oReader["MajescoItemNumber"].ToString())
            ? -1
            : Convert.ToInt32(oReader["MajescoItemNumber"]);

        SystemActivityNumber = string.IsNullOrWhiteSpace(oReader["SystemActivityNumber"].ToString()) 
            ? string.Empty
            : oReader["SystemActivityNumber"].ToString()!;

        TransactionType = string.IsNullOrWhiteSpace(oReader["TransactionType"].ToString()) 
            ? string.Empty
            : oReader["TransactionType"].ToString()!;

        ReceivableCode = string.IsNullOrWhiteSpace(oReader["ReceivableCode"].ToString()) 
            ? string.Empty
            : oReader["ReceivableCode"].ToString()!;

        Amount = string.IsNullOrWhiteSpace(oReader["Amount"].ToString()) 
            ? 0.00M 
            : Convert.ToDecimal(oReader["Amount"]);

        CreditDebit = string.IsNullOrWhiteSpace(oReader["CreditDebit"].ToString()) 
            ? string.Empty
            : oReader["CreditDebit"].ToString()!;

        CarnetHolder = string.IsNullOrWhiteSpace(oReader["CarnetHolder"].ToString()) 
            ? string.Empty
            : oReader["CarnetHolder"].ToString()!;

        BankCode = string.IsNullOrWhiteSpace(oReader["BankCode"].ToString()) 
            ? string.Empty
            : oReader["BankCode"].ToString()!;

        AccountingYearMonth = string.IsNullOrWhiteSpace(oReader["AccountingYearMonth"].ToString()) 
            ? string.Empty
            : oReader["AccountingYearMonth"].ToString()!;

        DepositDate = string.IsNullOrWhiteSpace(oReader["DepositDate"].ToString()) 
            ? DateTime.MinValue 
            : Convert.ToDateTime(oReader["DepositDate"]);

        LocationId = string.IsNullOrWhiteSpace(oReader["LocationId"].ToString()) 
            ? string.Empty
            : oReader["LocationId"].ToString();

        FormNo = string.IsNullOrWhiteSpace(oReader["FormNo"].ToString())
            ? string.Empty
            : oReader["FormNo"].ToString();

        ClientId = string.IsNullOrWhiteSpace(oReader["UniqClient"].ToString())
            ? -1
            : Convert.ToInt32(oReader["UniqClient"]);

        MatchPolicyId = string.IsNullOrWhiteSpace(oReader["UniqPolicyMatch"].ToString())
            ? -1
            : Convert.ToInt32(oReader["UniqPolicyMatch"]);

        MatchLineId = string.IsNullOrWhiteSpace(oReader["UniqLineMatch"].ToString())
            ? -1
            : Convert.ToInt32(oReader["UniqLineMatch"]);

        TransactionId = string.IsNullOrWhiteSpace(oReader["UniqTransHead"].ToString())
            ? -1
            : Convert.ToInt32(oReader["UniqTransHead"]);

        DeferredTransactionId = string.IsNullOrWhiteSpace(oReader["UniqTransHeadDeferred"].ToString())
            ? -1
            : Convert.ToInt32(oReader["UniqTransHeadDeferred"]);

        PolicyId = string.IsNullOrWhiteSpace(oReader["UniqPolicy"].ToString())
            ? -1
            : Convert.ToInt32(oReader["UniqPolicy"]);

        LineId = string.IsNullOrWhiteSpace(oReader["UniqLine"].ToString()) 
            ? -1 
            : Convert.ToInt32(oReader["UniqLine"]);

        ActivityId = string.IsNullOrWhiteSpace(oReader["UniqActivity"].ToString())
            ? -1
            : Convert.ToInt32(oReader["UniqActivity"]);

        ReceiptId = string.IsNullOrWhiteSpace(oReader["UniqReceipt"].ToString())
            ? -1
            : Convert.ToInt32(oReader["UniqReceipt"]);

        DisbursementId = string.IsNullOrWhiteSpace(oReader["UniqDisbursement"].ToString())
            ? -1
            : Convert.ToInt32(oReader["UniqDisbursement"]);

        InvoiceNumber = string.IsNullOrWhiteSpace(oReader["InvoiceNumber"].ToString())
            ? -1
            : Convert.ToInt32(oReader["InvoiceNumber"]);

        ItemNumber = string.IsNullOrWhiteSpace(oReader["ItemNumber"].ToString())
            ? -1
            : Convert.ToInt32(oReader["ItemNumber"]);

        BillNumber = string.IsNullOrWhiteSpace(oReader["BillNumber"].ToString())
            ? -1
            : Convert.ToInt32(oReader["BillNumber"]);

        BankAccount = string.IsNullOrWhiteSpace(oReader["BankAccount"].ToString())
            ? -1
            : Convert.ToInt32(oReader["BankAccount"]);

        GlAccount = string.IsNullOrWhiteSpace(oReader["GlAccount"].ToString())
            ? -1
            : Convert.ToInt32(oReader["GlAccount"]);

        ReferNumber = string.IsNullOrWhiteSpace(oReader["ReferNumber"].ToString())
            ? -1
            : Convert.ToInt32(oReader["ReferNumber"]);

        LineType = string.IsNullOrWhiteSpace(oReader["LineType"].ToString()) 
            ? string.Empty
            : oReader["LineType"].ToString()!;

        TransactionCode = string.IsNullOrWhiteSpace(oReader["TransactionCode"].ToString()) 
            ? string.Empty
            : oReader["TransactionCode"].ToString()!;

        Description = string.IsNullOrWhiteSpace(oReader["Description"].ToString()) 
            ? string.Empty
            : oReader["Description"].ToString()!;

        ActivityCode = string.IsNullOrWhiteSpace(oReader["ActivityCode"].ToString()) 
            ? string.Empty
            : oReader["ActivityCode"].ToString()!;

        Processed = string.IsNullOrWhiteSpace(oReader["Processed"].ToString()) 
            ? 0 
            : Convert.ToInt32(oReader["Processed"]);

        ApplyToTransactionCode = string.IsNullOrWhiteSpace(oReader["ApplyToTransactionCode"].ToString()) 
            ? string.Empty
            : oReader["ApplyToTransactionCode"].ToString()!;

        ApplyToTransactionId = string.IsNullOrWhiteSpace(oReader["ApplyToTransactionId"].ToString())
            ? -1
            : Convert.ToInt32(oReader["ApplyToTransactionId"]);

        ApplyToDeferredTransactionId = string.IsNullOrWhiteSpace(oReader["ApplyToDeferredTransactionId"].ToString())
            ? -1
            : Convert.ToInt32(oReader["ApplyToDeferredTransactionId"]);

        ApplyToItemNumber = string.IsNullOrWhiteSpace(oReader["ApplyToItemNumber"].ToString())
            ? -1
            : Convert.ToInt32(oReader["ApplyToItemNumber"]);

        ApplyToEffectiveDate = string.IsNullOrWhiteSpace(oReader["ApplyToEffectiveDate"].ToString()) 
            ? DateTime.MinValue 
            : Convert.ToDateTime(oReader["ApplyToEffectiveDate"]);

        DeferredAmount = string.IsNullOrWhiteSpace(oReader["DeferredAmount"].ToString())
            ? 0.00M
            : Convert.ToDecimal(oReader["DeferredAmount"]);

        UnderwritingCompanyCode = string.IsNullOrWhiteSpace(oReader["UnderwritingCompanyCode"].ToString())
            ? string.Empty
            : oReader["UnderwritingCompanyCode"].ToString()!;

        CompanyId = string.IsNullOrWhiteSpace(oReader["UniqCompany"].ToString())
            ? -1
            : Convert.ToInt32(oReader["UniqCompany"]);

        CompanyLookupCode = string.IsNullOrWhiteSpace(oReader["CompanyLookupCode"].ToString())
            ? string.Empty
            : oReader["CompanyLookupCode"].ToString()!;

        PaymentSequence = string.IsNullOrWhiteSpace(oReader["PaymentSequence"].ToString())
            ? -1
            : Convert.ToInt32(oReader["PaymentSequence"]);

        PaymentItemSequence = string.IsNullOrWhiteSpace(oReader["PaymentItemSequence"].ToString())
            ? -1
            : Convert.ToInt32(oReader["PaymentItemSequence"]);

        ProcessType = string.IsNullOrWhiteSpace(oReader["ProcessType"].ToString())
            ? string.Empty
            : oReader["ProcessType"].ToString()!;

        CustomerEntityCode = string.IsNullOrWhiteSpace(oReader["CustomerEntityCode"].ToString())
            ? string.Empty
            : oReader["CustomerEntityCode"].ToString()!;

        // Update bond cargo description
        if (ProcessType!.Equals("Bond", StringComparison.OrdinalIgnoreCase))
        {
            string[] parsePolicyNumber = CarnetNumber.Split('_');

            switch (parsePolicyNumber.Count())
            {
                case >= 2:
                    ProcessTypeDescription = $"{parsePolicyNumber[0]}_{parsePolicyNumber[1]}_{CarnetHolder}";
                    ProcessTypeDescription.Truncate(70);
                    break;
                case 1 when !string.IsNullOrWhiteSpace(FormNo):
                    ProcessTypeDescription = $"{parsePolicyNumber[0]}_{FormNo}_{CarnetHolder}";
                    ProcessTypeDescription.Truncate(70);
                    break;
            }
        }
        else if (ProcessType!.Equals("Cargo", StringComparison.OrdinalIgnoreCase))
        {
            string[] parsePolicyNumber = CarnetNumber.Split('_');

            if (parsePolicyNumber.Count() > 1)
            {
                ProcessTypeDescription = $"{parsePolicyNumber[1]}";
                ProcessTypeDescription.Truncate(70);
            }
        }
        else
        {
            ProcessTypeDescription = Description;
        }
    }
    #endregion
}