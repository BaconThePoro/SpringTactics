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
    
    //context Menu buttons/interaction
    public GameObject contextMenu = null;
    //Above must be attached in editor
    private Button moveButton = null;
    private Button attackButton = null;
    private Button upgradeButton = null;
    private Button inspectButton = null;
    private Button deselectButton = null;
    private Vector3 menuOffset = new Vector3(2.5f, -2f, 0);
    
    //Character Information Panel Info
    public GameObject charInfoPanel = null;
    //Above must be attached in editor
    private GameObject movLeftTXT = null;
    private GameObject movLeftNUMObj = null;
    private TMPro.TextMeshProUGUI charNameTXT = null;
    private TMPro.TextMeshProUGUI hpNUM = null;
    private TMPro.TextMeshProUGUI strNUM = null;
    private TMPro.TextMeshProUGUI magNUM = null;
    private TMPro.TextMeshProUGUI spdNUM = null;
    private TMPro.TextMeshProUGUI defNUM = null;
    private TMPro.TextMeshProUGUI resNUM = null;
    private TMPro.TextMeshProUGUI movNUM = null;
    private TMPro.TextMeshProUGUI movLeftNUM = null;
    
    //Movement Area Squares 
    private bool moveActive = false;
    private bool attackActive = false;
    private GameObject moveAreaParent = null;
    private GameObject attackAreaParent = null;
    private GameObject[] moveAreas;
    private GameObject[] attackAreas;
    public Pathfinding pathfinding = null;
    private Tilemap collisionGrid = null;
    private Tilemap overlayGrid = null;
    private Tile moveTile = null;



    
    void Start()
    {

        //Getting all context menu buttons
        moveButton = contextMenu.transform.GetChild(0).GetComponent<Button>();
        attackButton = contextMenu.transform.GetChild(1).GetComponent<Button>();
        upgradeButton = contextMenu.transform.GetChild(2).GetComponent<Button>();
        inspectButton = contextMenu.transform.GetChild(3).GetComponent<Button>();
        deselectButton = contextMenu.transform.GetChild(4).GetComponent<Button>();
      
        //Char Info gets all information for stats
        charNameTXT = charInfoPanel.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        movLeftTXT = charInfoPanel.transform.GetChild(9).gameObject;
        hpNUM = charInfoPanel.transform.GetChild(10).GetComponent<TMPro.TextMeshProUGUI>();
        strNUM = charInfoPanel.transform.GetChild(11).GetComponent<TMPro.TextMeshProUGUI>();
        magNUM = charInfoPanel.transform.GetChild(12).GetComponent<TMPro.TextMeshProUGUI>();
        spdNUM = charInfoPanel.transform.GetChild(13).GetComponent<TMPro.TextMeshProUGUI>();
        defNUM = charInfoPanel.transform.GetChild(14).GetComponent<TMPro.TextMeshProUGUI>();
        resNUM = charInfoPanel.transform.GetChild(15).GetComponent<TMPro.TextMeshProUGUI>();
        movNUM = charInfoPanel.transform.GetChild(16).GetComponent<TMPro.TextMeshProUGUI>();
        movLeftNUMObj = charInfoPanel.transform.GetChild(17).gameObject;
        movLeftNUM = movLeftNUMObj.GetComponent<TMPro.TextMeshProUGUI>();
        
        //Finding Parents for movment area display
        pathfinding = new Pathfinding(17, 11, collisionGrid);
        collisionGrid = GameObject.Find("collisionMap").GetComponent<Tilemap>();
        overlayGrid = GameObject.Find("overlayMap").GetComponent<Tilemap>();
        moveTile = GameObject.Find("blue").GetComponent<Tile>();
        moveAreaParent = GameObject.Find("moveAreas").gameObject;
        attackAreaParent = GameObject.Find("attackAreas").gameObject;
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
        
        moveAreas = new GameObject[moveAreaParent.transform.childCount];
        i = 0;
        foreach (Transform child in moveAreaParent.transform)
        {
            moveAreas[i] = child.gameObject;
            i += 1;
        }

        attackAreas = new GameObject[attackAreaParent.transform.childCount];
        i = 0;
        foreach (Transform child in attackAreaParent.transform)
        {
            attackAreas[i] = child.gameObject;
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

            // checking if an ally or enemy was clicked
            clickedCharServerRpc(mousePos, new ServerRpcParams());
            
            
            
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
    public void clickedCharServerRpc(Vector3Int mousePos, ServerRpcParams serverRpcParams)
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
                    GameObject target = p1Units[i];
                    deselectTargetServerRpc(serverRpcParams);
                    targetServerRpc(target.transform.name, serverRpcParams);                                   
                    P1openContextMenuClientRpc(mousePos);
                    Debug.Log("Player 1 clicked an Ally");
                    return;
                }
            }
            for (int i = 0; i < p2Units.Length; i++)
            {
                // an opp was clicked
                if (mousePos == p2Units[i].transform.position && p2Stats[i].getIsDead() == false)
                {
                    GameObject target = p2Units[i];
                    deselectTargetServerRpc(serverRpcParams);
                    targetServerRpc(target.transform.name, serverRpcParams);                                   
                    P1openContextMenuClientRpc(mousePos);
                    return;
                }
            }

            // else, clicked nothing
            deselectTargetServerRpc(serverRpcParams);
            contextMenu.SetActive(false);
        }
        // player 2
        else
        {
            for (int i = 0; i < p2Units.Length; i++)
            {
                // an ally was clicked
                if (mousePos == p2Units[i].transform.position && p2Stats[i].getIsDead() == false)
                {                     
                    GameObject target = p2Units[i];
                    deselectTargetServerRpc(serverRpcParams);
                    targetServerRpc(target.transform.name, serverRpcParams);                                   
                    P2openContextMenuClientRpc(mousePos);
                    Debug.Log("Player 2 clicked an Ally");
                    return;
                }
            }
            
            for (int i = 0; i < p1Units.Length; i++)
            {
                // an enemy was clicked
                if (mousePos == p1Units[i].transform.position && p1Stats[i].getIsDead() == false)
                {              
                    GameObject target = p1Units[i];
                    deselectTargetServerRpc(serverRpcParams);
                    targetServerRpc(target.transform.name, serverRpcParams);                                   
                    P2openContextMenuClientRpc(mousePos);
                    return;
                }
            }

            // else, clicked nothing
            deselectTargetServerRpc(serverRpcParams);
            contextMenu.SetActive(false);
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
    void targetServerRpc(String targetName, ServerRpcParams serverRpcParams)
    {
        GameObject target = GameObject.Find(targetName);
        if ((int)serverRpcParams.Receive.SenderClientId == 1)
        {
            //moveActive = false;
            //attackActive = false;
            if (target.GetComponent<Character>().getIsDead() == true)
                return;
            p1Targeted = target;
            p1TargetedStats = p1Targeted.GetComponent<Character>();
            p1Targeted.transform.GetChild(0).gameObject.SetActive(true);
            //gameController.updateUpgradeMenu(currTargeted);
            //hideArea();
            p1SelectClientRpc(p1Targeted.transform.name);
        }
        else
        {
            //moveActive = false;
            //attackActive = false;
            if (target.GetComponent<Character>().getIsDead() == true)
                return;
            p2Targeted = target;
            p2TargetedStats = p2Targeted.GetComponent<Character>();
            p2Targeted.transform.GetChild(0).gameObject.SetActive(true);
            //gameController.updateUpgradeMenu(currTargeted);
            //hideArea();
            p2SelectClientRpc(p2Targeted.transform.name);
        }
    }

    [ClientRpc]
    void p1SelectClientRpc(String name)
    {
        //if player 2 ignore 
        if (NetworkManager.Singleton.LocalClientId == 2)
        {
            return;
        }
        
        p1Targeted = GameObject.Find(name).gameObject;
        p1TargetedStats = p1Targeted.GetComponent<Character>();
        p1Targeted.transform.GetChild(0).gameObject.SetActive(true);
        
    }

    [ClientRpc]
    void p1DeselectClientRpc()
    {
        //if player 2 ignore 
        if (NetworkManager.Singleton.LocalClientId == 2)
        {
            return;
        }
        
        p1Targeted.transform.GetChild(0).gameObject.SetActive(false);
        p1Targeted = null;
        p1TargetedStats = null;
    }

    [ClientRpc]
    void p2SelectClientRpc(String name)
    {
        
        //if player 1 ignore 
        if (NetworkManager.Singleton.LocalClientId == 1)
        {
            return;
        }
        
        p2Targeted = GameObject.Find(name).gameObject;
        p2TargetedStats = p2Targeted.GetComponent<Character>();
        p2Targeted.transform.GetChild(0).gameObject.SetActive(true);
    }

    [ClientRpc]
    void p2DeselectClientRpc()
    {
        //if player 1 ignore 
        if (NetworkManager.Singleton.LocalClientId == 1)
        {
            return;
        }
        
        p2Targeted.transform.GetChild(0).gameObject.SetActive(false);
        p2Targeted = null;
        p2TargetedStats = null;
    }

    [ClientRpc]
    public void P1openContextMenuClientRpc(Vector3 mousePos)
    {
        //Ignore for player 2. 
        if (NetworkManager.Singleton.LocalClientId == 2)
        {
            return;
        }
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
        if (p1TargetedStats.getIsEnemy() == false)
        {
            charInfoPanel.gameObject.SetActive(true);
            P1updateCharInfo();
            //hideArea();
            //showArea(currTargeted);

            // move button
            if (p1TargetedStats.movLeft > 0)
            {
                moveButton.interactable = true;
            }
            else
            {
                moveButton.interactable = false;
                moveActive = false;
            }

            // attack button
            if (p1TargetedStats.getCanAttack() == true)
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
            P1updateCharInfo();
            //hideArea();
            //showArea(currTargeted);

            moveButton.interactable = false;
            attackButton.interactable = false;
            upgradeButton.interactable = false;
            inspectButton.interactable = true;
            deselectButton.interactable = true; 
        }
    }
    
    [ClientRpc]
    public void P2openContextMenuClientRpc(Vector3 mousePos)
    {
        //Ignore for player 1 
        if (NetworkManager.Singleton.LocalClientId == 1)
        {
            return;
        }
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
        if (p2TargetedStats.getIsEnemy() == false)
        {
            charInfoPanel.gameObject.SetActive(true);
            P2updateCharInfo();
            //hideArea();
            //showArea(currTargeted);

            // move button
            if (p2TargetedStats.movLeft > 0)
            {
                moveButton.interactable = true;
            }
            else
            {
                moveButton.interactable = false;
                moveActive = false;
            }

            // attack button
            if (p2TargetedStats.getCanAttack() == true)
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
            P2updateCharInfo();
            //hideArea();
            //showArea(currTargeted);

            moveButton.interactable = false;
            attackButton.interactable = false;
            upgradeButton.interactable = false;
            inspectButton.interactable = true;
            deselectButton.interactable = true; 
        }
    }
    
    
     public void P1updateCharInfo()
    {
        charNameTXT.text = "Name: " + p1TargetedStats.charName;
        hpNUM.text = "" + p1TargetedStats.hpLeft + " / " + p1TargetedStats.HP;
        strNUM.text = "" + p1TargetedStats.STR;
        magNUM.text = "" + p1TargetedStats.MAG;
        defNUM.text = "" + p1TargetedStats.DEF;
        resNUM.text = "" + p1TargetedStats.RES;
        spdNUM.text = "" + p1TargetedStats.SPD;

        if (!p1Targeted.transform.IsChildOf(p2.transform))
        {
            movNUM.text = "" + p1TargetedStats.MOV;
            movLeftNUM.text = "" + p1TargetedStats.movLeft;
            movLeftTXT.SetActive(true);
            movLeftNUMObj.SetActive(true);
        }
        else
        {
            movNUM.text = "" + p1TargetedStats.MOV;
            movLeftTXT.SetActive(false);
            movLeftNUMObj.SetActive(false);
        }
    }
    
    public void P2updateCharInfo()
    {
        charNameTXT.text = "Name: " + p2TargetedStats.charName;
        hpNUM.text = "" + p2TargetedStats.hpLeft + " / " + p2TargetedStats.HP;
        strNUM.text = "" + p2TargetedStats.STR;
        magNUM.text = "" + p2TargetedStats.MAG;
        defNUM.text = "" + p2TargetedStats.DEF;
        resNUM.text = "" + p2TargetedStats.RES;
        spdNUM.text = "" + p2TargetedStats.SPD;

        if (!p2Targeted.transform.IsChildOf(p2.transform))
        {
            movNUM.text = "" + p2TargetedStats.MOV;
            movLeftNUM.text = "" + p2TargetedStats.movLeft;
            movLeftTXT.SetActive(true);
            movLeftNUMObj.SetActive(true);
        }
        else
        {
            movNUM.text = "" + p2TargetedStats.MOV;
            movLeftTXT.SetActive(false);
            movLeftNUMObj.SetActive(false);
        }
    }
    
    [ClientRpc]
    public void highlightMoveClientRpc()
    {
        //For Player 1 movment 
        if (NetworkManager.Singleton.LocalClientId == 1)
        {
            moveActive = true;
            attackActive = false;
            List<PathNode> vectorPath = new List<PathNode>();
            Vector3Int currPos = Vector3Int.FloorToInt(p1Targeted.transform.position);
            int currMov = p1TargetedStats.movLeft;
            // highlighting moveable tiles
            for (int i = -currMov; i <= currMov; i++)
            {
                for (int j = -currMov; j <= currMov; j++)
                {
                    vectorPath = pathfinding.FindPath(currPos.x, currPos.y, 
                        currPos.x + i, currPos.y + j, currMov);
                    // if path exists
                    if (vectorPath != null)
                    {
                        Vector3Int newPos = new Vector3Int(currPos.x + i, currPos.y + j, 0);
                        overlayGrid.SetTile(newPos, moveTile);
                    }
                }
            }
        }
        
        //For Player 2 movment 
        if (NetworkManager.Singleton.LocalClientId == 2)
        {
            moveActive = true;
            attackActive = false;
            List<PathNode> vectorPath = new List<PathNode>();
            Vector3Int currPos = Vector3Int.FloorToInt(p2Targeted.transform.position);
            int currMov = p2TargetedStats.movLeft;
            // highlighting moveable tiles
            for (int i = -currMov; i <= currMov; i++)
            {
                for (int j = -currMov; j <= currMov; j++)
                {
                    vectorPath = pathfinding.FindPath(currPos.x, currPos.y, 
                        currPos.x + i, currPos.y + j, currMov);
                    // if path exists
                    if (vectorPath != null)
                    {
                        Vector3Int newPos = new Vector3Int(currPos.x + i, currPos.y + j, 0);
                        overlayGrid.SetTile(newPos, moveTile);
                    }
                }
            }
        }
        
        contextMenu.SetActive(false);
    }

    //calls client RPC for move squares 
    public void moveButtonPressed()
    {
        highlightMoveClientRpc();
        
    }

}
