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
    //GameObject tempOVR;
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
            //tempOVR = ovr;

            photonView.RPC("SyncOVRObject", RpcTarget.AllBuffered, photonView.ViewID);
        }
    }

    [PunRPC]
    void SyncOVRObject(int viewID)
    {
        StartCoroutine(SetupVRTargets(viewID));
    }

    IEnumerator SetupVRTargets(int viewID)
    {
        // OVR 객체가 완전히 생성될 때까지 대기
        yield return new WaitForEndOfFrame();

        PhotonView ownerPV = PhotonView.Find(viewID);
        if (ownerPV != null)
        {
            var ovrObject = ownerPV.gameObject.GetComponentInChildren<GetVRTrackingPosition>();
            if (ovrObject != null)
            {
                hbr.head.VRTarget = ovrObject.ReturnCenterEyeAnchor();
                hbr.rightHand.VRTarget = ovrObject.ReturnRightHandAnchor();
                hbr.leftHand.VRTarget = ovrObject.ReturnLeftHandAnchor();
            }
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
