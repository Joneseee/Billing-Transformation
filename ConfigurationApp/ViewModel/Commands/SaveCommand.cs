using System;
using System.Windows.Input;
using IntegrationLogic.Models;

namespace ConfigurationApp.ViewModel.Commands;

// Command for saving the config file
public class SaveCommand : ICommand
{
    public ConfigurationViewModel ConfigurationViewModel { get; set; }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public SaveCommand(ConfigurationViewModel configurationViewModel)
    {
        ConfigurationViewModel = configurationViewModel;
    }

    public bool CanExecute(object? parameter)
    {
        // If parameter is null return false
        if(parameter == null) return false;

        // If config file properties are not null or white space make save button available
        Config config = (parameter as Config)!;
        if (string.IsNullOrWhiteSpace(config.Url)) return false;
        if (string.IsNullOrWhiteSpace(config.DatabaseName)) return false;
        if (string.IsNullOrWhiteSpace(config.AuthenticationKey)) return false;
        if (string.IsNullOrWhiteSpace(config.SqlDatabase)) return false;
        if (string.IsNullOrWhiteSpace(config.SqlServer)) return false;
        if (string.IsNullOrWhiteSpace(config.FilePath)) return false;
        return !ConfigurationViewModel.CanEdit;
    }

    public void Execute(object? parameter)
    {
        Config config = (parameter as Config)!;
        ConfigurationViewModel.SaveConfig(config);
        ConfigurationViewModel.CanEdit = true;
    }
}