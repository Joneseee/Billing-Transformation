namespace IntegrationLogic.Models;

public class LineType
{
    #region Properties

    public string Code { get; set; }
    public int LineId = -1;
    public int ItemId { get; set; }
    public DateTime? EffectiveDate { get; set; }

    #endregion

    #region Constructor

    public LineType(string sCode, int iItemId, DateTime? dtEffectiveDate)
    {
        Code = sCode;
        ItemId = iItemId;
        EffectiveDate = dtEffectiveDate;
    }

    #endregion
}