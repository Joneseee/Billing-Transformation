using IntegrationLogic.Models;
using schemas.appliedsystems.com.epic.sdk._2011._01._account;
using schemas.appliedsystems.com.epic.sdk._2011._01._common;
using schemas.appliedsystems.com.epic.sdk._2011._01._common._activity._common1;
using schemas.appliedsystems.com.epic.sdk._2011._01._common._activity._common1._noteitem;

namespace IntegrationLogic.Helpers;

public class ActivityHelper
{
    public static Activity GenerateActivity(Policy oPolicy, int iAssociatedToId, ProcessItem oProcessItem)
    {
        NoteItems notes = GetNoteItems(oProcessItem);

        return new Activity
        {
            AccountID = oPolicy.AccountID,
            AccountTypeCode = "CUST",
            ActivityCode = oProcessItem.ActivityCode,
            AgencyCode = oPolicy.AgencyCode,
            AssociatedToID = iAssociatedToId,
            AssociatedToType = "TRANSACTION",
            BranchCode = oPolicy.BranchCode,
            CloseDetailValue = new()
            {
                ClosedReason = string.Empty,
                ClosedStatus = "Successful",
                IgnoreOpenTasks = true
            },
            Description = oProcessItem.ProcessTypeDescription,
            DetailValue = new()
            {
                ExtensionData = null,
                ContactName = null,
                ContactNumberEmail = null,
                ContactVia = null,
                FollowUpEndDate = null,
                FollowUpEndTime = null,
                FollowUpStartDate = DateTime.Now,
                FollowUpStartTime = null,
                IssuingCompanyLookupCode = null,
                Notes = notes,
                PremiumPayableLookupCode = null,
                PremiumPayableTypeCode = null,
                ReminderDate = null,
                ReminderTime = null,
                ContactPhoneCountryCode = null,
                Amount = null,
                AmountQualifier = null,
                Update = null,
                ContactID = null
            },
            Priority = null,
            StatusOption = null,
            Tasks = null,
            WhoOwnerCode = null,
            CategoryID = 0,
            GeneralLedgerItemIDForAssociatedToID = 0,
            EnteredDate = null,
            AttachmentsYesNo = null,
            LastUpdatedDate = null,
            ServicingContacts = null,
            WhoOwnerOption = null,
            AssociatedToGUID = null,
            LineID = null,
            SMS = null,
            IndioSubmissionValue = null
        };
    }

    public static Activity GenerateDisbursementActivity(Policy oPolicy, ProcessItem oProcessItem)
    {
        NoteItems notes = GetNoteItems(oProcessItem);

        return new Activity
        {
            AccountID = oPolicy.AccountID,
            AccountTypeCode = "CUST",
            ActivityCode = oProcessItem.ActivityCode,
            AgencyCode = oPolicy.AgencyCode,
            AssociatedToID = oProcessItem.DisbursementId,
            AssociatedToType = "Disbursement",
            BranchCode = oPolicy.BranchCode,
            CloseDetailValue = new()
            {
                ClosedReason = string.Empty,
                ClosedStatus = "Successful",
                IgnoreOpenTasks = true
            },
            Description = oProcessItem.ProcessTypeDescription,
            DetailValue = new()
            {
                ExtensionData = null,
                ContactName = null,
                ContactNumberEmail = null,
                ContactVia = null,
                FollowUpEndDate = null,
                FollowUpEndTime = null,
                FollowUpStartDate = DateTime.Now,
                FollowUpStartTime = null,
                IssuingCompanyLookupCode = null,
                Notes = notes,
                PremiumPayableLookupCode = null,
                PremiumPayableTypeCode = null,
                ReminderDate = null,
                ReminderTime = null,
                ContactPhoneCountryCode = null,
                Amount = null,
                AmountQualifier = null,
                Update = null,
                ContactID = null
            },
            Priority = null,
            StatusOption = null,
            Tasks = null,
            WhoOwnerCode = null,
            CategoryID = 0,
            GeneralLedgerItemIDForAssociatedToID = 0,
            EnteredDate = null,
            AttachmentsYesNo = null,
            LastUpdatedDate = null,
            ServicingContacts = null,
            WhoOwnerOption = null,
            AssociatedToGUID = null,
            LineID = null,
            SMS = null,
            IndioSubmissionValue = null
        };
    }

    public static Activity GenerateReceiptActivity(Policy oPolicy, ProcessItem oProcessItem)
    {
        NoteItems notes = GetNoteItems(oProcessItem);

        return new Activity
        {
            AccountID = oPolicy.AccountID,
            AccountTypeCode = "CUST",
            ActivityCode = string.IsNullOrWhiteSpace(oProcessItem.ActivityCode) ? "MBA3" : oProcessItem.ActivityCode,
            AgencyCode = oPolicy.AgencyCode,
            AssociatedToID = oProcessItem.ReceiptId,
            AssociatedToType = "Receipt",
            BranchCode = oPolicy.BranchCode,
            CloseDetailValue = new()
            {
                ClosedReason = string.Empty,
                ClosedStatus = "Successful",
                IgnoreOpenTasks = true
            },
            Description = oProcessItem.ProcessTypeDescription,
            DetailValue = new()
            {
                ExtensionData = null,
                ContactName = null,
                ContactNumberEmail = null,
                ContactVia = null,
                FollowUpEndDate = null,
                FollowUpEndTime = null,
                FollowUpStartDate = DateTime.Now,
                FollowUpStartTime = null,
                IssuingCompanyLookupCode = null,
                Notes = notes,
                PremiumPayableLookupCode = null,
                PremiumPayableTypeCode = null,
                ReminderDate = null,
                ReminderTime = null,
                ContactPhoneCountryCode = null,
                Amount = null,
                AmountQualifier = null,
                Update = null,
                ContactID = null
            },
            Priority = null,
            StatusOption = null,
            Tasks = null,
            WhoOwnerCode = null,
            CategoryID = 0,
            GeneralLedgerItemIDForAssociatedToID = 0,
            EnteredDate = null,
            AttachmentsYesNo = null,
            LastUpdatedDate = null,
            ServicingContacts = null,
            WhoOwnerOption = null,
            AssociatedToGUID = null,
            LineID = null,
            SMS = null,
            IndioSubmissionValue = null
        };
    }

    private static NoteItems GetNoteItems(ProcessItem oProcessItem)
    {
        return new NoteItems
        {
            // Note 1
            new NoteItem
            {
                Flag = Flags.Insert,
                NoteText = $"Majesco Item #: {oProcessItem.MajescoItemNumber}"
            },
            // Note 2
            new NoteItem
            {
                Flag = Flags.Insert,
                NoteText = $"Carnet #: {oProcessItem.CarnetNumber}"
            },
            // Note 3
            new NoteItem
            {
                Flag = Flags.Insert,
                NoteText = $"Effective Date: {oProcessItem.EffectiveDate}"
            },
            // Note 4
            new NoteItem
            {
                Flag = Flags.Insert,
                NoteText = $"Expiration Date: {oProcessItem.ExpirationDate}"
            },
            // Note 5
            new NoteItem
            {
                Flag = Flags.Insert,
                NoteText = $"Carnet Holder: {oProcessItem.CarnetHolder}"
            },
            // Note 6
            new NoteItem
            {
                Flag = Flags.Insert,
                NoteText = $"Bank Name: {oProcessItem.BankCode}"
            },
            // Note 7
            new NoteItem
            {
                Flag = Flags.Insert,
                NoteText = $"Deposit Date: {oProcessItem.DepositDate}"
            },
            // Note 8
            new NoteItem
            {
                Flag = Flags.Insert,
                NoteText = $"Record Type: {oProcessItem.RecordType}"
            },
            // Note 9
            new NoteItem
            {
                Flag = Flags.Insert,
                NoteText = $"Source Entity Code: {oProcessItem.EntityCode}"
            },
            // Note 10
            new NoteItem
            {
                Flag = Flags.Insert,
                NoteText = $"System Activity Number: {oProcessItem.SystemActivityNumber}"
            },
            // Note 11
            new NoteItem
            {
                Flag = Flags.Insert,
                NoteText = $"Transaction Type: {oProcessItem.TransactionType}"
            },
            // Note 12
            new NoteItem
            {
                Flag = Flags.Insert,
                NoteText = $"Receivable Code: {oProcessItem.ReceivableCode}"
            },
            // Note 13
            new NoteItem
            {
                Flag = Flags.Insert,
                NoteText = $"Amount: ${oProcessItem.Amount}"
            },
            // Note 14
            new NoteItem
            {
                Flag = Flags.Insert,
                NoteText = $"Deferred Amount: ${oProcessItem.DeferredAmount}"
            },
            // Note 15
            new NoteItem
            {
                Flag = Flags.Insert,
                NoteText = $"Credit/Debit Code: {oProcessItem.CreditDebit}"
            },
            // Note 16
            new NoteItem
            {
                Flag = Flags.Insert,
                NoteText = $"Accounting Year Month: {oProcessItem.AccountingYearMonth}"
            }
        };
    }
}