using IntegrationLogic.Extensions;
using IntegrationLogic.Models;
using schemas.appliedsystems.com.epic.sdk._2009._07._common;
using schemas.appliedsystems.com.epic.sdk._2011._01._account;
using schemas.appliedsystems.com.epic.sdk._2011._01._generalledger;
using schemas.appliedsystems.com.epic.sdk._2011._01._generalledger._receipt._detail;
using schemas.appliedsystems.com.epic.sdk._2011._01._generalledger._receipt._detail._detailitem._applycreditstodebits;
using Flags = schemas.appliedsystems.com.epic.sdk._2011._01._generalledger._receipt._detail._detailitem.Flags;

namespace IntegrationLogic.Helpers;

// Abandon hope all ye who enter here...
public class ReceiptHelper
{
    public static Receipt CreateReceipt(ProcessItem oProcessItem, Policy oPolicy)
    {

        OptionType debitCreditOption = oProcessItem.CreditDebit!.Equals("DB")
            ? new() { OptionName = "Debit", Value = 0 }
            : new() { OptionName = "Credit", Value = 1 };

        return new Receipt
        {
            ExtensionData = null,
            BankAccountNumberCode = "1232",
            BankSubAccountNumberCode = null,
            DetailValue = new()
            {
                DetailItemsValue = new()
                {
                    new()
                    {
                        Amount = 1,
                        DebitCreditOption = debitCreditOption,
                        ApplyTo = "Account",
                        ApplyToPolicyClientLookupCode = oProcessItem.EntityCode,
                        ApplyToPolicyIncludeHistory = false,
                        ApplyToPolicyLineID = oProcessItem.LineId,
                        Description = $"{oProcessItem.ProcessTypeDescription.Truncate(58)}_PLACEHOLDER",
                        DetailItemAccountLookupCode = oProcessItem.EntityCode,
                        PaymentDate = DateTime.Today,
                        Flag = Flags.Insert,
                        StructureAgencyCode = oPolicy.AgencyCode,
                        StructureBranchCode = oPolicy.BranchCode,
                    }
                }

            },
            FinalizedReceipt = false,
            IsReadOnly = false,
            ProcessOutstandingPaymentsValue = new()
            {
                PaymentCreateTransmissionFile = false,
                PaymentNoTransmissionFile = false,
                ReceiptForPayment = false,
                UpdateAccountingMonthToMatchReceipt = false
            },
            ReceiptAccountingMonth = oProcessItem.AccountingYearMonth,
            ReceiptDescription = oProcessItem.ProcessTypeDescription,
            ReceiptEffectiveDate = Convert.ToDateTime(oProcessItem.DepositDate),
            SuspendedReceipt = true,
            IgnoreAccountingMonthVerification = false
        };
    }

    public static Receipt CreateApplyToPolicyReceipt(ProcessItem oProcessItem, Policy oPolicy)
    {

        OptionType debitCreditOption = oProcessItem.CreditDebit!.Equals("DB")
            ? new() { OptionName = "Debit", Value = 0 }
            : new() { OptionName = "Credit", Value = 1 };

        return new Receipt
        {
            ExtensionData = null,
            BankAccountNumberCode = "1232",
            BankSubAccountNumberCode = null,
            DetailValue = new()
            {
                DetailItemsValue = new()
                {
                    new()
                    {
                        Amount = (decimal)oProcessItem.Amount!,
                        DebitCreditOption = debitCreditOption,
                        ApplyTo = "Policy",
                        ApplyToPolicyClientLookupCode = oProcessItem.EntityCode,
                        ApplyToPolicyIncludeHistory = false,
                        ApplyToPolicyLineID = oProcessItem.LineId,
                        Description = $"{oProcessItem.ProcessTypeDescription.Truncate(70)}",
                        DetailItemAccountLookupCode = oProcessItem.EntityCode,
                        PaymentDate = DateTime.Today,
                        Flag = Flags.Insert,
                        StructureAgencyCode = oPolicy.AgencyCode,
                        StructureBranchCode = oPolicy.BranchCode,
                    }
                }

            },
            FinalizedReceipt = false,
            IsReadOnly = false,
            ProcessOutstandingPaymentsValue = new()
            {
                PaymentCreateTransmissionFile = false,
                PaymentNoTransmissionFile = false,
                ReceiptForPayment = false,
                UpdateAccountingMonthToMatchReceipt = false
            },
            ReceiptAccountingMonth = oProcessItem.AccountingYearMonth,
            ReceiptDescription = oProcessItem.ProcessTypeDescription,
            ReceiptEffectiveDate = Convert.ToDateTime(oProcessItem.DepositDate),
            SuspendedReceipt = true,
            IgnoreAccountingMonthVerification = false
        };
    }

    public static Receipt CreateReturnedPayment(ProcessItem oProcessItem, Policy oPolicy)
    {

        return new Receipt
        {
            ExtensionData = null,
            BankAccountNumberCode = "1232",
            BankSubAccountNumberCode = null,
            DetailValue = new()
            {
                DetailItemsValue = new()
                {
                    new()
                    {
                        Amount = (decimal)oProcessItem.Amount!,
                        DebitCreditOption = new()
                        {
                            OptionName = "Debit",
                            Value = 0
                        },
                        ApplyTo = "Account",
                        ApplyToPolicyClientLookupCode = oProcessItem.EntityCode,
                        ApplyToPolicyIncludeHistory = false,
                        ApplyToPolicyLineID = oProcessItem.LineId,
                        Description = $"{oProcessItem.ProcessTypeDescription.Truncate(53)}_RETURNED_PAYMENT",
                        DetailItemAccountLookupCode = oProcessItem.EntityCode,
                        PaymentDate = DateTime.Today,
                        Flag = Flags.Insert,
                        StructureAgencyCode = oPolicy.AgencyCode,
                        StructureBranchCode = oPolicy.BranchCode,
                    }
                }

            },
            FinalizedReceipt = false,
            IsReadOnly = false,
            ProcessOutstandingPaymentsValue = new()
            {
                PaymentCreateTransmissionFile = false,
                PaymentNoTransmissionFile = false,
                ReceiptForPayment = false,
                UpdateAccountingMonthToMatchReceipt = false
            },
            ReceiptAccountingMonth = oProcessItem.AccountingYearMonth,
            ReceiptDescription = oProcessItem.ProcessTypeDescription,
            ReceiptEffectiveDate = Convert.ToDateTime(oProcessItem.DepositDate),
            SuspendedReceipt = true,
            IgnoreAccountingMonthVerification = false
        };
    }

    public static Receipt UpdateReceipt(Receipt oReceipt, ProcessItem oProcessItem)
    {
        bool bPaymentApplied = false;

        if (oReceipt.DetailValue.DetailItemsValue.Count <= 0) return oReceipt;

        foreach (DetailItem detailItem in oReceipt.DetailValue.DetailItemsValue.Where(detailItem => detailItem.Description.Contains("PLACEHOLDER", StringComparison.OrdinalIgnoreCase) ||
                     detailItem.Amount == -1))
        {
            detailItem.Flag = Flags.Delete;
        }

        foreach (DetailItem? detailItem in oReceipt.DetailValue.DetailItemsValue.Where(detailItem => detailItem.ApplyToSelectedItemsApplyCreditsToDebits.Credits.Count > 0))
        {
            if (oProcessItem.CreditDebit is "DB")
            {
                detailItem.ApplyToSelectedItemsApplyCreditsToDebits.Debits = new() { new()
                    {
                        ARDueDate = default,
                        AccountingMonth = oProcessItem.AccountingYearMonth,
                        Balance = 0,
                        Description = oProcessItem.ProcessTypeDescription,
                        InvoiceNumber = null,
                        ItemNumber = oProcessItem.ItemNumber.ToString(),
                        Pending = false,
                        PolicyNumber = oProcessItem.CarnetNumber,
                        TransactionAmount = (decimal)oProcessItem.Amount!,
                        TransactionCode = null,
                        TransactionEffectiveDate = default,
                        TransactionID = (int)oProcessItem.ApplyToTransactionId!
                    }
                };
            }
            else
            {
                foreach (CreditItem? credit in detailItem.ApplyToSelectedItemsApplyCreditsToDebits.Credits.Where(
                             credit => credit.Description.Equals(oProcessItem.ProcessTypeDescription, StringComparison.OrdinalIgnoreCase)))
                {
                    if (credit.Balance == -1.00M) continue;

                    if (credit.TransactionAmount != oProcessItem.Amount * -1)
                    {
                        credit.Payments.Add(new()
                        {
                            ApplyToDebitTransactionID = Convert.ToInt32(oProcessItem.ApplyToTransactionId),
                            FullPayment = false,
                            PartialPaymentAmount = oProcessItem.Amount,
                        });

                        bPaymentApplied = true;
                        break;
                    }

                    credit.Payments.Add(new()
                    {
                        ApplyToDebitTransactionID = Convert.ToInt32(oProcessItem.ApplyToTransactionId),
                        FullPayment = true,
                    });

                    bPaymentApplied = true;
                    break;
                }
            }

            if (bPaymentApplied)
            {
                break;
            }
        }
        return oReceipt;
    }
}