using FinTracker.Domain.Entities;
using FinTracker.Domain.Enums;
using FinTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinTracker.Infrastructure.Data;

public static class DbInitializer
{
    public const string DemoEmail = "demo@fintracker.local";
    public const string DemoPassword = "Demo123!";

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        await context.Database.MigrateAsync();

        await EnsureRoleAsync(roleManager, "User");
        await EnsureRoleAsync(roleManager, "Admin");

        var user = await userManager.FindByEmailAsync(DemoEmail);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = DemoEmail,
                Email = DemoEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, DemoPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Failed to create demo user: {Errors}", errors);
                return;
            }

            await userManager.AddToRoleAsync(user, "User");
            logger.LogInformation("Demo user created: {Email}", DemoEmail);
        }

        if (await context.UserProfiles.AnyAsync(p => p.UserId == user.Id))
            return;

        var now = DateTime.UtcNow;
        var profile = new UserProfile
        {
            UserId = user.Id,
            DisplayName = "Демо пользователь",
            PreferredCurrency = "RUB",
            CreatedAt = now
        };
        context.UserProfiles.Add(profile);

        var categories = new List<Category>
        {
            new() { UserId = user.Id, Name = "Продукты", Type = TransactionType.Expense, Color = "#4CAF50", Icon = "shopping_cart" },
            new() { UserId = user.Id, Name = "Транспорт", Type = TransactionType.Expense, Color = "#2196F3", Icon = "directions_car" },
            new() { UserId = user.Id, Name = "Развлечения", Type = TransactionType.Expense, Color = "#9C27B0", Icon = "movie" },
            new() { UserId = user.Id, Name = "Зарплата", Type = TransactionType.Income, Color = "#FF9800", Icon = "payments" },
            new() { UserId = user.Id, Name = "Подработка", Type = TransactionType.Income, Color = "#00BCD4", Icon = "work" }
        };
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var accounts = new List<Account>
        {
            new()
            {
                UserId = user.Id,
                Name = "Наличные",
                Type = AccountType.Cash,
                Balance = 5000m,
                Currency = "RUB",
                CreatedAt = now
            },
            new()
            {
                UserId = user.Id,
                Name = "Основная карта",
                Type = AccountType.Card,
                Balance = 42500m,
                Currency = "RUB",
                Description = "Дебетовая карта",
                CreatedAt = now
            }
        };
        context.Accounts.AddRange(accounts);
        await context.SaveChangesAsync();

        var tags = new List<Tag>
        {
            new() { UserId = user.Id, Name = "обязательное", Color = "#F44336" },
            new() { UserId = user.Id, Name = "плановое", Color = "#607D8B" }
        };
        context.Tags.AddRange(tags);
        await context.SaveChangesAsync();

        var food = categories[0];
        var transport = categories[1];
        var salary = categories[3];
        var cash = accounts[0];
        var card = accounts[1];

        var transactions = new List<Transaction>
        {
            new()
            {
                UserId = user.Id,
                AccountId = card.Id,
                CategoryId = salary.Id,
                Amount = 85000m,
                Type = TransactionType.Income,
                Date = now.AddDays(-5).Date,
                Note = "Зарплата за месяц",
                CreatedAt = now
            },
            new()
            {
                UserId = user.Id,
                AccountId = card.Id,
                CategoryId = food.Id,
                Amount = 2450.50m,
                Type = TransactionType.Expense,
                Date = now.AddDays(-3).Date,
                Note = "Супермаркет",
                CreatedAt = now
            },
            new()
            {
                UserId = user.Id,
                AccountId = cash.Id,
                CategoryId = transport.Id,
                Amount = 350m,
                Type = TransactionType.Expense,
                Date = now.AddDays(-2).Date,
                Note = "Метро",
                CreatedAt = now
            },
            new()
            {
                UserId = user.Id,
                AccountId = card.Id,
                CategoryId = food.Id,
                Amount = 890m,
                Type = TransactionType.Expense,
                Date = now.AddDays(-1).Date,
                CreatedAt = now
            }
        };
        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();

        context.TransactionTags.Add(new TransactionTag
        {
            TransactionId = transactions[1].Id,
            TagId = tags[0].Id
        });

        var currentMonth = now.Month;
        var currentYear = now.Year;
        context.Budgets.AddRange(
            new Budget
            {
                UserId = user.Id,
                CategoryId = food.Id,
                LimitAmount = 15000m,
                Year = currentYear,
                Month = currentMonth,
                Note = "Лимит на продукты"
            },
            new Budget
            {
                UserId = user.Id,
                CategoryId = transport.Id,
                LimitAmount = 5000m,
                Year = currentYear,
                Month = currentMonth
            });

        await context.SaveChangesAsync();
        logger.LogInformation("Demo data seeded for user {UserId}", user.Id);
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new IdentityRole(roleName));
    }
}
