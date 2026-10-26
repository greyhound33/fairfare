using System.Windows;
using FairFare.BLL.Interfaces;
using FairFare.BLL.Services;
using FairFare.DAL;
using FairFare.DAL.Repositories;
using FairFare.Domain.Interfaces;
using FairFare.UI.ViewModels;
using FairFare.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace FairFare.UI;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        ServiceProvider = serviceCollection.BuildServiceProvider();
        
        try
        {
            using var scope = ServiceProvider.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Не вдалося підключитися до бази даних. Запустіть docker compose up -d і перевірте FAIRFARE_CONNECTION.\n\n" + ex.Message,
                "FairFare", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // DAL
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(FairFareDb.ConnectionString));
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<TripStorageService>();

        // BLL
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IFinancialService, FinancialService>();
        services.AddScoped<IRouteService, RouteService>();
        services.AddScoped<IReportService, ReportService>();

        // UI ViewModels & Views
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();
    }
}