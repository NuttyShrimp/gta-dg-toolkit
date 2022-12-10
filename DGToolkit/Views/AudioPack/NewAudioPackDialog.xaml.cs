using System.Windows;

namespace DGToolkit.Views.AudioPack;

public partial class NewAudioPackDialog : Window
{
    public string packName;
    public NewAudioPackDialog()
    {
        packName = "";
        InitializeComponent();
    }

    private void CancelInput(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OkInput(object sender, RoutedEventArgs e)
    {
        packName = AudioPackInput.Text;
        DialogResult = true;
        Close();
    }
}