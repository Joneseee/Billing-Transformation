namespace IntegrationLogic.Models;

public class Criteria
{
    #region Properties

    public string? AgencyCode { get; set; }
    public string? BranchCode { get; set; }
    public string? DepartmentCode { get; set; }
    public string? ProfitCenterCode { get; set; }
    public string? IssuingLocation { get; set; }
    public string? LineType { get; set; }
    public string? Status { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string? CompanyCode { get; set; }
    public string? BillMode { get; set; }

    #endregion

    #region Constructor

    public Criteria()
    {

    }

    #endregion
}