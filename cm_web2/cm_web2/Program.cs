using cm;
using System.Data.SQLite;
using Microsoft.AspNetCore.Http;
using cm_web2.Controllers;

var builder = WebApplication.CreateBuilder(args);
const string connectionString = "Data Source=data.db;Version=3;";

builder.Services.AddControllersWithViews();
var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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
                    Name TEXT,
                    FileData BLOB
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
