using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip btnSound;
    public AudioClip sfxSound;
    public AudioClip uiBtnSound;
    public AudioClip bombSound;
    public AudioClip typingSound;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BtnSound()
    {
        audioSource.PlayOneShot(btnSound);
    }
    public void SfxSound()
    {
        audioSource.PlayOneShot(sfxSound);
    }
    public void UIBtnSound()
    {
        audioSource.PlayOneShot(uiBtnSound, 4);
    }
    public void BombSound()
    {
        audioSource.PlayOneShot(bombSound);
    }
    public void TypingSound()
    {
        audioSource.PlayOneShot(typingSound);
    }
}
