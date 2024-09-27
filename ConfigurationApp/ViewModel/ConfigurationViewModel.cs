using System.ComponentModel;
using System.Windows;
using ConfigurationApp.ViewModel.Commands;
using IntegrationLogic.Helpers;
using IntegrationLogic.Models;

// ViewModel for the ConfigurationWindow
namespace ConfigurationApp.ViewModel;

public class ConfigurationViewModel : INotifyPropertyChanged
{
    // Property for the config file
    private Config? _config;

    public Config? Config
    {
        get => _config;
        set
        {
            _config = value;
            OnPropertyChanged(nameof(Config));
        } 
    }

    // Property for edit command boolean
    private bool _canEdit = true;

    public bool CanEdit
    {
        get => _canEdit;
        set
        {
            _canEdit = value;
            OnPropertyChanged(nameof(CanEdit));
        }
    }

    // Property for determining if status text should be shown
    // in the Status Bar
    private bool _showStatus = false;

    public bool ShowStatus
    {
        get => _showStatus;
        set
        {
            _showStatus = value;
            OnPropertyChanged(nameof(ShowStatus));
        }
    }

    // Property for Test Connection Status
    private string _testConnectionStatus = string.Empty;

    public string TestConnectionStatus
    {
        get => _testConnectionStatus;
        set
        {
            _testConnectionStatus = value;
            OnPropertyChanged(nameof(TestConnectionStatus));
        }
    }

    // Property for Majesco File Path
    private string _majescoFilePath = string.Empty;

    public string MajescoFilePath
    {
        get => _majescoFilePath;
        set
        {
            _majescoFilePath = value;
            OnPropertyChanged(nameof(MajescoFilePath));
        }
    }

    // Property for selected scheduled DateTime
    private System.DateTime _selectedDateTime;

    public System.DateTime SelectedDateTime
    {
        get => _selectedDateTime;
        set
        {
            _selectedDateTime = value;
            OnPropertyChanged(nameof(SelectedDateTime));
            Config!.ScheduleDateTime = SelectedDateTime;
        }
    }

    // Property for determining if current task exists
    private bool _taskExists = false;

    public bool TaskExists
    {
        get => _taskExists;
        set
        {
            _taskExists = value;
            OnPropertyChanged(nameof(TaskExists));
        }
    }

    // Property for Current Task Schedule
    private string _currentTaskSchedule = string.Empty;

    public string CurrentTaskSchedule
    {
        get => _currentTaskSchedule;
        set
        {
            _currentTaskSchedule = value;
            OnPropertyChanged(nameof(CurrentTaskSchedule));
        }
    }

    // Load config data
    public void LoadConfig()
    {
        Config = ConfigFileHelper.ReadFile();
        if (Config.FilePath != null) MajescoFilePath = Config.FilePath;
    }

    // Save config as file
    public static void SaveConfig(Config config)
    {
        ConfigFileHelper.SaveFile(config);
    }

    public EditCommand EditCommand { get; set; }

    public SaveCommand? SaveCommand { get; set; }

    public CancelCommand? CancelCommand { get; set; }

    public FinishCommand? FinishCommand { get; set; }

    public TestAsyncCommand? TestAsyncCommand { get; set; }

    public ScheduleCommand ScheduleCommand { get; set; }

    public FileDirectoryCommand FileDirectoryCommand { get; set; }

    // Constructor for the ViewModel
    public ConfigurationViewModel()
    {
        EditCommand = new EditCommand(this);
        SaveCommand = new SaveCommand(this);
        CancelCommand = new CancelCommand(this);
        FinishCommand = new FinishCommand(this);
        TestAsyncCommand = new TestAsyncCommand(this);
        ScheduleCommand = new ScheduleCommand(this);
        FileDirectoryCommand = new FileDirectoryCommand(this);
        Config = new Config();
    }

    public void Close()
    {
        // Close all application windows
        foreach (Window window in Application.Current.Windows)
        {
            window.Close();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}