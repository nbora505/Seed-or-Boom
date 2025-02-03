using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetVRTrackingPosition : MonoBehaviour
{
    public Transform centerEyeAnchor;
    public Transform rightHandAnchor;
    public Transform leftHandAnchor;

    [PunRPC]
    public Transform ReturnCenterEyeAnchor()
    {
        return centerEyeAnchor;
    }
    [PunRPC]
    public Transform ReturnRightHandAnchor()
    {
        return rightHandAnchor;
    }
    [PunRPC]
    public Transform ReturnLeftHandAnchor()
    {
        return leftHandAnchor;
    }
}
