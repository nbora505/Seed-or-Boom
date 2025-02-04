//using System.Collections;
//using Unity.VisualScripting;
//using UnityEngine;

//public class ReadyButton : MonoBehaviour
//{
//    public PlayerController playerController; // �� ��ư�� ����� �÷��̾�
//    public GameObject readyText;
//    public GameObject readyButton;
//    public GameObject readyButtonEffect;

//    private void Start()
//    {
//        //�� ��ư�� ������ AI�÷��̾��
//        if (playerController.isAIPlayer || this.GetComponentInParent<AIPlayer>().isAIPlayer)
//        {
//            playerController.isReady = true; //�ٷ� �غ��Ű��
//            readyText.SetActive(false); //�ؽ�Ʈ ��Ȱ��ȭ
//            this.gameObject.SetActive(false); // ��ư ��Ȱ��ȭ
//        }
//    }

//    public void readyBtn()
//    {
//        if (playerController != null)
//        {
//            playerController.isReady = true;
//            Debug.Log($"{playerController.name}�� �غ�Ǿ����ϴ�!");

//            readyText.SetActive(false); //�ؽ�Ʈ ��Ȱ��ȭ
//            readyButton.transform.Find("Button2").gameObject.SetActive(false); // ��ư �𵨸� ��Ȱ��ȭ

//            StartCoroutine(removeReadyButton());
//        }
//        else
//        {
//            Debug.LogError("PlayerController�� �������� �ʾҳ�?");
//        }
//    }

//    IEnumerator removeReadyButton()
//    {
//        readyButtonEffect.SetActive(true); //����Ʈ ����
//        yield return new WaitForSeconds(1); //1�� ��ٸ� ������

//        this.gameObject.SetActive(false); // ��ư ��Ȱ��ȭ
//    }
//}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
public class ReadyButton : MonoBehaviourPunCallbacks
{
    public PlayerController playerController; // �� ��ư�� ����� �÷��̾�
    public GameObject readyText;
    public GameObject readyButton;
    public GameObject readyButtonEffect;

    public Button btn;
    public MultiplayGameManager gameManager;

    private void Start()
    {
        PhotonView photonView = GetComponent<PhotonView>();
        if (btn != null && photonView.IsMine)
        {
            btn.onClick.AddListener(OnReadyButtonClicked);
        }

        // MultiplayGameManager �ν��Ͻ� ���� (���� �� �� �� �ִٰ� ����)
        gameManager = FindObjectOfType<MultiplayGameManager>();
        // �� ������Ʈ�� Button ������Ʈ�� �ִٸ� �����ͼ� �̺�Ʈ ������ �߰�
        btn = GetComponent<Button>();

        // ���� �� ��ư�� ������ AI��� �ٷ� �غ� ���·� ��ȯ�ϰ� ��ư ��Ȱ��ȭ
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
        // �̺�Ʈ ������ ���� (�޸� ���� ����)
        if (btn != null)
        {
            btn.onClick.RemoveListener(OnReadyButtonClicked);
        }
    }

    //public void OnReadyButtonClicked()
    //{
    //    if (gameManager != null)
    //    {
    //        // ��� Ŭ���̾�Ʈ�� �÷��̾� �غ� ���¸� �˸�
    //        gameManager.OnClickReadyButtonEventListener();

    //        // ��ư ���Ÿ� ��� Ŭ���̾�Ʈ���� �����ϵ��� RPC ȣ��
    //        photonView.RPC("RemoveButtonRPC", RpcTarget.All);
    //    }


    //}
    public void OnReadyButtonClicked()
    {
        // 로컬 플레이어의 커스텀 프로퍼티를 업데이트 (필요한 경우)
        PhotonNetwork.LocalPlayer.CustomProperties["IsReady"] = true;

        // 이제 모든 클라이언트에 RPC를 보내 준비 상태를 업데이트하도록 함
        gameManager.photonView.RPC("CheckPlayerReady", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);

        // 버튼 제거 (모든 클라이언트에서 로컬 UI 업데이트)
        RemoveButtonRPC();
    }

    public void readyBtn()
    {
        if (playerController != null)
        {
            playerController.isReady = true;
            Debug.Log($"{playerController.name}�� �غ�Ǿ����ϴ�!");

            // �غ� �ؽ�Ʈ ��Ȱ��ȭ
            readyText.SetActive(false);

            // ��ư ������ "Button2" ������Ʈ�� �ִٸ� ��Ȱ��ȭ (�𵨸� ���� ��)
            Transform button2 = readyButton.transform.Find("Button2");
            if (button2 != null)
            {
                button2.gameObject.SetActive(false);
            }

            // ����Ʈ�� �����ְ� ���� �ð� �� ��ư ��Ȱ��ȭ ó��
            StartCoroutine(RemoveReadyButton());
        }
        else
        {
            Debug.LogError("PlayerController�� �������� �ʾҽ��ϴ�!");
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



