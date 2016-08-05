using UnityEngine;
using System;
using System.Collections;

public class s_Controller : MonoBehaviour
{
	private float FixSpotLength = 45;
	private float MaxSpotLength = 95;
	private DateTime LastHit;
	private DateTime LastNonHit;
	private Vector3 offset;
	public float ZeroCenterD;
	public float MaxAngle;
	public float MinAngle;
	public bool Ratcheting = false; //use later to trigger ratcheting vs. velocity
	public float DZRadius;

	int CountDownMilliSeconds;

	Tests CurrentTest;

	s_Data Data;
	s_Gesture Gesture;
	s_Self SelfMove;
	s_Gui Gui;
	s_Sound Sound;
	GameObject Self;
	GameObject Target;
	Canvas canvas;

	bool Clicked = false;

	public float CalSpeedAccdD(float d)
	{
		float speed = 0;
		float t;
		if (d < ZeroCenterD - CurrentTest.DeadZoneRadius) //moved out of deadzone, forward motion
		{
//			t = (float) (((ZeroCenterD - CurrentTest.DeadZoneRadius) - d) * 1.25);
			t = ((ZeroCenterD - CurrentTest.DeadZoneRadius) - d);
			if (t > MaxSpotLength) t = MaxSpotLength;
			t /= FixSpotLength;
			speed = Mathf.Pow(t, CurrentTest.CurveShapeExp) * CurrentTest.Sensitivity;
		}
		else if (d > ZeroCenterD + CurrentTest.DeadZoneRadius) //moved out of deadzone, backward motion
		{
//			t = (float) ((d - (ZeroCenterD + CurrentTest.DeadZoneRadius)) * 1.25);
			t = (d - (ZeroCenterD + CurrentTest.DeadZoneRadius));
			if (t > MaxSpotLength) t = MaxSpotLength;
			t /= FixSpotLength;
//			t *= 0.7f; //could make this another testing parameter (if backward speed should be slower or not)
			speed = -Mathf.Pow(t, CurrentTest.CurveShapeExp) * CurrentTest.Sensitivity;
		}
//		if (d > ZeroCenterD + CurrentTest.DeadZoneRadius + MaxSpotLength) //ceiling?
//			speed = 0;
		return speed;
	}

	void SetSpeedAccdD()
	{
		if (Ratcheting) {
			if (Gesture.RatchetRotate != 0f) {
				SelfMove.Speed = 0f;
				SelfMove.Rotate = Gesture.RatchetRotate;
			} else {
				SelfMove.Speed = Gesture.RatchetSpeed;
				SelfMove.Rotate = 0f;
			}
		} else {
			float speed = CalSpeedAccdD(Gesture.D); //change how Gesture.D is calculated to LeapMotion
			SelfMove.Speed = speed;
		}
	}

	void ResetMode(char mode, char submode = '0')
	{
		Gui.ClearAllTexts();
		Data.Mode = mode;
		Data.SubMode = submode;
		LastHit = default (DateTime);
		LastNonHit = default (DateTime);

		if (mode == '1') //only road
		{
			Data.PauseMoving = 1;
			Data.NewGroupOfCommonTests();
			StartNextTest();
			Gui.ShowTimer = false;
			Target.transform.Translate(-10, -10, -10);
		}
		else if (mode == '2') //endless targets for practice
		{
			Data.PauseMoving = 1;
			Data.NewGroupOfCommonTests();
			Data.CurrentTest = Data.Tests.GetTests(0);
			CurrentTest = Data.CurrentTest;
			StartNextTest();
			Gui.ShowTimer = false;
		}
		else if (mode == '3') //group practice
		{
			Data.PauseMoving = 0;
			Data.NewGroupOfCommonTests();
			Data.CurrentTest = Data.Tests.GetTests(0);
			CurrentTest = Data.CurrentTest;
			Gui.RedrawCurve();
			Gui.DrawText("Ready for a Practice Group?", 1000000);
			Data.Step = '1';
			Gui.ShowTimer = false;
			Target.transform.Translate(-10, -10, -10);
		}
		else if (Data.Mode == '4') //warmup for conditions s, d, x
		{
			Data.PauseMoving = 0;
			Data.BlockRandomize(Data.SubjectID, Data.SubMode);

			Data.GroupLabel = 1;
			Data.Condition = Data.ConditionOrder[Data.GroupLabel];
			Data.NewGroupOfTests(Data.Mode, Data.SubMode, Data.Condition, Data.GroupLabel);
			Data.CurrentTest = Data.Tests.GetTests(0);
			CurrentTest = Data.CurrentTest;
			Gui.RedrawCurve();
			StartNextTest();
			Gui.DrawText("Testing with Condition A", 10000000);
			Gui.ShowTimer = false;
		}
		else if (mode == 'd' || mode == 's' || mode == 'x') //real experiment, collect data
		{
			Data.PauseMoving = 0;
			Data.Step = '1';
			Data.ConditionOrder = new int[6];

			// randomize the order of 3 groups
			//System.Random rm = new System.Random(DateTime.Now.Millisecond + DateTime.Now.Second * 1000);
			//int rn = rm.Next(1, 6);
			// block-randomize the order of 3 groups with regard to Subject ID
			Data.BlockRandomize(Data.SubjectID, Data.Mode);

			Data.GroupLabel = 1;
			Data.Condition = Data.ConditionOrder[Data.GroupLabel];
			Data.NewGroupOfTests(Data.Mode, Data.SubMode, Data.Condition, Data.GroupLabel);
			Data.CurrentTest = Data.Tests.GetTests(0);
			CurrentTest = Data.CurrentTest;
			Gui.RedrawCurve();
			Gui.ShowTimer = false;
			string modetext = "";
			if (Data.Mode == 'd') modetext = "Dead Zone Width";
			if (Data.Mode == 's') modetext = "Simple Sensitivity";
			if (Data.Mode == 'x') modetext = "Complex Sensitivity";
			Gui.DrawText("3 Groups with Variable " + modetext, 2000);
			Gui.DrawText("Ready for Group A?", 1000000, 2000);
			Target.transform.Translate(-10, -10, -10);
		}
	}

	int TargetHit() {
		if (Data.Mode == '1')
			return 0;
		if (Data.Mode == '3' && Data.Step != '3')
			return 0;
		if ((Data.Mode == 'd' || Data.Mode == 's' || Data.Mode == 'x') && Data.Step != '3')
			return 0;
		
		if ((Self.transform.position.z > Target.transform.position.z - CurrentTest.TargetDiameter / 2 &&
		    Self.transform.position.z < Target.transform.position.z + CurrentTest.TargetDiameter / 2)) {
			if (LastHit == default(DateTime)) {
				LastHit = DateTime.Now;
//				Debug.Log ("entering target zone");
				return 0;
			} else if ((DateTime.Now - LastHit).TotalMilliseconds >= 500) { //wait half a second in target
                CurrentTest.ClickTime = DateTime.Now;
                CurrentTest.ClickDistance = Self.transform.position.z - Target.transform.position.z;
                Sound.PlayHit ();
				CurrentTest.Hit = 1;
				LastHit = new DateTime ();
				StartNextTest ();
				return 1;
			}
            return 0;
		} else {
			LastHit = new DateTime ();
//			Debug.Log ("not in the target zone");
		}

        //StartNextTest ();
		return 0;
	}


	int Click()
	{
		if (Data.Mode == '1')
			return 0;
		if (Data.Mode == '3' && Data.Step != '3')
			return 0;
		if ((Data.Mode == 'd' || Data.Mode == 's' || Data.Mode == 'x') && Data.Step != '3')
			return 0;

//		Debug.Log (DateTime.Now);
//		Debug.Log (LastHit);
//		Debug.Log ((DateTime.Now - LastHit).TotalMilliseconds);
//		if ( LastHit == default(DateTime) || (DateTime.Now - LastHit).TotalMilliseconds > 1000) 
//		{
		if ((Mathf.Abs (ZeroCenterD - Gesture.D) > CurrentTest.DeadZoneRadius + 2) &&
		    (LastNonHit == default(DateTime) || (DateTime.Now - LastNonHit).TotalMilliseconds > 900) &&
			(DateTime.Now - LastHit).TotalMilliseconds > 500) {
			//Debug.Log ("trying to click when not standing still");
			Sound.PlayNotHit ();
			LastNonHit = DateTime.Now;
			return -2;
		} else {
			CurrentTest.ClickTime = DateTime.Now;
			CurrentTest.ClickDistance = Self.transform.position.z - Target.transform.position.z;
			if ((Self.transform.position.z > Target.transform.position.z - CurrentTest.TargetDiameter / 2 &&
			   Self.transform.position.z < Target.transform.position.z + CurrentTest.TargetDiameter / 2) &&
			   (LastHit == default(DateTime) || (DateTime.Now - LastHit).TotalMilliseconds > 900) &&
				(DateTime.Now - LastNonHit).TotalMilliseconds > 500) {
				Sound.PlayHit ();
				CurrentTest.Hit = 1;
				Gesture.Closed = false;
				LastHit = DateTime.Now;
				StartNextTest ();
			} else {
				if ((LastNonHit == default(DateTime) || (DateTime.Now - LastNonHit).TotalMilliseconds > 900) &&
				   (DateTime.Now - LastHit).TotalMilliseconds > 500) {
					Debug.Log ("else case for not clicking correctly");
					Sound.PlayNotHit ();
					LastNonHit = DateTime.Now;
					return -1;
					CurrentTest.Hit = 0;
				}
			}
		}

//		StartNextTest();
		return 0;
	}

	void StartNextTest()
	{
		Data.CurrentTest = Data.Tests.GetNextTest();
		CurrentTest = Data.CurrentTest;
		if (CurrentTest != null)
		{
			// scale and show the next target

			Target.transform.position = Self.transform.position;
			Target.transform.Translate(0, 0, CurrentTest.Distance, Space.World);

			if (Target.name == "Target_wiredbubble")
			{
				float scale = CurrentTest.TargetDiameter;
				Target.transform.FindChild("Sphere").localScale = new Vector3(scale, scale, scale);
				Target.transform.FindChild("Sphere_inner").localScale = new Vector3(scale, scale, scale);
				Target.transform.FindChild("Hurdle").localPosition = new Vector3(0, 0, scale / 2);
			}
			else if (Target.name == "Target_lines")
			{
				float scale = CurrentTest.TargetDiameter;
				Target.transform.FindChild("Sphere").localScale = new Vector3(scale, scale, scale);
				Target.transform.FindChild("Sphere_inner").localScale = new Vector3(scale, scale, scale);
				Target.transform.FindChild("Sphere").localPosition = new Vector3(0, scale / 2, 0);
				Target.transform.FindChild("Sphere_inner").localPosition = new Vector3(0, scale / 2, 0.01f);
				Target.transform.FindChild("Particle Line 1").localPosition = new Vector3(-2, 0.3f, -CurrentTest.TargetDiameter / 2);
				Target.transform.FindChild("Particle Line 2").localPosition = new Vector3(-2, 0.3f, CurrentTest.TargetDiameter / 2);
			}
			else if (Target.name == "Target_torus")
			{
				float scale = CurrentTest.TargetDiameter;
				Target.transform.FindChild("Sphere").localScale = new Vector3(scale, scale, scale);
				Target.transform.FindChild("Sphere_inner").localScale = new Vector3(scale, scale, scale);
				Target.transform.FindChild("Torus").localScale = new Vector3(scale, scale * 0.6f, scale);
			}

			Target.SetActive(true);
			Gui.RedrawCurve();
			CurrentTest.StartTime = DateTime.Now;
		}
		else
		{
			// what happens when all the targets in a group is done

			if (Data.Mode == '1')
			{
			}
			else if (Data.Mode == '2')
			{
				Data.NewGroupOfCommonTests();
				StartNextTest();
			}
			else if (Data.Mode == '3')
			{
				Data.CurrentTest = Data.Tests.GetTests(0);
				CurrentTest = Data.CurrentTest;
				Gui.ShowTimer = false;
				TimeSpan timer = DateTime.Now - Data.GroupStartTime;
				Gui.DrawText("Time: " + timer.Minutes.ToString("00") + ":" + timer.Seconds.ToString("00") + "." + (timer.Milliseconds / 100).ToString(), 1000000);
				Gui.DrawText("Practice Group Done.", 1000000, 0, 60, 140, 80);
				Data.Step = '4';
				Target.transform.Translate(-10, -10, -10);
			}
			else if (Data.Mode == '4')
			{
				Data.NewGroupOfTests(Data.Mode, Data.SubMode, Data.Condition, Data.GroupLabel);
				StartNextTest();
			}
			else if ((Data.Mode == 'd' || Data.Mode == 's' || Data.Mode == 'x'))
			{
				Data.Record();
				Data.GroupLabel++;
				Data.Condition = Data.ConditionOrder[Data.GroupLabel];
				if (Data.GroupLabel < 4)
				{
					Data.Step = '1';
					Data.NewGroupOfTests(Data.Mode, Data.SubMode, Data.Condition, Data.GroupLabel);
					Data.CurrentTest = Data.Tests.GetTests(0);
					CurrentTest = Data.CurrentTest;
					Gui.RedrawCurve();
					Gui.ShowTimer = false;
					Gui.ClearAllTexts();
					TimeSpan timer = DateTime.Now - Data.GroupStartTime;
					Gui.DrawText("Time: " + timer.Minutes.ToString("00") + ":" + timer.Seconds.ToString("00") + "." + (timer.Milliseconds / 100).ToString(), 1000000);
                    Gui.DrawText("Ready for Group " + ((char)(Data.GroupLabel + 0x40)).ToString() + "?", 1000000, 0, 60, 140, 80);
					Target.transform.Translate(-10, -10, -10);
				}
				else
				{
					Data.Step = '4';
					Data.CurrentTest = Data.Tests.GetTests(0);
					CurrentTest = Data.CurrentTest;
					Gui.ShowTimer = false;
					Gui.ClearAllTexts();
					TimeSpan timer = DateTime.Now - Data.GroupStartTime;
					Gui.DrawText("Time: " + timer.Minutes.ToString("00") + ":" + timer.Seconds.ToString("00") + "." + (timer.Milliseconds / 100).ToString(), 1000000);
					Gui.DrawText("All Groups Done.", 1000000, 0, 60, 140, 80);
					Gui.DrawText("Please Answer the Questionnaire.", 1000000, 0, 60, 140, 140);
					Data.PauseMoving = 1;
					Target.transform.Translate(-10, -10, -10);
				}
			}
		}
	}

	// Use this for initialization
	void Start ()
	{
		Data = GameObject.Find("Data").GetComponent<s_Data>();
		Gesture = GameObject.Find("Gesture").GetComponent<s_Gesture>();
		Gui = GameObject.Find("Gui").GetComponent<s_Gui>();
		Sound = GameObject.Find("Sound").GetComponent<s_Sound>();
		Self = GameObject.FindWithTag("Self");
		canvas = GameObject.Find ("Canvas").GetComponent<Canvas> ();
		offset = canvas.transform.position - Self.transform.position;
//		Debug.Log (Self);
		if (Self == null)
			Debug.Log("Self not found.");
		SelfMove = Self.GetComponent<s_Self>();	
		Target = GameObject.FindWithTag("Target");
		if (Target == null)
			Debug.Log("Target not found");
		ResetMode('1');
<<<<<<< HEAD
        ZeroCenterD = -100f;
=======
		ZeroCenterD = -100f;
>>>>>>> origin/master
	}
	
	// Update is called once per frame
	void Update ()
	{
		SetSpeedAccdD();
		Clicked = false;
		DZRadius = CurrentTest.DeadZoneRadius;

		canvas.transform.position = Self.transform.position + offset;

		if (Self.transform.position.z > Target.transform.position.z + CurrentTest.TargetDiameter / 2)
			CurrentTest.OverShot = 1;
		if (ZeroCenterD - Gesture.D > CurrentTest.MaxXReached)
			CurrentTest.MaxXReached = ZeroCenterD - Gesture.D;

		if (Input.GetKey(KeyCode.UpArrow))
		{
			float shift = Input.GetKey(KeyCode.LeftShift) ? 0.1f : 0.5f;
			float speed = CurrentTest.Sensitivity * shift;
			SelfMove.Speed = speed;
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			float shift = Input.GetKey(KeyCode.LeftShift) ? 0.1f : 0.5f;
			float speed = -CurrentTest.Sensitivity * shift;
			SelfMove.Speed = speed;
		}

		if (Mathf.Abs (ZeroCenterD - Gesture.D) < CurrentTest.DeadZoneRadius + 2) 
		{
//			Click();
			TargetHit();
		}
		if (Input.GetKey(KeyCode.Z) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
		{
			ZeroCenterD = Gesture.D;
            //Debug.Log(ZeroCenterD);
//			ZeroCenterD = (MaxAngle + MinAngle) * 0.5f;
//			ZeroCenterD = (MaxAngle + MinAngle) / 2;
		}

		//ratcheting controls
		if (Input.GetKey(KeyCode.R) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
		{
			Ratcheting = true;
			canvas.enabled = false;
		}
		if (Input.GetKey(KeyCode.E) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
		{
			Ratcheting = false;
			canvas.enabled = true;
		}
		if (Ratcheting && !Gesture.Closed) 
		{
			TargetHit ();
		}

		if (Data.Mode == '3' && Data.Step == '1')
		{
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
			{
				Sound.Play5to1();
				//Gui.CancelText("Ready for a Practice Group?");
				Gui.ClearAllTexts();
				Data.Step = '2';
				CountDownMilliSeconds = 4800;
			}
		}
		else if (Data.Mode == '3' && Data.Step == '2')
		{
			CountDownMilliSeconds -= (int)(Time.deltaTime * 1000);
			if (CountDownMilliSeconds < 0 && ((Mathf.Abs(Gesture.D - ZeroCenterD) <= CurrentTest.DeadZoneRadius) || Ratcheting))
			{
				Sound.PlayLetsGo();
				Data.GroupStartTime = DateTime.Now;
				Gui.ShowTimer = true;
				Data.Step = '3';
				StartNextTest();
			}
		}
		else if (Data.Mode == '4')
		{
			if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.C))
			{
				if (Input.GetKeyDown(KeyCode.A))
				{
					Data.GroupLabel = 1;
				}
				else if (Input.GetKeyDown(KeyCode.B))
				{
					Data.GroupLabel = 2;
				}
				else if (Input.GetKeyDown(KeyCode.C))
				{
					Data.GroupLabel = 3;
				}
				Data.Condition = Data.ConditionOrder[Data.GroupLabel];
				Data.NewGroupOfTests(Data.Mode, Data.SubMode, Data.Condition, Data.GroupLabel);
				Data.CurrentTest = Data.Tests.GetTests(0);
				CurrentTest = Data.CurrentTest;
				Gui.ClearAllTexts();
				Gui.RedrawCurve();
				StartNextTest();
				Gui.DrawText("Testing with Condition " + (char)(Data.GroupLabel + 0x40), 10000000);
			}
		}
		else if ((Data.Mode == 'd' || Data.Mode == 's' || Data.Mode == 'x') && Data.Step == '1')
		{
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
			{
				Sound.Play5to1();
				//Gui.CancelText("Ready for Group " + ((char)(Data.GroupLabel + 0x40)).ToString() + "?");
				Gui.ClearAllTexts();
				Gui.DrawText("Group " + ((char)(Data.GroupLabel + 0x40)).ToString(), 300000);
				Data.Step = '2';
				CountDownMilliSeconds = 4800;
			}
		}
		else if ((Data.Mode == 'd' || Data.Mode == 's' || Data.Mode == 'x') && Data.Step == '2')
		{
			CountDownMilliSeconds -= (int)(Time.deltaTime * 1000);
			if (CountDownMilliSeconds < 0 && Mathf.Abs(Gesture.D - ZeroCenterD) <= CurrentTest.DeadZoneRadius)
			{
				Sound.PlayLetsGo();
				Data.GroupStartTime = DateTime.Now;
				Gui.ShowTimer = true;
				Data.Step = '3';
				StartNextTest();
			}
		}
		else if ((Data.Mode == 'd' || Data.Mode == 's' || Data.Mode == 'x') && Data.Step == '4')
		{
			if (Data.SubMode == '1')
				if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
				{
					ResetMode(Data.Mode, '2');
				}
		}

		if (Input.GetKeyDown(KeyCode.N) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
		{
			Data.SubjectID++;
		}
		else if (Input.GetKeyDown(KeyCode.M) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
		{
			Data.SubjectID--;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha1) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
		{
			ResetMode('1');
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
		{
			ResetMode('2');
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
		{
			ResetMode('3');
		}
		else if (Input.GetKey(KeyCode.Alpha4) && Input.GetKeyDown(KeyCode.D))
		{
			ResetMode('4', 'd');
		}
		else if (Input.GetKey(KeyCode.Alpha4) && Input.GetKeyDown(KeyCode.S))
		{
			ResetMode('4', 's');
		}
		else if (Input.GetKey(KeyCode.Alpha4) && Input.GetKeyDown(KeyCode.X))
		{
			ResetMode('4', 'x');
		}
		else if (Input.GetKeyDown(KeyCode.D) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
		{
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				ResetMode('d', '2');
			else
				ResetMode('d', '1');
		}
		else if (Input.GetKeyDown(KeyCode.S) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
		{
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				ResetMode('s', '2');
			else
				ResetMode('s', '1');
		}
		else if (Input.GetKeyDown(KeyCode.X) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
		{
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				ResetMode('x', '2');
			else
				ResetMode('x', '1');
		}
		else if
		(
			Input.GetKeyDown(KeyCode.T) &&
			(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
			(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) &&
			(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		)
		{
			Data.DebugFewerTrials = !Data.DebugFewerTrials;
		}

		else if (Input.GetKeyDown(KeyCode.LeftBracket))
		{
			Gui.DrawCurveBmp(true, Color.HSVToRGB(0f, 0.8f, 0.8f));
		}
		else if (Input.GetKeyDown(KeyCode.RightBracket))
		{
			Gui.DrawCurveBmp(false, Color.HSVToRGB(0.4f, 0.8f, 0.8f));
		}
		else if (Input.GetKeyDown(KeyCode.Minus))
		{
			Gui.DrawCurveBmp(false, Color.HSVToRGB(0.8f, 0.8f, 0.8f));
		}

		if (Self.transform.position.z >= 22.8f)
		{
			Self.transform.Translate(0, 0, -22.8f, Space.World);
			Target.transform.Translate(0, 0, -22.8f, Space.World);
		}
		else if (Self.transform.position.z < 0)
		{
			Self.transform.Translate(0, 0, 22.8f, Space.World);
			Target.transform.Translate(0, 0, 22.8f, Space.World);
		}
	}
}
