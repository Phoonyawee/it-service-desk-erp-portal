using System.ComponentModel.DataAnnotations;

namespace SupportFlow.Models;

public enum TicketPriority { Low, Medium, High, Critical }
public enum TicketStatus { Open, InProgress, Resolved, Closed }
public enum AssetStatus { Active, Repair, Retired }
public enum AccessRequestStatus { Pending, Approved, Rejected, Fulfilled }

public sealed class Ticket
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Title { get; set; } = "";
    [Required, StringLength(1000)] public string Description { get; set; } = "";
    [Required, StringLength(40)] public string Category { get; set; } = "Hardware";
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    [Required] public string RequestedById { get; set; } = "";
    [StringLength(100)] public string RequestedByName { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
}

public sealed class Asset
{
    public int Id { get; set; }
    [Required, StringLength(30)] public string AssetTag { get; set; } = "";
    [Required, StringLength(60)] public string Type { get; set; } = "Laptop";
    [Required, StringLength(100)] public string Model { get; set; } = "";
    [StringLength(100)] public string AssignedTo { get; set; } = "";
    [StringLength(60)] public string Department { get; set; } = "";
    public AssetStatus Status { get; set; } = AssetStatus.Active;
}

public sealed class AccessRequest
{
    public int Id { get; set; }
    [Required, StringLength(60)] public string Module { get; set; } = "Inventory";
    [Required, StringLength(30)] public string AccessLevel { get; set; } = "Read";
    [Required, StringLength(500)] public string Reason { get; set; } = "";
    [Required] public string RequestedById { get; set; } = "";
    [StringLength(100)] public string RequestedByName { get; set; } = "";
    public AccessRequestStatus Status { get; set; } = AccessRequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class AuditLog
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Actor { get; set; } = "";
    [Required, StringLength(80)] public string Action { get; set; } = "";
    [Required, StringLength(80)] public string Record { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
