using System;
using System.Windows.Input;

namespace ConfigurationApp.ViewModel.Commands;

// Command for editing config settings
public class EditCommand : ICommand
{
    public ConfigurationViewModel ConfigurationViewModel { get; set; }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public EditCommand(ConfigurationViewModel configurationViewModel)
    {
        ConfigurationViewModel = configurationViewModel;
    }

    public bool CanExecute(object? parameter)
    {
        return ConfigurationViewModel.CanEdit;
    }

    public void Execute(object? parameter)
    {
        ConfigurationViewModel.CanEdit = false;
    }
}