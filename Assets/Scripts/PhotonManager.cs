using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using System.Linq;
public class PhotonManager : MonoBehaviourPunCallbacks
{
    

    private readonly string version = "1.0";

    private string userId = "zack";

    public InputField roomNameIF; //룸 이름 입력 인풋필드
    public InputField userIF;

    private Dictionary<string,GameObject> rooms = new Dictionary<string,GameObject>();
    private GameObject roomItemPrefab;
    public Transform scrollContent;

    public MultiplayGameManager multiplayGameManager;

    [Tooltip("Character's Prefabs")]
    public GameObject[] characterPrefabs;

    [Tooltip("Empty Gameobj of SpawnPoints")]
    public Transform[] spawnPoints;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
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
        //PhotonNetwork.NickName = userId;
    }
    
    public void SetUserId()
    {
        if(string.IsNullOrEmpty(userIF.text))
        {
            userId = $"USER_{Random.Range(1, 4 ):00}";
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
        Debug.Log($"photonNetwork.룸 = {PhotonNetwork.InRoom}");
        Debug.Log($"player count = {PhotonNetwork.CurrentRoom.PlayerCount} ");
        
        foreach(var player in PhotonNetwork.CurrentRoom.Players)
        {
            Debug.Log($"{player.Value.NickName},{player.Value.ActorNumber}");

            if (player.Value.CustomProperties.ContainsKey("CharacterIndex"))
            {
                int charIndex = (int)player.Value.CustomProperties["CharacterIndex"];
                Debug.Log($"{player.Value.NickName} 선택한 캐릭터 인덱스: {charIndex}");
            }
        }
        
        PhotonNetwork.LoadLevel("Map1");   
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    #region 쓰레기
    //private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    if (scene.name == "Map1")
    //    {
    //        var multiplayGameManager = GameObject.Find("MultiplayGameManager").GetComponent<MultiplayGameManager>();

    //        spawnPoints = multiplayGameManager.spawnPoints;

    //        //if (PhotonNetwork.IsMasterClient)
    //        //{
    //        //    StartCoroutine(DelayedSpawnForAll());
    //        //}

    //        StartCoroutine(WaitForPlayerListAndSpawn(PhotonNetwork.LocalPlayer.ActorNumber));

    //    }
    //    SceneManager.sceneLoaded -= OnSceneLoaded;
    //}

    //IEnumerator DelayedSpawnForAll()
    //{
    //    // 로컬 플레이어가 완전히 준비될 시간주는거
    //    yield return new WaitForSeconds(3f);
    //    foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
    //    {
    //        photonView.RPC("SpwanPlayer", RpcTarget.All, player.ActorNumber);
    //    }
    //}
    //public IEnumerator WaitForPlayerListAndSpawn(int actorNumberID)
    //{
        
        
    //    Debug.Log("플레이어 리스트 동기화 대기 중");
        
    //    yield return new WaitUntil(() => PhotonNetwork.PlayerList.Length > 0);
    //    yield return new WaitForSeconds(1f);
    //    Photon.Realtime.Player newPlayer = PhotonNetwork.PlayerList.FirstOrDefault(
    //   player => player.ActorNumber == actorNumberID);

        
    //    Debug.Log($"현재 플레이어 리스트 수: {PhotonNetwork.PlayerList.Length}");

    //    SpwanPlayer(actorNumberID);

    //}
   

    
    //public void SpwanPlayer(int actorNumberID)
    //{
    //    var multiplayGameManager = GameObject.Find("MultiplayGameManager").GetComponent<MultiplayGameManager>();
    //    if (actorNumberID <= 0)
    //    {
    //        Debug.LogWarning($"[SpwanPlayer] actorNumberID가 {actorNumberID} 입니다. LocalPlayer.ActorNumber로 변경.");
    //        actorNumberID = PhotonNetwork.LocalPlayer.ActorNumber;
    //    }

    //    Photon.Realtime.Player newPlayer = PhotonNetwork.PlayerList.FirstOrDefault(
    //        player => player.ActorNumber == actorNumberID);


    //    int characterSelectIndex = newPlayer.CustomProperties.ContainsKey("CharacterIndex")
    //   ? (int)newPlayer.CustomProperties["CharacterIndex"]
    //    : 1;

    //    Debug.Log($"플레이어 {actorNumberID}의 캐릭터 인덱스: {characterSelectIndex}");


    //    GameObject selectedCharacter = characterPrefabs[characterSelectIndex];
    //    GameObject player = PhotonNetwork.Instantiate(selectedCharacter.name,
    //    spawnPoints[(actorNumberID-1 ) % spawnPoints.Length].position,
    //    Quaternion.identity);

        
    //    multiplayGameManager.playerList.Add(player);

        
    //}
    #endregion
    void SpawnLocalPlayer()
    {
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        // 로컬 플레이어의 CustomProperties에서 CharacterIndex 가져오기 (없으면 기본 1)
        Photon.Realtime.Player newPlayer = PhotonNetwork.LocalPlayer;
        int characterSelectIndex = newPlayer.CustomProperties.ContainsKey("CharacterIndex")
                                    ? (int)newPlayer.CustomProperties["CharacterIndex"]
                                    : 1;
        Debug.Log($"플레이어 {actorNumber}의 캐릭터 인덱스: {characterSelectIndex}");

        // 프리팹 선택 및 스폰 위치 계산
        GameObject selectedCharacter = characterPrefabs[characterSelectIndex];
        GameObject spawnedPlayer = PhotonNetwork.Instantiate(
            selectedCharacter.name,
            spawnPoints[(actorNumber - 1) % spawnPoints.Length].position,
            Quaternion.identity);



        // 생성된 플레이어 오브젝트의 PhotonView ID를 획득
        int viewID = spawnedPlayer.GetComponent<PhotonView>().ViewID;
        // 마스터 클라이언트에 자신의 스폰 정보를 전달
        photonView.RPC("ReportSpawn", RpcTarget.MasterClient, actorNumber, viewID);
    }

    [PunRPC]
    void ReportSpawn(int actorNumber, int viewID)
    {
        // 이 코드는 마스터 클라이언트에서 실행
        GameObject spawnedPlayer = PhotonView.Find(viewID).gameObject;
        var masterManager = GameObject.Find("MultiplayGameManager").GetComponent<MultiplayGameManager>();

        // 중복 추가를 방지
        if (!masterManager.playerList.Contains(spawnedPlayer))
        {
            masterManager.playerList.Add(spawnedPlayer);
            Debug.Log($"Master added spawned player for actor {actorNumber}");
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Map1")
        {
            var multiplayGameManager = GameObject.Find("MultiplayGameManager").GetComponent<MultiplayGameManager>();
            spawnPoints = multiplayGameManager.spawnPoints;

            // 모든 클라이언트가 자신의 캐릭터를 로컬에서 생성하도록 함.
            StartCoroutine(SpawnLocalPlayerWithDelay());
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    IEnumerator SpawnLocalPlayerWithDelay()
    {
        
        yield return new WaitForSeconds(1f);
        SpawnLocalPlayer();
    }


    public void SetCharacterIndex(int index)
    {
        Hashtable playerProperties = new Hashtable { { "CharacterIndex", index } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
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
