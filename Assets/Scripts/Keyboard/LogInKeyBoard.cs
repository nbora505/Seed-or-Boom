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

    public void SetKeyBoardNick()
    {
        keyboadrdLogin.SetActive(true);
    }
}
