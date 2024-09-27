using System;
using System.Windows.Input;

namespace ConfigurationApp.ViewModel.Commands;

// Cancel commands cancels configuration editing and reloads
// the configuration file into the ViewModel
public class CancelCommand : ICommand
{
    public ConfigurationViewModel ConfigurationViewModel { get; set; }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public CancelCommand(ConfigurationViewModel configurationViewModel)
    {
        ConfigurationViewModel = configurationViewModel;
    }

    public bool CanExecute(object? parameter)
    {
        return !ConfigurationViewModel.CanEdit;
    }

    public void Execute(object? parameter)
    {
        ConfigurationViewModel.CanEdit = true;
        ConfigurationViewModel.LoadConfig();
    }
}