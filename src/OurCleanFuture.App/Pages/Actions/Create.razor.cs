﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using OurCleanFuture.App.Extensions;
using OurCleanFuture.App.Services;
using OurCleanFuture.Data;
using OurCleanFuture.Data.Entities;
using Action = OurCleanFuture.Data.Entities.Action;

namespace OurCleanFuture.App.Pages.Actions;

[Authorize(Roles = "Administrator, 1")]
public partial class Create : IDisposable
{
    private readonly Func<DirectorsCommittee, string> _committeeConverter = d => d.Name;
    private readonly Func<IndigenousGroup, string> _territoryConverter = d => d.AbbreviatedName;
    private AppDbContext _context = null!;
    private bool _isLoaded;
    private ClaimsPrincipal _user = null!;

    private IEnumerable<Lead> SelectedLeads { get; set; } = new List<Lead>();

    private List<Objective> Objectives { get; set; } = new();
    private List<Lead> Leads { get; set; } = new();
    private Action Action { get; } = new();

    private IEnumerable<DirectorsCommittee> SelectedDirectorsCommittees { get; set; } =
        new List<DirectorsCommittee>();

    private List<DirectorsCommittee> DirectorsCommittees { get; set; } = new();

    private IEnumerable<IndigenousGroup> SelectedTerritories { get; set; } =
        new List<IndigenousGroup>();
    private List<IndigenousGroup> UndertakenInTheTraditionalTerritoriesOf { get; set; } = new();

    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;

    [Inject]
    private IDbContextFactory<AppDbContext> ContextFactory { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private StateContainerService StateContainer { get; init; } = null!;

    public void Dispose() => _context.Dispose();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _context = await ContextFactory.CreateDbContextAsync();
            Leads = await _context.Leads
                .Include(l => l.Organization)
                .Include(l => l.Branch)
                .ThenInclude(b => b!.Department)
                .OrderBy(l => l.Branch!.Department.ShortName)
                .ThenBy(l => l.Branch!.Name)
                .ToListAsync();
            Objectives = await _context.Objectives
                .Include(o => o.Area)
                .OrderBy(o => o.Area.Title)
                .ThenBy(o => o.Title)
                .ToListAsync();
            DirectorsCommittees = await _context.DirectorsCommittees
                .OrderBy(dc => dc.Name)
                .ToListAsync();
            UndertakenInTheTraditionalTerritoriesOf = await _context.IndigenousGroups
                .OrderBy(ig => ig.FullName)
                .ToListAsync();
            await GetUserPrincipal();
        }
        catch (Exception ex)
        {
            Log.Error("{Exception}", ex);
            throw;
        }
        finally
        {
            _isLoaded = true;
        }

        await base.OnInitializedAsync();
    }

    private async Task GetUserPrincipal()
    {
        var authState = await AuthenticationStateTask;
        _user = authState.User;
    }

    private async Task Update()
    {
        if (
            Action.TargetCompletionDate < Action.ActualCompletionDate
            && Action.InternalStatus == InternalStatus.InProgress
        )
        {
            Snackbar.Add(
                "The <b>Internal Status</b> cannot be set to <b>In progress</b>, as the <b>Actual/Anticipated Completion Date</b> occurs after the <b>Target Completion Date</b>."
                    + " Either revise the <b>Actual/Anticipated Completion Date</b>, or change the <b>Internal Status</b> to <b>Delayed</b>.",
                Severity.Error
            );
            return;
        }

        if (!SelectedTerritories.Any())
        {
            Snackbar.Add("Must contain at least one traditional territory.", Severity.Error);
            return;
        }

        Action.InternalStatusUpdatedBy = _user.GetFormattedName();
        Action.InternalStatusUpdatedDate = DateTimeOffset.Now;

        Action.ExternalStatusUpdatedBy = _user.GetFormattedName();
        Action.ExternalStatusUpdatedDate = DateTimeOffset.Now;

        Action.DirectorsCommittees.Clear();
        Action.DirectorsCommittees.AddRange(SelectedDirectorsCommittees);

        Action.UndertakenInTheTraditionalTerritoriesOf.Clear();
        Action.UndertakenInTheTraditionalTerritoriesOf.AddRange(SelectedTerritories);

        Action.Leads.Clear();
        Action.Leads.AddRange(SelectedLeads);

        _context.Add(Action);
        try
        {
            await _context.SaveChangesAsync();
            Snackbar.Add($"Successfully created action: {Action.Number}", Severity.Success);
            Log.Information(
                "{User} created action {ActionId}: {ActionTitle}",
                StateContainer.ClaimsPrincipalEmail,
                Action.Id,
                Action.Title
            );
            Navigation.NavigateTo($"/actions/details/{Action.Id}");
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is SqlException sqlException && sqlException.Number == 2601)
            {
                Snackbar.Add(
                    $"An action with number {Action.Number} already exists",
                    Severity.Error
                );
            }
        }
    }
}
