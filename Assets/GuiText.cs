using UnityEngine;
using System.Collections.Generic;

public class GuiText
{
	public string Text;
	public int X;
	public int Y;
	public int Size;
	public float DelayMilliSeconds;
	public float MilliSeconds;
	public GUIStyle Style;

	public GuiText()
	{
		Style = new GUIStyle();
		Style.fontSize = 60;
		Style.fontStyle = FontStyle.Bold;
		Style.normal.textColor = Color.black;
	}
}
