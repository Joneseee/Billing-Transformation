#nullable enable

using System;
using System.Windows;
using System.Windows.Input;
using IntegrationLogic.Helpers;
using IntegrationLogic.Models;

namespace ConfigurationApp.ViewModel.Commands;

// Command for saving the config file
public class ScheduleCommand : ICommand
{
    public ConfigurationViewModel ConfigurationViewModel { get; set; }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public ScheduleCommand(ConfigurationViewModel configurationViewModel)
    {
        ConfigurationViewModel = configurationViewModel;
    }

    public bool CanExecute(object? parameter)
    {
        // If parameter is null return false
        if(parameter == null) return false;

        // If config file properties are not null or white space make save button available
        Config config = (parameter as Config)!;
        return config.ScheduleDateTime != null;
    }

    public void Execute(object? parameter)
    {
        try
        {
            if (ConfigurationViewModel.TaskExists)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(
                    "Overwrite current scheduled task?",
                    "Task Scheduler", MessageBoxButton.YesNo);

                if (messageBoxResult == MessageBoxResult.No)
                {
                    return;
                }

                TaskScheduleHelper.DeleteScheduledTask();

            }
            TaskScheduleHelper.ScheduleReportTask(ConfigurationViewModel.SelectedDateTime);

            ConfigurationViewModel.TaskExists = true;
            ConfigurationViewModel.CurrentTaskSchedule = TaskScheduleHelper.GetScheduledTask();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while scheduling tasks:{Environment.NewLine}{ex.Message}");
        }
    }
}