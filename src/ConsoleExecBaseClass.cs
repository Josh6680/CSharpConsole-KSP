/*
	Interactive C# Console
	https://github.com/Josh6680/CSharpConsole-KSP
	Copyright (C) 2015 Josh

	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License along
	with this program; if not, write to the Free Software Foundation, Inc.,
	51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

	This library is intended to be used as a plugin for Kerbal Space Program
	which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
	itself is governed by the terms of its EULA, not the license above.
	https://kerbalspaceprogram.com/

	This library makes use of the Mono.CSharp library
	which is copyright 2001, 2002, 2003 The Mono Project
	The Mono.CSharp library itself is licensed under the MIT X11 license.
	https://github.com/mono/mono/tree/master/mcs/class/Mono.CSharp
*/

using System;
using System.IO;
using System.Reflection;
using Mono.CSharp;
using Evaluator = CSharpConsoleKSP.Evaluator;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

[Serializable]
public class ConsoleExecBaseClass
{
	// This class is used as the base class for anything executed by the console.
	// Below are some "basic console commands" which can be used simply by entering their name.

	public static string about
	{
		get
		{
			return version + "\n" + license;
		}
	}

	protected static string version
	{
		get
		{
#if DEBUG
			const string buildType = " [Debug Build]";
#elif RELEASE
			const string buildType = " [Release Build]";
#else
			const string buildType = " [Unknown Build]";
#endif
			return "<< Interactive C# Console v" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + buildType + " by Josh >>";
		}
	}

	protected static string license
	{
		get
		{
			return "<< This plugin/library is licensed under the GNU General Public License Version 2 >>\n" +
				"<< The Mono.CSharp library is copyright The Mono Project and licensed under the MIT X11 license >>";
		}
	}

	protected static string help
	{
		get
		{
			return version + "\n" +
				"The following are some commands that are available:\n" +
				"\t<b>help</b> - Displays that which you are currently reading.\n" +
				"\t<b>about</b> - Displays information about this plugin.\n" +
				"\t<b>version</b> - Displays version information for this plugin.\n" +
				"\t<b>license</b> - Displays licensing information for this plugin.\n" +
				"\t<b>Log(<i>message</i>)</b> - Log a message to this console and the Alt+F2 console (also sends it to the log files).\n" +
				"\t<b>showhistory</b> - Displays a list of commands entered into this console.\n" +
				"\t<b>clearhistory</b> - Clears the history of commands entered into this console.\n" +
				"\t<b>clear</b> - Clears all text displayed in this console.\n" +
				"\t<b>showconsole</b>, <b>hideconsole</b>, <b>toggleconsole</b> - Shows, hides, and toggles this console respectively.\n" +
				"\t<b>LoadAssembly(<i>assembly_name</i>)</b> - Loads / references the specified assembly by name.";
		}
	}

	protected static string quit = "Oh please, what do you expect me to do when you enter <b>quit</b>?";
	protected static string quti = "Enter <b>quit</b> for more info.";

	protected static string hello_hal
	{
		get
		{
			if (!string.IsNullOrEmpty(HighLogic.SaveFolder) && HighLogic.SaveFolder != "default")
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
			if (CSharpConsole.IsVisible())
			{
				CSharpConsole.HideConsole();
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
			if (CSharpConsole.IsVisible())
			{
				return "Console is already visible!";
			}
			else
			{
				CSharpConsole.ShowConsole();
				return "Console shown.";
			}
		}
	}

	protected static string toggleconsole
	{
		get
		{
			if (CSharpConsole.IsVisible())
			{
				CSharpConsole.HideConsole();
				return "Console hidden.";
			}
			else
			{
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
			return Evaluator.fetch.GetUsing();
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
			return Evaluator.fetch.GetVars();
		}
	}

	public static void ShowVars()
	{
		Con.Log(showvars);
	}

	public static CompiledMethod Compile(string input)
	{
		return Evaluator.fetch.Compile(input);
	}

	public static string Compile(string input, out CompiledMethod compiled)
	{
		return Evaluator.fetch.Compile(input, out compiled);
	}

	public static object Evaluate(string input)
	{
		return Evaluator.fetch.Evaluate(input);
	}

	public static string Evaluate(string input, out object result, out bool result_set)
	{
		return Evaluator.fetch.Evaluate(input, out result, out result_set);
	}

	public static bool Run(string statement)
	{
		return Evaluator.fetch.Run(statement);
	}

	public static void Interrupt()
	{
		Evaluator.fetch.Interrupt();
	}

	public static void ReferenceAssembly(Assembly a)
	{
		Evaluator.fetch.ReferenceAssembly(a);
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

	public static TimeSpan Time(Action a)
	{
		return InteractiveBase.Time(a);
	}

	public static InvisibleValue Log(string message)
	{
		Con.Log(message);
		return new InvisibleValue();
	}

	// Console variables
	private static bool _con_hooklog = false;

	public static bool con_hooklog
	{
		get
		{
			return _con_hooklog;
		}
		set
		{
			if (value)
			{
				if (_con_hooklog)
					Con.Log("The log is already hooked!");
				else
					KSPLog.AddLogCallback(CSharpConsole.HandleLog);
			}
			else
			{
				if (_con_hooklog)
					KSPLog.RemoveLogCallback(CSharpConsole.HandleLog);
				else
					Con.Log("The log is already <b>un</b>hooked!");
			}
			_con_hooklog = value;
		}
	}
}
