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
    // This class is used as the base class for anything executed by the console.
    // Below are some "basic console commands" which can be used simply by entering their name.

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
            if (CSharpConsole.IsVisible()) {
                CSharpConsole.HideConsole();
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
            if (CSharpConsole.IsVisible()) {
                return "Console is already visible!";
            } else {
                CSharpConsole.ShowConsole();
                return "Console shown.";
            }
        }
    }
    protected static string toggleconsole
    {
        get
        {
            if (CSharpConsole.IsVisible()) {
                CSharpConsole.HideConsole();
                return "Console hidden.";
            } else {
                CSharpConsole.ShowConsole();
                return "Console shown.";
            }
        }
    }
    protected static string showhistory
    {
        get
        {
            string history = "";
            CSharpConsole.GetHistory().ForEach(a => history += a + "\n");
            return history;
        }
    }
    protected static InvisibleValue clearhistory
    {
        get
        {
            CSharpConsole.ClearHistory();
            return new InvisibleValue();
        }
    }
    protected static InvisibleValue clear
    {
        get
        {
            CSharpConsole.Clear();
            return new InvisibleValue();
        }
    }
    protected static InvisibleValue reset
    {
        get
        {
            CSharpConsole.Reset();
            return new InvisibleValue();
        }
    }
    protected static string showusing
    {
        get
        {
            return Evaluator.GetUsing();
        }
    }
    public static void ShowUsing()
    {
        Con.Log(showusing);
    }

    protected static string showvars
    {
        get
        {
            return Evaluator.GetVars();
        }
    }
    public static void ShowVars()
    {
        Con.Log(showvars);
    }

    public static CompiledMethod Compile(string input)
    {
        return Evaluator.Compile(input);
    }
    public static string Compile(string input, out CompiledMethod compiled)
    {
        return Evaluator.Compile(input, out compiled);
    }
    public static object Evaluate(string input)
    {
        return Evaluator.Evaluate(input);
    }
    public static string Evaluate(string input, out object result, out bool result_set)
    {
        return Evaluator.Evaluate(input, out result, out result_set);
    }
    public static bool Run(string statement)
    {
        return Evaluator.Run(statement);
    }
    public static void Interrupt()
    {
        Evaluator.Interrupt();
    }
    public static void ReferenceAssembly(Assembly a)
    {
        Evaluator.ReferenceAssembly(a);
    }

    public static string ContinuationPrompt
    {
        get
        {
            return InteractiveBase.ContinuationPrompt;
        }
    }
    public static TextWriter Error
    {
        get
        {
            return InteractiveBase.Error;
        }
    }
    public static TextWriter Output
    {
        get
        {
            return InteractiveBase.Output;
        }
    }
    public static string Prompt
    {
        get
        {
            return InteractiveBase.Prompt;
        }
    }

    public static void LoadAssembly(string assembly)
    {
        InteractiveBase.LoadAssembly(assembly);
    }
    public static void LoadPackage(string pkg)
    {
        InteractiveBase.LoadPackage(pkg);
    }
    public static TimeSpan Time(InteractiveBase.Simple a)
    {
        return InteractiveBase.Time(a);
    }

    public static InvisibleValue Log(string message)
    {
        Con.Log(message);
        return new InvisibleValue();
    }
}
