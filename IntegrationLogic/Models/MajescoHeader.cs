using System.Globalization;

namespace IntegrationLogic.Models;

public class MajescoHeader
{
    #region Properties

    public int? UniqMajescoHeader = -1;
    public string? RecordType { get; set; }
    public int? FileLoadNumber { get; set; }
    public int? NumberOfRecords { get; set; }
    public int? CountOfPolicies { get; set; }
    public string? CreditDebit { get; set; }
    public decimal? TotalTransactionsAmount { get; set; }
    public DateTime? DateGenerated { get; set; }
    public string? FilePath { get; set; }

    #endregion

    #region Constructor

    public MajescoHeader(string sHeader)
    {
        RecordType = sHeader.Substring(0, 2).Trim();
        FileLoadNumber = Convert.ToInt32(sHeader.Substring(2, 10));
        NumberOfRecords = Convert.ToInt32(sHeader.Substring(12, 8));
        CountOfPolicies = Convert.ToInt32(sHeader.Substring(20, 8));
        CreditDebit = sHeader.Substring(28, 2).Trim();
        TotalTransactionsAmount = Convert.ToDecimal(sHeader.Substring(30, 22));
        DateGenerated = DateTime.ParseExact(sHeader.Substring(52, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
    }

    #endregion
}