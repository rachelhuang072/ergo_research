using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class s_Gui : MonoBehaviour
{
	s_Data Data;
	s_Controller Controller;
	s_Gesture Gesture;
	s_Self SelfMove;
	Canvas canvas;
	UnityEngine.UI.Text txt;
	UnityEngine.UI.Image img;

	public bool ShowTimer = false;

	Texture2D BackTexture;
	Texture2D CurveTexture;
	Texture2D ShowTexture;

	// Use this for initialization
	void Start ()
	{
		Controller = GameObject.Find("Controller").GetComponent<s_Controller>();
		Data = GameObject.Find("Data").GetComponent<s_Data>();
		if (Data == null) Debug.Log("herenull");
		Gesture = GameObject.Find("Gesture").GetComponent<s_Gesture>();
		SelfMove = GameObject.FindWithTag("Self").GetComponent<s_Self>();

		Color32[] backc = new Color32[1];
		backc[0] = new Color32(100, 100, 250, 120);
		BackTexture = new Texture2D(100, 150);
		for (int i = 0; i < 100; i++)
			for (int j = 0; j < 150; j++)
				BackTexture.SetPixels32(i, j, 1, 1, backc);

		Cursor.visible = false;

		canvas = GameObject.Find("Canvas").GetComponent<Canvas> ();
		canvas.enabled = true;
		UnityEngine.UI.Text[] textTest = canvas.GetComponentsInChildren<UnityEngine.UI.Text> ();
		img = canvas.GetComponentInChildren<UnityEngine.UI.Image> ();
		txt = textTest [0];
        txt.enabled = false;
	}

	// Update is called once per frame
	float deltaTime = 0.0f;
	void Update ()
	{
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
	}

	public void DrawText(string text, int ms = 1500, int delayms = 0, int fontsize = 60, int x = 140, int y = 20, float h = 0.07f, float s = 0.5f, float v = 0.5f)
	{
		GuiText guitext = new GuiText();
		guitext.X = x;
		guitext.Y = y;
		guitext.MilliSeconds = ms;
		guitext.DelayMilliSeconds = delayms;
		guitext.Text = text;
		guitext.Style.fontSize = fontsize;
		guitext.Style.normal.textColor = Color.HSVToRGB(h, s, v);
		if (Data == null)
			Data = GameObject.Find("Data").GetComponent<s_Data>();
		if (Data.GuiTexts == null) Debug.Log("GuiTextsNull");
		Data.GuiTexts.Add(guitext);

        String temp_text = "";
        for (int i = 0; i < Data.GuiTexts.Count; i++) {
            temp_text += Data.GuiTexts[i].Text + "\n";
            }
		//change text in canvas
		txt.text = temp_text;
        txt.enabled = true;

	}

	public void CancelText(string text)
	{
		for (int i = 0; i < Data.GuiTexts.Count; i++)
		{
			if (Data.GuiTexts[i].Text == text)
			{
				Data.GuiTexts.RemoveAt(i);
				i--;
			}
		}
        txt.enabled = false;
	}

	public void ClearAllTexts()
	{
		//Data.GuiTexts.Clear();
		for (int i = 0; i < Data.GuiTexts.Count; i++)
		{
			if (Data.GuiTexts[i].Text.Length > 3 && Data.GuiTexts[i].Text.Substring(0, 3) == "Err")
				continue;
			
			Data.GuiTexts.RemoveAt(i);
			i--;
		}
        txt.enabled = false;
	}

	public void RedrawCurve()
	{
		Color32[] c = new Color32[1];
		c[0] = new Color32(250, 250, 250, 150);

		Destroy(CurveTexture);
		CurveTexture = Instantiate(BackTexture) as Texture2D;

		// dead zone boundry
		for (int j = 10; j < 90; j++)
		{
			CurveTexture.SetPixels32(j, 75 + (int)(Data.CurrentTest.DeadZoneRadius / 2), 1, 1, c);
			CurveTexture.SetPixels32(j, 75 - (int)(Data.CurrentTest.DeadZoneRadius / 2), 1, 1, c);
		}
		// velocity axis
		for (int j = 10; j < 90; j++)
		{
			CurveTexture.SetPixels32(j, 75, 1, 1, c);
		}
		// hand position axis
		for (int i = 0; i < 150; i++)
		{
			CurveTexture.SetPixels32(50, i, 1, 1, c);
		}

		// velocity curve
		for (int i = 0; i < 150; i++)
		{
			float speed = Controller.CalSpeedAccdD(Controller.ZeroCenterD + (75 - i) * 3);
			CurveTexture.SetPixel((int)(speed / 2) + 50, i, Color.yellow);
			CurveTexture.SetPixel((int)(speed / 2) + 50 + 1, i, Color.yellow);
		}
		CurveTexture.Apply();

		//update image to the texture2D
		Rect testRect = new Rect(0, 0, CurveTexture.width, CurveTexture.height);
		Vector2 testVect = new Vector2 (0.0f, 0.0f);
		Sprite curve = Sprite.Create(CurveTexture, testRect, testVect);
		img.sprite = curve;
	}

	Texture2D BmpTexture;
	public void DrawCurveBmp(bool newbmp, Color curvecolor)
	{
		int height = 1000;
		int width = 1600;
		int margin = 100;
		if (newbmp)
		{
			Destroy(BmpTexture);
			Color32[] backc = new Color32[1];
			backc[0] = new Color32(255, 255, 255, 255);
			BmpTexture = new Texture2D(width, height);
			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
					BmpTexture.SetPixels32(i, j, 1, 1, backc);

			Color32[] c = new Color32[1];
			c[0] = new Color32(180, 180, 180, 255);
			for (int i = margin; i <= width - margin; i += (width - margin * 2) / 20)
				for (int j = margin; j < height - margin; j++)
					BmpTexture.SetPixels32(i, j, 1, 1, c);
			for (int j = margin; j <= height - margin; j += (height - margin * 2) / 10)
				for (int i = margin; i < width - margin; i++)
					BmpTexture.SetPixels32(i, j, 1, 1, c);

			c[0] = new Color32(30, 30, 30, 255);
			for (int i = margin; i < width - margin; i++)
				BmpTexture.SetPixels32(i, height / 2, 1, 1, c);
			for (int j = margin; j < height - margin; j++)
				BmpTexture.SetPixels32(width / 2, j, 1, 1, c);
		}

		for (int i = margin; i < width - margin; i++)
		{
			float speed = Controller.CalSpeedAccdD(Controller.ZeroCenterD + (width / 2.0f - i) / (width / 2.0f - margin) * 200);
			BmpTexture.SetPixel(i, (int)(speed / 75 * (height / 2.0f - margin) + height / 2 + 0.5), curvecolor);
			BmpTexture.SetPixel(i, (int)(speed / 75 * (height / 2.0f - margin) + height / 2 + 0.5) + 1, curvecolor);
			BmpTexture.SetPixel(i, (int)(speed / 75 * (height / 2.0f - margin) + height / 2 + 0.5) - 1, curvecolor);
		}
		BmpTexture.Apply();
		byte[] output = BmpTexture.EncodeToPNG();

		FileStream fs = new FileStream("curve.png", FileMode.Create);
		fs.Write(output, 0, output.Length);
		fs.Close();
	}

	void OnGUI()
	{
		Destroy(ShowTexture);
		ShowTexture = Instantiate(CurveTexture) as Texture2D;
		int i = -(int)(Gesture.D - Controller.ZeroCenterD) / 3 + 75;
		if (i < 0) i = 0;
		if (i >= 150) i = 150;
		for (int j = 10; j < 90; j++)
			ShowTexture.SetPixel(j, i, Color.red);
		ShowTexture.Apply();
		Graphics.DrawTexture(new Rect(10, 10, 100, 150), ShowTexture);

		Rect testRect = new Rect(0, 0, 100, 150); //for canvas update
		Vector2 testVect = new Vector2 (0.0f, 0.0f);
		Sprite curve = Sprite.Create(ShowTexture, testRect, testVect);
		img.sprite = curve;

		for (int t = 0; t < Data.GuiTexts.Count; t++)
		{
			GuiText text = Data.GuiTexts[t];
			text.DelayMilliSeconds -= Time.smoothDeltaTime * 1000;
			if (text.DelayMilliSeconds > 0)
				continue;
			text.MilliSeconds -= Time.smoothDeltaTime * 1000;
			if (text.MilliSeconds > 0)
			{
				GUI.Label(new Rect(text.X, text.Y, 100, 100), text.Text, text.Style);
			}
			else
			{
				Data.GuiTexts.Remove(text);
				t--;
			}
		}

		if (ShowTimer)
		{
			TimeSpan timer = DateTime.Now - Data.GroupStartTime;
			GUIStyle timerstyle = new GUIStyle();
			timerstyle.fontSize = 80;
			timerstyle.normal.textColor = Color.HSVToRGB(0.1f, 0.1f, 0.2f);
			//GUI.Label(new Rect(720, 20, 200, 100), timer.Minutes.ToString("00") + ":" + timer.Seconds.ToString("00") + "." + (timer.Milliseconds / 100).ToString(), timerstyle);
			timerstyle.normal.textColor = Color.HSVToRGB(0.1f, 0.1f, 0.2f);
			string totaltn = (Data.DebugFewerTrials ? 9 : 30).ToString();
			GUI.Label(new Rect(1050, 20, 200, 100), Data.CurrentTest.Trial.ToString() + "/" + totaltn, timerstyle);
		}

		GUIStyle hiddenstyle = new GUIStyle();
		hiddenstyle.fontSize = 15;
		hiddenstyle.normal.textColor = Color.HSVToRGB(0.1f, 0.1f, 0.2f);
		GUI.Label(new Rect(10, 190, 50, 20), "Experiment: " + ((char)Data.Mode).ToString() + "-" + ((char)Data.SubMode).ToString() + "-" + ((char)(Data.GroupLabel + 0x40)).ToString(), hiddenstyle);
		GUI.Label(new Rect(10, 170, 50, 20), "Subject ID: " + Data.SubjectID.ToString(), hiddenstyle);
		//GUI.Label(new Rect(10, 200, 50, 20), (Gesture.D - Controller.ZeroCenterD).ToString() + " - " + SelfMove.Speed.ToString(), hiddenstyle);

		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		GUI.Label(new Rect(10, 210, 50, 20), "FPS: " + fps.ToString("0"), hiddenstyle);
	}
}
