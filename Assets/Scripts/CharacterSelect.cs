using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;  

public class CharacterSelect : MonoBehaviour
{
    public GameObject[] characterPrefabs;

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void SelectCharacter(int index)
    {
        // 선택한 캐릭터 인덱스 저장
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "CharacterIndex", index }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        Debug.Log($"캐릭터 {index} 선택 완료! CustomProperties 저장됨: {PhotonNetwork.LocalPlayer.CustomProperties["CharacterIndex"]}");
    }
}
