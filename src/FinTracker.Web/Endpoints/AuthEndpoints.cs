using System.ComponentModel.DataAnnotations;
using FinTracker.Application.Interfaces;
using FinTracker.Domain.Entities;
using FinTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FinTracker.Web.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/login", LoginAsync).DisableAntiforgery();
        group.MapPost("/register", RegisterAsync).DisableAntiforgery();
        group.MapPost("/logout", LogoutAsync).DisableAntiforgery();
        group.MapGet("/logout", LogoutAsync);
    }

    private static async Task<IResult> LoginAsync(
        [FromForm] LoginRequest request,
        [FromQuery] string? returnUrl,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Results.Redirect("/login?error=empty");

        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
            return Results.Redirect("/login?error=invalid");

        var result = await signInManager.PasswordSignInAsync(
            user.UserName!, request.Password, isPersistent: true, lockoutOnFailure: false);

        return result.Succeeded
            ? Results.Redirect(GetSafeReturnUrl(returnUrl))
            : Results.Redirect("/login?error=invalid");
    }

    private static string GetSafeReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) &&
        returnUrl.StartsWith('/') &&
        !returnUrl.StartsWith("//", StringComparison.Ordinal) &&
        !returnUrl.StartsWith("/\\", StringComparison.Ordinal)
            ? returnUrl
            : "/";

    private static async Task<IResult> RegisterAsync(
        [FromForm] RegisterRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUserProfileRepository profileRepository,
        ICategoryRepository categoryRepository)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Results.Redirect("/register?error=empty");

        var email = request.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var code = Uri.EscapeDataString(string.Join(", ", createResult.Errors.Select(e => e.Description)));
            return Results.Redirect($"/register?error={code}");
        }

        await userManager.AddToRoleAsync(user, "User");

        await profileRepository.AddAsync(new UserProfile
        {
            UserId = user.Id,
            DisplayName = email,
            PreferredCurrency = "RUB",
            CreatedAt = DateTime.UtcNow
        });
        await profileRepository.SaveChangesAsync();

        await SeedDefaultCategoriesAsync(categoryRepository, user.Id);

        await signInManager.SignInAsync(user, isPersistent: true);
        return Results.Redirect("/");
    }

    private static async Task<IResult> LogoutAsync(SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return Results.Redirect("/login");
    }

    private static async Task SeedDefaultCategoriesAsync(ICategoryRepository categoryRepository, string userId)
    {
        var defaults = new[]
        {
            (Name: "Продукты", Type: Domain.Enums.TransactionType.Expense, Color: "#4CAF50"),
            (Name: "Транспорт", Type: Domain.Enums.TransactionType.Expense, Color: "#2196F3"),
            (Name: "Зарплата", Type: Domain.Enums.TransactionType.Income, Color: "#FF9800")
        };

        foreach (var (name, type, color) in defaults)
        {
            await categoryRepository.AddAsync(new Category
            {
                UserId = userId,
                Name = name,
                Type = type,
                Color = color
            });
        }
    }

    public sealed class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }

    public sealed class RegisterRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(6)]
        public string Password { get; set; } = "";
    }
}
