using IntegrationLogic.Extensions;
using IntegrationLogic.Models;
using schemas.appliedsystems.com.epic.sdk._2011._01._account;
using schemas.appliedsystems.com.epic.sdk._2011._01._account._policy._line._producerbrokercommissions;
using schemas.appliedsystems.com.epic.sdk._2011._01._account._transaction._action;
using schemas.appliedsystems.com.epic.sdk._2011._01._account._transaction._commissions._splititem;
using schemas.appliedsystems.com.epic.sdk._2011._01._account._transaction._commissionsplititem;
using schemas.appliedsystems.com.epic.sdk._2017._02._account._policy;
using Transaction = schemas.appliedsystems.com.epic.sdk._2011._01._account.Transaction;
using ProducerBroker = schemas.appliedsystems.com.epic.sdk._2011._01._account._transaction._commissionsplititem._producerbrokercommissionitem;


namespace IntegrationLogic.Helpers;

public class TransactionHelper
{
    // Method for generating write-off
    public static AccountsReceivableWriteOff GenerateWriteOff(ProcessItem oProcessItem)
    {
        return new()
        {
            AccountingMonth = oProcessItem.AccountingYearMonth,
            AssociatedAccountID = oProcessItem.ClientId,
            AssociatedAccountTypeCode = "CUST",
            Description = oProcessItem.ProcessTypeDescription,
            TransactionCode = "WOFF",
            TransactionID = oProcessItem.TransactionId
        };
    }

    // Method for the generating new transactions
    public static Transaction GenerateTransaction(Policy oPolicy, Line oLine, string sDescription, decimal dTransactionAmount, ProcessItem oProcessItem)
    {
        // Blank issuing company check
        if (string.IsNullOrWhiteSpace(oLine.IssuingCompanyLookupCode))
        {
            throw new Exception("Error inserting transaction: Epic line issuing company is blank.");
        }

        // Blank premium payable entity check
        if (string.IsNullOrWhiteSpace(oLine.PremiumPayableLookupCode))
        {
            throw new Exception("Error inserting transaction: Epic line premium payable entity is blank.");
        }

        try
        {
            DateTime effectiveDate = (oProcessItem.ProcessType!.Equals("Bond", StringComparison.OrdinalIgnoreCase) ||
                                     oProcessItem.ProcessType.Equals("Cargo", StringComparison.OrdinalIgnoreCase)) &&
                                    oProcessItem.EffectiveDate != null
                ? Convert.ToDateTime(oProcessItem.EffectiveDate)
                : oPolicy.EffectiveDate;

            Transaction transaction = new()
            {
                AccountID = oPolicy.AccountID,
                AccountTypeCode = "CUST",
                AttachToOption = new()
                {
                    Value = 2,
                    OptionName = "Policy"
                },
                Balance = 0,
                BillingValue = new()
                {
                    ARDueDate = effectiveDate,
                    AccountingMonth = oProcessItem.AccountingYearMonth,
                    AgencyCode = oPolicy.AgencyCode,
                    BillingModeOption = oPolicy.BillingModeOption,
                    BranchCode = oPolicy.BranchCode,
                    DepartmentCode = oPolicy.DepartmentCode,
                    EffectiveDate = effectiveDate,
                    GenerateInvoiceDate = DateTime.Now,
                    ProductionMonth = oProcessItem.AccountingYearMonth,
                    ProfitCenterCode = oLine.ProfitCenterCode
                },
                Description = sDescription.Truncate(70),
                DetailValue = new()
                {
                    Balance = dTransactionAmount,
                    DetailItemsValue = new()
                    {
                        new()
                        {
                            ARDueDate = effectiveDate,
                            Amount = dTransactionAmount,
                            Description = sDescription.Truncate(70),
                            TransactionCode = oProcessItem.TransactionCode,
                            TransactionDetailNumber = 1
                        }
                    }
                },
                InvoiceValue = new()
                {
                    SendInvoiceTos = new()
                    {
                        new()
                        {
                            AccountLookupCode = oLine.BillingValue.InvoiceToAccountLookupCode,
                            Address = new()
                            {
                                Street1 = oLine.BillingValue.InvoiceToAddress.Street1,
                                Street2 = oLine.BillingValue.InvoiceToAddress.Street2,
                                Street3 = oLine.BillingValue.InvoiceToAddress.Street3,
                                City = oLine.BillingValue.InvoiceToAddress.City,
                                StateOrProvinceCode = oLine.BillingValue.InvoiceToAddress.StateOrProvinceCode,
                                ZipOrPostalCode = oLine.BillingValue.InvoiceToAddress.ZipOrPostalCode,
                                County = oLine.BillingValue.InvoiceToAddress.County,
                                CountryCode = oLine.BillingValue.InvoiceToAddress.CountryCode
                            },
                            AddressDescription = oLine.BillingValue.InvoiceToAddressDescription,
                            Contact = oLine.BillingValue.InvoiceToContactName,
                            ContactID = oLine.BillingValue.InvoiceToContactID,
                            DeliveryMethod = oLine.BillingValue.InvoiceToDeliveryMethod,
                            Email = oLine.BillingValue.InvoiceToEmail,
                            FaxCountryCode = oLine.BillingValue.InvoiceToFaxCountryCode,
                            FaxExtension = oLine.BillingValue.InvoiceToFaxExtension,
                            FaxNumber = oLine.BillingValue.InvoiceToFaxNumber,
                            GenerateInvoice = true,
                            InvoiceMessage = null,
                            InvoiceNumber = null,
                            InvoiceGrouping = "Account Page Break",
                            InvoiceToType = oLine.BillingValue.InvoiceToType,
                            LoanNumber = oLine.BillingValue.LoanNumber,
                            SiteID = oLine.BillingValue.InvoiceToSiteID
                        }
                    }
                },
                IsReadOnly = false,
                PolicyID = oPolicy.PolicyID,
                PolicyTypeCode = oPolicy.PolicyTypeCode,
                TransactionAmount = dTransactionAmount,
                TransactionCode = oProcessItem.TransactionCode,
                CommissionsValue = new()
                {
                    Splits = new()
                },
                IgnoreAgencyCommissionLessThanProducerBroker = true,
                IgnoreFlatCommission = true,
                BasicInstallmentOption = new()
                {
                    Value = 0,
                    OptionName = "Basic"
                },
                InvoicePaymentOption = new()
                {
                    Value = 0,
                    OptionName = "Invoice"
                },
                AutoGenerateTaxes = false
            };

            // If item number is not -1 then existing bill
            if (oProcessItem.BillNumber != -1)
            {
                transaction.BillNumber = oProcessItem.BillNumber;
                transaction.BillNumberOption = new()
                {
                    OptionName = "ExistingBill",
                    Value = 1
                };
            }

            // If invoice number is not -1 then invoice grouping
            if (oProcessItem.InvoiceNumber != -1)
            {
                transaction.InvoiceValue.SendInvoiceTos.FirstOrDefault()!.InvoiceGroupingExistingInvoiceNumber = oProcessItem.InvoiceNumber;
            }
            // Create Producer Broker Commission Items
            ProducerBrokerCommissionItems producerBrokerCommissionItems = new();

            if (!oProcessItem.TransactionCode!.Equals("WPAT"))
            {
                if (oLine.ProducerBrokerCommissionsValue != null)
                {
                    if (oLine.ProducerBrokerCommissionsValue.ProducerBrokerCommissionTermOption != null &&
                        oLine.ProducerBrokerCommissionsValue.ProducerBrokerCommissionTermOption.Value != 1)
                    {
                        if (oLine.ProducerBrokerCommissionsValue.Commissions != null)
                        {
                            foreach (CommissionItem commissionItem in oLine.ProducerBrokerCommissionsValue.Commissions)
                            {
                                int contractId;
                                if (commissionItem.ContractID != null)
                                {
                                    contractId = (int)commissionItem.ContractID!;
                                }
                                else
                                {
                                    contractId = -1;
                                }

                                producerBrokerCommissionItems.Add(new()
                                {
                                    ExtensionData = null,
                                    CommissionAmount = Convert.ToDecimal(commissionItem.CommissionAmount),
                                    CommissionID = commissionItem.CommissionAgreementID,
                                    CommissionPercentage = Convert.ToDecimal(commissionItem.CommissionPercent),
                                    CommissionTypeCode = commissionItem.CommissionType,
                                    Flag = ProducerBroker.Flags.Insert,
                                    LookupCode = commissionItem.LookupCode,
                                    OrderNumber = commissionItem.OrderNumber,
                                    ProducerBrokerCode = commissionItem.ProducerBrokerCode,
                                    ProductionCredit = commissionItem.ProductionCredit,
                                    ContractID = contractId,
                                    PayableDueDate = default,
                                    ShareRevenueAgencyCode = commissionItem.ShareRevenueAgencyCode,
                                    ShareRevenueAgencyID = commissionItem.ShareRevenueAgencyID,
                                    ShareRevenueAgencyName = commissionItem.ShareRevenueAgencyName,
                                    ShareRevenueAmount = null,
                                    ShareRevenueBranchCode = commissionItem.ShareRevenueBranchCode,
                                    ShareRevenueBranchID = commissionItem.ShareRevenueBranchID,
                                    ShareRevenueBranchName = commissionItem.ShareRevenueBranchName,
                                    ShareRevenueDepartmentCode = commissionItem.ShareRevenueDepartmentCode,
                                    ShareRevenueDepartmentID = commissionItem.ShareRevenueDepartmentID,
                                    ShareRevenueDepartmentName = commissionItem.ShareRevenueDepartmentName,
                                    ShareRevenuePercent = commissionItem.ShareRevenuePercent,
                                    ShareRevenueProfitCenterCode = commissionItem.ShareRevenueProfitCenterCode,
                                    ShareRevenueProfitCenterID = commissionItem.ShareRevenueProfitCenterID,
                                    ShareRevenueProfitCenterName = commissionItem.ShareRevenueProfitCenterName,
                                    DefaultProducerBrokerCommissionID = null
                                });
                            }
                        }
                    }
                }
            }

            // Create Split Item
            transaction.CommissionsValue.Splits.Add(new()
            {
                AgencyCommissionAmount = Convert.ToDecimal(oLine.AgencyCommissionAmount),
                AgencyCommissionPercentage = Convert.ToDecimal(oLine.AgencyCommissionPercent),
                AgencyCommissionTypeCode = oLine.AgencyCommissionType,
                AgencySplitAmount = dTransactionAmount,
                Flag = Flags.Insert,
                IssuingCompanyLookupCode = oLine.IssuingCompanyLookupCode,
                LineID = oLine.LineID,
                MultiCarrierScheduleID = 0,
                PremiumPayableContractID = string.IsNullOrEmpty(oLine.PayableContractID.ToString()) ? 0 : (int)oLine.PayableContractID!,
                PremiumPayableLookupCode = oLine.PremiumPayableLookupCode,
                PremiumPayableTypeCode = oLine.PremiumPayableTypeCode,
                CompanyPayableDueDate = effectiveDate,  
                ProducerBrokerCommissions = producerBrokerCommissionItems
            });

            return transaction;
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while creating transaction :{Environment.NewLine}{Environment.NewLine}{ex.Message}");
        }
    }
}