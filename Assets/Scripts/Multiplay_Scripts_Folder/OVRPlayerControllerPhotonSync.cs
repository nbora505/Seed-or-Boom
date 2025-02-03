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
    HeadBodyRig hbr; // 모든 클라이언트에서 접근해야 하므로 할당이 필요함.

    void Awake()
    {
        // 모든 클라이언트에서 HeadBodyRig 컴포넌트를 가져옵니다.
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

            // 로컬 플레이어는 다른 클라이언트에 VR 설정 정보를 보내줍니다.
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
        // 원격 클라이언트에서는 자식 오브젝트에서 GetVRTrackingPosition 컴포넌트를 찾아서 설정합니다.
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
