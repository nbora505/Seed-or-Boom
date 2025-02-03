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
    GameObject tempOVR;
    HeadBodyRig hbr;

    // Start is called before the first frame update
    void Start()
    {
        if(photonView.IsMine)
        {
            GameObject ovr = Instantiate(ovrCamera);
            ovr.transform.SetParent(this.gameObject.transform, false);

            tempOVR = ovr;

            hbr = GetComponent<HeadBodyRig>();

            //photonView.RPC("SetHBRTransform()", RpcTarget.All);
            //photonView.RPC("SetHBRTransform()", RpcTarget.All);
            //photonView.RPC("SetHBRTransform()", RpcTarget.All);

            SetupVRTargets();
            //hbr.head.VRTarget = ovr.GetComponent<GetVRTrackingPosition>().ReturnCenterEyeAnchor();
            //hbr.rightHand.VRTarget = ovr.GetComponent<GetVRTrackingPosition>().ReturnRightHandAnchor();
            //hbr.leftHand.VRTarget = ovr.GetComponent<GetVRTrackingPosition>().ReturnLeftHandAnchor();

            photonView.RPC("SyncVRSetup", RpcTarget.Others);
        }
    }

    void SetupVRTargets()
    {
        var trackingComponent = tempOVR.GetComponent<GetVRTrackingPosition>();
        if (trackingComponent != null)
        {
            hbr.head.VRTarget = trackingComponent.ReturnCenterEyeAnchor();
            hbr.rightHand.VRTarget = trackingComponent.ReturnRightHandAnchor();
            hbr.leftHand.VRTarget = trackingComponent.ReturnLeftHandAnchor();
        }
    }

    [PunRPC]
    void SyncVRSetup()
    {
        // 다른 클라이언트들에서는 OVR 카메라를 찾아서 설정
        var trackingComponent = GetComponentInChildren<GetVRTrackingPosition>();
        if (trackingComponent != null)
        {
            hbr.head.VRTarget = trackingComponent.ReturnCenterEyeAnchor();
            hbr.rightHand.VRTarget = trackingComponent.ReturnRightHandAnchor();
            hbr.leftHand.VRTarget = trackingComponent.ReturnLeftHandAnchor();
        }
    }

    //[PunRPC]
    //void SetHBRTransform()
    //{
    //    hbr.head.VRTarget = tempOVR.GetComponent<GetVRTrackingPosition>().ReturnCenterEyeAnchor();
    //    hbr.rightHand.VRTarget = tempOVR.GetComponent<GetVRTrackingPosition>().ReturnRightHandAnchor();
    //    hbr.leftHand.VRTarget = tempOVR.GetComponent<GetVRTrackingPosition>().ReturnLeftHandAnchor();
    //}
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
