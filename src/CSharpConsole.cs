/*
    Interactive C# Console
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
using Mono.CSharp;

[KSPAddon(KSPAddon.Startup.Instantly, true)]
class CSharpConsoleLoader : MonoBehaviour
{
    public CSharpConsoleLoader()
    {
        // Manually load the Mono.CSharp library.
        Dependancy.Load("../lib/Mono.CSharp.dll.dat");

        // Instantiate the console.
        CSharpConsole.Initialize();

        // All loaded, now show the console.
        CSharpConsole.ShowConsole();
    }
}

public class CSharpConsole : ExtendedBehaviour<CSharpConsole>
{
    public static bool Initialize()
    {
        if (instance == null) {
            instance = new GameObject("CSharpConsole", typeof(CSharpConsole));
            MonoBehaviour.DontDestroyOnLoad(instance);
            return true;
        } else {
            Debug.LogWarning("CSharpConsole: Already initialized!");
            return false;
        }
    }
    public CSharpConsole()
    {
        Debug.Log("CSharpConsole: New instance created.");
    }

    private bool isVisible = false;
    public Rect windowRect = new Rect(0, 0, 650, 450);
    private Rect titleBarRect
    {
        get
        {
            return new Rect(0, 0, windowRect.width - 21, 20);
        }
    }
    private Vector2 scrollPosition = Vector2.zero;
    private bool autoScroll = true;

    private string consoleText = "<color=white>";
    private string cmd = "";
    private History history = new History();

    public void Update()
    {
        // Toggle the console visibility with the "`" (or "~") key.
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            isVisible = !isVisible;
            Debug.Log("CSharpConsole: isVisible = " + isVisible.ToString());
        }

        // TODO: If only there was some way to register these key events here while the TextField is focused...
        /*if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.PageUp)) {
            // Get previous input history item and set it as current.
            history.IndexPrev(ref cmd);
        } else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.PageDown)) {
            // Get next input history item and set it as current.
            history.IndexNext(ref cmd);
        }*/
    }

    public void OnGUI()
    {
        if (isVisible) {
            windowRect = GUI.Window(GUIUtility.GetControlID(FocusType.Passive, windowRect), windowRect, ConsoleWindow, "Interactive C# Console");
        }
    }
    private void ConsoleWindow(int windowID)
    {
        // Make sure the default text coloring is white.
        GUI.contentColor = Color.white;

        // The "x" (close) button in the top-right corner.
        if (GUI.Button(new Rect(windowRect.width - 28, 2, 21, 18), "x")) {
            HideConsole();
        }

        // Begin vertical block with textArea skin.
        GUILayout.BeginVertical(GUI.skin.textArea, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        {
            // Begin scroll view block.
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            {
                // Display the console text.
                GUILayout.TextArea(consoleText + "</color>", int.MaxValue, new GUIStyle()
                {
                    richText = true,
                    wordWrap = false,
                    clipping = TextClipping.Overflow,
                    stretchHeight = true,
                    stretchWidth = true
                });
            }
            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();

        // Begin horizontal layout block.
        GUILayout.BeginHorizontal();
        {
            // Clear button, resets the console text.
            if (GUILayout.Button("Clear", GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false))) {
                Clear();
            }

            // Toggle button for automatic scrolling.
            autoScroll = GUILayout.Toggle(autoScroll, "Autoscroll", GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));

            // TODO: Have to check for key events here instead of Update() so that the TextField doesn't steal the event!
            if (Event.current.isKey && Event.current.type == EventType.keyDown && (Event.current.keyCode == KeyCode.UpArrow || Event.current.keyCode == KeyCode.PageUp)) {
                // Get previous input history item and set it as current.
                history.IndexPrev(ref cmd);
            } else if (Event.current.isKey && Event.current.type == EventType.keyDown && (Event.current.keyCode == KeyCode.DownArrow || Event.current.keyCode == KeyCode.PageDown)) {
                // Get next input history item and set it as current.
                history.IndexNext(ref cmd);
            }

            // C# input box!
            // TODO: Remove the hardcoded "magic" number 200 in the width!
            cmd = GUILayout.TextField(cmd, int.MaxValue, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false), GUILayout.Width(windowRect.width - 200));

            // Submit button.
            if (GUILayout.Button("Submit", GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false)) || (Event.current.isKey && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return))) {
                // Print the code about to be executed to the console.
                Print("] " + cmd + "\n");

                // Add this to the console input history.
                fetch.history.Add(cmd);

                // Now, execute the code.
                Execute(cmd);

                // Clear the input box.
                cmd = "";
            }
        }
        GUILayout.EndHorizontal();

        // Make this window draggable by it's titlebar.
        GUI.DragWindow(titleBarRect);
    }

    // Executes the specified C# code.
    public static void Execute(string code)
    {
        // Really don't want this section to crash out.
        // Any caught errors will be logged to the console.
        try {
            // The Evaluator will use our custom class as the base class (in which scope and context the entered code is executed).
            // TODO: This could probably be moved to plugin load where it will only be set once (might save some overhead?).
            Evaluator.InteractiveBaseClass = typeof(ConsoleExecBaseClass);

            // An attempt at pre-referencing some assemblies.
            // Additional assembies can be referenced in-game by executing:
            // LoadAssembly("Assembly_Name");
            // using Namespace_Name;
            Evaluator.ReferenceAssembly(Assembly.GetExecutingAssembly());
            Evaluator.ReferenceAssembly(typeof(System.Object).Assembly);
            Evaluator.ReferenceAssembly(typeof(MonoBehaviour).Assembly);
            Evaluator.ReferenceAssembly(typeof(PSystemBody).Assembly);

            // TODO: Some of these actually don't seem to work "out of the box", KSP being one of them.
            string usings = @"
                using System;
                using System.Collections.Generic;
                using System.Text;
                using System.Xml;
                using KSP;
                using UnityEngine;
                using System.Reflection;
                using System.Linq;";

            try {
                // Attempt to run the using statements.
                Evaluator.Run(usings);
            } catch (Exception ex) {
                // Log the error if it failed for whatever reason.
                Print("<color=red>Run imports failed: " + ex.ToString() + "</color>\n");
                Debug.LogException(ex);
            }

            object res; // The returned value.
            bool ress; // Is the return value set?

            // Redirect the Evaluator message output.
            StringWriter err = new StringWriter();
            Evaluator.MessageOutput = err;

            // Evaluate the code, adding an extra semicolon just in case, and get the results.
            string s = Evaluator.Evaluate(code + ";", out res, out ress);

            // Get any errors that may have ocurred during execution.
            string error = err.ToString();

            // Check if we got an error returned.
            if (error.Length > 0) {
                // Log the error message.
                Print("<color=red>" + error + "</color>\n");
                Debug.LogError(error);
            } else {
                if (ress) {
                    if (res.GetType().Equals(typeof(InvisibleValue))) {
                        // HACK: Workaround to explicitly disable showing the returned value.
                    } else if (res != null) {
                        // Print the returned object as a string.
                        Print(res.ToString() + "\n");
                        Debug.Log(res.ToString());
                    } else {
                        // A null value was returned.
                        Print("<color=red><b><i>null</i></b></color>\n");
                    }
                } else {
                    // The return value was not set (most likely because it was a function with void return).
                    Print("<color=#EEEEEE><b><i>no result</i></b></color>\n");
                }
            }
        } catch (Exception ex) {
            // Log any uncaught error messages.
            Print("<color=red>" + ex.ToString() + "</color>\n");
            Debug.LogException(ex);
        }
    }

    // Shows the console and creates it if necessary.
    public static void ShowConsole()
    {
        if (instance == null) {
            // For lazy peeps that didn't bother to Initialize() it.
            Initialize();
        }
        fetch.isVisible = true;
        //Debug.Log("CSharpConsole: isVisible = " + fetch.isVisible.ToString());
    }

    // Hides the console, of course.
    public static void HideConsole()
    {
        fetch.isVisible = false;
        //Debug.Log("CSharpConsole: isVisible = " + fetch.isVisible.ToString());
    }

    // Returns wether the console is visible.
    public static bool IsVisible()
    {
        return fetch.isVisible;
    }

    // Gets a list of console command history.
    public static List<string> GetHistory()
    {
        return fetch.history.Get();
    }

    // Clears the console command history, of course.
    public static void ClearHistory()
    {
        fetch.history.Clear();
    }

    // Clears all text in the console and sets it to the default.
    public static void Clear()
    {
        fetch.consoleText = "<color=white>";
    }

    // Resets the console to it's default state, more or less.
    public static void Reset()
    {
        Clear();
        ClearHistory();
    }

    // Prints the specified text to the console.
    public static void Print(string message)
    {
        fetch.consoleText += message;

        if (fetch.autoScroll) {
            // If autoScroll enabled, set scrollPosition to the bottom.
            fetch.scrollPosition = new Vector2(fetch.scrollPosition.x, float.MaxValue);
        }
    }
}
