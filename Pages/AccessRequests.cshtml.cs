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
public class AccessRequestsModel(ApplicationDbContext db, UserManager<IdentityUser> users) : PageModel
{
    public IReadOnlyList<AccessRequest> Requests { get; private set; } = [];
    public bool CanDecide => User.IsInRole("Manager") || User.IsInRole("Admin");
    public bool CanFulfill => User.IsInRole("IT Support") || User.IsInRole("Admin");
    private bool CanViewAll => CanDecide || CanFulfill;
    [BindProperty] public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required, StringLength(60)] public string Module { get; set; } = "Inventory";
        [Required, StringLength(30)] public string AccessLevel { get; set; } = "Read";
        [Required, StringLength(500)] public string Reason { get; set; } = "";
    }

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid) { await LoadAsync(); return Page(); }
        var user = await users.GetUserAsync(User) ?? throw new InvalidOperationException("Signed-in user not found.");
        db.AccessRequests.Add(new AccessRequest { Module = Input.Module, AccessLevel = Input.AccessLevel, Reason = Input.Reason, RequestedById = user.Id, RequestedByName = user.Email ?? "Employee" });
        db.AuditLogs.Add(Log(user.Email, "Requested ERP access", $"{Input.Module} / {Input.AccessLevel}"));
        await db.SaveChangesAsync();
        TempData["Message"] = "ERP access request submitted.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveAsync(int id, AccessRequestStatus next)
    {
        var request = await db.AccessRequests.FindAsync(id);
        if (request is null) return NotFound();
        var authorized = request.Status == AccessRequestStatus.Pending ? CanDecide : CanFulfill;
        if (!authorized) return Forbid();
        if (!WorkflowRules.CanMove(request.Status, next)) return BadRequest("Invalid access request transition.");
        request.Status = next;
        db.AuditLogs.Add(Log(User.Identity?.Name, $"Moved ERP request to {next}", $"Request #{id}"));
        await db.SaveChangesAsync();
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        var query = db.AccessRequests.AsNoTracking();
        if (!CanViewAll)
        {
            var id = users.GetUserId(User);
            query = query.Where(r => r.RequestedById == id);
        }
        Requests = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    private static AuditLog Log(string? actor, string action, string record) => new() { Actor = actor ?? "Unknown", Action = action, Record = record };
}
