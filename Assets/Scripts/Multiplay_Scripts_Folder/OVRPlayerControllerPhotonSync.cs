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

            //hbr.head.VRTarget = ovr.GetComponent<GetVRTrackingPosition>().ReturnCenterEyeAnchor();
            //hbr.rightHand.VRTarget = ovr.GetComponent<GetVRTrackingPosition>().ReturnRightHandAnchor();
            //hbr.leftHand.VRTarget = ovr.GetComponent<GetVRTrackingPosition>().ReturnLeftHandAnchor();

            photonView.RPC("InitializeRig", RpcTarget.AllBuffered, photonView.ViewID);

        }
    }

    [PunRPC]
    void InitializeRig(int viewID)
    {
        PhotonView ownerPV = PhotonView.Find(viewID);
        if (ownerPV != null)
        {
            Transform ovrTransform = ownerPV.transform.GetComponentInChildren<GetVRTrackingPosition>().transform;

            if (ovrTransform != null)
            {
                var trackingComponent = ovrTransform.GetComponent<GetVRTrackingPosition>();
                hbr.head.VRTarget = trackingComponent.ReturnCenterEyeAnchor();
                hbr.rightHand.VRTarget = trackingComponent.ReturnRightHandAnchor();
                hbr.leftHand.VRTarget = trackingComponent.ReturnLeftHandAnchor();
            }
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
