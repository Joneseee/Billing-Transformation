using System.Windows;
using ConfigurationApp.View;

namespace ConfigurationApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private void App_Startup(object sender, StartupEventArgs e)
    {
        // Create new ConfigurationWindow
        ConfigurationWindow configurationWindow = new()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        // Show the ConfigurationWindow
        configurationWindow.Show();
    }
}