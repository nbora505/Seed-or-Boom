using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetVRTrackingPosition : MonoBehaviour
{
    public Transform centerEyeAnchor;
    public Transform rightHandAnchor;
    public Transform leftHandAnchor;

    public Transform ReturnCenterEyeAnchor()
    {
        return centerEyeAnchor;
    }
    public Transform ReturnRightHandAnchor()
    {
        return rightHandAnchor;
    }
    public Transform ReturnLeftHandAnchor()
    {
        return leftHandAnchor;
    }
}
