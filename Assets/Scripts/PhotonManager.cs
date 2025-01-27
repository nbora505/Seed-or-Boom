using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    private readonly string version = "1.0";

    private string userId = "zack";

    public InputField roomNameIF; //룸 이름 입력 인풋필드
    public InputField userIF;

    private Dictionary<string,GameObject> rooms = new Dictionary<string,GameObject>();
    private GameObject roomItemPrefab;
    public Transform scrollContent;
     void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; 

        PhotonNetwork.GameVersion = version;
        PhotonNetwork.NickName = userId;

        Debug.Log(PhotonNetwork.SendRate);

        roomItemPrefab = Resources.Load<GameObject>("RoomItem");

        if(PhotonNetwork.IsConnected == false)
        {
            PhotonNetwork.ConnectUsingSettings();
        }

    }
    public void Start()
    {
        userId = PlayerPrefs.GetString("유저아이디", $"USER_{Random.Range(1, 21):00}");
        userIF.text = userId;
        PhotonNetwork.NickName = userId;
    }
    
    public void SetUserId()
    {
        if(string.IsNullOrEmpty(userIF.text))
        {
            userId = $"USER_{Random.Range(1, 21):00}";
        }
        else
        {
            userId = userIF.text;
        }

        PlayerPrefs.SetString("USER_ID",userId);
        PhotonNetwork.NickName=userId;
    }

    string SetRoomName()
    {
        if(string.IsNullOrEmpty (roomNameIF.text))
        {
            roomNameIF.text = $"ROOM_{Random.Range(1, 101):000}";
        }
        return roomNameIF.text;
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("마스터 연결");
        Debug.Log($"포톤넷워크.inLobby = { PhotonNetwork.InLobby}");
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        Debug.Log($"포톤넷워크.inLobby = {PhotonNetwork.InLobby}");
    }

    public override void OnJoinRandomFailed(short returnCode,string message)
    {
        Debug.Log($"joinRandom Filed {returnCode}:{message}");
        OnMakeRoomClick(); // 룸 생성 함수
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("방 생성");
        Debug.Log($"Room Name = {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"photonNetwork.inroom = {PhotonNetwork.InRoom}");
        Debug.Log($"player count = {PhotonNetwork.CurrentRoom.PlayerCount} ");

        foreach(var player in PhotonNetwork.CurrentRoom.Players)
        {
            Debug.Log($"{player.Value.NickName},{player.Value.ActorNumber}");
        }

        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Map1");
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        GameObject tempRoom = null;

        foreach(var roomInfo  in roomList)
        {
            if(roomInfo.RemovedFromList == true) // 룸이 삭제된경우 
            {
                rooms.TryGetValue(roomInfo.Name, out tempRoom);
                Destroy(tempRoom);
                rooms.Remove(roomInfo.Name);
            }
            else // 룸 정보가 변경된경우
            {
                 if(rooms.ContainsKey(roomInfo.Name) == false)
                {
                    GameObject roomPrefab = Instantiate(roomItemPrefab, scrollContent);

                    roomPrefab.GetComponent<RoomData>().RoomInfo = roomInfo;

                    rooms.Add(roomInfo.Name,roomPrefab); 
                }
                else
                {
                    rooms.TryGetValue(roomInfo.Name, out tempRoom);
                    tempRoom.GetComponent<RoomData>().RoomInfo = roomInfo;
                }
            }
            Debug.Log($"Room = {roomInfo.Name}({roomInfo.PlayerCount}/{roomInfo.MaxPlayers}");
        }
        foreach(var room in roomList)
        {
            Debug.Log($"Room = {room.Name} ({room.PlayerCount}/{room.MaxPlayers})");
        }
    }
    #region UI_BUTTON_EVENT
    public void OnLoginClick()
    {
        SetUserId(); //유저명저장

        //PhotonNetwork.JoinRandomRoom();
    }

    public void OnMakeRoomClick()
    {
        SetUserId();

        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 4;
        ro.IsOpen = true;
        ro.IsVisible = true;

        PhotonNetwork.CreateRoom(SetRoomName(),ro); 
    }
    #endregion

}
