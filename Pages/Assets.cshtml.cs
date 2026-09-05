using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SupportFlow.Data;
using SupportFlow.Models;

namespace SupportFlow.Pages;

[Authorize(Roles = "IT Support,Admin")]
public class AssetsModel(ApplicationDbContext db) : PageModel
{
    public IReadOnlyList<Asset> Assets { get; private set; } = [];
    [BindProperty] public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required, StringLength(30)] public string AssetTag { get; set; } = "";
        [Required, StringLength(60)] public string Type { get; set; } = "Laptop";
        [Required, StringLength(100)] public string Model { get; set; } = "";
        [StringLength(100)] public string AssignedTo { get; set; } = "";
        [StringLength(60)] public string Department { get; set; } = "";
        public AssetStatus Status { get; set; } = AssetStatus.Active;
    }

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (await db.Assets.AnyAsync(a => a.AssetTag == Input.AssetTag)) ModelState.AddModelError("Input.AssetTag", "Asset tag already exists.");
        if (!ModelState.IsValid) { await LoadAsync(); return Page(); }
        db.Assets.Add(new Asset { AssetTag = Input.AssetTag, Type = Input.Type, Model = Input.Model, AssignedTo = Input.AssignedTo, Department = Input.Department, Status = Input.Status });
        db.AuditLogs.Add(new AuditLog { Actor = User.Identity?.Name ?? "Unknown", Action = "Created asset", Record = Input.AssetTag });
        await db.SaveChangesAsync();
        TempData["Message"] = $"Asset {Input.AssetTag} added.";
        return RedirectToPage();
    }

    private async Task LoadAsync() => Assets = await db.Assets.AsNoTracking().OrderBy(a => a.AssetTag).ToListAsync();
}
