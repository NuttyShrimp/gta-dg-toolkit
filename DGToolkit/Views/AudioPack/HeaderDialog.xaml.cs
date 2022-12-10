using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using DGToolkit.Models.AudioPack.Dat54;
using DGToolkit.Models.Util;

namespace DGToolkit.Views.AudioPack;

public partial class HeaderDialog : Window
{
    public BaseSoundHeader? returnHeaders = null;
    public HeaderDialog(BaseSoundHeader header)
    {
        InitializeComponent();
        DataContext = header;
    }

    private void NumberInputText(object sender, TextCompositionEventArgs e)
    {
        Util.NumberInputText(sender, e);
    }

    private void Nax1NumberInput(object sender, TextCompositionEventArgs e)
    {
        Util.NumberInputText(sender, e);
        if (!e.Handled)
        {
            return;
        }

        float f = float.Parse(e.Text);
        e.Handled = f <= 1.0 && f >= 0.0;
    }

    private void CancelInput(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        this.Close();
    }

    private void OkInput(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        returnHeaders = (BaseSoundHeader)DataContext;
        this.Close();
    }
}