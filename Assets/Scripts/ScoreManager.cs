using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    GameManager gm;
    PlayerController pc;
    bool isSelect = false;
    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator CheckBomb(PlayerController player)
    {
        //벌칙 대상자의 남아있는 폭탄들 활성화시키기
        for (int i = 0; i < player.remainingBomb; i++)
        {
            player.bombPrefab[i].SetActive(true);
        }

        // player.남아 있는 포탄들 활성화 시키는 함수();
        // -> 이렇게 되면 한명에 대한 함수로 바뀌는 것.
        // -> 게임 메니저에서 for문 돌려서 해당하는 모든 플레이어에게 이 함수 실행킨다.

        //남아있는 심지 수만큼 랜덤 돌려서 당첨 심지 결정하기
        int realBomb = Random.Range(0, player.remainingBomb);

        //심지가 골라질때까지 기다리기(기본값은 -1)
        yield return new WaitUntil(() => gm.selectedBomb >= 0);

        // pc.DrawBomb()은 폭탄을 선택했을 때, 실행되어야 함.
        player.DrawBomb(gm.selectedBomb);

    }
    public IEnumerator Timeout(PlayerController player)//함수 타임 아웃(파라메타 없음)
    {        //    GameManager.벌칙 대상 리스트.Clear();
             //   벌칙대상 리스트는 스코어 매니저, 폭탄터지는 건 게임매니저
             //    GameManager.폭탄 비교(this.player, 랜덤);

        yield return new WaitForSeconds(10);

        if (!isSelect)
        {
            player.DrawBomb(0);//무조건 터지도록 추후 수정 필요
        }
        else
        {
            yield return null;
        }
    }   //    함수 끝

    public void DistinguishScore(PlayerController player)
    {
            player.remainingBomb--; // This is a temporary line put in for the current structure.
                                   // If a bomb selection line is ever created,
                                   // it should run the function, subtract the bomb, and so on.
    }
}
