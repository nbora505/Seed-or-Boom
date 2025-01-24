using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InLobbyPlayerController : MonoBehaviour
{
    public GameObject multiCanvas;
    public GameObject singleCanvas;
    public GameObject playerInfoCanvas;

    // Start is called before the first frame update
    void Start()
    {
        multiCanvas.SetActive(false);
        singleCanvas.SetActive(false);
        playerInfoCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "MultiPlayColider") multiCanvas.SetActive(true);
        if (other.gameObject.name == "PlayerInfoColider") playerInfoCanvas.SetActive(true);
        if (other.gameObject.name == "SinglePlayColider") singleCanvas.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "MultiPlayColider") multiCanvas.SetActive(false);
        if (other.gameObject.name == "PlayerInfoColider") playerInfoCanvas.SetActive(false);
        if (other.gameObject.name == "SinglePlayColider") singleCanvas.SetActive(false);
    }
}
