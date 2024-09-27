namespace IntegrationLogic.Models;

public class TaxFeeInfo
{
    public int UniqVendor { get; set; }
    public int UniqContactName { get; set; }
    public string CdStateCode { get; set; }
    public decimal TaxableAmount { get; set; }

    public TaxFeeInfo(int iUniqVendor, int iUniqContactName, string sCdStateCode, decimal dTaxableAmount)
    {
        UniqVendor = iUniqVendor;
        UniqContactName = iUniqContactName;
        CdStateCode = sCdStateCode;
        TaxableAmount = dTaxableAmount;
    }
}