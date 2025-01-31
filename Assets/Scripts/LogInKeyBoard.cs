using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using TMPro;
using UnityEngine.UI;

public class LogInKeyBoard : MonoBehaviour
{
    public OVRVirtualKeyboard keyBoard;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public GameObject p1;

    bool emailKey = false;
    bool passwordKey = false;

    private void Start()
    {
        p1.transform.position = Vector3.zero;
        p1.transform.rotation = Quaternion.Euler(0, 0, 0);

        keyBoard.CommitTextEvent.AddListener(ChangeString);
        keyBoard.EnterEvent.AddListener(KeyboardEnd);
        keyBoard.BackspaceEvent.AddListener(BackSpaceKeyBoard);

        SetKeyBoardEmail();
    }
    public void SetKeyBoardEmail()
    {
        keyBoard.gameObject.SetActive(true);
        emailKey = true;
    }
    public void SetKeyBoardPassWord()
    {
        keyBoard.gameObject.SetActive(true);
        passwordKey = true;
    }

    public void ChangeString(string str)
    {
        if (emailKey)
        {
            emailInput.text += str;
        }
        else if (passwordKey)
        {
            passwordInput.text += str;
        }
    }

    public void BackSpaceKeyBoard()
    {
        if (emailKey)
        {
            emailInput.text.Remove(emailInput.text.Length -1);
        }
        else if (passwordKey)
        {
            passwordInput.text.Remove(passwordInput.text.Length - 1);
        }
    }

    public void KeyboardEnd()
    {
        keyBoard.gameObject.SetActive(false);
        emailKey = false;
        passwordKey = false;
    }
}
