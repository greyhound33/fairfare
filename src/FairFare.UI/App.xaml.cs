// FairFare.UI/App.xaml.cs
using System.Windows;
using FairFare.BLL.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace FairFare.UI;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();
        services.AddSingleton<FinancialModule>();
        ServiceProvider = services.BuildServiceProvider();
        base.OnStartup(e);
    }
}