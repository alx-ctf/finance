using FinTracker.Application.Interfaces;
using FinTracker.Application.Services;
using FinTracker.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FinTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateTransactionDtoValidator>();

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ITagService, TagService>();

        return services;
    }
}
