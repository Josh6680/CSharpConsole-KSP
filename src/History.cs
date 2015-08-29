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
using System.Collections.Generic;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Global

[Serializable]
public class History
{
	private readonly List<string> history;

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
		if (lastindex == -1)
		{
			return false;
		}
		else if (lastindex == 0)
		{
			str = Get(lastindex);
			return true;
		}
		else if (lastindex > 0)
		{
			str = Get(lastindex);
			lastindex--;
			return true;
		}
		else
		{
			return false;
		}
	}

	public bool IndexNext(ref string str)
	{
		if (lastindex == -1)
		{
			return false;
		}
		else if (lastindex == history.Count - 1)
		{
			str = Get(lastindex);
			return true;
		}
		else if (lastindex < history.Count - 1)
		{
			str = Get(lastindex);
			lastindex++;
			return true;
		}
		else
		{
			return false;
		}
	}
}
