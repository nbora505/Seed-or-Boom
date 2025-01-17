//using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    //[변수]
    public bool AIPlayer = false;
    public List<int> cardList = new List<int>(); // 카드를 나눠줄 때, 해당 플레이어에 .Add()

    public List<bool> bombList = new List<bool>() {
        false, false, true };
    public GameObject[] bombPrefab;
    public int remainingBomb = 3; //  int 남은 폭탄심지 수
    //public int playerOrder = 0; //    플레이어 순서별 가중치 0 to 3;
    public int submitTime = 0; // 만약에, 유저가 레이케이스트로 카드를 선택하고 제출했을 때, 그때의 유저의 순서가 몇 번인가를 기준으로 초기화
    public bool bombVisible = false;
    public bool isDead = false;//플레이어의 사망
    public GameManager gm;

    public int expectedWins = 0;
    public int nowTotalWins = 0;

    // 요거를 게임메니저에서 받아와가지고 초기화할 수 있도록.
    // 플레이어 콘트롤러 쪽에서 게임메니저의 예상 승수를 받아오고, 자신의 턴이랑 비교해서 넣으면 되겠죠?
    public List<int> expectedWin = new List<int>();
    private void Update()
    {
        if(isDead) return;

        // 왼팔이나, 다른 팔을 트레킹을 못하게 하려면
        // 우리 프리펩 내에 트래킹 관련 오브젝트가 있음
        // 그거 disable 하면 되드라고요.
        // 요로면 발작 안 일어남.

        // -> 게임 메니저에서 전부 체크하고 맵을 컨트롤 하는.
        // -> 게임 메니저에서 턴 관련이 있으니까, 
        // -> Enum을 써서 스위치문을 통해 쓰는 것.


        // 오른손 트리거를 눌렀을 때, 카드를 선택할 수 있어야 하고.
        // 왼손 트리거를 눌렀을 땐, 내 패를 볼 수 있어야 겠죠?
    }
    // 카드 제출과 카드 확인이 필요하다.

    //[함수]
    public void DeclareWins() //함수 승수 선언()
    {
        //    0~4 승수 선택용 숫자 등장();
        Invoke("TimeOut", 10f);
        //타임아웃 함수는 여기 캔슬은 버튼매니저에서

        // 버튼에서 vr object 찍을 수 있는 상태로 만들어 준 카드를 선택 only

    }//함수 끝

    public void AppearBomb() //함수 심지 등장(int 심지 수)
    {
        for (int i = 0; i < remainingBomb; i++)
        {
            gameObject.GetComponentInChildren<GameObject>().SetActive(true);
        }

        //  플레이어 선택 존중하려면 랜덤 배제해야 하나 편의상 선택과
        //  무관하게 랜덤으로 터지는 폭탄 생성

    }

    // Must to called at GameManager
    public void InitsubmitTime(int submitTime)
    {
        this.submitTime = submitTime;
    }

    public void DrawBomb(int selectedBombNum)//함수 폭탄 결정(파라메터:선택된 심지 번호)
    {
        int isTrueBomb = Random.Range(0, bombList.Count);

        if (isTrueBomb == selectedBombNum)
        {
            Debug.Log(gameObject.name + "... 사망!!!!!");
            gm.deadList.Add(this.gameObject);
            // 사망처리
            isDead = true;
            //사망 연출은 여기서 처리하는 걸로
        }
        else
        {
            Debug.Log(gameObject.name + "... 생존!!!!!");
            remainingBomb--;
            bombList.RemoveAt(isTrueBomb);// false가 지워지겠죠.
            // 생존처리
        }
        gm.selectedBomb = -1;//다시 선택된 폭탄 번호 초기화
    }
}
