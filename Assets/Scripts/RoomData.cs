using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
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

            GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnEnterRoom(_roomInfo.Name));
        }
    }

    private void Awake()
    {
        roomInfoText = GetComponent<Text>();
        photonManager = GameObject.Find("PhotonManager").GetComponent<PhotonManager>();
    }

    void OnEnterRoom(string roomName)
    {
        photonManager.SetUserId();

        RoomOptions ro = new RoomOptions
        {
            MaxPlayers = 4,
            IsOpen = true,
            IsVisible = true
        };

        // 방의 기본 설정을 CustomProperties에 저장하는거임
        Hashtable roomProperties = new Hashtable
        {
            { "Map1","Map2"},   // 맵 이름
            { "MaxPlayers", 4 }    // 최대 플레이어 수
        };

        ro.CustomRoomProperties = roomProperties;

        PhotonNetwork.JoinOrCreateRoom(roomName, ro, TypedLobby.Default);
    }
}
