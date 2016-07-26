using UnityEngine;
using UnityEngine.VR;
using System;
using System.Linq;

public class VRHeightOffset : MonoBehaviour {

  [Serializable]
  public class DeviceHeightPair {
    public string DeviceName;
    public float HeightOffset;

    public DeviceHeightPair(string deviceName, float heightOffset) {
      DeviceName = deviceName;
      HeightOffset = heightOffset;
    }
  }

  public DeviceHeightPair[] _deviceOffsets;

  void Reset() {
    _deviceOffsets = new DeviceHeightPair[1];
    _deviceOffsets[0] = new DeviceHeightPair("oculus", 1f);
  }

  void Start() {
    if (VRDevice.isPresent && VRSettings.enabled && _deviceOffsets != null) {
#if UNITY_5_4_OR_NEWER
      string deviceName = VRSettings.loadedDeviceName;
#else
      string deviceName = VRDevice.family;
#endif
      var deviceHeightPair = _deviceOffsets.FirstOrDefault(d => deviceName.ToLower().Contains(d.DeviceName.ToLower()));
      if (deviceHeightPair != null) {
        transform.Translate(Vector3.up * deviceHeightPair.HeightOffset);
      }
      transform.position = new Vector3(0f, 1.9f, -4.5f);
      transform.rotation = Quaternion.identity;

        }
    }

    void Update()
    {
        transform.rotation = Quaternion.identity;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.position = new Vector3(0f, 1.9f, -4.5f);
        }
    }
}
