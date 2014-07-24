using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using KSP;
using Mono;
using Mono.CSharp;

public static class Dependancy
{
    public static Assembly Load(string filename)
    {
        string path = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName + "\\" + filename;
        if (File.Exists(path)) {
            return Assembly.LoadFile(path);
        } else {
            Debug.LogError("CSharpConsole: Failed to load dependancy: '" + path + "'");
            return null;
        }
    }
}

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
    public static void Log(string message)
    {
        Con.Log(message);
    }
}

// Entire purpose of this class is to log debug messages to wherever.
public static class Con
{
    public static void Log(string message)
    {
        CSharpConsole.fetch.consoleText += message + "\n";
        Debug.Log(message);
    }
}
