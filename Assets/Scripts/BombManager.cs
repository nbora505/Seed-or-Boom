using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    //이 녀석이 몇 번째 폭탄인지
    public int bombNum;
    GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("GameObject").GetComponent<GameManager>();

        //만약 플레이어가 이 녀석을 선택한다면 gameManager의 selectedBomb에 이 녀석 번호 넣어주기.
        //임시로 start에 집어넣어둠
        gameManager.selectedBomb = bombNum;
    }

    

    void Update()
    {
        
    }
}
