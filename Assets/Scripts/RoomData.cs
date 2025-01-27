using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class RoomData : MonoBehaviour
{
    private RoomInfo _roomInfo;

    private Text roomInfoText;

    private PhotonManager photonManager;
   
    public RoomInfo RoomInfo
    {
        get { return _roomInfo; }
        set
        {
            _roomInfo = value;

            roomInfoText.text = $"{_roomInfo.Name} ({_roomInfo.PlayerCount}/{_roomInfo.MaxPlayers})";

           
        }
    }
}
