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
public class History
{
    private List<string> history;
    [NonSerialized]
    private int lastindex = -1;
    public History()
    {
        history = new List<string>();
    }
    public void Add(string item)
    {
        history.Add(item);
        lastindex = history.Count - 1;
    }
    public void Clear()
    {
        lastindex = -1;
        history.Clear();
    }
    public List<string> Get()
    {
        return history;
    }
    public string Get(int index)
    {
        return history[index];
    }
    public bool Exists(string item)
    {
        return history.Exists(i => i.Equals(item));
    }
    public int LastIndex()
    {
        return lastindex;
    }
    public bool IndexPrev(ref string str)
    {
        if (lastindex == -1) {
            return false;
        } else if (lastindex == 0) {
            str = Get(lastindex);
            return true;
        } else if (lastindex > 0) {
            str = Get(lastindex);
            lastindex--;
            return true;
        } else {
            return false;
        }
    }
    public bool IndexNext(ref string str)
    {
        if (lastindex == -1) {
            return false;
        } else if (lastindex == history.Count - 1) {
            str = Get(lastindex);
            return true;
        } else if (lastindex < history.Count - 1) {
            str = Get(lastindex);
            lastindex++;
            return true;
        } else {
            return false;
        }
    }
}
