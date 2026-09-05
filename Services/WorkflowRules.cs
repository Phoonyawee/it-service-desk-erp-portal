using SupportFlow.Models;

namespace SupportFlow.Services;

public static class WorkflowRules
{
    public static bool CanMove(TicketStatus from, TicketStatus to) => (from, to) switch
    {
        (TicketStatus.Open, TicketStatus.InProgress) => true,
        (TicketStatus.InProgress, TicketStatus.Resolved) => true,
        (TicketStatus.Resolved, TicketStatus.Closed) => true,
        _ => false
    };

    public static bool CanMove(AccessRequestStatus from, AccessRequestStatus to) => (from, to) switch
    {
        (AccessRequestStatus.Pending, AccessRequestStatus.Approved or AccessRequestStatus.Rejected) => true,
        (AccessRequestStatus.Approved, AccessRequestStatus.Fulfilled) => true,
        _ => false
    };

    public static void SelfCheck()
    {
        if (!CanMove(TicketStatus.Open, TicketStatus.InProgress) ||
            CanMove(TicketStatus.Open, TicketStatus.Closed) ||
            !CanMove(AccessRequestStatus.Pending, AccessRequestStatus.Approved) ||
            CanMove(AccessRequestStatus.Rejected, AccessRequestStatus.Fulfilled))
        {
            throw new InvalidOperationException("Workflow self-check failed.");
        }

        Console.WriteLine("Workflow self-check passed.");
    }
}
