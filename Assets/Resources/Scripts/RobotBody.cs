using UnityEngine;
using System.Collections;

public class RobotBody : MonoBehaviour {

  private const int FORCE_MAGNITUDE = 8;

	void FixedUpdate () {
    if (Input.GetKey(KeyCode.UpArrow))
      GetComponent<Rigidbody>().AddForce(FORCE_MAGNITUDE * Vector3.forward);
    if (Input.GetKey(KeyCode.DownArrow))
      GetComponent<Rigidbody>().AddForce(FORCE_MAGNITUDE * Vector3.back);
    if (Input.GetKey(KeyCode.LeftArrow))
      GetComponent<Rigidbody>().AddForce(FORCE_MAGNITUDE * Vector3.left);
    if (Input.GetKey(KeyCode.RightArrow))
      GetComponent<Rigidbody>().AddForce(FORCE_MAGNITUDE * Vector3.right);
    if (Input.GetKey(KeyCode.Space))
      GetComponent<Rigidbody>().AddForce(-30 * FORCE_MAGNITUDE * transform.up);
	
	}
}
