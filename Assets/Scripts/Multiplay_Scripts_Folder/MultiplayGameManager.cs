using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;

public class MultiplayGameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Public Fields & UI
    [Header("Game Objects & UI")]
    public List<GameObject> playerList;         // 플레이어 리스트 (0 기반 인덱스)
    public List<GameObject> deadList;
    public GameObject startBtn;
    public GameObject leaderPlayer;
    public Text noticeturnText;
    public Text LogText;

    [Header("Game Settings")]
    public bool isGameReady = false;
    public int maxPlayerCnt = 4;
    public int curRound = 1;
    public int maxRound = 3;
    public int curTurn;
    public int maxCardCnt = 5;
    public int leaderIndex = 0; // 플레이어 리스트 내 리더의 0 기반 인덱스

    [Header("Game State Variables")]
    public int selectedBomb = -1;
    public int selectedWin = -1;
    public int checkSubmitCard = -1;
    public int selectedCard = 0;
    public List<int> submitCardList = new List<int>(); // 제출된 카드 리스트

    // 동기화할 배열들 (플레이어 리스트의 0 기반 인덱스 순서를 기준으로 함)
    public int[] predictedWinCnt;  // 각 플레이어별 예상 승수
    public int[] winCntOfEachTurn; // 각 턴별 승리 횟수 기록

    [Header("Managers")]
    public CardManager cardManager;
    public ScoreManager scoreManager;
    public MultiPlayBtnManager buttonManager;
    public CameraManager cameraManager;
    public FirebaseManager firebaseManager;

    [Tooltip("Character's Prefabs")]
    public GameObject[] characterPrefabs;
    [Tooltip("Spawn Points")]
    public Transform[] spawnPoints;

    // 승수 선택 단계가 모두 끝났는지 확인 (동기화 대상)
    public bool winCountSelectionComplete = false;
    #endregion

    public void OnStartButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // RpcTarget.AllBuffered를 사용하여 나중에 접속하는 클라이언트에게도 적용
            photonView.RPC("RPC_StartGame", RpcTarget.AllBuffered);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }
    #region MonoBehaviour & Photon Setup
    private void Awake()
    {
        // Photon 동기화 문제 방지를 위해 자동 씬 동기화 비활성화
        PhotonNetwork.AutomaticallySyncScene = false;
    }

    [PunRPC]
    public void RPC_UpdatePlayerList()
    {
        // "Player" 태그가 붙은 모든 오브젝트를 찾아 ActorNumber 기준(오름차순)으로 정렬
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        Debug.LogError($"Found players count: {players.Length}"); // 디버그 로그 추가

        playerList = new List<GameObject>(
            players.OrderBy(p => p.GetComponent<PhotonView>().Owner.ActorNumber)
        );
        Debug.Log($"UpdatePlayerList: playerList Count = {playerList.Count}");
    }

    public void UpdatePlayerList()
    {
        photonView.RPC("RPC_UpdatePlayerList", RpcTarget.All);
    }

    #region MonoBehaviour Methods
    private void Start()
    {
        // 씬 로드 후 또는 시작 시에 플레이어 리스트를 업데이트합니다.
        UpdatePlayerList();
    }

    private new void OnEnable()
    {
        // 씬이 로드될 때마다 업데이트하도록 구독
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private new void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name == "Map1")
        {
            // 게임씬 로드 후 각 클라이언트에서 플레이어 리스트를 다시 업데이트
            Debug.Log("Scene loaded: Map1");
            StartCoroutine(DelayedPlayerListUpdate());
        }
    }

    IEnumerator DelayedPlayerListUpdate()
    {
        // 다른 클라이언트에서 플레이어 리스트에 나 자신이 포함되지 않은 것을 확인함.
        // 일단 무식하게 생성될 때까지 약 3초간의 지연을 주는 것으로 트라이.
        yield return new WaitForSeconds(3f); // 약간의 지연
        UpdatePlayerList();
    }

    #endregion
    // IPunObservable 인터페이스 구현 – 마스터가 쓰고 나머지가 읽음
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(winCountSelectionComplete);
            stream.SendNext(predictedWinCnt);
            stream.SendNext(winCntOfEachTurn);
        }
        else
        {
            winCountSelectionComplete = (bool)stream.ReceiveNext();
            predictedWinCnt = (int[])stream.ReceiveNext();
            winCntOfEachTurn = (int[])stream.ReceiveNext();
        }
    }
    #endregion

    #region Photon Callbacks
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();

        // 여기서 actorNumber(Photon의 1부터 시작하는 번호)를 사용
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_DisconnectedUserDeadAnimation", RpcTarget.All, otherPlayer.ActorNumber);
            LogText.text = $"플레이어 {otherPlayer.NickName}가 방을 떠났습니다.";
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        LogText.text = $"현재 방장은 {newMasterClient.NickName}입니다.";
    }
    #endregion

    #region RPC Methods
    // 액터 넘버를 사용하여 플레이어 찾기 – 액터 넘버는 Photon에서 1부터 시작함.
    [PunRPC]
    void RPC_DisconnectedUserDeadAnimation(int actorNumber)
    {
        GameObject player = playerList.Find(p => p.GetComponent<PhotonView>().Owner.ActorNumber == actorNumber);
        if (player != null)
            player.GetComponent<Animator>().Play("Death");
        photonView.RPC("RPC_RemoveDisconnectUserFromList", RpcTarget.All, actorNumber);
    }

    [PunRPC]
    void RPC_RemoveDisconnectUserFromList(int actorNumber)
    {
        GameObject player = playerList.Find(p => p.GetComponent<PhotonView>().Owner.ActorNumber == actorNumber);
        if (player != null)
            playerList.Remove(player);
    }

    // 여기서는 액터 넘버 대신 플레이어 리스트의 0 기반 인덱스(playerIndex)를 사용함
    [PunRPC]
    public void RPC_CheckPlayerReady(int actorNumber)
    {
        GameObject player = playerList.Find(p => p.GetComponent<PhotonView>().Owner.ActorNumber == actorNumber);
        if (player != null)
            player.GetComponent<PlayerController>().isReady = true;

        if (PhotonNetwork.IsMasterClient)
            CheckAllPlayerReady();
    }

    [PunRPC]
    public void RPC_StartGame()
    {
        startBtn.SetActive(false);
        LogText.text = "게임 시작!";
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(StartRoundCoroutine());
    }

    // 리더 업데이트 : 플레이어 리스트의 인덱스를 사용
    [PunRPC]
    void RPC_UpdateLeaderPlayer(int newLeaderIndex)
    {
        leaderIndex = newLeaderIndex;
        leaderPlayer = playerList[newLeaderIndex];
    }

    [PunRPC]
    void RPC_ResetLists()
    {
        // 플레이어 리스트 순서(인덱스)를 기준으로 배열 초기화
        predictedWinCnt = new int[playerList.Count];
        winCntOfEachTurn = new int[playerList.Count];
        winCountSelectionComplete = false;
        cardManager.ResetCardSet();
        foreach (var player in playerList)
        {
            player.GetComponent<PlayerController>().cardList.Clear();
        }
    }

    // 승수 선택이 완료된 후 카드 분배 진행
    [PunRPC]
    void RPC_DistributeCards()
    {
        cardManager.DoCardShuffle();
        cardManager.TestUserCard(playerList.Count, true);
    }

    // 플레이어 리스트 내 0 기반 인덱스(playerIndex)를 사용 – 여기서는 액터 넘버와 달리 UI나 배열 접근에 쓰임
    [PunRPC]
    void RPC_SubmitWinCount(int playerIndex, int winCount)
    {
        Debug.Log($"[SubmitWinCount] Player Index: {playerIndex}, Win Count: {winCount}, Leader Index: {leaderIndex}");

        if (predictedWinCnt == null || playerIndex < 0 || playerIndex >= predictedWinCnt.Length)
        {
            Debug.LogError($"RPC_SubmitWinCount: playerIndex {playerIndex} out of range. Array length: {predictedWinCnt?.Length}");
            return;
        }
        predictedWinCnt[playerIndex] = winCount;

        if (playerIndex < 0 || playerIndex >= playerList.Count)
        {
            Debug.LogError($"RPC_SubmitWinCount: playerIndex {playerIndex} out of range in playerList. Count: {playerList.Count}");
            return;
        }
        LogText.text = $"{playerList[playerIndex].name} 승수: {winCount}";

        // 다음 플레이어의 승수 입력 순서를 진행 (현재 순서는 playerIndex 기준이 아닌 leader 기준의 순번)
        if (PhotonNetwork.IsMasterClient)
        {
            int currentOrder = (playerIndex - leaderIndex + playerList.Count) % playerList.Count;

            int nextOrder = currentOrder + 1;

            Debug.Log($"Current Order: {currentOrder}, Next Order: {nextOrder}");

            // 다음 순서는 현재 playerIndex + 1를 의미 (leader 기준 순서 계산)
            photonView.RPC("RPC_ProcessDecideWinCount", RpcTarget.AllBuffered, nextOrder);
        }
    }

    // 승수 선택 프로세스 : nextPlayerOrder는 리더 기준 순서 (0부터 시작)
    [PunRPC]
    void RPC_ProcessDecideWinCount(int nextPlayerOrder)
    {
        if (playerList == null || playerList.Count == 0)
        {
            Debug.LogError("PlayerList is empty or null!");
            UpdatePlayerList(); // 리스트 재업데이트 시도
            return;
        }

        Debug.Log($"[ProcessDecideWinCount] nextPlayerOrder: {nextPlayerOrder}, playerList.Count: {playerList.Count}, leaderIndex: {leaderIndex}");

        if (nextPlayerOrder >= playerList.Count)
        {
            bool allPlayersSubmitted = true;
            for (int i = 0; i < playerList.Count; i++)
            {
                if (predictedWinCnt[i] == 0)  // 아직 제출하지 않은 플레이어가 있는 경우
                {
                    allPlayersSubmitted = false;
                    break;
                }
            }

            if (allPlayersSubmitted)
            {
                winCountSelectionComplete = true;
                return;
            }
        }
        // leaderIndex를 기준으로 실제 플레이어 리스트 내 0 기반 인덱스 계산
        int playerIndex = (leaderIndex + nextPlayerOrder) % playerList.Count;
        GameObject currentPlayer = playerList[playerIndex];

        Debug.Log($"[ProcessDecideWinCount] Processing player {playerIndex}: {currentPlayer.name}");
        Debug.Log($"Current predictedWinCnt status: {string.Join(", ", predictedWinCnt)}");

        noticeturnText.text += $"{playerIndex + 1}번째: {currentPlayer.name}\n";

        PhotonView pv = currentPlayer.GetComponent<PhotonView>();
        if (pv.IsMine)
        {
            // AI인지 사람인지 구분
            AIPlayer ai = currentPlayer.GetComponent<AIPlayer>();
            if (ai != null && ai.isAIPlayer)
            {
                int aiWin = ai.CalculateOddsOfWinning(0.69f, 0.29f);
                photonView.RPC("RPC_SubmitWinCount", RpcTarget.All, playerIndex, aiWin);
            }
            else
            {
                Debug.Log($"[ProcessDecideWinCount] Human Player {currentPlayer.name} waiting for input");
                StartCoroutine(WaitForPlayerWinCountSubmit(playerIndex));
            }
        }
    }

    // 턴 진행 RPC – turnOrder는 현재 턴 순서에 따른 플레이어 리스트의 인덱스 (0부터 시작)
    [PunRPC]
    void RPC_ProcessTurn(int turnOrder)
    {
        if (turnOrder >= playerList.Count)
        {
            photonView.RPC("RPC_CheckTurnResult", RpcTarget.All);
            return;
        }
        GameObject currentPlayer = playerList[turnOrder];
        if (currentPlayer.GetComponent<PhotonView>().IsMine)
        {
            AIPlayer ai = currentPlayer.GetComponent<AIPlayer>();
            if (ai != null && ai.isAIPlayer)
            {
                StartCoroutine(ai.AITurn());
            }
            else
            {
                photonView.RPC("RPC_ShowCardUI", RpcTarget.All, turnOrder);
            }
        }
    }

    [PunRPC]
    void RPC_CheckTurnResult()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        for (int i = 0; i < playerList.Count; i++)
        {
            bool isWinner = cardManager.CardCompare(submitCardList, i);
            if (isWinner)
            {
                photonView.RPC("RPC_UpdateTurnWinner", RpcTarget.All, i);
            }
        }
        StartNextTurn();
    }

    [PunRPC]
    void RPC_UpdateTurnWinner(int playerIndex)
    {
        winCntOfEachTurn[playerIndex]++;
        LogText.text = $"{playerList[playerIndex].name} 턴 승리!";
    }

    [PunRPC]
    void RPC_StartRoundEnd()
    {
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(ProcessRoundEnd());
    }

    [PunRPC]
    void RPC_PrepareNextRound()
    {
        RemovePlayerList();
        noticeturnText.text = "";
        curRound++;

        if (playerList.Count <= 1 || curRound > maxRound)
        {
            photonView.RPC("RPC_EndGame", RpcTarget.All);
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                leaderIndex = (leaderIndex + 1) % playerList.Count;
                photonView.RPC("RPC_UpdateLeaderPlayer", RpcTarget.All, leaderIndex);
                StartCoroutine(StartRoundCoroutine());
            }
        }
    }

    [PunRPC]
    void RPC_EndGame()
    {
        if (playerList.Count == 1)
        {
            LogText.text = $"최후의 승자: {playerList[0].name}";
            string winnerName = playerList[0].GetComponent<PhotonView>().Owner.NickName;
            List<string> playerNames = playerList.Select(p => p.GetComponent<PhotonView>().Owner.NickName).ToList();
            firebaseManager.SaveGameResult(winnerName, playerNames);
            bool isWinner = (PhotonNetwork.LocalPlayer.NickName == winnerName);
            firebaseManager.UpdateUserWin(isWinner);
        }
    }

    // UI용 RPC – 카드 제출 UI 활성화 (playerIndex는 플레이어 리스트 내 인덱스)
    [PunRPC]
    void RPC_ShowCardUI(int playerIndex)
    {
        if (playerList[playerIndex].GetComponent<PhotonView>().IsMine)
        {
            List<int> curCardList = playerList[playerIndex].GetComponent<PlayerController>().cardList;
            buttonManager.ShowCard(curCardList, playerIndex);
        }
    }

    // 모든 클라이언트에 시작 버튼 활성화 (Buffered RPC)
    [PunRPC]
    void RPC_UpdateStartButtonUI()
    {
        startBtn.SetActive(true);
    }
    #endregion

    #region Helper Methods & Coroutines
    // 모든 플레이어가 준비되었는지 검사하여 시작 버튼을 활성화
    void CheckAllPlayerReady()
    {
        if (playerList.All(p => p.GetComponent<PlayerController>().isReady))
        {
            startBtn.SetActive(true);
            photonView.RPC("RPC_UpdateStartButtonUI", RpcTarget.AllBuffered);
        }
    }

    // 승수 선택 후 카드 분배 및 턴 진행을 위한 코루틴
    public IEnumerator StartRoundCoroutine()
    {
        yield return new WaitForSeconds(3f);
        // 상태 초기화를 위해 Buffered RPC 호출
        photonView.RPC("RPC_ResetLists", RpcTarget.AllBuffered);

        // 승수 선택 단계 시작 (Buffered)
        photonView.RPC("RPC_StartDecideWinCount", RpcTarget.AllBuffered);

        yield return new WaitUntil(() => winCountSelectionComplete);

        // 승수 선택 완료 후 카드 분배 및 턴 진행
        photonView.RPC("RPC_DistributeCards", RpcTarget.AllBuffered);
        StartCoroutine(StartTurnCoroutine());
    }

    // 승수 선택 시작 RPC 호출 – 마스터 클라이언트에서 시작
    [PunRPC]
    void RPC_StartDecideWinCount()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 순서 번호 0부터 시작하여 승수 선택 진행
            photonView.RPC("RPC_ProcessDecideWinCount", RpcTarget.All, 0);
        }
    }

    IEnumerator StartTurnCoroutine()
    {
        yield return new WaitForSeconds(2f);
        StartTurn();
    }

    // 마스터 클라이언트에서 턴 진행을 제어
    void StartTurn()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("RPC_ProcessTurn", RpcTarget.All, 0);
    }

    // 턴이 끝나면 다음 턴 또는 라운드 종료를 진행
    void StartNextTurn()
    {
        if (submitCardList.Count >= playerList.Count * 4)
        {
            photonView.RPC("RPC_StartRoundEnd", RpcTarget.All);
        }
        else
        {
            submitCardList.Clear();
            StartCoroutine(StartTurnCoroutine());
        }
    }

    // 사람 플레이어의 승수 입력 대기 코루틴 (playerIndex: 0 기반 인덱스)
    IEnumerator WaitForPlayerWinCountSubmit(int playerIndex)
    {
        LogText.text = $"{playerList[playerIndex].name} 승수 선택 중...";
        // UI 버튼 표시 (playerIndex 기준)
        buttonManager.showWinBtn(playerIndex, playerList[playerIndex].GetComponent<PlayerController>().winBtn);
        buttonManager.ShowPlayerPanel(true);

        // 플레이어가 승수를 선택할 때까지 대기 (UI에서 predictedWinCnt[playerIndex]가 설정되고, selectedWin가 변경되어야 함)
        yield return new WaitUntil(() => selectedWin != -1 && predictedWinCnt[playerIndex] != 0);

        photonView.RPC("RPC_SubmitWinCount", RpcTarget.All, playerIndex, predictedWinCnt[playerIndex]);

        buttonManager.ShowPlayerPanel(false);
        buttonManager.hideWinBtn(playerList[playerIndex].GetComponent<PlayerController>().winBtn);
        selectedWin = -1;
    }

    // 각 플레이어의 라운드 결과를 확인하여 폭탄 처리 등의 로직 진행
    IEnumerator ProcessRoundEnd()
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            yield return StartCoroutine(CheckPlayerResult(i));
            if (CheckGameEnd())
            {
                photonView.RPC("RPC_EndGame", RpcTarget.All);
                yield break;
            }
        }
        yield return new WaitForSeconds(2f);
        photonView.RPC("RPC_PrepareNextRound", RpcTarget.All);
    }

    // 각 플레이어별 승수 결과 검사 (playerIndex 기준)
    IEnumerator CheckPlayerResult(int playerIndex)
    {
        if (predictedWinCnt[playerIndex] != winCntOfEachTurn[playerIndex])
        {
            PlayerController controller = playerList[playerIndex].GetComponent<PlayerController>();
            AIPlayer ai = playerList[playerIndex].GetComponent<AIPlayer>();
            if (ai != null && ai.isAIPlayer)
                yield return StartCoroutine(ai.AIDrawBomb());
            else
                yield return StartCoroutine(scoreManager.CheckBomb(controller));
        }
        yield return new WaitForSeconds(1f);
    }

    // deadList에 포함된 플레이어 제거
    void RemovePlayerList()
    {
        foreach (GameObject dead in deadList)
        {
            if (playerList.Contains(dead))
                playerList.Remove(dead);
        }
    }

    // 게임 종료 조건 검사 (예: 최대 플레이어 수 - 1 이상의 플레이어가 제거되었을 때)
    bool CheckGameEnd()
    {
        return deadList.Count >= maxPlayerCnt - 1;
    }
    #endregion
}
