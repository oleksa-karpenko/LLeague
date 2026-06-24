using LLeague.Api.Application.Abstractions;
using LLeague.Api.Domain;
using LLeague.Api.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LLeague.Api.Application.Services;

public class EventService(IAppDbContext db)
{
    public async Task<IEnumerable<EventResponse>> ListAsync(Guid? seasonId)
    {
        IQueryable<Event> q = db.Events.AsQueryable();
        if (seasonId is Guid sid)
        {
            q = q.Where(e => e.SeasonId == sid);
        }

        return await q.OrderBy(e => e.StartDate).Select(e => Map(e)).ToListAsync();
    }

    public async Task<EventResponse> GetAsync(Guid id)
    {
        Event e = await db.Events.FindAsync(id) ?? throw new NotFoundException();
        return Map(e);
    }

    public async Task<EventResponse> CreateAsync(EventRequest req)
    {
        RequireName(req);
        if (!await db.Seasons.AnyAsync(s => s.Id == req.SeasonId))
        {
            throw new ValidationException("Season not found");
        }

        RequireDateOrder(req);

        var e = new Event
        {
            Id = Guid.NewGuid(),
            SeasonId = req.SeasonId,
            Name = req.Name.Trim(),
            Slug = string.IsNullOrWhiteSpace(req.Slug) ? Slugify(req.Name) : req.Slug.Trim(),
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            Location = req.Location.Trim()
        };
        _ = db.Events.Add(e);
        _ = await db.SaveChangesAsync();
        return Map(e);
    }

    public async Task<EventResponse> UpdateAsync(Guid id, EventRequest req)
    {
        Event e = await db.Events.FindAsync(id) ?? throw new NotFoundException();
        RequireName(req);
        RequireDateOrder(req);
        e.Name = req.Name.Trim();
        e.Slug = string.IsNullOrWhiteSpace(req.Slug) ? Slugify(req.Name) : req.Slug.Trim();
        e.StartDate = req.StartDate;
        e.EndDate = req.EndDate;
        e.Location = req.Location.Trim();
        _ = await db.SaveChangesAsync();
        return Map(e);
    }

    public async Task DeleteAsync(Guid id)
    {
        Event e = await db.Events.FindAsync(id) ?? throw new NotFoundException();
        _ = db.Events.Remove(e);
        _ = await db.SaveChangesAsync();
    }

    private static void RequireName(EventRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
        {
            throw new ValidationException("Name is required");
        }
    }

    private static void RequireDateOrder(EventRequest req)
    {
        if (req.EndDate < req.StartDate)
        {
            throw new ValidationException("End date cannot be before start date");
        }
    }

    private static string Slugify(string s) =>
        new string([.. s.Trim().ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-')]).Trim('-');

    private static EventResponse Map(Event e) =>
        new(e.Id, e.SeasonId, e.Name, e.Slug, e.StartDate, e.EndDate, e.Location);
}
