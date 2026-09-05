using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SupportFlow.Models;

namespace SupportFlow.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // ponytail: EnsureCreated keeps the demo portable; use migrations before production deployment.
        await db.Database.EnsureCreatedAsync();

        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        foreach (var role in new[] { "Employee", "IT Support", "Manager", "Admin" })
            if (!await roles.RoleExistsAsync(role)) EnsureSucceeded(await roles.CreateAsync(new IdentityRole(role)));

        await AddUser(users, "employee@supportflow.local", "Employee");
        await AddUser(users, "support@supportflow.local", "IT Support");
        await AddUser(users, "manager@supportflow.local", "Manager");
        await AddUser(users, "admin@supportflow.local", "Admin");

        if (await db.Tickets.AnyAsync()) return;

        var employee = await users.FindByEmailAsync("employee@supportflow.local") ?? throw new InvalidOperationException();
        db.Tickets.AddRange(
            new Ticket { Title = "Cannot access ERP Inventory", Description = "Inventory menu is missing after a department transfer.", Category = "ERP", Priority = TicketPriority.High, RequestedById = employee.Id, RequestedByName = "Demo Employee" },
            new Ticket { Title = "Laptop cannot connect to Wi-Fi", Description = "Office Wi-Fi disconnects every few minutes.", Category = "Network", RequestedById = employee.Id, RequestedByName = "Demo Employee", Status = TicketStatus.InProgress });
        db.Assets.AddRange(
            new Asset { AssetTag = "NB-001", Type = "Laptop", Model = "Dell Latitude 5450", AssignedTo = "Demo Employee", Department = "Warehouse" },
            new Asset { AssetTag = "PR-004", Type = "Printer", Model = "HP LaserJet Pro", Department = "Accounting", Status = AssetStatus.Repair });
        db.AccessRequests.Add(new AccessRequest { Module = "Inventory", AccessLevel = "Edit", Reason = "Required to update receiving records.", RequestedById = employee.Id, RequestedByName = "Demo Employee" });
        db.AuditLogs.Add(new AuditLog { Actor = "System", Action = "Seeded demo data", Record = "SupportFlow" });
        await db.SaveChangesAsync();
    }

    private static async Task AddUser(UserManager<IdentityUser> users, string email, string role)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await users.CreateAsync(user, "Demo123!");
            if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
        if (!await users.IsInRoleAsync(user, role)) EnsureSucceeded(await users.AddToRoleAsync(user, role));
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
