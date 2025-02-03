using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SpatialTracking;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.Animations.Rigging;

public class OVRPlayerControllerPhotonSync : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject cardObj;
    public GameObject ovrCamera;
    HeadBodyRig hbr;

    // 다른 플레이어의 움직임을 표현할 Transform들
    private Transform networkHead;
    private Transform networkLeftHand;
    private Transform networkRightHand;

    private void Start()
    {
        if (photonView.IsMine)
        {
            // 로컬 플레이어 설정
            GameObject ovr = Instantiate(ovrCamera);
            ovr.transform.SetParent(this.gameObject.transform, false);

            hbr = GetComponent<HeadBodyRig>();
            hbr.head.VRTarget = ovr.GetComponent<GetVRTrackingPosition>().ReturnCenterEyeAnchor();
            hbr.rightHand.VRTarget = ovr.GetComponent<GetVRTrackingPosition>().ReturnRightHandAnchor();
            hbr.leftHand.VRTarget = ovr.GetComponent<GetVRTrackingPosition>().ReturnLeftHandAnchor();
        }
        else
        {
            // 네트워크 플레이어 설정
            GetComponent<RigBuilder>().enabled = false;
            GetComponent<BoneRenderer>().enabled = false;
            GetComponent<HeadBodyRig>().enabled = false;

            // 네트워크 플레이어용 Transform 생성
            networkHead = new GameObject("NetworkHead").transform;
            networkLeftHand = new GameObject("NetworkLeftHand").transform;
            networkRightHand = new GameObject("NetworkRightHand").transform;

            networkHead.SetParent(transform);
            networkLeftHand.SetParent(transform);
            networkRightHand.SetParent(transform);
        }
    }

    private void Update()
    {
        if (!photonView.IsMine)
            return;

        if (OVRInput.Get(OVRInput.RawButton.LIndexTrigger))
        {
            cardObj.SetActive(true);
        }

        if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))
        {
            cardObj.SetActive(false);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 로컬 플레이어의 위치 정보 전송
            stream.SendNext(hbr.head.VRTarget.position);
            stream.SendNext(hbr.head.VRTarget.rotation);
            stream.SendNext(hbr.leftHand.VRTarget.position);
            stream.SendNext(hbr.leftHand.VRTarget.rotation);
            stream.SendNext(hbr.rightHand.VRTarget.position);
            stream.SendNext(hbr.rightHand.VRTarget.rotation);
        }
        else
        {
            // 네트워크 플레이어의 위치 정보 수신
            networkHead.position = (Vector3)stream.ReceiveNext();
            networkHead.rotation = (Quaternion)stream.ReceiveNext();
            networkLeftHand.position = (Vector3)stream.ReceiveNext();
            networkLeftHand.rotation = (Quaternion)stream.ReceiveNext();
            networkRightHand.position = (Vector3)stream.ReceiveNext();
            networkRightHand.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}