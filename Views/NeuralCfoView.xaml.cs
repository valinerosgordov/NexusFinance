using System.Windows.Controls;
using NexusFinance.ViewModels;

namespace NexusFinance.Views;

public partial class NeuralCfoView : UserControl
{
    public NeuralCfoView()
    {
        InitializeComponent();
        DataContext = new NeuralCfoViewModel();
    }
}
