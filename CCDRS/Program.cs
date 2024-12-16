/*
    Copyright 2022 University of Toronto
    This file is part of CCDRS.
    CCDRS is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    CCDRS is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with CCDRS.  If not, see <http://www.gnu.org/licenses/>.
*/

using CCDRS;
using CCDRS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
// Add our database contexts to PostgreSql
builder.Services.AddDbContext<CCDRSContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TestDBContext") ?? throw new InvalidOperationException("Connection string 'CCDRSContext' not found. Are you connecting to the correct Postgresql Database ?.")));

// Mongodb connection and add services to the container.
builder.Services.Configure<MongoDBUserContext>(
    builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoDBUserService>();

//Authentication using cookies 
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.AccessDeniedPath = "/Forbidden/";
    }
);

// Block pages and ensure all pages require authentication.
builder.Services.AddRazorPages(options =>
    {
        options.Conventions.AuthorizePage("/Index");
        options.Conventions.AuthorizePage("/AllStation");
        options.Conventions.AuthorizePage("/AllScreenline");
        options.Conventions.AuthorizePage("/SpecificStation");
        options.Conventions.AuthorizePage("/SpecificScreenline");
    }
);

// configure sessions
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

var app = builder.Build();

// Create and acquire the CCDRSContext service on project startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CCDRS.Data.CCDRSContext>();
    Utility.Initialize(dbContext);
};

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
// Activate the login authentication system
app.UseAuthentication();
app.UseAuthorization();

//enable session
app.UseSession();

app.MapRazorPages();
app.MapDefaultControllerRoute();
app.Run();
