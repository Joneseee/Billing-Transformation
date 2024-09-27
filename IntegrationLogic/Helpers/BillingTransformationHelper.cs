using IntegrationLogic.Extensions;
using IntegrationLogic.Models;
using schemas.appliedsystems.com.epic.sdk._2009._07._account._common;
using schemas.appliedsystems.com.epic.sdk._2011._01._account;
using schemas.appliedsystems.com.epic.sdk._2011._01._account._policy._action;
using schemas.appliedsystems.com.epic.sdk._2011._01._account._policy._action._renew;
using schemas.appliedsystems.com.epic.sdk._2011._01._account._policy._action._renew._lineitem;
using schemas.appliedsystems.com.epic.sdk._2017._02._account._policy;
using schemas.appliedsystems.com.epic.sdk._2011._01._account._policy._line._producerbrokercommissions;
using CommissionFlags = schemas.appliedsystems.com.epic.sdk._2011._01._account._policy._line._producerbrokercommissions._commissionitem.Flags;
using schemas.appliedsystems.com.epic.sdk._2011._01._common;
using schemas.appliedsystems.com.epic.sdk._2011._01._generalledger;
using schemas.appliedsystems.com.epic.sdk._2011._01._account._transaction._action;
using schemas.appliedsystems.com.epic.sdk._2011._01._account._transaction._action._generatetaxfee;
using schemas.appliedsystems.com.epic.sdk._2011._01._generalledger._disbursement._action;
using schemas.appliedsystems.com.epic.sdk._2011._01._generalledger._receipt._detail;
using schemas.appliedsystems.com.epic.sdk._2011._12._account;
using schemas.appliedsystems.com.epic.sdk._2011._12._configure._transaction;
using schemas.appliedsystems.com.epic.sdk._2014._11._account;
using ReceiptFlags = schemas.appliedsystems.com.epic.sdk._2011._01._generalledger._receipt._detail._detailitem.Flags;
using AgencyDefinedCategoryFlags = schemas.appliedsystems.com.epic.sdk._2009._07._account._common._agencydefinedcodeitem.Flags;

namespace IntegrationLogic.Helpers;

public class BillingTransformationHelper : IDisposable
{
    private readonly SdkHelper _sdkHelper;
    private readonly BdeHelper _bdeHelper;
    private readonly Dictionary<string, List<LineType>> _dictPolicy;
    private List<TransactionCode> _transactionCodeList;

    public BillingTransformationHelper(Config oConfig)
    {
        _sdkHelper = new(oConfig.Url!, oConfig.DatabaseName!, oConfig.AuthenticationKey!, oConfig.Usercode!);
        _bdeHelper = new(oConfig.SqlServer!, oConfig.SqlDatabase!, oConfig.SqlUsername!, oConfig.SqlPassword!);
        _dictPolicy = new();
        _transactionCodeList = new();
    }

    #region Other Methods

    public bool SqlTableCheck()
    {
        _bdeHelper.SqlTableCheck();
        _bdeHelper.SqlMappingTableCheck();
        _bdeHelper.CreateIndexes();
        return _bdeHelper.CreateTableColumns();
    }

    public bool ReprocessErrors()
    {
        return _bdeHelper.ReprocessErrors();
    }

    public bool PostProcessErrors()
    {
        return _bdeHelper.PostProcessErrors();
    }

    public bool PostProcessSync()
    {
        return _bdeHelper.DataSync();
    }

    public void InsertFile(string sFilePath, string sArchivePath)
    {
        int uniqHeader = -1;
        try
        {
            // Read file header and insert into database
            string header = File.ReadLines(sFilePath).First();
            string[] strings = sFilePath.Split('\\');
            string newPath = $"{sArchivePath}\\{strings[^1]}";

            if (File.Exists(newPath))
            {
                newPath = newPath.Replace(".txt", $"_{DateTime.Now.ToString("HH:mm:ss").Replace(":", string.Empty)}.txt");
            }

            MajescoHeader majescoHeader = new(header)
            {
                FilePath = newPath
            };

            majescoHeader.UniqMajescoHeader = _bdeHelper.InsertHeader(majescoHeader);
            uniqHeader = (int)majescoHeader.UniqMajescoHeader;

            // Read file items
            IEnumerable<string> items = File.ReadLines(sFilePath);
            items = items.Skip(1).ToArray();
            List<MajescoItem> majescoItems = items.Select(item => new MajescoItem(item)).ToList();

            // Insert items
            foreach (MajescoItem item in majescoItems)
            {
                try
                {
                    item.UniqMajescoHeader = majescoHeader.UniqMajescoHeader;
                    _bdeHelper.InsertItem(item);
                }
                catch (Exception ex)
                {
                    try
                    {
                        _bdeHelper.DeleteItems(uniqHeader);
                    }
                    catch (Exception ex2)
                    {
                        throw new Exception(
                            $"Error occurred inserting Majesco Item Number: {item.MajescoItemNumber}{Environment.NewLine}{ex.Message}{Environment.NewLine}" +
                                $"Error occurred deleting Majesco Items for header: {uniqHeader}{Environment.NewLine}{ex2.Message}");
                    }

                    throw new Exception($"Error occurred inserting Majesco Item Number: {item.MajescoItemNumber}{Environment.NewLine}{ex.Message}");
                }
            }

            // Finally move file to archive after inserting data
            File.Move(sFilePath, newPath);
        }
        catch (Exception ex)
        {
            if (uniqHeader != -1)
            {
                _bdeHelper.HeaderException(uniqHeader, ex.Message);
            }
            throw;
        }
    }

    public void ValidateFiles()
    {
        _bdeHelper.ValidationSync();
    }

    public void Dispose()
    {
        _sdkHelper.Dispose();
        _bdeHelper.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    public async Task<bool> GetTransactionCodes()
    {
        _transactionCodeList = await _sdkHelper.GetTransactionCodes();
        return true;
    }

    #endregion
    
    #region New Business

    public async Task<bool> CreateCarnetPolicies()
    {
        // Sync data
        _bdeHelper.DataSync("Policy");

        List<int> deletePolicies = new();

        // Locate items for processing
        List<ProcessItem> processItems = _bdeHelper.GetProcessItems();

        // Locate distinct carnet/policy numbers
        try
        {
            foreach (ProcessItem item in processItems.Where(item =>
                         item is { TransactionType: not null, CarnetNumber: not null, LineType: not null, ProcessType: not null } &&
                         (item.TransactionType.Equals("NEW", StringComparison.OrdinalIgnoreCase) ||
                          item.TransactionType.Equals("RENEWAL", StringComparison.OrdinalIgnoreCase)) &&
                         item.ProcessType.Equals("Carnet", StringComparison.OrdinalIgnoreCase)))
            {
                if (_dictPolicy.Count == 0)
                {
                    _dictPolicy.Add(item.CarnetNumber!,
                        new List<LineType> { new(item.LineType!, item.UniqMajescoItem, item.EffectiveDate) });
                }
                else if (!_dictPolicy.ContainsKey(item.CarnetNumber!))
                {
                    _dictPolicy.Add(item.CarnetNumber!,
                        new List<LineType> { new(item.LineType!, item.UniqMajescoItem, item.EffectiveDate) });
                }
                else
                {
                    bool exists = _dictPolicy[item.CarnetNumber!].Any(lineType => lineType.Code.Equals(item.LineType));

                    if (!exists)
                    {
                        _dictPolicy[item.CarnetNumber!]
                            .Add(new(item.LineType!, item.UniqMajescoItem, item.EffectiveDate));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while building policy dictionary:{Environment.NewLine}{ex.Message}");
        }


        // Begin policy creation
        if (_dictPolicy.Count <= 0) return true;

        foreach (ProcessItem item in processItems.Where(processItem =>
                     processItem is { TransactionType: not null, CarnetNumber: not null, LineType: not null, ProcessType: not null } &&
                     processItem.TransactionType.Equals("NEW", StringComparison.OrdinalIgnoreCase) &&
                     processItem.ProcessType.Equals("Carnet", StringComparison.OrdinalIgnoreCase) &&
                     _dictPolicy.ContainsKey(processItem.CarnetNumber!)))
        {
            if (item.CarnetNumber == null || string.IsNullOrEmpty(item.LineType) || item.PolicyId != -1) continue;

            try
            {
                // Get existing policy
                Policy policy = await _sdkHelper.GetPolicy(item.ClientId, item.MatchPolicyId);

                // Get existing lines
                List<Line> lines = await _sdkHelper.GetLinesByPolicyId(item.MatchPolicyId);

                // Get existing line
                Line line = await _sdkHelper.GetLine(item.MatchLineId);

                // Create the new policy object
                Policy newPolicy = BuildPolicy(policy, line, item.CarnetNumber, Convert.ToDateTime(item.EffectiveDate),
                    Convert.ToDateTime(item.ExpirationDate));

                // Insert policy object
                item.PolicyId = await _sdkHelper.InsertPolicy(newPolicy);

                // Get the newly created line to add Line ID to dictionary and add producer broker commission items
                Line newLine = await _sdkHelper.GetLineByPolicyId(item.PolicyId);

                // Update the line ID in dictionary for the created line
                foreach (LineType? lineType in _dictPolicy[item.CarnetNumber].Where(lineType => lineType.Code.Trim().Equals(newLine.LineTypeCode.Trim())))
                {
                    lineType.LineId = newLine.LineID;
                }

                // Copy fields from existing line and update
                newLine = UpdateLine(line, newLine);

                // Update line
                bool updated = await _sdkHelper.UpdateLine(newLine);

                // Add any missing existing lines
                foreach (Line addLine in lines.Where(addLine => !addLine.LineTypeCode.Equals(line.LineTypeCode)))
                {
                    newLine = UpdateLine(addLine, newLine, lines);
                    newLine.LineID = 0;
                    int lineId = await _sdkHelper.InsertLine(newLine);
                }

                // Get current lines
                lines = await _sdkHelper.GetLinesByPolicyId(item.PolicyId);

                // Update Line ID for matching dictionary items and insert any missing lines.
                if (_dictPolicy[item.CarnetNumber].Count > 1)
                {
                    foreach (LineType value in _dictPolicy[item.CarnetNumber])
                    {
                        bool found = false;
                        foreach (Line currentLine in lines.Where(currentLine =>
                                     currentLine.LineTypeCode.Equals(value.Code)))
                        {
                            found = true;
                            value.LineId = currentLine.LineID;
                            break;
                        }

                        if (!found)
                        {
                            Line templateLine = lines.FirstOrDefault()!;
                            templateLine.LineTypeCode = value.Code;
                            templateLine.LineID = 0;
                            value.LineId = await _sdkHelper.InsertLine(templateLine);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                item.Exception = ex.Message;

                if (deletePolicies.Count == 0 || !deletePolicies.Contains(item.PolicyId))
                {
                    deletePolicies.Add(item.PolicyId);
                }
            }

            // Update Policy ID for all matching process items
            foreach (ProcessItem processItem in processItems.Where(processItem =>
                         processItem.CarnetNumber!.Equals(item.CarnetNumber) && processItem.EffectiveDate ==
                         item.EffectiveDate))
            {
                processItem.PolicyId = item.PolicyId;
            }
        }

        // Missing lines
        try
        {
            foreach (ProcessItem item in processItems.Where(item => item is { LineId: -1, CarnetNumber: not null, TransactionType: "NEW" }))
            {
                if (!_dictPolicy.TryGetValue(item.CarnetNumber!, out List<LineType>? lines)) continue;

                foreach (LineType line in lines.Where(line =>
                             item.LineType!.Equals(line.Code) && item.EffectiveDate == line.EffectiveDate))
                {
                    item.LineId = line.LineId;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while checking for missing lines:{Environment.NewLine}{ex.Message}");
        }

        // Delete policies if any portion failed to create
        if (deletePolicies.Count > 0)
        {
            foreach (int policyId in deletePolicies)
            {
                try
                {
                    if (policyId != -1)
                    {
                        Delete_PolicyResponse response = await _sdkHelper.DeletePolicy(policyId);
                    }
                }
                catch (Exception ex)
                {
                    foreach (ProcessItem item in processItems.Where(item =>
                                 item.TransactionType != null &&
                                 item.TransactionType.Equals("NEW", StringComparison.OrdinalIgnoreCase) &&
                                 _dictPolicy.ContainsKey(item.CarnetNumber!)))
                    {
                        if (item.PolicyId == policyId)
                        {
                            item.Exception = string.IsNullOrWhiteSpace(item.Exception)
                                ? ex.Message
                                : $"{item.Exception}{Environment.NewLine}{ex.Message}";
                        }
                    }
                }
            }
        }

        // Update items
        try
        {
            foreach (ProcessItem item in processItems.Where(item =>
                         item is { ProcessType: not null, TransactionType: not null } &&
                         item.TransactionType.Equals("NEW", StringComparison.OrdinalIgnoreCase) &&
                         item.ProcessType.Equals("Carnet")))
            {
                _bdeHelper.UpdateItem(item);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while updating integration database:{Environment.NewLine}{ex.Message}");
        }

        return true;
    }

    private static Policy BuildPolicy(Policy oPolicy, Line oLine, string sPolicyNumber, DateTime dtEffectiveDate, DateTime dtExpirationDate)
    {
        return new Policy
        {
            AccountID = oPolicy.AccountID,
            AgencyCode = oPolicy.AgencyCode,
            AgencyCommissionAmount = oPolicy.AgencyCommissionAmount,
            AgencyCommissionPercent = oPolicy.AgencyCommissionPercent,
            AgencyCommissionType = oPolicy.AgencyCommissionType,
            AgreementID = oPolicy.AgreementID,
            AnnualizedCommission = oPolicy.AnnualizedCommission,
            AnnualizedPremium = oPolicy.AnnualizedPremium,
            BillingModeOption = oPolicy.BillingModeOption,
            BranchCode = oPolicy.BranchCode,
            DefaultCommissionAgreement = oPolicy.DefaultCommissionAgreement,
            DepartmentCode = oPolicy.DepartmentCode,
            Description = "Carnet Fees",
            EffectiveDate = dtEffectiveDate,
            EstimatedCommission = oPolicy.EstimatedCommission,
            EstimatedPremium = oPolicy.EstimatedPremium,
            ExpirationDate = dtExpirationDate,
            IsProspectivePolicy = oPolicy.IsProspectivePolicy,
            IssuingCompanyLookupCode = oLine.IssuingCompanyLookupCode,
            IssuingLocationCode = oLine.IssuingLocationCode,
            LineEstimatedCommission = oLine.EstimatedCommission,
            LineEstimatedPremium = oLine.EstimatedPremium,
            LineTypeCode = oLine.LineTypeCode,
            PayableContractID = oLine.PayableContractID,
            PolicyNumber = sPolicyNumber.Truncate(25),
            PolicyTypeCode = "CARN",
            PrefillID = oPolicy.PrefillID,
            PremiumPayableLookupCode = oLine.PremiumPayableLookupCode,
            PremiumPayableTypeCode = oLine.PremiumPayableTypeCode,
            ProfitCenterCode = oLine.ProfitCenterCode,
            Source = oPolicy.Source,
            StatusCode = oLine.StatusCode,
            MultiCarrierSchedule = oPolicy.MultiCarrierSchedule,
            Downloaded = oPolicy.Downloaded,
            EstimatedMonthlyCommission = oPolicy.EstimatedMonthlyCommission,
            EstimatedMonthlyPremium = oPolicy.EstimatedMonthlyPremium,
            TurnOffPolicyDownload = oPolicy.TurnOffPolicyDownload,
            OverrideCommissionAgreementPercentageOrAmount = oPolicy.OverrideCommissionAgreementPercentageOrAmount
        };
    }

    private static Line UpdateLine(Line oSourceLine, Line oNewLine, List<Line>? lsLine = null)
    {
        bool match = false;
        Line matchLine = new();

        if (lsLine is { Count: > 1 })
        {
            foreach (Line line in lsLine.Where(line => line.LineTypeCode.Trim().Equals(oSourceLine.LineTypeCode.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                match = true;
                matchLine = line;
                break;
            }
        }

        if (!match)
        {
            oNewLine.AgencyCommissionAmount = oSourceLine.AgencyCommissionAmount;
            oNewLine.AgencyCommissionPercent = oSourceLine.AgencyCommissionPercent;
            oNewLine.AgencyCommissionType = oSourceLine.AgencyCommissionType;
            oNewLine.AgencyDefinedCategoryItems = oSourceLine.AgencyDefinedCategoryItems;
            oNewLine.AgreementID = oSourceLine.AgreementID;
            oNewLine.AnnualizedCommission = oSourceLine.AnnualizedCommission;
            oNewLine.AnnualizedPremium = oSourceLine.AnnualizedPremium;
            oNewLine.BilledCommission = oSourceLine.BilledCommission;
            oNewLine.BilledPremium = oSourceLine.BilledPremium;
            oNewLine.BillingModeOption = oSourceLine.BillingModeOption;
            oNewLine.BillingValue = oSourceLine.BillingValue;
            oNewLine.DefaultCommissionAgreement = oSourceLine.DefaultCommissionAgreement;
            oNewLine.Downloaded = oSourceLine.Downloaded;
            oNewLine.EstimatedCommission = oSourceLine.EstimatedCommission;
            oNewLine.EstimatedMonthlyCommission = oSourceLine.EstimatedMonthlyCommission;
            oNewLine.EstimatedMonthlyPremium = oSourceLine.EstimatedMonthlyPremium;
            oNewLine.EstimatedMonthlyPremiumAsOfDate = oSourceLine.EstimatedMonthlyPremiumAsOfDate;
            oNewLine.EstimatedPremium = oSourceLine.EstimatedPremium;
            oNewLine.HistoryValue = oSourceLine.HistoryValue;
            oNewLine.IgnoreAtLeastOnePrBrCommissionRequirement = oSourceLine.IgnoreAtLeastOnePrBrCommissionRequirement;
            oNewLine.IssuingCompanyLookupCode = oSourceLine.IssuingCompanyLookupCode;
            oNewLine.IssuingLocationCode = oSourceLine.IssuingLocationCode;
            oNewLine.LineTypeCode = oSourceLine.LineTypeCode;
            oNewLine.LineTypeDescription = oSourceLine.LineTypeDescription;
            oNewLine.OverrideCommissionAgreementPercentageOrAmount = oSourceLine.OverrideCommissionAgreementPercentageOrAmount;
            oNewLine.PayableContractID = oSourceLine.PayableContractID;
            oNewLine.PlanOptionName = oSourceLine.PlanOptionName;
            oNewLine.PremiumPayableTypeCode = oSourceLine.PremiumPayableTypeCode;
            oNewLine.PremiumPayableLookupCode = oSourceLine.PremiumPayableLookupCode;

            if (oSourceLine.ProducerBrokerCommissionsValue != null)
            {
                oNewLine.ProducerBrokerCommissionsValue = oSourceLine.ProducerBrokerCommissionsValue;
                foreach (CommissionItem? commissionItem in oNewLine.ProducerBrokerCommissionsValue.Commissions)
                {
                    commissionItem.Flag = CommissionFlags.Insert;
                }
            }
            else
            {
                oNewLine.ProducerBrokerCommissionsValue = null;
            }

            oNewLine.ProfitCenterCode = oSourceLine.ProfitCenterCode;
            oNewLine.RisksInsured = oSourceLine.RisksInsured;
            oNewLine.RisksInsuredDescription = oSourceLine.RisksInsuredDescription;
            oNewLine.ServicingContacts = oSourceLine.ServicingContacts;
            oNewLine.StatusCode = oSourceLine.StatusCode;
            oNewLine.TotalEligible = oSourceLine.TotalEligible;
            oNewLine.TotalEligibleDescription = oSourceLine.TotalEligibleDescription;

            if (oNewLine.AgencyDefinedCategoryItems.Count <= 0) return oNewLine;

            foreach (AgencyDefinedCodeItem? agencyDefinedCategoryItem in oNewLine.AgencyDefinedCategoryItems.Where(
                         agencyDefinedCategoryItem => !agencyDefinedCategoryItem.ADCCategory.Trim()
                             .Equals("Carnet Flag", StringComparison.OrdinalIgnoreCase)))
            {
                agencyDefinedCategoryItem.Flag = AgencyDefinedCategoryFlags.Insert;
            }
        }
        else
        {
            oNewLine.AgencyCommissionAmount = matchLine.AgencyCommissionAmount;
            oNewLine.AgencyCommissionPercent = matchLine.AgencyCommissionPercent;
            oNewLine.AgencyCommissionType = matchLine.AgencyCommissionType;
            oNewLine.AgencyDefinedCategoryItems = matchLine.AgencyDefinedCategoryItems;
            oNewLine.AgreementID = matchLine.AgreementID;
            oNewLine.AnnualizedCommission = matchLine.AnnualizedCommission;
            oNewLine.AnnualizedPremium = matchLine.AnnualizedPremium;
            oNewLine.BilledCommission = matchLine.BilledCommission;
            oNewLine.BilledPremium = matchLine.BilledPremium;
            oNewLine.BillingModeOption = matchLine.BillingModeOption;
            oNewLine.BillingValue = matchLine.BillingValue;
            oNewLine.DefaultCommissionAgreement = matchLine.DefaultCommissionAgreement;
            oNewLine.Downloaded = matchLine.Downloaded;
            oNewLine.EstimatedCommission = matchLine.EstimatedCommission;
            oNewLine.EstimatedMonthlyCommission = matchLine.EstimatedMonthlyCommission;
            oNewLine.EstimatedMonthlyPremium = matchLine.EstimatedMonthlyPremium;
            oNewLine.EstimatedMonthlyPremiumAsOfDate = matchLine.EstimatedMonthlyPremiumAsOfDate;
            oNewLine.EstimatedPremium = matchLine.EstimatedPremium;
            oNewLine.HistoryValue = matchLine.HistoryValue;
            oNewLine.IgnoreAtLeastOnePrBrCommissionRequirement = matchLine.IgnoreAtLeastOnePrBrCommissionRequirement;
            oNewLine.IssuingCompanyLookupCode = matchLine.IssuingCompanyLookupCode;
            oNewLine.IssuingLocationCode = matchLine.IssuingLocationCode;
            oNewLine.LineTypeCode = matchLine.LineTypeCode;
            oNewLine.LineTypeDescription = matchLine.LineTypeDescription;
            oNewLine.OverrideCommissionAgreementPercentageOrAmount = matchLine.OverrideCommissionAgreementPercentageOrAmount;
            oNewLine.PayableContractID = matchLine.PayableContractID;
            oNewLine.PlanOptionName = matchLine.PlanOptionName;
            oNewLine.PremiumPayableTypeCode = matchLine.PremiumPayableTypeCode;
            oNewLine.PremiumPayableLookupCode = matchLine.PremiumPayableLookupCode;

            if (matchLine.ProducerBrokerCommissionsValue != null)
            {
                oNewLine.ProducerBrokerCommissionsValue = matchLine.ProducerBrokerCommissionsValue;
                foreach (CommissionItem? commissionItem in oNewLine.ProducerBrokerCommissionsValue.Commissions)
                {
                    commissionItem.Flag = CommissionFlags.Insert;
                }
            }
            else
            {
                oNewLine.ProducerBrokerCommissionsValue = null;
            }

            oNewLine.ProfitCenterCode = matchLine.ProfitCenterCode;
            oNewLine.RisksInsured = matchLine.RisksInsured;
            oNewLine.RisksInsuredDescription = matchLine.RisksInsuredDescription;
            oNewLine.ServicingContacts = matchLine.ServicingContacts;
            oNewLine.StatusCode = matchLine.StatusCode;
            oNewLine.TotalEligible = matchLine.TotalEligible;
            oNewLine.TotalEligibleDescription = matchLine.TotalEligibleDescription;

            if (oNewLine.AgencyDefinedCategoryItems.Count <= 0) return oNewLine;

            if (!oNewLine.LineTypeCode.Equals("CARN", StringComparison.OrdinalIgnoreCase)) return oNewLine;

            foreach (AgencyDefinedCodeItem? agencyDefinedCategoryItem in oNewLine.AgencyDefinedCategoryItems.Where(
                         agencyDefinedCategoryItem => !agencyDefinedCategoryItem.ADCCategory.Trim()
                             .Equals("Carnet Flag", StringComparison.OrdinalIgnoreCase)))
            {
                agencyDefinedCategoryItem.Flag = AgencyDefinedCategoryFlags.Insert;
            }
        }

        return oNewLine;
    }

    private async Task<Line> UpdatePrBrCommissionAgreements(Policy oPolicy, Line oLine)
    {
        string agreementType = "producer";

        // Check producer broker commissions
        if (oLine.ProducerBrokerCommissionsValue.Commissions.Count <= 0) return oLine;

        foreach (CommissionItem? commissionItem in oLine.ProducerBrokerCommissionsValue.Commissions.Where(
                     commissionItem => !string.IsNullOrWhiteSpace(commissionItem.LookupCode)))
        {
            try
            {
                if (commissionItem.ProducerBrokerCode.Equals("PPAY"))
                {
                    List<EmployeeCommission> employeeCommissions =
                        await _sdkHelper.GetEmployeeCommissionAgreements(commissionItem.LookupCode);

                    if (employeeCommissions is { Count: > 0 })
                    {
                        EmployeeCommission commission =
                            CommissionAgreementHelper.GetValidEmployeeAgreement(oPolicy, oLine,
                                employeeCommissions);

                        commissionItem.CommissionAgreementID = commission.CommissionID;
                        commissionItem.CommissionType = commission.AgreementValue.CommissionTypeCode;
                        commissionItem.CommissionPercent = commission.AgreementValue.CommissionPercent;
                        commissionItem.CommissionAmount = commission.AgreementValue.CommissionAmount;
                    }
                }
                else if (commissionItem.ProducerBrokerCode.Equals("BPAY"))
                {
                    agreementType = "broker";

                    List<BrokerCommission> brokerCommissions =
                        await _sdkHelper.GetBrokerCommissionAgreementByLookup(commissionItem.LookupCode);

                    if (brokerCommissions is { Count: > 0 })
                    {
                        BrokerCommission commission =
                            CommissionAgreementHelper.GetValidBrokerAgreement(oPolicy, oLine,
                                brokerCommissions);

                        commissionItem.CommissionAgreementID = commission.CommissionID;
                        commissionItem.CommissionType = commission.AgreementValue.CommissionTypeCode;
                        commissionItem.CommissionPercent = commission.AgreementValue.CommissionPercent;
                        commissionItem.CommissionAmount = commission.AgreementValue.CommissionAmount;
                    }
                }
            }
            catch
            {
                if (commissionItem.CommissionAmount == null && commissionItem.CommissionPercent == null)
                {
                    throw new Exception(
                        $"Unable to insert line. A valid {agreementType} commission agreement could not be located and {agreementType} commission amount/percent are blank for PrBr {commissionItem.OrderNumber}.");
                }
            }
        }

        return oLine;
    }

    private async Task<Line> UpdateLineCommissionAgreements(Policy oPolicy, Line oLine, Company oBillingCompany)
    {
        try
        {
            // Get commission agreements
            if (oLine.PremiumPayableTypeCode.Equals("BR"))
            {
                List<BrokerCommission> brokerCommissions =
                    await _sdkHelper.GetBrokerCommissionAgreement(oBillingCompany.CompanyID);

                if (brokerCommissions is { Count: > 0 })
                {
                    BrokerCommission commission =
                        CommissionAgreementHelper.GetValidBrokerAgreement(oPolicy, oLine,
                            brokerCommissions);

                    oLine.AgreementID = commission.CommissionID;
                    oLine.AgencyCommissionType = commission.AgreementValue.CommissionTypeCode;
                    oLine.AgencyCommissionPercent = commission.AgreementValue.CommissionPercent;
                    oLine.AgencyCommissionAmount = commission.AgreementValue.CommissionAmount;
                }
            }
            else
            {
                List<CompanyCommission> companyCommissions =
                    await _sdkHelper.GetCompanyCommissionAgreement(oBillingCompany.CompanyID);

                if (companyCommissions is { Count: > 0 })
                {
                    CompanyCommission commission =
                        CommissionAgreementHelper.GetValidCompanyAgreement(oPolicy, oLine,
                            companyCommissions);

                    oLine.AgreementID = commission.CommissionID;
                    oLine.AgencyCommissionType = commission.AgreementValue.CommissionTypeCode;
                    oLine.AgencyCommissionPercent = commission.AgreementValue.CommissionPercent;
                    oLine.AgencyCommissionAmount = commission.AgreementValue.CommissionAmount;
                }
            }
        }
        catch
        {
            if (oLine.AgencyCommissionAmount == null && oLine.AgencyCommissionPercent == null)
            {
                throw new Exception(
                    "Unable to insert line. A valid commission agreement could not be located and line agency commission amount/percent are blank.");
            }
        }

        return oLine;
    }

    public async Task<bool> CreateBondCargoPolicies()
    {
        // Sync data
        _bdeHelper.DataSync("Policy");

        List<int> deletePolicies = new();

        // Locate items for processing
        List<ProcessItem> items = _bdeHelper.GetProcessItems();

        foreach (ProcessItem item in items.Where(item =>
                     item is { TransactionType: not null, CarnetNumber: not null, LineType: not null, ProcessType: not null, PolicyId: -1, LineId: -1 } 
                     && (item.TransactionType.Equals("NEW", StringComparison.OrdinalIgnoreCase) 
                         || item.TransactionType.Equals("RENEWAL", StringComparison.OrdinalIgnoreCase))
                     && ( item.ProcessType.Equals("Bond", StringComparison.OrdinalIgnoreCase) 
                          || item.ProcessType.Equals("Cargo", StringComparison.OrdinalIgnoreCase))))
        {
            if (item.CarnetNumber == null || string.IsNullOrEmpty(item.LineType) || item.PolicyId != -1) continue;

            try
            {
                // Get existing policy
                Policy policy = await _sdkHelper.GetPolicy(item.ClientId, item.MatchPolicyId);

                // Get existing lines
                List<Line> lines = await _sdkHelper.GetLinesByPolicyId(item.MatchPolicyId);

                // Get existing line
                Line line = await _sdkHelper.GetLine(item.MatchLineId);

                // Get billing company associated to issuing company. SDK Helper method will return the configured billing company
                Company billingCompany = !string.IsNullOrWhiteSpace(item.CompanyLookupCode)
                    ? await _sdkHelper.GetBillingFromIssuingCompanyId((int)item.CompanyId!)
                    : new();

                // Look for an existing line match
                if (!line.IssuingCompanyLookupCode.Equals(item.CompanyLookupCode, StringComparison.OrdinalIgnoreCase) &&
                    !line.PremiumPayableLookupCode.Equals(billingCompany.LookupCode, StringComparison.OrdinalIgnoreCase))
                {
                    if (lines.Count > 0)
                    {
                        foreach (Line matchLine in lines.Where(matchLine => matchLine.IssuingCompanyLookupCode.Equals(item.CompanyLookupCode, StringComparison.OrdinalIgnoreCase) &&
                                                                            matchLine.PremiumPayableLookupCode.Equals(billingCompany.LookupCode, StringComparison.OrdinalIgnoreCase) &&
                                                                            matchLine.LineTypeCode.Equals(item.LineType, StringComparison.OrdinalIgnoreCase)))
                        {
                            line = matchLine;
                            item.PolicyId = matchLine.PolicyID;
                            item.LineId = matchLine.LineID;
                            break;
                        }
                    }
                }

                // Check transaction effective date to see if it falls within range.
                if (Convert.ToDateTime(policy.EffectiveDate).Date <= Convert.ToDateTime(item.EffectiveDate) &&
                    Convert.ToDateTime(policy.ExpirationDate).Date >= Convert.ToDateTime(item.EffectiveDate))
                {
                    if (!string.IsNullOrWhiteSpace(item.CompanyLookupCode))
                    {
                        // Create new line if companies do not match
                        if (billingCompany.CompanyID != 0 && !line.PremiumPayableLookupCode.Equals(billingCompany.LookupCode))
                        {
                            line.LineID = 0;
                            line.IssuingCompanyLookupCode = item.CompanyLookupCode;
                            line.PremiumPayableLookupCode = billingCompany.LookupCode;
                            line.AgreementID = 0;

                            // Update line commission agreements
                            line = await UpdateLineCommissionAgreements(policy, line, billingCompany);

                            // Update pr br commission agreements
                            line = await UpdatePrBrCommissionAgreements(policy, line);

                            // Grab the first company Payable Contract ID for Account Current ONLY
                            line.PayableContractID = await _sdkHelper.GetFirstCompanyPayableContractId(billingCompany.CompanyID, policy.EffectiveDate);
                            line.DefaultCommissionAgreement = true;

                            foreach (AgencyDefinedCodeItem? category in line.AgencyDefinedCategoryItems)
                            {
                                category.Flag = AgencyDefinedCategoryFlags.Insert;
                            }

                            foreach (CommissionItem? commissionItem in line.ProducerBrokerCommissionsValue.Commissions)
                            {
                                commissionItem.Flag = CommissionFlags.Insert;
                            }

                            item.PolicyId = item.MatchPolicyId;
                            item.LineId = await _sdkHelper.InsertLine(line);
                        }
                        else
                        {
                            item.PolicyId = line.PolicyID;
                            item.LineId = line.LineID;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(item.UnderwritingCompanyCode))
                        {
                            item.Exception =
                                "The underwriting company code has not been mapped. Refer to MajescoCompanyMapping.";
                        }
                        else
                        {
                            item.PolicyId = line.PolicyID;
                            item.LineId = line.LineID;
                        }
                    }
                }
                else
                {
                    if (Convert.ToDateTime(policy.ExpirationDate).Date <= Convert.ToDateTime(item.EffectiveDate))
                    {
                        Renew renew = await _sdkHelper.GetRenew(policy.PolicyID);
                        renew.ExpirationDate = policy.ExpirationDate.AddYears(1);

                        foreach (LineItem? lineItem in renew.Lines)
                        {
                            lineItem.Flag = Flags.Renew;

                            switch (line.StatusCode)
                            {
                                case "REN":
                                    lineItem.StatusCode = "RE2";
                                    break;
                                case "RE2":
                                    break;
                                default:
                                    lineItem.StatusCode = "REN";
                                    break;
                            }
                        }

                        Action_Policy_RenewResponse response = await _sdkHelper.RenewPolicy(renew);
                        item.PolicyId = response.Action_Policy_RenewResult;

                        Policy newPolicy = await _sdkHelper.GetPolicy(item.ClientId, response.Action_Policy_RenewResult);

                        List<Line> newLines = await _sdkHelper.GetLinesByPolicyId(response.Action_Policy_RenewResult);

                        foreach (Line newLine in newLines.Where(newLine => newLine.IssuingCompanyLookupCode.Equals(item.CompanyLookupCode,
                                     StringComparison.OrdinalIgnoreCase)))
                        {
                            item.LineId = newLine.LineID;
                            break;
                        }

                        foreach (ProcessItem processItem in items.Where(processItem =>
                                     processItem is
                                     {
                                         TransactionType: not null, CarnetNumber: not null, LineType: not null,
                                         ProcessType: not null, PolicyId: -1, LineId: -1
                                     }
                                     && (processItem.TransactionType.Equals("NEW", StringComparison.OrdinalIgnoreCase) ||
                                         processItem.TransactionType.Equals("RENEWAL", StringComparison.OrdinalIgnoreCase) ||
                                         processItem.TransactionType.Equals("ENDORSEMENT", StringComparison.OrdinalIgnoreCase) ||
                                         processItem.TransactionType.Equals("CANCELLATION", StringComparison.OrdinalIgnoreCase) ||
                                         processItem.TransactionType.Equals("REINSTATEMENT", StringComparison.OrdinalIgnoreCase))
                                     && !processItem.ProcessType.Equals("Carnet", StringComparison.OrdinalIgnoreCase)))
                        {
                            foreach (Line newLine in newLines.Where(newLine => 
                                         newLine.BillingValue.InvoiceToSiteID.Trim().Equals(processItem.LocationId!.Trim()) &&
                                         newLine.LineTypeCode.Equals(processItem.LineType) &&
                                         Convert.ToDateTime(newPolicy.EffectiveDate).Date <= Convert.ToDateTime(processItem.EffectiveDate).Date)
                                            .Where(newLine => 
                                                !string.IsNullOrWhiteSpace(processItem.CompanyLookupCode) &&
                                                newLine.IssuingCompanyLookupCode.Equals(processItem.CompanyLookupCode)))
                            {
                                if (!processItem.TransactionType!.Equals("NEW", StringComparison.OrdinalIgnoreCase) || !processItem.TransactionType!.Equals("RENEWAL", StringComparison.OrdinalIgnoreCase))
                                {
                                    processItem.MatchPolicyId = newPolicy.PolicyID;
                                    processItem.MatchLineId = newLine.LineID;
                                }

                                processItem.PolicyId = newPolicy.PolicyID;
                                processItem.LineId = newLine.LineID;
                                break;
                            }
                        }
                    }
                    else if (Convert.ToDateTime(policy.EffectiveDate).Date > Convert.ToDateTime(item.EffectiveDate))
                    {
                        item.Exception = "Majesco Item has an effective date prior to the matched policy/line effective date.";
                    }
                }
            }
            catch (Exception ex)
            {
                item.Exception = ex.Message;
            }
        }

        // Update items
        try
        {
            foreach (ProcessItem item in items.Where(item =>
                         item is { ProcessType: not null, TransactionType: not null } &&
                         (item.TransactionType.Equals("NEW", StringComparison.OrdinalIgnoreCase) ||
                          item.TransactionType.Equals("RENEWAL", StringComparison.OrdinalIgnoreCase) ||
                          item.TransactionType.Equals("ENDORSEMENT", StringComparison.OrdinalIgnoreCase) ||
                          item.TransactionType.Equals("CANCELLATION", StringComparison.OrdinalIgnoreCase) ||
                          item.TransactionType.Equals("REINSTATEMENT", StringComparison.OrdinalIgnoreCase)) &&
                         !item.ProcessType.Equals("Carnet")))
            {
                _bdeHelper.UpdateItem(item);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while updating integration database:{Environment.NewLine}{ex.Message}");
        }

        return true;
    }

    public async Task<bool> CreateNewEndorsementTransactionsActivities()
    {
        // Sync data
        _bdeHelper.DataSync("Policy");
        _bdeHelper.DataSync("TransactionSync");

        // Locate items for processing
        List<ProcessItem> processItems = _bdeHelper.GetProcessItems();

        IOrderedEnumerable<ProcessItem> processItemsSorted = processItems.Where(processItem => 
            processItem is { TransactionType: not null, PolicyId: not -1, LineId: not -1, TransactionId: -1 } &&
            (processItem.TransactionType.Equals("NEW", StringComparison.OrdinalIgnoreCase) ||
             processItem.TransactionType.Equals("RENEWAL", StringComparison.OrdinalIgnoreCase) ||
             processItem.TransactionType.Equals("ENDORSEMENT", StringComparison.OrdinalIgnoreCase) ||
             processItem.TransactionType.Equals("CANCELLATION", StringComparison.OrdinalIgnoreCase) ||
             processItem.TransactionType.Equals("REINSTATEMENT", StringComparison.OrdinalIgnoreCase))).OrderBy(x => x.UniqMajescoItem);

        foreach (ProcessItem processItem in processItemsSorted)
        {
            try
            {
                // Check transaction code to determine transaction processing workflow is non-tax
                bool isTaxCode = _transactionCodeList.Any(transactionCode =>
                    transactionCode.ClassDescription.Contains("Government", StringComparison.OrdinalIgnoreCase) &&
                    transactionCode.Code.Equals(processItem.TransactionCode, StringComparison.OrdinalIgnoreCase));

                if (isTaxCode) continue;

                // Get existing policy
                Policy policy = await _sdkHelper.GetPolicy(processItem.ClientId, processItem.PolicyId);

                // Get existing line
                Line line = await _sdkHelper.GetLine(processItem.LineId);

                // Policy resync
                if (processItem.TransactionCode!.Equals("AFEE", StringComparison.OrdinalIgnoreCase))
                {
                    // Get existing lines
                    List<Line> lines = await _sdkHelper.GetLinesByPolicyId(processItem.MatchPolicyId);

                    // Get billing company associated to issuing company. SDK Helper method will return the configured billing company
                    Company billingCompany = !string.IsNullOrWhiteSpace(processItem.CompanyLookupCode)
                        ? await _sdkHelper.GetBillingFromIssuingCompanyId((int)processItem.CompanyId!)
                        : new();

                    // Look for an existing line match
                    if (!line.IssuingCompanyLookupCode.Equals(processItem.CompanyLookupCode, StringComparison.OrdinalIgnoreCase) &&
                        !line.PremiumPayableLookupCode.Equals(billingCompany.LookupCode, StringComparison.OrdinalIgnoreCase))
                    {
                        if (lines.Count > 0)
                        {
                            foreach (Line matchLine in lines.Where(matchLine => matchLine.IssuingCompanyLookupCode.Equals(processItem.CompanyLookupCode, StringComparison.OrdinalIgnoreCase) &&
                                         matchLine.PremiumPayableLookupCode.Equals(billingCompany.LookupCode, StringComparison.OrdinalIgnoreCase) &&
                                         matchLine.LineTypeCode.Equals(processItem.LineType, StringComparison.OrdinalIgnoreCase)))
                            {
                                line = matchLine;
                                processItem.PolicyId = matchLine.PolicyID;
                                processItem.LineId = matchLine.LineID;
                                break;
                            }
                        }
                    }
                }

                // Get transaction amount
                decimal transactionAmount = Convert.ToDecimal(processItem.Amount) + Convert.ToDecimal(processItem.DeferredAmount);

                // Change transaction code for reinstatement
                if (processItem.TransactionType!.Equals("REINSTATEMENT", StringComparison.OrdinalIgnoreCase))
                {
                    processItem.TransactionCode = "REIN";
                }

                // Generate transaction object
                Transaction transaction = TransactionHelper.GenerateTransaction(policy, line, processItem.ProcessTypeDescription, transactionAmount, processItem);

                // Update transaction for cancellations
                if (processItem.CreditDebit == "CR" || processItem.TransactionCode!.Equals("CANC", StringComparison.OrdinalIgnoreCase))
                {
                    transaction.TransactionAmount *= -1;
                    transaction.CommissionsValue.Splits[0].AgencySplitAmount *= -1;
                }

                // Insert transaction
                processItem.TransactionId = await _sdkHelper.InsertTransaction(transaction);

                // Get inserted transaction to update information
                transaction = await _sdkHelper.GetTransaction(processItem.TransactionId);

                // Update item number
                if (!string.IsNullOrEmpty(transaction.ItemNumber))
                {
                    processItem.ItemNumber = Convert.ToInt32(transaction.ItemNumber);
                }

                // Update bill number
                if (processItem.BillNumber == -1)
                {
                    // Get invoice and item number for inserted transaction
                    processItem.InvoiceNumber = (int)transaction.InvoiceValue.SendInvoiceTos.FirstOrDefault()!.InvoiceNumber!;
                    processItem.BillNumber = Convert.ToInt32(transaction.BillNumber);

                    // It's OK that renewals will process first.
                    IOrderedEnumerable<ProcessItem> updateItems = processItems.Where(updateItem =>
                        updateItem.ClientId == processItem.ClientId &&
                        updateItem.PolicyId == processItem.PolicyId &&
                        (updateItem.TransactionType!.Equals("NEW", StringComparison.OrdinalIgnoreCase) ||
                         updateItem.TransactionType!.Equals("RENEWAL", StringComparison.OrdinalIgnoreCase) ||
                         updateItem.TransactionType!.Equals("ENDORSEMENT", StringComparison.OrdinalIgnoreCase) ||
                         updateItem.TransactionType!.Equals("CANCELLATION", StringComparison.OrdinalIgnoreCase) ||
                         updateItem.TransactionType!.Equals("CANCELLATION", StringComparison.OrdinalIgnoreCase))).OrderBy(x => x.UniqMajescoItem);

                    foreach (ProcessItem updateItem in updateItems)
                    {
                        updateItem.BillNumber = processItem.BillNumber;
                        updateItem.InvoiceNumber = processItem.InvoiceNumber;
                    }
                }

                // Deferred amount logic
                if (processItem.DeferredAmount != 0.00M && processItem.TransactionType!.Equals("NEW", StringComparison.OrdinalIgnoreCase))
                {
                    processItem.TransactionCode = "AFEE";

                    decimal deferredAmount = Convert.ToDecimal(processItem.DeferredAmount);

                    if (deferredAmount > 0)
                    {
                        deferredAmount *= -1;
                    }

                    // Generate deferred transaction object
                    Transaction deferredTransaction = TransactionHelper.GenerateTransaction(policy, line, $"{processItem.ProcessTypeDescription}_Agency Discount", deferredAmount, processItem);

                    // Insert agency fee and record new transaction id for payment processing
                    processItem.DeferredTransactionId = await _sdkHelper.InsertTransaction(deferredTransaction);
                }

                // Generate Activity
                if (processItem.TransactionId != -1)
                {
                    Activity activity = ActivityHelper.GenerateActivity(policy, processItem.TransactionId, processItem);
                    processItem.ActivityId = await _sdkHelper.InsertActivity(activity);
                    processItem.Processed = 1;
                    processItem.ProcessedDate = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                processItem.Exception = ex.Message;
            }
        }

        foreach (ProcessItem processItem in processItems)
        {
            _bdeHelper.UpdateItem(processItem);
        }

        return true;
    }

    public async Task<bool> CreateNewEndorsementGenerateTaxTaxActivities()
    {
        // Sync data
        string taxFeeTransactionCodes = _transactionCodeList
            .Where(transaction =>
                transaction.ClassDescription.Contains("Government", StringComparison.OrdinalIgnoreCase)).Aggregate(string.Empty, (current, transaction) => $"{current}{transaction.Code};");

        _bdeHelper.GenerateTaxFeeSync(taxFeeTransactionCodes);

        // Locate items for processing
        List<ProcessItem> processItems = _bdeHelper.GetProcessItems();

        IOrderedEnumerable<ProcessItem> processItemsSorted = processItems.Where(processItem =>
            processItem is { TransactionType: not null, PolicyId: not -1, LineId: not -1, ApplyToTransactionId: not -1 } &&
            (processItem.TransactionType.Equals("NEW", StringComparison.OrdinalIgnoreCase) ||
             processItem.TransactionType.Equals("RENEWAL", StringComparison.OrdinalIgnoreCase) ||
             processItem.TransactionType.Equals("ENDORSEMENT", StringComparison.OrdinalIgnoreCase) ||
             processItem.TransactionType.Equals("CANCELLATION", StringComparison.OrdinalIgnoreCase) ||
             processItem.TransactionType.Equals("REINSTATEMENT", StringComparison.OrdinalIgnoreCase))).OrderBy(x => x.UniqMajescoItem);

        foreach (ProcessItem processItem in processItemsSorted)
        {
            try
            {
                // Check transaction code to determine transaction processing workflow is tax
                TransactionCode transactionCode = new();

                foreach (TransactionCode code in _transactionCodeList.Where(code => code.Code.Equals(processItem.TransactionCode, StringComparison.OrdinalIgnoreCase)))
                {
                    transactionCode = code;
                    break;
                }

                bool isTaxCode =
                    transactionCode.ClassDescription.Contains("Government", StringComparison.OrdinalIgnoreCase);

                if (!isTaxCode) continue;

                // Get existing policy
                Policy policy = await _sdkHelper.GetPolicy(processItem.ClientId, processItem.PolicyId);

                // Get generate tax fee
                GenerateTaxFee generateTaxFee = await _sdkHelper.GetGenerateTaxFee(Convert.ToInt32(processItem.ApplyToTransactionId));

                // Update tax fee amount and remove all other not applicable transaction codes from object
                if (generateTaxFee.GenerateTaxFeeItems.Count == 1)
                {
                    // Get transaction info
                    Transaction transaction =
                        await _sdkHelper.GetTransaction(Convert.ToInt32(processItem.ApplyToTransactionId));

                    // Get vendor for account
                    TaxFeeInfo taxFeeInfo = _bdeHelper.GetTaxFeeInfo(Convert.ToInt32(processItem.ApplyToTransactionId));

                    if (taxFeeInfo is { UniqVendor: -1, UniqContactName: -1, TaxableAmount: 0, CdStateCode: "" })
                        throw new Exception("Get tax fee info returned no results.");

                    // Populate TaxFeeValue
                    generateTaxFee.GenerateTaxFeeItems[0].TaxFeeValue.TypeCode = "$";
                    generateTaxFee.GenerateTaxFeeItems[0].TaxFeeValue.Amount = Convert.ToDecimal(processItem.Amount);
                    generateTaxFee.GenerateTaxFeeItems[0].TaxFeeValue.Percentage = 0;
                    generateTaxFee.GenerateTaxFeeItems[0].TaxFeeValue.Description = processItem.Description;
                    generateTaxFee.GenerateTaxFeeItems[0].TaxFeeValue.GovernmentEntityID = taxFeeInfo.UniqVendor;
                    generateTaxFee.GenerateTaxFeeItems[0].TaxFeeValue.GovernmentContactID = taxFeeInfo.UniqContactName;
                    generateTaxFee.GenerateTaxFeeItems[0].TaxFeeValue.StateProvinceCode = taxFeeInfo.CdStateCode;
                    generateTaxFee.GenerateTaxFeeItems[0].TaxFeeValue.TaxableTransactionCode = transaction.TransactionCode;
                    generateTaxFee.GenerateTaxFeeItems[0].TaxFeeValue.TaxableAmount = taxFeeInfo.TaxableAmount;
                    generateTaxFee.GenerateTaxFeeItems[0].TaxFeeValue.TaxFeeCode = transactionCode.Code;
                }
                else
                {
                    foreach (GenerateTaxFeeItem? generateTaxFeeItem in generateTaxFee.GenerateTaxFeeItems.ToList())
                    {
                        if (generateTaxFeeItem.TaxFeeValue.TaxFeeCode.Equals(processItem.TransactionCode, StringComparison.OrdinalIgnoreCase))
                        {
                            generateTaxFeeItem.TaxFeeValue.TypeCode = "$";
                            generateTaxFeeItem.TaxFeeValue.Amount = Convert.ToDecimal(processItem.Amount);
                            generateTaxFeeItem.TaxFeeValue.Percentage = 0;
                            generateTaxFeeItem.TaxFeeValue.Description = processItem.Description;
                        }
                        else
                        {
                            generateTaxFee.GenerateTaxFeeItems.Remove(generateTaxFeeItem);
                        }
                    }

                    if (generateTaxFee.GenerateTaxFeeItems.Count == 0)
                        throw new Exception("Transaction code does not appear in generated tax fee item for the ApplyToTransactionId listed.");
                    
                }

                // Insert generate tax fee
                processItem.TransactionId = await _sdkHelper.InsertGenerateTaxFee(generateTaxFee);

                // Generate Activity
                if (processItem.TransactionId == -1) continue;

                Activity activity = ActivityHelper.GenerateActivity(policy, processItem.TransactionId, processItem);
                processItem.ActivityId = await _sdkHelper.InsertActivity(activity);
                processItem.Processed = 1;
                processItem.ProcessedDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                processItem.Exception = ex.Message;
            }
        }

        foreach (ProcessItem processItem in processItems)
        {
            _bdeHelper.UpdateItem(processItem);
        }

        return true;
    }

    #endregion

    #region Payments

    public async Task<bool> ProcessPayments()
    {
        // Sync data
        _bdeHelper.DataSync("Reprocess Payment");

        // Locate items for processing
        List<ProcessItem> processItems = _bdeHelper.GetProcessItems();

        foreach (ProcessItem processItem in processItems
                     .Where(processItem =>
                         processItem.TransactionType!.Equals("PAYMENT_ADJUSTMENT", StringComparison.OrdinalIgnoreCase))
                     .Where(processItem => _bdeHelper.PaymentAdjustmentCheck(processItem)))
        {
            processItem.TransactionType = "VIRTUAL_PAYMENT";
        }

        IOrderedEnumerable<ProcessItem> sortedProcessItems = processItems.Where(processItem =>
            processItem is { TransactionType: not null, PolicyId: not -1, LineId: not -1, ApplyToTransactionId: not -1, ReceiptId: -1 } &&
            (processItem.TransactionType.Equals("PAYMENT", StringComparison.OrdinalIgnoreCase) ||
             processItem.TransactionType.Equals("VIRTUAL_PAYMENT", StringComparison.OrdinalIgnoreCase) ||
             processItem.TransactionType.Equals("PAYMENT_TRANSFER_INTERNAL", StringComparison.OrdinalIgnoreCase) ||
             processItem.TransactionType.Equals("PAYMENT_TRASNFER_INTERNAL", StringComparison.OrdinalIgnoreCase) ||
             processItem.TransactionType.Equals("PAYMENT_TRANSFER_EXTERNAL", StringComparison.OrdinalIgnoreCase) ||
             processItem.TransactionType.Equals("PAYMENT_TRASNFER_EXTERNAL", StringComparison.OrdinalIgnoreCase))).OrderBy(x => x.UniqMajescoItem);

        processItems = sortedProcessItems.ToList();

        foreach (ProcessItem processItem in processItems)
        {
            try
            {
                // Get transaction to check current balance
                Transaction transaction = await _sdkHelper.GetTransaction(Convert.ToInt32(processItem.ApplyToTransactionId));

                // Get existing policy
                Policy policy = await _sdkHelper.GetPolicyById(processItem.PolicyId);

                if (transaction.Balance != 0)
                {
                    // Create receipt for payment
                    Receipt receipt = ReceiptHelper.CreateReceipt(processItem, policy);

                    // Insert receipt
                    processItem.ReceiptId = await _sdkHelper.InsertReceipt(receipt);

                    // Get receipt
                    receipt = await GetApplyCreditsToDebits(processItem, policy);

                    // Add receivables
                    receipt = ReceiptHelper.UpdateReceipt(receipt, processItem);

                    // Update receipt
                    await _sdkHelper.UpdateReceipt(receipt);

                    // Finalize receipt
                    await _sdkHelper.FinalizeReceipt(processItem.ReceiptId);

                    // Set processed to partial
                    processItem.PartialProcess = true;
                }
                else // Process zero balance receipt
                {
                    // Create receipt for payment
                    Receipt receipt = ReceiptHelper.CreateApplyToPolicyReceipt(processItem, policy);

                    // Insert receipt
                    processItem.ReceiptId = await _sdkHelper.InsertReceipt(receipt);

                    // Finalize receipt
                    await _sdkHelper.FinalizeReceipt(processItem.ReceiptId);

                    // Set processed to partial
                    processItem.PartialProcess = true;
                }
                
            }
            catch (Exception ex)
            {
                processItem.Exception = ex.Message;

                if (processItem.ReceiptId != -1)
                {
                    try
                    {
                        // Delete partially created receipt
                        await _sdkHelper.DeleteReceipt(processItem.ReceiptId);
                        processItem.ReceiptId = -1;
                    }
                    catch (Exception dex)
                    {
                        processItem.Exception = $"{processItem.Exception}{Environment.NewLine}{dex.Message}";
                    }
                }
            }
        }

        foreach (ProcessItem processItem in processItems)
        {
            _bdeHelper.UpdateItem(processItem);
        }

        await CreateReceiptActivity(processItems);

        return true;
    }

    //public async Task<bool> CreateReceipts()
    //{
    //    // Sync data
    //    _bdeHelper.DataSync("Reprocess Payment");

    //    // Locate items for processing
    //    List<ProcessItem> processItems = _bdeHelper.GetProcessItems();

    //    IOrderedEnumerable<ProcessItem> sortedProcessItems = processItems.Where(processItem =>
    //        processItem is { TransactionType: not null, PolicyId: not -1, LineId: not -1, ReceiptId: -1 } &&
    //        (processItem.TransactionType.Equals("PAYMENT", StringComparison.OrdinalIgnoreCase) ||
    //         processItem.TransactionType.Equals("VIRTUAL_PAYMENT", StringComparison.OrdinalIgnoreCase) ||
    //         processItem.TransactionType.Equals("PAYMENT_TRANSFER_INTERNAL", StringComparison.OrdinalIgnoreCase) ||
    //         processItem.TransactionType.Equals("PAYMENT_TRASNFER_INTERNAL", StringComparison.OrdinalIgnoreCase) ||
    //         processItem.TransactionType.Equals("PAYMENT_TRANSFER_EXTERNAL", StringComparison.OrdinalIgnoreCase) ||
    //         processItem.TransactionType.Equals("PAYMENT_TRASNFER_EXTERNAL", StringComparison.OrdinalIgnoreCase))).OrderBy(x => x.UniqMajescoItem);

    //    processItems = sortedProcessItems.ToList();

    //    foreach (ProcessItem processItem in processItems)
    //    {
    //        try
    //        {
    //            // Get existing policy
    //            Policy policy = await _sdkHelper.GetPolicyById(processItem.PolicyId);

    //            // Create receipt for payment
    //            Receipt receipt = ReceiptHelper.CreateReceipt(processItem, policy);

    //            // Insert receipt
    //            processItem.ReceiptId = await _sdkHelper.InsertReceipt(receipt);

    //        }
    //        catch (Exception ex)
    //        {
    //            processItem.Exception = ex.Message;
    //        }
    //    }

    //    await AddReceivablesAndFinalizeReceipt(processItems);

    //    return true;
    //}

    //public async Task<bool> AddReceivablesAndFinalizeReceipt(List<ProcessItem> lsProcessItems)
    //{
    //    foreach (ProcessItem processItem in lsProcessItems.Where(processItem =>
    //                 processItem is { TransactionType: not null, PolicyId: not -1, LineId: not -1, ReceiptId: not -1 } &&
    //                 (processItem.TransactionType.Equals("PAYMENT", StringComparison.OrdinalIgnoreCase) ||
    //                  processItem.TransactionType.Equals("VIRTUAL_PAYMENT", StringComparison.OrdinalIgnoreCase) ||
    //                  processItem.TransactionType.Equals("PAYMENT_TRANSFER_INTERNAL", StringComparison.OrdinalIgnoreCase) ||
    //                  processItem.TransactionType.Equals("PAYMENT_TRASNFER_INTERNAL", StringComparison.OrdinalIgnoreCase) ||
    //                  processItem.TransactionType.Equals("PAYMENT_TRANSFER_EXTERNAL", StringComparison.OrdinalIgnoreCase) ||
    //                  processItem.TransactionType.Equals("PAYMENT_TRASNFER_EXTERNAL", StringComparison.OrdinalIgnoreCase))))
    //    {
    //        try
    //        {
    //            // Get existing policy
    //            //Policy policy = await _sdkHelper.GetPolicyByMajescoItem(item);
    //            Policy policy = await _sdkHelper.GetPolicyById(processItem.PolicyId);

    //            // Get receipt
    //            Receipt receipt = await GetApplyCreditsToDebits(processItem, policy);

    //            // Add receivables
    //            receipt = ReceiptHelper.UpdateReceipt(receipt, processItem);

    //            // Update receipt
    //            await _sdkHelper.UpdateReceipt(receipt);

    //            // Finalize receipt
    //            await _sdkHelper.FinalizeReceipt(processItem.ReceiptId);

    //            // Set processed to partial
    //            processItem.PartialProcess = true;
    //        }
    //        catch (Exception ex)
    //        {
    //            processItem.Exception = ex.Message;

    //            try
    //            {
    //                // Delete partially created receipt
    //                await _sdkHelper.DeleteReceipt(processItem.ReceiptId);
    //                processItem.ReceiptId = -1;
    //            }
    //            catch (Exception dex)
    //            {
    //                processItem.Exception = $"{processItem.Exception}{Environment.NewLine}{dex.Message}";
    //            }
    //        }
    //    }

    //    foreach (ProcessItem processItem in lsProcessItems)
    //    {
    //        _bdeHelper.UpdateItem(processItem);
    //    }


    //    await CreateReceiptActivity(lsProcessItems);

    //    return true;
    //}

    public async Task<Receipt> GetApplyCreditsToDebits(ProcessItem oProcessItem, Policy oPolicy)
    {
        DetailItem detailItem = new()
        {
            DetailItemAccountLookupCode = oProcessItem.EntityCode,
            ApplyTo = "Receivables",
            DebitCreditOption = new()
            {
                OptionName = oProcessItem.CreditDebit is "DB" ? "Debit" : "Credit",
                Value = oProcessItem.CreditDebit is "DB" ? 0 : 1
            },
            Amount = Convert.ToDecimal(oProcessItem.Amount),
            Description = oProcessItem.ProcessTypeDescription,
            StructureAgencyCode = oPolicy.AgencyCode,
            StructureBranchCode = oPolicy.BranchCode,
            Flag = ReceiptFlags.Insert,
            ApplyToSelectedItemsApplyCreditsToDebits = new()
            {
                Credits = new()
                {
                    new()
                    {
                        ARDueDate = Convert.ToDateTime(oProcessItem.EffectiveDate),
                        AccountingMonth = oProcessItem.AccountingYearMonth,
                        Balance = 0,
                        Description = oProcessItem.ProcessTypeDescription,
                        Pending = false,
                        PolicyNumber = oProcessItem.CarnetNumber,
                        TransactionAmount = Convert.ToDecimal(oProcessItem.Amount) * -1,
                        TransactionCode = oProcessItem.TransactionCode,
                        TransactionEffectiveDate = Convert.ToDateTime(oProcessItem.EffectiveDate),
                        TransactionID = -1
                    }
                }
            }
        };

        if (oProcessItem.ApplyToDeferredTransactionId != -1)
        {
            detailItem.ApplyToSelectedItemsApplyCreditsToDebits.Credits.Add(new()
            {
                ARDueDate = Convert.ToDateTime(oProcessItem.EffectiveDate),
                AccountingMonth = oProcessItem.AccountingYearMonth,
                Balance = 0,
                Description = oProcessItem.ProcessTypeDescription,
                Pending = false,
                PolicyNumber = oProcessItem.CarnetNumber,
                TransactionAmount = Convert.ToDecimal(oProcessItem.DeferredAmount),
                TransactionCode = "AFEE",
                TransactionEffectiveDate = Convert.ToDateTime(oProcessItem.EffectiveDate),
                TransactionID = Convert.ToInt32(oProcessItem.ApplyToDeferredTransactionId)
            });
        }

        return await _sdkHelper.GetApplyCreditsToDebits(oProcessItem.ReceiptId, detailItem);
    }

    public async Task<bool> CreateReceiptActivity(List<ProcessItem> lsProcessItems)
    {
        foreach (ProcessItem processItem in lsProcessItems.Where(processItem =>
                     processItem is { TransactionType: not null, PolicyId: not -1, LineId: not -1, ReceiptId: not -1, PartialProcess: true } &&
                     (processItem.TransactionType.Equals("RETURNED_PAYMENT", StringComparison.OrdinalIgnoreCase) ||
                      processItem.TransactionType.Equals("PAYMENT", StringComparison.OrdinalIgnoreCase) ||
                      processItem.TransactionType.Equals("VIRTUAL_PAYMENT", StringComparison.OrdinalIgnoreCase) ||
                      processItem.TransactionType.Equals("PAYMENT_ADJUSTMENT", StringComparison.OrdinalIgnoreCase) ||
                      processItem.TransactionType.Equals("PAYMENT_TRANSFER_INTERNAL", StringComparison.OrdinalIgnoreCase) ||
                      processItem.TransactionType.Equals("PAYMENT_TRASNFER_INTERNAL", StringComparison.OrdinalIgnoreCase) ||
                      processItem.TransactionType.Equals("PAYMENT_TRANSFER_EXTERNAL", StringComparison.OrdinalIgnoreCase) ||
                      processItem.TransactionType.Equals("PAYMENT_TRASNFER_EXTERNAL", StringComparison.OrdinalIgnoreCase))))
        {
            try
            {
                // Get existing policy
                Policy policy = await _sdkHelper.GetPolicyById(processItem.PolicyId);

                // Generate Activity
                Activity activity = ActivityHelper.GenerateReceiptActivity(policy, processItem);
                processItem.ActivityId = await _sdkHelper.InsertGeneralLedgerActivity(activity);
                processItem.Processed = 1;
                processItem.ProcessedDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                processItem.Exception = ex.Message;
            }
        }

        foreach (ProcessItem processItem in lsProcessItems)
        {
            _bdeHelper.UpdateItem(processItem);
        }

        return true;
    }

    #endregion

    #region Returned Payments

    public async Task<bool> ProcessReturnedPayments()
    {
        // Sync data
        _bdeHelper.DataSync("Payment");

        // Locate items for processing
        List<ProcessItem> processItems = _bdeHelper.GetProcessItems();

        foreach (ProcessItem processItem in processItems.Where(processItem =>
                     processItem is { TransactionType: not null, PolicyId: not -1, LineId: not -1, ReceiptId: -1 } &&
                     processItem.TransactionType.Equals("RETURNED_PAYMENT", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                // Get existing policy
                Policy policy = await _sdkHelper.GetPolicyById(processItem.PolicyId);

                // Create receipt for payment
                Receipt receipt = ReceiptHelper.CreateReturnedPayment(processItem, policy);

                // Insert receipt
                processItem.ReceiptId = await _sdkHelper.InsertReceipt(receipt);

                // Finalize receipt
                await _sdkHelper.FinalizeReceipt(processItem.ReceiptId);

                // Set processed to partial
                processItem.PartialProcess = true;
            }
            catch (Exception ex)
            {
                processItem.Exception = ex.Message;

                if (processItem.ReceiptId != -1)
                {
                    try
                    {
                        // Delete partially created receipt
                        await _sdkHelper.DeleteReceipt(processItem.ReceiptId);
                        processItem.ReceiptId = -1;
                    }
                    catch (Exception dex)
                    {
                        processItem.Exception = $"{processItem.Exception}{Environment.NewLine}{dex.Message}";
                    }
                }
            }
        }

        foreach (ProcessItem processItem in processItems)
        {
            _bdeHelper.UpdateItem(processItem);
        }

        await CreateReceiptActivity(processItems);

        return true;
    }

    //public async Task<bool> AddReceivablesAndFinalizeReturnedReceipt(List<ProcessItem> lsProcessItems)
    //{
    //    foreach (ProcessItem processItem in lsProcessItems.Where(processItem =>
    //                 processItem is { TransactionType: not null, PolicyId: not -1, LineId: not -1, ReceiptId: not -1 } &&
    //                 processItem.TransactionType.Equals("RETURNED_PAYMENT", StringComparison.OrdinalIgnoreCase)))
    //    {
    //        try
    //        {
    //            // Get existing policy
    //            Policy policy = await _sdkHelper.GetPolicyById(processItem.PolicyId);

    //            // Get receipt
    //            Receipt receipt = await GetApplyCreditsToDebits(processItem, policy);

    //            //// Add receivables
    //            //receipt = ReceiptHelper.UpdateReturnedReceipt(receipt, processItem);

    //            // Update receipt
    //            await _sdkHelper.UpdateReceipt(receipt);

    //            // Finalize receipt
    //            await _sdkHelper.FinalizeReceipt(processItem.ReceiptId);

    //            // Set processed to partial
    //            processItem.PartialProcess = true;
    //        }
    //        catch (Exception ex)
    //        {
    //            processItem.Exception = ex.Message;

    //            try
    //            {
    //                // Delete partially created receipt
    //                await _sdkHelper.DeleteReceipt(processItem.ReceiptId);
    //                processItem.ReceiptId = -1;
    //            }
    //            catch (Exception dex)
    //            {
    //                processItem.Exception = $"{processItem.Exception}{Environment.NewLine}{dex.Message}";
    //            }
    //        }
    //    }

    //    foreach (ProcessItem processItem in lsProcessItems)
    //    {
    //        _bdeHelper.UpdateItem(processItem);
    //    }


    //    await CreateReceiptActivity(lsProcessItems);

    //    return true;
    //}

    #endregion

    #region Payment Adjustments

    public async Task<bool> CreatePaymentAdjustments()
    {
        // Sync data
        _bdeHelper.DataSync("Payment");

        // Locate items for processing
        List<ProcessItem> processItems = _bdeHelper.GetProcessItems();

        foreach (ProcessItem processItem in processItems.Where(processItem => processItem.TransactionType != null && processItem.ReceiptId != -1 && processItem.PolicyId != -1 &&
                                                                              (processItem.TransactionType.Equals("PAYMENT_ADJUSTMENT", StringComparison.OrdinalIgnoreCase))))
        {
            try
            {
                // Create receipt for payment
                Receipt receipt = await _sdkHelper.GetReceipt(processItem.ReceiptId);
                receipt.DetailValue.DetailItemsValue[0].Amount = Convert.ToDecimal(processItem.Amount);

                // Update receipt
                await _sdkHelper.UpdateReceipt(receipt);

                // Set processed to partial
                processItem.PartialProcess = true;
            }
            catch (Exception ex)
            {
                processItem.Exception = ex.Message;
            }
        }

        foreach (ProcessItem item in processItems)
        {
            _bdeHelper.UpdateItem(item);
        }

        await CreateReceiptActivity(processItems);

        return true;
    }

    #endregion

    #region Refund

    public async Task<bool> CreateDisbursements()
    {
        // Sync data
        _bdeHelper.DataSync("Disbursement");

        // Locate items for processing
        List<ProcessItem> processItems = _bdeHelper.GetProcessItems();

        foreach (ProcessItem processItem in processItems.Where(processItem =>
                     processItem.TransactionType != null &&
                     processItem.PolicyId != -1 &&
                     processItem.LineId != -1 &&
                     processItem.TransactionId != -1 &&
                     processItem.TransactionType.Equals("REFUND", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                // Get existing policy
                Policy policy = await _sdkHelper.GetPolicyById(processItem.PolicyId);

                // Get existing transaction
                Transaction transaction = await _sdkHelper.GetTransaction(processItem.TransactionId);

                // Generate Disbursement
                Disbursement disbursement = DisbursementHelper.GenerateDisbursement(processItem, policy, transaction);

                // Insert Disbursement
                processItem.DisbursementId = await _sdkHelper.InsertDisbursement(disbursement);

                // Set processed to partial
                processItem.PartialProcess = true;
            }
            catch (Exception ex)
            {
                processItem.Exception = ex.Message;
            }
        }

        foreach (ProcessItem processItem in processItems)
        {
            _bdeHelper.UpdateItem(processItem);
        }

        await CreateDisbursementActivity(processItems);

        return true;
    }

    public async Task<bool> CreateDisbursementActivity(List<ProcessItem> lsProcessItems)
    {
        foreach (ProcessItem processItem in lsProcessItems.Where(processItem =>
                     processItem.TransactionType != null &&
                     processItem.PolicyId != -1 &&
                     processItem.LineId != -1 &&
                     processItem.PartialProcess &&
                     processItem.TransactionType.Equals("REFUND", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                // Get existing policy
                Policy policy = await _sdkHelper.GetPolicyById(processItem.PolicyId);

                // Generate Activity
                if (processItem.DisbursementId != -1)
                {
                    Activity activity = ActivityHelper.GenerateDisbursementActivity(policy, processItem);
                    processItem.ActivityId = await _sdkHelper.InsertGeneralLedgerActivity(activity);
                    processItem.Processed = 1;
                    processItem.ProcessedDate = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                processItem.Exception = ex.Message;
            }
        }

        foreach (ProcessItem processItem in lsProcessItems)
        {
            _bdeHelper.UpdateItem(processItem);
        }

        return true;
    }

    #endregion

    #region Void Refund

    public async Task<bool> CreateVoidDisbursements()
    {
        // Sync data
        _bdeHelper.DataSync("Disbursement");

        // Locate items for processing
        List<ProcessItem> processItems = _bdeHelper.GetProcessItems();

        foreach (ProcessItem processItem in processItems.Where(processItem =>
                     processItem.DisbursementId != -1 &&
                     processItem.TransactionType!.Equals("VOID_REFUND", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                // Generate Disbursement
                DisbursementVoid disbursementVoid = DisbursementHelper.GenerateVoidDisbursement(processItem);

                // Insert Disbursement
                await _sdkHelper.VoidDisbursement(disbursementVoid);

                // Set processed to partial
                processItem.PartialProcess = true;
            }
            catch (Exception ex)
            {
                processItem.Exception = ex.Message;
            }
        }

        foreach (ProcessItem processItem in processItems)
        {
            _bdeHelper.UpdateItem(processItem);
        }

        await CreateVoidDisbursementActivity(processItems);

        return true;
    }

    public async Task<bool> CreateVoidDisbursementActivity(List<ProcessItem> lsProcessItems)
    {
        foreach (ProcessItem processItem in lsProcessItems.Where(processItem =>
                     processItem.DisbursementId != -1 &&
                     processItem.PolicyId != -1 && processItem.PartialProcess &&
                     processItem.TransactionType!.Equals("VOID_REFUND", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                // Get existing policy
                Policy policy = await _sdkHelper.GetPolicyById(processItem.PolicyId);

                // Generate Activity
                if (processItem.DisbursementId != -1)
                {
                    Activity activity = ActivityHelper.GenerateDisbursementActivity(policy, processItem);
                    processItem.ActivityId = await _sdkHelper.InsertGeneralLedgerActivity(activity);
                    processItem.Processed = 1;
                    processItem.ProcessedDate = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                processItem.Exception = ex.Message;
            }
        }

        foreach (ProcessItem processItem in lsProcessItems)
        {
            _bdeHelper.UpdateItem(processItem);
        }

        return true;
    }

    #endregion

    #region Writeoff

    public async Task<bool> CreateWriteOffs()
    {
        // Sync data
        _bdeHelper.DataSync("Writeoff");

        // Locate items for processing
        List<ProcessItem> processItems = _bdeHelper.GetProcessItems();

        foreach (ProcessItem processItem in processItems.Where(processItem => processItem.TransactionType != null && processItem.PolicyId != -1 && processItem.LineId != -1 &&
                                                         processItem.TransactionType.Equals("WRITEOFF", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                // Get existing transaction
                Transaction transaction = await _sdkHelper.GetTransaction(processItem.TransactionId);

                // Get existing policy
                Policy policy = await _sdkHelper.GetPolicy(processItem.ClientId, processItem.PolicyId);

                // Get existing line
                Line line = await _sdkHelper.GetLine(processItem.LineId);

                // If partial writeoff - non-standard workflow create negative transaction for same Bill Number
                if (transaction.TransactionAmount != processItem.Amount)
                {
                    // Transaction code is specific to partial writeoff
                    processItem.TransactionCode = "WPAT";

                    // Generate transaction object
                    Transaction writeoffTransaction = TransactionHelper.GenerateTransaction(policy, line, processItem.ProcessTypeDescription, Convert.ToDecimal(processItem.Amount), processItem);

                    if (processItem.CreditDebit == "CR")
                    {
                        writeoffTransaction.TransactionAmount *= -1;
                        writeoffTransaction.CommissionsValue.Splits[0].AgencySplitAmount *= -1;
                    }
                    processItem.TransactionId = await _sdkHelper.InsertTransaction(writeoffTransaction);
                }
                else // Full writeoff - standard workflow
                {
                    // Create Write Off
                    AccountsReceivableWriteOff writeOff = TransactionHelper.GenerateWriteOff(processItem);

                    // Insert Write Off
                    await _sdkHelper.InsertWriteOff(writeOff);
                }

                // Generate Activity
                if (processItem.TransactionId != -1)
                {
                    Activity activity = ActivityHelper.GenerateActivity(policy, processItem.TransactionId, processItem);
                    processItem.ActivityId = await _sdkHelper.InsertActivity(activity);
                    processItem.Processed = 1;
                    processItem.ProcessedDate = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                processItem.Exception = ex.Message;
            }
        }

        foreach (ProcessItem item in processItems)
        {
            _bdeHelper.UpdateItem(item);
        }

        return true;
    }

    #endregion
}