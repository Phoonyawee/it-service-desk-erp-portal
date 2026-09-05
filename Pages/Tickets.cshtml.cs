using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SupportFlow.Data;
using SupportFlow.Models;
using SupportFlow.Services;

namespace SupportFlow.Pages;

[Authorize]
public class TicketsModel(ApplicationDbContext db, UserManager<IdentityUser> users) : PageModel
{
    public IReadOnlyList<Ticket> Tickets { get; private set; } = [];
    public bool CanProcess => User.IsInRole("IT Support") || User.IsInRole("Admin");

    [BindProperty] public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required, StringLength(100)] public string Title { get; set; } = "";
        [Required, StringLength(1000)] public string Description { get; set; } = "";
        [Required, StringLength(40)] public string Category { get; set; } = "Hardware";
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    }

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid) { await LoadAsync(); return Page(); }
        var user = await users.GetUserAsync(User) ?? throw new InvalidOperationException("Signed-in user not found.");
        var ticket = new Ticket
        {
            Title = Input.Title, Description = Input.Description, Category = Input.Category,
            Priority = Input.Priority, RequestedById = user.Id, RequestedByName = user.Email ?? "Employee"
        };
        db.Tickets.Add(ticket);
        db.AuditLogs.Add(Log(user.Email, "Created ticket", Input.Title));
        await db.SaveChangesAsync();
        TempData["Message"] = $"Ticket #{ticket.Id} created.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveAsync(int id, TicketStatus next)
    {
        if (!CanProcess) return Forbid();
        var ticket = await db.Tickets.FindAsync(id);
        if (ticket is null) return NotFound();
        if (!WorkflowRules.CanMove(ticket.Status, next)) return BadRequest("Invalid ticket status transition.");
        ticket.Status = next;
        ticket.ClosedAt = next == TicketStatus.Closed ? DateTime.UtcNow : null;
        db.AuditLogs.Add(Log(User.Identity?.Name, $"Moved ticket to {next}", $"Ticket #{id}"));
        await db.SaveChangesAsync();
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        var query = db.Tickets.AsNoTracking();
        if (!CanProcess && !User.IsInRole("Manager"))
        {
            var id = users.GetUserId(User);
            query = query.Where(t => t.RequestedById == id);
        }
        Tickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    private static AuditLog Log(string? actor, string action, string record) =>
        new() { Actor = actor ?? "Unknown", Action = action, Record = record };
}
