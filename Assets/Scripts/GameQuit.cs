using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameQuit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void GoMap1()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("Map1");

    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Map1")
        {       
            SetPlayer();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
    }
    public void SetPlayer()
    {
        GameObject playersParent = GameObject.Find("Players"); // 부모 오브젝트 찾기
        
        string[] playerNames = { "Player2", "Player3", "Player4" };

        foreach (string playerName in playerNames)
        {
            Transform playerTransform = playersParent.transform.Find(playerName);        
            PlayerController playerController = playerTransform.GetComponent<PlayerController>();          
            playerController.isAIPlayer = true;
                 
        }
    }
}
