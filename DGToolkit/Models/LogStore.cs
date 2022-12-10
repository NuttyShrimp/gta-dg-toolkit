using System;
using System.Diagnostics;

namespace DGToolkit.Models;

// TODO: make observable
public class LogStore
{
    public delegate void LogChangedHandler(object sender, LogChangedEventArgs e);

    public event LogChangedHandler LogChanged;
    public string log { get; private set; }

    public LogStore()
    {
        log = "";
    }

    public void AddLogEntry(string entry)
    {
        Debug.WriteLine(entry);
        log += entry + "\n";
        LogChanged(this, new LogChangedEventArgs(log));
    }

    public void Clear()
    {
        log = "";
        LogChanged(this, new LogChangedEventArgs(log));
    }
}

public class LogChangedEventArgs : EventArgs
{
    private object _log;

    public LogChangedEventArgs(object log)
    {
        _log = log;
    }

    public object Log
    {
        get { return _log; }
    }
}