using System;
using System.Windows.Input;
using IntegrationLogic.Models;

namespace ConfigurationApp.ViewModel.Commands;

public class FinishCommand : ICommand
{
    public ConfigurationViewModel ConfigurationViewModel { get; set; }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public FinishCommand(ConfigurationViewModel configurationViewModel)
    {
        ConfigurationViewModel = configurationViewModel;
    }

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        Config config = (parameter as Config)!;
        ConfigurationViewModel.SaveConfig(config);
        ConfigurationViewModel.CanEdit = true;
        ConfigurationViewModel.Close();
    }
}