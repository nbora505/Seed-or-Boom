using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class GameManager : MonoBehaviour
{
    public List<GameObject> playerList;
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
        //yield return StartCoroutine(DecideWinCnt());

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
        yield return new WaitForSeconds(10f);

        //라운드 종료
        //사망 리스트에 있는 플레이어가 플레이어리스트에 아직 남아있을 경우 지워주기
        RemovePlayerList();

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
        for (int i = 0; i < playerList.Count; i++)
        {
<<<<<<< Updated upstream
            Debug.LogWarning(i+1 + "번째 순서");
            //여기에서 플레이어 리스트[현재 차례]의 승수 선언 UI 활성화
            LogText.text = i+1 + "번쨰 순서" + " 승 수 선택하세요";
            TestBtn.SetActive(true);
            yield return new WaitUntil(() => selectedWin == 0);
            TestBtn.SetActive(false);
            selectedWin = -1;
            //predictedWinCnt[curTurn] = Random.Range(0, 4);
            Debug.Log(playerList[curTurn] + "의 승수 선언 : " + predictedWinCnt[curTurn] + "승");
=======
            string playerName = playerList[curTurn].name;
            Debug.LogWarning( i+1 + "번째 순서" + playerName + "입니다.");
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
                LogText.text = playerName + " 승 수 선택하세요";
                TestBtn.SetActive(true);
                yield return new WaitUntil(() => selectedWin == 0);

                TestBtn.SetActive(false);
                selectedWin = -1;
            }
            
            Debug.Log(playerName + "의 승수 선언 : " + predictedWinCnt[curTurn] + "승");
>>>>>>> Stashed changes

            curTurn++;
            if (curTurn >= playerList.Count) curTurn = 0;

            yield return new WaitForSeconds(1f);
        }
        yield return null;
    }

    IEnumerator SubmitCard() // SubmitCard(int subitCard)
    {
<<<<<<< Updated upstream
        //리더 플레이어부터 차례로 카드 제출. 임시로 랜덤숫자로 카드제출 처리해둠
        List<int> curCardList = playerList[curTurn].GetComponent<PlayerController>().cardList;
        // tempCard was init for testing
        int tempCard = Random.Range(0, curCardList.Count);
        
        // 실제 구현 단계에서는, 이거를 레이케이스트로 해서 받아온 카드의 정보가 되겠죠?
        int selectedCard = curCardList[tempCard];
=======
        // 만약에 겟 컴포넌트를 했을 때, 그게 널이면, 
        if (playerList[curTurn].GetComponent<PlayerController>().isAIPlayer || playerList[curTurn].GetComponent<AIPlayer>().isAIPlayer)
        {
            StartCoroutine(playerList[curTurn].GetComponent<AIPlayer>().AITurn());
            //buttonManager.logText.text = $"{submitCardList[submitCardList.Count - 1]}번 카드 제출 완료";
        }
        else
        {
            // 현재 플레이어의 카드 리스트 가져오기
            List<int> curCardList = playerList[curTurn].GetComponent<PlayerController>().cardList;
            string playerName = playerList[curTurn].name;
>>>>>>> Stashed changes

        Debug.Log(playerList[curTurn] + "의 카드 제출 : " + selectedCard); //isAIplayer를 기준으로 해서 한번 정제하고, 거기서 다시 isAIturn으로 한번 더 정제 해야 한다.

        curCardList.RemoveAt(tempCard);
        submitCardList.Add(selectedCard);//제출된 카드끼리 비교하기 위해 제출카드리스트에 넣기

        curTurn++;
        if (curTurn >= playerList.Count) curTurn = 0;

        yield return new WaitForSeconds(1f);
    }

    IEnumerator CheckTurnResult()
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            bool checker = cardManager.CardCompare(submitCardList, i);

            if (checker) //checker의 반환값이 true면...
            {
                //playerList[curTurn]이 이번 턴 승자라는 뜻!
                winCntOfEachTurn[curTurn]++;
                Debug.Log("이번 턴의 승자는 " + playerList[curTurn] + "! (현재 " + winCntOfEachTurn[curTurn] + "승)");
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
        if(curTurn > playerList.Count - 1)
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
}
