using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ReadyButton : MonoBehaviourPunCallbacks
{
    public PlayerController playerController;
    public GameObject readyText;
    public GameObject readyButton;
    public GameObject readyButtonEffect;

    public Button btn;
    public MultiplayGameManager gameManager;

    private void Start()
    {
        PhotonView photonView = GetComponent<PhotonView>();
        btn = GetComponent<Button>();  // Button 컴포넌트를 가져옵니다.
        if (btn != null && photonView.IsMine)
        {
            btn.onClick.AddListener(OnReadyButtonClicked);
        }

        // MultiplayGameManager를 찾습니다.
        gameManager = FindObjectOfType<MultiplayGameManager>();

        // AI 플레이어인 경우
        if (playerController != null && (playerController.isAIPlayer || GetComponentInParent<AIPlayer>()?.isAIPlayer == true))
        {
            playerController.isReady = true;
            readyText.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (btn != null)
        {
            btn.onClick.RemoveListener(OnReadyButtonClicked);
        }
    }

    public void OnReadyButtonClicked()
    {
        // 로컬 플레이어의 준비 상태를 설정하고 모든 클라이언트에 동기화합니다.
        PhotonNetwork.LocalPlayer.CustomProperties["IsReady"] = true;
        gameManager.photonView.RPC("CheckPlayerReady", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);

        // 버튼 제거 RPC 호출 (모든 클라이언트에 적용)
        gameManager.photonView.RPC("RemoveButtonRPC", RpcTarget.AllBuffered);
    }

    public void readyBtn()
    {
        if (playerController != null)
        {
            playerController.isReady = true;
            Debug.Log($"{playerController.name} 준비 완료!");

            readyText.SetActive(false);
            Transform button2 = readyButton.transform.Find("Button2");
            if (button2 != null)
            {
                button2.gameObject.SetActive(false);
            }
            StartCoroutine(RemoveReadyButton());
        }
        else
        {
            Debug.LogError("PlayerController가 할당되지 않았습니다!");
        }
    }

    IEnumerator RemoveReadyButton()
    {
        readyButtonEffect.SetActive(true);
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }

    [PunRPC]
    public void RemoveButtonRPC()
    {
        gameObject.SetActive(false);
    }
}
