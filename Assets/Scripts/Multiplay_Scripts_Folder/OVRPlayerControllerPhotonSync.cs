using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class MetaAllInOnePlayerPhotonSync : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Local VR Setup for Meta All-In-One")]
    [Tooltip("VR 카메라 프리팹 (GetVRTrackingPosition 스크립트가 포함되어 있어야 합니다.)")]
    public GameObject ovrCameraPrefab;

    [Tooltip("LIndexTrigger 버튼 활성화 시 표시할 카드 오브젝트")]
    public GameObject cardObj;

    [Header("Remote Avatar Parts (네트워크 플레이어용)")]
    [Tooltip("원격 플레이어 아바타의 머리 Transform (인스펙터에서 할당하거나, 계층 구조가 'Avatar/Head' 인 경우 자동 검색됩니다.)")]
    public Transform avatarHead;
    [Tooltip("원격 플레이어 아바타의 왼손 Transform (인스펙터에서 할당하거나, 계층 구조가 'Avatar/LeftHand' 인 경우 자동 검색됩니다.)")]
    public Transform avatarLeftHand;
    [Tooltip("원격 플레이어 아바타의 오른손 Transform (인스펙터에서 할당하거나, 계층 구조가 'Avatar/RightHand' 인 경우 자동 검색됩니다.)")]
    public Transform avatarRightHand;

    // 로컬 플레이어의 VR 트래킹 대상 (ovrCameraPrefab에서 가져옴)
    private Transform localHeadTarget;
    private Transform localLeftHandTarget;
    private Transform localRightHandTarget;

    // 로컬 플레이어의 HeadBodyRig (아바타의 본 제어용)
    private HeadBodyRig headBodyRig;

    // 원격 플레이어의 동기화 데이터(위치 및 회전)
    private Vector3 networkHeadPos;
    private Quaternion networkHeadRot;
    private Vector3 networkLeftHandPos;
    private Quaternion networkLeftHandRot;
    private Vector3 networkRightHandPos;
    private Quaternion networkRightHandRot;

    [Tooltip("원격 아바타 보간(Lerp) 속도")]
    public float lerpSpeed = 10f;

    private void Start()
    {
        if (photonView.IsMine)
        {
            // ★ 로컬 플레이어 설정 ★
            // Meta 올인원 기기에서 사용할 VR 카메라(트래킹 데이터 제공) 프리팹을 인스턴스화하고 자식으로 붙입니다.
            GameObject ovrCamObj = Instantiate(ovrCameraPrefab, transform);
            GetVRTrackingPosition vrTracking = ovrCamObj.GetComponent<GetVRTrackingPosition>();
            if (vrTracking != null)
            {
                localHeadTarget = vrTracking.ReturnCenterEyeAnchor();
                localLeftHandTarget = vrTracking.ReturnLeftHandAnchor();
                localRightHandTarget = vrTracking.ReturnRightHandAnchor();
            }
            else
            {
                Debug.LogError("GetVRTrackingPosition 컴포넌트를 찾을 수 없습니다. 프리팹을 확인하세요.");
            }

            // HeadBodyRig를 사용하여 로컬 아바타의 본(머리, 양손)과 VR 타깃을 연결합니다.
            headBodyRig = GetComponent<HeadBodyRig>();
            if (headBodyRig != null)
            {
                headBodyRig.head.VRTarget = localHeadTarget;
                headBodyRig.leftHand.VRTarget = localLeftHandTarget;
                headBodyRig.rightHand.VRTarget = localRightHandTarget;
            }
            else
            {
                Debug.LogError("HeadBodyRig 컴포넌트를 찾을 수 없습니다.");
            }
        }
        else
        {
            // ★ 원격 플레이어 설정 ★
            // 로컬 VR 관련 컴포넌트는 원격 플레이어에선 사용하지 않으므로 비활성화합니다.
            RigBuilder rigBuilder = GetComponent<RigBuilder>();
            if (rigBuilder != null)
                rigBuilder.enabled = false;
            HeadBodyRig hbr = GetComponent<HeadBodyRig>();
            if (hbr != null)
                hbr.enabled = false;

            // 인스펙터에서 미리 할당되지 않았다면, 계층 구조에서 아바타의 머리와 손 Transform을 찾아봅니다.
            if (avatarHead == null)
            {
                Transform foundHead = transform.Find("Avatar/Head");
                if (foundHead != null)
                    avatarHead = foundHead;
                else
                    Debug.LogWarning("Avatar/Head를 찾을 수 없습니다. 인스펙터에서 할당하세요.");
            }
            if (avatarLeftHand == null)
            {
                Transform foundLeft = transform.Find("Avatar/LeftHand");
                if (foundLeft != null)
                    avatarLeftHand = foundLeft;
                else
                    Debug.LogWarning("Avatar/LeftHand를 찾을 수 없습니다. 인스펙터에서 할당하세요.");
            }
            if (avatarRightHand == null)
            {
                Transform foundRight = transform.Find("Avatar/RightHand");
                if (foundRight != null)
                    avatarRightHand = foundRight;
                else
                    Debug.LogWarning("Avatar/RightHand를 찾을 수 없습니다. 인스펙터에서 할당하세요.");
            }

            // 원격 플레이어의 스냅 현상을 막기 위해 초기 위치와 회전을 현재 아바타 값으로 설정합니다.
            if (avatarHead != null)
            {
                networkHeadPos = avatarHead.position;
                networkHeadRot = avatarHead.rotation;
            }
            if (avatarLeftHand != null)
            {
                networkLeftHandPos = avatarLeftHand.position;
                networkLeftHandRot = avatarLeftHand.rotation;
            }
            if (avatarRightHand != null)
            {
                networkRightHandPos = avatarRightHand.position;
                networkRightHandRot = avatarRightHand.rotation;
            }
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            // ★ 로컬 플레이어 입력 처리 ★
            // OVRInput을 사용하여 Meta 올인원(메타 퀘스트)에서 트리거 입력을 처리합니다.
            if (OVRInput.Get(OVRInput.RawButton.LIndexTrigger))
            {
                cardObj.SetActive(true);
            }
            if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))
            {
                cardObj.SetActive(false);
            }
        }
        else
        {
            // ★ 원격 플레이어 아바타 보간 업데이트 ★
            if (avatarHead != null)
            {
                avatarHead.position = Vector3.Lerp(avatarHead.position, networkHeadPos, Time.deltaTime * lerpSpeed);
                avatarHead.rotation = Quaternion.Lerp(avatarHead.rotation, networkHeadRot, Time.deltaTime * lerpSpeed);
            }
            if (avatarLeftHand != null)
            {
                avatarLeftHand.position = Vector3.Lerp(avatarLeftHand.position, networkLeftHandPos, Time.deltaTime * lerpSpeed);
                avatarLeftHand.rotation = Quaternion.Lerp(avatarLeftHand.rotation, networkLeftHandRot, Time.deltaTime * lerpSpeed);
            }
            if (avatarRightHand != null)
            {
                avatarRightHand.position = Vector3.Lerp(avatarRightHand.position, networkRightHandPos, Time.deltaTime * lerpSpeed);
                avatarRightHand.rotation = Quaternion.Lerp(avatarRightHand.rotation, networkRightHandRot, Time.deltaTime * lerpSpeed);
            }
        }
    }

    // Photon의 데이터 송수신 함수
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 로컬 플레이어: VR 트래킹 데이터(위치와 회전)를 전송합니다.
            stream.SendNext(localHeadTarget.position);
            stream.SendNext(localHeadTarget.rotation);
            stream.SendNext(localLeftHandTarget.position);
            stream.SendNext(localLeftHandTarget.rotation);
            stream.SendNext(localRightHandTarget.position);
            stream.SendNext(localRightHandTarget.rotation);
        }
        else
        {
            // 원격 플레이어: 데이터를 수신하여 보간할 목표값으로 저장합니다.
            networkHeadPos = (Vector3)stream.ReceiveNext();
            networkHeadRot = (Quaternion)stream.ReceiveNext();
            networkLeftHandPos = (Vector3)stream.ReceiveNext();
            networkLeftHandRot = (Quaternion)stream.ReceiveNext();
            networkRightHandPos = (Vector3)stream.ReceiveNext();
            networkRightHandRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
