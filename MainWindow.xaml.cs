using System.Windows;
using NexusFinance.ViewModels;

namespace NexusFinance;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
