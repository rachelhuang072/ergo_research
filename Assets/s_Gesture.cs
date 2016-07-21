using UnityEngine;
using Leap;
using System;

public class s_Gesture : MonoBehaviour
{
	s_Gui Gui;
	s_Controller Controller;
	Controller m_leapController;

	public int X, Y;
	public float D;
	public bool Closed;
	public float GestureDist;
	public float GestureAngle;
	public float scaleMovement = 0.5f;
	public float RatchetSpeed;
	public float RatchetRotate = 0f;

	bool CameraConnected = true;
	bool m_twoHandGrabLastFrame = false;
	bool m_oneHandGrabLastFrame = false;
	float m_angleLastFrame;
	float m_marginOfError = 15.5f;
	float m_averageDiff = 55.5f;

	Vector m_lastRightPos;
	Vector m_lastLeftPos;
	Vector centerRight;
	Vector centerLeft;
	float prev_d;

	float[] angles = new float[10];
	int ptr = 0;
	float dz_min = 0f;
	float dz_max;
	int ext_to_stop = 0;
	int curl_to_stop = 0;

	private bool flag_initialized_ = false;
	public bool isHeadMounted = false;

	// Use this for initialization
	void Start ()
	{
		Gui = GameObject.Find("Gui").GetComponent<s_Gui>();
		m_leapController = new Controller();
		Closed = false;
		Controller = GameObject.Find("Controller").GetComponent<s_Controller>();

	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!CameraConnected)
			return;
		Frame frame = m_leapController.Frame ();
		if (frame.Hands.Count > 0) {
			Hand hand = frame.Hands [0];
			Vector palm_dist = hand.PalmPosition;

//			FingerList indexes = hand.Fingers.FingerType(Finger.FingerType.TYPE_INDEX);
            Finger index = hand.Fingers[(int)Finger.FingerType.TYPE_INDEX];
//			FingerList thumbs = hand.Fingers.FingerType(Finger.FingerType.TYPE_THUMB);
            Finger thumb = hand.Fingers[(int)Finger.FingerType.TYPE_THUMB];
            //			FingerList middles = hand.Fingers.FingerType (Finger.FingerType.TYPE_MIDDLE);
            Finger middle = hand.Fingers[(int)Finger.FingerType.TYPE_MIDDLE];
            //			Debug.Log (thumbs.Count);

            if (Controller.Ratcheting) { //scene-in-hand/grab-and-drag/ratcheting 
				HandRatchet (frame);
			}
			else if (index != null) { //velocity control

				float coord = fingerCoord (hand, index);
				if (hand.Confidence < 0.12f) {
					D = prev_d;
//					Debug.Log (hand.Confidence);
				} else {
//					D = hand.PalmPosition.y;
					D = (float)Math.Round (coord);
					//Debug.Log (D);
/*
					if (index.TipVelocity.Magnitude > 80.0f) {
						if (ext_to_stop == 5) {
							D = dz_min;
							ext_to_stop = 0;
						} else if (ext_to_stop > 0 && prev_d < D) {
							ext_to_stop += 1;
						} else {
							ext_to_stop = 0;
						}

						if (curl_to_stop == 5) {
							D = dz_min;
							curl_to_stop = 0;
						} else if (curl_to_stop > 0 && prev_d > D) {
							curl_to_stop += 1;
						} else {
							curl_to_stop = 0;
						}
					} else if (ext_to_stop > 0) {
						if (ext_to_stop == 5) {
							D = dz_min;
							ext_to_stop = 0;
						} else if (prev_d < D) {
							ext_to_stop += 1;
						} else {
							ext_to_stop = 0;
						}
					} else if (curl_to_stop > 0) {
						if (curl_to_stop == 5) {
							D = dz_min;
							curl_to_stop = 0;
						} else if (prev_d > D) {
							curl_to_stop += 1;
						} else {
							curl_to_stop = 0;
						}
					}
*/
//					//other potential fix, but difficult to resolve when hand in thumbs up position, even more finnicky
//					Debug.Log (index.Bone(Bone.BoneType.TYPE_DISTAL).Direction.z); //change in the z axis
//					float directionZ = index.Bone(Bone.BoneType.TYPE_DISTAL).Direction.z;
				}
					
				if (Input.GetKey (KeyCode.Z) && (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl))) {
					dz_min = Controller.ZeroCenterD;
				}

				prev_d = D;

			}

		} else if (Controller.Ratcheting) { //no hand in ratcheting condition --> no movement
			RatchetSpeed = 0f;
			RatchetRotate = 0f;
		}
	}

//-------------------------------------------------------------------------------------------------------//

	float fingerCoord(Hand leapHand, Finger leapFinger) {

		Vector handXBasis = leapHand.PalmNormal.Cross (leapHand.Direction).Normalized;
    	Vector handYBasis = -leapHand.PalmNormal;
		Vector handZBasis = -leapHand.Direction;
   		Vector handOrigin = leapHand.PalmPosition;
  		// UnityEngine.Matrix4x4 handTransform = new UnityEngine.Matrix4x4 (handXBasis, handYBasis, handZBasis, handOrigin);
        //		handTransform = handTransform.RigidInverse ();

        //		Vector td = handTransform.TransformPoint (leapFinger.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint);
        //		Vector tm = handTransform.TransformPoint (leapFinger.Bone(Bone.BoneType.TYPE_INTERMEDIATE).PrevJoint);
        //		Vector tf = handTransform.TransformPoint (leapFinger.StabilizedTipPosition);
        //		Vector diff = (td - tm);
        //		Debug.Log (td);
        //		Debug.Log (diff.z);
        //		Debug.Log(Math.Exp((double) (Math.Abs(diff.z - dz_min) / 25)));

        //		float temp = diff.z * 3.54f;
        Vector td = leapFinger.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint;
        return (td - handOrigin).z * 2.74f;
        //return td.z * 2.74f;
	}


//-------------------------------------------------------------------------------------------------------//

	Hand GetLeftMostHand(Frame f) {
		float xComp = float.MaxValue;
		Hand candidate = null;
		for(int i = 0; i < f.Hands.Count; ++i) {
			if (f.Hands[i].PalmPosition.x < xComp) {
				candidate = f.Hands[i];
				xComp = f.Hands[i].PalmPosition.x;
			}
		}	
		return candidate;
	}

	Hand GetRightMostHand(Frame f) {
		float xComp = -float.MaxValue;
		Hand candidate = null;
		for(int i = 0; i < f.Hands.Count; ++i) {
			if (f.Hands[i].PalmPosition.x > xComp) {
				candidate = f.Hands[i];
				xComp = f.Hands[i].PalmPosition.x;
			}
		}
		return candidate;
	}

	bool Grabbing(Hand h) { //can adjust this after Wanhong/Lear's findings
		if (h == null) return false;
		return h.GrabStrength > 0.45f;
	}

	void ProcessTranslate(Hand hand) {
        //		RatchetSpeed = -(hand.PalmVelocity.z * 15f); //could change the scale factor
        RatchetSpeed = hand.PalmVelocity.z * 0.1f;
    }

	void HandRatchet(Frame f) {
		Hand left_hand = GetLeftMostHand(f);
		// Get second front most hand
		Hand right_hand = GetRightMostHand(f);

		bool twoHandGrab = Grabbing(left_hand) && Grabbing(right_hand) && f.Hands.Count > 1;
		//		bool twoHandGrab = false;

		if (f.Hands.Count == 2 && !twoHandGrab) {
			centerRight = right_hand.PalmPosition;
			centerLeft = left_hand.PalmPosition;
		}

		// rotation gets priority over translation
		if (twoHandGrab) {
			if (m_twoHandGrabLastFrame) {

				Leap.Vector currentLeftToRight = right_hand.PalmPosition - left_hand.PalmPosition;
				Leap.Vector lastLeftToRight = m_lastRightPos - m_lastLeftPos;

				Vector meanHandPos = (centerRight + centerLeft) / 2;
				Vector currMeanHandPos = (right_hand.PalmPosition + left_hand.PalmPosition) / 2;

				transform.Rotate (0, -(currMeanHandPos.x - meanHandPos.x), 0);

				centerRight = right_hand.PalmPosition;
				centerLeft = left_hand.PalmPosition;
			}

			m_lastRightPos = right_hand.PalmPosition;
			m_lastLeftPos = left_hand.PalmPosition;
		} else {
			RatchetSpeed = 0f;
		}

		if (!twoHandGrab) {
			if (Grabbing (left_hand) || Grabbing (right_hand)) {
				for (int i = 0; i < f.Hands.Count; ++i) {
					if (Grabbing (f.Hands [i]))
						ProcessTranslate (f.Hands [i]);
				}
			} else {
				RatchetSpeed = 0f;
			}
		}

		m_twoHandGrabLastFrame = twoHandGrab;
	}
}
