using System.Data.SqlClient;
using System.Diagnostics;
using IntegrationLogic;
using IntegrationLogic.Helpers;
using IntegrationLogic.Models;

namespace MajescoReIndexApp;

class Program
{
    private static Config? _config; 
    private static BdeHelper? _bdeHelper;
    private static readonly Mutex Mutex = new(true, "F6133E37-F0FE-4C28-BBE9-77BF6C002C9B");

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
                return;
            }

            // Reindex Database
            Console.WriteLine("Reindexing database.");
            _bdeHelper = new(_config.SqlServer!, _config.SqlDatabase!, _config.SqlUsername!, _config.SqlPassword!);

            // Open SQL Connection
            using SqlConnection sqlConnection = new(_bdeHelper.GetConnectionString());
            sqlConnection.Open();
            _bdeHelper.ExecuteQuery("DatabaseReindex.sql", sqlConnection);
            sqlConnection.Close();

            // Total elapsed time
            TimeSpan timeSpan = timer.Elapsed;
            Console.WriteLine($@"Process completed in {timeSpan:hh\:mm\:ss\:fff}");
        }
    }

};
