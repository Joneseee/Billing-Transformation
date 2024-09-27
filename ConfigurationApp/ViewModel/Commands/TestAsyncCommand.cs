using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AppliedIntegration.Commands;
using IntegrationLogic.Helpers;
using IntegrationLogic.Models;

namespace ConfigurationApp.ViewModel.Commands;

public class TestAsyncCommand : AsyncCommand
{
    public ConfigurationViewModel ConfigurationViewModel { get; set; }

    private bool _sdkSuccess = false;
    private bool _bdeSuccess = false;

    public TestAsyncCommand(ConfigurationViewModel configurationViewModel)
    {
        ConfigurationViewModel = configurationViewModel;
    }

    public override bool CanExecute()
    {
        // If parameter is null return false
        if (ConfigurationViewModel.Config == null) return false;
        Config config = ConfigurationViewModel.Config!;

        // If config file properties are not null or white space make save button available
        // and CanEdit is true.
        if (string.IsNullOrWhiteSpace(config.Url)) return false;
        if (string.IsNullOrWhiteSpace(config.DatabaseName)) return false;
        if (string.IsNullOrWhiteSpace(config.AuthenticationKey)) return false;
        if (string.IsNullOrWhiteSpace(config.SqlServer)) return false;
        if (string.IsNullOrWhiteSpace(config.SqlDatabase)) return false;
        
        return !RunningTasks.Any() && ConfigurationViewModel.CanEdit;
    }

    public override async Task ExecuteAsync()
    {
        ConfigurationViewModel.ShowStatus = true;
        await Task.Factory.StartNew(TestConnection);
    }

    public async void TestConnection()
    {
        
        try
        {
            Config config = ConfigurationViewModel.Config!;
            ConfigurationViewModel.TestConnectionStatus = "SDK test is running...";
            _sdkSuccess = await TestConnectionHelper.SdkConnection(config.Url!, config.DatabaseName!, config.AuthenticationKey!);
            ConfigurationViewModel.TestConnectionStatus = _sdkSuccess ? "Successful SDK connection" : "Failed to connect to SDK";
            string message = _sdkSuccess ? "SDK: Passed" : "SDK: Failed";

            ConfigurationViewModel.TestConnectionStatus = "BDE test is running...";
            _bdeSuccess = TestConnectionHelper.BdeConnection(config.SqlServer!, config.SqlDatabase!, config.SqlUsername!, config.SqlPassword!);
            ConfigurationViewModel.TestConnectionStatus = _bdeSuccess ? "Successful BDE connection" : "Failed to connect to BDE";
            message = _bdeSuccess ? $"{message}, BDE: Passed" : $"{message}, BDE: Failed";

            ConfigurationViewModel.TestConnectionStatus = message;
        }
        catch (Exception ex)
        {
            ConfigurationViewModel.TestConnectionStatus = "Connection test failed.";
            MessageBox.Show(
                $"Connection test failed:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "Test connection", MessageBoxButton.OK,
                MessageBoxImage.Error, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
        }

    }
}