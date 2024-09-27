using IntegrationLogic.Models;

namespace IntegrationLogic.Helpers;

public class ArgumentsHelper
{
    // Helper for fetching Command Line Arguments values
    public static CommandLineArgs GetCommandLineArgs()
    {
        CommandLineArgs commandLineArgs = new();
        string[] sArgs = Environment.GetCommandLineArgs();

        foreach (string sArg in sArgs)
        {
            if (string.IsNullOrEmpty(sArg)) continue;
            if (sArg.Contains("PolicyID"))
            {
                commandLineArgs.PolicyId = Convert.ToInt32(sArg[(sArg.IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1)..]);
            }
            else if (sArg.Contains("LineID"))
            {
                commandLineArgs.LineId = Convert.ToInt32(sArg[(sArg.IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1)..]);
            }
            else if (sArg.Contains("ActivityID"))
            {
                commandLineArgs.ActivityId = Convert.ToInt32(sArg[(sArg.IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1)..]);
            }
            else if (sArg.Contains("AccountType"))
            {
                commandLineArgs.AccountType = sArg[(sArg.IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1)..];
            }
            else if (sArg.Contains("AttachmentID"))
            {
                commandLineArgs.AttachmentId = Convert.ToInt32(sArg[(sArg.IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1)..]);
            }
            else if (sArg.Contains("ClientID") || sArg.Contains("EntityID"))
            {
                commandLineArgs.ClientId = Convert.ToInt32(sArg[(sArg.IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1)..]);
            }
            else if (sArg.Contains("ClaimID"))
            {
                commandLineArgs.ClaimId = Convert.ToInt32(sArg[(sArg.IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1)..]);
            }
            else if (sArg.Contains("ContactID"))
            {
                commandLineArgs.ContactId = Convert.ToInt32(sArg[(sArg.IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1)..]);
            }
            else if (sArg.Contains("TransactionID"))
            {
                commandLineArgs.TransactionId = Convert.ToInt32(sArg[(sArg.IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1)..]);
            }
            else if (sArg.Contains("user"))
            {
                commandLineArgs.User = sArg[(sArg.IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1)..];
            }
            else if (sArg.Contains("area"))
            {
                commandLineArgs.User = sArg[(sArg.IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1)..];
            }
        }
        return commandLineArgs;
    }

    // Helper for displaying all arguments passed from Epic. Built for troubleshooting.
    public static string ShowPassedArguments()
    {
        CommandLineArgs commandLineArgs = new();
        string[] sArgs = Environment.GetCommandLineArgs();
        string message = sArgs.Aggregate(string.Empty, (current, sArg) => $"{current}{sArg}{Environment.NewLine}");
        return message;
    }
}