using LLeague.Api.Application.Abstractions;
using LLeague.Api.Domain;
using LLeague.Api.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LLeague.Api.Application.Services;

public class SeasonService(IAppDbContext db)
{
    public async Task<IEnumerable<SeasonResponse>> ListAsync()
    {
        return await db.Seasons
            .OrderByDescending(s => s.Year)
            .Select(s => new SeasonResponse(s.Id, s.Name, s.Year))
            .ToListAsync();
    }

    public async Task<SeasonResponse> GetAsync(Guid id)
    {
        Season season = await db.Seasons.FindAsync(id) ?? throw new NotFoundException();
        return Map(season);
    }

    public async Task<SeasonResponse> CreateAsync(SeasonRequest req)
    {
        Require(req);
        var season = new Season { Id = Guid.NewGuid(), Name = req.Name.Trim(), Year = req.Year };
        _ = db.Seasons.Add(season);
        _ = await db.SaveChangesAsync();
        return Map(season);
    }

    public async Task<SeasonResponse> UpdateAsync(Guid id, SeasonRequest req)
    {
        Season season = await db.Seasons.FindAsync(id) ?? throw new NotFoundException();
        Require(req);
        season.Name = req.Name.Trim();
        season.Year = req.Year;
        _ = await db.SaveChangesAsync();
        return Map(season);
    }

    public async Task DeleteAsync(Guid id)
    {
        Season season = await db.Seasons.FindAsync(id) ?? throw new NotFoundException();
        _ = db.Seasons.Remove(season);
        _ = await db.SaveChangesAsync();
    }

    private static void Require(SeasonRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
        {
            throw new ValidationException("Name is required");
        }
    }

    private static SeasonResponse Map(Season s) => new(s.Id, s.Name, s.Year);
}
