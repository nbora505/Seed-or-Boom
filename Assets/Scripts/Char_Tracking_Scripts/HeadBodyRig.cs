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
        // 플레이어 위치 자동 이동을 막기 위해 주석 처리
        // transform.position = headConstraint.position + offset;

        // 자동 회전 기능도 비활성화
        // Vector3 projectionVector = headConstraint.up;
        // switch (forwardAxis)
        // {
        //     case ForwardAxis.green:
        //         projectionVector = headConstraint.up;
        //         break;
        //     case ForwardAxis.blue:
        //         projectionVector = headConstraint.forward;
        //         break;
        //     case ForwardAxis.red:
        //         projectionVector = headConstraint.right;
        //         break;
        // }
        // transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(projectionVector, Vector3.up).normalized, Time.deltaTime * turnFactor);

        // Debug.Log(transform.position);
        // transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        // VR 헤드와 손의 위치 매핑은 유지 (VR 기기 사용 시 필요)
        head.Map();
        rightHand.Map();
        leftHand.Map();
    }

}
