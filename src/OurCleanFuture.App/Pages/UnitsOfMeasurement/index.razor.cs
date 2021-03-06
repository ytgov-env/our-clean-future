using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using OurCleanFuture.Data;
using OurCleanFuture.Data.Entities;

namespace OurCleanFuture.App.Pages.UnitsOfMeasurement;

[Authorize(Roles = "Administrator, 1")]
public partial class Index : IDisposable
{
    private bool _isLoaded;
    private AppDbContext _context = null!;

    private List<UnitOfMeasurement> UnitsOfMeasurement { get; set; } = new();

    [Inject]
    private IDbContextFactory<AppDbContext> ContextFactory { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private StateContainer StateContainer { get; init; } = null!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _context = ContextFactory.CreateDbContext();
            UnitsOfMeasurement = await _context.UnitsOfMeasurement.OrderBy(u => u.Symbol).ToListAsync();
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

    private async Task Create()
    {
        var dialog = DialogService.Show<CreateDialog>("Create");
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            var newUnitOfMeasurement = (UnitOfMeasurement)result.Data;
            try
            {
                _context.Add(newUnitOfMeasurement);
                var entriesSaved = await _context.SaveChangesAsync();
                if (entriesSaved == 1)
                {
                    UnitsOfMeasurement.Add(newUnitOfMeasurement);
                    Log.Information("{User} created unit of measurement: {UnitOfMeasurementId}, {UnitOfMeasurementName}, {UnitOfMeasurementSymbol}",
                                    StateContainer.ClaimsPrincipalEmail,
                                    newUnitOfMeasurement.Id,
                                    newUnitOfMeasurement.Name,
                                    newUnitOfMeasurement.Symbol);
                    Snackbar.Add($"Created unit {newUnitOfMeasurement.Symbol}", Severity.Success);
                }
            }
            catch (DbUpdateException)
            {
                Snackbar.Add($"Unable to add new unit {newUnitOfMeasurement.Symbol}. Does it already exist?", Severity.Error);
            }
        }
    }

    private async Task Edit(UnitOfMeasurement unitOfMeasurement)
    {
        var parameters = new DialogParameters { ["UnitOfMeasurement"] = unitOfMeasurement };

        var dialog = DialogService.Show<EditDialog>("Edit", parameters);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            var updatedUnitOfMeasurement = (UnitOfMeasurement)result.Data;
            try
            {
                var entriesSaved = await _context.SaveChangesAsync();
                if (entriesSaved == 1)
                {
                    Snackbar.Add($"Updated unit {updatedUnitOfMeasurement.Symbol}", Severity.Success);
                    Log.Information("{User} updated unit of measurement: {UnitOfMeasurementId}, {UnitOfMeasurementName}, {UnitOfMeasurementSymbol}",
                                    StateContainer.ClaimsPrincipalEmail,
                                    updatedUnitOfMeasurement.Id,
                                    updatedUnitOfMeasurement.Name,
                                    updatedUnitOfMeasurement.Symbol);
                }
            }
            catch (DbUpdateException)
            {
                Snackbar.Add($"Unable to edit unit {unitOfMeasurement.Symbol}", Severity.Error);
            }
        }
    }

    private async Task Delete(UnitOfMeasurement unitOfMeasurement)
    {
        var result = await DialogService.ShowMessageBox(
            $"Delete {unitOfMeasurement.Symbol}?",
            "This action cannot not be undone.",
            yesText: "Delete", cancelText: "Cancel");
        if (result == true)
        {
            try
            {
                _context.Remove(unitOfMeasurement);
                await _context.SaveChangesAsync();
                UnitsOfMeasurement.Remove(unitOfMeasurement);
                Snackbar.Add($"Deleted unit {unitOfMeasurement.Symbol}", Severity.Success);
                Log.Information("{User} deleted unit of measurement: {UnitOfMeasurementId}, {UnitOfMeasurementName}, {UnitOfMeasurementSymbol}",
                                StateContainer.ClaimsPrincipalEmail,
                                unitOfMeasurement.Id,
                                unitOfMeasurement.Name,
                                unitOfMeasurement.Symbol);
            }
            catch (DbUpdateException)
            {
                Snackbar.Add($"Unable to delete unit {unitOfMeasurement.Symbol}, as it is associated with an indicator", Severity.Error);
            }
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
