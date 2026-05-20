using FinTracker.Application.DTOs;
using FinTracker.Application.Interfaces;
using FinTracker.Domain.Entities;
using FinTracker.Domain.Enums;

namespace FinTracker.Application.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(
        string userId,
        TransactionType? type = null,
        CancellationToken cancellationToken = default)
    {
        var categories = await categoryRepository.GetAllAsync(userId, type, cancellationToken);
        return categories.Select(MapToDto).ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, userId, cancellationToken);
        return category is null ? null : MapToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(string userId, CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            UserId = userId,
            Name = dto.Name,
            Type = dto.Type,
            Color = dto.Color,
            Icon = dto.Icon
        };

        var created = await categoryRepository.AddAsync(category, cancellationToken);
        return MapToDto(created);
    }

    public async Task<CategoryDto> UpdateAsync(int id, string userId, CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, userId, cancellationToken)
            ?? throw new InvalidOperationException("Category not found.");

        category.Name = dto.Name;
        category.Type = dto.Type;
        category.Color = dto.Color;
        category.Icon = dto.Icon;

        await categoryRepository.UpdateAsync(category, cancellationToken);
        return MapToDto(category);
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, userId, cancellationToken)
            ?? throw new InvalidOperationException("Category not found.");

        if (await categoryRepository.HasTransactionsAsync(id, userId, cancellationToken))
        {
            throw new InvalidOperationException("Cannot delete a category that has transactions.");
        }

        await categoryRepository.DeleteAsync(category, cancellationToken);
    }

    private static CategoryDto MapToDto(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Type = category.Type,
        Color = category.Color,
        Icon = category.Icon
    };
}
