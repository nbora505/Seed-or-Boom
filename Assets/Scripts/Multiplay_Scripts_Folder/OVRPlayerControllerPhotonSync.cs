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
    public List<GameObject> cameras = new List<GameObject>();
    public GameObject cardObj;

    // Start is called before the first frame update
    void Start()
    {
        ovrCameraRig = GetComponent<OVRCameraRig>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!photonView.IsMine)
        {
            ovrCameraRig.enabled = false;
            cameras[0].SetActive(false);
            cameras[1].SetActive(false);
            cameras[2].SetActive(false);

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
