using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using KSP;
using Mono;
using Mono.CSharp;

[Serializable]
public class ConsoleExecBaseClass
{
    protected static string help = "The following commands are available:\n" +
                                    "\t<b>help</b> - Displays that which you are currently reading.\n" +
                                    "\t<b>Log(<i>message</i>)</b> - Log a message to this console and the Alt+F2 console (also sends it to the log files).\n" +
                                    "\t<b>showhistory</b> - Displays a list of commands entered into this console.\n" +
                                    "\t<b>clearhistory</b> - Clears the history of commands entered into this console.\n" +
                                    "\t<b>clear</b> - Clears all text displayed in this console.\n" +
                                    "\t<b>showconsole</b>, <b>hideconsole</b>, <b>toggleconsole</b> - Shows, hides, and toggles this console respectively.";
    protected static string quit = "Oh please, what do you expect me to do when you enter <b>quit</b>?";
    protected static string quti = "Enter <b>quit</b> for more info.";
    protected static string hello_hal
    {
        get
        {
            if (HighLogic.SaveFolder != null && HighLogic.SaveFolder != "" && HighLogic.SaveFolder != "default") {
                return "Hello, " + HighLogic.SaveFolder + ".";
            } else {
                return "Hello, Dave.";
            }
        }
    }
    protected static string hideconsole
    {
        get
        {
            if (CSharpConsoleHelper.IsVisible()) {
                CSharpConsoleHelper.HideConsole();
                return "Console hidden.";
            } else {
                return "Console is already hidden!";
            }
        }
    }
    protected static string showconsole
    {
        get
        {
            if (CSharpConsoleHelper.IsVisible()) {
                return "Console is already visible!";
            } else {
                CSharpConsoleHelper.ShowConsole();
                return "Console shown.";
            }
        }
    }
    protected static string toggleconsole
    {
        get
        {
            if (CSharpConsoleHelper.IsVisible()) {
                CSharpConsoleHelper.HideConsole();
                return "Console hidden.";
            } else {
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
