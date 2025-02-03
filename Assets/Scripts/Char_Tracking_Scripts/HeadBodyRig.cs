using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VRMap
{
    public Transform VRTarget;
    public Transform rigTarget;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    public void Map()
    {
        rigTarget.position = VRTarget.TransformPoint(positionOffset);
        rigTarget.rotation = VRTarget.rotation * Quaternion.Euler(rotationOffset);
    }
}

public class HeadBodyRig : MonoBehaviour
{
    public VRMap head;
    public VRMap rightHand;
    public VRMap leftHand;

    public Transform headConstraint;
    Vector3 offset;

    public float turnFactor = 1f;
    public ForwardAxis forwardAxis;

    public enum ForwardAxis
    {
        blue,
        green,
        red
    }

    void Start()
    {
        offset = transform.position - headConstraint.position;
    }

    void FixedUpdate()
    {
        if (headConstraint == null)
        {
            Debug.LogError("headConstraint가 설정되지 않았습니다!");
            return;
        }

        if (!GetComponent<PhotonView>().IsMine) // 내 캐릭터만 변경
        {
            return;
        }

        // VR 장치가 없을 경우 움직임 방지
        if (headConstraint.position.magnitude < 0.1f)
        {
            return;
        }

        transform.position = headConstraint.position + offset;
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        // head.Map();
        // rightHand.Map();
        // leftHand.Map();
    }
}
