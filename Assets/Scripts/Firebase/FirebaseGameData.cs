using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
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
        public string winner { get; set; }

        [FirestoreProperty]
        public Timestamp startTime { get; set; }
    }

    public FirebaseMain firebaseMain;

    public int daysGameCnt;

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

        StartCoroutine(QueryCheck(day));

        DocumentReference docRef = db.Collection("gameData").Document(firebaseMain.auth.CurrentUser.UserId)
            .Collection(day).Document("game" + daysGameCnt);
        GameDataProperty updateDic = new GameDataProperty()
        {
            member1 = member1,
            member2 = member2,
            member3 = member3,
            mode = mode,
            startTime = startTime,
            winner = winner
        };

        //upadte new data to user's doc.
        docRef.SetAsync(updateDic).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Update Complete");
            }
            else
            {
                Debug.Log("Plese Update One More");
            }
        });

        if (daysGameCnt == 0)
        {
            DocumentReference docRefUser = db.Collection("gameData")
                .Document(firebaseMain.auth.CurrentUser.UserId);
            Dictionary<string, object> colNameDic = new Dictionary<string, object>()
            {
                {"collectionNames", FieldValue.ArrayUnion(day) }
            };
            docRefUser.UpdateAsync(colNameDic).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log("collectionName Update");
                }
                else
                {
                    Debug.Log("bug!");
                }
            });

            DeleteDocOver30();
            //remove the data over 30 days.
        }
    }

    /// <summary>
    /// select day's game count. It change dayGameCnt.
    /// </summary>
    /// <param name="day"></param>
    public void SearchDaysGameCnt(int day)
    {
        StartCoroutine(QueryCheck(day.ToString()));
    }

    private IEnumerator QueryCheck(string dayCollection)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        Query dayCollectionQuery = db.Collection("gameData")
            .Document(firebaseMain.auth.CurrentUser.UserId).Collection(dayCollection);

        yield return dayCollectionQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                QuerySnapshot snapshotDaysDocCnt = task.Result;
                daysGameCnt = snapshotDaysDocCnt.Count;
            }
            else
            {
                daysGameCnt = 0;
            }
        });
    }

    private void DeleteDocOver30()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        DocumentReference docRef = db.Collection("gameData")
            .Document(firebaseMain.auth.CurrentUser.UserId);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result.Exists)
            {
                List<string> collections = task.Result.GetValue<List<string>>("collectionNames");
                collections.ForEach(collection =>
                {
                    if (int.Parse(collection) <= int.Parse(DateTime.Now.AddDays(-30).ToString("MMdd")))
                    {
                        CollectionReference colRef = docRef.Collection(collection);

                        Query colQuery = docRef.Collection(collection);
                        colQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
                        {
                            QuerySnapshot snaps = task.Result;
                            List<System.Threading.Tasks.Task> listTask
                            = new List<System.Threading.Tasks.Task>();
                            foreach (DocumentSnapshot docSnap in snaps.Documents)
                            {
                                listTask.Add(docSnap.Reference.DeleteAsync());
                                //docSnap.Reference.DeleteAsync();
                            }

                            System.Threading.Tasks.Task.WhenAll(listTask).Wait();

                            Dictionary<string, object> colNameDic = new Dictionary<string, object>()
                            {
                                {"collectionNames", FieldValue.ArrayRemove(collection)}
                            };
                            docRef.UpdateAsync(colNameDic);
                        });
                    }
                });
            }
        });
    }

    private void Awake()
    {
        //DontDestroy();
    }
    private void DontDestroy()
    {
        if (GameObject.Find(gameObject.name))
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
