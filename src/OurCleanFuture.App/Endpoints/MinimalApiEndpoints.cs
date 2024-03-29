﻿using Microsoft.EntityFrameworkCore;
using OurCleanFuture.Data;
using OurCleanFuture.Data.Entities;

// ReSharper disable NotAccessedPositionalProperty.Local

namespace OurCleanFuture.App.Endpoints;

#pragma warning disable RCS1077 // Optimize LINQ method call.
public static class MinimalApiEndpoints
{
    public static void MapIndicatorEndpoints(this WebApplication app)
    {
        app.MapGet("api/v1/indicators", GetIndicators)
            .WithTags("Indicators")
            .WithName("GetIndicators")
            .Produces<List<IndicatorDto>>();

        app.MapGet("api/v1/indicators/{id:int}", GetIndicatorById)
            .WithTags("Indicators")
            .WithName("GetIndicatorById")
            .Produces<IndicatorDto>()
            .Produces(404);
    }

    private static async Task<IResult> GetIndicators(AppDbContext context)
    {
        var indicators = await context.Indicators
            .Select(
                i =>
                    new IndicatorDto(
                        i.Id,
                        i.Title.Trim(),
                        i.Description.Trim(),
                        i.CollectionInterval,
                        i.DataType,
                        i.UnitOfMeasurement.Name,
                        i.UnitOfMeasurement.Symbol,
                        i.Leads
                            .Select(
                                l =>
                                    new LeadDto(
                                        l.Id,
                                        l.Organization.Name,
                                        l.Branch!.Department.Name,
                                        l.Branch.Name
                                    )
                            )
                            .ToList(),
                        i.Action != null
                            ? ParentType.Action
                            : i.Objective != null
                                ? ParentType.Objective
                                : ParentType.Goal,
                        i.Action != null
                            ? i.Action.Objective.Goals
                                .Select(g => new GoalDto(g.Id, g.Title))
                                .ToList()
                            : i.Objective != null
                                ? i.Objective.Goals.Select(g => new GoalDto(g.Id, g.Title)).ToList()
                                : new List<GoalDto> { new(i.Id, i.Title) },
                        i.Action == null ? i.Objective!.Area.Id : i.Action.Objective.Area.Id,
                        i.Action == null ? i.Objective!.Area.Title : i.Action.Objective.Area.Title,
                        i.Objective == null ? i.Action!.Objective.Id : i.Objective.Id,
                        i.Objective == null ? i.Action!.Objective.Title : i.Objective.Title,
                        i.Action == null ? null : i.Action.Id,
                        i.Action == null ? default : i.Action.Number,
                        i.Action == null ? default : i.Action.Title,
                        i.Entries
                            .Select(
                                e =>
                                    new EntryDto(
                                        e.StartDate,
                                        e.EndDate,
                                        e.Value,
                                        e.Note,
                                        e.UpdatedBy
                                    )
                            )
                            .ToList(),
                        i.Target == null ? default : i.Target.Description,
                        i.Target == null ? default : i.Target.Value,
                        i.Target == null ? default : i.Target.CompletionDate,
                        i.Note
                    )
            )
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();

        return Results.Ok(indicators);
    }

    private static async Task<IResult> GetIndicatorById(AppDbContext context, int id)
    {
        var indicator = await context.Indicators
            .Where(i => i.Id == id)
            .Select(
                i =>
                    new IndicatorDto(
                        i.Id,
                        i.Title.Trim(),
                        i.Description.Trim(),
                        i.CollectionInterval,
                        i.DataType,
                        i.UnitOfMeasurement.Name,
                        i.UnitOfMeasurement.Symbol,
                        i.Leads
                            .Select(
                                l =>
                                    new LeadDto(
                                        l.Id,
                                        l.Organization.Name,
                                        l.Branch!.Department.Name,
                                        l.Branch.Name
                                    )
                            )
                            .ToList(),
                        i.Action != null
                            ? ParentType.Action
                            : i.Objective != null
                                ? ParentType.Objective
                                : ParentType.Goal,
                        i.Action != null
                            ? i.Action.Objective.Goals
                                .Select(g => new GoalDto(g.Id, g.Title))
                                .ToList()
                            : i.Objective != null
                                ? i.Objective.Goals.Select(g => new GoalDto(g.Id, g.Title)).ToList()
                                : new List<GoalDto> { new(i.Id, i.Title) },
                        i.Action == null ? i.Objective!.Area.Id : i.Action.Objective.Area.Id,
                        i.Action == null ? i.Objective!.Area.Title : i.Action.Objective.Area.Title,
                        i.Objective == null ? i.Action!.Objective.Id : i.Objective.Id,
                        i.Objective == null ? i.Action!.Objective.Title : i.Objective.Title,
                        i.Action == null ? null : i.Action.Id,
                        i.Action == null ? default : i.Action.Number,
                        i.Action == null ? default : i.Action.Title,
                        i.Entries
                            .Select(
                                e =>
                                    new EntryDto(
                                        e.StartDate,
                                        e.EndDate,
                                        e.Value,
                                        e.Note,
                                        e.UpdatedBy
                                    )
                            )
                            .ToList(),
                        i.Target == null ? default : i.Target.Description,
                        i.Target == null ? default : i.Target.Value,
                        i.Target == null ? default : i.Target.CompletionDate,
                        i.Note
                    )
            )
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync();

        return indicator is not null ? Results.Ok(indicator) : Results.NotFound();
    }

    public static void MapActionEndpoints(this WebApplication app)
    {
        app.MapGet("api/v1/actions", GetActions)
            .WithTags("Actions")
            .WithName("GetActions")
            .Produces<List<ActionDto>>();

        app.MapGet("api/v1/actions/{id:int}", GetActionById)
            .WithTags("Actions")
            .WithName("GetActionById")
            .Produces<ActionDto>()
            .Produces(404);
    }

    private static async Task<IResult> GetActions(AppDbContext context)
    {
        var actions = await context.Actions
            .Select(
                a =>
                    new ActionDto(
                        a.Id,
                        a.Number,
                        a.Title.Trim(),
                        a.Leads
                            .Select(
                                l =>
                                    new LeadDto(
                                        l.Id,
                                        l.Organization.Name,
                                        l.Branch!.Department.Name,
                                        l.Branch.Name
                                    )
                            )
                            .ToList(),
                        a.DirectorsCommittees
                            .Select(d => new DirectorsCommitteeDto(d.Id, d.Name))
                            .ToList(),
                        a.UndertakenInTheTraditionalTerritoriesOf
                            .Select(
                                i => new IndigenousGroupDto(i.Id, i.FullName, i.AbbreviatedName)
                            )
                            .ToList(),
                        a.EngagementsAndPartnershipActivities.Trim(),
                        a.PublicExplanation.Trim(),
                        a.Note.Trim(),
                        a.InternalStatus,
                        a.InternalStatusUpdatedDate,
                        a.ExternalStatus,
                        a.ExternalStatusUpdatedDate,
                        a.StartDate,
                        a.TargetCompletionDate,
                        a.ActualCompletionDate,
                        a.Indicators.Select(i => new IndicatorHeaderDto(i.Id, i.Title)).ToList()
                    )
            )
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();

        return Results.Ok(actions);
    }

    private static async Task<IResult> GetActionById(AppDbContext context, int id)
    {
        var action = await context.Actions
            .Where(a => a.Id == id)
            .Select(
                a =>
                    new ActionDto(
                        a.Id,
                        a.Number,
                        a.Title.Trim(),
                        a.Leads
                            .Select(
                                l =>
                                    new LeadDto(
                                        l.Id,
                                        l.Organization.Name,
                                        l.Branch!.Department.Name,
                                        l.Branch.Name
                                    )
                            )
                            .ToList(),
                        a.DirectorsCommittees
                            .Select(d => new DirectorsCommitteeDto(d.Id, d.Name))
                            .ToList(),
                        a.UndertakenInTheTraditionalTerritoriesOf
                            .Select(
                                i => new IndigenousGroupDto(i.Id, i.FullName, i.AbbreviatedName)
                            )
                            .ToList(),
                        a.EngagementsAndPartnershipActivities.Trim(),
                        a.PublicExplanation.Trim(),
                        a.Note.Trim(),
                        a.InternalStatus,
                        a.InternalStatusUpdatedDate,
                        a.ExternalStatus,
                        a.ExternalStatusUpdatedDate,
                        a.StartDate,
                        a.TargetCompletionDate,
                        a.ActualCompletionDate,
                        a.Indicators.Select(i => new IndicatorHeaderDto(i.Id, i.Title)).ToList()
                    )
            )
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync();

        return action is not null ? Results.Ok(action) : Results.NotFound();
    }

    public static void MapAreaEndpoints(this WebApplication app)
    {
        app.MapGet("api/v1/areas", GetAreas)
            .WithTags("Areas")
            .WithName("GetAreas")
            .Produces<List<AreaDto>>();
        app.MapGet("api/v1/areas/{id:int}", GetAreaById)
            .WithTags("Areas")
            .WithName("GetAreaById")
            .Produces<AreaDto>()
            .Produces(404);
    }

    private static async Task<IResult> GetAreas(AppDbContext context)
    {
        var areas = await context.Areas
            .Select(
                a =>
                    new AreaDto(
                        a.Id,
                        a.Title,
                        a.Objectives
                            .Select(
                                o =>
                                    new ObjectiveDto(
                                        o.Id,
                                        o.Title,
                                        o.Goals.Select(g => new GoalDto(g.Id, g.Title)).ToList()
                                    )
                            )
                            .ToList()
                    )
            )
            .AsNoTracking()
            .AsSingleQuery()
            .ToListAsync();

        return Results.Ok(areas);
    }

    private static async Task<IResult> GetAreaById(AppDbContext context, int id)
    {
        var area = await context.Areas
            .Where(a => a.Id == id)
            .Select(
                a =>
                    new AreaDto(
                        a.Id,
                        a.Title,
                        a.Objectives
                            .Select(
                                o =>
                                    new ObjectiveDto(
                                        o.Id,
                                        o.Title,
                                        o.Goals.Select(g => new GoalDto(g.Id, g.Title)).ToList()
                                    )
                            )
                            .ToList()
                    )
            )
            .AsNoTracking()
            .AsSingleQuery()
            .FirstOrDefaultAsync();

        return area is not null ? Results.Ok(area) : Results.NotFound();
    }

    private record AreaDto(int Id, string Title, List<ObjectiveDto> Objectives);

    private record ObjectiveDto(int Id, string Title, List<GoalDto> Goals);

    private record IndicatorHeaderDto(int Id, string Title);

    private record IndicatorDto(
        int Id,
        string Title,
        string Description,
        CollectionInterval CollectionInterval,
        DataType DataType,
        string UnitOfMeasurementName,
        string UnitOfMeasurementSymbol,
        List<LeadDto> Leads,
        ParentType ParentType,
        List<GoalDto> Goals,
        int? AreaId,
        string? AreaTitle,
        int? ObjectiveId,
        string? ObjectiveTitle,
        int? ActionId,
        string? ActionNumber,
        string? ActionTitle,
        List<EntryDto> Entries,
        string? TargetDescription,
        double? TargetValue,
        DateTime? TargetCompletionDate,
        string? Note
    );

    private record EntryDto(
        DateTime StartDate,
        DateTime EndDate,
        decimal Value,
        string Note,
        string LastUpdatedBy
    );

    private record GoalDto(int Id, string Title);

    private record ActionDto(
        int Id,
        string Number,
        string Title,
        List<LeadDto> Leads,
        List<DirectorsCommitteeDto> DirectorsCommittees,
        List<IndigenousGroupDto> UndertakenInTheTraditionalTerritoriesOf,
        string EngagementsAndPartnershipActivities,
        string PublicExplanation,
        string Note,
        InternalStatus InternalStatus,
        DateTimeOffset? InternalStatusLastUpdatedDateTime,
        ExternalStatus ExternalStatus,
        DateTimeOffset? ExternalStatusLastUpdatedDateTime,
        DateTime? StartDate,
        DateTime? TargetCompletionDate,
        DateTime? ActualOrAnticipatedCompletionDate,
        List<IndicatorHeaderDto> Indicators
    );

    private record LeadDto(int Id, string Organization, string? Department, string? Branch);

    private record DirectorsCommitteeDto(int Id, string Name);

    private record IndigenousGroupDto(int Id, string FullName, string AbbreviatedName);

    private enum ParentType
    {
        Action,
        Goal,
        Objective
    }
}
#pragma warning restore RCS1077 // Optimize LINQ method call.
