using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();


string connectionString = builder.Configuration.GetConnectionString(("DefaultConnection"));

builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)), ServiceLifetime.Scoped);

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddMvc().AddRazorPagesOptions(option =>
{
    option.Conventions.AddPageRoute("/OwnerTasks", "/Zarzadzanie zadaniami");
    option.Conventions.AddPageRoute("/UserTasks", "/Zadania");
    option.Conventions.AddPageRoute("/Groups", "/Aktywna grupa");
    option.Conventions.AddPageRoute("/JoinGroup", "/Twoje grupy");
    option.Conventions.AddPageRoute("/ActiveGroup", "/Aktywna grupa");
    option.Conventions.AddPageRoute("/EditGroup", "/Zarzadzanie grupa");
    option.Conventions.AddPageRoute("/Login", "/Logowanie");
    option.Conventions.AddPageRoute("/Register", "/Rejestracja");
    option.Conventions.AddPageRoute("/UserProfile", "/Profil uzytkownika");
    option.Conventions.AddPageRoute("/UserNotices", "/Ogłoszenia");
    option.Conventions.AddPageRoute("/OwnerNotices", "/Zarzadzanie ogłoszeniami");
    option.Conventions.AddPageRoute("/PasswordRecovery", "/Odzyskiwanie dostepu");
    option.Conventions.AddPageRoute("/Messages", "/Wiadomosci");
    option.Conventions.AddPageRoute("/Calendar", "/Kalendarz");
});

var app = builder.Build();

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

app.UseAuthorization();

app.UseSession();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
});

app.MapRazorPages();

app.Run();
