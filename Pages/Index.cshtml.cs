using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportFlow.Data;
using SupportFlow.Models;

namespace SupportFlow.Pages;

public class IndexModel(ApplicationDbContext db, SignInManager<IdentityUser> signInManager) : PageModel
{
    public int OpenTickets { get; private set; }
    public int ActiveAssets { get; private set; }
    public int PendingAccess { get; private set; }
    public IReadOnlyList<Ticket> RecentTickets { get; private set; } = [];

    public async Task OnGetAsync()
    {
        if (!signInManager.IsSignedIn(User)) return;
        OpenTickets = await db.Tickets.CountAsync(t => t.Status != TicketStatus.Closed);
        ActiveAssets = await db.Assets.CountAsync(a => a.Status == AssetStatus.Active);
        PendingAccess = await db.AccessRequests.CountAsync(r => r.Status == AccessRequestStatus.Pending);
        RecentTickets = await db.Tickets.OrderByDescending(t => t.CreatedAt).Take(5).ToListAsync();
    }
}
