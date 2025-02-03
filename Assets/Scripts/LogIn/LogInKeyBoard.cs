using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using TMPro;
using UnityEngine.UI;

public class LogInKeyBoard : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public GameObject p1;

    public KeyBoardControllerLogIN keyBoardScript;

    public GameObject keyboadrdLogin;
    private void Start()
    {
        p1.transform.position = Vector3.zero;
        p1.transform.rotation = Quaternion.Euler(0, 0, 0);

        SetKeyBoardEmail();
    }
    public void SetKeyBoardEmail()
    {
        keyboadrdLogin.SetActive(true);
        keyBoardScript.GetComponent<KeyBoardControllerLogIN>().emailOn = true;
    }
    public void SetKeyBoardPassWord()
    {
        keyboadrdLogin.SetActive(true);
        keyBoardScript.pwOn = true;
    }

    public void ChangeString(string str)
    {
        if (keyBoardScript.emailOn)
        {
            emailInput.text += str;
        }
        else if (keyBoardScript.pwOn)
        {
            passwordInput.text += str;
        }
    }

    public void BackSpaceKeyBoard()
    {
        if (keyBoardScript.emailOn)
        {
            emailInput.text.Remove(emailInput.text.Length -1);
        }
        else if (keyBoardScript.pwOn)
        {
            passwordInput.text.Remove(passwordInput.text.Length - 1);
        }
    }

    public void KeyboardEnd()
    {
        keyBoardScript.emailOn = false;
        keyBoardScript.pwOn = false;
    }
}
