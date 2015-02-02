/*
    Interactive C# Console
    https://github.com/Josh6680/CSharpConsole-KSP
    Copyright (C) 2014 Josh

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
    which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
    itself is governed by the terms of its EULA, not the license above.
    https://kerbalspaceprogram.com/

    This library makes use of the Mono.CSharp library
    which is copyright 2001, 2002, 2003 The Mono Project
    The Mono.CSharp library itself is licensed under the MIT X11 license.
    https://github.com/mono/mono/tree/master/mcs/class/Mono.CSharp
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using KSP;
using Mono;

// HACK: Workaround class to tell the console not to show the returned value.
public class InvisibleValue { /* Nothing to see here... */ }

[Serializable]
public class ExtendedBehaviour<T> : MonoBehaviour where T : Component
{
    // Contains the instance of this class.
    protected static GameObject instance = null;

    // Fetches the instance of this class.
    public static T fetch
    {
        get
        {
            return instance.GetComponent<T>();
        }
    }

    // Logs a debug message.
    private static void Log(string message)
    {
        Con.Log(message);
    }
}

/// <summary>
/// Provides convenient utilities for logging debug messages.
/// </summary>
public static class Con
{
    public static readonly Dictionary<LogType, string> logTypeColors = new Dictionary<LogType, string>
	{
		{ LogType.Error, "#9C0000FF" },
		{ LogType.Assert, "orange" },
		{ LogType.Warning, "yellow" },
		{ LogType.Log, "white" },
		{ LogType.Exception, "red" },
	};

    public static void Log(string message)
    {
        if (message != null) {
            CSharpConsole.Print(message + "\n");
            Debug.Log(message);
        } else {
#if DEBUG
            CSharpConsole.Print("[Debug] A null message log was attempted!\n", LogType.Warning);
            Debug.LogWarning("CSharpConsole: A null message log was attempted!");
#endif
        }
    }

    public static void Log(string message, LogType type)
    {
        if (message != null) {
            CSharpConsole.Print(message + "\n");
            Debug.Log(message);
        } else {
#if DEBUG
            CSharpConsole.Print("[Debug] A null message log was attempted!\n", LogType.Warning);
            Debug.LogWarning("CSharpConsole: A null message log was attempted!");
#endif
        }
    }
}
