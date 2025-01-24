using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;

public class GetDataFirebase : MonoBehaviour
{   
    /// <summary>
     /// GameData Dicionary class.
     /// </summary>
    [FirestoreData]
    class GameDataProperty
    {
        [FirestoreProperty]
        public string member1 { get; set; }
        [FirestoreProperty]
        public string member2 { get; set; }
        [FirestoreProperty]
        public string member3 { get; set; }

        [FirestoreProperty]
        public string mode { get; set; }

        [FirestoreProperty]
        public string where { get; set; }

        [FirestoreProperty]
        public string result { get; set; }

        [FirestoreProperty]
        public string winner { get; set; }

        [FirestoreProperty]
        public string selectChar { get; set; }

        [FirestoreProperty]
        public Timestamp startTime { get; set; }

        [FirestoreProperty]
        public Timestamp endTime { get; set; }
    }

    /// <summary>
    /// These property are currentUser's data of DB.
    /// You can use these after call fun GetData().
    /// </summary>
    public string userUID;
    public string userEmail;
    public string userNick;
    public int userWin;
    public int userLose;
    public string userChar;

    /// <summary>
    /// These property are currentUser's game data of DB.
    /// You can use these after call fun GetGameData().
    /// </summary>
    public string member1;
    public string member2;
    public string member3;
    public string mode;
    public Timestamp startTime;
    public Timestamp endTime;
    public string where;
    public string result;
    public string winner;
    public string selectChar;

    FirebaseMain firebaseMain;

    #region dataSearch
    /// <summary>
    /// After call this fun, we can use currentUser's userdata of DB
    /// </summary>
    public void GetData()
    {
        GetDataToUserCollection();
    }

    /// <summary>
    ///  After call this fun, we can use currentUser's gamedata of DB
    /// </summary>
    /// <param name="day">Collection name. when day did you play the game. EX) 0124
    /// </param>
    /// <param name="gameNum">Doccument Name. Is that a first game of day? or more? EX) 2
    /// </param>
    public void GetGameData(int day, int gameNum)
    {
        GetDataToGameDataCollection(day, gameNum);
    }

    /// <summary>
    /// Init property and you can use userdata property
    /// </summary>
    private void GetDataToUserCollection()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("users").Document(firebaseMain.auth.CurrentUser.UserId);

        //pull your Email to DB.
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snap = task.Result;
            if ((snap.Exists))
            {
                Dictionary<string, object> snapDic = snap.ToDictionary();
                userUID = snapDic["uid"].ToString();
                userEmail = snapDic["email"].ToString();
                userNick = snapDic["nickName"].ToString();
                userWin = (int)snapDic["win"];
                userLose = (int)snapDic["lose"];
                userChar = snapDic["char"].ToString();
                Debug.Log("data search success");
            }
            else
            {
                Debug.Log("search db data error");
            }
        });
    }

    /// <summary>
    /// Init property. You can use GameData property.
    /// </summary>
    /// <param name="day">Collection name. when day did you play the game. EX) 0124
    /// </param>
    /// <param name="gameNum">Doccument Name. Is that a first game of day? or second? or more?
    /// </param>
    private void GetDataToGameDataCollection(int day, int gameNum)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("gameData").Document(firebaseMain.auth.CurrentUser.UserId).Collection(day.ToString()).Document("game"+gameNum);

        //pull your Email to DB.
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snap = task.Result;
            if ((snap.Exists))
            {
                //Change custom class
                GameDataProperty snapGameData = snap.ConvertTo<GameDataProperty>();
                member1 = snapGameData.member1;
                member2 = snapGameData.member2;
                member3 = snapGameData.member3;
                mode = snapGameData.mode;
                startTime = snapGameData.startTime;
                endTime = snapGameData.endTime;
                where = snapGameData.where;
                result = snapGameData.result;
                winner = snapGameData.winner;
                selectChar = snapGameData.selectChar;
                Debug.Log("data search success");
            }
            else
            {
                Debug.Log("check Day or GameNum");
            }
        });
    }

    #endregion
}
