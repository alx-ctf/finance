using FinTracker.Application.Helpers;
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

    private const int DemoMinTransactionCount = 120;

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

        if (!await context.UserProfiles.AnyAsync(p => p.UserId == user.Id))
        {
            context.UserProfiles.Add(new UserProfile
            {
                UserId = user.Id,
                DisplayName = "Демо пользователь",
                PreferredCurrency = "RUB",
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        var txCount = await context.Transactions.CountAsync(t => t.UserId == user.Id);
        if (txCount >= DemoMinTransactionCount)
        {
            var categories = await EnsureDemoCategoriesAsync(context, user.Id);
            await EnsureDemoBudgetsAsync(context, user.Id, categories, DateTime.UtcNow);
            await EnsureDemoTagsAsync(context, user.Id);
            await RecalculateAccountBalancesAsync(context, user.Id);
            return;
        }

        try
        {
            if (!await context.Accounts.AnyAsync(a => a.UserId == user.Id))
                await SeedDemoDataAsync(context, user.Id, logger);
            else
                await EnrichDemoDataAsync(context, user.Id, logger);

            await RecalculateAccountBalancesAsync(context, user.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to seed demo data for user {UserId}", user.Id);
        }
    }

    private static async Task SeedDemoDataAsync(
        AppDbContext context,
        string userId,
        ILogger logger)
    {
        var now = DateTime.UtcNow;
        var categories = await EnsureDemoCategoriesAsync(context, userId);
        var accounts = await CreateDemoAccountsAsync(context, userId, now);
        var tags = await EnsureDemoTagsAsync(context, userId);

        var transactions = BuildDemoTransactions(userId, accounts, categories, now);
        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();

        await EnsureDemoBudgetsAsync(context, userId, categories, now);
        await AttachDemoTransactionTagsAsync(context, transactions, tags);
        await context.SaveChangesAsync();

        logger.LogInformation(
            "Demo data seeded for user {UserId}: {TxCount} transactions",
            userId,
            transactions.Count);
    }

    private static async Task EnrichDemoDataAsync(
        AppDbContext context,
        string userId,
        ILogger logger)
    {
        var now = DateTime.UtcNow;
        var categories = await EnsureDemoCategoriesAsync(context, userId);
        var accounts = await context.Accounts.Where(a => a.UserId == userId).ToListAsync();

        if (!accounts.Any(a => a.Name == "Накопительный"))
        {
            accounts.Add(new Account
            {
                UserId = userId,
                Name = "Накопительный",
                Type = AccountType.Savings,
                Balance = 0m,
                Currency = "RUB",
                Description = "Подушка безопасности",
                CreatedAt = now
            });
            context.Accounts.Add(accounts[^1]);
            await context.SaveChangesAsync();
        }

        var tags = await EnsureDemoTagsAsync(context, userId);
        var transactions = BuildDemoTransactions(userId, accounts, categories, now);
        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();

        await EnsureDemoBudgetsAsync(context, userId, categories, now);
        await AttachDemoTransactionTagsAsync(context, transactions, tags);
        await context.SaveChangesAsync();

        logger.LogInformation(
            "Demo data enriched for user {UserId}: +{TxCount} transactions",
            userId,
            transactions.Count);
    }

    private static async Task<DemoCategories> EnsureDemoCategoriesAsync(AppDbContext context, string userId)
    {
        var food = await EnsureCategoryAsync(context, userId, "Продукты", TransactionType.Expense, "#4CAF50", "shopping_cart");
        var transport = await EnsureCategoryAsync(context, userId, "Транспорт", TransactionType.Expense, "#2196F3", "directions_car");
        var entertainment = await EnsureCategoryAsync(context, userId, "Развлечения", TransactionType.Expense, "#9C27B0", "movie");
        var health = await EnsureCategoryAsync(context, userId, "Здоровье", TransactionType.Expense, "#E91E63", "medical_services");
        var utilities = await EnsureCategoryAsync(context, userId, "ЖКХ", TransactionType.Expense, "#795548", "home");
        var shopping = await EnsureCategoryAsync(context, userId, "Покупки", TransactionType.Expense, "#FF5722", "shopping_bag");
        var cafe = await EnsureCategoryAsync(context, userId, "Кафе", TransactionType.Expense, "#FF9800", "local_cafe");
        var education = await EnsureCategoryAsync(context, userId, "Образование", TransactionType.Expense, "#3F51B5", "school");
        var subscriptions = await EnsureCategoryAsync(context, userId, "Подписки", TransactionType.Expense, "#607D8B", "subscriptions");

        var salary = await EnsureCategoryAsync(context, userId, "Зарплата", TransactionType.Income, "#8BC34A", "payments");
        var freelance = await EnsureCategoryAsync(context, userId, "Фриланс", TransactionType.Income, "#00BCD4", "work");
        var cashback = await EnsureCategoryAsync(context, userId, "Кэшбэк", TransactionType.Income, "#CDDC39", "savings");

        return new DemoCategories(
            food, transport, entertainment, health, utilities,
            shopping, cafe, education, subscriptions,
            salary, freelance, cashback);
    }

    private static async Task<List<Account>> CreateDemoAccountsAsync(
        AppDbContext context,
        string userId,
        DateTime now)
    {
        var accounts = new List<Account>
        {
            new()
            {
                UserId = userId,
                Name = "Наличные",
                Type = AccountType.Cash,
                Balance = 0m,
                Currency = "RUB",
                CreatedAt = now
            },
            new()
            {
                UserId = userId,
                Name = "Основная карта",
                Type = AccountType.Card,
                Balance = 0m,
                Currency = "RUB",
                Description = "Дебетовая карта",
                CreatedAt = now
            },
            new()
            {
                UserId = userId,
                Name = "Накопительный",
                Type = AccountType.Savings,
                Balance = 0m,
                Currency = "RUB",
                Description = "Подушка безопасности",
                CreatedAt = now
            }
        };
        context.Accounts.AddRange(accounts);
        await context.SaveChangesAsync();
        return accounts;
    }

    private static async Task<List<Tag>> EnsureDemoTagsAsync(AppDbContext context, string userId)
    {
        var tagDefs = new (string Name, string Color)[]
        {
            ("обязательное", "#F44336"),
            ("плановое", "#607D8B"),
            ("разовое", "#FF9800"),
            ("подарок", "#E91E63"),
            ("работа", "#2196F3"),
            ("отпуск", "#4CAF50")
        };

        foreach (var (name, color) in tagDefs)
        {
            if (!await context.Tags.AnyAsync(t => t.UserId == userId && t.Name == name))
            {
                context.Tags.Add(new Tag { UserId = userId, Name = name, Color = color });
            }
        }

        await context.SaveChangesAsync();
        return await context.Tags.Where(t => t.UserId == userId).ToListAsync();
    }

    private static List<Transaction> BuildDemoTransactions(
        string userId,
        List<Account> accounts,
        DemoCategories categories,
        DateTime now)
    {
        var cash = accounts.First(a => a.Name == "Наличные");
        var card = accounts.First(a => a.Name == "Основная карта");
        var savings = accounts.FirstOrDefault(a => a.Name == "Накопительный") ?? card;

        var rng = new Random(42);
        var transactions = new List<Transaction>();
        var createdAt = now;

        var foodNotes = new[] { "Супермаркет", "Пятёрочка", "Магнит", "Рынок", "Доставка продуктов" };
        var transportNotes = new[] { "Метро", "Такси", "Бензин", "Каршеринг", "Автобус" };
        var cafeNotes = new[] { "Кофейня", "Обед", "Пекарня", "Суши", "Пицца" };
        var shopNotes = new[] { "Одежда", "Маркетплейс", "Электроника", "Косметика", "Обувь" };

        for (var monthOffset = 5; monthOffset >= 0; monthOffset--)
        {
            var monthStart = UtcDateHelper.StartOfMonthUtc(now.AddMonths(-monthOffset));
            var year = monthStart.Year;
            var month = monthStart.Month;
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var salaryBase = 82_000m + monthOffset * 1_500m;

            AddIncome(transactions, userId, card.Id, categories.Salary.Id, salaryBase,
                DayInMonth(year, month, 5), "Зарплата за месяц", createdAt);

            if (monthOffset % 2 == 0)
            {
                AddIncome(transactions, userId, card.Id, categories.Freelance.Id,
                    rng.Next(12_000, 28_000), DayInMonth(year, month, 15), "Проектная оплата", createdAt);
            }

            if (rng.NextDouble() > 0.35)
            {
                AddIncome(transactions, userId, card.Id, categories.Cashback.Id,
                    rng.Next(300, 2_500), DayInMonth(year, month, 22), "Кэшбэк по карте", createdAt);
            }

            if (monthOffset == 1)
            {
                AddIncome(transactions, userId, savings.Id, categories.Salary.Id, 15_000m,
                    DayInMonth(year, month, 28), "Перевод на накопительный", createdAt);
            }

            AddExpense(transactions, userId, card.Id, categories.Utilities.Id,
                rng.Next(4_800, 7_200), DayInMonth(year, month, 8), "Квартплата и связь", createdAt);

            AddExpense(transactions, userId, card.Id, categories.Subscriptions.Id,
                rng.Next(890, 2_400), DayInMonth(year, month, 12), "Подписки: стриминг, облако", createdAt);

            if (monthOffset % 3 == 0)
            {
                AddExpense(transactions, userId, card.Id, categories.Education.Id,
                    rng.Next(3_500, 12_000), DayInMonth(year, month, 18), "Онлайн-курс", createdAt);
            }

            if (monthOffset == 2)
            {
                AddExpense(transactions, userId, card.Id, categories.Health.Id,
                    4_200m, DayInMonth(year, month, 14), "Стоматолог", createdAt);
            }

            AddExpense(transactions, userId, card.Id, categories.Entertainment.Id,
                rng.Next(1_500, 6_000), DayInMonth(year, month, rng.Next(20, Math.Min(26, daysInMonth))),
                monthOffset % 2 == 0 ? "Концерт" : "Кино и досуг", createdAt);

            for (var i = 0; i < rng.Next(10, 15); i++)
            {
                var day = rng.Next(1, daysInMonth + 1);
                AddExpense(transactions, userId,
                    rng.NextDouble() > 0.25 ? card.Id : cash.Id,
                    categories.Food.Id,
                    (decimal)(rng.NextDouble() * 2_800 + 180),
                    DayInMonth(year, month, day),
                    foodNotes[rng.Next(foodNotes.Length)],
                    createdAt);
            }

            for (var i = 0; i < rng.Next(6, 10); i++)
            {
                var day = rng.Next(1, daysInMonth + 1);
                AddExpense(transactions, userId,
                    rng.NextDouble() > 0.4 ? card.Id : cash.Id,
                    categories.Transport.Id,
                    (decimal)(rng.NextDouble() * 900 + 45),
                    DayInMonth(year, month, day),
                    transportNotes[rng.Next(transportNotes.Length)],
                    createdAt);
            }

            for (var i = 0; i < rng.Next(5, 9); i++)
            {
                var day = rng.Next(1, daysInMonth + 1);
                AddExpense(transactions, userId, card.Id, categories.Cafe.Id,
                    (decimal)(rng.NextDouble() * 1_600 + 120),
                    DayInMonth(year, month, day),
                    cafeNotes[rng.Next(cafeNotes.Length)],
                    createdAt);
            }

            for (var i = 0; i < rng.Next(2, 5); i++)
            {
                var day = rng.Next(1, daysInMonth + 1);
                AddExpense(transactions, userId, card.Id, categories.Shopping.Id,
                    (decimal)(rng.NextDouble() * 8_500 + 500),
                    DayInMonth(year, month, day),
                    shopNotes[rng.Next(shopNotes.Length)],
                    createdAt);
            }

            if (monthOffset <= 1)
            {
                for (var i = 0; i < rng.Next(2, 4); i++)
                {
                    var day = rng.Next(1, daysInMonth + 1);
                    AddExpense(transactions, userId, cash.Id, categories.Health.Id,
                        (decimal)(rng.NextDouble() * 2_000 + 400),
                        DayInMonth(year, month, day),
                        "Аптека", createdAt);
                }
            }
        }

        return transactions;
    }

    private static async Task EnsureDemoBudgetsAsync(
        AppDbContext context,
        string userId,
        DemoCategories categories,
        DateTime now)
    {
        for (var offset = 0; offset < 3; offset++)
        {
            var refMonth = now.AddMonths(-offset);
            var year = refMonth.Year;
            var month = refMonth.Month;

            await EnsureBudgetAsync(context, userId, categories.Food.Id, year, month, 18_000m, "Лимит на продукты");
            await EnsureBudgetAsync(context, userId, categories.Transport.Id, year, month, 6_000m);
            await EnsureBudgetAsync(context, userId, categories.Cafe.Id, year, month, 5_000m);
            await EnsureBudgetAsync(context, userId, categories.Entertainment.Id, year, month, 8_000m);
            await EnsureBudgetAsync(context, userId, categories.Utilities.Id, year, month, 8_500m);
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureBudgetAsync(
        AppDbContext context,
        string userId,
        int categoryId,
        int year,
        int month,
        decimal limit,
        string? note = null)
    {
        if (await context.Budgets.AnyAsync(b =>
                b.UserId == userId && b.CategoryId == categoryId && b.Year == year && b.Month == month))
            return;

        context.Budgets.Add(new Budget
        {
            UserId = userId,
            CategoryId = categoryId,
            LimitAmount = limit,
            Year = year,
            Month = month,
            Note = note
        });
    }

    private static async Task AttachDemoTransactionTagsAsync(
        AppDbContext context,
        List<Transaction> transactions,
        List<Tag> tags)
    {
        if (tags.Count == 0 || transactions.Count == 0)
            return;

        var tagByName = tags.ToDictionary(t => t.Name, t => t.Id);
        var mandatory = tagByName.GetValueOrDefault("обязательное");
        var planned = tagByName.GetValueOrDefault("плановое");
        var work = tagByName.GetValueOrDefault("работа");
        var oneOff = tagByName.GetValueOrDefault("разовое");

        var rng = new Random(99);
        var links = new List<TransactionTag>();

        foreach (var tx in transactions)
        {
            if (tx.Type == TransactionType.Income && work != 0 && rng.NextDouble() > 0.4)
            {
                links.Add(new TransactionTag { TransactionId = tx.Id, TagId = work });
                continue;
            }

            if (tx.Note?.Contains("Квартплата", StringComparison.Ordinal) == true && mandatory != 0)
            {
                links.Add(new TransactionTag { TransactionId = tx.Id, TagId = mandatory });
                continue;
            }

            if (tx.Note?.Contains("Подписки", StringComparison.Ordinal) == true && planned != 0)
            {
                links.Add(new TransactionTag { TransactionId = tx.Id, TagId = planned });
                continue;
            }

            if (rng.NextDouble() > 0.72 && oneOff != 0)
                links.Add(new TransactionTag { TransactionId = tx.Id, TagId = oneOff });
            else if (rng.NextDouble() > 0.55 && planned != 0)
                links.Add(new TransactionTag { TransactionId = tx.Id, TagId = planned });
        }

        context.TransactionTags.AddRange(links);
    }

    private static void AddIncome(
        List<Transaction> list,
        string userId,
        int accountId,
        int categoryId,
        decimal amount,
        DateTime date,
        string note,
        DateTime createdAt) =>
        list.Add(CreateTransaction(userId, accountId, categoryId, amount, TransactionType.Income, date, note, createdAt));

    private static void AddIncome(
        List<Transaction> list,
        string userId,
        int accountId,
        int categoryId,
        int amount,
        DateTime date,
        string note,
        DateTime createdAt) =>
        AddIncome(list, userId, accountId, categoryId, (decimal)amount, date, note, createdAt);

    private static void AddExpense(
        List<Transaction> list,
        string userId,
        int accountId,
        int categoryId,
        decimal amount,
        DateTime date,
        string note,
        DateTime createdAt) =>
        list.Add(CreateTransaction(userId, accountId, categoryId, amount, TransactionType.Expense, date, note, createdAt));

    private static Transaction CreateTransaction(
        string userId,
        int accountId,
        int categoryId,
        decimal amount,
        TransactionType type,
        DateTime date,
        string note,
        DateTime createdAt) =>
        new()
        {
            UserId = userId,
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = Math.Round(amount, 2),
            Type = type,
            Date = UtcDateHelper.ToUtcDate(date),
            Note = note,
            CreatedAt = createdAt
        };

    private static DateTime DayInMonth(int year, int month, int day)
    {
        var maxDay = DateTime.DaysInMonth(year, month);
        return new DateTime(year, month, Math.Min(day, maxDay), 0, 0, 0, DateTimeKind.Utc);
    }

    private static async Task<Category> EnsureCategoryAsync(
        AppDbContext context,
        string userId,
        string name,
        TransactionType type,
        string color,
        string icon)
    {
        var existing = await context.Categories
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == name && c.Type == type);

        if (existing is not null)
            return existing;

        var category = new Category
        {
            UserId = userId,
            Name = name,
            Type = type,
            Color = color,
            Icon = icon
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }

    private static async Task RecalculateAccountBalancesAsync(AppDbContext context, string userId)
    {
        var accounts = await context.Accounts
            .Where(a => a.UserId == userId)
            .ToListAsync();

        var transactions = await context.Transactions
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Date)
            .ThenBy(t => t.Id)
            .ToListAsync();

        foreach (var account in accounts)
            account.Balance = 0;

        foreach (var tx in transactions)
        {
            var account = accounts.FirstOrDefault(a => a.Id == tx.AccountId);
            if (account is null) continue;
            account.Balance = FinanceCalculator.ApplyTransaction(account.Balance, tx.Amount, tx.Type);
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new IdentityRole(roleName));
    }

    private sealed record DemoCategories(
        Category Food,
        Category Transport,
        Category Entertainment,
        Category Health,
        Category Utilities,
        Category Shopping,
        Category Cafe,
        Category Education,
        Category Subscriptions,
        Category Salary,
        Category Freelance,
        Category Cashback);
}

