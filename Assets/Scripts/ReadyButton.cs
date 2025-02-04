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

    public GameObject[] AIReadyButtons;
    public GameObject[] AIReadyTexts;
    public GameObject[] AIReadyButtonEffects;

    public Button btn;
    public GameManager gameManager;

    private void Start()
    {
        // PhotonView 컴포넌트를 ReadyButton 오브젝트에 부착했는지 확인합니다.
        //PhotonView photonView = GetComponent<PhotonView>();
        //btn = GetComponent<Button>();  // Button 컴포넌트를 가져옵니다.
        //if (btn != null && photonView.IsMine)
        //{
        //    btn.onClick.AddListener(OnReadyButtonClicked);
        //}

        // MultiplayGameManager를 찾습니다.
        //gameManager = FindObjectOfType<MultiplayGameManager>();

        // AI 플레이어인 경우, 준비 처리 후 버튼을 제거합니다.
        //if (playerController != null && (playerController.isAIPlayer || GetComponentInParent<AIPlayer>()?.isAIPlayer == true))
        //{
        //    playerController.isReady = true;
        //    readyText.SetActive(false);
        //    gameObject.SetActive(false);
        //}
    }

    //private void OnDestroy()
    //{
    //    if (btn != null)
    //    {
    //        btn.onClick.RemoveListener(OnReadyButtonClicked);
    //    }
    //}

    public void OnReadyButtonClicked()
    {
        // 로컬 플레이어의 준비 상태를 설정합니다.
        //PhotonNetwork.LocalPlayer.CustomProperties["IsReady"] = true;

        // 모든 클라이언트에 준비 상태를 동기화합니다.
        //gameManager.photonView.RPC("CheckPlayerReady", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);

        // **수정된 부분:** ReadyButton 스크립트에 부착된 자신의 PhotonView를 사용해 RPC를 호출합니다.
        //photonView.RPC("RemoveButtonRPC", RpcTarget.AllBuffered);
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
        //본체 없어지는 이펙트 터뜨리고...
        readyButtonEffect.SetActive(true);

        yield return new WaitForSeconds(2f);
        AIReadyTexts[1].SetActive(false);
        Transform button1 = AIReadyButtons[1].transform.Find("Button2");
        button1.gameObject.SetActive(false);
        AIReadyButtonEffects[1].SetActive(true);

        yield return new WaitForSeconds(1f);
        AIReadyTexts[2].SetActive(false);
        Transform button2 = AIReadyButtons[2].transform.Find("Button2");
        button2.gameObject.SetActive(false);
        AIReadyButtonEffects[2].SetActive(true);

        yield return new WaitForSeconds(2f);
        AIReadyTexts[0].SetActive(false);
        Transform button0 = AIReadyButtons[0].transform.Find("Button2");
        button0.gameObject.SetActive(false);
        AIReadyButtonEffects[0].SetActive(true);

        yield return new WaitForSeconds(0.5f);
        gameManager.isReady = true;
        yield return new WaitForSeconds(0.5f);

        //버튼 본체들 없애기
        AIReadyButtons[0].SetActive(false);
        AIReadyButtons[1].SetActive(false);
        AIReadyButtons[2].SetActive(false);
        gameObject.SetActive(false);
    }

    [PunRPC]
    public void RemoveButtonRPC()
    {
        // 이 RPC 메서드는 ReadyButton 오브젝트를 모든 클라이언트에서 비활성화합니다.
        gameObject.SetActive(false);
    }
}
