using UnityEngine;
using System;
using System.Collections.Generic;

public class s_Data : MonoBehaviour
{
	public Tests Tests;
	public List<GuiText> GuiTexts;

	// Mode == '1': only road, no target
	// Mode == '2': endless random target
	// Mode == '3': a group of target with fixed parameters that are common
	// Mode == '4': subject being able to swtich between ABC groups to feel the differences between them before the real experiment
	// Mode == 'd': deadzone experiment, 6 groups with different deadzone width
	// Mode == 's': sensitivity experiment, 6 groups with different sensitivity
	// Mode == 'x': complex sensitivity experiment, 6 groups with different complex sensitivity (or curve shape)
	public char Mode;

	// same as Mode, but only used for practice before ABC groups, i.e. in '4' Mode.
	public char SubMode;

	public int SubjectID;
	public char Step;
	public int Condition;
	public int GroupLabel;
	public int PauseMoving;
	public int[] ConditionOrder;
	public DateTime GroupStartTime;

	public Tests CurrentTest;
	public Tests BlankTest;

	public bool DebugFewerTrials = false;

	public s_Data()
	{
		GuiTexts = new List<GuiText>();
		BlankTest = new Tests();
		ConditionOrder = new int[6];
	}

	public void Start()
	{
	}

	public void BlockRandomize(int id, char mode) //randomizes the order of the conditions a, b, c; should not need to touch
	{
		int rn = (id + mode) % 6;

		switch (rn)
		{
			case 0:
				ConditionOrder[1] = 2;
				ConditionOrder[2] = 3;
				ConditionOrder[3] = 1;
				break;
			case 1:
				ConditionOrder[1] = 3;
				ConditionOrder[2] = 1;
				ConditionOrder[3] = 2;
				break;
			case 2:
				ConditionOrder[1] = 1;
				ConditionOrder[2] = 3;
				ConditionOrder[3] = 2;
				break;
			case 3:
				ConditionOrder[1] = 3;
				ConditionOrder[2] = 2;
				ConditionOrder[3] = 1;
				break;
			case 4:
				ConditionOrder[1] = 1;
				ConditionOrder[2] = 2;
				ConditionOrder[3] = 3;
				break;
			case 5:
				ConditionOrder[1] = 2;
				ConditionOrder[2] = 1;
				ConditionOrder[3] = 3;
				break;
		}
	}

	public int[] DistanceRandomize(int id, char mode) //for distances
	{
		int trialnum = DebugFewerTrials ? 9 : 30;
		int warmupnum = 6;
		int[] distances = new int[trialnum];
		int[] count = new int[3];
		for (int r = 0; r < 3; r++)
			count[r] = 0;
		System.Random rm = new System.Random(id * 100 + mode);
		for (int i = 0; i < warmupnum; i++)
		{
            if (mode == 'x')
                //distances[i] = 15 - (i % 3) * 6;
                distances[i] = 27 - (i % 3) * 12;
            else
                //distances[i] = (i % 3) * 6 + 3;
                distances[i] = (i % 3) * 12 + 3;
		}
		int mostr = 1;
		for (int i = warmupnum; i < trialnum; i++)
		{
			int r;
			do
			{
				do
				{
					r = rm.Next(0, 2 + 1);  // the upper boundry is exclusive
				}
				while (r == mostr && rm.Next(0, 10) > 0);
			}
			while (count[r] >= (trialnum - warmupnum) / 3);
			count[r]++;
            //distances[i] = r * 6 + 3;
            distances[i] = r * 12 + 3;
			int maxr = 0;
			for (int ri = 0; ri < 3; ri++)
			{
				if (count[ri] > maxr)
				{
					mostr = ri;
					maxr = count[ri];
				}
			}
		}
		return distances;
	}

	public void NewGroupOfCommonTests()
	{
		float targetdiameter = 2f;
		Tests = new Tests();
		int[] distances = DistanceRandomize(SubjectID, Mode);
		for (int j = 0; j < distances.Length; j++)
		{
			Tests subtest_2 = new Tests();
			subtest_2.SubjectID = SubjectID;
			subtest_2.Mode = '3';
			subtest_2.GroupLabel = 0;
			subtest_2.Condition = 0;
			subtest_2.Trial = j;
			subtest_2.DeadZoneRadius = 25f;
			subtest_2.Sensitivity = 21f;
			subtest_2.CurveShapeExp = 1.0f;
			subtest_2.TargetDiameter = targetdiameter;
			subtest_2.Distance = distances[j];
			Tests.AddTests(subtest_2);
		}
	}

	public void NewGroupOfTests(char mode, char submode, int condition, int grouplabel)
	{
		Tests defaultvalues = new Tests();
		float deadzoneradius = defaultvalues.DeadZoneRadius;
		float sensitivity = defaultvalues.Sensitivity;
		float curveshapeexp = defaultvalues.CurveShapeExp;

		char themode = '0';
		float targetdiameter = 2f;
		if (mode == 'd' || mode == 's' || mode == 'x')
		{
			themode = mode;
			if ((SubjectID + mode + submode) % 2 == 0)
				targetdiameter = 2f;
			else
				targetdiameter = 1f;
		}
		else if (mode == '4')
		{
			themode = submode;
			targetdiameter = 2f;
		}

		//this is where the different levels are set (changing them will change parameters in game)
		if (themode == 'd' && condition == 1)
			deadzoneradius = 10f;
		else if (themode == 'd' && condition == 2)
			deadzoneradius = 25f;
		else if (themode == 'd' && condition == 3)
			deadzoneradius = 40f;
		else if (themode == 's' && condition == 1)
			sensitivity = 14f;
		else if (themode == 's' && condition == 2)
			sensitivity = 21f;
		else if (themode == 's' && condition == 3)
			sensitivity = 30f;
		else if (themode == 'x' && condition == 1)
			curveshapeexp = 0.5f;
		else if (themode == 'x' && condition == 2)
			curveshapeexp = 1.0f;
		else if (themode == 'x' && condition == 3)
			curveshapeexp = 1.7f;

		Tests = new Tests();
		int[] distances = DistanceRandomize(SubjectID, themode);
		for (int j = 0; j < distances.Length; j++)
		{
			Tests subtest_2 = new Tests();
			subtest_2.SubjectID = SubjectID;
			subtest_2.Mode = themode;
			subtest_2.GroupLabel = grouplabel;
			subtest_2.Condition = condition;
			subtest_2.Trial = j;
			subtest_2.DeadZoneRadius = deadzoneradius;
			subtest_2.Sensitivity = sensitivity;
			subtest_2.CurveShapeExp = curveshapeexp;
			subtest_2.TargetDiameter = targetdiameter;
			subtest_2.Distance = distances[j];
			Tests.AddTests(subtest_2);
		}
	}

	public void Record(int combine = 1)
	{
		System.IO.FileStream fs = null;
		System.IO.StreamWriter sw = null;
		string head = "Subject #,Experiment,Group,Condition,Trial,DZ Radius,Sensitivity,Curve Shape,Target Distance,Target Diameter,Click Hit,Click Distance,Overshot,Farthest Hand Position,Click Time";
		if (combine == 0)
		{
			System.DateTime Now = System.DateTime.Now;
			fs = new System.IO.FileStream("record\\record_time_" + Now.Day.ToString("00") + "-" + Now.Hour.ToString("00") + Now.Minute.ToString("00") + Now.Second.ToString("00") + ".txt", System.IO.FileMode.CreateNew);
			sw = new System.IO.StreamWriter(fs);
			sw.WriteLine(head);
		}
		else if (combine == 1)
		{
			string filepath = "record\\record_subject_" + SubjectID.ToString() + ".txt";
			if (!System.IO.File.Exists(filepath))
			{
				fs = new System.IO.FileStream(filepath, System.IO.FileMode.CreateNew);
				sw = new System.IO.StreamWriter(fs);
				sw.WriteLine(head);
			}
			else
			{
				fs = new System.IO.FileStream(filepath, System.IO.FileMode.Append);
				sw = new System.IO.StreamWriter(fs);
			}
			
		}
		Tests tp;
		while ((tp = Tests.GetNextTest()) != null)
		{
			sw.Write(tp.SubjectID);
			sw.Write(",");
			sw.Write((char)tp.Mode);
			sw.Write(",");
			sw.Write((char)(tp.GroupLabel + 0x40));
			sw.Write(",");
			sw.Write(tp.Condition);
			sw.Write(",");
			sw.Write(tp.Trial);
			sw.Write(",");
			sw.Write(tp.DeadZoneRadius);
			sw.Write(",");
			sw.Write(tp.Sensitivity);
			sw.Write(",");
			sw.Write(tp.CurveShapeExp);
			sw.Write(",");

			sw.Write(tp.Distance.ToString("0.00"));
			sw.Write(",");
			sw.Write(tp.TargetDiameter.ToString("0.00"));
			sw.Write(",");

			sw.Write(tp.Hit);
			sw.Write(",");
			sw.Write(tp.ClickDistance.ToString("0.00"));
			sw.Write(",");
			sw.Write(tp.OverShot.ToString());
			sw.Write(",");
			sw.Write(tp.MaxXReached.ToString("0.00"));
			sw.Write(",");
			sw.Write((tp.ClickTime - tp.StartTime).TotalMilliseconds.ToString("0"));
			sw.Write("\r\n");
		}
		sw.Close();
		fs.Close();

		if (combine == 1)
			Record(0);
	}
}
