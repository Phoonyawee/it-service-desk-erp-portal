using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SupportFlow.Data;
using SupportFlow.Models;

namespace SupportFlow.Pages;

[Authorize(Roles = "IT Support,Admin")]
public class AuditModel(ApplicationDbContext db) : PageModel
{
    public IReadOnlyList<AuditLog> Logs { get; private set; } = [];
    public async Task OnGetAsync() => Logs = await db.AuditLogs.AsNoTracking().OrderByDescending(l => l.CreatedAt).Take(100).ToListAsync();
}
