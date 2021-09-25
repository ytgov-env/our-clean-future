﻿using OurCleanFuture.Data;
using OurCleanFuture.Data.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurCleanFuture.App.Pages.UnitsOfMeasurement
{
    public partial class Index
    {
        private bool isLoaded;
        private bool mayRender = true;
        private AppDbContext context = null!;

        public List<UnitOfMeasurement> UnitsOfMeasurement { get; set; } = new();

        [Inject]
        public IDbContextFactory<AppDbContext> ContextFactory { get; set; } = null!;

        [Inject]
        public IDialogService DialogService { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            try {
                context = ContextFactory.CreateDbContext();
                UnitsOfMeasurement = await context.UnitsOfMeasurement.OrderBy(u => u.Symbol).ToListAsync();
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
            finally {
                isLoaded = true;
            }

            await base.OnInitializedAsync();
        }

        private async Task Create()
        {
            var dialog = DialogService.Show<CreateDialog>("Create");
            var result = await dialog.Result;
            if (!result.Cancelled) {
                var newUnitOfMeasurement = (UnitOfMeasurement)result.Data;
                try {
                    context.Add(newUnitOfMeasurement);
                    var entriesSaved = await context.SaveChangesAsync();
                    if (entriesSaved == 1) {
                        UnitsOfMeasurement.Add(newUnitOfMeasurement);
                        Snackbar.Add($"Created unit {newUnitOfMeasurement.Symbol}", Severity.Success);
                    }
                }
                catch (DbUpdateException) {

                    Snackbar.Add($"Unable to add new unit {newUnitOfMeasurement.Symbol}. Does it already exist?", Severity.Error);
                }
            }
        }

        private async Task Edit(UnitOfMeasurement unitOfMeasurement)
        {
            var parameters = new DialogParameters { ["UnitOfMeasurement"] = unitOfMeasurement };

            var dialog = DialogService.Show<EditDialog>("Edit", parameters);
            var result = await dialog.Result;
            if (!result.Cancelled) {
                var updatedUnitOfMeasurement = (UnitOfMeasurement)result.Data;
                try {
                    var entriesSaved = await context.SaveChangesAsync();
                    if (entriesSaved == 1) {
                        Snackbar.Add($"Updated unit {updatedUnitOfMeasurement.Symbol}", Severity.Success);
                    }
                }
                catch (DbUpdateException) {

                    Snackbar.Add($"Unable to edit unit {unitOfMeasurement.Symbol}", Severity.Error);
                }
            }
        }

        private async Task Delete(UnitOfMeasurement unitOfMeasurement)
        {
            bool? result = await DialogService.ShowMessageBox(
                $"Delete {unitOfMeasurement.Symbol}?",
                "This action cannot not be undone.",
                yesText: "Delete", cancelText: "Cancel");
            if (result == true) {
                //Prevents mid-method rerendering of the component, which avoids overlapping threads
                mayRender = false;
                try {
                    context.Remove(unitOfMeasurement);
                    await context.SaveChangesAsync();
                    UnitsOfMeasurement.Remove(unitOfMeasurement);
                    Snackbar.Add($"Deleted unit {unitOfMeasurement.Symbol}", Severity.Success);
                }
                catch (DbUpdateException) {
                    Snackbar.Add($"Unable to delete unit {unitOfMeasurement.Symbol}, as it is associated with an indicator", Severity.Error);
                }
                finally {
                    mayRender = true;
                }
            }
        }
    }
}