using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject[] playerList;
    public List<GameObject> deadList;
    GameObject leaderPlayer;
    public Text LogText;
    public int maxPlayerCnt = 4;

    public int curRound = 1;
    public int maxRound = 3;
    public int curTurn;
    public int maxCardCnt = 5;

    public int selectedBomb = -1;
    public int selectedWin = -1;
    public List<int> submitCardList; //제출된 카드리스트
    public int[] predictedWinCnt; //각각의 라운드마다 플레이어들이 예측한 승리 횟수
    public int[] winCntOfEachTurn; //각각의 턴마다 플레이어들이 기록한 승리 횟수

    public CardManager cardManager;
    public ScoreManager scoreManager;
    public ButtonManager buttonManager;
    public GameObject TestBtn;
    void Start()
    {
        playerList = GameObject.FindGameObjectsWithTag("Player");

        //리더 플레이어(맨 처음 시작할 사람) 정하기
        curTurn = Random.Range(0, playerList.Length);
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
        cardManager.TestUserCard(playerList.Length);

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
            for (int j = 0; j < playerList.Length; j++)
            {
                yield return StartCoroutine(SubmitCard());
            }

            //제출한 카드 보고 승자 결정하기(카드매니저에 들어가 있는 함수 호출)
            Debug.Log("=========이번 턴 승자 결정=========");
            for (int j = 0; j < playerList.Length; j++)
            {
                bool checker = cardManager.CardCompare(submitCardList, j);

                if (checker) //checker의 반환값이 true면...
                {
                    //playerList[curTurn]이 이번 턴 승자라는 뜻!
                    winCntOfEachTurn[curTurn]++;
                    Debug.Log("이번 턴의 승자는 " + playerList[curTurn] + "! (현재 " + winCntOfEachTurn[curTurn] + "승)");
                }
                curTurn++;
                if (curTurn >= playerList.Length) curTurn = 0;
            }
            yield return new WaitForSeconds(5f);
        }

        //라운드가 끝날 때마다 승수 맞췄는지 판단, 벌칙 결정(미완성)
        Debug.Log("=========이번 라운드 결과=========");
        for (int i = 0; i < playerList.Length; i++)
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
            curTurn++;
            if (curTurn >= playerList.Length) curTurn = 0;
        }
        yield return new WaitForSeconds(10f);

        //라운드 종료
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

        //리더 플레이어부터 차례로 승수 선언. 임시로 랜덤숫자로 승리선언 처리해둠
        for (int i = 0; i < playerList.Length; i++)
        {
            Debug.LogWarning(i+1 + "번째 순서");
            //여기에서 플레이어 리스트[현재 차례]의 승수 선언 UI 활성화
            LogText.text = i+1 + "번쨰 순서" + " 승 수 선택하세요";
            TestBtn.SetActive(true);
            yield return new WaitUntil(() => selectedWin == 0);
            TestBtn.SetActive(false);
            selectedWin = -1;
            //predictedWinCnt[curTurn] = Random.Range(0, 4);
            Debug.Log(playerList[curTurn] + "의 승수 선언 : " + predictedWinCnt[curTurn] + "승");

            curTurn++;
            if (curTurn >= playerList.Length) curTurn = 0;

            yield return new WaitForSeconds(1f);
        }
        yield return null;
    }

    IEnumerator SubmitCard() // SubmitCard(int subitCard)
    {
        //리더 플레이어부터 차례로 카드 제출. 임시로 랜덤숫자로 카드제출 처리해둠
        List<int> curCardList = playerList[curTurn].GetComponent<PlayerController>().cardList;
        // tempCard was init for testing
        int tempCard = Random.Range(0, curCardList.Count);
        
        // 실제 구현 단계에서는, 이거를 레이케이스트로 해서 받아온 카드의 정보가 되겠죠?
        int selectedCard = curCardList[tempCard];

            Debug.Log(playerList[curTurn] + "의 카드 제출 : " + selectedCard);

            curCardList.RemoveAt(tempCard);
            submitCardList.Add(selectedCard);//제출된 카드끼리 비교하기 위해 제출카드리스트에 넣기

            curTurn++;
            if (curTurn >= playerList.Length) curTurn = 0;

        yield return new WaitForSeconds(1f);
    }
    void ResetLists()
    {
        predictedWinCnt = new int[playerList.Length];
        winCntOfEachTurn = new int[playerList.Length];

        cardManager.GetComponent<CardManager>().ResetCardSet();
        for (int i = 0; i < playerList.Length; i++)
        {
            playerList[i].GetComponent<PlayerController>().cardList.Clear();
        }
    }
}
