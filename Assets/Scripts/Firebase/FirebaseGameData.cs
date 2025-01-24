using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FirebaseGameData : MonoBehaviour
{
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

    FirebaseMain firebaseMain;

    public int daysGameCnt;
    // Start is called before the first frame update
    void Start()
    {
    }

    /// <summary>
    /// made DB on fireStore. this data is game's result data. this fun is called end of each game.
    /// </summary>
    /// <param name="member1">other members nickname. If other member is AI please write bot. else if other member is null. please write blank"
    /// </param>
    /// <param name="member2">other members nickname. If other member is AI please write bot. else if other member is null. please write blank"
    /// </param>
    /// <param name="member3">other members nickname. If other member is AI please write bot. else if other member is null. please write blank"
    /// </param>
    /// <param name="mode">Single or Multi. Be careful to write "Single".
    /// </param>
    /// <param name="startTime">please write Game Start Time's TimeStamp
    /// </param>
    /// <param name="winner">please write winner's nickname
    /// </param>
    public void UpdateEndGameData(string member1, string member2, string member3,
        string mode, Timestamp startTime, string winner)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        string day = DateTime.Today.ToString("MMdd");

        //for (int gameNum = 1; gameNum < 1000; gameNum++)
        //{
        //    Debug.Log(gameNum);
        //    DocumentReference docRef = db.Collection("gameData").Document(firebaseMain.auth.CurrentUser.UserId).Collection(day).Document("game" + gameNum);
        //    docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        //    {
        //    });
        //} 
    }

    /// <summary>
    /// select day's game count. It change dayGameCnt.
    /// </summary>
    /// <param name="day"></param>
    public void SearchDaysGameCnt(int day)
    {
        
    }
}
