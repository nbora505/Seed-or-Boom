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
        multiplayGameManager.playerList[playerid].GetComponent<PlayerController>().playerCanvas.SetActive(onoff);
        //    .transform.Find("Player_Canvas/Panel");
        //PlayerPanel.gameObject.SetActive(onoff);
    }

    public void showWinBtn(int playerID, GameObject winBtn)
    {
        winBtn.SetActive(true);

        PlayerController playerController = multiplayGameManager.playerList[playerid].GetComponent<PlayerController>();

        playerController.increaseBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        playerController.increaseBtn.GetComponent<Button>().onClick.RemoveAllListeners();

        playerController.increaseBtn.GetComponent<Button>().onClick.AddListener(OnIncreaseScoreButtonClicked);
        playerController.increaseBtn.GetComponent<Button>().onClick.AddListener(OnDecreaseScoreButtonClicked);

        playerid = playerID;
        Debug.LogWarning("In***********************");
        GameObject appearEffect = Resources.Load<GameObject>("AppearEffect");
        Instantiate(appearEffect, winBtn.transform);
    }

    public void hideWinBtn(GameObject winBtn)
    {
        winBtn.gameObject.SetActive(false);
    }

    public void OnIncreaseScoreButtonClicked()
    {
        if (!multiplayGameManager.playerList[playerid].GetComponent<PhotonView>().IsMine)
            return;

        Transform PlayerPanelPos = multiplayGameManager.playerList[playerid].GetComponent<PlayerController>().winningText.gameObject.transform;


        Text playerText = PlayerPanelPos.GetComponent<Text>();

        expectedWin++;
        logText.text = expectedWin.ToString();
        playerText.text = expectedWin.ToString();

        buttonTransform = multiplayGameManager.playerList[playerid].GetComponent<PlayerController>().increaseBtn.gameObject.transform;

        buttonTransform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 2.5f, 5, 2);
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

        buttonTransform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 2.5f, 5, 2);
        // 비슷한 방식으로 구현
    }

    public void OnSubmitScoreButtonClicked()
    {
        if (!multiplayGameManager.playerList[multiplayGameManager.curTurn].GetComponent<PhotonView>().IsMine)
            return;

        if (expectedWin >= 0 && expectedWin <= 4)
        {
            multiplayGameManager.selectedWin = 0;
            multiplayGameManager.predictedWinCnt[multiplayGameManager.curTurn] = expectedWin;
            logText.DOText("승리횟수 :" + expectedWin.ToString() + "번 제출완료", 1.2f);
            expectedWin = 0;
        }
        else
        {
            logText.text = "0에서 4사이의 값만 입력 가능합니다.";
            logText.color = Color.red;
        }
    }
    #endregion

    #region Cards
    public void ShowCard(List<int> cardList, int playerID)
    {
        foreach (var cardInstance in activeCardInstances)
        {
            Destroy(cardInstance);
        }
        activeCardInstances.Clear();

        Transform[] cardPositions = new Transform[4];

        GameObject currentPlayer = multiplayGameManager.playerList[playerID];
        Debug.LogError(currentPlayer);
        

        for (int i = 0; i < 4; i++)
        {
            cardPositions[i] = currentPlayer.GetComponent<PlayerController>().cardPosList[i];
            Debug.LogError(cardPositions[i].transform.position);
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
        logText.DOText("카드값 : " + cardValue.ToString() + " 제출완료", 1f);

        multiplayGameManager.checkSubmitCard = 0;
    }
    #endregion
}