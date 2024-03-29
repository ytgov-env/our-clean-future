using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using OurCleanFuture.App.Services;
using OurCleanFuture.Data;
using OurCleanFuture.Data.Entities;
using Action = OurCleanFuture.Data.Entities.Action;

namespace OurCleanFuture.App.Pages.Actions;

public partial class Details : IDisposable
{
    private AppDbContext _context = null!;
    private bool _isLoaded;

    [Parameter]
    public int Id { get; set; }

    private Action? Action { get; set; }

    private List<IndigenousGroup> Territories { get; set; } = new();

    [Inject]
    private IDbContextFactory<AppDbContext> ContextFactory { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private StateContainerService StateContainer { get; init; } = null!;

    public void Dispose() => _context.Dispose();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _context = await ContextFactory.CreateDbContextAsync();
            Action = await _context.Actions
                .Include(a => a.Indicators)
                .Include(a => a.DirectorsCommittees)
                .Include(a => a.UndertakenInTheTraditionalTerritoriesOf)
                .Include(i => i.Leads)
                .ThenInclude(l => l.Branch)
                .ThenInclude(b => b!.Department)
                .Include(i => i.Leads)
                .ThenInclude(l => l.Organization)
                .Include(a => a.Objective)
                .ThenInclude(a => a.Area)
                .Include(a => a.Objective)
                .ThenInclude(a => a.Goals)
                .AsSingleQuery()
                .FirstOrDefaultAsync(a => a.Id == Id);
            Territories = await _context.IndigenousGroups.ToListAsync();
            if (Action is not null)
            {
                Log.Information(
                    "{User} is viewing action {ActionId}: {ActionTitle}",
                    StateContainer.ClaimsPrincipalEmail,
                    Action.Id,
                    Action.Title
                );
            }
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

    private string InternalStatusToString()
    {
        // Only append updated by information if the InternalStatus has been updated after database creation
        return !string.IsNullOrWhiteSpace(Action!.InternalStatusUpdatedBy)
            ? $"Last updated by {Action.InternalStatusUpdatedBy} on {Action.InternalStatusUpdatedDate?.LocalDateTime:f}"
            : string.Empty;
    }

    private string ExternalStatusToString()
    {
        // Only append updated by information if the ExternalStatus has been updated after database creation
        if (!string.IsNullOrWhiteSpace(Action!.ExternalStatusUpdatedBy))
        {
            return $"Last updated by {Action.ExternalStatusUpdatedBy} on {Action.ExternalStatusUpdatedDate?.LocalDateTime:f}";
        }

        return string.Empty;
    }

    private void Edit() => Navigation.NavigateTo("/actions/edit/" + Action!.Id);

    private void ViewIndicator(Indicator indicator) =>
        Navigation.NavigateTo("/indicators/details/" + indicator.Id);

    private void ViewArea(Area area) =>
        Navigation.NavigateTo("/areas/" + area.Title.ToLower().Replace(' ', '-'));
}
