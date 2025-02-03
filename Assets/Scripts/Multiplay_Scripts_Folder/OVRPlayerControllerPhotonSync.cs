using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SpatialTracking;
using DG.Tweening;
using Unity.VisualScripting;

public class OVRPlayerControllerPhotonSync : MonoBehaviourPunCallbacks
{
    public GameObject cardObj;
    public GameObject ovrCamera;
    HeadBodyRig hbr;

    // Start is called before the first frame update
    void Start()
    {
        if(photonView.IsMine)
        {
            GameObject ovr = Instantiate(ovrCamera);
            ovr.transform.SetParent(this.gameObject.transform, false);
            
            hbr = GetComponent<HeadBodyRig>();

            hbr.head.VRTarget = ovr.GetComponent<GetVRTrackingPosition>().ReturnCenterEyeAnchor();
            hbr.rightHand.VRTarget = ovr.GetComponent<GetVRTrackingPosition>().ReturnRightHandAnchor();
            hbr.leftHand.VRTarget = ovr.GetComponent<GetVRTrackingPosition>().ReturnLeftHandAnchor();
        }
    }
    // Update is called once per frame
    void Update()
    {

        if (!photonView.IsMine)
        {
            return;
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
