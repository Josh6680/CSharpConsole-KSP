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

[KSPAddon(KSPAddon.Startup.Instantly, true)]
class CSharpConsoleLoader : ExtendedBehaviour
{
    //public static Assembly a;
    public CSharpConsoleLoader()
    {
        Dependancy.Load("../lib/Mono.CSharp.dll.dat");
        //using (Stream resFilestream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CSharpConsole.lib.Mono.CSharp.dll"))
        //{
        //    if (resFilestream == null)
        //    {
        //        Log("CSharpConsole: Failed to load dependencies!");
        //        throw new MissingReferenceException("CSharpConsole: Failed to load embedded dependency: 'CSharpConsole.lib.Mono.CSharp.dll'!");
        //    }
        //    byte[] ba = new byte[resFilestream.Length];
        //    resFilestream.Read(ba, 0, ba.Length);
        //    a = Assembly.Load(ba);
        //}
        //Log("CSharpConsole: Loaded dependancy: " + a.FullName);

        // Add assembly resolve event handler.
        //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

        // All loaded, now show the console.
        CSharpConsoleHelper.ShowConsole();
    }
    //public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    //{
    //    return a;
    //}
}

public static class CSharpConsoleHelper
{
    private static GameObject instance = null;
    public static CSharpConsole fetch
    {
        get
        {
            return instance.GetComponent<CSharpConsole>();
        }
    }
    public static void ShowConsole()
    {
        if (instance == null)
        {
            instance = new GameObject("CSharpConsole", typeof(CSharpConsole));
            MonoBehaviour.DontDestroyOnLoad(instance);
        }
        //instance.ShowConsole();
        //instance.SendMessage("ShowConsole");
        fetch.ShowConsole();
    }
    public static void HideConsole()
    {
        //instance.HideConsole();
        //instance.SendMessage("HideConsole");
        fetch.HideConsole();
    }
    public static bool IsVisible()
    {
        return fetch.IsVisible();
    }
    public static List<string> GetHistory()
    {
        return fetch.history.Get();
    }
    public static void ClearHistory()
    {
        fetch.history.Clear();
    }
    public static void Clear()
    {
        fetch.consoleText = "<color=white>";
    }
}

public class CSharpConsole : ExtendedBehaviour
{
    public CSharpConsole() : base()
    {
        Log("CSharpConsole: New instance created.");
    }
    private bool isVisible = false;
    public Rect winrect = new Rect(0, 0, 600, 450);
    public Vector2 scrollPosition = Vector2.zero;
    public string consoleText = "<color=white>";
    private string cmd = "";
    public History history = new History();
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            isVisible = !isVisible;
            Debug.Log("CSharpConsole: isVisible = " + isVisible.ToString());
        }
    }
    public void OnGUI()
    {
        if (isVisible)
        {
            winrect = GUI.Window(888888888, winrect, wnd, "Interactive C# Console");
        }
    }
    private void wnd(int windowID)
    {
        GUI.DragWindow(new Rect(0, 0, winrect.width - 21, 20));

        if (GUI.Button(new Rect(winrect.width - 22, 2, 20, 20), "x"))
        {
            HideConsole();
        }

        GUI.BeginGroup(new Rect(0, 0, winrect.width, winrect.height - 20));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Width(winrect.width - 12), GUILayout.Height(winrect.height - 21 * 2), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
        //scrollPosition = GUI.BeginScrollView(new Rect(2, 21, winrect.width - 2, winrect.height - 20 * 2),scrollPosition, new Rect(0, 21, winrect.width, winrect.height - 20 * 2), false, true);
        //GUI.Label(new Rect(0, 21, winrect.width, winrect.height - 20), consoleText + "</color>", new GUIStyle() {
        GUILayout.Label(consoleText + "</color>", new GUIStyle()
        {
            richText = true,
            wordWrap = false,
            clipping = TextClipping.Overflow,
            stretchHeight = true,
            stretchWidth = true
        });
        //GUI.EndScrollView();
        GUILayout.EndScrollView();
        GUI.EndGroup();

        if (GUI.Button(new Rect(0, winrect.height - 20, 50, 20), "Clear"))
        {
            CSharpConsoleHelper.Clear();
        }

        if (Event.current.isKey && Event.current.type == EventType.keyDown && (Event.current.keyCode == KeyCode.UpArrow || Event.current.keyCode == KeyCode.PageUp))
        {
            history.IndexPrev(ref cmd);
            //Event.current.Use();
        }
        else if (Event.current.isKey && Event.current.type == EventType.keyDown && (Event.current.keyCode == KeyCode.DownArrow || Event.current.keyCode == KeyCode.PageDown))
        {
            history.IndexNext(ref cmd);
            //Event.current.Use();
        }

        cmd = GUI.TextField(new Rect(50, winrect.height - 20, winrect.width - 50 - 50, 20), cmd);
        if (GUI.Button(new Rect(winrect.width - 50, winrect.height - 20, 50, 20), "Submit") || (Event.current.isKey && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return)))
        {
            consoleText += "] " + cmd + "\n";
            history.Add(cmd);
            try
            {
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
                try
                {
                    result = Evaluator.Run(usings);
                }
                catch (Exception e)
                {
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
                if (error.Length > 0)
                {
                    consoleText += "<color=red>" + error + "</color>\n";
                    Debug.LogError(error);
                }
                else
                {
                    if (ress)
                    {
                        if (res.GetType().Equals(typeof(InvisibleValue)))
                        {
                            // HACK: Workaround to explicitly disable showing the returned value.
                        }
                        else if (res != null)
                        {
                            consoleText += res.ToString() + "\n";
                            Debug.Log(res.ToString());
                        }
                        else
                        {
                            consoleText += "<color=red><b><i>null</i></b></color>\n";
                        }
                    }
                    else
                    {
                        consoleText += "<color=#EEEEEE><b><i>no result</i></b></color>\n";
                    }
                }
            }
            catch (Exception ex)
            {
                consoleText += "<color=red>" + ex.ToString() + "</color>\n";
                Debug.LogException(ex);
            }
            cmd = "";
        }
    }
    public void ShowConsole()
    {
        isVisible = true;
        Debug.Log("CSharpConsole: isVisible = " + isVisible.ToString());
    }
    public void HideConsole()
    {
        isVisible = false;
        Debug.Log("CSharpConsole: isVisible = " + isVisible.ToString());
    }
    public bool IsVisible()
    {
        return isVisible;
    }
}
