using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using IntegrationLogic.Models;

namespace IntegrationLogic.Helpers;

public class BdeHelper : IDisposable
{
    #region Properties

    private readonly string _sqlServer;
    private readonly string _sqlDatabase;
    private readonly string _sqlUsername;
    private readonly string _sqlPassword;

    #endregion

    #region Constructor

    public BdeHelper(string sSqlServer, string sSqlDatabase, string sSqlUsername, string sSqlPassword)
    {
        _sqlServer = sSqlServer;
        _sqlDatabase = sSqlDatabase;
        _sqlUsername = sSqlUsername;
        _sqlPassword = sSqlPassword;

    }

    #endregion

    #region Methods

    public string GetSqlServer()
    {
        return _sqlDatabase ?? "";
    }

    public string GetEpicDatabase()
    {
        return _sqlDatabase ?? "";
    }

    public string GetConnectionString()
    {
        if (string.IsNullOrWhiteSpace(_sqlUsername))
            return "Data Source=" + _sqlServer + ";Initial Catalog=" + _sqlDatabase + ";Integrated Security=SSPI;MultipleActiveResultSets=true;";
        return "Data Source=" + _sqlServer + ";Initial Catalog=" + _sqlDatabase + ";User Id=" + _sqlUsername + ";Password=" + _sqlPassword + ";MultipleActiveResultSets=true;";
    }

    public bool IsServerConnected()
    {
        using SqlConnection connection = new SqlConnection(GetConnectionString());
        try
        {
            connection.Open();
            return true;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    // Method for reading .sql from Resources
    public string ReadResource(string name)
    {
        name = $"IntegrationLogic.Queries.{name}";

        // Determine path
        Assembly assembly = Assembly.GetExecutingAssembly();

        using Stream stream = assembly.GetManifestResourceStream(name)!;
        using StreamReader reader = new(stream!);

        return reader.ReadToEnd();
    }

    // Method for checking if Majesco SQL tables exist.
    // If tables do not exist, create them
    public bool SqlTableCheck()
    {
        string sqlQuery = ReadResource("MajescoCheck.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);
        try
        {
            sqlCommand.CommandTimeout = 18000;
            sqlConnection.Open();

            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            int count = 0;
            while (sqlDataReader.Read())
            {
                count = Convert.ToInt32(sqlDataReader["Number"]);
            }

            if (count == 0)
            {
                CreateTables();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error checking database for existing tables (MajescoCheck.sql): {ex.Message}");
        }

        return true;
    }

    // Method for checking if Majesco Mapping SQL tables exist.
    // If tables do not exist, create them
    public bool SqlMappingTableCheck()
    {
        string sqlQuery = ReadResource("MajescoMappingCheck.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);
        try
        {
            sqlCommand.CommandTimeout = 18000;
            sqlConnection.Open();

            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            int count = 0;
            while (sqlDataReader.Read())
            {
                count = Convert.ToInt32(sqlDataReader["Number"]);
            }

            if (count == 0)
            {
                CreateMappingTables();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error checking database for existing tables (MajescoMappingCheck.sql): {ex.Message}");
        }

        return true;
    }

    // Create indexes
    public bool CreateIndexes()
    {
        string sqlQuery = ReadResource("CreateIndexes.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);
        try
        {
            sqlCommand.CommandTimeout = 18000;
            sqlConnection.Open();
            sqlCommand.ExecuteReader();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating indexes: {ex.Message}");
        }

        return true;
    }

    // Create columns
    public bool CreateTableColumns()
    {
        string sqlQuery = ReadResource("CreateTableColumns.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);
        try
        {
            sqlCommand.CommandTimeout = 18000;
            sqlConnection.Open();
            sqlCommand.ExecuteReader();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating columns: {ex.Message}");
        }

        return true;
    }

    // Method for executing a query using a SQL Connection
    public bool ExecuteQuery(string sQueryName, SqlConnection oSqlConnection)
    {
        try
        {
            string sqlQuery = ReadResource(sQueryName);
            using SqlCommand sqlCommand = new(sqlQuery, oSqlConnection);
            sqlCommand.CommandTimeout = 18000;
            sqlCommand.ExecuteReader();
            sqlCommand.Dispose();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error executing query ({sQueryName}): {ex.Message}");
        }

        return true;
    }

    // Method for syncing SQL data
    public bool DataSync(string sProcessType = "")
    {
        // Open SQL Connection
        using SqlConnection sqlConnection = new(GetConnectionString());
        sqlConnection.Open();

        switch (sProcessType)
        {
            case "Policy":
                // Updates Majesco Header rows to processed where applicable
                ExecuteQuery("DataSync.MajescoHeaderSync.sql", sqlConnection);

                // Updates Transaction ID, Bill Number, and Item Number to -1 when NULL
                ExecuteQuery("DataSync.NullValueSync.sql", sqlConnection);

                // Updates Policy ID and Line ID for endorsements, cancellations, and reinstatements
                ExecuteQuery("DataSync.PolicySync.sql", sqlConnection);
                break;
            case "Payment":
                // Updates Majesco Header rows to processed where applicable
                ExecuteQuery("DataSync.MajescoHeaderSync.sql", sqlConnection);

                // Updates Item Number where transaction exists, but Item Number remains unpopulated
                ExecuteQuery("DataSync.ItemNumberSync.sql", sqlConnection);

                // Updates Item Number, Transaction ID, and Bill Number for created receipts
                ExecuteQuery("DataSync.ReceiptIdSync.sql", sqlConnection);

                // Updates payment receipt with reference number
                ExecuteQuery("DataSync.PaymentReferSync.sql", sqlConnection);

                // Updates Transaction ID, Item Number, Bill Number, Receipt ID, Policy ID, and Line ID
                // for payment adjustment
                ExecuteQuery("DataSync.PaymentAdjustmentSync.sql", sqlConnection);

                // Updates ApplyToTransactionId, ApplyToDeferredTransactionId, DeferredAmount, ApplyToTransactionCode,
                // ApplyToItemNumber, and ApplyToEffectiveDate for payments
                ExecuteQuery("DataSync.PaymentApplySync.sql", sqlConnection);

                // Updates Receipt ID, Transaction ID, Policy ID, Line ID, Item Number, Bill Number,
                // and Refer Number for returned payments
                ExecuteQuery("DataSync.ReturnedPaymentSync.sql", sqlConnection);

                // Updates Policy ID and Line ID for payments
                ExecuteQuery("DataSync.PaymentSync.sql", sqlConnection);
                break;
            case "Reprocess Payment":
                // Resets exceptions for payments
                ExecuteQuery("DataSync.ReprocessPaymentSync.sql", sqlConnection);

                // Updates Majesco Header rows to processed where applicable
                ExecuteQuery("DataSync.MajescoHeaderSync.sql", sqlConnection);

                // Updates Item Number where transaction exists, but Item Number remains unpopulated
                ExecuteQuery("DataSync.ItemNumberSync.sql", sqlConnection);

                // Updates Item Number, Transaction ID, and Bill Number for created receipts
                ExecuteQuery("DataSync.ReceiptIdSync.sql", sqlConnection);

                // Updates payment receipt with reference number
                ExecuteQuery("DataSync.PaymentReferSync.sql", sqlConnection);

                // Updates Transaction ID, Item Number, Bill Number, Receipt ID, Policy ID, and Line ID
                // for payment adjustment
                ExecuteQuery("DataSync.PaymentAdjustmentSync.sql", sqlConnection);

                // Updates ApplyToTransactionId, ApplyToDeferredTransactionId, DeferredAmount, ApplyToTransactionCode,
                // ApplyToItemNumber, and ApplyToEffectiveDate for payments
                ExecuteQuery("DataSync.PaymentApplySync.sql", sqlConnection);

                // Updates Receipt ID, Transaction ID, Policy ID, Line ID, Item Number, Bill Number,
                // and Refer Number for returned payments
                ExecuteQuery("DataSync.ReturnedPaymentSync.sql", sqlConnection);

                // Updates Policy ID and Line ID for payments
                ExecuteQuery("DataSync.PaymentSync.sql", sqlConnection);
                break;
            case "Disbursement":
                // Updates Majesco Header rows to processed where applicable
                ExecuteQuery("DataSync.MajescoHeaderSync.sql", sqlConnection);

                // Updates Transaction ID, Item Number, and Bill Number for Refunds
                ExecuteQuery("DataSync.RefundSync.sql", sqlConnection);

                // Updates disbursement with corresponding reference number
                ExecuteQuery("DataSync.DisbursementReferSync.sql", sqlConnection);

                // Updates Transaction ID, Item Number, Bill Number, Disbursement ID, Policy ID, and Line ID
                // for void refund
                ExecuteQuery("DataSync.VoidRefundSync.sql", sqlConnection);
                break;
            case "Writeoff":
                // Updates Majesco Header rows to processed where applicable
                ExecuteQuery("DataSync.MajescoHeaderSync.sql", sqlConnection);

                // Updates Transaction ID, Policy ID, Line ID, and Bill Number for 
                // writeoffs with a matching Majesco Item Number
                ExecuteQuery("DataSync.WriteOffSync.sql", sqlConnection);
                break;
            case "TransactionSync":
                // Re-syncs Endorsement, Cancellations, and Reinstatements to match any created lines -Bond/Cargo ONLY
                ExecuteQuery("DataSync.PolicyReSync.sql", sqlConnection);
                break;
            default:
                // Updates Majesco Header rows to processed where applicable
                ExecuteQuery("DataSync.MajescoHeaderSync.sql", sqlConnection);

                // Updates Transaction ID, Bill Number, and Item Number to -1 when NULL
                ExecuteQuery("DataSync.NullValueSync.sql", sqlConnection);

                // Updates Item Number where transaction exists, but Item Number remains unpopulated
                ExecuteQuery("DataSync.ItemNumberSync.sql", sqlConnection);

                // Updates Item Number, Transaction ID, and Bill Number for created receipts
                ExecuteQuery("DataSync.ReceiptIdSync.sql", sqlConnection);

                // Updates Transaction ID, Policy ID, Line ID, and Bill Number for 
                // writeoffs with a matching Majesco Item Number
                ExecuteQuery("DataSync.WriteOffSync.sql", sqlConnection);

                // Updates Transaction ID, Item Number, and Bill Number for Refunds
                ExecuteQuery("DataSync.RefundSync.sql", sqlConnection);

                // Updates payment receipt with reference number
                ExecuteQuery("DataSync.PaymentReferSync.sql", sqlConnection);

                // Updates disbursement with corresponding reference number
                ExecuteQuery("DataSync.DisbursementReferSync.sql", sqlConnection);

                // Updates Transaction ID, Item Number, Bill Number, Disbursement ID, Policy ID, and Line ID
                // for void refund
                ExecuteQuery("DataSync.VoidRefundSync.sql", sqlConnection);

                // Updates Transaction ID, Item Number, Bill Number, Receipt ID, Policy ID, and Line ID
                // for payment adjustment
                ExecuteQuery("DataSync.PaymentAdjustmentSync.sql", sqlConnection);

                // Updates ApplyToTransactionId, ApplyToDeferredTransactionId, DeferredAmount, ApplyToTransactionCode,
                // ApplyToItemNumber, and ApplyToEffectiveDate for payments
                ExecuteQuery("DataSync.PaymentApplySync.sql", sqlConnection);

                // Updates Receipt ID, Transaction ID, Policy ID, Line ID, Item Number, Bill Number,
                // and Refer Number for returned payments
                ExecuteQuery("DataSync.ReturnedPaymentSync.sql", sqlConnection);

                // Updates Policy ID and Line ID for endorsements, cancellations, and reinstatements
                ExecuteQuery("DataSync.PolicySync.sql", sqlConnection);

                // Updates Policy ID and Line ID for payments
                ExecuteQuery("DataSync.PaymentSync.sql", sqlConnection);
                break;
        }

        sqlConnection.Close();

        return true;
    }

    // Method for updating Majesco Items with errors for reprocessing
    public bool ReprocessErrors()
    {
        string sqlQuery = ReadResource("ReprocessErrors.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);
        try
        {
            sqlCommand.CommandTimeout = 18000;
            sqlConnection.Open();
            sqlCommand.ExecuteReader();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating errors for reprocessing (ReprocessErrors.sql): {ex.Message}");
        }

        return true;
    }

    // Method for updating Majesco Items with post process error messaging
    public bool PostProcessErrors()
    {
        string sqlQuery = ReadResource("PostProcessing.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);
        try
        {
            sqlCommand.CommandTimeout = 18000;
            sqlConnection.Open();
            sqlCommand.ExecuteReader();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating exceptions with post process exceptions (PostProcessing.sql): {ex.Message}");
        }

        return true;
    }

    // Method for getting items to process
    public List<ProcessItem> GetProcessItems(bool bZeroReceipt = false)
    {
        List<ProcessItem> processItems = new();
        string sqlQuery = ReadResource(bZeroReceipt ? "ZeroBalanceReceiptItems.sql" : "ProcessItems.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);
        try
        {
            sqlCommand.CommandTimeout = 18000;
            sqlConnection.Open();

            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            while (sqlDataReader.Read())
            {
                processItems.Add(new ProcessItem(sqlDataReader));
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting items for processing (ProcessItems.sql): {ex.Message}");
        }

        return processItems;
    }

    // Method for syncing taxes and fees
    public bool GenerateTaxFeeSync(string sTaxFeeTransactionCodes)
    {
        string sqlQuery = ReadResource("DataSync.GenerateTaxFeeSync.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);

        try
        {
            // Transaction code string parameter
            sqlCommand.Parameters.Add("@@transactionCodeString", SqlDbType.VarChar);
            sqlCommand.Parameters["@@transactionCodeString"].Value = sTaxFeeTransactionCodes;

            sqlConnection.Open();
            sqlCommand.ExecuteScalar();

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error syncing majesco items (DataSync.GenerateTaxFeeSync.sql): {ex.Message}");
        }
    }

    // Method for checking if payment adjustment should be processed as a payment
    public bool PaymentAdjustmentCheck(ProcessItem oProcessItem)
    {
        DataSet dataSet = new();
        string sqlQuery = ReadResource("PaymentAdjustmentCheck.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);

        try
        {
            // MajescoItemNumber parameter
            sqlCommand.Parameters.Add("@@majescoItemNumber", SqlDbType.Int);
            sqlCommand.Parameters["@@majescoItemNumber"].Value = oProcessItem.MajescoItemNumber;

            sqlConnection.Open();

            SqlDataAdapter sqlDataAdapter = new(sqlCommand);

            sqlDataAdapter.Fill(dataSet);
            
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                return Convert.ToInt32(row["Result"].ToString()) switch
                {
                    0 => false,
                    _ => true
                };
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error performing payment adjustment check: {ex.Message}");
        }
        return false;
    }

    // Method for syncing taxes and fees
    public TaxFeeInfo GetTaxFeeInfo(int iApplyToTransactionId)
    {
        DataSet dataSet = new();
        string sqlQuery = ReadResource("GetTaxFeeInfo.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);

        try
        {
            // ApplyToTransactionId parameter
            sqlCommand.Parameters.Add("@@applyToTransactionId", SqlDbType.Int);
            sqlCommand.Parameters["@@applyToTransactionId"].Value = iApplyToTransactionId;

            sqlConnection.Open();

            SqlDataAdapter sqlDataAdapter = new(sqlCommand);

            sqlDataAdapter.Fill(dataSet);

            if (dataSet.Tables[0].Rows.Count == 0)
                throw new Exception("Tax fee info could not be located for the ApplyToTransactionId provided.");

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                int uniqVendor = string.IsNullOrWhiteSpace(row["UniqVendor"].ToString())
                    ? -1
                    : Convert.ToInt32(row["UniqVendor"]);

                int uniqContactName = string.IsNullOrWhiteSpace(row["UniqContactName"].ToString())
                    ? -1
                    : Convert.ToInt32(row["UniqContactName"]);

                string cdStateCode = string.IsNullOrWhiteSpace(row["CdStateCode"].ToString())
                    ? string.Empty
                    : row["CdStateCode"].ToString()!;

                decimal taxableAmount = string.IsNullOrWhiteSpace(row["TaxableAmount"].ToString())
                    ? 0
                    : Convert.ToDecimal(row["TaxableAmount"].ToString()!);

                TaxFeeInfo taxFeeInfo = new(uniqVendor, uniqContactName, cdStateCode, taxableAmount);

                return taxFeeInfo;
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting tax fee info: {ex.Message}");
        }

        return new(-1,-1, string.Empty, 0);
    }

    // Method for updating items
    public void UpdateItem(ProcessItem oItem)
    {
        string sqlQuery = ReadResource("UpdateItem.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);
        try
        {
            // Unique Majesco Item Parameter
            sqlCommand.Parameters.Add("@@uniqMajescoItem", SqlDbType.Int);
            sqlCommand.Parameters["@@uniqMajescoItem"].Value = oItem.UniqMajescoItem;

            // UniqPolicy Parameter
            sqlCommand.Parameters.Add("@@uniqPolicy", SqlDbType.Int);
            sqlCommand.Parameters["@@uniqPolicy"].Value = oItem.PolicyId;

            // UniqLine Parameter
            sqlCommand.Parameters.Add("@@uniqLine", SqlDbType.Int);
            sqlCommand.Parameters["@@uniqLine"].Value = oItem.LineId;

            // UniqPolicyMatch Parameter
            sqlCommand.Parameters.Add("@@uniqPolicyMatch", SqlDbType.Int);
            sqlCommand.Parameters["@@uniqPolicyMatch"].Value = oItem.MatchPolicyId;

            // UniqLineMatch Parameter
            sqlCommand.Parameters.Add("@@uniqLineMatch", SqlDbType.Int);
            sqlCommand.Parameters["@@uniqLineMatch"].Value = oItem.MatchLineId;

            // UniqTransHead Parameter
            sqlCommand.Parameters.Add("@@uniqTransHead", SqlDbType.Int);
            sqlCommand.Parameters["@@uniqTransHead"].Value = oItem.TransactionId;

            // UniqTransHeadDeferred Parameter
            sqlCommand.Parameters.Add("@@uniqTransHeadDeferred", SqlDbType.Int);
            sqlCommand.Parameters["@@uniqTransHeadDeferred"].Value = oItem.DeferredTransactionId;

            // UniqReceipt Parameter
            sqlCommand.Parameters.Add("@@uniqReceipt ", SqlDbType.Int);
            sqlCommand.Parameters["@@uniqReceipt "].Value = oItem.ReceiptId;

            // UniqActivity Parameter
            sqlCommand.Parameters.Add("@@uniqActivity", SqlDbType.Int);
            sqlCommand.Parameters["@@uniqActivity"].Value = oItem.ActivityId;

            // UniqDisbursement Parameter
            sqlCommand.Parameters.Add("@@uniqDisbursement", SqlDbType.Int);
            sqlCommand.Parameters["@@uniqDisbursement"].Value = oItem.DisbursementId;

            // Item Number Parameter
            sqlCommand.Parameters.Add("@@itemNumber", SqlDbType.Int);
            sqlCommand.Parameters["@@itemNumber"].Value = oItem.ItemNumber;

            // Bill Number Parameter
            sqlCommand.Parameters.Add("@@billNumber", SqlDbType.Int);
            sqlCommand.Parameters["@@billNumber"].Value = oItem.BillNumber;

            // Refer Number Parameter
            sqlCommand.Parameters.Add("@@referNumber", SqlDbType.Int);
            sqlCommand.Parameters["@@referNumber"].Value = oItem.ReferNumber;

            // Invoice Number Parameter
            sqlCommand.Parameters.Add("@@invoiceNumber", SqlDbType.Int);
            sqlCommand.Parameters["@@invoiceNumber"].Value = oItem.InvoiceNumber;

            // Processed Parameter
            sqlCommand.Parameters.Add("@@processed", SqlDbType.Int);
            sqlCommand.Parameters["@@processed"].Value = oItem.Processed;

            // Exception Parameter
            sqlCommand.Parameters.Add("@@exception", SqlDbType.VarChar);
            sqlCommand.Parameters["@@exception"].Value = oItem.Exception;

            // Transaction Code Parameter
            sqlCommand.Parameters.Add("@@transactionCode", SqlDbType.VarChar);
            sqlCommand.Parameters["@@transactionCode"].Value = oItem.TransactionCode;

            // Description Parameter
            sqlCommand.Parameters.Add("@@description", SqlDbType.VarChar);
            sqlCommand.Parameters["@@description"].Value = oItem.Description;

            // Processed Date Parameter
            sqlCommand.Parameters.Add("@@processedDate", SqlDbType.DateTime);
            sqlCommand.Parameters["@@processedDate"].Value = oItem.ProcessedDate != null
                ? oItem.ProcessedDate
                : DBNull.Value;

            sqlConnection.Open();
            sqlCommand.ExecuteScalar();

        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating majesco items (UpdateItem.sql): {ex.Message}");
        }
    }

    // Method for updating items
    public void HeaderException(int iUniqMajescoHeader, string sException)
    {
        string sqlQuery = ReadResource("HeaderException.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);
        try
        {
            // Unique Majesco Header Parameter
            sqlCommand.Parameters.Add("@@uniqMajescoHeader", SqlDbType.Int);
            sqlCommand.Parameters["@@uniqMajescoHeader"].Value = iUniqMajescoHeader;

            // Exception Parameter
            sqlCommand.Parameters.Add("@@headerException", SqlDbType.VarChar);
            sqlCommand.Parameters["@@headerException"].Value = sException;

            sqlConnection.Open();
            sqlCommand.ExecuteScalar();

        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating majesco header (HeaderException.sql): {ex.Message}");
        }
    }

    // Create Majesco Header and Item tables
    public void CreateTables()
    {
        string sqlQuery = ReadResource("CreateTables.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);
        try
        {
            sqlCommand.CommandTimeout = 18000;
            sqlConnection.Open();
            sqlCommand.ExecuteReader();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating tables (CreateTables.sql): {ex.Message}");
        }
    }

    // Create Majesco Mapping tables
    public void CreateMappingTables()
    {
        string sqlQuery = ReadResource("CreateMappingTables.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);
        try
        {
            sqlCommand.CommandTimeout = 18000;
            sqlConnection.Open();
            sqlCommand.ExecuteReader();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating tables (CreateMappingTables.sql): {ex.Message}");
        }
    }

    // Validate files for processing
    public bool ValidationSync()
    {
        // Open SQL Connection
        using SqlConnection sqlConnection = new(GetConnectionString());
        sqlConnection.Open();

        // Update Line Type with Receivable Code/Form No combination
        // Update Transaction Code with Receivable Code/Form No combination
        ExecuteQuery("Validation.UpdateReceivableFormCombo.sql", sqlConnection);

        // Validates the majesco header number of records, count of policies, and transaction amount
        // If validation passes majesco header is updated to validated
        ExecuteQuery("Validation.MajescoHeaderValidation.sql", sqlConnection);

        // Locates the existing client in Epic
        ExecuteQuery("Validation.LocateExistingClient.sql", sqlConnection);

        // Locates the existing policy / line in Epic
        ExecuteQuery("Validation.GetPolicyLineMatch.sql", sqlConnection);

        // Updates line type for applicable receivable codes to mirror matched policy
        ExecuteQuery("Validation.UpdateLineTypes.sql", sqlConnection);

        // Update Company Mapping for new items
        ExecuteQuery("Validation.UpdateCompanyMapping.sql", sqlConnection);

        // Update UniqCompany with mapping
        ExecuteQuery("Validation.UpdateItemCompanyMapping.sql", sqlConnection);

        sqlConnection.Close();

        return true;
    }

    // Method for inserting header into SQL database
    public int InsertHeader(MajescoHeader oHeader)
    {
        string sqlQuery = ReadResource("HeaderInsert.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);
        try
        {
            // Record Type Parameter
            sqlCommand.Parameters.Add("@@recordType", SqlDbType.VarChar);
            sqlCommand.Parameters["@@recordType"].Value = oHeader.RecordType;

            // File Load Number Parameter
            sqlCommand.Parameters.Add("@@fileLoadNumber", SqlDbType.Int);
            sqlCommand.Parameters["@@fileLoadNumber"].Value = oHeader.FileLoadNumber;

            // Number of Records Parameter
            sqlCommand.Parameters.Add("@@numberOfRecords", SqlDbType.Int);
            sqlCommand.Parameters["@@numberOfRecords"].Value = oHeader.NumberOfRecords;

            // Count of Policies Parameter
            sqlCommand.Parameters.Add("@@countOfPolicies", SqlDbType.Int);
            sqlCommand.Parameters["@@countOfPolicies"].Value = oHeader.CountOfPolicies;

            // Credit Debit Parameter
            sqlCommand.Parameters.Add("@@creditDebit", SqlDbType.VarChar);
            sqlCommand.Parameters["@@creditDebit"].Value = oHeader.CreditDebit;

            // Total Transactions Amount Parameter
            sqlCommand.Parameters.Add("@@totalTransactionsAmount", SqlDbType.Decimal);
            sqlCommand.Parameters["@@totalTransactionsAmount"].Value = oHeader.TotalTransactionsAmount;

            // Date Generated Parameter
            sqlCommand.Parameters.Add("@@dateGenerated", SqlDbType.DateTime);
            sqlCommand.Parameters["@@dateGenerated"].Value = oHeader.DateGenerated;

            // File Path Parameter
            sqlCommand.Parameters.Add("@@filePath", SqlDbType.VarChar);
            sqlCommand.Parameters["@@filePath"].Value = oHeader.FilePath;

            sqlConnection.Open();

            return Convert.ToInt32(sqlCommand.ExecuteScalar());

        }
        catch (Exception ex)
        {
            throw new Exception($"Error inserting majesco header into SQL (HeaderInsert.sql): {ex.Message}");
        }
    }

    // Method for inserting items into SQL database
    public int InsertItem(MajescoItem oItem)
    {
        string sqlQuery = ReadResource("ItemInsert.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);
        try
        {
            // Unique Majesco Header Parameter
            sqlCommand.Parameters.Add("@@uniqMajescoHeader", SqlDbType.Int);
            sqlCommand.Parameters["@@uniqMajescoHeader"].Value = oItem.UniqMajescoHeader;

            // Record Type Parameter
            sqlCommand.Parameters.Add("@@recordType", SqlDbType.VarChar);
            sqlCommand.Parameters["@@recordType"].Value = oItem.RecordType;

            // Entity Code Parameter
            sqlCommand.Parameters.Add("@@entityCode", SqlDbType.VarChar);
            sqlCommand.Parameters["@@entityCode"].Value = oItem.EntityCode;

            // Carnet Number Parameter
            sqlCommand.Parameters.Add("@@carnetNumber", SqlDbType.VarChar);
            sqlCommand.Parameters["@@carnetNumber"].Value = oItem.CarnetNumber;

            // Effective Date Parameter
            sqlCommand.Parameters.Add("@@effectiveDate", SqlDbType.DateTime);
            sqlCommand.Parameters["@@effectiveDate"].Value = oItem.EffectiveDate;

            // Expiration Date Parameter
            sqlCommand.Parameters.Add("@@expirationDate", SqlDbType.DateTime);
            sqlCommand.Parameters["@@expirationDate"].Value = oItem.ExpirationDate;

            // Majesco Item Number Parameter
            sqlCommand.Parameters.Add("@@majescoItemNumber", SqlDbType.Int);
            sqlCommand.Parameters["@@majescoItemNumber"].Value = oItem.MajescoItemNumber;

            // System Activity Number Parameter
            sqlCommand.Parameters.Add("@@systemActivityNumber", SqlDbType.VarChar);
            sqlCommand.Parameters["@@systemActivityNumber"].Value = oItem.SystemActivityNumber;

            // Transaction Type Parameter
            sqlCommand.Parameters.Add("@@transactionType", SqlDbType.VarChar);
            sqlCommand.Parameters["@@transactionType"].Value = oItem.TransactionType;

            // Receivable Code Parameter
            sqlCommand.Parameters.Add("@@receivableCode", SqlDbType.VarChar);
            sqlCommand.Parameters["@@receivableCode"].Value = oItem.ReceivableCode;

            // Amount Parameter
            sqlCommand.Parameters.Add("@@amount", SqlDbType.Decimal);
            sqlCommand.Parameters["@@amount"].Value = oItem.Amount;

            // Deferred Amount Parameter
            sqlCommand.Parameters.Add("@@deferredAmount", SqlDbType.Decimal);
            sqlCommand.Parameters["@@deferredAmount"].Value = oItem.DeferredAmount;

            // Credit Debit Parameter
            sqlCommand.Parameters.Add("@@creditDebit", SqlDbType.VarChar);
            sqlCommand.Parameters["@@creditDebit"].Value = oItem.CreditDebit;

            // Carnet Holder Parameter
            sqlCommand.Parameters.Add("@@carnetHolder", SqlDbType.VarChar);
            sqlCommand.Parameters["@@carnetHolder"].Value = oItem.CarnetHolder;

            // Bank Code Parameter
            sqlCommand.Parameters.Add("@@bankCode", SqlDbType.VarChar);
            sqlCommand.Parameters["@@bankCode"].Value = oItem.BankCode;

            // Bank Account Number Parameter
            sqlCommand.Parameters.Add("@@bankAccount", SqlDbType.Int);
            sqlCommand.Parameters["@@bankAccount"].Value = oItem.BankAccount;

            // GL Account Number Parameter
            sqlCommand.Parameters.Add("@@glAccount", SqlDbType.Int);
            sqlCommand.Parameters["@@glAccount"].Value = oItem.GlAccount;

            // Reference Number Parameter
            sqlCommand.Parameters.Add("@@referNumber", SqlDbType.Int);
            sqlCommand.Parameters["@@referNumber"].Value = oItem.ReferNumber;

            // Accounting Year Month Parameter
            sqlCommand.Parameters.Add("@@accountingYearMonth", SqlDbType.VarChar);
            sqlCommand.Parameters["@@accountingYearMonth"].Value = oItem.AccountingYearMonth;

            // Deposit Date Parameter
            sqlCommand.Parameters.Add("@@depositDate", SqlDbType.DateTime);
            sqlCommand.Parameters["@@depositDate"].Value = oItem.DepositDate;

            // Location ID Parameter
            sqlCommand.Parameters.Add("@@locationId", SqlDbType.VarChar);
            sqlCommand.Parameters["@@locationId"].Value = oItem.LocationId;

            // Line Type Parameter
            sqlCommand.Parameters.Add("@@lineType", SqlDbType.VarChar);
            sqlCommand.Parameters["@@lineType"].Value = oItem.LineType;

            // Transaction Code Parameter
            sqlCommand.Parameters.Add("@@transactionCode", SqlDbType.VarChar);
            sqlCommand.Parameters["@@transactionCode"].Value = oItem.TransactionCode;

            // Description Parameter
            sqlCommand.Parameters.Add("@@description", SqlDbType.VarChar);
            sqlCommand.Parameters["@@description"].Value = oItem.Description;

            // Activity Code Parameter
            sqlCommand.Parameters.Add("@@activityCode", SqlDbType.VarChar);
            sqlCommand.Parameters["@@activityCode"].Value = oItem.ActivityCode;

            // Receivable Item Seq Number Parameter
            sqlCommand.Parameters.Add("@@receivableItemSequence", SqlDbType.Int);

            sqlCommand.Parameters["@@receivableItemSequence"].Value = oItem.ReceivableItemSequence != null
                ? oItem.ReceivableItemSequence
                : DBNull.Value;

            // Underwriting Company Code Parameter
            sqlCommand.Parameters.Add("@@underwritingCompanyCode", SqlDbType.VarChar);
            sqlCommand.Parameters["@@underwritingCompanyCode"].Value = oItem.UnderwritingCompanyCode;

            // Process Type Parameter
            sqlCommand.Parameters.Add("@@processType", SqlDbType.VarChar);
            sqlCommand.Parameters["@@processType"].Value = oItem.ProcessType;

            // Form No Parameter
            sqlCommand.Parameters.Add("@@formNo", SqlDbType.VarChar);
            sqlCommand.Parameters["@@formNo"].Value = oItem.FormNo;

            // Payment Seq Number Parameter
            sqlCommand.Parameters.Add("@@paymentSequence", SqlDbType.Int);

            sqlCommand.Parameters["@@paymentSequence"].Value = oItem.PaymentSequence != null
                ? oItem.PaymentSequence
                : DBNull.Value;

            // Payment Item Seq Number Parameter
            sqlCommand.Parameters.Add("@@paymentItemSequence", SqlDbType.Int);

            sqlCommand.Parameters["@@paymentItemSequence"].Value = oItem.PaymentItemSequence != null
                ? oItem.PaymentItemSequence
                : DBNull.Value;

            // Customer Entity Code Parameter
            sqlCommand.Parameters.Add("@@customerEntityCode", SqlDbType.VarChar);
            sqlCommand.Parameters["@@customerEntityCode"].Value = oItem.CustomerEntityCode != null
                ? oItem.CustomerEntityCode
                : DBNull.Value;

            sqlConnection.Open();

            return Convert.ToInt32(sqlCommand.ExecuteScalar());

        }
        catch (Exception ex)
        {
            throw new Exception($"{ex.Message}");
        }
    }

    // Method for deleting items from SQL database when an error occurs during insert
    public int DeleteItems(int iHeaderId)
    {
        string sqlQuery = ReadResource("DeleteItemByHeader.sql");

        using SqlConnection sqlConnection = new(GetConnectionString());
        using SqlCommand sqlCommand = new(sqlQuery, sqlConnection);
        try
        {
            // Unique Majesco Header Parameter
            sqlCommand.Parameters.Add("@@uniqMajescoHeader", SqlDbType.Int);
            sqlCommand.Parameters["@@uniqMajescoHeader"].Value = iHeaderId;

            sqlConnection.Open();

            return Convert.ToInt32(sqlCommand.ExecuteScalar());

        }
        catch (Exception ex)
        {
            throw new Exception($"{ex.Message}");
        }
    }

    public void Dispose()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    #endregion
}