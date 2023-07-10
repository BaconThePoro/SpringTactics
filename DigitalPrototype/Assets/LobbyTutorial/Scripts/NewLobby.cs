using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NewLobby : MonoBehaviour
{
    private GameObject networkManager = null;
    private GameObject lobbyData = null;
    [SerializeField] private GameObject networkPrefab;
    [SerializeField] private GameObject lobbyPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        networkManager = GameObject.Find("NetworkManager");
        lobbyData = GameObject.Find("LobbyData");

        if (networkManager == null)
            Instantiate(networkPrefab);

        if (lobbyData == null)
            Instantiate(lobbyPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
