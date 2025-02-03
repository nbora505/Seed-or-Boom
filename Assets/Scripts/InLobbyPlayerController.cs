using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class InLobbyPlayerController : MonoBehaviour
{
    public GameObject multiCanvas;
    public GameObject singleCanvas;
    public GameObject playerInfoCanvas;

    Vector3 multiCanvasScale;
    Vector3 singleCanvasScale;
    Vector3 playerInfoScale;

    // Start is called before the first frame update
    void Start()
    {
        DOTween.Init();
        multiCanvas.SetActive(false);
        singleCanvas.SetActive(false);
        playerInfoCanvas.SetActive(false);

        multiCanvasScale = multiCanvas.transform.localScale;
        singleCanvasScale = singleCanvas.transform.localScale;
        playerInfoScale = singleCanvas.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "MultiPlayColider")
        {
            multiCanvas.transform.localScale = Vector3.zero;
            multiCanvas.SetActive(true);
            multiCanvas.transform.DOScale(multiCanvasScale, 1f);
        }
        if (other.gameObject.name == "PlayerInfoColider")
        {
            playerInfoCanvas.transform.localScale = Vector3.zero;
            playerInfoCanvas.SetActive(true);
            playerInfoCanvas.transform.DOScale(singleCanvasScale, 1f);
        }
        if (other.gameObject.name == "SinglePlayColider")
        {
            singleCanvas.transform.localScale = Vector3.zero;
            singleCanvas.SetActive(true);
            singleCanvas.transform.DOScale(playerInfoScale, 1f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "MultiPlayColider")
        {
            multiCanvas.transform.DOScale(Vector3.zero, 1f).OnComplete(() => multiCanvas.SetActive(false));
        }
        if (other.gameObject.name == "PlayerInfoColider")
        {
            playerInfoCanvas.transform.DOScale(Vector3.zero, 1f).OnComplete(() => multiCanvas.SetActive(false));
        }
        if (other.gameObject.name == "SinglePlayColider")
        {
            singleCanvas.transform.DOScale(Vector3.zero, 1f).OnComplete(() => multiCanvas.SetActive(false));
        }
    }
}
