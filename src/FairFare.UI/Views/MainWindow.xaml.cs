using System.Windows;
using FairFare.UI.ViewModels;

namespace FairFare.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}