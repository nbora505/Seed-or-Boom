using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.PunBasics;

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

    public int selectedBomb = -1;
    public int selectedWin = -1;
    public int checkSubmitCard = -1;
    public int selectedCard = 0;
    public List<int> submitCardList; //제출된 카드리스트
    public int[] predictedWinCnt; //각각의 라운드마다 플레이어들이 예측한 승리 횟수
    public int[] winCntOfEachTurn; //각각의 턴마다 플레이어들이 기록한 승리 횟수

    public CardManager cardManager;
    public ScoreManager scoreManager;
    public ButtonManager buttonManager;
    public CameraManager cameraManager;

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

    [PunRPC]
    void DisconnectedUserDeadAnimation(int playerID)
    {
        playerList[playerID - 1].GetComponent<Animator>().Play("Death");
        Invoke("RemoveDisconnectUserFromList", 2f);
    }

    [PunRPC]
    void RemoveDisconnectUserFromList(int playerID)
    {
        playerList.Remove(playerList[playerID - 1]);
    }

    #region PunCallBacksLines
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SpwanPlayer", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

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

    #region PunRPCLines
    [PunRPC]
    void SpwanPlayer(int actorNumberID)
    {
        Photon.Realtime.Player newPlayer = PhotonNetwork.PlayerList.FirstOrDefault(
            player => player.ActorNumber == actorNumberID);

        int characterSelectIndex = (int)newPlayer.CustomProperties["CharacterIndex"];
        GameObject selectedCharacter = characterPrefabs[characterSelectIndex];

        GameObject player = PhotonNetwork.Instantiate(selectedCharacter.name,
            spawnPoints[(actorNumberID - 1) % spawnPoints.Length].position,
            Quaternion.identity);

        playerList.Add(player);
    }

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
        3. Show Player Nickname
        4. Send Winning or losing data to databass table
     
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
            startBtn.SetActive(true);
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
        if(!PhotonNetwork.IsMasterClient) return;

        Debug.Log("***************Win Decide");
        photonView.RPC("ProcessDecideWinCount",RpcTarget.All, 0);
    }

    [PunRPC]
    void ProcessDecideWinCount(int playerID)
    {
        if(playerID >= playerList.Count)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(StartTurnCoroutine());
            }
            return;
        }

        string playerName = playerList[playerID].name;
        Debug.LogWarning($"{playerID + 1}번 째 순서 {playerName}입니다.");
        noticeturnText.text += $"{playerID + 1}번 째 순서 : {playerName}\n";

        if (playerList[playerID].GetComponent<PhotonView>().IsMine)
        {
            if (playerList[playerID].GetComponent<AIPlayer>().isAIPlayer)
            {
                int aiWinCount = playerList[playerID].GetComponent<AIPlayer>().CalculateOddsOfWinning(0.69f, 0.29f);
                photonView.RPC("SubmitWinCount", RpcTarget.All, playerID, aiWinCount);
            }
            else
            {
                StartCoroutine(WaitForPlayerWinCountSubmit(playerID));
            }
        }
    }

    IEnumerator WaitForPlayerWinCountSubmit(int playerID)
    {
        LogText.text = "";
        LogText.DOText($"{playerList[playerID].name}님이 승수를 선택할 차례입니다.", 1);
        buttonManager.showWinBtn();
        buttonManager.ShowPlayerPanel(true);

        yield return new WaitUntil(() => selectedWin == 0);

        photonView.RPC("SubmitWinCount", RpcTarget.All, playerID, predictedWinCnt[playerID]);

        buttonManager.ShowPlayerPanel(false);
        buttonManager.hideWinBtn();

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
        cardManager.DoCardShuffle();
        cardManager.TestUserCard(playerList.Count);
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
                buttonManager.ShowCard(curCardList);

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
/*
    void Start()
    {
        //플레이어 객체들 리스트에 추가
        GameObject[] tempPlayerList = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < maxPlayerCnt; i++)
        {
            playerList.Add(tempPlayerList[i]);
        }

        //리더 플레이어(맨 처음 시작할 사람) 정하기
        curTurn = Random.Range(0, playerList.Count);
        leaderPlayer = playerList[curTurn];

        //준비/시작버튼 대기
        StartCoroutine(ReadyToStart());
    }

    //플레이어들이 모두 Ready 상태인지 체크
    IEnumerator ReadyToStart()
    {
        //플레이어들이 준비버튼을 눌렀는지 확인. 확인만 순서대로 하는거지 준비버튼 누르는 단계가 순서대로 진행되는 건 아님!
        for (int i = 0; i < playerList.Count; i++)
        {
            //AI 플레이어인 경우에는 그냥 넘어가고...
            if (playerList[i].GetComponent<PlayerController>().isAIPlayer || playerList[i].GetComponent<AIPlayer>().isAIPlayer) ;
            //플레이어인 경우 상태가 isReady가 될 때까지 대기하다가 체크되면 다음 플레이어로 넘어가서 체크.
            else
            {
                yield return new WaitUntil(() => playerList[i].GetComponent<PlayerController>().isReady);

            }
        }

        //마지막 플레이어까지 넘어갔으면 게임 시작 버튼 활성화
        startBtn.SetActive(true);

        //게임 시작 버튼 눌릴 때까지 대기
        yield return new WaitUntil(() => isGameReady);

        //버튼이 눌리면 게임 시작
        startBtn.SetActive(false);
        Debug.Log("::::::::: 게임 시작!!! ::::::::");
        yield return new WaitForSeconds(3f);
        StartCoroutine(StartRound());
    }

    IEnumerator StartRound()
    {
        //리스트 초기화
        ResetLists();

        Debug.Log("=========Round " + curRound + " =========");

        //플레이어들에게 카드 나눠주기
        cardManager.DoCardShuffle();
        cardManager.TestUserCard(playerList.Count);

        //승수 결정받기;
        yield return StartCoroutine(DecideWinCnt());

        //4번의 턴 시작
        for (int i = 1; i <= 4; i++)
        {
            Debug.Log("=========Turn " + i + " =========");
            //승수 비교해서 승자를 가리는 함수에 필요한 '제출받은 카드 리스트'를 턴마다 초기화
            submitCardList.Clear();

            //카드 제출받기
            Debug.Log("=========카드 제출 단계=========");
            for (int j = 0; j < playerList.Count; j++)
            {
                yield return StartCoroutine(SubmitCard());
            }

            //제출한 카드 보고 승자 결정하기(카드매니저에 들어가 있는 함수 호출)
            Debug.Log("=========이번 턴 승자 결정=========");
            yield return StartCoroutine(CheckTurnResult());

        }

        //라운드가 끝날 때마다 승수 맞췄는지 판단, 벌칙 결정
        Debug.Log("=========이번 라운드 결과=========");
        for (int i = 0; i < playerList.Count; i++)
        {
            //승수 맞췄는지 판단
            yield return StartCoroutine(CheckRoundResult());

            //벌칙 단계가 끝날 때마다 최후의 1인이 남았는지 확인하기
            JudgeGameResult();

            curTurn++;
            if (curTurn >= playerList.Count) curTurn = 0;
        }
        yield return new WaitForSeconds(5f);

        //라운드 종료
        //사망 리스트에 있는 플레이어가 플레이어리스트에 아직 남아있을 경우 지워주기
        RemovePlayerList();
        noticeturnText.text = "";
        curRound++;

        if (playerList.Count <= 1)
        {
            LogText.text = $"최후의 승자는 {playerList[0].gameObject.name}";
        }
        else
        {
            if (curRound > maxRound)
            {
                Debug.Log("최대 라운드 초과");
            }
            else
            {
                StartCoroutine(StartRound());
            }
        }
    }

    IEnumerator DecideWinCnt()
    {
        Debug.Log("=========승수 선언 단계=========");

        //리더 플레이어부터 차례로 승수 선언
        for (int i = 0; i < playerList.Count; i++)
        {
            string playerName = playerList[curTurn].name;
            Debug.LogWarning(i + 1 + "번째 순서" + playerName + "입니다.");
            noticeturnText.text += (i + 1 + "번째 순서 " + playerName + "입니다.\n");

            //ai일 경우
            if (playerList[curTurn].GetComponent<PlayerController>().isAIPlayer || playerList[curTurn].GetComponent<AIPlayer>().isAIPlayer)
            {
                playerList[curTurn].GetComponent<AIPlayer>().expectedWins = playerList[curTurn].GetComponent<AIPlayer>().CalculateOddsOfWinning(0.69f, 0.29f);
                predictedWinCnt[curTurn] = playerList[curTurn].GetComponent<AIPlayer>().expectedWins;
            }
            else
            {
                //여기에서 플레이어 리스트[현재 차례]의 승수 선언 UI 활성화
                yield return new WaitForSeconds(2f);
                LogText.text = "";
                LogText.DOText(playerName + " 승 수 선택하세요", 1);
                buttonManager.showWinBtn();
                buttonManager.ShowPlayerPanel(true);
                yield return new WaitUntil(() => selectedWin == 0);
                buttonManager.ShowPlayerPanel(false);
                buttonManager.hideWinBtn();
                selectedWin = -1;
            }

            Debug.Log(playerName + "의 승수 선언 : " + predictedWinCnt[curTurn] + "승");

            curTurn++;
            if (curTurn >= playerList.Count) curTurn = 0;

            yield return new WaitForSeconds(1f);
        }
        yield return null;
    }

    IEnumerator SubmitCard()
    {
        // 만약에 겟 컴포넌트를 했을 때, 그게 널이면, 
        if (playerList[curTurn].GetComponent<PlayerController>().isAIPlayer || playerList[curTurn].GetComponent<AIPlayer>().isAIPlayer)
        {
            StartCoroutine(playerList[curTurn].GetComponent<AIPlayer>().AITurn());
        }
        else
        {
            // 현재 플레이어의 카드 리스트 가져오기
            List<int> curCardList = playerList[curTurn].GetComponent<PlayerController>().cardList;
            string playerName = playerList[curTurn].name;


            yield return new WaitForSeconds(2f);
            // 현재 플레이어의 카드만 표시
            LogText.text = "";
            LogText.text = playerName + " 카드 선택 하세요";
            buttonManager.ShowCard(curCardList);

            // 플레이어가 카드를 제출할 때까지 대기
            yield return new WaitUntil(() => checkSubmitCard == 0);

            checkSubmitCard = -1;
        }

        // 턴 이동
        curTurn++;
        if (curTurn >= playerList.Count)
        {
            curTurn = 0;
        }

    }

    IEnumerator CheckTurnResult()
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            bool checker = cardManager.CardCompare(submitCardList, i);
            string playerName = playerList[curTurn].name;

            if (checker) //checker의 반환값이 true면...
            {
                //playerList[curTurn]이 이번 턴 승자라는 뜻!
                winCntOfEachTurn[curTurn]++;
                Debug.Log("이번 턴의 승자는 " + playerList[curTurn] + "! (현재 " + winCntOfEachTurn[curTurn] + "승)");
                LogText.text = "";
                LogText.DOText("이번 턴의 승자는 : " + playerName + "! (현재 " + winCntOfEachTurn[curTurn] + "승", 1f);
                yield return new WaitForSeconds(1.5f);
            }
            curTurn++;
            if (curTurn >= playerList.Count) curTurn = 0;
        }
        yield return new WaitForSeconds(3f);
    }

    IEnumerator CheckRoundResult()
    {
        GameObject curPlayer = playerList[curTurn];

        if (predictedWinCnt[curTurn] == winCntOfEachTurn[curTurn])
        {
            Debug.Log(curPlayer + " 예측 성공!");
        }
        else
        {
            Debug.Log(curPlayer + " 예측 실패...");

            //실패한 플레이어한테 폭탄 심지 등장시키게 하기
            if (curPlayer.GetComponent<PlayerController>().isAIPlayer || curPlayer.GetComponent<AIPlayer>().isAIPlayer)
            {
                yield return StartCoroutine(curPlayer.GetComponent<AIPlayer>().AIDrawBomb());
            }
            else
            {
                yield return StartCoroutine(scoreManager.CheckBomb(curPlayer.GetComponent<PlayerController>()));
            }
        }
        yield return new WaitForSeconds(1f);
    }

    //사망 리스트에 있는 플레이어가 플레이어리스트에 아직 남아있을 경우 지워주는 함수
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

    //게임에 최후의 1인이 남았는지 판단하는 함수
    void JudgeGameResult()
    {
        if (deadList.Count == maxPlayerCnt - 1)
        {
            RemovePlayerList();
            Debug.Log("::::: 게임 종료! :::::");
            Debug.Log("::::: 승자는 " + playerList[0] + "! :::::");
        }
    }

    //리스트 초기화
    void ResetLists()
    {
        if (curTurn > playerList.Count - 1)
        {
            curTurn = 0;
        }

        predictedWinCnt = new int[playerList.Count];
        winCntOfEachTurn = new int[playerList.Count];

        cardManager.GetComponent<CardManager>().ResetCardSet();
        for (int i = 0; i < playerList.Count; i++)
        {
            playerList[i].GetComponent<PlayerController>().cardList.Clear();
        }
    }
*/
}
