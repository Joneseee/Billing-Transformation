namespace IntegrationLogic.Models;

public class Config
{
    #region Properties

    public string? Url { get; set; }
    public string? DatabaseName { get; set; }
    public string? AuthenticationKey { get; set; }
    public string? Usercode { get; set; }
    public string? FilePath { get; set; }
    public string? LogPath { get; set; }
    public string? SqlServer { get; set; }
    public string? SqlDatabase { get; set; }
    public string? SqlUsername { get; set; }
    public string? SqlPassword { get; set; }
    public DateTime? ScheduleDateTime { get; set; }

    #endregion
}