using UnityEngine;

/// <summary>
/// Attach to the XR Origin (XR Rig) to cancel out vehicle pitch/roll inherited
/// from its parent. Only the yaw (horizontal turning) is kept, preventing VR
/// dizziness caused by the vehicle tilting during acceleration/deceleration.
/// </summary>
public class YawStabilizer : MonoBehaviour
{
    void LateUpdate()
    {
        float yaw = transform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}
