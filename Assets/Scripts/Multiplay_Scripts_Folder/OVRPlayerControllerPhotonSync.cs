using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SpatialTracking;
using DG.Tweening;
<<<<<<< HEAD
<<<<<<< HEAD
using Unity.VisualScripting;
using UnityEngine.Animations.Rigging;
=======
>>>>>>> parent of 41a190a (test tracking)
=======
>>>>>>> parent of 41a190a (test tracking)

public class OVRPlayerControllerPhotonSync : MonoBehaviourPunCallbacks
{
    OVRCameraRig ovrCameraRig;
    public List<GameObject> cameras = new List<GameObject>();
    public GameObject cardObj;

    // Start is called before the first frame update
    void Start()
    {
<<<<<<< HEAD
<<<<<<< HEAD
        if (photonView.IsMine)
        {
            GameObject ovr = Instantiate(ovrCamera);
            ovr.transform.SetParent(this.gameObject.transform, false);

            hbr = GetComponent<HeadBodyRig>();

            hbr.head.VRTarget = ovr.GetComponent<GetVRTrackingPosition>().ReturnCenterEyeAnchor();
            hbr.rightHand.VRTarget = ovr.GetComponent<GetVRTrackingPosition>().ReturnRightHandAnchor();
            hbr.leftHand.VRTarget = ovr.GetComponent<GetVRTrackingPosition>().ReturnLeftHandAnchor();
        }
        else
        {
            GetComponent<RigBuilder>().enabled = false;
            GetComponent<BoneRenderer>().enabled = false;
            GetComponent<HeadBodyRig>().enabled = false;
        }
=======
        ovrCameraRig = GetComponent<OVRCameraRig>();
>>>>>>> parent of 41a190a (test tracking)
=======
        ovrCameraRig = GetComponent<OVRCameraRig>();
>>>>>>> parent of 41a190a (test tracking)
    }
    // Update is called once per frame
    void Update()
    {

        if (!photonView.IsMine)
        {
            //var trackedPoseDriver = ovrCameraRig.GetComponent<TrackedPoseDriver>();
            //if (trackedPoseDriver != null)
            //    trackedPoseDriver.enabled = false;

            return;
        }

        if (OVRInput.Get(OVRInput.RawButton.LIndexTrigger))
        {
            cardObj.SetActive(true);
            // card apear
        }

        if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))
        {
            cardObj.SetActive(false);
            // card disapear
        }
    }
}