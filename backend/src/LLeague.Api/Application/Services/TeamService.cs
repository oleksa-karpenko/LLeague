using System.Text.RegularExpressions;
using LLeague.Api.Application.Abstractions;
using LLeague.Api.Domain;
using LLeague.Api.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LLeague.Api.Application.Services;

public partial class TeamService(IAppDbContext db)
{
    [GeneratedRegex("^[A-Za-z]{2}$")]
    private static partial Regex RegionRegex();

    public async Task<IEnumerable<TeamResponse>> ListAsync()
    {
        return await db.Teams.OrderBy(t => t.Number).Select(t => Map(t)).ToListAsync();
    }

    public async Task<TeamResponse> GetAsync(Guid id)
    {
        Team t = await db.Teams.FindAsync(id) ?? throw new NotFoundException();
        return Map(t);
    }

    public async Task<TeamResponse> CreateAsync(TeamRequest req)
    {
        Validate(req);
        var region = req.Region.Trim().ToUpperInvariant();
        if (await db.Teams.AnyAsync(t => t.Number == req.Number && t.Region == region))
        {
            throw new ConflictException($"Team {req.Number} already exists in region {region}");
        }

        var t = new Team
        {
            Id = Guid.NewGuid(),
            Number = req.Number,
            Name = req.Name.Trim(),
            Affiliation = req.Affiliation.Trim(),
            City = req.City.Trim(),
            Region = region
        };
        _ = db.Teams.Add(t);
        _ = await db.SaveChangesAsync();
        return Map(t);
    }

    public async Task<TeamResponse> UpdateAsync(Guid id, TeamRequest req)
    {
        Team t = await db.Teams.FindAsync(id) ?? throw new NotFoundException();
        Validate(req);
        var region = req.Region.Trim().ToUpperInvariant();
        if (await db.Teams.AnyAsync(x => x.Id != id && x.Number == req.Number && x.Region == region))
        {
            throw new ConflictException($"Team {req.Number} already exists in region {region}");
        }

        t.Number = req.Number;
        t.Name = req.Name.Trim();
        t.Affiliation = req.Affiliation.Trim();
        t.City = req.City.Trim();
        t.Region = region;
        _ = await db.SaveChangesAsync();
        return Map(t);
    }

    public async Task DeleteAsync(Guid id)
    {
        Team t = await db.Teams.FindAsync(id) ?? throw new NotFoundException();
        _ = db.Teams.Remove(t);
        _ = await db.SaveChangesAsync();
    }

    private static void Validate(TeamRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
        {
            throw new ValidationException("Name is required");
        }

        if (req.Number <= 0)
        {
            throw new ValidationException("Team number must be a positive integer");
        }

        if (string.IsNullOrWhiteSpace(req.Affiliation))
        {
            throw new ValidationException("Affiliation is required");
        }

        if (string.IsNullOrWhiteSpace(req.City))
        {
            throw new ValidationException("City is required");
        }

        if (string.IsNullOrWhiteSpace(req.Region) || !RegionRegex().IsMatch(req.Region.Trim()))
        {
            throw new ValidationException("Region must be a 2-letter ISO country code (e.g. IL, US, PL)");
        }
    }

    private static TeamResponse Map(Team t) =>
        new(t.Id, t.Number, t.Name, t.Affiliation, t.City, t.Region);
}
