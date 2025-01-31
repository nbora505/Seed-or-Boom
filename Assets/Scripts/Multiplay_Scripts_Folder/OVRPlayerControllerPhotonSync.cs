using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OVRPlayerControllerPhotonSync : MonoBehaviourPunCallbacks
{
    OVRCameraRig ovrCameraRig;
    
    // Start is called before the first frame update
    void Start()
    {
        ovrCameraRig = GetComponent<OVRCameraRig>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }


    }
}
