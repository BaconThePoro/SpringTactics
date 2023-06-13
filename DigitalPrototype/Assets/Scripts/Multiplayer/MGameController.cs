using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Tilemaps;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MGameController : NetworkBehaviour
{
    // enum for whose turn it is currently, the players or the enemies.
    public enum turnMode { Player1Turn, Player2Turn };
    private turnMode currTurnMode;

    // enum for what the game is currently doing/displayhing, a menu, the map, or a battle. 
    public enum gameMode { MenuMode, MapMode, BattleMode };
    private gameMode currGameMode;

    private TMPro.TMP_Text turnModeTXT = null;
    private Grid currGrid = null;

    // p1 stuff
    private GameObject p1; 
    private GameObject[] p1Units;
    private Character[] p1Stats;
    public Vector3[] p1StartPos;
    public Character.bodyType[] p1bodysList;
    public Character.weaponType[] p1weaponsList;
    private GameObject p1Targeted = null;
    private Character p1TargetedStats = null;
    // p2 stuff
    private GameObject p2;
    private GameObject[] p2Units;
    private Character[] p2Stats;
    public Vector3[] p2StartPos;
    public Character.bodyType[] p2bodysList;
    public Character.weaponType[] p2weaponsList;
    private GameObject p2Targeted = null;
    private Character p2TargetedStats = null;

    void Start()
    {
        turnModeTXT = GameObject.Find("currentTurnTXT").GetComponent<TMPro.TextMeshProUGUI>();
        currGrid = GameObject.Find("Grid").gameObject.GetComponent<Grid>();

        // initialize p1
        p1 = transform.GetChild(0).gameObject;
        p1Units = new GameObject[p1.transform.childCount];
        p1Stats = new Character[p1.transform.childCount];
        // initialize p2
        p2 = transform.GetChild(1).gameObject;
        p2Units = new GameObject[p2.transform.childCount];
        p2Stats = new Character[p2.transform.childCount];

        int i = 0;
        foreach (Transform child in p1.transform)
        {
            p1Units[i] = child.gameObject;
            p1Stats[i] = p1Units[i].GetComponent<Character>();
            p1Units[i].transform.position = p1StartPos[i];
            p1Stats[i].changeBody(p1bodysList[i]);
            p1Stats[i].changeWeapon(p1weaponsList[i]);
            p1Stats[i].playerNum = 1;
            p1Stats[i].setBodyVisuals();

            i += 1;
        }

        i = 0;
        foreach (Transform child in p2.transform)
        {
            p2Units[i] = child.gameObject;
            p2Stats[i] = p2Units[i].GetComponent<Character>();
            p2Units[i].transform.position = p2StartPos[i];
            p2Stats[i].changeBody(p2bodysList[i]);
            p2Stats[i].changeWeapon(p2weaponsList[i]);
            p2Stats[i].playerNum = 2;
            p2Stats[i].setBodyVisuals();

            i += 1;
        }

        changeTurn(turnMode.Player1Turn);
        changeMode(gameMode.MapMode);
        updateTurnText();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            // if over UI ignore
            if (EventSystem.current.IsPointerOverGameObject())
            {
                //Debug.Log("Clicked UI, ignoring");
                return;
            }

            // adjust to z level for units
            Vector3Int mousePos = GetMousePosition();
            mousePos = new Vector3Int(mousePos.x, mousePos.y, 0);

            // checking if an ally was clicked
            clickedAllyServerRpc(mousePos, new ServerRpcParams());
        }
    }

    public void changeTurn(turnMode newTurn)
    {
        turnMode prevTurnMode = currTurnMode;
        currTurnMode = newTurn;

        // make sure to update turn text as well
        updateTurnText();

        if (currTurnMode == newTurn)
        {
            Debug.Log("turnMode changed from " + prevTurnMode + " to " + newTurn);

            // if player1 turn
            if (currTurnMode == turnMode.Player1Turn)
            {

            }
            // if player2 turn
            else
            {

            }
        }
        else
        {
            Debug.Log("!!! Failed to change turnMode from " + prevTurnMode + " to " + newTurn);
        }
    }

    public void changeMode(gameMode newMode)
    {
        gameMode prevGameMode = currGameMode;
        currGameMode = newMode;

        if (currGameMode == newMode)
        {
            Debug.Log("gameMode changed from " + prevGameMode + " to " + newMode);
        }
        else
        {
            Debug.Log("!!! Failed to change gameMode from " + prevGameMode + " to " + newMode);
        }
    }

    public void updateTurnText()
    {
        if (currTurnMode == turnMode.Player1Turn)
        {
            turnModeTXT.text = "Player 1 Turn";
            updateTurnTextClientRpc("Player 1 Turn");
        }
        else
        {
            turnModeTXT.text = "Player 2 Turn";
            updateTurnTextClientRpc("Player 2 Turn");
        }
    }

    [ClientRpc]
    public void updateTurnTextClientRpc(string s)
    {
        turnModeTXT.text = s;
    }

    Vector3Int GetMousePosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return currGrid.WorldToCell(mouseWorldPos);
    }

    [ServerRpc(RequireOwnership = false)]
    public void clickedAllyServerRpc(Vector3Int mousePos, ServerRpcParams serverRpcParams)
    {
        Debug.Log("Request from Player " + (serverRpcParams.Receive.SenderClientId));

        // if player 1
        if ((int)serverRpcParams.Receive.SenderClientId == 1)
        {
            for (int i = 0; i < p1Units.Length; i++)
            {
                // an ally was clicked
                if (mousePos == p1Units[i].transform.position && p1Stats[i].getIsDead() == false)
                {
                    deselectTargetServerRpc(serverRpcParams);
                    targetAllyServerRpc(i, serverRpcParams);                                   
                    //openContextMenu(mousePos);
                    Debug.Log("Player 1 clicked an Ally");
                    return;
                }
            }

            // else, clicked nothing
            deselectTargetServerRpc(serverRpcParams);
            //contextMenu.SetActive(false);
        }
        // player 2
        else
        {
            for (int i = 0; i < p2Units.Length; i++)
            {
                // an ally was clicked
                if (mousePos == p2Units[i].transform.position && p2Stats[i].getIsDead() == false)
                {                     
                    deselectTargetServerRpc(serverRpcParams);
                    targetAllyServerRpc(i, serverRpcParams);
                    //openContextMenu(mousePos);
                    Debug.Log("Player 2 clicked an Ally");
                    return;
                }
            }

            // else, clicked nothing
            deselectTargetServerRpc(serverRpcParams);
            //contextMenu.SetActive(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void endTurnServerRpc(ServerRpcParams serverRpcParams)
    {
        // if not in mapmode dont do anything
        if (currGameMode != gameMode.MapMode)
            return;

        // player 1 turn
        if (currTurnMode == turnMode.Player1Turn)
        {
            // make sure player 1 sent this request
            if ((int)serverRpcParams.Receive.SenderClientId != 1)
            {
                Debug.Log("Player 2 tried to end Player 1s turn");
            }
            else
            {
                changeTurn(turnMode.Player2Turn);
                resetP2Attack();
            }
        }
        // player 2 turn
        if (currTurnMode == turnMode.Player2Turn)
        {
            // make sure player 2 sent this request
            if ((int)serverRpcParams.Receive.SenderClientId != 2)
            {
                Debug.Log("Player 1 tried to end Player 2s turn");
            }
            else
            {
                changeTurn(turnMode.Player1Turn);
                resetP1Attack();
            }
        }
        return; 
    }

    public void endTurnButton()
    {
        endTurnServerRpc(new ServerRpcParams());
    }

    public void resetP1Move()
    {
        for (int i = 0; i < p1.transform.childCount; i++)
        {
            p1Stats[i].resetMove();
        }
    }

    public void resetP1Attack()
    {
        for (int i = 0; i < p1.transform.childCount; i++)
        {
            p1Stats[i].setAttack(true);
        }
    }

    public void resetP2Move()
    {
        for (int i = 0; i < p2.transform.childCount; i++)
        {
            p2Stats[i].resetMove();
        }
    }

    public void resetP2Attack()
    {
        for (int i = 0; i < p2.transform.childCount; i++)
        {
            p2Stats[i].setAttack(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void deselectTargetServerRpc(ServerRpcParams serverRpcParams)
    {
        if ((int)serverRpcParams.Receive.SenderClientId == 1)
        {
            if (p1Targeted == null)
                return;

            p1Targeted.transform.GetChild(0).gameObject.SetActive(false);
            //moveActive = false;
            //attackActive = false;
            if (p1Targeted == null)
                return;      
            p1Targeted = null;
            p1TargetedStats = null;
            //charInfoPanel.gameObject.SetActive(false);
            //hideArea();
            //contextMenu.SetActive(false);
            //overlapGrid.ClearAllTiles();
            p1DeselectClientRpc();
        }
        else
        {
            if (p2Targeted == null)
                return;

            p2Targeted.transform.GetChild(0).gameObject.SetActive(false);
            //moveActive = false;
            //attackActive = false;
            if (p2Targeted == null)
                return;
            p2Targeted = null;
            p2TargetedStats = null;
            //charInfoPanel.gameObject.SetActive(false);
            //hideArea();
            //contextMenu.SetActive(false);
            //overlapGrid.ClearAllTiles();
            p2DeselectClientRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void targetAllyServerRpc(int i, ServerRpcParams serverRpcParams)
    {
        if ((int)serverRpcParams.Receive.SenderClientId == 1)
        {
            //moveActive = false;
            //attackActive = false;
            if (p1Stats[i].getIsDead() == true)
                return;
            p1Targeted = p1Units[i];
            p1TargetedStats = p1Targeted.GetComponent<Character>();
            p1Targeted.transform.GetChild(0).gameObject.SetActive(true);
            //gameController.updateUpgradeMenu(currTargeted);
            //hideArea();
            p1SelectClientRpc(i);
        }
        else
        {
            //moveActive = false;
            //attackActive = false;
            if (p2Stats[i].getIsDead() == true)
                return;
            p2Targeted = p2Units[i];
            p2TargetedStats = p2Targeted.GetComponent<Character>();
            p2Targeted.transform.GetChild(0).gameObject.SetActive(true);
            //gameController.updateUpgradeMenu(currTargeted);
            //hideArea();
            p2SelectClientRpc(i);
        }
    }

    [ClientRpc]
    void p1SelectClientRpc(int i)
    {
        p1Targeted = p1Units[i];
        p1TargetedStats = p1Targeted.GetComponent<Character>();
        p1Targeted.transform.GetChild(0).gameObject.SetActive(true);
    }

    [ClientRpc]
    void p1DeselectClientRpc()
    {
        p1Targeted.transform.GetChild(0).gameObject.SetActive(false);
        p1Targeted = null;
        p1TargetedStats = null;
    }

    [ClientRpc]
    void p2SelectClientRpc(int i)
    {
        p2Targeted = p2Units[i];
        p2TargetedStats = p2Targeted.GetComponent<Character>();
        p2Targeted.transform.GetChild(0).gameObject.SetActive(true);
    }

    [ClientRpc]
    void p2DeselectClientRpc()
    {
        p2Targeted.transform.GetChild(0).gameObject.SetActive(false);
        p2Targeted = null;
        p2TargetedStats = null;
    }


    /*public void openContextMenu(Vector3 mousePos)
    {
        contextMenu.SetActive(true);

        menuOffset = new Vector3(Screen.width*0.11f, -Screen.height*0.16f, 0);
        Vector3 menuPos = Camera.main.WorldToScreenPoint(mousePos);
        menuPos = menuPos + menuOffset;

        // right % of screen
        float widthPercent = 0.9f;
        if (menuPos.x > Screen.width * widthPercent)
        {          
            float widthP = menuPos.x / Screen.width; // what % of screen are we at
            float inverseW = 1 - widthP;

            Vector3 newOffset = new Vector3((Screen.width * 0.25f) * widthP, 0, 0);
            menuPos = menuPos - newOffset;
        }
        // bottom % of screen
        float heightPercent = 0.25f;
        if (menuPos.y < Screen.height * heightPercent)
        {  
            float heightP = menuPos.y / Screen.height; // what % of screen are we at
            float inverseH = 1 - heightP;

            Vector3 newOffset = new Vector3(0, (Screen.height * heightPercent) * inverseH, 0);
            menuPos = menuPos + newOffset;
        }

        contextMenu.transform.position = menuPos;

        // if ally
        if (currTargetedStats.getIsEnemy() == false)
        {
            charInfoPanel.gameObject.SetActive(true);
            updateCharInfo();
            //hideArea();
            //showArea(currTargeted);

            // move button
            if (currTargetedStats.movLeft > 0)
            {
                moveButton.interactable = true;
            }
            else
            {
                moveButton.interactable = false;
                moveActive = false;
            }

            // attack button
            if (currTargetedStats.getCanAttack() == true)
            {
                attackButton.interactable = true;
            }
            else
            {
                attackButton.interactable = false;
            }

            // upgrade button
            upgradeButton.interactable = true;

            // inspect button
            inspectButton.interactable = true;

            // deselect button
            deselectButton.interactable = true;
        }

        // if enemy
        else
        {
            charInfoPanel.gameObject.SetActive(true);
            updateCharInfo();
            //hideArea();
            //showArea(currTargeted);

            moveButton.interactable = false;
            attackButton.interactable = false;
            upgradeButton.interactable = false;
            inspectButton.interactable = true;
            deselectButton.interactable = true; 
        }
    }*/
}
