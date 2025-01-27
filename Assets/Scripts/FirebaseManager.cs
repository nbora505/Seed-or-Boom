using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseManager : MonoBehaviour
{
    public GetDataFirebase getDataFirebase;
   
    public Text userUID;
    public Text userEmail;
    public Text userNick;
    public Text userWin;
    public Text userLose;
    public Text userChar;
    // Start is called before the first frame update
    void Start()
    {
        getDataFirebase = GameObject.Find("FirebaseManager").GetComponent<GetDataFirebase>();
        
    }

    public void GetPlayerInfo()
    {
        getDataFirebase.GetData();
        userUID.text = $"UID : {getDataFirebase.userUID}";
        userEmail.text = $"UID : {getDataFirebase.userEmail}";
        userNick.text = $"UID : {getDataFirebase.userNick}";
        userWin.text = $"UID : {getDataFirebase.userWin}";
        userLose.text = $"UID : {getDataFirebase.userLose}";
        userChar.text = $"UID : {getDataFirebase.userChar}";

    }
}
