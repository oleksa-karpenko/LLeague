namespace LLeague.Api.Application;

public record EventRequest(
    Guid SeasonId,
    string Name,
    string Slug,
    DateOnly StartDate,
    DateOnly EndDate,
    string Location
    );

public record EventResponse(
    Guid Id,
    Guid SeasonId,
    string Name,
    string Slug,
    DateOnly StartDate,
    DateOnly EndDate,
    string Location
    );
