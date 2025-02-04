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
    public void ShowPlayerPanel(bool onoff)
    {
        multiplayGameManager.playerList[playerid]
            .GetComponent<PlayerController>().playerCanvas.SetActive(onoff);
    }

    public void showWinBtn(int playerID, GameObject winBtn)
    {
        // 전달받은 playerID를 바로 할당
        playerid = playerID;
        winBtn.SetActive(true);

        PlayerController playerController = multiplayGameManager.playerList[playerid].GetComponent<PlayerController>();

        playerController.increaseBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        playerController.decreaseBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        playerController.submitBtn.GetComponent<Button>().onClick.RemoveAllListeners();

        playerController.increaseBtn.GetComponent<Button>().onClick.AddListener(OnIncreaseScoreButtonClicked);
        playerController.decreaseBtn.GetComponent<Button>().onClick.AddListener(OnDecreaseScoreButtonClicked);
        playerController.submitBtn.GetComponent<Button>().onClick.AddListener(OnSubmitScoreButtonClicked);

        Debug.LogWarning("WinBtn shown for player " + playerid);
        GameObject appearEffect = Resources.Load<GameObject>("AppearEffect");
        Instantiate(appearEffect, winBtn.transform);
    }

    public void hideWinBtn(GameObject winBtn)
    {
        winBtn.SetActive(false);
    }

    public void OnIncreaseScoreButtonClicked()
    {
        if (!multiplayGameManager.playerList[playerid].GetComponent<PhotonView>().IsMine)
            return;

        Text playerText = multiplayGameManager.playerList[playerid]
            .GetComponent<PlayerController>().winningText.GetComponent<Text>();

        expectedWin++;
        logText.text = expectedWin.ToString();
        playerText.text = expectedWin.ToString();

        buttonTransform = multiplayGameManager.playerList[playerid]
            .GetComponent<PlayerController>().increaseBtn.transform;
    }

    public void OnDecreaseScoreButtonClicked()
    {
        if (!multiplayGameManager.playerList[playerid].GetComponent<PhotonView>().IsMine)
            return;

        Text playerText = multiplayGameManager.playerList[playerid]
            .GetComponent<PlayerController>().winningText.GetComponent<Text>();

        expectedWin--;
        logText.text = expectedWin.ToString();
        playerText.text = expectedWin.ToString();

        buttonTransform = multiplayGameManager.playerList[playerid]
            .GetComponent<PlayerController>().decreaseBtn.transform;
    }

    public void OnSubmitScoreButtonClicked()
    {
        if (!multiplayGameManager.playerList[playerid].GetComponent<PhotonView>().IsMine)
            return;

        if (expectedWin >= 0 && expectedWin <= 4)
        {
            multiplayGameManager.gameObject.GetPhotonView()
                .RPC("SubmitWinCount", RpcTarget.All, playerid, expectedWin);
            logText.DOText("승리횟수: " + expectedWin.ToString() + " 제출완료", 1.2f);
            multiplayGameManager.selectedWin = 0;
        }
        else
        {
            logText.text = "0에서 4 사이의 값만 입력 가능합니다.";
            logText.color = Color.red;
        }
    }
    #endregion

    #region Cards
    public void ShowCard(List<int> cardList, int playerID)
    {
        foreach (var instance in activeCardInstances)
        {
            Destroy(instance);
        }
        activeCardInstances.Clear();

        Transform[] cardPositions = new Transform[4];
        GameObject currentPlayer = multiplayGameManager.playerList[playerID];

        for (int i = 0; i < 4; i++)
        {
            cardPositions[i] = currentPlayer.GetComponent<PlayerController>().cardPosList[i];
        }

        for (int i = 0; i < cardList.Count && i < cardPositions.Length; i++)
        {
            GameObject cardPrefab = cardButtonPrefab[cardList[i] - 1];
            GameObject cardInstance = Instantiate(cardPrefab, cardPositions[i].position,
                cardPositions[i].rotation, cardPositions[i]);

            activeCardInstances.Add(cardInstance);

            Button buttonComponent = cardInstance.GetComponent<Button>();
            int cardValue = cardList[i];
            buttonComponent.onClick.AddListener(() => OnSubmitCardButtonClicked(cardValue, cardInstance, playerID));
        }
    }

    public void OnSubmitCardButtonClicked(int cardValue, GameObject cardInstance, int playerID)
    {
        if (!multiplayGameManager.playerList[playerID].GetComponent<PhotonView>().IsMine)
            return;

        PlayerController playerController = multiplayGameManager.playerList[playerID]
            .GetComponent<PlayerController>();

        playerController.cardList.Remove(cardValue);
        multiplayGameManager.submitCardList.Add(cardValue);

        activeCardInstances.Remove(cardInstance);
        Destroy(cardInstance);

        logText.text = "";
        logText.DOText("카드값: " + cardValue.ToString() + " 제출완료", 1f);

        multiplayGameManager.checkSubmitCard = 0;
    }
    #endregion
}
