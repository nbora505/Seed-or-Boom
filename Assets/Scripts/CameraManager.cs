using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject parentObject;

    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void CameraMoveTrue(int playernum)
    {
        parentObject = GameObject.Find("CameraPos");
        GameObject cameraPos = parentObject.transform.Find("Camera").gameObject;

        cameraPos.SetActive(true);
    }
    public void CameraMoveFalse(int playernum)
    {
        parentObject = GameObject.Find("CameraPos");
        GameObject cameraPos = parentObject.transform.Find("Camera").gameObject;
        cameraPos.SetActive(false);    
    }
}
