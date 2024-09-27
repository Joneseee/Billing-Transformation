using System;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Input;
using IntegrationLogic.Models;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ConfigurationApp.ViewModel.Commands;

public class FileDirectoryCommand : ICommand
{
    public ConfigurationViewModel ConfigurationViewModel { get; set; }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public FileDirectoryCommand(ConfigurationViewModel configurationViewModel)
    {
        ConfigurationViewModel = configurationViewModel;
    }

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        try
        {
            if (ConfigurationViewModel.Config != null)
            {
                CommonOpenFileDialog dialog = new();
                dialog.InitialDirectory = ConfigurationViewModel.Config.FilePath;
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    ConfigurationViewModel.Config.FilePath = dialog.FileName;
                    ConfigurationViewModel.MajescoFilePath = dialog.FileName;
                }

                ConfigurationViewModel.SaveConfig(ConfigurationViewModel.Config);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred while setting file directory:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "File directory", MessageBoxButton.OK,
                MessageBoxImage.Error, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
        }
    }
}