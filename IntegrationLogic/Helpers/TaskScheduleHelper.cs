using Microsoft.Win32.TaskScheduler;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace IntegrationLogic.Helpers;

public class TaskScheduleHelper
{
    public static void ScheduleReportTask(DateTime dtStartDateTime)
    {
        TaskDefinition taskDefinition = TaskService.Instance.NewTask();
        taskDefinition.RegistrationInfo.Description = "Billing Transformation Process";

        TaskFolder taskFolder = TaskService.Instance.GetFolder("Applied Systems");
        if (taskFolder == null)
        {
            TaskService.Instance.RootFolder.CreateFolder("Applied Systems");
        }

        DailyTrigger dailyTrigger = new()
        {
            Enabled = true,
            EndBoundary = dtStartDateTime.AddYears(50),
            ExecutionTimeLimit = TimeSpan.FromMinutes(120),
            StartBoundary = dtStartDateTime,
            RandomDelay = default
        };

        taskDefinition.Triggers.Add(dailyTrigger);
        taskDefinition.Settings.DeleteExpiredTaskAfter = TimeSpan.FromHours(2);
        taskDefinition.Settings.ExecutionTimeLimit = TimeSpan.FromMinutes(120);

        // Added conditions for successful run
        taskDefinition.Settings.WakeToRun = false;
        taskDefinition.Settings.DisallowStartOnRemoteAppSession = false;
        taskDefinition.Settings.DisallowStartIfOnBatteries = false;
        taskDefinition.Settings.Enabled = true;
        taskDefinition.Settings.StopIfGoingOnBatteries = true;


        ExecAction execAction = new($"{AppDomain.CurrentDomain.BaseDirectory}BillingTransformation.exe", string.Empty,
            AppDomain.CurrentDomain.BaseDirectory);
        taskDefinition.Actions.Add(execAction);

        string taskName = $"Applied Systems\\Billing Transformation";

        try
        {
            TaskService.Instance.RootFolder.RegisterTaskDefinition(taskName, taskDefinition, TaskCreation.CreateOrUpdate,
                Environment.UserName);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to register task:{Environment.NewLine}{ex.Message}");
        }
    }

    public static string GetScheduledTask()
    {
        using TaskService taskService = new();
        Task task = taskService.GetTask("Applied Systems\\Billing Transformation");
        if (task != null)
        {
            DateTime startDateTime = task.Definition.Triggers[0].StartBoundary;
            DateTime endDateTime = task.Definition.Triggers[0].EndBoundary;
            return $"Task is currently scheduled to run daily at {startDateTime.ToShortTimeString()} and will end on {endDateTime.ToLongDateString()} at {endDateTime.ToShortTimeString()}";
        }

        return string.Empty;
    }

    public static void DeleteScheduledTask()
    {
        using TaskService taskService = new();
        Task task = taskService.GetTask("Applied Systems\\Billing Transformation");
        if (task != null)
        {
            taskService.RootFolder.DeleteTask("Applied Systems\\Billing Transformation");
        }
    }

}