using UnityEngine;

public class ReadyButton : MonoBehaviour
{
    public PlayerController playerController; // 이 버튼이 연결된 플레이어

    public void readyBtn()
    {
        if (playerController != null)
        {
            playerController.isReady = true;
            Debug.Log($"{playerController.name}가 준비되었습니다!");
            this.gameObject.SetActive(false); // 버튼 비활성화
        }
        else
        {
            Debug.LogError("PlayerController가 설정되지 않았네?");
        }
    }
}
