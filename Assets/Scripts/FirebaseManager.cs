using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
public class FirebaseManager : MonoBehaviour
{
    private FirebaseFirestore db;
    private FirebaseAuth auth;

    public GetDataFirebase getDataFirebase;
   
    public Text userUID;
    public Text userEmail;
    public Text userNick;
    public Text userWin;
    public Text userLose;
    public Text userChar;

    public Text member1;
    public Text member2;
    public Text member3;
    public Text mode;
    public Text startTime;
    public Text winner;
    // Start is called before the first frame update
    public void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }
    void Start()
    {
        getDataFirebase = GameObject.Find("GetDataFirebase").GetComponent<GetDataFirebase>();

    }
    /// <summary>
    /// 게임 결과를 Firebase Firestore에 저장
    /// </summary>
    /// 

    public void SaveGameResult(string winner, List<string> players)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("Firebase 인증된 사용자가 없다");
            return;
        }

        Dictionary<string, object> gameData = new Dictionary<string, object>
        {
            { "winner", winner },
            { "players", players },
            { "gameMode", "Multi" },
            { "timestamp", FieldValue.ServerTimestamp }
        };

        db.Collection("gameResults").AddAsync(gameData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("게임 결과가 Firestore에 저장됨");
            }
            else
            {
                Debug.LogError("게임 결과 저장 실패: " + task.Exception);
            }
        });
    }

    /// <summary>
    /// 현재 로그인된 유저의 승리/패배 횟수를 업데이트
    /// </summary>
    public void UpdateUserWin(bool isWinner)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("Firebase 인증된 사용자가 없습니다!");
            return;
        }

        DocumentReference userDocRef = db.Collection("users").Document(auth.CurrentUser.UserId);

        Dictionary<string, object> updateData = new Dictionary<string, object>();

        if (isWinner)
        {
            updateData["winner"] = FieldValue.Increment(1); // 승리 횟수 +1
        }
        

        userDocRef.UpdateAsync(updateData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("유저의 승/패 기록이 업데이트되었습니다.");
            }
           
        });
    }
    public IEnumerator GetPlayerInfo()
    {
        getDataFirebase.GetData();
        yield return new WaitForSeconds(1);
        userUID.text = $"UID : {getDataFirebase.userUID}";
        userEmail.text = $"Email : {getDataFirebase.userEmail}";
        userNick.text = $"NickName : {getDataFirebase.userNick}";
        userWin.text = $"Win : {getDataFirebase.userWin}";
        userLose.text = $"Lose : {getDataFirebase.userLose}";
        
    }
    public void GetPlayerInfo2()
    {
        StartCoroutine( GetPlayerInfo() );
    }


    public IEnumerator GetGamedata()
    {
        getDataFirebase.GetGameData(1,1);
        yield return new WaitForSeconds(1);
        member1.text = $"member1 : {getDataFirebase.member1}";
        member2.text = $"member2 : {getDataFirebase.member2}";
        member3.text = $"member3 : {getDataFirebase.member3}";
        mode.text = $"mode : {getDataFirebase.mode}";
        startTime.text = $"startTime : 2025년 2월 4일 오전11시";
        winner.text = $"winner : {getDataFirebase.winner}";
    }
    public void GetGameData()
    {
        StartCoroutine( GetGamedata() );
    }
}
