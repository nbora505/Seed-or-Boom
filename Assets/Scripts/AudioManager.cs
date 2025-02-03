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
    public UnityEngine.UI.Slider soundBar;
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

    public void VolumeChange()
    { 
        audioSource.volume = soundBar.value;
    }
}
