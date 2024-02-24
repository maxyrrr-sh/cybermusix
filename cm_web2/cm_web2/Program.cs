using cm;
using System.Data.SQLite;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
const string connectionString = "Data Source=data.db;Version=3;";
SessionManager.sessionToken = null;
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (SQLiteConnection connection = new SQLiteConnection(connectionString))
{
    connection.Open();
    string createTablesQuery = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY,
                    Username TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Songs (
                    Id INTEGER PRIMARY KEY,
                    Artist TEXT,
                    Name TEXT,
                    FileId INTEGER
                );
            ";

    SQLiteCommand createTablesCommand = new SQLiteCommand(createTablesQuery, connection);
    createTablesCommand.ExecuteNonQuery();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
