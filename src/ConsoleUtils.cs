using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using KSP;
using KSP.IO;
using Mono;
using Mono.CSharp;

public static class Dependancy
{
    public static Assembly Load(string filename)
    {
        string path = new System.IO.FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName + "\\" + filename;
        if (System.IO.File.Exists(path))
        {
            return Assembly.LoadFile(path);
        }
        else
        {
            Debug.LogError("CSharpConsole: Failed to load dependancy: '" + path + "'");
            return null;
        }
    }
}

[Serializable]
public class ConsoleExecBaseClass
{
    protected static string help = "The following are available:\nhelp // Displays that which you are currently reading.\nLog(message) // Log a message to this console and the (Alt+F2) KSP console, which also sends it to the log files.";
    protected static string quit = "Oh please, what do you expect me to do when you type 'quit'?";
    protected static string quti = "quti - Type 'quit' for more info.";
    protected static string hello_hal
    {
        get {
            if (HighLogic.SaveFolder != null && HighLogic.SaveFolder != "")
            {
                return "Hello, " + HighLogic.SaveFolder + ".";
            }
            else
            {
                return "Hello, Dave.";
            }
        }
    }
    protected static string hideconsole
    {
        get
        {
            if (CSharpConsoleHelper.IsVisible())
            {
                CSharpConsoleHelper.HideConsole();
                return "Console hidden.";
            }
            else
            {
                return "Console is already hidden!";
            }
        }
    }
    protected static string showconsole
    {
        get
        {
            if (CSharpConsoleHelper.IsVisible())
            {
                return "Console is already visible!";
            }
            else
            {
                CSharpConsoleHelper.ShowConsole();
                return "Console shown.";
            }
        }
    }
    protected static string toggleconsole
    {
        get
        {
            if (CSharpConsoleHelper.IsVisible())
            {
                CSharpConsoleHelper.HideConsole();
                return "Console hidden.";
            }
            else
            {
                CSharpConsoleHelper.ShowConsole();
                return "Console shown.";
            }
        }
    }
    protected static string showhistory
    {
        get
        {
            string history = "";
            CSharpConsoleHelper.GetHistory().ForEach(a => history += a + "\n");
            return history;
        }
    }
    protected static InvisibleValue clearhistory
    {
        get
        {
            CSharpConsoleHelper.ClearHistory();
            return new InvisibleValue();
        }
    }
    protected static InvisibleValue clear
    {
        get
        {
            CSharpConsoleHelper.Clear();
            return new InvisibleValue();
        }
    }

    public static InvisibleValue Log(string message)
    {
        Con.Log(message);
        return new InvisibleValue();
    }
}

[Serializable]
public class History
{
    private List<string> history;
    [NonSerialized]
    private int lastindex = -1;
    public History()
    {
        history = new List<string>();
    }
    public void Add(string item)
    {
        history.Add(item);
        lastindex = history.Count - 1;
    }
    public void Clear()
    {
        history.Clear();
        lastindex = -1;
    }
    public List<string> Get()
    {
        return history;
    }
    public string Get(int index)
    {
        return history[index];
    }
    public bool Exists(string item)
    {
        return history.Exists(i => i.Equals(item));
    }
    public int LastIndex()
    {
        return lastindex;
    }
    public bool IndexNext(ref string str) {
        if (lastindex == -1)
        {
            return false;
        }
        else if (lastindex == history.Count - 1) {
            str = Get(LastIndex());
            return true;
        }
        else if (lastindex < history.Count - 1)
        {
            lastindex++;
            str = Get(LastIndex());
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool IndexPrev(ref string str)
    {
        if (lastindex == -1) {
            return false;
        }
        else if (lastindex == 0)
        {
            str = Get(LastIndex());
            return true;
        }
        else if (lastindex >= 0)
        {
            str = Get(LastIndex());
            lastindex--;
            return true;
        }
        else
        {
            return false;
        }
    }
}

// HACK: Workaround class to tell the console not to show the returned value.
public class InvisibleValue
{
    public InvisibleValue()
    {

    }
}

[Serializable]
public class ExtendedBehaviour : MonoBehaviour
{
    public static void Log(string message)
    {
        Con.Log(message);
    }
}

public static class Con
{
    public static void Log(string message)
    {
        CSharpConsoleHelper.fetch.consoleText += message + "\n";
        Debug.Log(message);
    }
}