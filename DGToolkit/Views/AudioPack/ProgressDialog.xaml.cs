using System;
using System.ComponentModel;
using System.Windows;
using DGToolkit.Models;

namespace DGToolkit.Views.AudioPack;

public partial class ProgressDialog : Window
{
    private LogStore store;
    public ProgressDialog(LogStore store)
    {
        InitializeComponent();
        this.store = store;
        progressText.Text = store.log;
        store.LogChanged += updateLog;
    }

    private void updateLog(object sender, LogChangedEventArgs e)
    {
        progressText.Text = store.log;
    }

    public void Finish()
    {
        closeBtn.IsEnabled = true;
    }

    private void onCloseBtnClick(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}