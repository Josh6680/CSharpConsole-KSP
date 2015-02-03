using System.IO;
using System.Reflection;
using UnityEngine;

public static class Dependancy
{
	public static Assembly Load(string filename)
	{
		string path = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName + "\\" + filename;
		if (File.Exists(path))
		{
			return Assembly.LoadFile(path);
		}
		else
		{
			Debug.LogError("CSharpConsoleLoader: Failed to load dependancy: '" + path + "'");
			return null;
		}
	}
}
