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

    bool emailKey = false;
    bool passwordKey = false;

    private void Start()
    {
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

    public void LeaveEmailKeyboard()
    {
        keyBoard.gameObject.SetActive(false);
        emailKey = false;
    }
    public void LeavePWKeyboard()
    {
        keyBoard.gameObject.SetActive(false);
        passwordKey = false;
    }

    private void Update()
    {
    }
    public void ChangeString()
    {
        if (emailKey)
        {
            emailInput.text = keyBoard.TextHandler.Text;
        }
        else if (passwordKey)
        {
            passwordInput.text = keyBoard.TextHandler.Text;
        }
    }

    public void KeyboardEnd()
    {
        keyBoard.gameObject.SetActive(false);
        emailKey = false;
        passwordKey = false;
    }
}
