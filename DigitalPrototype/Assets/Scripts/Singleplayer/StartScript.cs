using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartScript : MonoBehaviour
{
    public GameObject relayObj = null;
    private Relay relayScript = null; 
    public GameObject textBox = null;
    
    // Start is called before the first frame update
    void Start()
    {
        if (relayObj != null)
            relayScript = relayObj.GetComponent<Relay>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void loadOnClick()
    {
        SceneManager.LoadScene("Level1");
    }

    public void loadTutorial(){
        SceneManager.LoadScene("TutorialScene", LoadSceneMode.Additive);
    }

    public void loadCredits(){
        SceneManager.LoadScene("Credits");
    }

    public void leaveCredits()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void exitButton()
    {
        Application.Quit();
    }

    public void returnLobby()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void host()
    {
        SceneManager.LoadScene("MultiplayerScene");
        relayScript.CreateRelay();
    }
    
    public void joinButton()
    {
        SceneManager.LoadScene("MultiplayerScene");
        if (textBox != null)
            relayScript.JoinRelay(textBox.GetComponent<TMP_InputField>().text);
    }

    public void multiplayer()
    {
        SceneManager.LoadScene("Lobby");
    }
}
