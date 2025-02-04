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
    [Header("Game Objects & UI")]
    public List<GameObject> playerList;
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
    public int leaderIndex = 0; // 현재 리더 플레이어의 인덱스

    [Header("Game State Variables")]
    public int selectedBomb = -1;
    public int selectedWin = -1;
    public int checkSubmitCard = -1;
    public int selectedCard = 0;
    public List<int> submitCardList = new List<int>(); // 제출된 카드 리스트

    // 동기화할 배열들
    public int[] predictedWinCnt;  // 플레이어별 예상 승수
    public int[] winCntOfEachTurn; // 턴별 승리 횟수 기록

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

    // 승수 선택 단계가 모두 끝났는지 확인하기 위한 플래그 (동기화 대상)
    public bool winCountSelectionComplete = false;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = false; // 방 동기화 문제 방지
    }

    public void UpdatePlayerList()
    {
        // "Player" 태그가 붙은 모든 오브젝트를 찾아서 ActorNumber 기준으로 정렬 후 리스트로 저장합니다.
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        playerList = new List<GameObject>(players.OrderBy(p => p.GetComponent<PhotonView>().Owner.ActorNumber));
        Debug.Log($"UpdatePlayerList: playerList Count = {playerList.Count}");
    }
    #region IPunObservable 구현
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // MasterClient가 쓰고, 나머지는 읽습니다.
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
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("DisconnectedUserDeadAnimation", RpcTarget.All, otherPlayer.ActorNumber);
            LogText.text = $"플레이어 {otherPlayer.NickName}가 방을 떠났습니다.";
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        LogText.text = $"현재 방장은 {newMasterClient.NickName}입니다.";
    }
    #endregion

    #region RPC Methods
    [PunRPC]
    void DisconnectedUserDeadAnimation(int playerID)
    {
        GameObject player = playerList.Find(p => p.GetComponent<PhotonView>().Owner.ActorNumber == playerID);
        if (player != null)
            player.GetComponent<Animator>().Play("Death");
        photonView.RPC("RemoveDisconnectUserFromList", RpcTarget.All, playerID);
    }

    [PunRPC]
    void RemoveDisconnectUserFromList(int playerID)
    {
        GameObject player = playerList.Find(p => p.GetComponent<PhotonView>().Owner.ActorNumber == playerID);
        if (player != null)
            playerList.Remove(player);
    }

    [PunRPC]
    public void CheckPlayerReady(int playerID)
    {
        GameObject player = playerList.Find(p => p.GetComponent<PhotonView>().Owner.ActorNumber == playerID);
        if (player != null)
            player.GetComponent<PlayerController>().isReady = true;

        if (PhotonNetwork.IsMasterClient)
            CheckAllPlayerReady();
    }

    [PunRPC]
    public void StartGame()
    {
        startBtn.SetActive(false);
        LogText.text = "게임 시작!";
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(StartRoundCoroutine());
    }

    [PunRPC]
    void UpdateLeaderPlayer(int newLeaderIndex)
    {
        leaderIndex = newLeaderIndex;
        leaderPlayer = playerList[newLeaderIndex];
    }

    [PunRPC]
    void ResetLists()
    {
        predictedWinCnt = new int[playerList.Count];
        winCntOfEachTurn = new int[playerList.Count];
        winCountSelectionComplete = false;
        cardManager.ResetCardSet();
        foreach (var player in playerList)
        {
            player.GetComponent<PlayerController>().cardList.Clear();
        }
    }

    // 카드 분배는 승수 선택이 완료된 후 진행
    [PunRPC]
    void DistributeCards()
    {
        cardManager.DoCardShuffle();
        cardManager.TestUserCard(playerList.Count);
    }

    [PunRPC]
    void SubmitWinCount(int playerID, int winCount)
    {
        if (predictedWinCnt == null || playerID < 0 || playerID >= predictedWinCnt.Length)
        {
            Debug.LogError($"SubmitWinCount: playerID {playerID} out of range. Array length: {predictedWinCnt?.Length}");
            return;
        }

        predictedWinCnt[playerID] = winCount;

        if (playerID < 0 || playerID >= playerList.Count)
        {
            Debug.LogError($"SubmitWinCount: playerID {playerID} out of range in playerList. Count: {playerList.Count}");
            return;
        }

        LogText.text = $"{playerList[playerID].name} 승수: {winCount}";

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ProcessDecideWinCount", RpcTarget.AllBuffered, playerID + 1);
        }
    }

    // 승수 선택 프로세스 – 모든 플레이어가 차례대로 진행
    [PunRPC]
    void ProcessDecideWinCount(int playerID)
    {
        if (playerID >= playerList.Count)
        {
            winCountSelectionComplete = true;
            return;
        }
        int actualPlayerIndex = (leaderIndex + playerID) % playerList.Count;
        GameObject currentPlayer = playerList[actualPlayerIndex];
        noticeturnText.text += $"{actualPlayerIndex + 1}번째: {currentPlayer.name}\n";

        if (currentPlayer.GetComponent<PhotonView>().IsMine)
        {
            if (currentPlayer.GetComponent<AIPlayer>().isAIPlayer)
            {
                int aiWin = currentPlayer.GetComponent<AIPlayer>().CalculateOddsOfWinning(0.69f, 0.29f);
                photonView.RPC("SubmitWinCount", RpcTarget.All, actualPlayerIndex, aiWin);
            }
            else
            {
                StartCoroutine(WaitForPlayerWinCountSubmit(actualPlayerIndex));
            }
        }
    }

    // 턴 진행 (카드 분배 후 시작)
    [PunRPC]
    void ProcessTurn(int playerID)
    {
        if (playerID >= playerList.Count)
        {
            photonView.RPC("CheckTurnResult", RpcTarget.All);
            return;
        }
        if (playerList[playerID].GetComponent<PhotonView>().IsMine)
        {
            if (playerList[playerID].GetComponent<AIPlayer>().isAIPlayer)
            {
                StartCoroutine(playerList[playerID].GetComponent<AIPlayer>().AITurn());
            }
            else
            {
                photonView.RPC("ShowCardUI", RpcTarget.All, playerID);
            }
        }
    }

    [PunRPC]
    void CheckTurnResult()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        for (int i = 0; i < playerList.Count; i++)
        {
            bool isWinner = cardManager.CardCompare(submitCardList, i);
            if (isWinner)
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
        LogText.text = $"{playerList[winnerID].name} 턴 승리!";
    }

    [PunRPC]
    void StartRoundEnd()
    {
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(ProcessRoundEnd());
    }

    [PunRPC]
    void PrepareNextRound()
    {
        RemovePlayerList();
        noticeturnText.text = "";
        curRound++;

        if (playerList.Count <= 1 || curRound > maxRound)
        {
            photonView.RPC("EndGame", RpcTarget.All);
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                leaderIndex = (leaderIndex + 1) % playerList.Count;
                photonView.RPC("UpdateLeaderPlayer", RpcTarget.All, leaderIndex);
                StartCoroutine(StartRoundCoroutine());
            }
        }
    }

    [PunRPC]
    void EndGame()
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

    // UI용 RPC – 각 클라이언트에서 카드 제출 UI 활성화
    [PunRPC]
    void ShowCardUI(int playerID)
    {
        if (playerList[playerID].GetComponent<PhotonView>().IsMine)
        {
            List<int> curCardList = playerList[playerID].GetComponent<PlayerController>().cardList;
            buttonManager.ShowCard(curCardList, playerID);
        }
    }

    // RPC를 통해 모든 클라이언트에 시작 버튼 활성화 신호 전달 (Buffered 사용)
    [PunRPC]
    void UpdateStartButtonUI()
    {
        startBtn.SetActive(true);
    }
    #endregion

    #region Helper Methods & Coroutines
    void CheckAllPlayerReady()
    {
        if (playerList.All(p => p.GetComponent<PlayerController>().isReady))
        {
            startBtn.SetActive(true);
            photonView.RPC("UpdateStartButtonUI", RpcTarget.AllBuffered);
        }
    }

    // 승수 선택 후 카드 분배 및 턴 진행을 위한 코루틴
    public IEnumerator StartRoundCoroutine()
    {
        yield return new WaitForSeconds(3f);
        // 버퍼링된 RPC로 상태 초기화
        photonView.RPC("ResetLists", RpcTarget.AllBuffered);

        // 승수 선택 단계 시작 (버퍼링)
        photonView.RPC("StartDecideWinCount", RpcTarget.AllBuffered);

        yield return new WaitUntil(() => winCountSelectionComplete);

        // 승수 선택 완료 후 카드 분배 및 턴 진행 (버퍼링)
        photonView.RPC("DistributeCards", RpcTarget.AllBuffered);
        StartCoroutine(StartTurnCoroutine());
    }

    // 승수 선택 시작 RPC 호출 – MasterClient에서 순차 진행
    [PunRPC]
    void StartDecideWinCount()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ProcessDecideWinCount", RpcTarget.All, 0);
        }
    }

    IEnumerator StartTurnCoroutine()
    {
        yield return new WaitForSeconds(2f);
        StartTurn();
    }

    void StartTurn()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("ProcessTurn", RpcTarget.All, 0);
    }

    void StartNextTurn()
    {
        if (submitCardList.Count >= playerList.Count * 4)
        {
            photonView.RPC("StartRoundEnd", RpcTarget.All);
        }
        else
        {
            submitCardList.Clear();
            StartCoroutine(StartTurnCoroutine());
        }
    }

    IEnumerator WaitForPlayerWinCountSubmit(int playerID)
    {
        LogText.text = $"{playerList[playerID].name} 승수 선택 중...";
        buttonManager.showWinBtn(playerID, playerList[playerID].GetComponent<PlayerController>().winBtn);
        buttonManager.ShowPlayerPanel(true);

        yield return new WaitUntil(() => selectedWin == 0);

        photonView.RPC("SubmitWinCount", RpcTarget.All, playerID, predictedWinCnt[playerID]);

        buttonManager.ShowPlayerPanel(false);
        buttonManager.hideWinBtn(playerList[playerID].GetComponent<PlayerController>().winBtn);
        selectedWin = -1;
    }

    IEnumerator ProcessRoundEnd()
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            yield return StartCoroutine(CheckPlayerResult(i));
            if (CheckGameEnd())
            {
                photonView.RPC("EndGame", RpcTarget.All);
                yield break;
            }
        }
        yield return new WaitForSeconds(2f);
        photonView.RPC("PrepareNextRound", RpcTarget.All);
    }

    IEnumerator CheckPlayerResult(int playerID)
    {
        if (predictedWinCnt[playerID] != winCntOfEachTurn[playerID])
        {
            if (playerList[playerID].GetComponent<AIPlayer>().isAIPlayer)
                yield return StartCoroutine(playerList[playerID].GetComponent<AIPlayer>().AIDrawBomb());
            else
                yield return StartCoroutine(scoreManager.CheckBomb(playerList[playerID].GetComponent<PlayerController>()));
        }
        yield return new WaitForSeconds(1f);
    }

    void RemovePlayerList()
    {
        foreach (GameObject dead in deadList)
        {
            if (playerList.Contains(dead))
                playerList.Remove(dead);
        }
    }

    bool CheckGameEnd()
    {
        return deadList.Count >= maxPlayerCnt - 1;
    }
    #endregion
}
