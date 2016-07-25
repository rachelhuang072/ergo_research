using UnityEngine;
using System;
using System.Collections.Generic;

public class Tests
{
	protected List<Tests> SubTests = null;
	protected int SubTestsPointer = 0;

	public int SubjectID;
	public int Mode;
	public int Condition;
	public int GroupLabel;
	public int Trial;

	// important: the default parameter values is indeed here
	public float DeadZoneRadius = 25f;
	public float CurveShapeExp = 1.0f;
	public float Sensitivity = 21f;

	public float TargetDiameter;
	public float Distance;

	public DateTime StartTime;
	public DateTime ClickTime;
	public int Hit = -1;
	public float ClickDistance = -1;
	public int OverShot = 0;
	public float MaxXReached = 0;

	public int AddTests(Tests tests)
	{
		if (SubTests == null)
			SubTests = new List<Tests>();
		SubTests.Add(tests);
		return 0;
	}

	public Tests GetTests(params int[] index)
	{
		int level = index.Length;
		if (level == 0)
			return this;
		else
		{
			int[] subindex = new int[level - 1];
			for (int i = 1; i < level; i++)
				subindex[i - 1] = index[i];
			return SubTests[index[0]].GetTests(subindex);
		} 
	}

	public Tests GetNextTest()
	{
		if (SubTests == null)
		{
			if (SubTestsPointer == 0)
			{
				SubTestsPointer++;
				return this;
			}
			else
			{
				SubTestsPointer = 0;
				return null;
			}
		}
		else
		{
			if (SubTestsPointer < SubTests.Count)
			{
				Tests subtests = SubTests[SubTestsPointer].GetNextTest();
				if (subtests != null)
				{
					return subtests;
				}
				else
				{
					SubTestsPointer++;
					return GetNextTest();
				}
			}
			else
			{
				SubTestsPointer = 0;
				return null;
			}
		}
	}
}
