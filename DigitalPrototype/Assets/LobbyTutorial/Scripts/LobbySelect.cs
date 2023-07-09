using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbySelect : MonoBehaviour
{
    public static LobbySelect Instance { get; private set; }
    
    [SerializeField] private Button quickJoinButton = null;
    [SerializeField] private Button lobbyBrowserButton = null;
    [SerializeField] private Button joinByCodeButton = null;
    [SerializeField] private TMP_InputField codeInputField = null;
    [SerializeField] private TextMeshProUGUI quickJoinText = null;
    private string joinCode = "";

    private void Awake()
    {
        Instance = this;
        
        quickJoinButton.onClick.AddListener(() => 
        {
            LobbyManager.Instance.QuickJoinLobby();

            // successfully quickjoined
            if (LobbyManager.Instance.GetJoinedLobby() != null)
            {
                Hide();
            }
            else
            {
                quickJoinText.text = "No lobbies available :(... Try creating your own!";
            }
        });
        lobbyBrowserButton.onClick.AddListener(() => 
        {
            Hide();
        });
        joinByCodeButton.onClick.AddListener(() => 
        {
            if (joinCode != "")
            {
                LobbyManager.Instance.JoinLobbyByCode(joinCode);
                Hide();
            }
        });
        codeInputField.onValueChanged.AddListener(delegate
        {
            joinCode = codeInputField.text;
        });
    }
    
    public void Hide() {
        gameObject.SetActive(false);
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    // Start is called before the first frame update
    

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
}
