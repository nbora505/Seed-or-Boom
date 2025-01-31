using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class TriggerRaycaster : MonoBehaviour
{
    public float rayDistance = 300f;
    //public GameObject rayPrefab; // 레이 시각화 오브젝트
    public Transform rayOrigin; // 레이 시작 위치 
    public OVRInput.Button triggerButton = OVRInput.Button.PrimaryIndexTrigger; 
    private LineRenderer lineRenderer;

    void Start()
    {
        // 라인 렌더러 초기화 (Ray 시각화)
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = Color.red;
    }

    void Update()
    {
        

        // 레이 시각화
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        lineRenderer.SetPosition(0, rayOrigin.position);
        lineRenderer.SetPosition(1, rayOrigin.position + rayOrigin.forward * rayDistance);

        // 트리거 버튼 입력 감지
        if (OVRInput.GetDown(triggerButton))
        {
            ShootRay();
        }
    }

    void ShootRay()
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            Debug.Log("Hit: " + hit.collider.name);

            // UI 버튼 감지
            Button button = hit.collider.GetComponent<Button>();
            if (button != null)
            {
                Debug.Log("UI 버튼 클릭: " + button.name);
                button.onClick.Invoke(); 
                return;
            }

            TMP_InputField inputs = hit.collider.GetComponent<TMP_InputField>();
            if (inputs != null)
            {
                Debug.Log("UI 버튼 클릭: " + inputs.name);
                inputs.onSelect.Invoke(inputs.text);
                return;
            }

            
            ButtonManager buttonManager = hit.collider.GetComponent<ButtonManager>();
            if (buttonManager != null)
            {
                buttonManager.GetLayName(hit.collider.name);
            }

            // 레이 충돌 지점에 효과 추가 사운드나 이펙트>?
            //if (rayPrefab)
            //{
            //    Instantiate(rayPrefab, hit.point, Quaternion.identity);
            //}
        }
    }
}
