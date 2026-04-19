using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WPFDatabase.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var projectDirectory = File.Exists(Path.Combine(currentDirectory, "WPFDatabase.csproj"))
            ? currentDirectory
            : Path.Combine(currentDirectory, "WPFDatabase");

        var dbPath = Path.Combine(projectDirectory, "bin", "Debug", "net9.0-windows", "wpfdatabase.db");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new AppDbContext(optionsBuilder.Options);
    }
}
