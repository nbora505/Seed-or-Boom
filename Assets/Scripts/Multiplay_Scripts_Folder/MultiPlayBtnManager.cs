using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiPlayBtnManager : MonoBehaviourPunCallbacks
{
    [Header("Scripts")]
    public MultiplayGameManager multiplayGameManager;
    public ScoreManager scoreManager;
    public CardManager cardManager;

    [Header("UI Elements")]
    public Text logText;
    public Text playerLogText;
    public List<GameObject> cardButtonPrefab;
    public Transform buttonTransform;

    [Header("Game State")]
    public int expectedWin = 0;
    public int selectCard;
    public List<GameObject> activeCardInstances = new List<GameObject>();

    public int playerid;

    private void Start()
    {
        DOTween.Init();
    }

    #region Wins
    [PunRPC]
    public void ShowPlayerPanel(bool onoff)
    {
        if (multiplayGameManager.playerList[playerid].GetComponent<PhotonView>().IsMine)
        {
            multiplayGameManager.playerList[playerid].GetComponent<PlayerController>().playerCanvas.SetActive(onoff);
        }
    }

    [PunRPC]
    public void ShowWinBtn(int playerID, GameObject winBtn)
    {
        playerid = playerID;

        if (multiplayGameManager.playerList[playerid].GetComponent<PhotonView>().IsMine)
        {
            winBtn.SetActive(true);

            PlayerController playerController = multiplayGameManager.playerList[playerid].GetComponent<PlayerController>();

            // 버튼 리스너 설정
            SetupButtonListeners(playerController);

            // 이펙트 생성
            CreateAppearEffect(winBtn);
        }
    }

    private void SetupButtonListeners(PlayerController playerController)
    {
        playerController.increaseBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        playerController.decreaseBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        playerController.submitBtn.GetComponent<Button>().onClick.RemoveAllListeners();

        playerController.increaseBtn.GetComponent<Button>().onClick.AddListener(OnIncreaseScoreButtonClicked);
        playerController.decreaseBtn.GetComponent<Button>().onClick.AddListener(OnDecreaseScoreButtonClicked);
        playerController.submitBtn.GetComponent<Button>().onClick.AddListener(OnSubmitScoreButtonClicked);
    }

    private void CreateAppearEffect(GameObject winBtn)
    {
        GameObject appearEffect = Resources.Load<GameObject>("AppearEffect");
        Instantiate(appearEffect, winBtn.transform);
    }


    public void hideWinBtn(GameObject winBtn)
    {
        if (multiplayGameManager.playerList[playerid].GetComponent<PhotonView>().IsMine)
        {
            winBtn.SetActive(false);
        }
    }

    public void OnIncreaseScoreButtonClicked()
    {
        if (!multiplayGameManager.playerList[playerid].GetComponent<PhotonView>().IsMine)
            return;

        photonView.RPC("UpdateExpectedWin", RpcTarget.All, playerid, expectedWin + 1);
    }

    [PunRPC]
    private void UpdateExpectedWin(int playerId, int newValue)
    {
        expectedWin = newValue;
        UpdateWinCountUI(playerId);
    }

    private void UpdateWinCountUI(int playerId)
    {
        Transform PlayerPanelPos = multiplayGameManager.playerList[playerId].GetComponent<PlayerController>().winningText.gameObject.transform;
        Text playerText = PlayerPanelPos.GetComponent<Text>();

        logText.text = expectedWin.ToString();
        playerText.text = expectedWin.ToString();
    }



    public void OnDecreaseScoreButtonClicked()
    {
        if (!multiplayGameManager.playerList[playerid].GetComponent<PhotonView>().IsMine)
            return;

        Transform PlayerPanelPos = multiplayGameManager.playerList[playerid].GetComponent<PlayerController>().winningText.gameObject.transform;


        Text playerText = PlayerPanelPos.GetComponent<Text>();

        expectedWin--;
        logText.text = expectedWin.ToString();
        playerText.text = expectedWin.ToString();

        buttonTransform = multiplayGameManager.playerList[playerid].GetComponent<PlayerController>().decreaseBtn.gameObject.transform;

        //buttonTransform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 2.5f, 5, 2);
        // 비슷한 방식으로 구현
    }

    public void OnSubmitScoreButtonClicked()
    {
        if (!multiplayGameManager.playerList[playerid].GetComponent<PhotonView>().IsMine)
            return;

        if (expectedWin >= 0 && expectedWin <= 4)
        {
            photonView.RPC("SubmitWinCountUI", RpcTarget.All, playerid, expectedWin);
        }
        else
        {
            ShowErrorMessage();
        }
    }
    private void ShowErrorMessage()
    {
        logText.text = "0에서 4사이의 값만 입력 가능합니다.";
        logText.color = Color.red;
    }
    #endregion

    [PunRPC]
    private void SubmitWinCountUI(int playerId, int winCount)
    {
        logText.DOText("승리횟수 :" + winCount.ToString() + "번 제출완료", 1.2f);
        multiplayGameManager.selectedWin = 0;
        multiplayGameManager.gameObject.GetPhotonView().RPC("SubmitWinCount", RpcTarget.All, playerId, winCount);
    }

    #region Cards
    [PunRPC]
    public void ShowCard(List<int> cardList, int playerID)
    {
        if (!multiplayGameManager.playerList[playerID].GetComponent<PhotonView>().IsMine)
            return;

        ClearActiveCards();
        CreateCardInstances(cardList, playerID);
    }

    private void ClearActiveCards()
    {
        foreach (var cardInstance in activeCardInstances)
        {
            Destroy(cardInstance);
        }
        activeCardInstances.Clear();
    }

    private void CreateCardInstances(List<int> cardList, int playerID)
    {
        Transform[] cardPositions = GetCardPositions(playerID);

        for (int i = 0; i < cardList.Count && i < cardPositions.Length; i++)
        {
            CreateCardInstance(cardList[i], cardPositions[i], playerID);
        }
    }
    private Transform[] GetCardPositions(int playerID)
    {
        Transform[] cardPositions = new Transform[4];
        GameObject currentPlayer = multiplayGameManager.playerList[playerID];

        for (int i = 0; i < 4; i++)
        {
            cardPositions[i] = currentPlayer.GetComponent<PlayerController>().cardPosList[i];
        }

        return cardPositions;
    }

    private void CreateCardInstance(int cardValue, Transform cardPosition, int playerID)
    {
        GameObject cardPrefab = cardButtonPrefab[cardValue - 1];
        GameObject cardInstance = Instantiate(cardPrefab, cardPosition.position,
            cardPosition.rotation, cardPosition);

        activeCardInstances.Add(cardInstance);

        Button buttonComponent = cardInstance.GetComponent<Button>();
        buttonComponent.onClick.AddListener(() => OnSubmitCardButtonClicked(cardValue, cardInstance, playerID));
    }


    [PunRPC]
    public void OnSubmitCardButtonClicked(int cardValue, GameObject cardInstance, int playerID)
    {
        if (!multiplayGameManager.playerList[playerID].GetComponent<PhotonView>().IsMine)
            return;

        photonView.RPC("ProcessCardSubmission", RpcTarget.All, playerID, cardValue);

        if (cardInstance != null)
        {
            activeCardInstances.Remove(cardInstance);
            Destroy(cardInstance);
        }
    }

    [PunRPC]
    private void ProcessCardSubmission(int playerID, int cardValue)
    {
        PlayerController playerController = multiplayGameManager.playerList[playerID]
            .GetComponent<PlayerController>();

        // 카드 리스트에서 제거
        playerController.cardList.Remove(cardValue);

        // 제출된 카드 리스트에 추가
        multiplayGameManager.submitCardList.Add(cardValue);

        // UI 업데이트
        UpdateCardSubmissionUI(cardValue);

        // 제출 완료 상태 업데이트
        multiplayGameManager.checkSubmitCard = 0;
    }
    private void UpdateCardSubmissionUI(int cardValue)
    {
        logText.text = "";
        logText.DOText("카드값 : " + cardValue.ToString() + " 제출완료", 1f);
    }

    [PunRPC]
    public void SyncCardState(int playerID, List<int> cardList)
    {
        if (multiplayGameManager.playerList[playerID].GetComponent<PhotonView>().IsMine)
        {
            ClearActiveCards();
            CreateCardInstances(cardList, playerID);
        }
    }

    [PunRPC]
    public void ResetCardState()
    {
        ClearActiveCards();
        expectedWin = 0;
        selectCard = 0;
    }

    private void HandleCardError(string errorMessage)
    {
        Debug.LogError($"Card Error: {errorMessage}");
        logText.text = "카드 처리 중 오류가 발생했습니다.";
        logText.color = Color.red;
    }

    private bool CheckNetworkState()
    {
        if (!PhotonNetwork.IsConnected)
        {
            HandleCardError("Network connection lost");
            return false;
        }
        return true;
    }

    private void PlayCardAnimation(GameObject cardInstance)
    {
        if (cardInstance != null)
        {
            // DOTween을 사용한 카드 제출 애니메이션
            cardInstance.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack)
                .OnComplete(() => {
                    if (cardInstance != null)
                    {
                        Destroy(cardInstance);
                    }
                });
        }
    }

    private void LogCardOperation(string operation, int playerID, int cardValue)
    {
        Debug.Log($"Card Operation: {operation} | Player: {playerID} | Card: {cardValue}");
    }

    private void OnDestroy()
    {
        // 리소스 정리
        ClearActiveCards();
        DOTween.KillAll();
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        HandleCardError($"Disconnected: {cause}");
    }

    // 게임 상태 체크
    private bool ValidateGameState(int playerID)
    {
        if (playerID >= multiplayGameManager.playerList.Count)
        {
            HandleCardError("Invalid player ID");
            return false;
        }
        return true;
    }

    #endregion
}