using FinTracker.Application.DTOs;
using FinTracker.Application.Interfaces;
using FinTracker.Domain.Entities;
using FluentValidation;

namespace FinTracker.Application.Services;

public class BudgetService(
    IBudgetRepository budgetRepository,
    ICategoryRepository categoryRepository,
    ITransactionRepository transactionRepository,
    IValidator<CreateBudgetDto> validator) : IBudgetService
{
    public async Task<IReadOnlyList<BudgetDto>> GetAllAsync(
        string userId,
        int? year = null,
        int? month = null,
        CancellationToken cancellationToken = default)
    {
        var budgets = await budgetRepository.GetAllAsync(userId, year, month, cancellationToken);
        var result = new List<BudgetDto>(budgets.Count);

        foreach (var budget in budgets)
        {
            result.Add(await MapToDtoAsync(budget, userId, cancellationToken));
        }

        return result;
    }

    public async Task<BudgetDto?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var budget = await budgetRepository.GetByIdAsync(id, userId, cancellationToken);
        return budget is null ? null : await MapToDtoAsync(budget, userId, cancellationToken);
    }

    public async Task<BudgetDto> CreateAsync(string userId, CreateBudgetDto dto, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);
        await EnsureCategoryExistsAsync(dto.CategoryId, userId, cancellationToken);

        var existing = await budgetRepository.GetByCategoryAndPeriodAsync(
            userId, dto.CategoryId, dto.Year, dto.Month, cancellationToken);

        if (existing is not null)
        {
            throw new InvalidOperationException("A budget for this category and period already exists.");
        }

        var budget = new Budget
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            LimitAmount = dto.LimitAmount,
            Year = dto.Year,
            Month = dto.Month,
            Note = dto.Note
        };

        var created = await budgetRepository.AddAsync(budget, cancellationToken);
        created.Category = await categoryRepository.GetByIdAsync(dto.CategoryId, userId, cancellationToken)
            ?? throw new InvalidOperationException("Category not found.");

        return await MapToDtoAsync(created, userId, cancellationToken);
    }

    public async Task<BudgetDto> UpdateAsync(int id, string userId, CreateBudgetDto dto, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);

        var budget = await budgetRepository.GetByIdAsync(id, userId, cancellationToken)
            ?? throw new InvalidOperationException("Budget not found.");

        await EnsureCategoryExistsAsync(dto.CategoryId, userId, cancellationToken);

        var duplicate = await budgetRepository.GetByCategoryAndPeriodAsync(
            userId, dto.CategoryId, dto.Year, dto.Month, cancellationToken);

        if (duplicate is not null && duplicate.Id != id)
        {
            throw new InvalidOperationException("A budget for this category and period already exists.");
        }

        budget.CategoryId = dto.CategoryId;
        budget.LimitAmount = dto.LimitAmount;
        budget.Year = dto.Year;
        budget.Month = dto.Month;
        budget.Note = dto.Note;

        await budgetRepository.UpdateAsync(budget, cancellationToken);
        budget.Category = await categoryRepository.GetByIdAsync(dto.CategoryId, userId, cancellationToken)
            ?? throw new InvalidOperationException("Category not found.");

        return await MapToDtoAsync(budget, userId, cancellationToken);
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var budget = await budgetRepository.GetByIdAsync(id, userId, cancellationToken)
            ?? throw new InvalidOperationException("Budget not found.");

        await budgetRepository.DeleteAsync(budget, cancellationToken);
    }

    private async Task EnsureCategoryExistsAsync(int categoryId, string userId, CancellationToken cancellationToken)
    {
        if (!await categoryRepository.ExistsAsync(categoryId, userId, cancellationToken))
        {
            throw new InvalidOperationException("Category not found.");
        }
    }

    private async Task<BudgetDto> MapToDtoAsync(Budget budget, string userId, CancellationToken cancellationToken)
    {
        var spent = await transactionRepository.GetExpenseSumForCategoryAsync(
            userId, budget.CategoryId, budget.Year, budget.Month, cancellationToken);

        return new BudgetDto
        {
            Id = budget.Id,
            CategoryId = budget.CategoryId,
            CategoryName = budget.Category?.Name ?? string.Empty,
            CategoryColor = budget.Category?.Color ?? "#000",
            LimitAmount = budget.LimitAmount,
            SpentAmount = spent,
            Year = budget.Year,
            Month = budget.Month
        };
    }
}
