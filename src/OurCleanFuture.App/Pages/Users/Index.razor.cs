using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using OurCleanFuture.App;
using OurCleanFuture.App.Shared;
using MudBlazor;
using Microsoft.EntityFrameworkCore;
using OurCleanFuture.Data;
using OurCleanFuture.Data.Entities;

namespace OurCleanFuture.App.Pages.Users
{
    public partial class Index
    {
        private bool isLoaded;
        private AppDbContext _context = null!;
        public List<User> Users { get; set; } = new();
        public List<Role> Roles { get; set; } = new();
        [Inject]
        private IDbContextFactory<AppDbContext> ContextFactory { get; set; } = null!;

        [Inject]
        private NavigationManager Navigation { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            try {
                _context = ContextFactory.CreateDbContext();
                Users = await _context.Users.Include(u => u.Roles).ToListAsync();
                Roles = await _context.Roles.ToListAsync();
            }
            finally {
                isLoaded = true;
            }
        }
    }
}