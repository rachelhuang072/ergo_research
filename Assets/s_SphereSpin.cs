using UnityEngine;
using System.Collections;

public class s_SphereSpin : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		float Amount = Time.smoothDeltaTime * 20;
		transform.Rotate(0, Amount, 0);
	}
}
