//using System.Collections;
//using Unity.VisualScripting;
//using UnityEngine;

//public class ReadyButton : MonoBehaviour
//{
//    public PlayerController playerController; // 이 버튼이 연결된 플레이어
//    public GameObject readyText;
//    public GameObject readyButton;
//    public GameObject readyButtonEffect;

//    private void Start()
//    {
//        //이 버튼의 주인이 AI플레이어면
//        if (playerController.isAIPlayer || this.GetComponentInParent<AIPlayer>().isAIPlayer)
//        {
//            playerController.isReady = true; //바로 준비시키고
//            readyText.SetActive(false); //텍스트 비활성화
//            this.gameObject.SetActive(false); // 버튼 비활성화
//        }
//    }

//    public void readyBtn()
//    {
//        if (playerController != null)
//        {
//            playerController.isReady = true;
//            Debug.Log($"{playerController.name}가 준비되었습니다!");

//            readyText.SetActive(false); //텍스트 비활성화
//            readyButton.transform.Find("Button2").gameObject.SetActive(false); // 버튼 모델링 비활성화

//            StartCoroutine(removeReadyButton());
//        }
//        else
//        {
//            Debug.LogError("PlayerController가 설정되지 않았네?");
//        }
//    }

//    IEnumerator removeReadyButton()
//    {
//        readyButtonEffect.SetActive(true); //이펙트 띄우고
//        yield return new WaitForSeconds(1); //1초 기다린 다음에

//        this.gameObject.SetActive(false); // 버튼 비활성화
//    }
//}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ReadyButton : MonoBehaviourPun
{
    public PlayerController playerController; // 이 버튼이 연결된 플레이어
    public GameObject readyText;
    public GameObject readyButton;
    public GameObject readyButtonEffect;

    private Button btn;
    private MultiplayGameManager gameManager;

    private void Start()
    {
        if (photonView.IsMine)
        {
            readyButton.SetActive(true);
            gameManager = FindObjectOfType<MultiplayGameManager>();
            // 이 오브젝트에 Button 컴포넌트가 있다면 가져와서 이벤트 리스너 추가
            btn = readyButton.GetComponent<Button>();

            btn.onClick.AddListener(OnReadyButtonClicked);
        }
        else
        {
            readyButton.SetActive (false);
        }

        // 만약 이 버튼의 주인이 AI라면 바로 준비 상태로 전환하고 버튼 비활성화
        if (playerController != null &&
            (playerController.isAIPlayer || GetComponentInParent<AIPlayer>()?.isAIPlayer == true))
        {
            playerController.isReady = true;
            readyText.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        Debug.Log("Destroy");
        // 이벤트 리스너 제거 (메모리 누수 방지)
        if (btn != null)
        {
            btn.onClick.RemoveListener(OnReadyButtonClicked);
        }
    }

    private void OnReadyButtonClicked()
    {
        var hash = PhotonNetwork.LocalPlayer.CustomProperties;
        hash["IsReady"] = true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        // 기존 ReadyButton 기능 수행 (UI 처리 등)
        readyBtn();

        if (!PhotonNetwork.IsMasterClient) return;
        gameManager.CheckAllPlayerReady();
    }

    public void readyBtn()
    {
        if (playerController != null)
        {
            playerController.isReady = true;
            Debug.Log($"{playerController.name}가 준비되었습니다!");

            // 준비 텍스트 비활성화
            readyText.SetActive(false);

            // 버튼 내부의 "Button2" 오브젝트가 있다면 비활성화 (모델링 제거 등)
            Transform button2 = readyButton.transform.Find("Button2");
            if (button2 != null)
            {
                button2.gameObject.SetActive(false);
            }

            // 이펙트를 보여주고 일정 시간 후 버튼 비활성화 처리
            StartCoroutine(RemoveReadyButton());
        }
        else
        {
            Debug.LogError("PlayerController가 설정되지 않았습니다!");
        }
    }

    IEnumerator RemoveReadyButton()
    {
        readyButtonEffect.SetActive(true);
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }
}



