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
using Mono.CSharp;
using Event = UnityEngine.Event;
using Evaluator = CSharpConsoleKSP.Evaluator;

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
    private string[] completions = null;
    private string completionPrefix = "";
    private bool updateCompletions = false;
    private GUIStyle autoCompleteSkin = null;
    private Texture2D skinBackground = null;

    public void Update()
    {
        // Toggle the console visibility with the "`" (or "~") key.
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            isVisible = !isVisible;
            Debug.Log("CSharpConsole: isVisible = " + isVisible.ToString());
        }

        if (updateCompletions && !cmd.StartsWith("using ")) {
            completions = Evaluator.fetch.GetCompletions(cmd, out completionPrefix);
            updateCompletions = false;
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
        // Create the auto complete skin the first time.
        // This has to be done in OnGUI() due to restrictions in place by Unity Engine.
        if (autoCompleteSkin == null) {
            skinBackground = new Texture2D(1, 1, TextureFormat.Alpha8, false);

            autoCompleteSkin = new GUIStyle(GUI.skin.label);
            autoCompleteSkin.alignment = TextAnchor.MiddleLeft;
            autoCompleteSkin.richText = false;
            autoCompleteSkin.wordWrap = false;
            autoCompleteSkin.stretchWidth = true;
            autoCompleteSkin.stretchHeight = false;

            autoCompleteSkin.normal = new GUIStyleState()
            {
                background = skinBackground,
                textColor = Color.white
            };

            autoCompleteSkin.active = new GUIStyleState()
            {
                background = skinBackground,
                textColor = new Color(0, 0.5f, 0, 1)
            };

            autoCompleteSkin.focused = new GUIStyleState()
            {
                background = skinBackground,
                textColor = Color.green
            };

            autoCompleteSkin.hover = new GUIStyleState()
            {
                background = skinBackground,
                textColor = Color.green
            };

            autoCompleteSkin.border = new RectOffset(0, 0, 0, 0);
            autoCompleteSkin.padding = new RectOffset(3, 0, 0, 0);
        }

        if (isVisible) {
            windowRect = GUI.Window(GUIUtility.GetControlID(FocusType.Passive, windowRect), windowRect, ConsoleWindow, "Interactive C# Console");
            if (completions != null && completions.Length > 0) {
                GUI.Window(GUIUtility.GetControlID(FocusType.Passive), new Rect(windowRect.x + windowRect.width / 2, windowRect.y + windowRect.height, 200, 200), AutoCompleteWindow, "", GUI.skin.box);
            }
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
            string input = GUILayout.TextField(cmd, int.MaxValue, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false), GUILayout.Width(windowRect.width - 200));

            if (input != cmd) {
                cmd = input;
                updateCompletions = true;
            }

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
    private void AutoCompleteWindow(int windowID)
    {
        int y = 0;
        foreach (string str in completions) {
            if (completionPrefix != null) {
                if (GUI.Button(new Rect(0, y, 200, 20), completionPrefix + str, autoCompleteSkin)) {
                    cmd += str;
                    completions = null;
                    completionPrefix = "";
                }
            } else {
                if (GUI.Button(new Rect(0, y, 200, 20), str, autoCompleteSkin)) {
                    cmd += str;
                    completions = null;
                    completionPrefix = "";
                }
            }
            y += 18;
        }
    }

    // Executes the specified C# code.
    public static void Execute(string code)
    {
        // Really don't want this section to crash out.
        // Any caught errors will be logged to the console.
        try {
            // The Evaluator will use our custom class as the base class (in which scope and context the entered code is executed).
            // TODO: This could probably be moved to plugin load where it will only be set once (might save some overhead?).
            Evaluator.fetch.InteractiveBaseClass = typeof(ConsoleExecBaseClass);

            // An attempt at pre-referencing some assemblies.
            // Additional assembies can be referenced in-game by executing:
            // LoadAssembly("Assembly_Name");
            // using Namespace_Name;
            Evaluator.fetch.ReferenceAssembly(Assembly.GetExecutingAssembly());
            Evaluator.fetch.ReferenceAssembly(typeof(System.Object).Assembly);
            Evaluator.fetch.ReferenceAssembly(typeof(MonoBehaviour).Assembly);
            Evaluator.fetch.ReferenceAssembly(typeof(PSystemBody).Assembly);

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
                Evaluator.fetch.Run(usings);
            } catch (Exception ex) {
                // Log the error if it failed for whatever reason.
                Print("<color=red>Run imports failed: " + ex.ToString() + "</color>\n");
                Debug.LogException(ex);
            }

            object res; // The returned value.
            bool ress; // Is the return value set?

            // Redirect the Evaluator message output.
            StringWriter err = new StringWriter();
            //Evaluator.fetch.MessageOutput = err;
            CSharpConsoleKSP.EvaluatorPrinter.output = err;

            // Evaluate the code, adding an extra semicolon just in case, and get the results.
            string s = Evaluator.fetch.Evaluate(code + ";", out res, out ress);

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
            Print(ex.ToString() + "\n", LogType.Exception);
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

    /// <summary>
    /// Prints the passed message to the console with the color mapped to the LogType.
    /// </summary>
    /// <param name="message">The message to print to the console.</param>
    /// <param name="type">The LogType mapped to the color defined in Con.logTypeColors</param>
    public static void Print(string message, LogType type)
    {
        Print("<color=" + Con.logTypeColors[type] + ">" + message + "</color>");
    }

    /// <summary>
    /// Log callback method that prints the log message to the console.
    /// </summary>
    /// <param name="message">The log message to be shown.</param>
    /// <param name="stackTrace">Trace of where the log message came from.</param>
    /// <param name="type">Type of log message (error, exception, warning, assert).</param>
    public static void HandleLog(string message, string stackTrace, LogType type)
    {
        string output = "[" + type.ToString() + "]: " + message;
        if (stackTrace != null && stackTrace != "") {
            output += "\n" + stackTrace;
        }
        output += "\n";
        Print(output, type);
    }
}
