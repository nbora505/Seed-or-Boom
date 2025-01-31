using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip btnSound;
    public AudioClip sfxSound;
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
}
