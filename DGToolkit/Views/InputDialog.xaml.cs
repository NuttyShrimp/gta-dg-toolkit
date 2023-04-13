using System.Windows;

namespace DGToolkit.Views;

public partial class InputDialog : Window
{
    public InputDialog(string title)
    {
        Title.Header = title;
        InitializeComponent();
    }
    
    public string InputText
    {
        get => Input.Text;
    }

    private void OkOnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancleOnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}