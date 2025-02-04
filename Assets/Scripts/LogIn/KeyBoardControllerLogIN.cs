using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyBoardControllerLogIN: MonoBehaviour
{
    public GameObject keyBoardPanel;
    public GameObject nickPanel;
    public Button capsLockBtn;

    public TMP_InputField emailField;
    public TMP_InputField pwField;
    public TMP_InputField nickField;

    public bool emailOn = true;
    public bool pwOn = false;
    public bool capsLockOn = true;

    public List<TMP_Text> keyBtnTxts;

    public FirebaseMain firebaseMain;
    // Start is called before the first frame update
    void Start()
    {
        emailOn = true;
        pwOn = false;
        capsLockOn = true;
    }

    public void WriteChar(string str)
    {
        if (emailOn)
        {
            if (capsLockOn)
            {
                str = str.Replace(str, str.ToUpper());
            }
            emailField.text += str;
        }
        else if (pwOn)
        {
            if (capsLockOn)
            {
                str = str.Replace(str, str.ToUpper());
            }
            pwField.text += str;
        }
        else if (!emailOn && !pwOn && nickField)
        {
            if (capsLockOn)
            {
                str = str.Replace(str, str.ToUpper());
            }
            nickField.text += str;
        }
    }
    public void EraseChar()
    {
        if (emailOn)
        {
            if (emailField.text.Length > 0)
            {
                emailField.text = emailField.text.Substring(0, emailField.text.Length - 1);
            }
        }
        else if (pwOn)
        {
            if (pwField.text.Length > 0)
            {
                pwField.text = pwField.text.Substring(0, pwField.text.Length - 1);
            }
        }
        else if (!emailOn && !pwOn && nickField)
        {
            if (nickField.text.Length > 0)
            {
                nickField.text = nickField.text.Substring(0, nickField.text.Length - 1);
            }
        }
    }

    public void ClickCloseOrEnter()
    {
        emailOn = false;
        pwOn = false;
        keyBoardPanel.SetActive(false);
    }

    public void EnterNick()
    {
        firebaseMain.UpdateNickInLogin();
    }

    public void CloseNick()
    {
        nickField.text = "";
        nickPanel.SetActive(false);
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
