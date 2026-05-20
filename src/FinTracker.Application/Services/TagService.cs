using FinTracker.Application.DTOs;
using FinTracker.Application.Interfaces;
using FinTracker.Domain.Entities;

namespace FinTracker.Application.Services;

public class TagService(ITagRepository tagRepository) : ITagService
{
    public async Task<IReadOnlyList<TagDto>> GetAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        var tags = await tagRepository.GetAllAsync(userId, cancellationToken);
        return tags.Select(MapToDto).ToList();
    }

    public async Task<TagDto?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var tag = await tagRepository.GetByIdAsync(id, userId, cancellationToken);
        return tag is null ? null : MapToDto(tag);
    }

    public async Task<TagDto> CreateAsync(string userId, CreateTagDto dto, CancellationToken cancellationToken = default)
    {
        var tag = new Tag
        {
            UserId = userId,
            Name = dto.Name,
            Color = dto.Color
        };

        var created = await tagRepository.AddAsync(tag, cancellationToken);
        return MapToDto(created);
    }

    public async Task<TagDto> UpdateAsync(int id, string userId, CreateTagDto dto, CancellationToken cancellationToken = default)
    {
        var tag = await tagRepository.GetByIdAsync(id, userId, cancellationToken)
            ?? throw new InvalidOperationException("Tag not found.");

        tag.Name = dto.Name;
        tag.Color = dto.Color;

        await tagRepository.UpdateAsync(tag, cancellationToken);
        return MapToDto(tag);
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var tag = await tagRepository.GetByIdAsync(id, userId, cancellationToken)
            ?? throw new InvalidOperationException("Tag not found.");

        await tagRepository.DeleteAsync(tag, cancellationToken);
    }

    private static TagDto MapToDto(Tag tag) => new()
    {
        Id = tag.Id,
        Name = tag.Name,
        Color = tag.Color
    };
}
