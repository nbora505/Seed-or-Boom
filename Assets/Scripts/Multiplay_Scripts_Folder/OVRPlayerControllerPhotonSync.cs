using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SpatialTracking;
using DG.Tweening;

public class OVRPlayerControllerPhotonSync : MonoBehaviourPunCallbacks
{
    OVRCameraRig ovrCameraRig;
    Transform centerEyeAnchor;
    public GameObject cardObj;

    // Start is called before the first frame update
    void Start()
    {
        ovrCameraRig = GetComponent<OVRCameraRig>();
        centerEyeAnchor = GetComponentInChildren<Transform>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!photonView.IsMine)
        {
            ovrCameraRig.enabled = false;

            var trackedPoseDriver = ovrCameraRig.GetComponent<TrackedPoseDriver>();
            if (trackedPoseDriver != null)
                trackedPoseDriver.enabled = false;
        }

        if(OVRInput.Get(OVRInput.RawButton.LIndexTrigger))
        {
            cardObj.SetActive(true);
            // card apear
        }

        if(OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))
        {
            cardObj.SetActive(false);
            // card disapear
        }
    }
}
