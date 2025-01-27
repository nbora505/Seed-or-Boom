using UnityEngine;

public class ReadyButton : MonoBehaviour
{
    public PlayerController playerController; // 이 버튼이 연결된 플레이어
    public GameObject readyText;

    private void Start()
    {
        //이 버튼의 주인이 AI플레이어면
        if (playerController.isAIPlayer || this.GetComponentInParent<AIPlayer>().isAIPlayer)
        {
            playerController.isReady = true; //바로 준비시키고
            readyText.SetActive(false); //텍스트 비활성화
            this.gameObject.SetActive(false); // 버튼 비활성화
        }
    }

    public void readyBtn()
    {
        if (playerController != null)
        {
            playerController.isReady = true;
            Debug.Log($"{playerController.name}가 준비되었습니다!");
            readyText.SetActive(false); //텍스트 비활성화
            this.gameObject.SetActive(false); // 버튼 비활성화
        }
        else
        {
            Debug.LogError("PlayerController가 설정되지 않았네?");
        }
    }
}
