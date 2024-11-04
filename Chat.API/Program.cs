using Chat.API.Controllers;
using Chat.API.Filters;
using Chat.API.Filters.SignalR;
using Chat.API.Hubs;
using Chat.API.Interfaces;
using Chat.API.Services;
using Chat.BLL;
using Core.Extensions;
using Core.Filters;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
    });

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<StatusCodeExceptionFilter>();
    options.Filters.Add<AddUserDataFilter>();
});
builder.Services.AddSignalR(options =>
{
    options.AddFilter<AddUserDataHubFilter>();
});

builder.Services.AddTransient<IRazorRenderer, RazorRenderer>();

builder.Services.AddBLL(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<MainHub>("/sockets", options =>
{
    options.CloseOnAuthenticationExpiration = true;
    options.AllowStatefulReconnects = true;
});

app.Run();
