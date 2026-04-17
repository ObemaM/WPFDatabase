using System.Windows;
using Microsoft.EntityFrameworkCore;
using WPFDatabase.Data;

namespace WPFDatabase;

public partial class App : Application
{
    public static AppDbContext DbContext { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=wpfdatabase.db")
            .Options;

        DbContext = new AppDbContext(options);
        
        // Загрузка данных из файла
        DataSeeder.Seed(DbContext);

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        DbContext.Dispose();
        base.OnExit(e);
    }
}