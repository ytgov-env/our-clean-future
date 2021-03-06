using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using OurCleanFuture.Data;
using OurCleanFuture.Data.Entities;
using Action = OurCleanFuture.Data.Entities.Action;

namespace OurCleanFuture.App.Pages.Areas;

public partial class Details : IDisposable
{
    private bool _isLoaded;
    private AppDbContext _context = null!;

    private Area Area { get; set; } = null!;

    [Parameter]
    public string AreaTitle { get; set; } = null!;

    [Inject]
    private IDbContextFactory<AppDbContext> ContextFactory { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private StateContainer StateContainer { get; init; } = null!;

    // Required to force the app to re-render when navigating between Areas.
    // OnInitializedAsync is not called by default in this situation, as the user is merely changing the parameter, while staying on the same page.
    protected override async Task OnParametersSetAsync()
    {
        await OnInitializedAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _context = ContextFactory.CreateDbContext();
#pragma warning disable CS8601 // Possible null reference assignment.
            Area = await _context.Areas.Include(a => a.Objectives).ThenInclude(o => o.Actions).AsSingleQuery().AsNoTracking().FirstOrDefaultAsync(a => a.Title == AreaTitle.Replace('-', ' '));
#pragma warning restore CS8601 // Possible null reference assignment.
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
        Log.Information("{User} is viewing area {AreaId}: {AreaTitle}", StateContainer.ClaimsPrincipalEmail, Area?.Id, Area?.Title);

        await base.OnInitializedAsync();
    }

    private void ViewAction(Action action)
    {
        Navigation.NavigateTo("/actions/details/" + action.Id);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
