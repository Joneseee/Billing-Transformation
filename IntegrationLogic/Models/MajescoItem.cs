using System.Globalization;
using IntegrationLogic.Extensions;

namespace IntegrationLogic.Models;

public class MajescoItem
{
    #region Properties

    public int? UniqMajescoHeader = -1;
    public int? UniqMajescoItem = -1;
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
    public bool Success = false;
    public string Exception = string.Empty;
    public int ClientId = -1;
    public int MatchPolicyId = -1;
    public int MatchLineId = -1;
    public int TransactionId = -1;
    public int PolicyId = -1;
    public int LineId = -1;
    public int ActivityId = -1;
    public int ReceiptId = -1;
    public int DisbursementId = -1;
    public int ReferNumber = -1;
    public int BankAccount;
    public int GlAccount = 1331;
    public string? LineType { get; set; }
    public string? TransactionCode { get; set; }
    public string Description { get; set; }
    public string ActivityCode = string.Empty;
    public decimal? DeferredAmount { get; set; }
    public int? ReceivableItemSequence { get; set; }
    public string? UnderwritingCompanyCode { get; set; }
    public int? PaymentSequence { get; set; }
    public int? PaymentItemSequence { get; set; }
    public string? ProcessType { get; set; }
    public string? FormNo { get; set; }
    public string? CustomerEntityCode { get; set; }

    #endregion

    #region Constructor

    public MajescoItem(string sItem)
    {
        RecordType = sItem.Substring(0, 2).Trim();

        // Entity Code - Starts at position 13 and is 20 characters in length.
        // We only read the first 10 characters to extract the Entity Code
        EntityCode = sItem.Substring(12, 10).Trim();

        EffectiveDate = DateTime.ParseExact(sItem.Substring(32, 8), "yyyyMMdd", CultureInfo.InvariantCulture);

        ExpirationDate = DateTime.ParseExact(sItem.Substring(40, 8), "yyyyMMdd", CultureInfo.InvariantCulture);

        MajescoItemNumber = Convert.ToInt32(sItem.Substring(48, 25));

        SystemActivityNumber = sItem.Substring(73, 25).Trim();

        TransactionType = sItem.Substring(98, 38).Trim();

        // Receivable Code - Starts at position 139.
        ReceivableCode = sItem.Substring(138, 40).Trim();

        if (sItem.Substring(178, 22).Contains("-"))
        {
            Amount = Convert.ToDecimal(sItem.Substring(178, 22).Replace("-", "0")) * -1;
        }
        else
        {
            Amount = Convert.ToDecimal(sItem.Substring(178, 22));
        }

        CreditDebit = sItem.Substring(200, 2).Trim();

        CarnetHolder = sItem.Substring(202, 750).Trim();

        BankCode = sItem.Substring(952, 20).Trim();

        AccountingYearMonth = sItem.Substring(972, 6);

        try
        {
            DepositDate = DateTime.ParseExact(sItem.Substring(978, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
        }
        catch
        {
            // DepositDate = DateTime.Now;
            DepositDate = DateTime.ParseExact("19010101", "yyyyMMdd", CultureInfo.InvariantCulture);
        }

        // Location Id - Updated for enhancement. Only reads the first 10 characters instead of 20
        LocationId = sItem.Substring(994, 10).Trim();

        // Form No - Added for enhancement
        FormNo = sItem.Substring(1004, 10).Trim();

        try
        {
            DeferredAmount = Convert.ToDecimal(sItem.Substring(1014, 22));
        }
        catch
        {
            DeferredAmount = 0.00M;
        }

        // Underwriting Company Code - Added for enhancement
        UnderwritingCompanyCode = sItem.Substring(1036, 10).Trim();

        try
        {
            ReceivableItemSequence = Convert.ToInt32(sItem.Substring(1046, 25).Trim());
        }
        catch
        {
            ReceivableItemSequence = null;
        }

        // Carnet Number and Process Type (Carnet, Bonds, Cargo) - Added for enhancement
        try
        {
            // Carnet Number - Entity Code moved to Carnet field. Carnet moved to end of file. Remove leading zeroes
            CarnetNumber = sItem.Substring(1071, 50).TrimStart(new[] { '0' });
        }
        catch
        {
            CarnetNumber = string.Empty;
        }
        

        // Payment Sequence - Added for enhancement
        try
        {
            PaymentSequence = Convert.ToInt32(sItem.Substring(1121, 25));
        }
        catch
        {
            PaymentSequence = -1;
        }

        // Payment Item Sequence - Added for enhancement
        try
        {
            PaymentItemSequence = Convert.ToInt32(sItem.Substring(1146, 25));
        }
        catch
        {
            PaymentItemSequence = -1;
        }

        // Customer Entity Code
        try
        {
            CustomerEntityCode = sItem.Substring(1171, 20).Trim();
        }
        catch
        {
            CustomerEntityCode = null;
        }

        // Map Line Type
        if (!string.IsNullOrEmpty(ReceivableCode))
        {
            LineType = ReceivableCode switch
            {
                // Carnet
                "BDF" => "CARN",
                "BF" => "CARN",
                "BC" => "CARN",
                "CAL" => "CARN",
                "CC" => "CC",
                "SF" => "CARN",
                "CDC" => "CARN",
                "CD" => "CARN",
                "EF" => "CARN",
                "GLC" => "CARN",
                "LDI" => "CARN",
                "PF" => "CARN",
                "RFACT" => "CARN",
                "RFAPP" => "CARN",
                "SH" => "CARN",
                "STFEE" => "CARN",
                "TYP" => "CARN",
                "ISH" => "CARN",
                "PRF" => "CARN",
                "USCIB Replacement - SH" => "CARN",
                "STAX" => "CARN",
                "SFEE" => "CARN",
                "CTAX" => "CARN",
                "CBND" => "CBND",
                "CF" => "CCF",
                "CS" => "CCS",
                "CCF" => "CCF",
                "CCS" => "CCS",
                "CMAR" => "CMAR",
                "URSH" => "CARN",

                // Bonds
                "CTB" => "CTB",
                "CBM" => "CBM",
                "CFTZ" => "CFTZ",
                "CNCB" => "CNCB",
                "DRBK" => "DRBK",
                "IC" => "IC",
                "IIT" => "IIT",
                "STB" => "STB",
                "BMC" => "BMC",
                "CCTB" => "CCTB",
                "DOT" => "DOT",
                "FF" => "FF",
                "FTZ" => "FTZ",
                "NUSC" => "NUSC",
                "NVOC" => "NVOC",
                "SURT" => "SURT",

                // Marine Cargo
                "MAR" => "CRGO",
                "WAR" => "WAR",
                "MFEE" => "CRGO",
                "ICS" => "CRGO",
                "ISFEE" => "CRGO",
                "POL" => "CRGO",
                "SLFEE" => "CRGO",
                "SLTAX" => "CRGO",
                "STAMP" => "CRGO",
                _ => string.Empty
            };
        }

        // Map Transaction Code
        if (!string.IsNullOrEmpty(ReceivableCode) && !string.IsNullOrEmpty(TransactionType))
        {
            TransactionCode = ReceivableCode switch
            {
                // Carnet
                "BDF" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "BF" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "BC" => "AFEE",
                "CAL" => "AFEE",
                "SF" => "AFEE",
                "CDC" => "AFEE",
                "CD" => "AFEE",
                "EF" => "AFEE",
                "GLC" => "AFEE",
                "LDI" => "AFEE",
                "PF" => "AFEE",
                "RFACT" => "AFEE",
                "RFAPP" => "AFEE",
                "SH" => "AFEE",
                "STFEE" => "AFEE",
                "TYP" => "AFEE",
                "ISH" => "AFEE",
                "PRF" => "AFEE",
                "USCIB Replacement - SH" => "CFEE",
                "STAX" => "GOVT",
                "SFEE" => "GOVT",
                "CTAX" => "GOVT",
                "URSH" => "CFEE",
                "CC" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "CBND" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "CF" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "CS" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "CCF" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "CCS" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "CMAR" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },

                // Bonds
                "CTB" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "CBM" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "CFTZ" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "CNCB" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "DRBK" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "IC" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "IIT" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "STB" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "BMC" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "CCTB" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "DOT" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "FF" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "FTZ" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "NUSC" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "NVOC" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "BCS" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "BSFEE" => "AFEE",
                "CBFEE" => "AFEE",
                "IPFEE" => "AFEE",
                "SBFEE" => "AFEE",
                "SNUSC" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "SRFEE" => "AFEE",

                // Marine Cargo
                "MAR" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "WAR" => TransactionType switch
                {
                    "NEW" => "NEWB",
                    "RENEWAL" => "RENB",
                    _ => "ENDT"
                },
                "MFEE" => "AFEE",
                "ICS" => "AFEE",
                "ISFEE" => "AFEE",
                "POL" => "AFEE",
                "SLFEE" => "SFEE",
                "SLTAX" => "STAX",
                "STAMP" => "SFEE",
                _ => string.Empty
            };
        }

        // Map Process Code
        if (!string.IsNullOrEmpty(ReceivableCode))
        {
            ProcessType = ReceivableCode switch
            {
                // Carnet
                "BDF" => "Carnet",
                "BF" => "Carnet",
                "BC" => "Carnet",
                "CAL" => "Carnet",
                "SF" => "Carnet",
                "CDC" => "Carnet",
                "CD" => "Carnet",
                "EF" => "Carnet",
                "GLC" => "Carnet",
                "LDI" => "Carnet",
                "PF" => "Carnet",
                "RFACT" => "Carnet",
                "RFAPP" => "Carnet",
                "SH" => "Carnet",
                "STFEE" => "Carnet",
                "TYP" => "Carnet",
                "ISH" => "Carnet",
                "PRF" => "Carnet",
                "USCIB Replacement - SH" => "Carnet",
                "STAX" => "Carnet",
                "SFEE" => "Carnet",
                "CTAX" => "Carnet",
                "URSH" => "Carnet",
                "CC" => "Carnet",
                "CBND" => "Carnet",
                "CF" => "Carnet",
                "CS" => "Carnet",
                "CCF" => "Carnet",
                "CCS" => "Carnet",
                "CMAR" => "Carnet",

                // Bonds
                "CTB" => "Bond",
                
                "CBM" => "Bond",
                "CFTZ" => "Bond",
                "CNCB" => "Bond",
                "DRBK" => "Bond",
                "IC" => "Bond",
                "IIT" => "Bond",
                "STB" => "Bond",
                "BMC" => "Bond",
                "CCTB" => "Bond",
                "DOT" => "Bond",
                "FF" => "Bond",
                "FTZ" => "Bond",
                "NUSC" => "Bond",
                "NVOC" => "Bond",
                "SURT" => "Bond",

                // Marine Cargo
                "MAR" => "Cargo",
                "WAR" => "Cargo",
                "MFEE" => "Cargo",
                "ICS" => "Cargo",
                "ISFEE" => "Cargo",
                "POL" => "Cargo",
                "SLFEE" => "Cargo",
                "SLTAX" => "Cargo",
                "STAMP" => "Cargo",

                // Not Mapped
                _ => "Not Mapped"
            };
        }

        // Update Cancellation Transaction Code
        if (TransactionType.Equals("CANCELLATION"))
        {
            TransactionCode = ReceivableCode switch
            {
                "BC" => "AFEE",
                "CAL" => "AFEE",
                "SF" => "AFEE",
                "CDC" => "AFEE",
                "CD" => "AFEE",
                "EF" => "AFEE",
                "GLC" => "AFEE",
                "LDI" => "AFEE",
                "PF" => "AFEE",
                "RFACT" => "AFEE",
                "RFAPP" => "AFEE",
                "SH" => "AFEE",
                "STFEE" => "AFEE",
                "TYP" => "AFEE",
                "ISH" => "AFEE",
                "PRF" => "AFEE",
                "USCIB Replacement - SH" => "CFEE",
                "STAX" => "GOVT",
                "SFEE" => "GOVT",
                "CTAX" => "GOVT",
                "URSH" => "CFEE",
                _ => "CANC"
            };
        }

        // Map Activity Code
        if (!string.IsNullOrEmpty(TransactionType))
        {
            ActivityCode = TransactionType switch
            {
                "NEW" => "MBA2",
                "ENDORSEMENT" => "MBA2",
                "CANCELLATION" => "MBA2",
                "REINSTATEMENT" => "MBA2",
                "PAYMENT" => "MBA3",
                "REFUND" => "MBA3",
                "VOID_REFUND" => "VPMT",
                "RETURNED_PAYMENT" => "VPMT",
                "PAYMENT_TRANSFER_INTERNAL" => "MBA3",
                "PAYMENT_TRASNFER_INTERNAL" => "MBA3",
                "PAYMENT_TRANSFER_EXTERNAL" => "MBA3",
                "PAYMENT_TRASNFER_EXTERNAL" => "MBA3",
                "WRITEOFF" => "VPMT",
                "PAYMENT_ADJUSTMENT" => "MBA3",
                _ => string.Empty
            };
        }

        // Map Transaction Code
        if (!string.IsNullOrEmpty(TransactionType))
        {
            TransactionCode = TransactionType switch
            {
                "PAYMENT" => "PYMT",
                "REFUND" => "PYMT",
                "PAYMENT_TRANSFER_INTERNAL" => "PYMT",
                "PAYMENT_TRASNFER_INTERNAL" => "PYMT",
                "PAYMENT_TRANSFER_EXTERNAL" => "PYMT",
                "PAYMENT_TRASNFER_EXTERNAL" => "PYMT",
                _ => TransactionCode
            };
        }

        // Map Bank Account Number
        BankAccount = !string.IsNullOrEmpty(BankCode)
            ? BankCode switch
            {
                "CHASE" => 1233,
                "CHASE2" => 1232,
                "WF" => 1232,
                _ => 1232
            }
            : 1232;

        // Create transaction description
        Description = $"{MajescoItemNumber}_{CarnetNumber}_{CarnetHolder}";

        // Truncate if necessary
        if (Description.Length > 70)
        {
            Description = Description.Truncate(70);
        }
    }

    #endregion
}