using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public List<GameObject> playerList;
    public List<GameObject> deadList;
    GameObject leaderPlayer;
    
    public Text noticeturnText;
    public Text LogText;
    public int maxPlayerCnt = 4;

    public int curRound = 1;
    public int maxRound = 3;
    public int curTurn;
    public int maxCardCnt = 5;


    public int selectedBomb = -1;
    public int selectedWin = -1;
    public int checkSubmitCard = -1;
    public int selectedCard =0;
    public List<int> submitCardList; //제출된 카드리스트
    public int[] predictedWinCnt; //각각의 라운드마다 플레이어들이 예측한 승리 횟수
    public int[] winCntOfEachTurn; //각각의 턴마다 플레이어들이 기록한 승리 횟수

    public CardManager cardManager;
    public ScoreManager scoreManager;
    public ButtonManager buttonManager;
    public GameObject TestBtn;
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

        //라운드 시작
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
        if (curRound > maxRound)
        {
            Debug.Log("최대 라운드 초과");
        }
        else
        {
            StartCoroutine(StartRound());
        }
    }

    IEnumerator DecideWinCnt()
    {
        Debug.Log("=========승수 선언 단계=========");
        
        //리더 플레이어부터 차례로 승수 선언
        for (int i = 0; i < playerList.Count; i++)
        {
            string playerName = playerList[curTurn].name;
            Debug.LogWarning( i+1 + "번째 순서" + playerName + "입니다.");
            noticeturnText.text += (i + 1 + "번째 순서 " + playerName + "입니다.\n");

            //여기에서 플레이어 리스트[현재 차례]의 승수 선언 UI 활성화
            LogText.text = playerName + " 승 수 선택하세요";
            TestBtn.SetActive(true);
            yield return new WaitUntil(() => selectedWin == 0);
            TestBtn.SetActive(false);
            selectedWin = -1;
            
            Debug.Log(playerName + "의 승수 선언 : " + predictedWinCnt[curTurn] + "승");

            curTurn++;
            if (curTurn >= playerList.Count) curTurn = 0;

            yield return new WaitForSeconds(1f);
        }
        yield return null;
    }

    IEnumerator SubmitCard()
    {
        
            // 현재 플레이어의 카드 리스트 가져오기
            List<int> curCardList = playerList[curTurn].GetComponent<PlayerController>().cardList;
        string playerName = playerList[curTurn].name;

        LogText.text = playerName + " 카드 선택 하세요";

            // 현재 플레이어의 카드만 표시
            buttonManager.ShowCard(curCardList);

            // 플레이어가 카드를 제출할 때까지 대기
            yield return new WaitUntil(() => checkSubmitCard == 0);
            checkSubmitCard = -1;

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
                LogText.text = "이번 턴의 승자는 : " + playerName + "! (현재 " + winCntOfEachTurn[curTurn] + "승)";
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
            yield return StartCoroutine(scoreManager.CheckBomb(curPlayer.GetComponent<PlayerController>()));
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
                if(playerList.Count <= maxPlayerCnt - deadList.Count)
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
        if(deadList.Count == maxPlayerCnt - 1)
        {
            RemovePlayerList();
            Debug.Log("::::: 게임 종료! :::::");
            Debug.Log("::::: 승자는 " + playerList[0] + "! :::::");
        }
    }

        //리스트 초기화
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
}
