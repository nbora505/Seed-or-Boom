
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class TriggerRaycaster : ButtonManager
{
    public float rayDistance = 50f;
   // public GameObject rayPrefab; // 레이 시각화를 위한 프리팹

    void Update()
    {
        // 트리거 버튼 입력 감지
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootRay();
        }
    }

    void ShootRay()
    {
        // 레이 발사
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // 레이 캐스팅
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            Debug.Log("Hit: " + hit.collider.name);

            // 버튼인지 확인하고 기능 활성화
            ButtonManager button = FindObjectOfType<ButtonManager>();
            if (button != null)
            {
                button.GetLayName(hit.collider.name);
            }
            // 레이 시각화
           // Instantiate(rayPrefab, hit.point, Quaternion.identity);
        }
    }
}