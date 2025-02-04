using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyKeyBoard : MonoBehaviour
{
    public GameObject keyBoardPanel;
    public Button capsLockBtn;

    public InputField roomNameField;

    public bool roomOn = true;
    public bool capsLockOn = true;

    public List<TMP_Text> keyBtnTxts;
    // Start is called before the first frame update
    void Start()
    {
        roomOn = true;
        capsLockOn = true;
    }
    public void SetKeyBoardRoom()
    {
        keyBoardPanel.SetActive(true);
        roomOn = true;
    }

    public void WriteChar(string str)
    {
        if (roomOn)
        {
            if (capsLockOn)
            {
                str = str.Replace(str, str.ToUpper());
            }
            roomNameField.text += str;
        }
    }
    public void EraseChar()
    {
        if (roomOn)
        {
            if (roomNameField.text.Length > 0)
            {
                roomNameField.text = roomNameField.text.Substring(0, roomNameField.text.Length - 1);
            }
        }
    }

    public void ClickCloseOrEnter()
    {
        keyBoardPanel.SetActive(false);
        roomOn = false;
    }

    public void ClickCapsLockBtn()
    {
        if (capsLockOn)
        {
            capsLockOn = false;
            capsLockBtn.GetComponent<Image>().color = Color.grey;
            for (int i = 0; i < keyBtnTxts.Count; i++)
            {
                string str = keyBtnTxts[i].text.Replace(keyBtnTxts[i].text, keyBtnTxts[i].text.ToLower());
                keyBtnTxts[i].text = str;
            }
        }
        else
        {
            capsLockOn = true;
            capsLockBtn.GetComponent<Image>().color = Color.white;
            for (int i = 0; i < keyBtnTxts.Count; i++)
            {
                string str = keyBtnTxts[i].text.Replace(keyBtnTxts[i].text, keyBtnTxts[i].text.ToUpper());
                keyBtnTxts[i].text = str;
            }
        }
    }
}
