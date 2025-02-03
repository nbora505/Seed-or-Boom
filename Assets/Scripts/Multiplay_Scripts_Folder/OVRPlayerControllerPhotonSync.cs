using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OVRPlayerControllerPhotonSync : MonoBehaviourPunCallbacks
{
    public GameObject cardObj;
    public GameObject ovrCamera;
    GameObject tempOVR;
    HeadBodyRig hbr;

    void Awake()
    {
        hbr = GetComponent<HeadBodyRig>();
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            GameObject ovr = Instantiate(ovrCamera);
            ovr.transform.SetParent(this.gameObject.transform, false);
            tempOVR = ovr;

            SetupVRTargets();

            photonView.RPC("SyncVRSetup", RpcTarget.Others);
        }
    }

    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("UpdateRemotePlayerTransform", RpcTarget.Others,
                hbr.head.VRTarget.position, hbr.head.VRTarget.rotation,
                hbr.rightHand.VRTarget.position, hbr.rightHand.VRTarget.rotation,
                hbr.leftHand.VRTarget.position, hbr.leftHand.VRTarget.rotation);
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
    void UpdateRemotePlayerTransform(Vector3 headPos, Quaternion headRot,
                                     Vector3 rightHandPos, Quaternion rightHandRot,
                                     Vector3 leftHandPos, Quaternion leftHandRot)
    {
        if (hbr.head.VRTarget != null)
        {
            hbr.head.VRTarget.position = headPos;
            hbr.head.VRTarget.rotation = headRot;
        }
        if (hbr.rightHand.VRTarget != null)
        {
            hbr.rightHand.VRTarget.position = rightHandPos;
            hbr.rightHand.VRTarget.rotation = rightHandRot;
        }
        if (hbr.leftHand.VRTarget != null)
        {
            hbr.leftHand.VRTarget.position = leftHandPos;
            hbr.leftHand.VRTarget.rotation = leftHandRot;
        }
    }

    [PunRPC]
    void SyncVRSetup()
    {
        var trackingComponent = GetComponentInChildren<GetVRTrackingPosition>();
        if (trackingComponent != null)
        {
            hbr.head.VRTarget = trackingComponent.ReturnCenterEyeAnchor();
            hbr.rightHand.VRTarget = trackingComponent.ReturnRightHandAnchor();
            hbr.leftHand.VRTarget = trackingComponent.ReturnLeftHandAnchor();
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (OVRInput.Get(OVRInput.RawButton.LIndexTrigger))
        {
            cardObj.SetActive(true);
        }

        if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))
        {
            cardObj.SetActive(false);
        }
    }
}
