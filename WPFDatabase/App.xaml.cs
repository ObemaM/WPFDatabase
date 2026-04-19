using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using WPFDatabase.Data;
using WPFDatabase.Models;
using WPFDatabase.Views;

namespace WPFDatabase;

public partial class App : Application
{
    public static AppDbContext DbContext { get; private set; } = null!;
    public static User? CurrentUser { get; set; }

    // При старте создаем контекст БД, применяем миграции и сначала показываем окно входа
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var dbPath = Path.Combine(AppContext.BaseDirectory, "wpfdatabase.db");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        DbContext = new AppDbContext(options);
        // Если БД новая, сюда же подтянутся начальные данные для демонстрации дерева
        DataSeeder.Seed(DbContext);

        var loginWindow = new LoginWindow();
        var loginResult = loginWindow.ShowDialog();

        if (loginResult == true && CurrentUser is not null)
        {
            // Главное окно открываем только после успешного входа
            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            mainWindow.Show();
            return;
        }

        Shutdown();
    }

    // При закрытии приложения освобождаем контекст и очищаем текущего пользователя
    protected override void OnExit(ExitEventArgs e)
    {
        CurrentUser = null;
        DbContext.Dispose();
        base.OnExit(e);
    }
}
