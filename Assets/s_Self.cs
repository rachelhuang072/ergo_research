
using UnityEngine;
using System.Collections;
using System;

public class s_Self : MonoBehaviour
{
	s_Data Data;
	public float Speed = 0;
	public float Rotate = 0;

	s_Controller Controller;

	// Use this for initialization
	void Start ()
	{
		Data = GameObject.Find("Data").GetComponent<s_Data>();
		if (Data == null) Debug.Log("herenull");
		Controller = GameObject.Find("Controller").GetComponent<s_Controller>();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Q))
		{
			Data.PauseMoving = 1 - Data.PauseMoving;
		}

		if (Data.PauseMoving == 0 && Speed != 0) 
		{
			float moveAmount = Time.smoothDeltaTime * Speed;
			transform.Translate (0f, 0f, moveAmount, Space.World);
		} 

		else if (Controller.Ratcheting) 
		{
			if (Speed != 0) {
				float moveAmount = Time.smoothDeltaTime * Speed;
				if (Math.Abs (transform.rotation.y) > 0.9f) {
					transform.Translate (0f, 0f, -moveAmount, Space.World);
				} else {
					transform.Translate (0f, 0f, moveAmount, Space.World);
				}
//				Debug.Log (transform.rotation.y);
			} else if (Rotate != 0) { //maybe change the color of the line to notify a turn
				transform.Rotate (0f, Rotate, 0f); //when turned around, pulling forward is going backward...
			}
		}
	}
}























