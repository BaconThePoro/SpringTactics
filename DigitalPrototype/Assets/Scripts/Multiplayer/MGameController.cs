using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using Vector3 = UnityEngine.Vector3;
using Unity.Netcode;

public class MGameController : NetworkBehaviour
{
    // enum for whose turn it is currently, the players or the enemies.
    public enum turnMode { Player1Turn, Player2Turn };
    private turnMode currTurnMode;

    // enum for what the game is currently doing/displaying, a menu, the map, or a battle. 
    public enum gameMode { MenuMode, MapMode, BattleMode };
    private gameMode currGameMode;

    private TMPro.TMP_Text turnModeTXT = null;
    private Grid currGrid = null;
    private float delay = 0.6f;
    
    // p1 stuff
    private GameObject p1; 
    private GameObject[] p1Units;
    private Character[] p1Stats;
    public Vector3[] p1StartPos;
    public Character.bodyType[] p1bodysList;
    public Character.weaponType[] p1weaponsList;
    private GameObject p1Targeted = null;
    private Character p1TargetedStats = null;
    private int p1gearAmount = 0;

    // p2 stuff
    private GameObject p2;
    private GameObject[] p2Units;
    private Character[] p2Stats;
    public Vector3[] p2StartPos;
    public Character.bodyType[] p2bodysList;
    public Character.weaponType[] p2weaponsList;
    private GameObject p2Targeted = null;
    private Character p2TargetedStats = null;
    private int p2gearAmount = 0;

    
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
    // right panel 
    public GameObject charInfoPanelR = null; 
    private GameObject RmovLeftTXT = null;
    private GameObject RmovLeftNUMObj = null;
    private TMPro.TextMeshProUGUI RcharNameTXT = null;
    private TMPro.TextMeshProUGUI RhpNUM = null;
    private TMPro.TextMeshProUGUI RstrNUM = null;
    private TMPro.TextMeshProUGUI RmagNUM = null;
    private TMPro.TextMeshProUGUI RspdNUM = null;
    private TMPro.TextMeshProUGUI RdefNUM = null;
    private TMPro.TextMeshProUGUI RresNUM = null;
    private TMPro.TextMeshProUGUI RmovNUM = null;
    //private TMPro.TextMeshProUGUI RmovLeftNUM = null;
    
    
    //Movement Area Squares 
    private bool moveActive = false;
    private bool attackActive = false;
    private GameObject moveAreaParent = null;
    private GameObject attackAreaParent = null;
    private GameObject[] moveAreas;
    private GameObject[] attackAreas;
    private MPathfinding pathfinding = null;
    public Tilemap collisionMap = null;
    private Tilemap overlayMap = null;
    public Tile moveTile = null;
    private int clickLock = 0; // 0 = no blocking, 1 = block player 1, 2 = block player 2, 3 = block both
    
    // Battle stuff
    private enum direction { left, right, up, down };
    private Vector3 leftBattlePos1 = new Vector3(-1.5f, 0, -1);
    private Vector3 rightBattlePos1 = new Vector3(1.5f, 0, -1);
    private Vector3 leftBattlePos2 = new Vector3(-2.5f, 0, -1);
    private Vector3 rightBattlePos2 = new Vector3(2.5f, 0, -1);
    private Vector3 camBattlePos = new Vector3(0, 1.25f, -50);
    private float camBattleSize = 2;
    private Quaternion leftBattleQua = new Quaternion();
    private Quaternion rightBattleQua = new Quaternion(0, 180, 0, 1);
    private Vector3 savedPosLeft;
    private Vector3 savedPosRight;
    private Vector3 savedPosCam;
    private Quaternion savedQuaLeft;
    private Quaternion savedQuaRight;
    private float savedCamSize;
    private float inbetweenAttackDelay = 0f;
    private float animationDuration = 0.3f;
    private Vector3 damageTextOffset = new Vector3(0, 0.8f, 0);
    
    
    // UI stuff
    GameObject turnPanel = null;
    public GameObject gearNumPanel = null;
    public GameObject settingsPanel = null; 
    public GameObject Mapmode = null;
    public GameObject Battlemode = null;
    public GameObject mainCameraObj = null;
    private Camera mainCamera = null;
    public GameObject damageTXTPanel = null;
    private TMPro.TextMeshProUGUI damageTXT = null;
    private GameObject gearNumPlus = null;

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
        
        //For Right Char Info Panel
        RcharNameTXT = charInfoPanel.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        RmovLeftTXT = charInfoPanel.transform.GetChild(9).gameObject;
        RhpNUM = charInfoPanel.transform.GetChild(10).GetComponent<TMPro.TextMeshProUGUI>();
        RstrNUM = charInfoPanel.transform.GetChild(11).GetComponent<TMPro.TextMeshProUGUI>();
        RmagNUM = charInfoPanel.transform.GetChild(12).GetComponent<TMPro.TextMeshProUGUI>();
        RspdNUM = charInfoPanel.transform.GetChild(13).GetComponent<TMPro.TextMeshProUGUI>();
        RdefNUM = charInfoPanel.transform.GetChild(14).GetComponent<TMPro.TextMeshProUGUI>();
        RresNUM = charInfoPanel.transform.GetChild(15).GetComponent<TMPro.TextMeshProUGUI>();
        RmovNUM = charInfoPanel.transform.GetChild(16).GetComponent<TMPro.TextMeshProUGUI>();
        RmovLeftNUMObj = charInfoPanel.transform.GetChild(17).gameObject;
        ///RmovLeftNUM = movLeftNUMObj.GetComponent<TMPro.TextMeshProUGUI>();
        
        
        //Finding Parents for movment area display
        overlayMap = GameObject.Find("overlayMap").GetComponent<Tilemap>();
        pathfinding = new MPathfinding(17, 11, collisionMap);
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
            if (NetworkManager.Singleton.LocalClientId == 1 && (clickLock == 1 || clickLock == 3))
                return;
            
            if (NetworkManager.Singleton.LocalClientId == 2 && (clickLock == 2 || clickLock == 3))
                return;
            
            // if over UI ignore
            if (EventSystem.current.IsPointerOverGameObject())
            {
                //Debug.Log("Clicked UI, ignoring");
                return;
            }

            // adjust to z level for units
            Vector3Int mousePos = GetMousePosition();
            mousePos = new Vector3Int(mousePos.x, mousePos.y, 0);

            if (attackActive == true)
            {
                validateAttackServerRpc(mousePos, new ServerRpcParams());
            }

            else if (moveActive == true)
            {
                validatePathServerRpc(mousePos, new ServerRpcParams());
                return;
            }
            
            clickedCharServerRpc(mousePos, new ServerRpcParams());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void validateAttackServerRpc(Vector3Int mousePos, ServerRpcParams serverRpcParams)
    {
        GameObject target = null;
        
        for (int i = 0; i < p1Units.Length; i++)
        {
            if (mousePos == p1Units[i].transform.position && p1Stats[i].getIsDead() == false)
            {
                if (currTurnMode == turnMode.Player1Turn)
                {
                    return;
                }
                else
                {
                     target = p1Units[i];
                }
            }
        }
        for (int i = 0; i < p2Units.Length; i++)
        {
            if (mousePos == p2Units[i].transform.position && p2Stats[i].getIsDead() == false)
            {
                if (currTurnMode == turnMode.Player2Turn)
                {
                    return;
                }
                else
                { 
                     target = p2Units[i];
                }
            }
        }

        if (target == null)
        {
            return;
        }
        
        if ((int)serverRpcParams.Receive.SenderClientId == 1)
        { 
            if (p1TargetedStats.getCanAttack() == true && inAttackRange(mousePos, p1Targeted)) 
            {
                startBattle(p1Targeted.gameObject, target, serverRpcParams);
                return; 
            }

        }
        else if ((int)serverRpcParams.Receive.SenderClientId == 2)
        {
            if (p2TargetedStats.getCanAttack() == true && inAttackRange(mousePos, p2Targeted))
            {
                startBattle(p2Targeted.gameObject, target, serverRpcParams);
                return;
            }
        }
    }

    
    public void startBattle(GameObject ally, GameObject enemy, ServerRpcParams serverRpcParams)
    {
        Debug.Log("Starting battle");
        attackActive = false;
        clickLock = 3;
        passClickLockClientRpc(clickLock);

        // can only fight once per turn, reduce movement to 0
        if (serverRpcParams.Receive.SenderClientId == 1)
        {
            p1TargetedStats.movLeft = 0;
            passMovStatClientRpc(p1Targeted.name, p1TargetedStats.movLeft, p1TargetedStats.baseMOV);
        }
        else if (serverRpcParams.Receive.SenderClientId == 2)
        {
            p2TargetedStats.movLeft = 0;
            passMovStatClientRpc(p2Targeted.name, p2TargetedStats.movLeft, p2TargetedStats.baseMOV);
        }
        
        changeMode(gameMode.BattleMode);
        
        // figure out which way to face (ally on left or right)
        direction battleDirection = facingWhere(ally.transform.position, enemy.transform.position);

        // calculate range of this battle
        int battleRange = 0;
        Vector3 distance = enemy.transform.position - ally.transform.position;
        if ((Mathf.Abs(distance.x) == 1 && Mathf.Abs(distance.y) == 0)
            || (Mathf.Abs(distance.x) == 0 && Mathf.Abs(distance.y) == 1))
        {
            battleRange = 1;
        }
        else if ((Mathf.Abs(distance.x) == 1 && Mathf.Abs(distance.y) == 1)
                 || (Mathf.Abs(distance.x) == 2 && Mathf.Abs(distance.y) == 0)
                 || (Mathf.Abs(distance.x) == 0 && Mathf.Abs(distance.y) == 2))
        {
            battleRange = 2;
        }

        // if facing to the right or down then put ally on the left 
        if (battleDirection == direction.right || battleDirection == direction.down)
            StartCoroutine(continueBattle(ally, enemy, true, battleRange));
        // else put ally on the right
        else
            StartCoroutine(continueBattle(enemy, ally, true, battleRange));
    }

    public IEnumerator continueBattle(GameObject leftChar, GameObject rightChar, bool playerTurn, int battleRange)
    {
        //Debug.Log("starting battle");
        Character leftStats = leftChar.GetComponent<Character>();
        Character rightStats = rightChar.GetComponent<Character>();
        // go to battlemode
        turnPanel.SetActive(false);
        gearNumPanel.SetActive(false);
        settingsPanel.SetActive(false);
        deselectTargetServerRpc(new ServerRpcParams()); // might need to deselect both
        deactivateAllChildren();
        Mapmode.SetActive(false);
        Battlemode.SetActive(true);
        savedCamSize = mainCamera.orthographicSize;
        mainCamera.orthographicSize = camBattleSize;
        charInfoPanel.SetActive(true);
        charInfoPanelR.SetActive(true);
        updateBattleStats(leftStats, rightStats);
        // reactivate participants
        leftChar.SetActive(true);
        rightChar.SetActive(true);
        // save position and rotation for both participants (and camera) before we move them
        savedPosLeft = leftChar.transform.position;
        savedPosRight = rightChar.transform.position;
        savedPosCam = mainCamera.transform.position;
        savedQuaLeft = leftChar.transform.rotation;
        savedQuaRight = rightChar.transform.rotation;
        // move both participants (and camera) to position for battle
        if (battleRange == 1)
        {
            leftChar.transform.position = leftBattlePos1;
            rightChar.transform.position = rightBattlePos1;
        }
        else if (battleRange == 2)
        {
            leftChar.transform.position = leftBattlePos2;
            rightChar.transform.position = rightBattlePos2;
        }
        mainCamera.transform.position = camBattlePos;
        leftChar.transform.rotation = leftBattleQua;
        rightChar.transform.rotation = rightBattleQua;

        // delay for 1.5s so user can see before battle starts
        yield return new WaitForSeconds(inbetweenAttackDelay * 3);

        // Determine who should attack first
        GameObject firstAttacker;
        Character firstStats;
        GameObject secondAttacker;
        Character secondStats;

        // left is faster
        if (leftStats.SPD > rightStats.SPD)
        {
            firstAttacker = leftChar;
            secondAttacker = rightChar;
            firstStats = leftStats;
            secondStats = rightStats;
        }
        // speed tie
        else if (leftStats.SPD == rightStats.SPD)
        {
            // coin flip who goes first
            if (UnityEngine.Random.value >= 0.5)
            {
                firstAttacker = leftChar;
                secondAttacker = rightChar;
                firstStats = leftStats;
                secondStats = rightStats;
            }
            else
            {
                firstAttacker = rightChar;
                secondAttacker = leftChar;
                firstStats = rightStats;
                secondStats = leftStats;
            }
        }
        // right is faster
        else
        {
            firstAttacker = rightChar;
            secondAttacker = leftChar;
            firstStats = rightStats;
            secondStats = leftStats;
        }

        // first attacks
        if (firstStats.getAttackRange() >= battleRange)
        {
            yield return StartCoroutine(LerpPosition(firstAttacker, firstAttacker.transform.position + firstAttacker.transform.right, animationDuration));
            yield return new WaitForSeconds(inbetweenAttackDelay);
            yield return StartCoroutine(updateDamageTXT(secondAttacker, Attack(firstStats, secondStats))); // person taking damage and damage value
            updateBattleStats(leftStats, rightStats);
        }
 
        // delay
        yield return new WaitForSeconds(inbetweenAttackDelay);

        // second attack
        if (secondStats.getAttackRange() >= battleRange)
        {
            yield return StartCoroutine(LerpPosition(secondAttacker, secondAttacker.transform.position + secondAttacker.transform.right, animationDuration));
            yield return new WaitForSeconds(inbetweenAttackDelay);
            yield return StartCoroutine(updateDamageTXT(firstAttacker, Attack(secondStats, firstStats))); // person taking damage and damage value
            updateBattleStats(leftStats, rightStats);
        }

        // delay again for 1.5s so user can see result of battle before leaving battlemode
        yield return new WaitForSeconds(inbetweenAttackDelay * 4);   
        
        // return them to prior positions
        leftChar.transform.position = savedPosLeft;
        rightChar.transform.position = savedPosRight;
        mainCamera.transform.position = savedPosCam;
        mainCamera.orthographicSize = savedCamSize;
        leftChar.transform.rotation = savedQuaLeft;
        rightChar.transform.rotation = savedQuaRight;
        // return to mapmode
        charInfoPanel.SetActive(false);
        charInfoPanelR.SetActive(false);
        turnPanel.SetActive(true);
        gearNumPanel.SetActive(true);
        settingsPanel.SetActive(true);
        activateAllChildren();
        Mapmode.SetActive(true);
        Battlemode.SetActive(false);
        changeMode(gameMode.MapMode); 
        // return to either player or enemy turn
        if (playerTurn == true)
        {

            clickLock = 0;
            leftStats.setAttack(false);
            rightStats.setAttack(false);
        }
        else
        {
            clickLock = 0;
            leftStats.setAttack(false);
            rightStats.setAttack(false);
        }

        // see if player gets some gears for killing something
        if (firstStats.getIsDead() == true && firstStats.transform.IsChildOf(p2.transform) == true
            || secondStats.getIsDead() == true && secondStats.transform.IsChildOf(p2.transform) == true)
        {
            giveGearNum(4, false);
            StartCoroutine(plusAnimation());
        }
        
        // see if player gets some gears for killing something
        if (firstStats.getIsDead() == true && firstStats.transform.IsChildOf(p1.transform) == true
            || secondStats.getIsDead() == true && secondStats.transform.IsChildOf(p1.transform) == true)
        {
            giveGearNum(4, true);
            StartCoroutine(plusAnimation());
        }

        
        
    }
    

    public bool inAttackRange(Vector3Int mousePos, GameObject unit)
    {
        Character unitStats = unit.GetComponent<Character>();

        // 1 Range
        if (unitStats.getAttackRange() == 1)
        {
            Vector3Int distance = mousePos - Vector3Int.FloorToInt(unit.transform.position);
            if ((Mathf.Abs(distance.x) == 1 && distance.y == 0) || (distance.x == 0 && Mathf.Abs(distance.y) == 1))
                return true;
        }
        // 2 Range
        else if (unitStats.getAttackRange() == 2)
        {
            Vector3Int distance = mousePos - Vector3Int.FloorToInt(unit.transform.position);
            if ((Mathf.Abs(distance.x) <= 2 && distance.y == 0) || (distance.x == 0 && Mathf.Abs(distance.y) <= 2) || (Mathf.Abs(distance.x) == 1 && Mathf.Abs(distance.y) == 1)) 
                return true;
        }

        return false; 
    }

    //pass click lock varibale to clients and 
    [ClientRpc]
    public void passClickLockClientRpc(int clocklock)
    {
        clickLock = clocklock;
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
            resetAllMoveServerRpc();
            resetAllAttackServerRpc();
        }
        else
        {
            Debug.Log("!!! Failed to change turnMode from " + prevTurnMode + " to " + newTurn);
        }
        
        passTurnModeClientRpc(currTurnMode);
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
                resetAllMoveServerRpc();
                resetAllAttackServerRpc();
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
                resetAllMoveServerRpc();
                resetAllAttackServerRpc();
            }
        }
        return; 
    }

    public void endTurnButton()
    {
        endTurnServerRpc(new ServerRpcParams());
    }

    
    [ServerRpc(RequireOwnership = false)]
    public void deselectTargetServerRpc(ServerRpcParams serverRpcParams)
    {
        if ((int)serverRpcParams.Receive.SenderClientId == 1)
        {
            if (p1Targeted == null)
                return;

            p1Targeted.transform.GetChild(0).gameObject.SetActive(false);
            moveActive = false;
            attackActive = false;
            if (p1Targeted == null)
                return;      
            p1Targeted = null;
            p1TargetedStats = null;
            //charInfoPanel.gameObject.SetActive(false);
            moveAreas[0].gameObject.SetActive(false);
            moveAreas[1].gameObject.SetActive(false);
            //contextMenu.SetActive(false);
            overlayMap.ClearAllTiles();
            p1DeselectClientRpc();
        }
        else
        {
            if (p2Targeted == null)
                return;

            p2Targeted.transform.GetChild(0).gameObject.SetActive(false);
            moveActive = false;
            attackActive = false;
            if (p2Targeted == null)
                return;
            p2Targeted = null;
            p2TargetedStats = null;
            //charInfoPanel.gameObject.SetActive(false);
            moveAreas[0].gameObject.SetActive(false);
            moveAreas[1].gameObject.SetActive(false);
            //contextMenu.SetActive(false);
            overlayMap.ClearAllTiles();
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
        attackAreas[0].gameObject.SetActive(false);
        attackAreas[1].gameObject.SetActive(false);
        p1Targeted.transform.GetChild(0).gameObject.SetActive(false);
        p1Targeted = null;
        p1TargetedStats = null;
        contextMenu.SetActive(false);
        overlayMap.ClearAllTiles(); // turn off movement highlighting
        moveActive = false;
        attackActive = false;
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
        attackAreas[0].gameObject.SetActive(false);
        attackAreas[1].gameObject.SetActive(false);
        p2Targeted.transform.GetChild(0).gameObject.SetActive(false);
        p2Targeted = null;
        p2TargetedStats = null;
        contextMenu.SetActive(false);
        overlayMap.ClearAllTiles(); // turn off movement highlighting
        moveActive = false;
        attackActive = false;
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
        if (p1Targeted.transform.IsChildOf(p1.transform))
        {
            charInfoPanel.gameObject.SetActive(true);
            P1updateCharInfo();
            //hideArea();
            //showArea(currTargeted);

            // move button
            if (p1TargetedStats.movLeft > 0 && currTurnMode == turnMode.Player1Turn)
            {
                moveButton.interactable = true;
            }
            else
            {
                moveButton.interactable = false;
                moveActive = false;
            }

            // attack button
            if (p1TargetedStats.getCanAttack() == true && currTurnMode == turnMode.Player1Turn)
            {
                attackButton.interactable = true;
            }
            else
            {
                attackButton.interactable = false;
            }

            // upgrade button
            if (currTurnMode == turnMode.Player1Turn)
            {
                upgradeButton.interactable = true;
            }
            else
            {
                upgradeButton.interactable = false;
            }
            

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
        if (p2Targeted.transform.IsChildOf(p2.transform))
        {
            charInfoPanel.gameObject.SetActive(true);
            P2updateCharInfo();
            //hideArea();
            //showArea(currTargeted);

            // move button
            if (p2TargetedStats.movLeft > 0 && currTurnMode == turnMode.Player2Turn)
            {
                moveButton.interactable = true;
            }
            else
            {
                moveButton.interactable = false;
                moveActive = false;
            }

            // attack button
            if (p2TargetedStats.getCanAttack() == true && currTurnMode == turnMode.Player2Turn)
            {
                attackButton.interactable = true;
            }
            else
            {
                attackButton.interactable = false;
            }

            // upgrade button
            if (currTurnMode == turnMode.Player2Turn)
            {
                upgradeButton.interactable = true;
            }
            else
            {
                upgradeButton.interactable = false;
            }
            
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
    
    public void highlightMove()
    {
        //For Player 1 movement 
        if (NetworkManager.Singleton.LocalClientId == 1 && currTurnMode == turnMode.Player1Turn)
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
                        overlayMap.SetTile(newPos, moveTile);
                    }
                }
            }
        }
        
        //For Player 2 movement 
        if (NetworkManager.Singleton.LocalClientId == 2 && currTurnMode == turnMode.Player2Turn)
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
                        overlayMap.SetTile(newPos, moveTile);
                    }
                }
            }
        }
        
        contextMenu.SetActive(false);
    }

    //calls client RPC for move squares 
    public void moveButtonPressed()
    {
        //Debug.Log("move button pressed");
        moveActiveServerRpc(true);
        highlightMove();
    }

    public void attackButtonPressed()
    {
        attackActiveServerRpc(true);
        highlightAttack();
    }

    [ServerRpc(RequireOwnership = false)]
    public void attackActiveServerRpc(bool cond)
    {
        attackActive = cond;
    }

    public void highlightAttack()
    {
        
        attackActive = true;
        if (NetworkManager.Singleton.LocalClientId == 1)
        {
            if (p1TargetedStats.getAttackRange() == 1)
            {
                attackAreas[0].gameObject.SetActive(true);
                attackAreas[0].transform.position = p1Targeted.transform.position;
            }
            else
            {
                attackAreas[0].gameObject.SetActive(true);
                attackAreas[0].transform.position = p1Targeted.transform.position; 
                attackAreas[1].gameObject.SetActive(true);
                attackAreas[1].transform.position = p1Targeted.transform.position; 
            }
            contextMenu.SetActive(false);
        }
        else if(NetworkManager.Singleton.LocalClientId==2)
        {
            if (p2TargetedStats.getAttackRange() == 1)
            {
                attackAreas[0].gameObject.SetActive(true);
                attackAreas[0].transform.position = p2Targeted.transform.position;
            }
            else
            {
                attackAreas[0].gameObject.SetActive(true);
                attackAreas[0].transform.position = p2Targeted.transform.position; 
                attackAreas[1].gameObject.SetActive(true);
                attackAreas[1].transform.position = p2Targeted.transform.position; 
                
            }
            contextMenu.SetActive(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void moveActiveServerRpc(bool cond)
    {
        moveActive = cond;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void validatePathServerRpc(Vector3Int mousePos, ServerRpcParams serverRpcParams)
    {
        if ((int)serverRpcParams.Receive.SenderClientId == 1 && currTurnMode == turnMode.Player1Turn)
        {
            List<PathNode> vectorPath = new List<PathNode>();
            vectorPath = pathfinding.FindPath((int)p1Targeted.transform.position.x, (int)p1Targeted.transform.position.y,
                mousePos.x, mousePos.y, p1TargetedStats.movLeft);

            // valid moveable path
            if (vectorPath != null)
            {
                clickLock = 1;
                passClickLockClientRpc(1);
                StartCoroutine(movePathServer(vectorPath, serverRpcParams));
                pathfinding.resetCollision();
            }
            // invalid path 
            else
            {
                deselectTargetServerRpc(serverRpcParams);
            }

            // unhighlight moveTiles
            clearOverlayClientRpc(false);
            overlayMap.ClearAllTiles();
            moveActive = false;
        }
        if ((int)serverRpcParams.Receive.SenderClientId == 2 && currTurnMode == turnMode.Player2Turn)
        {
            List<PathNode> vectorPath = new List<PathNode>();
            vectorPath = pathfinding.FindPath((int)p2Targeted.transform.position.x, (int)p2Targeted.transform.position.y,
                mousePos.x, mousePos.y, p2TargetedStats.movLeft);

            // valid moveable path
            if (vectorPath != null)
            {
                clickLock = 2; 
                passClickLockClientRpc(2);

                StartCoroutine(movePathServer(vectorPath, serverRpcParams));
                pathfinding.resetCollision();
            }
            // invalid path 
            else
            {
                deselectTargetServerRpc(serverRpcParams);
            }

            // unhighlight moveTiles
            overlayMap.ClearAllTiles();
            clearOverlayClientRpc(true);
            moveActive = false;
        }
    }
    
    
    public IEnumerator movePathServer(List<PathNode> vectorPath, ServerRpcParams serverRpcParams)
    {
        if ((int)serverRpcParams.Receive.SenderClientId == 1 && currTurnMode == turnMode.Player1Turn)
        {
            moveActive = false;
            clickLock = 1; 
            passClickLockClientRpc(1);

            // stop player from ending turn during movement
            changeMode(gameMode.MenuMode);

            for (int i = 1; i < vectorPath.Count; i++)
            {
                p1Targeted.transform.position = new Vector3(vectorPath[i].x, vectorPath[i].y, 0);
                copyMoveClientRpc(p1Targeted.name, new Vector3(vectorPath[i].x, vectorPath[i].y, 0));
                p1TargetedStats.movLeft--;
                passMovStatClientRpc(p1Targeted.name, p1TargetedStats.movLeft,p1TargetedStats.baseMOV);
                //hideArea();
                //showArea(currTargeted);
                P1updateCharInfo();
                
                yield return new WaitForSeconds(delay);
            }

            changeMode(gameMode.MapMode);
            clickLock = 0;
            passClickLockClientRpc(0);

            moveActiveServerRpc(false);
            P1openContextMenuClientRpc(p1Targeted.transform.position);
        }

        if ((int)serverRpcParams.Receive.SenderClientId == 2 && currTurnMode == turnMode.Player2Turn)
        {
            moveActive = false;
            clickLock = 2; 
            passClickLockClientRpc(2);

            // stop player from ending turn during movement
            changeMode(gameMode.MenuMode);

            for (int i = 1; i < vectorPath.Count; i++)
            {
                p2Targeted.transform.position = new Vector3(vectorPath[i].x, vectorPath[i].y, 0);
                copyMoveClientRpc(p2Targeted.name, new Vector3(vectorPath[i].x, vectorPath[i].y, 0));
                p2TargetedStats.movLeft--;
                passMovStatClientRpc(p2Targeted.name, p2TargetedStats.movLeft,p2TargetedStats.baseMOV);
                //hideArea();
                //showArea(currTargeted);
                P2updateCharInfo();
                yield return new WaitForSeconds(delay);
            }

            changeMode(gameMode.MapMode);
            clickLock = 0;
            passClickLockClientRpc(0);

            moveActiveServerRpc(false);
            P2openContextMenuClientRpc(p2Targeted.transform.position);
        }
    }

    [ClientRpc]
    public void copyMoveClientRpc(string name, Vector3 newPosition)
    {
        GameObject targetUnit = GameObject.Find(name);
        targetUnit.transform.position = newPosition;
    }

    // player1 = false, player2 = true
    [ClientRpc]
    public void clearOverlayClientRpc(bool Player)
    {
        // player 1
        if (!Player)
        {
            // ignore if player 2
            if (NetworkManager.Singleton.LocalClientId == 2)
            {
                return;
            }
        }
        // player 2
        else
        {
            if (NetworkManager.Singleton.LocalClientId == 1)
            {
                return;
            }
        }
        overlayMap.ClearAllTiles();
    }

    [ClientRpc]
    public void passTurnModeClientRpc(turnMode newTurnMode)
    {
        currTurnMode = newTurnMode; 
    }

    public bool unitHere(Vector3Int pos)
    {
        for (int i = 0; i < p1.transform.childCount; i++)
        {
            if (p1Units[i].transform.position == pos && p1Stats[i].getIsDead() == false)
            {
                return true;
            }
        }
        for (int i = 0; i < p2.transform.childCount; i++)
        {
            if (p2Units[i].transform.position == pos && p2Stats[i].getIsDead() == false)
            {
                return true;
            }
        }

        return false; 
    }
    [ServerRpc(RequireOwnership = false)]
    public void resetAllMoveServerRpc()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            p1Stats[i].resetMove();
            p2Stats[i].resetMove();
            passMovStatClientRpc(p1Stats[i].name,p1Stats[i].movLeft,p1Stats[i].baseMOV);
            passMovStatClientRpc(p2Stats[i].name,p2Stats[i].movLeft,p2Stats[i].baseMOV);

        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void resetAllAttackServerRpc()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            p1Stats[i].setAttack(true);
            p2Stats[i].setAttack(true);
            passAtkStatClientRpc(p1Stats[i].name,p1Stats[i].getCanAttack());
            passAtkStatClientRpc(p2Stats[i].name,p2Stats[i].getCanAttack());

        }
    }
    
    
    [ClientRpc]
    public void passMovStatClientRpc(string name, int movleft, int mov)
    {
        GameObject targetUnit = GameObject.Find(name);
        Character targetStats = targetUnit.GetComponent<Character>();
        targetStats.movLeft = movleft;
        targetStats.baseMOV = mov;

    }
    
        
    [ClientRpc]
    public void passAtkStatClientRpc(string name, bool canAttack)
    {
        GameObject targetUnit = GameObject.Find(name);
        Character targetStats = targetUnit.GetComponent<Character>();
        targetStats.setAttack(canAttack);

    }
    
    [ClientRpc]
    public void passHpStatClientRpc(string name, int hpleft, int hp)
    {
        GameObject targetUnit = GameObject.Find(name);
        Character targetStats = targetUnit.GetComponent<Character>();
        targetStats.hpLeft = hpleft;
        targetStats.baseHP = hp;

    }

    [ClientRpc]
    public void passAttackActiveClientRpc(bool cond)
    {
        attackActive = cond; 
    }

    direction facingWhere(Vector3 allyPos, Vector3 enemyPos)
    {
        Vector3 difference = enemyPos - allyPos;

        if (difference.x < 0)
            return direction.left;
        else if (difference.x > 0)
            return direction.right;

        if (difference.y < 0)
            return direction.down;
        else if (difference.y > 0)
            return direction.up;

        Debug.Log("facingWhere() is broke, units somehow standing on top of each other");
        return direction.left;
    }

    public void deactivateAllChildren()
    {
        for (int i = 0; i < p1.transform.childCount; i++)
        {
            p1Units[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < p2.transform.childCount; i++)
        {
            p2Units[i].gameObject.SetActive(false);
        }
    }

    public void activateAllChildren()
    {
        for (int i = 0; i < p1.transform.childCount; i++)
        {
            p1Units[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < p2.transform.childCount; i++)
        {
            p2Units[i].gameObject.SetActive(true);
        }
    }
    public void updateBattleStats(Character leftStats, Character rightStats)
    {
        charNameTXT.text = "Name: " + leftStats.charName;
        hpNUM.text = "" + leftStats.hpLeft + " / " + leftStats.HP;
        strNUM.text = "" + leftStats.STR;
        magNUM.text = "" + leftStats.MAG;
        defNUM.text = "" + leftStats.DEF;
        resNUM.text = "" + leftStats.RES;
        spdNUM.text = "" + leftStats.SPD;
        movNUM.text = "" + leftStats.MOV; 
        movLeftTXT.SetActive(false);
        movLeftNUMObj.SetActive(false);

        RcharNameTXT.text = "Name: " + rightStats.charName;
        RhpNUM.text = "" + rightStats.hpLeft + " / " + rightStats.HP;
        RstrNUM.text = "" + rightStats.STR;
        RmagNUM.text = "" + rightStats.MAG;
        RdefNUM.text = "" + rightStats.DEF;
        RresNUM.text = "" + rightStats.RES;
        RspdNUM.text = "" + rightStats.SPD;
        RmovNUM.text = "" + rightStats.MOV;
        RmovLeftTXT.SetActive(false);
        RmovLeftNUMObj.SetActive(false);
    }
    
    public IEnumerator LerpPosition(GameObject theObject, Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector2 startPosition = theObject.transform.position;
        while (time < duration)
        {
            theObject.transform.position = Vector2.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;   
        theObject.transform.position = targetPosition;

        while (time < duration)
        {
            theObject.transform.position = Vector2.Lerp(targetPosition, startPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        theObject.transform.position = startPosition;
    }
    
      
    // false == left hurt, true == right hurt
    public IEnumerator updateDamageTXT(GameObject unitHurt, int damageNum)
    {
        damageTXTPanel.gameObject.SetActive(true);
        damageTXTPanel.transform.position = mainCamera.WorldToScreenPoint(unitHurt.transform.position + damageTextOffset);

        // dead char attack number
        if (damageNum == -999)
            yield return null;
        else if (damageNum == 0)
        {
            damageTXT.color = Color.yellow;
            damageTXT.text = "(" + damageNum + ")";

            yield return new WaitForSeconds(inbetweenAttackDelay * 3);

            damageTXT.text = "";
        }
        else
        {
            damageTXT.color = Color.red;
            damageTXT.text = "(-" + damageNum + ")";

            yield return new WaitForSeconds(inbetweenAttackDelay * 3);

            damageTXT.text = "";
        }

        damageTXTPanel.gameObject.SetActive(false);
    }

    public int Attack(Character attacker, Character damageTaker)
    {
        if (attacker.getIsDead() == true || damageTaker.getIsDead() == true)
            return -999;

        int damageMinusDefense = -1;
        // if attacker has a physical weapon
        if (attacker.GetWeaponType() == Character.weaponType.Sword || attacker.GetWeaponType() == Character.weaponType.Bow 
                                                                   || attacker.GetWeaponType() == Character.weaponType.Axe || attacker.GetWeaponType() == Character.weaponType.Spear)
        {
            damageMinusDefense = attacker.STR - damageTaker.DEF;
            // make sure you cant do negative damage
            if (damageMinusDefense < 0)
                damageMinusDefense = 0;
            
            damageTaker.takeDamage(damageMinusDefense);
        }
        // attacker has magic weapon
        else
        {
            damageMinusDefense = attacker.MAG - damageTaker.RES;
            // make sure you cant do negative damage
            if (damageMinusDefense < 0)
                damageMinusDefense = 0;

            damageTaker.takeDamage(damageMinusDefense);
        }

        // all player characters dead
        if (p1allDead()) 
        {
            //Debug.Log("All allies dead you lose");
            //StartCoroutine(defeat());
        }
        // all enemy characters dead
        else if (p2allDead())
        {
            //Debug.Log("All enemies dead you win");
            //StartCoroutine(victory());
        }

        return damageMinusDefense;
    }
    
    public bool p1allDead()
    {
        for (int i = 0; i < p1Units.Length; i++)
        {
            if (p1Stats[i].getIsDead() == false)
                return false;
        }

        return true;
    }
    
    public bool p2allDead()
    {
        for (int i = 0; i < p2Units.Length; i++)
        {
            if (p2Stats[i].getIsDead() == false)
                return false;
        }

        return true;
    }
    
    public void giveGearNum(int i, bool player)
    {
        if (!player)
        {
            p1gearAmount = p1gearAmount + i;
            updateGearNumPanel(player);
        }
        else
        {
            p2gearAmount = p2gearAmount + i;
            updateGearNumPanel(player);
        }
  
    }
    
    public void updateGearNumPanel(bool player)
    {
        if (!player)
        {
            gearNumPanel.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + getGearNum(player);
        }
        else
        {
            
            gearNumPanel.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + getGearNum(player);
        }
    }

    public int getGearNum(bool player)
    {
        if (!player)
        {
            return p1gearAmount;
        }
        else
        {
            return p2gearAmount;

        }
    }
    
    public IEnumerator plusAnimation()
    {
        float time = 0;
        RawImage rI = gearNumPlus.GetComponent<RawImage>();
        Vector3 originalPos = gearNumPlus.transform.position;

        gearNumPlus.SetActive(true);
        while (time <= 0.2f)
        {
            gearNumPlus.transform.position = gearNumPlus.transform.position + new Vector3(0, 0.25f, 0);
            if (time > 0.1f)
                rI.color = new Color(1, 1, 1, rI.color.a * 0.80f);
            yield return new WaitForSeconds(0.01f);
            time = time + Time.deltaTime;
            //Debug.Log("time: " + time);
        }

        gearNumPlus.transform.position = originalPos;
        rI.color = new Color(1, 1, 1, 1);
        gearNumPlus.SetActive(false);
    }


    
}

