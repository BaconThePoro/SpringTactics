using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbySelect : MonoBehaviour
{
    [SerializeField] private Button quickJoinButton = null;
    [SerializeField] private Button lobbyBrowserButton = null;
    [SerializeField] private Button joinByCodeButton = null;
    [SerializeField] private TMP_InputField codeInputField = null;
    private string joinCode = "";

    private void Awake()
    {
        quickJoinButton.onClick.AddListener(() => 
        {
            LobbyManager.Instance.QuickJoinLobby();
            Hide();
        });
        lobbyBrowserButton.onClick.AddListener(() => 
        {
            Hide();
        });
        joinByCodeButton.onClick.AddListener(() => 
        {
            if (joinCode != "")
                LobbyManager.Instance.JoinLobbyByCode(joinCode);
        });
        codeInputField.onValueChanged.AddListener(delegate
        {
            joinCode = codeInputField.text;
        });
    }
    
    private void Hide() {
        gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
}
