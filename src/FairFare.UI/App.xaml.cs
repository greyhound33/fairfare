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

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // DAL
        services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=fairfare.db"));
        services.AddScoped<IEventRepository, EventRepository>();

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