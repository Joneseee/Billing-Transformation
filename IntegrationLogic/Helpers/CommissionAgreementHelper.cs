using schemas.appliedsystems.com.epic.sdk._2011._12._account;
using IntegrationLogic.Models;
using schemas.appliedsystems.com.epic.sdk._2011._01._account;
using schemas.appliedsystems.com.epic.sdk._2017._02._account._policy;

namespace IntegrationLogic.Helpers;

public class CommissionAgreementHelper
{
    public static BrokerCommission GetValidBrokerAgreement(Policy oPolicy, Line oLine, List<BrokerCommission> loBrokerCommissions)
    {
        try
        {
            Criteria criteria = GetPolicyLineCriteria(oPolicy, oLine);
            BrokerCommission brokerCommission = loBrokerCommissions.First(x => IsMatch_BrokerCommission(x, criteria));
            return brokerCommission;
        }
        catch
        {
            throw new Exception("A valid broker commission agreement could not be located.");
        }
    }

    public static CompanyCommission GetValidCompanyAgreement(Policy oPolicy, Line oLine, List<CompanyCommission> loCompanyCommissions)
    {
        try
        {
            Criteria criteria = GetPolicyLineCriteria(oPolicy, oLine);
            CompanyCommission companyCommission = loCompanyCommissions.First(x => IsMatch_CompanyCommission(x, criteria));
            return companyCommission;
        }
        catch
        {
            throw new Exception("A valid company commission agreement could not be located.");
        }
    }

    public static EmployeeCommission GetValidEmployeeAgreement(Policy oPolicy, Line oLine, List<EmployeeCommission> loEmployeeCommissions)
    {
        try
        {
            Criteria criteria = GetPolicyLineCriteria(oPolicy, oLine);
            EmployeeCommission employeeCommission = loEmployeeCommissions.First(x => IsMatch_EmployeeCommission(x, criteria));
            return employeeCommission;
        }
        catch
        {
            throw new Exception("A valid employee commission agreement could not be located.");
        }
    }

    private static Criteria GetPolicyLineCriteria(Policy oPolicy, Line oLine)
    {
        Criteria criteria = new()
        {
            AgencyCode = oPolicy.AgencyCode,
            BranchCode = oPolicy.BranchCode,
            DepartmentCode = oPolicy.DepartmentCode,
            ProfitCenterCode = oLine.ProfitCenterCode,
            IssuingLocation = oLine.IssuingLocationCode,
            LineType = oLine.LineTypeCode,
            Status = oLine.StatusCode,
            EffectiveDate = oPolicy.EffectiveDate,
            CompanyCode = oLine.PremiumPayableLookupCode,
            BillMode = oLine.BillingModeOption.OptionName
        };

        return criteria;
    }

    private static bool IsMatch_EmployeeCommission(EmployeeCommission oAgreement, Criteria oCriteria)
    {
        // Agency Criteria Match
        if (oAgreement.AgencyValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.AgencyValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.AgencyValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.AgencyCode, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Branch Criteria Match
        if (oAgreement.BranchValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.BranchValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.BranchValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.BranchCode, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Department Criteria Match
        if (oAgreement.DepartmentValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.DepartmentValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.DepartmentValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.DepartmentCode, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Profit Center Criteria Match
        if (oAgreement.ProfitCenterValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.ProfitCenterValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.ProfitCenterValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.ProfitCenterCode, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Company Criteria Match
        if (oAgreement.CompanyValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.CompanyValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.CompanyCode, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Issuing Location Criteria Match
        if (oAgreement.IssuingLocationValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.IssuingLocationValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.IssuingLocationValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.IssuingLocation, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Line of Business Criteria Match
        if (oAgreement.LineOfBusinessValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.LineOfBusinessValue.LineOfBusinessItemsValue.Count > 0)
        {
            if (!oAgreement.LineOfBusinessValue.LineOfBusinessItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.LineType, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Status Criteria Match
        if (oAgreement.StatusValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.StatusValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.StatusValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.Status, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Effective Criteria Match
        if (oAgreement.AgreementValue.EffectiveFromDate == null ||
            oAgreement.AgreementValue.EffectiveFromDate <= oCriteria.EffectiveDate)
        {
            if (oAgreement.AgreementValue.EffectiveToDate == null ||
                oAgreement.AgreementValue.EffectiveToDate >= oCriteria.EffectiveDate)
            {
                // All Criteria Match
                return true;
            }
            return false;
        }

        return false;
    }

    private static bool IsMatch_BrokerCommission(BrokerCommission oAgreement, Criteria oCriteria)
    {
        // Agency Criteria Match
        if (oAgreement.AgencyValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.AgencyValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.AgencyValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.AgencyCode, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Branch Criteria Match
        if (oAgreement.BranchValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.BranchValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.BranchValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.BranchCode, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Department Criteria Match
        if (oAgreement.DepartmentValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.DepartmentValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.DepartmentValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.DepartmentCode, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Profit Center Criteria Match
        if (oAgreement.ProfitCenterValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.ProfitCenterValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.ProfitCenterValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.ProfitCenterCode, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Company Criteria Match
        if (oAgreement.CompanyValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.CompanyValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.CompanyCode, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Issuing Location Criteria Match
        if (oAgreement.IssuingLocationValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.IssuingLocationValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.IssuingLocationValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.IssuingLocation, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Line of Business Criteria Match
        if (oAgreement.LineOfBusinessValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.LineOfBusinessValue.LineOfBusinessItemsValue.Count > 0)
        {
            if (!oAgreement.LineOfBusinessValue.LineOfBusinessItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.LineType, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Status Criteria Match
        if (oAgreement.StatusValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.StatusValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.StatusValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.Status, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Effective Criteria Match
        if (oAgreement.AgreementValue.EffectiveFromDate == null ||
            oAgreement.AgreementValue.EffectiveFromDate <= oCriteria.EffectiveDate)
        {
            if (oAgreement.AgreementValue.EffectiveToDate == null ||
                oAgreement.AgreementValue.EffectiveToDate >= oCriteria.EffectiveDate)
            {
                // All Criteria Match
                return true;
            }
            return false;
        }
            
        return false;
    }

    private static bool IsMatch_CompanyCommission(CompanyCommission oAgreement, Criteria oCriteria)
    {
        // Agency Criteria Match
        if (oAgreement.AgencyValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.AgencyValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.AgencyValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.AgencyCode, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Branch Criteria Match
        if (oAgreement.BranchValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.BranchValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.BranchValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.BranchCode, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Bill Mode Criteria Match
        if (oAgreement.BillModeAgencyDirectBothOption.Value == 0)
        {
            
        }
        else if (!oAgreement.BillModeAgencyDirectBothOption.OptionName.Equals(oCriteria.BillMode,
                     StringComparison.OrdinalIgnoreCase))
            return false;

        // Issuing Location Criteria Match
        if (oAgreement.IssuingLocationValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.IssuingLocationValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.IssuingLocationValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.IssuingLocation, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Line of Business Criteria Match
        if (oAgreement.LineOfBusinessValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.LineOfBusinessValue.LineOfBusinessItemsValue.Count > 0)
        {
            if (!oAgreement.LineOfBusinessValue.LineOfBusinessItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.LineType, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Status Criteria Match
        if (oAgreement.StatusValue.AllSelectedOption.Value == 0)
        {

        }
        else if (oAgreement.StatusValue.SelectedItemsValue.Count > 0)
        {
            if (!oAgreement.StatusValue.SelectedItemsValue.Any(oSelectedItem =>
                    oSelectedItem.Code.Equals(oCriteria.Status, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Effective Criteria Match
        if (oAgreement.AgreementValue.EffectiveFromDate == null ||
            oAgreement.AgreementValue.EffectiveFromDate <= oCriteria.EffectiveDate)
        {
            if (oAgreement.AgreementValue.EffectiveToDate == null ||
                oAgreement.AgreementValue.EffectiveToDate >= oCriteria.EffectiveDate)
            {
                // All Criteria Match
                return true;
            }
            return false;
        }

        return false;
    }
}