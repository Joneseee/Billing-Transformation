using System.Collections;

namespace IntegrationLogic.Models;

public class CommandLineArgs : IEnumerable
{
    #region Properties

    public string? User { get; set; }
    public string? Area { get; set; }
    public string? AccountType { get; set; }
    public int? ActivityId { get; set; }
    public int? AttachmentId { get; set; }
    public int? ClaimId { get; set; }
    public int? ClientId { get; set; }
    public int? ContactId { get; set; }
    public int? PolicyId { get; set; }
    public int? LineId { get; set; }
    public int? TransactionId { get; set; }
    public IEnumerator GetEnumerator()
    {
        throw new System.NotImplementedException();
    }

    #endregion
}