using System.Diagnostics;
using IntegrationLogic;
using IntegrationLogic.Helpers;
using IntegrationLogic.Models;

namespace BillingTransformationApp;

class Program
{
    private static Config? _config;
    private static BillingTransformationHelper? _billingTransformationHelper;
    private static readonly Mutex Mutex = new(true, "F6133E37-F0FE-4C26-BBE9-77BF6C002C7B");

    [STAThread]
    private static void Main()
    {
        if (Mutex.WaitOne(TimeSpan.FromSeconds(30), true))
        {
            // Elapsed process time
            Stopwatch timer = new();
            timer.Start();

            // Added due to issues with application freezing during processing for Roanoke. With Quick Edit enabled, clicking into the console application could terminate the process.
            ConsoleWindow.QuickEditMode(false);

            // Read config file
            Console.WriteLine("Reading configuration file.");
            
            _config = ConfigFileHelper.ReadFile();
            if (string.IsNullOrEmpty(_config.AuthenticationKey) || string.IsNullOrEmpty(_config.FilePath) || string.IsNullOrEmpty(_config.Url) || string.IsNullOrEmpty(_config.DatabaseName))
            {
                EventLog.WriteEntry(".NET Runtime", "Process cannot be started. Config file is blank or incomplete.", EventLogEntryType.Error, 1000);
                FinalizeProcess();
                return;
            }

            // Create BillingTransformationHelper object
            Console.WriteLine("Preparing to process.");
            try
            {
                _billingTransformationHelper = new(_config);
                _billingTransformationHelper.GetTransactionCodes().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(".NET Runtime", $"An error occurred during process start:{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
                FinalizeProcess();
                return;
            }

            // Check for SQL tables. If tables do not exist, they will be created
            Console.WriteLine("Checking SQL BDE Database for Majesco tables.");
            try
            {
                _billingTransformationHelper.SqlTableCheck();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(".NET Runtime", $"An error occurred while accessing SQL Server:{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
                FinalizeProcess();
                return;
            }

            // Update any Majesco Items with errors for reprocessing
            Console.WriteLine("Updating Majesco Items with errors for reprocessing.");
            try
            {
                _billingTransformationHelper.ReprocessErrors();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(".NET Runtime", $"An error occurred while updating Majesco Items for reprocessing:{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
                FinalizeProcess();
                return;
            }

            // Create directory for archiving files
            string archivePath = $"{_config.FilePath}\\Archive\\{DateTime.Now:yyyyMMdd}";

            // Get list of files in the directory
            List<FileInfo> files = new DirectoryInfo(_config.FilePath)
                .GetFiles("*.txt")
                .OrderBy(f => f.FullName)
                .ToList();

            try
            {
                Directory.CreateDirectory(archivePath);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(".NET Runtime", $"Failed to create archive for processing files:{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
                FinalizeProcess();
                return;
            }

            if (files.Count > 0)
            {
                // Loop through and process each file
                foreach (FileInfo fileInfo in files)
                {
                    // Process and upload files to SQL
                    Console.WriteLine($"------------------------------------------------------------------------------");
                    Console.WriteLine($"Processing Majesco files in path: {fileInfo.FullName}");
                    try
                    {
                        // Insert data to SQL
                        _billingTransformationHelper.InsertFile(fileInfo.FullName, archivePath);
                    }
                    catch (Exception ex)
                    {
                        EventLog.WriteEntry(".NET Runtime", $"An error occurred while processing files:{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
                        FinalizeProcess();
                        return;
                    }
                    RunProcess();
                }
            }
            else
            {
                RunProcess();
            }

            // Dispose Billing Transformation Helper
            Console.WriteLine("End of process.");

            // Total elapsed time
            TimeSpan timeSpan = timer.Elapsed;
            Console.WriteLine($"------------------------------------------------------------------------------");
            Console.WriteLine($@"Process completed in {timeSpan:hh\:mm\:ss\:fff}");

            try
            {
                FinalizeProcess();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(".NET Runtime", $"An error occurred while disposing billing transformation helper.{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
            }
        }
        else
        {
            Console.WriteLine("Billing transformation process is already running. Only one instance of this utility can be active.");
            EventLog.WriteEntry(".NET Runtime", "Billing transformation process is already running. Only one instance of this utility can be active.", EventLogEntryType.Error, 1000);
        }
    }

    private static void FinalizeProcess()
    {
        Mutex.ReleaseMutex();
        Mutex.Dispose();
        _billingTransformationHelper?.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Added due to issues with application freezing during processing for Roanoke. With Quick Edit enabled, clicking into the console application could terminate the process.
        ConsoleWindow.QuickEditMode(true);
    }

    private static void RunProcess()
    {
        // Validate uploaded files
        Console.WriteLine("Validating files.");
        try
        {
            // Insert data to SQL
            _billingTransformationHelper.ValidateFiles();

        }
        catch (Exception ex)
        {
            EventLog.WriteEntry(".NET Runtime", $"An error occurred while validating files:{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
            FinalizeProcess();
            return;
        }

        // Create policies for New/Endorsement items
        Console.WriteLine("Processing new business items.");
        try
        {
            _billingTransformationHelper.CreateBondCargoPolicies().GetAwaiter().GetResult();
            _billingTransformationHelper.CreateCarnetPolicies().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            EventLog.WriteEntry(".NET Runtime", $"An error occurred while creating policies.{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
            FinalizeProcess();
            return;
        }

        // Create transactions for New/Endorsement items
        Console.WriteLine("Processing transactions for new business, endorsements, cancellations, and reinstatements.");
        try
        {
            _billingTransformationHelper.CreateNewEndorsementTransactionsActivities().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            EventLog.WriteEntry(".NET Runtime", $"An error occurred while creating transactions.{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
            FinalizeProcess();
            return;
        }

        // Create taxes and fess
        Console.WriteLine("Processing taxes and fees.");
        try
        {
            _billingTransformationHelper.CreateNewEndorsementGenerateTaxTaxActivities().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            EventLog.WriteEntry(".NET Runtime", $"An error occurred while creating taxes and fees.{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
            FinalizeProcess();
            return;
        }

        // Create receipts for payment items
        Console.WriteLine("Processing receipts for payment items.");
        try
        {
            _billingTransformationHelper.ProcessPayments().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            EventLog.WriteEntry(".NET Runtime", $"An error occurred while creating receipts.{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
            FinalizeProcess();
            return;
        }

        // Process returned payments.
        Console.WriteLine("Processing returned payments for items.");
        try
        {
            _billingTransformationHelper.ProcessReturnedPayments().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            EventLog.WriteEntry(".NET Runtime", $"An error occurred while processing returned payments.{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
            FinalizeProcess();
            return;
        }

        // Create receipts for payment items - reprocess
        Console.WriteLine("Reprocessing receipts for payment items.");
        try
        {
            _billingTransformationHelper.ProcessPayments().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            EventLog.WriteEntry(".NET Runtime", $"An error occurred while creating receipts.{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
            FinalizeProcess();
            return;
        }

        // Create disbursements
        Console.WriteLine("Processing disbursements.");
        try
        {
            _billingTransformationHelper.CreateDisbursements().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            EventLog.WriteEntry(".NET Runtime", $"An error occurred while creating disbursements.{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
            FinalizeProcess();
            return;
        }

        // Void disbursements
        Console.WriteLine("Processing void disbursements.");
        try
        {
            _billingTransformationHelper.CreateVoidDisbursements().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            EventLog.WriteEntry(".NET Runtime", $"An error occurred while creating void disbursements.{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
            FinalizeProcess();
            return;
        }

        // Create write offs
        Console.WriteLine("Processing write offs.");
        try
        {
            _billingTransformationHelper.CreateWriteOffs().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            EventLog.WriteEntry(".NET Runtime", $"An error occurred while creating write offs.{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
            FinalizeProcess();
            return;
        }

        // Create adjustments for Payment Adjustment items
        Console.WriteLine("Processing payment adjustments.");
        try
        {
            _billingTransformationHelper.CreatePaymentAdjustments().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            EventLog.WriteEntry(".NET Runtime", $"An error occurred while adjusting payments.{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
            FinalizeProcess();
            return;
        }

        // Update any Majesco Items with exceptions with additional details
        Console.WriteLine("Executing post processing for items with exceptions.");
        try
        {
            _billingTransformationHelper.PostProcessSync();
            _billingTransformationHelper.PostProcessErrors();
        }
        catch (Exception ex)
        {
            EventLog.WriteEntry(".NET Runtime", $"An error occurred while executing post processing for items with exceptions:{Environment.NewLine}{Environment.NewLine}{ex.Message}", EventLogEntryType.Error, 1000);
            FinalizeProcess();
        }
    }
}