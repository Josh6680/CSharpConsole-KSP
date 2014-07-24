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
    public Rect winrect = new Rect(0, 0, 625, 450);
    public Vector2 scrollPosition = Vector2.zero;
    public string consoleText = "<color=white>";
    private string cmd = "";
    public History history = new History();
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            isVisible = !isVisible;
            Debug.Log("CSharpConsole: isVisible = " + isVisible.ToString());
        }
    }
    public void OnGUI()
    {
        if (isVisible) {
            winrect = GUI.Window(888888888, winrect, wnd, "Interactive C# Console");
        }
    }
    private void wnd(int windowID)
    {
        GUI.DragWindow(new Rect(0, 0, winrect.width - 21, 20));

        if (GUI.Button(new Rect(winrect.width - 22, 2, 20, 20), "x")) {
            HideConsole();
        }

        GUI.BeginGroup(new Rect(0, 0, winrect.width, winrect.height - 20));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Width(winrect.width - 12), GUILayout.Height(winrect.height - 21 * 2), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
        GUILayout.Label(consoleText + "</color>", new GUIStyle()
        {
            richText = true,
            wordWrap = false,
            clipping = TextClipping.Overflow,
            stretchHeight = true,
            stretchWidth = true
        });
        GUILayout.EndScrollView();
        GUI.EndGroup();

        if (GUI.Button(new Rect(0, winrect.height - 20, 50, 20), "Clear")) {
            Clear();
        }

        if (Event.current.isKey && Event.current.type == EventType.keyDown && (Event.current.keyCode == KeyCode.UpArrow || Event.current.keyCode == KeyCode.PageUp)) {
            history.IndexPrev(ref cmd);
        } else if (Event.current.isKey && Event.current.type == EventType.keyDown && (Event.current.keyCode == KeyCode.DownArrow || Event.current.keyCode == KeyCode.PageDown)) {
            history.IndexNext(ref cmd);
        }

        cmd = GUI.TextField(new Rect(50, winrect.height - 20, winrect.width - 50 - 50, 20), cmd);
        if (GUI.Button(new Rect(winrect.width - 50, winrect.height - 20, 50, 20), "Submit") || (Event.current.isKey && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return))) {
            consoleText += "] " + cmd + "\n";
            history.Add(cmd);
            try {
                Evaluator.ReferenceAssembly(Assembly.GetExecutingAssembly());
                Evaluator.ReferenceAssembly(typeof(System.Object).Assembly);
                Evaluator.ReferenceAssembly(typeof(MonoBehaviour).Assembly);
                Evaluator.ReferenceAssembly(typeof(PSystemBody).Assembly);
                //Evaluator.ReferenceAssembly(typeof(JUtil).Assembly);
                string usings = @"
                    using System;
                    using System.Collections.Generic;
                    using System.Text;
                    using System.Xml;
                    using UnityEngine;
                    using KSP;
                    using System.Reflection;";
                //using System.Linq;
                //using JUtils;";
                bool result = false;
                try {
                    result = Evaluator.Run(usings);
                } catch (Exception e) {
                    consoleText += "<color=red>Run imports failed: " + e.ToString() + "</color>\n";
                    Debug.LogException(e);
                }
                //consoleText += "Import result = " + result.ToString() + "\n";
                //Debug.Log("Import result = " + result.ToString());
                bool ress;
                object res;
                StringWriter err = new StringWriter();
                Evaluator.InteractiveBaseClass = typeof(ConsoleExecBaseClass);
                Evaluator.MessageOutput = err;
                string s = Evaluator.Evaluate(cmd + ";", out res, out ress);
                string error = err.ToString();
                if (error.Length > 0) {
                    consoleText += "<color=red>" + error + "</color>\n";
                    Debug.LogError(error);
                } else {
                    if (ress) {
                        if (res.GetType().Equals(typeof(InvisibleValue))) {
                            // HACK: Workaround to explicitly disable showing the returned value.
                        } else if (res != null) {
                            consoleText += res.ToString() + "\n";
                            Debug.Log(res.ToString());
                        } else {
                            consoleText += "<color=red><b><i>null</i></b></color>\n";
                        }
                    } else {
                        consoleText += "<color=#EEEEEE><b><i>no result</i></b></color>\n";
                    }
                }
            } catch (Exception ex) {
                consoleText += "<color=red>" + ex.ToString() + "</color>\n";
                Debug.LogException(ex);
            }
            cmd = "";
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
        Debug.Log("CSharpConsole: isVisible = " + fetch.isVisible.ToString());
    }

    // Hides the console, of course.
    public static void HideConsole()
    {
        fetch.isVisible = false;
        Debug.Log("CSharpConsole: isVisible = " + fetch.isVisible.ToString());
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
}
