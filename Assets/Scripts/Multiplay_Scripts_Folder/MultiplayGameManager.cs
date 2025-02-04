using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;


public class MultiplayGameManager : MonoBehaviourPunCallbacks
{
    public List<GameObject> playerList;
    public List<GameObject> deadList;
    public GameObject startBtn;
    public GameObject leaderPlayer;

    public Text noticeturnText;
    public Text LogText;

    public bool isGameReady = false;

    public int maxPlayerCnt = 4;
    public int curRound = 1;
    public int maxRound = 3;
    public int curTurn;
    public int maxCardCnt = 5;

    public int leaderIndex = 0;  // 현재 리더 플레이어의 인덱스

    public int selectedBomb = -1;
    public int selectedWin = -1;
    public int checkSubmitCard = -1;
    public int selectedCard = 0;
    public List<int> submitCardList; //제출된 카드리스트
    public int[] predictedWinCnt; //각각의 라운드마다 플레이어들이 예측한 승리 횟수
    public int[] winCntOfEachTurn; //각각의 턴마다 플레이어들이 기록한 승리 횟수

    public CardManager cardManager;
    public ScoreManager scoreManager;
    public MultiPlayBtnManager buttonManager;
    public CameraManager cameraManager;
    public FirebaseManager firebaseManager;
    
    [Tooltip("Character's Prefabs")]
    public GameObject[] characterPrefabs;

    [Tooltip("Empty Gameobj of SpawnPoints")]
    public Transform[] spawnPoints;

    
    #region UnityCallBacks
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = false; // 방 터짐 방지
        
    }

    #endregion
    //[PunRPC]
    //public void GetWinScore(int playerid, int expectedWin, Text logText)
    //{
    //    selectedWin = 0;
    //    predictedWinCnt[playerid] = expectedWin;
    //    logText.DOText("승리횟수 :" + expectedWin.ToString() + "번 제출완료", 1.2f);
    //    expectedWin = 0;
    //}

    [PunRPC]
    void DisconnectedUserDeadAnimation(int playerID)
    {
        playerList[playerID - 1].GetComponent<Animator>().Play("Death");
        photonView.RPC("RemoveDisconnectUserFromListRPC", RpcTarget.All, playerID);
    }

    [PunRPC]
    void RemoveDisconnectUserFromList(int playerID)
    {
        Invoke("nun", 2f);
        playerList.Remove(playerList[playerID - 1]);
    }
    void nun()
    {
        
    }
    #region PunCallBacksLines
    //public override void OnJoinedRoom()
    //{
    //    if (PhotonNetwork.IsMasterClient)
    //    {
    //        photonView.RPC("SpwanPlayer", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    //    }
    //}

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //deadList.Add(playerList[otherPlayer.ActorNumber - 1]);
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("DisconnectedUserDeadAnimation", RpcTarget.All, otherPlayer.ActorNumber);
            //playerList.Remove(playerList[otherPlayer.ActorNumber - 1]); // dead anim 1.4f
            LogText.text = $"현재 플레이어 {otherPlayer}가 방을 떠났습니다.";
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"크래쉬 사유 {cause}입니다.");
    }

    // AutomaticallySyncScene false로 설정해서 방 안 터짐.
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        LogText.text = $"현재 방장은 {newMasterClient}입니다.";

        Debug.LogWarning($"현재 방장은 {newMasterClient}입니다.");
    }
    #endregion
    //public IEnumerator WaitForPlayerListAndSpawn(int actorNumberID)
    //{
    //    Debug.Log("플레이어 리스트 동기화 대기 중...");
    //    yield return new WaitUntil(() => PhotonNetwork.PlayerList.Length > 0);

    //    Debug.Log($"현재 플레이어 리스트 수: {PhotonNetwork.PlayerList.Length}");

    //    SpwanPlayer(actorNumberID-1);
        
    //}
    #region PunRPCLines

    //[PunRPC]
    //void SpwanPlayer(int actorNumberID)
    //{
    //    Photon.Realtime.Player newPlayer = PhotonNetwork.PlayerList.FirstOrDefault(
    //        player => player.ActorNumber == actorNumberID);

    //    if (newPlayer == null) return;


    //    int characterSelectIndex = (int)newPlayer.CustomProperties["CharacterIndex"];



    //    GameObject selectedCharacter = characterPrefabs[characterSelectIndex];
    //    GameObject player = PhotonNetwork.Instantiate(selectedCharacter.name,
    //        spawnPoints[(actorNumberID - 1) % spawnPoints.Length].position,
    //        Quaternion.identity);

    //    playerList.Add(player);
    //}

    [PunRPC]
    public void CheckPlayerReady(int playerID)
    {
        //playerList[playerID].GetComponent<PlayerController>().isReady = true;

        //if(PhotonNetwork.IsMasterClient)
        //    CheckAllPlayerReady();
        ////if(playerList.All(player => player.GetComponent<PlayerController>().isReady))
        ////{
        ////    photonView.RPC("StartGame", RpcTarget.All);
        ////}
        ///
       
        GameObject player = playerList.Find(player => player.GetComponent<PhotonView>().Owner.ActorNumber == playerID);

        if(player != null) player.GetComponent<PlayerController>().isReady = true;

        if (PhotonNetwork.IsMasterClient) CheckAllPlayerReady();
    }
    #endregion


    /*
             ***|Operating structure|********************************************************************************
             *  % |At its core, it's an event system and doesn't require Start() or Update().| %                    *
             *                                                                                                      *
             *  *Joined Room                                                                                        *
             *      OnJoinedRoom() -> SpawnPlayer(RPC)                                                              *
             *  ->                                                                                                  *
             *  *Press Ready Button                                                                                 *
             *      OnClickReadyButtonEventListener() -> CheckPlayerReady(RPC) -> CheckAllPlayerReady()             *
             *          -> startBtn Activate                                                                        *
             *  ->                                                                                                  *
             *  *Press Start Button                                                                                 *
             *      OnClickStartGameEventListener() -> StartGame(RPC) -> StartRoundCoroutine() -> StartRound(RPC)   *
             *          -> ResetList(RPC) -> DistributeCards(RPC)                                                   *
             *  ->                                                                                                  *
             *  *Decide Win Count                                                                                   *
             *      StartDecideWinCount(RPC) -> ProcessDecideWinCount(RPC)                                          *
             *          -> if (player is AI): straight SubmitWinCount(RPC)                                          *
             *          -> if (player is non AI): WaitForPlayerWinCountSubmit() -> SubmitWinCount(RPC)              *
             *  ->                                                                                                  *
             *  *Circle Turn 4 time                                                                                 *
             *      StartTurn() -> ProcessTurn(PRC) -> SubmitCard(Coroutine)                                        *
             *          if (All Player submit card) CheckTurnResult(RPC) -> UpdateTurnWinner(RPC)                   *
             *          -> StartNextTurn()                                                                          *
             *  ->                                                                                                  *
             *  *Round End                                                                                          *
             *      StartRoundEnd(RPC) -> ProcessRoundEnd() -> CheckPlayerResuit()                                  *
             *          if (Failed expect winning count) ProcessAIBombPenalty() OR ProcessPlayerBombPenalty()       *
             *          -> PrepareNextRound(RPC)                                                                    *
             *  ->                                                                                                  *
             *  *Game End check                                                                                     *
             *      if(player count is 1 OR current turn bigger then max round) EndGame(RPC)                        *
             *      else Turn back StartRound(RPC)                                                                  *
             *                                                                                                      *
             ********************************************************************************************************
     */


    /*
        rest operating list

        1. switch masterclient => alert <- Done
        2. if player disconnected, that player dead. <- Done
        3. Show Player Nickname <- yet
        4. Send Winning or losing data to databass table <- yet need other contributes's step 
     
     */
    #region MultiPlayFuncLines
    /// <summary>
    /// A button function used in multiplayer. 
    /// This function should be called instead of the existing isPlayer function that sets the Ready button to true when pressed, 
    /// and when called, calls the CheckPlayerReady function, which sends an RPC to each player's Ready status.
    /// </summary>

    #region ButtonFuncLines
    public void  OnClickReadyButtonEventListener()
    {
        photonView.RPC("CheckPlayerReady", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        //Destroy(this.gameObject);
    }

    public void OnClickStartGameEventListener()
    {
        if (PhotonNetwork.IsMasterClient)
        { 
            photonView.RPC("StartGame", RpcTarget.All);
            isGameReady = true;
        }
    }
    #endregion

    void CheckAllPlayerReady()
    {
        if (playerList.All(player =>
        player.GetComponent<PlayerController>().isReady))
        {
            startBtn.SetActive(true);
            //startBtn.GetComponent<Button>().onClick.AddListener(OnClickStartGameEventListener);
        }
    }

    #region GameStartWithRoundProgress
    [PunRPC]
    void StartGame()
    {
        startBtn.SetActive(false);
        Debug.LogWarning("Start*****************************");

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(StartRoundCoroutine());
        //Invoke(nameof(StartRound), 3f);
    }
    
    [PunRPC]
    void UpdateLeaderPlayer(int newLeaderIndex)
    {
        leaderIndex = newLeaderIndex;
        // playerList는 모든 클라이언트에서 동일한 순서로 유지된다고 가정합니다.
        leaderPlayer = playerList[newLeaderIndex];
        // 필요하다면 UI 등에서 리더 플레이어를 표시하는 추가 로직을 넣습니다.
    }

    [PunRPC]
    void StartRound()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log($"*********************{curRound} - Round Start");

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ResetLists", RpcTarget.All);
            photonView.RPC("DistributeCards", RpcTarget.All);
            photonView.RPC("StartDecideWinCount", RpcTarget.All);
            StartCoroutine(StartTurnCoroutine());
        }
    }

    [PunRPC]
    void StartDecideWinCount()
    {
        Debug.LogError("Client Checker StartDecideWinCount ****************");
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("***************Win Decide");
            photonView.RPC("ProcessDecideWinCount", RpcTarget.All, 0);
        }
    }

    [PunRPC]
    void ProcessDecideWinCount(int playerID)
    {
        int actualPlayerIndex = (leaderIndex + playerID) % playerList.Count;
        GameObject currentPlayer = playerList[actualPlayerIndex];
        Debug.Log($"{actualPlayerIndex + 1}번째 순서의 플레이어: {currentPlayer.name}");

        string playerName = playerList[actualPlayerIndex].name;
        Debug.LogWarning($"{actualPlayerIndex + 1}번 째 순서 {playerName}입니다.");
        noticeturnText.text += $"{actualPlayerIndex + 1}번 째 순서 : {playerName}\n";

        if (playerList[actualPlayerIndex].GetComponent<PhotonView>().IsMine)
        {
            if (playerList[actualPlayerIndex].GetComponent<AIPlayer>().isAIPlayer)
            {
                int aiWinCount = playerList[actualPlayerIndex].GetComponent<AIPlayer>().CalculateOddsOfWinning(0.69f, 0.29f);
                photonView.RPC("SubmitWinCount", RpcTarget.All, actualPlayerIndex, aiWinCount);
            }
            else
            {
                StartCoroutine(WaitForPlayerWinCountSubmit(actualPlayerIndex));
            }
        }
    }

    IEnumerator WaitForPlayerWinCountSubmit(int playerID)
    {
        LogText.text = "";
        LogText.DOText($"{playerList[playerID].name}님이 승수를 선택할 차례입니다.", 1);
        buttonManager.showWinBtn(playerID, playerList[playerID].GetComponent<PlayerController>().winBtn);
        buttonManager.ShowPlayerPanel(true);

        yield return new WaitUntil(() => selectedWin == 0);

        photonView.RPC("SubmitWinCount", RpcTarget.All, playerID, predictedWinCnt[playerID]);

        buttonManager.ShowPlayerPanel(false);
        buttonManager.hideWinBtn(playerList[playerID].GetComponent<PlayerController>().winBtn);

        selectedWin = -1;
    }

    [PunRPC]
    void SubmitWinCount(int playerID, int winCount)
    {
        predictedWinCnt[playerID] = winCount;
        Debug.Log($"{playerList[playerID].name}의 승수 선언 : {winCount}승");

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ProcessDecideWinCount", RpcTarget.All, playerID + 1);
        }
    }

    [PunRPC]
    void ResetLists()
    {
        Debug.LogError("Client Check ResetList()**********");
        predictedWinCnt = new int[playerList.Count];
        winCntOfEachTurn = new int[playerList.Count];

        cardManager.GetComponent<CardManager>().ResetCardSet();
        for (int i = 0; i < playerList.Count; i++)
        {
            playerList[i].GetComponent<PlayerController>().cardList.Clear();
        }
    }
    [PunRPC]
    void DistributeCards()
    {
        Debug.LogError("Client Checker DistributeCards **********");
        cardManager.DoCardShuffle();
        cardManager.TestUserCard(playerList.Count, true);
    }
    void StartTurn()
    {
        if(!PhotonNetwork.IsMasterClient) return;

        Debug.LogWarning("*******************turn Start");
        photonView.RPC("ProcessTurn", RpcTarget.All, 0);
    }

    [PunRPC]
    void ProcessTurn(int playerID)
    {
        if(playerID >= playerList.Count)
        {
            if(PhotonNetwork.IsMasterClient)
                photonView.RPC("CheckTurnResult", RpcTarget.All);
            return;
        }

        Debug.Log($"{playerList[playerID]}'s turn");

        if (playerList[playerID].GetComponent<PhotonView>().IsMine)
        {
            if (playerList[playerID].GetComponent<AIPlayer>().isAIPlayer)
            {
                StartCoroutine(SubmitCard(playerID));
            }
            else
            {
                StartCoroutine(SubmitCard(playerID));
            }
        }
    }

    [PunRPC]
    IEnumerator SubmitCard(int playerID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (playerList[playerID].GetComponent<PlayerController>().isAIPlayer || playerList[playerID].GetComponent<AIPlayer>().isAIPlayer)
            {
                StartCoroutine(playerList[playerID].GetComponent<AIPlayer>().AITurn());
            }
            else
            {
                // 현재 플레이어의 카드 리스트 가져오기
                List<int> curCardList = playerList[playerID].GetComponent<PlayerController>().cardList;
                string playerName = playerList[playerID].name;


                // 현재 플레이어의 카드만 표시
                LogText.text = "";
                LogText.text = playerName + " 카드 선택 하세요";
                buttonManager.ShowCard(curCardList, playerID);

                //// 플레이어가 카드를 제출할 때까지 대기
                yield return new WaitUntil(() => checkSubmitCard == 0);

                checkSubmitCard = -1;
            }

            //// 턴 이동
            if(PhotonNetwork.IsMasterClient &&  submitCardList.Count >= playerList.Count)
            {
                photonView.RPC("ProcessTurn", RpcTarget.All, playerID + 1);
            }
        }
    }

    [PunRPC]
    void CheckTurnResult()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("************* Find Winner");

        for (int i = 0; i < playerList.Count; i++)
        {
            bool checker = cardManager.CardCompare(submitCardList, i);
            string playerName = playerList[i].name;

            if (checker)
            {
                photonView.RPC("UpdateTurnWinner", RpcTarget.All, i);
            }
        }
        StartNextTurn();
    }

    [PunRPC]
    void UpdateTurnWinner(int winnerID)
    {
        winCntOfEachTurn[winnerID]++;
        string playerName = playerList[winnerID].name;

        Debug.Log($"이번 턴의 승자는 {playerName}! (현재 {winCntOfEachTurn[winnerID]}승");
        LogText.text = "";
        LogText.DOText($"이번 턴의 승자는 : {playerName}! (현재{winCntOfEachTurn[winnerID]}승", 1f);
    }

    void StartNextTurn()
    {
        if(submitCardList.Count >= playerList.Count * 4)
        {
            photonView.RPC("StartRoundEnd", RpcTarget.All);
        }
        else
        {
            submitCardList.Clear();
            StartCoroutine(StartTurnCoroutine());
        }
    }

    [PunRPC]
    void StartRoundEnd()
    {
        Debug.Log("*****************result round");
        
        if(PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(ProcessRoundEnd());
        }
    }

    [PunRPC]
    void PlayerDead(int playerID)
    {
        GameObject deadPlayer = playerList[playerID];
        deadList.Add(deadPlayer);
    }
    
    [PunRPC]
    void PrepareNextRound()
    {
        RemovePlayerList();
        noticeturnText.text = "";
        curRound++;

        if(playerList.Count <= 1)
        {
            photonView.RPC("EndGame", RpcTarget.All);
        }
        else
        {
            if(curTurn > maxRound)
            {
                photonView.RPC("EndGame", RpcTarget.All);
            }
            else
            {
                if(PhotonNetwork.IsMasterClient)
                {
                    leaderIndex = (leaderIndex + 1) % playerList.Count;
                    photonView.RPC("UpdateLeaderPlayer", RpcTarget.All, leaderIndex);
                    StartCoroutine(StartRoundCoroutine());
                }
            }
        }


    }

    void RemovePlayerList()
    {
        for (int i = 0; i < deadList.Count; i++)
        {
            for (int j = 0; j < playerList.Count; j++)
            {
                if (playerList.Count <= maxPlayerCnt - deadList.Count)
                {
                    break;
                }
                else if (deadList[i] == playerList[j])
                {
                    playerList.Remove(playerList[j]);
                }
            }
        }
    }

    bool CheckGameEnd()
    {
        return deadList.Count >= maxPlayerCnt - 1;
    }

    [PunRPC]
    void EndGame()
    {
        if(playerList.Count == 1)
        {
            LogText.text = $"최후의 승자는 {playerList[0].gameObject.name}";
            string winnerName = playerList[0].GetComponent<PhotonView>().Owner.NickName;
            // 모든 참여 플레이어 리스트 만들기
            List<string> playerNames = new List<string>();
            foreach (var player in playerList)
            {
                playerNames.Add(player.GetComponent<PhotonView>().Owner.NickName);
            }

            // Firebase에 게임 결과 저장
            firebaseManager.SaveGameResult(winnerName, playerNames);

            // 현재 유저가 승자인지 확인하고 승패 업데이트
            string currentUserNick = PhotonNetwork.LocalPlayer.NickName;
            bool isWinner = (currentUserNick == winnerName);
            firebaseManager.UpdateUserWin(isWinner);
        }
    }
    #endregion
    #endregion

    #region CoroutineLines
    IEnumerator StartRoundCoroutine()
    {
        yield return new WaitForSeconds(3f);

        photonView.RPC("StartRound", RpcTarget.All);
    }

    IEnumerator StartTurnCoroutine()
    {
        yield return new WaitForSeconds(2f);
        StartTurn();
    }

    IEnumerator ProcessRoundEnd()
    {
        for(int i = 0; i< playerList.Count; i++) 
        {
            yield return StartCoroutine(CheckPlayerResult(i));

            if(CheckGameEnd())
            {
                photonView.RPC("EndGame", RpcTarget.All);
                yield break;
            }
        }

        yield return new WaitForSeconds(2f);
        photonView.RPC("PrepareNextRound", RpcTarget.All);
    }
    IEnumerator ProcessAIBombPenalty(int playerID)
    {
        yield return new WaitForSeconds(3f);
        StartCoroutine(playerList[playerID].GetComponent<AIPlayer>().AIDrawBomb());
    }

    IEnumerator ProcessPlayerBombPenalty(int playerID)
    {
        yield return StartCoroutine(scoreManager.CheckBomb(playerList[playerID].GetComponent<PlayerController>()));
    }

    IEnumerator CheckPlayerResult(int playerID)
    {
        if (predictedWinCnt[playerID] != winCntOfEachTurn[playerID])
        {
            if (playerList[playerID].GetComponent<AIPlayer>().isAIPlayer)
            {
                yield return StartCoroutine(ProcessAIBombPenalty(playerID));
            }
            else
            {
                yield return StartCoroutine(ProcessPlayerBombPenalty(playerID));
            }
        }
        yield return new WaitForSeconds(1f);
    }
    #endregion
}
