using System.Windows.Controls;
using DGToolkit.Models.Clothing;

namespace DGToolkit.Views.Clothing;

public partial class ClothingView : Page
{
    private ClothingOptions options = new();
    public ClothingView()
    {
        InitializeComponent();
    }
}