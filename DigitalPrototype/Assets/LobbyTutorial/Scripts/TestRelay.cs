using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Authentication;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.SceneManagement;

public class TestRelay : MonoBehaviour
{
    public static TestRelay Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public async Task<string> CreateRelay() { 
        try {
            SceneManager.LoadScene("MultiplayerScene", LoadSceneMode.Additive);
            
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joincode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joincode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
            return joincode;
        } 
        catch(RelayServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }
    
    public async void joinRelay(string joincode)
    {
        try 
        {
            SceneManager.LoadScene("MultiplayerScene", LoadSceneMode.Additive);
            Debug.Log("join relay " + joincode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joincode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            
            NetworkManager.Singleton.StartClient();
        } 
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }   
}
