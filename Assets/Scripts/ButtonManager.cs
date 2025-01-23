using DG.Tweening;
using Firebase.Firestore;
using Meta.WitAi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;




public class ButtonManager : MonoBehaviour
{
    [Header("Scripts")]
    public GameManager gameManager; 
    public ScoreManager scoreManager;
    public PlayerController playerController;
    public CardManager cardManager;

    [Header("Score,UI")]
    //public GameObject playerLogPanel;
    public Text playerLogText;
    public int expectedWin = 0;
    public int selectCard;
    public Text logText;
    public List<GameObject> cardButtonPrefab;
    
    

    [Header("bomb")]
    public List<int> selectedBomb = new List<int>() {0,1,2};
    public int selectedBombIndex;
    public Outline BombWickOutline;
    public Vector3 originalBombWickPosition;
    public Color originalBombWickOutlineColor;

    [Header("Card")]
    public List<int> playerCards;
    public List<Button> testBtn;
    public Button submitBtn;
    public Outline CardOutline;
    public Vector3 originalCardPosition;
    public Color originalOutlineColor;

    [Header("checkUI")]
    public GameObject checkPanel;


    public Transform target;
    public List<GameObject> activeCardInstances = new List<GameObject>();
    
    

    public void Start()
    {
        playerCards = cardManager.card;
        DOTween.Init();

    }
    #region 레이 충돌시 관련(추후 사용예정)
    public void GetLayName(string buttonName)
    {
        switch (buttonName)
        {
            // ---------------------   게임 내 버튼     ------------------------------

            case "OnIncreaseScoreButton":
                OnIncreaseScoreButtonClicked(); // 승수 증가
                break;

            case "OnDecreaseScoreButton":
                OnDecreaseScoreButtonClicked(); // 승수 감소
                break;

            case "StartButton":
                OnStartButtonClicked(); // 시작 버튼 클릭 시 호출
                break;

            case "ExitButton":
                OnExitButtonClicked(); // 나가기 버튼 클릭 시 호출
                break;

            case "SettingsButton":
                OnSettingsButtonClicked(); // 설정 버튼 클릭 시 호출
                break;

            case "CardButton":
                //OnSelectCardButtonClicked(); // 카드 선택 버튼 클릭 시 호출,인자넣어야함
                break;

            case "HeartButton":
                //OnSelectHeartButtonClicked(); // 심지 선택 버튼 클릭 시 호출
                break;

            case "SubmitButton":
                //OnSubmitCardButtonClicked(); // 제출 버튼 클릭 시 호출
                break;

            case "ReadyButton":
                OnReadyButtonClicked(); // 레디 버튼 클릭 시 호출
                break;
            ////// ---------------------   게임 내 버튼     ------------------------------


            ////// ---------------------   게임 외부 버튼     ------------------------------
            case "GoogleLoginButton":
                OnGoogleLoginButtonClicked(); // 구글 로그인 버튼 클릭 시 호출
                break;

            case "SignUpButton":
                OnSignUpButtonClicked(); // 회원가입 버튼 클릭 시 호출
                break;

            case "LoginButton":
                OnLoginButtonClicked(); // 로그인 버튼 클릭 시 호출
                break;

            case "LogoutButton":
                OnLogoutButtonClicked(); // 로그아웃 버튼 클릭 시 호출    
                break;

            case "ExitAppButton":
                OnExitAppButtonClicked(); // 앱 나가기 버튼 클릭 시 호출
                break;

            case "ChangePasswordButton":
                OnChangePasswordButtonClicked(); // 비밀번호 변경 버튼 클릭 시 호출
                break;

            case "ChangeNicknameButton":
                OnChangeNicknameButtonClicked(); // 닉네임 변경 버튼 클릭 시 호출
                break;

            case "CreateRoomButton":
                OnCreateRoomButtonClicked(); // 방 생성 버튼 클릭 시 호출
                break;

            case "JoinRoomButton":
                OnJoinRoomButtonClicked(); // 방 진입 버튼 클릭 시 호출
                break;

            case "StartSinglePlayerButton":
                OnStartSinglePlayerButtonClicked(); // 싱글 플레이 시작 버튼 클릭 시 호출
                break;

            case "StartTutorialVideoButton":
                OnStartTutorialVideoButtonClicked(); // 튜토리얼 영상 시작 버튼 클릭 시 호출
                break;
            
            case "OnWithdrawButton":
                OnWithdrawButtonClicked(); // 회원 탈퇴 버튼클릭시
                break;

            case "OnEmailVerificationButton":
                OnEmailVerificationButtonClicked();
                break;
            default:
                Debug.Log("알 수 없는 버튼: " + buttonName); //  그 외의 버튼 처리
                break;
            ////// ---------------------   게임 외부 버튼     ------------------------------
        }

    }
    #endregion 

    
    #region 승 수 관련
    public void ShowPlayerPanel(bool onoff)
    {
        Transform PlayerPanel = gameManager.playerList[gameManager.curTurn].transform.Find("Player_Canvas/Panel");
        GameObject playerPanel = PlayerPanel.gameObject;

        
        if (onoff)
        {
            playerPanel.SetActive(true);
        }
        else
        {
            playerPanel.SetActive(false);
        }
    }
    public void showWinBtn( )
    {
        Transform WinBtnParent = gameManager.playerList[gameManager.curTurn].transform.Find("winBtn");
        GameObject winBtnParent = WinBtnParent.gameObject;
        winBtnParent.SetActive(true);
        
    }
    public void hideWinBtn()
    {
        Transform WinBtnParent = gameManager.playerList[gameManager.curTurn].transform.Find("winBtn");
        GameObject winBtnParent = WinBtnParent.gameObject;
        winBtnParent.SetActive(false);
        
    }
    public void OnIncreaseScoreButtonClicked() // 승수 증가
    {
        
        Transform PlayerPanelPos = gameManager.playerList[gameManager.curTurn].transform.Find("Player_Canvas/Panel/P1_LogMain");
        Text playerText = PlayerPanelPos.GetComponent<Text>();

        expectedWin++;
        logText.text = "예상 승리횟수 : " + expectedWin.ToString() + "번";
        playerText.text = "예상 승리횟수 : " + expectedWin.ToString() + "번";
    }
   
    public void OnDecreaseScoreButtonClicked() // 승수 감소
    {
        
        Transform PlayerPanelPos = gameManager.playerList[gameManager.curTurn].transform.Find("Player_Canvas/Panel/P1_LogMain");
        Text playerText = PlayerPanelPos.GetComponent<Text>();
        expectedWin--;
        logText.text = "예상 승리횟수 " + expectedWin.ToString() + "번";
        playerText.text = "예상 승리횟수 : " + expectedWin.ToString() + "번";
    }
    public void OnSubmitScoreButtonClicked()
    {
        Transform PlayerPanelPos = gameManager.playerList[gameManager.curTurn].transform.Find("Player_Canvas/Panel/P1_LogMain");
        Text playerText = PlayerPanelPos.GetComponent<Text>();
        logText.color = Color.black;
        //logText.text = "예상 승리횟수 : " + expectedWin.ToString() + "번 제출완료";
        logText.text = "";
        logText.DOText("승리횟수 :" + expectedWin.ToString()+"번 제출완료", 2);
        playerText.text = "예상 승리횟수 : " + expectedWin.ToString() + "번 제출완료";
        if (expectedWin >= 0 && expectedWin <= 4)
        {
            gameManager.predictedWinCnt[gameManager.curTurn] = expectedWin;
            gameManager.selectedWin = 0;
        }
        else
        {
            playerText.text = "0에서 4사이의 값만 넣어라";
            logText.text = "0에서 4사이의 값만 넣어라";
            logText.color = Color.red;
        }
    }
    #endregion

    #region 카드 관련
    public void OnSelectCardButtonClicked(Button clickBtn) // 카드 선택 버튼 기능,클릭한 버튼 인자로 받음
    {
        
        //// 레이에 맞은 오브젝트의 테두리 색깔과 y값 포지션 증가
        //CardOutline = clickBtn.GetComponent<Outline>();
        //Vector3 CardClickYPosition = clickBtn.transform.position;

        //if (CardOutline != null)
        //{
        //    originalOutlineColor = CardOutline.effectColor; // 원래 색상 저장
        //    CardOutline.effectColor = Color.yellow; // 테두리 색상을 노란색으로 변경
        //}

        //originalCardPosition = clickBtn.transform.position; // 원래 카드 위치 저장
        //CardClickYPosition = originalCardPosition; // 현재 위치 값 저장
        //CardClickYPosition.y += 50; // y값 증가
        //clickBtn.transform.position = CardClickYPosition;// 카드 위치 변경
        //checkPanel.SetActive(true);

        //// 선택하시겠습니까?의 ui 활성화
        //// CardManager의 제출된 함수 쪽으로 해당 카드 제출
        ////testBtn.gameObject.SetActive(true); // 버튼 활성화

    }


    public void ShowCard(List<int> cardList)
    {
        // 이전에 생성된 카드 인스턴스 삭제
        foreach (var cardInstance in activeCardInstances)
        {
            Destroy(cardInstance);
        }
        activeCardInstances.Clear(); // 리스트 초기화

        // 현재 플레이어의 CardImage 자식 객체 가져오기
        Transform[] cardPositions = new Transform[4];
        cardPositions[0] = gameManager.playerList[gameManager.curTurn].transform.Find("CardImage/FirstCardPos");
        cardPositions[1] = gameManager.playerList[gameManager.curTurn].transform.Find("CardImage/SecondCardPos");
        cardPositions[2] = gameManager.playerList[gameManager.curTurn].transform.Find("CardImage/ThirdCardPos");
        cardPositions[3] = gameManager.playerList[gameManager.curTurn].transform.Find("CardImage/FourthCardPos");


        for (int i = 0; i < cardList.Count && i < cardPositions.Length; i++)
        {
            int cardValue = cardList[i];

            // 카드 값에 맞는 프리팹 선택
            GameObject cardPrefab = cardButtonPrefab[cardValue - 1]; // 카드 값이 1부터 시작
            Transform targetPosition = cardPositions[i];

            if (targetPosition == null)
            {
                Debug.LogWarning($"카드위치 {i + 1} 을 찾을수가없네");
                continue;
            }

            // 카드 인스턴스 생성 및 위치 설정
            GameObject myInstance = Instantiate(cardPrefab, targetPosition.position, targetPosition.rotation);
            myInstance.transform.SetParent(targetPosition); 

            // 활성화된 인스턴스 저장
            activeCardInstances.Add(myInstance);

            // 버튼 설정
            Button buttonComponent = myInstance.GetComponent<Button>();
            int capturedValue = cardValue; // 로컬 변수로 캡처
            buttonComponent.onClick.AddListener(() => OnSubmitCardButtonClicked(capturedValue, myInstance));
        }
    }


    public void OnSubmitCardButtonClicked(int cardValue,GameObject cardInstance) // 카드제출 버튼 기능
    {
        List<int> curCardList = gameManager.playerList[gameManager.curTurn].GetComponent<PlayerController>().cardList;

        // 카드 리스트에서 선택된 카드 제거 및 제출된 카드 리스트에 추가
        curCardList.Remove(cardValue);
        gameManager.submitCardList.Add(cardValue);
        
        //선택된 카드 파괴
        activeCardInstances.Remove(cardInstance);
        Destroy(cardInstance);



        // 제출 완료 메시지
        logText.text = "";
        logText.DOText("카드값 : "+cardValue.ToString() + "제출완료", 2.2f);
        
        selectCard = 0;
        gameManager.checkSubmitCard = selectCard;
        
        

        Debug.Log($"{gameManager.playerList[gameManager.curTurn].name}의 제출 카드: {cardValue}");
    }





    #endregion


    #region 폭탄 심지 관련
    public void OnSelectHeartButtonClicked(Button clickHeartBtn) // 심지 선택 버튼 기능
    {
        BombWickOutline = clickHeartBtn.GetComponent<Outline>();
        Vector3 BombWickClickYPosition = clickHeartBtn.transform.position;

        if (CardOutline != null)
        {

            originalOutlineColor = CardOutline.effectColor; // 원래 색상 저장
            CardOutline.effectColor = Color.yellow; // 테두리 색상을 노란색으로 변경
        }

        originalBombWickPosition = clickHeartBtn.transform.position; // 원래 카드 위치 저장
        BombWickClickYPosition = originalBombWickPosition; // 현재 위치 값 저장
        BombWickClickYPosition.y += 50; // y값 증가
        clickHeartBtn.transform.position = BombWickClickYPosition;// 카드 위치 변경
        


        //// 레이에 맞은 오브젝트 색깔 변화
        //// 선택하시겠습니까?의 ui 활성화
        //// 게임매니저에게 선택된 심지 전송
    }
    public void SendSelectedBombButtonClicked() //선택한 심지의 값을 게임매니저로 보내
    {
        gameManager.selectedBomb = 0;         

        // 게임 매니저에 선택된 심지의 값을 전달
        

        if (gameManager != null)
        {
            //gameManager.CheckBomb(selectedBombIndex);  // 심지의 인덱스를 게임 매니저로 전달
            logText.text = "선택된 폭탄 심지 제출 완료";
        }
        else
        {
            Debug.LogWarning("gameManager가 null임.");
        }

        /*if (CardOutline != null)
        {
            BombWickOutline.effectColor = originalBombWickOutlineColor;
        }

        if (originalCardPosition != null)
        {
            BombWickOutline.transform.position = originalBombWickPosition;
        }*/

        //게임매니저

    }
    #endregion
    


    public void OnReadyButtonClicked() // 레디 버튼 기능
    { 
       // ready 값을 true로 설정
       // 게임매니저에게 자신의 ready값 전송
    }
    
    public void OnStartButtonClicked() // 시작 버튼 기능
    {
        // 방장 플레이어만 사용이 가능
        // 게임매니저로부터 각 플레이어들의 레디값을 확인후 모두 true일경우 버튼 활성화 
        // 해당 버튼 클릭시 게임 시작
    }

    
    public void OnExitButtonClicked() // 나가기 버튼 기능
    {
        //
    }

    
    public void OnSettingsButtonClickedInGame()  // 설정 버튼 기능
    {
        //설정 UI TRUE로 변경
    }
    
    public void OnSignUpButtonClicked() // 회원가입 버튼 기능
    {
        // 회원가입시의 입려된 정보(아이디,패스워드 등등) firebase에 등록
        
    }

    
    public void OnEmailVerificationButtonClicked() // 이메일 인증 확인 버튼 기능
    {
        // 입력한 이메일값과 firebase에서의 이메일이 같은지 확인
    }

    
    public void OnLoginButtonClicked() // 로그인 버튼 기능
    {
        // 입력된 아이디,비밀번호를 firebase에 전송
        // 같은 값이 있다면 로그인 성공 및 씬 이동
    }

    
    public void OnLogoutButtonClicked() // 로그아웃 버튼 기능
    {
        // 로그인 창으로 이동
    }

    
    public void OnExitAppButtonClicked()  // 앱 나가기 버튼 기능
    { 
        // 자동 저장 
        Application.Quit();
    }

    
    public void OnGoogleLoginButtonClicked() // 구글 계정 로그인 버튼 기능
    {
        
    }

    
    public void OnWithdrawButtonClicked() // 회원 탈퇴 버튼 기능
    {
        // firebase에서 해당 유저의 정보 삭제

    }

    
    public void OnChangePasswordButtonClicked() // 비밀번호 변경 버튼 기능

    {
        // 현재 유저의 비밀번호 값을 입력된 값으로 변경
    }

    
    public void OnChangeNicknameButtonClicked() // 닉네임 변경 버튼 기능
    { 
        // 입력된 닉네임으로 값 변경
    }

    
    public void OnCreateRoomButtonClicked() // 방 생성 버튼 기능
    {
        // 방생성에 필요한 값들을 체크후 방 생성
    }

    
    public void OnStartSinglePlayerButtonClicked() // 싱글 플레이 시작 버튼 기능
    {
        //플레이어 1몀(자기자신),AIPlayer3명을 추가해 4명에서 게임시작
    }

   
    public void OnSettingsButtonClicked() // 설정 버튼 기능
    {
       
    }

    
    public void OnJoinRoomButtonClicked()  // 방 진입 버튼 기능
    {

        // ------------------비번이 있을경우-------------------
        // 입력된값과 설정된 방 비밀번호값 비교
        // 값이 동일할시 입장 다르다면 입장불가
    }


    
    public void OnStartTutorialVideoButtonClicked() // 튜토리얼 영상 시작 버튼 기능
    {
        // 영상 보여주기 
    }

    
}
