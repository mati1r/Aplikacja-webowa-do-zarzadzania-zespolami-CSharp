using Aplikacja_webowa_do_zarz¹dzania_zespo³ami.Models;
using Aplikacja_webowa_do_zarz¹dzania_zespo³ami.Repository;
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

builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddMvc().AddRazorPagesOptions(option =>
{
    option.Conventions.AddPageRoute("/OwnerTasks", "/Zarzadzanie zadaniami");
    option.Conventions.AddPageRoute("/UserTasks", "/Zadania");
    option.Conventions.AddPageRoute("/Groups", "/Aktywna grupa");
    option.Conventions.AddPageRoute("/JoinGroup", "/Grupy");
    option.Conventions.AddPageRoute("/Login", "/Logowanie");
    option.Conventions.AddPageRoute("/Register", "/Rejestracja");

    //W PRZYSZ£OŒCI UZUPE£NIÆ RESZTE PAMIÊTAÆ ZE REDIRECTY MUSZ¥ BYC NA NOWE NAZWY
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
