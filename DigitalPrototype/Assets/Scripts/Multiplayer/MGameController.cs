using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using Vector3 = UnityEngine.Vector3;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MGameController : NetworkBehaviour
{
    // enum for whose turn it is currently, the players or the enemies.
    public enum turnMode { Player1Turn, Player2Turn };
    private turnMode currTurnMode;

    // enum for what the game is currently doing/displaying, a menu, the map, or a battle. 
    public enum gameMode { MenuMode, MapMode, BattleMode };
    private gameMode currGameMode;

    private TMPro.TMP_Text turnModeTXT = null;
    private GameObject gridParent = null;
    private Grid currGrid = null;
    private float delay = 0.6f;
    
    // p1 stuff
    private int player1 = 0;
    private GameObject p1; 
    private GameObject[] p1Units;
    private Character[] p1Stats;
    public Vector3[] p1StartPos;
    public Character.bodyType[] p1bodysList;
    public Character.weaponType[] p1weaponsList;
    private GameObject p1Targeted = null;
    private Character p1TargetedStats = null;
    private int p1GearAmount = 0;

    // p2 stuff
    private int player2 = 1; 
    private GameObject p2;
    private GameObject[] p2Units;
    private Character[] p2Stats;
    public Vector3[] p2StartPos;
    public Character.bodyType[] p2bodysList;
    public Character.weaponType[] p2weaponsList;
    private GameObject p2Targeted = null;
    private Character p2TargetedStats = null;
    private int p2GearAmount = 0;

    //Map Features
    public Vector3[] featuresStartPos;
    private GameObject[] featureUnits;
    private mapFeatures[] featuresFeatures; 
    private GameObject mapFeatures;
    public mapFeatures.featureType[] featureTypeList;
    private int crateGearNum = 12;


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
    private GameObject LmovLeftTXT = null;
    private GameObject LmovLeftNUMObj = null;
    private TMPro.TextMeshProUGUI LcharNameTXT = null;
    private TMPro.TextMeshProUGUI LhpNUM = null;
    private TMPro.TextMeshProUGUI LstrNUM = null;
    private TMPro.TextMeshProUGUI LmagNUM = null;
    private TMPro.TextMeshProUGUI LspdNUM = null;
    private TMPro.TextMeshProUGUI LdefNUM = null;
    private TMPro.TextMeshProUGUI LresNUM = null;
    private TMPro.TextMeshProUGUI LmovNUM = null;
    private TMPro.TextMeshProUGUI LmovLeftNUM = null;
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
    private Tilemap collisionMap = null;
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
    private float inbetweenAttackDelay = 0.3f;
    private float animationDuration = 0.3f;
    private Vector3 damageTextOffset = new Vector3(0, 0.8f, 0);
    
    // UI stuff
    public GameObject turnPanel = null;
    public GameObject gearNumPanel = null;
    public GameObject settingsPanel = null;
    public GameObject pauseMenu = null;
    public GameObject Mapmode = null;
    public GameObject Battlemode = null;
    public GameObject mainCameraObj = null;
    private Camera mainCamera = null;
    public GameObject damageTXTPanel = null;
    private TMPro.TextMeshProUGUI damageTXT = null;
    private GameObject gearNumPlus = null;
    public GameObject p1Victory = null;
    public GameObject p2Victory = null;
    public GameObject turnPopupPanel = null;
    public GameObject joinCodeUI = null;
    private GameObject joinCodePanel = null;
    private GameObject joinCodeL = null;
    private GameObject joinCodeR = null; 
    private TMPro.TextMeshProUGUI joinCodeTXT = null;
    
    // upgrade panel stuff
    private TMPro.TMP_InputField charName = null;
    private Image charImage = null;
    private Dropdown bodyDropdown = null;
    private Dropdown weaponDropdown = null;
    private TMPro.TextMeshProUGUI hpNUM = null;
    private TMPro.TextMeshProUGUI strNUM = null;
    private TMPro.TextMeshProUGUI magNUM = null;
    private TMPro.TextMeshProUGUI spdNUM = null;
    private TMPro.TextMeshProUGUI defNUM = null;
    private TMPro.TextMeshProUGUI resNUM = null;
    private TMPro.TextMeshProUGUI movNUM = null;
    private TMPro.TextMeshProUGUI hpMOD = null;
    private TMPro.TextMeshProUGUI strMOD = null;
    private TMPro.TextMeshProUGUI magMOD = null;
    private TMPro.TextMeshProUGUI spdMOD = null;
    private TMPro.TextMeshProUGUI defMOD = null;
    private TMPro.TextMeshProUGUI resMOD = null;
    private TMPro.TextMeshProUGUI movMOD = null;
    private TMPro.TextMeshProUGUI hpCOST = null;
    private TMPro.TextMeshProUGUI strCOST = null;
    private TMPro.TextMeshProUGUI magCOST = null;
    private TMPro.TextMeshProUGUI spdCOST = null;
    private TMPro.TextMeshProUGUI defCOST = null;
    private TMPro.TextMeshProUGUI resCOST = null;
    private TMPro.TextMeshProUGUI movCOST = null;
    private Image weaponIMG = null;
    private Button hpButton = null;
    private Button strButton = null;
    private Button magButton = null;
    private Button defButton = null;
    private Button resButton = null;
    private Button spdButton = null;
    private Button movButton = null;
    private GameObject weaponStatsPanel = null;
    private TMPro.TextMeshProUGUI weaponStats1 = null;
    private TMPro.TextMeshProUGUI weaponStats2 = null;
    private TMPro.TextMeshProUGUI weaponRange = null;
    public GameObject weaponSprites = null; 
    public GameObject upgradeMenu = null;

    // Lobby/Multiplayer stuff
    private LobbyData lobbyData;
    private int clientsConnected = 0; // counts how many connected
    
    void Start()
    {
        //Getting all context menu buttons
        moveButton = contextMenu.transform.GetChild(0).GetComponent<Button>();
        attackButton = contextMenu.transform.GetChild(1).GetComponent<Button>();
        upgradeButton = contextMenu.transform.GetChild(2).GetComponent<Button>();
        inspectButton = contextMenu.transform.GetChild(3).GetComponent<Button>();
        deselectButton = contextMenu.transform.GetChild(4).GetComponent<Button>();

        //Char Info gets all information for stats
        LcharNameTXT = charInfoPanel.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        LmovLeftTXT = charInfoPanel.transform.GetChild(9).gameObject;
        LhpNUM = charInfoPanel.transform.GetChild(10).GetComponent<TMPro.TextMeshProUGUI>();
        LstrNUM = charInfoPanel.transform.GetChild(11).GetComponent<TMPro.TextMeshProUGUI>();
        LmagNUM = charInfoPanel.transform.GetChild(12).GetComponent<TMPro.TextMeshProUGUI>();
        LspdNUM = charInfoPanel.transform.GetChild(13).GetComponent<TMPro.TextMeshProUGUI>();
        LdefNUM = charInfoPanel.transform.GetChild(14).GetComponent<TMPro.TextMeshProUGUI>();
        LresNUM = charInfoPanel.transform.GetChild(15).GetComponent<TMPro.TextMeshProUGUI>();
        LmovNUM = charInfoPanel.transform.GetChild(16).GetComponent<TMPro.TextMeshProUGUI>();
        LmovLeftNUMObj = charInfoPanel.transform.GetChild(17).gameObject;
        LmovLeftNUM = LmovLeftNUMObj.GetComponent<TMPro.TextMeshProUGUI>();

        //For Right Char Info Panel
        RcharNameTXT = charInfoPanelR.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        RmovLeftTXT = charInfoPanelR.transform.GetChild(9).gameObject;
        RhpNUM = charInfoPanelR.transform.GetChild(10).GetComponent<TMPro.TextMeshProUGUI>();
        RstrNUM = charInfoPanelR.transform.GetChild(11).GetComponent<TMPro.TextMeshProUGUI>();
        RmagNUM = charInfoPanelR.transform.GetChild(12).GetComponent<TMPro.TextMeshProUGUI>();
        RspdNUM = charInfoPanelR.transform.GetChild(13).GetComponent<TMPro.TextMeshProUGUI>();
        RdefNUM = charInfoPanelR.transform.GetChild(14).GetComponent<TMPro.TextMeshProUGUI>();
        RresNUM = charInfoPanelR.transform.GetChild(15).GetComponent<TMPro.TextMeshProUGUI>();
        RmovNUM = charInfoPanelR.transform.GetChild(16).GetComponent<TMPro.TextMeshProUGUI>();
        RmovLeftNUMObj = charInfoPanelR.transform.GetChild(17).gameObject;
        ///RmovLeftNUM = movLeftNUMObj.GetComponent<TMPro.TextMeshProUGUI>();

        //upgrade menu stuff
        charName = upgradeMenu.transform.GetChild(1).transform.GetChild(3).GetComponent<TMPro.TMP_InputField>();
        charImage = upgradeMenu.transform.GetChild(1).transform.GetChild(6).GetComponent<Image>();
        bodyDropdown = upgradeMenu.transform.GetChild(1).transform.GetChild(8).GetComponent<Dropdown>();
        weaponDropdown = upgradeMenu.transform.GetChild(1).transform.GetChild(10).GetComponent<Dropdown>();
        hpNUM = upgradeMenu.transform.GetChild(1).transform.GetChild(18).GetComponent<TMPro.TextMeshProUGUI>();
        strNUM = upgradeMenu.transform.GetChild(1).transform.GetChild(19).GetComponent<TMPro.TextMeshProUGUI>();
        magNUM = upgradeMenu.transform.GetChild(1).transform.GetChild(20).GetComponent<TMPro.TextMeshProUGUI>();
        spdNUM = upgradeMenu.transform.GetChild(1).transform.GetChild(21).GetComponent<TMPro.TextMeshProUGUI>();
        defNUM = upgradeMenu.transform.GetChild(1).transform.GetChild(22).GetComponent<TMPro.TextMeshProUGUI>();
        resNUM = upgradeMenu.transform.GetChild(1).transform.GetChild(23).GetComponent<TMPro.TextMeshProUGUI>();
        movNUM = upgradeMenu.transform.GetChild(1).transform.GetChild(24).GetComponent<TMPro.TextMeshProUGUI>();
        hpMOD = upgradeMenu.transform.GetChild(1).transform.GetChild(25).GetComponent<TMPro.TextMeshProUGUI>();
        strMOD = upgradeMenu.transform.GetChild(1).transform.GetChild(26).GetComponent<TMPro.TextMeshProUGUI>();
        magMOD = upgradeMenu.transform.GetChild(1).transform.GetChild(27).GetComponent<TMPro.TextMeshProUGUI>();
        spdMOD = upgradeMenu.transform.GetChild(1).transform.GetChild(28).GetComponent<TMPro.TextMeshProUGUI>();
        defMOD = upgradeMenu.transform.GetChild(1).transform.GetChild(29).GetComponent<TMPro.TextMeshProUGUI>();
        resMOD = upgradeMenu.transform.GetChild(1).transform.GetChild(30).GetComponent<TMPro.TextMeshProUGUI>();
        movMOD = upgradeMenu.transform.GetChild(1).transform.GetChild(31).GetComponent<TMPro.TextMeshProUGUI>();
        hpCOST = upgradeMenu.transform.GetChild(1).transform.GetChild(34).GetComponent<TMPro.TextMeshProUGUI>();
        strCOST = upgradeMenu.transform.GetChild(1).transform.GetChild(36).GetComponent<TMPro.TextMeshProUGUI>();
        magCOST = upgradeMenu.transform.GetChild(1).transform.GetChild(38).GetComponent<TMPro.TextMeshProUGUI>();
        defCOST = upgradeMenu.transform.GetChild(1).transform.GetChild(40).GetComponent<TMPro.TextMeshProUGUI>();
        resCOST = upgradeMenu.transform.GetChild(1).transform.GetChild(42).GetComponent<TMPro.TextMeshProUGUI>();
        spdCOST = upgradeMenu.transform.GetChild(1).transform.GetChild(44).GetComponent<TMPro.TextMeshProUGUI>();
        movCOST = upgradeMenu.transform.GetChild(1).transform.GetChild(46).GetComponent<TMPro.TextMeshProUGUI>();
        weaponIMG = upgradeMenu.transform.GetChild(1).transform.GetChild(47).GetComponent<Image>();
        hpButton = upgradeMenu.transform.GetChild(1).transform.GetChild(32).GetComponent<Button>();
        strButton = upgradeMenu.transform.GetChild(1).transform.GetChild(35).GetComponent<Button>();
        magButton = upgradeMenu.transform.GetChild(1).transform.GetChild(37).GetComponent<Button>();
        defButton = upgradeMenu.transform.GetChild(1).transform.GetChild(39).GetComponent<Button>();
        resButton = upgradeMenu.transform.GetChild(1).transform.GetChild(41).GetComponent<Button>();
        spdButton = upgradeMenu.transform.GetChild(1).transform.GetChild(43).GetComponent<Button>();
        movButton = upgradeMenu.transform.GetChild(1).transform.GetChild(45).GetComponent<Button>();
        weaponStatsPanel = upgradeMenu.transform.GetChild(1).transform.GetChild(48).gameObject;
        weaponStats1 = weaponStatsPanel.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
        weaponStats2 = weaponStatsPanel.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        weaponRange = weaponStatsPanel.transform.GetChild(3).GetComponent<TMPro.TextMeshProUGUI>();

        //Finding Parents for movement area display
        gridParent = GameObject.Find("Grids").gameObject;
        currGrid = GameObject.Find("Grid - Map1").gameObject.GetComponent<Grid>();
        overlayMap = currGrid.transform.GetChild(2).GetComponent<Tilemap>();
        collisionMap = currGrid.transform.GetChild(1).GetComponent<Tilemap>();
        pathfinding = new MPathfinding(17, 11, collisionMap);
        moveAreaParent = GameObject.Find("moveAreas").gameObject;
        attackAreaParent = GameObject.Find("attackAreas").gameObject;
        turnModeTXT = GameObject.Find("currentTurnTXT").GetComponent<TMPro.TextMeshProUGUI>();
        

        // initializing UI stuff
        mainCamera = mainCameraObj.GetComponent<Camera>();
        damageTXT = damageTXTPanel.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
        gearNumPlus = gearNumPanel.transform.GetChild(2).gameObject;
        joinCodePanel = joinCodeUI.transform.GetChild(0).gameObject;
        joinCodeTXT = joinCodePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        joinCodeL = joinCodeUI.transform.GetChild(2).gameObject;
        joinCodeR = joinCodeUI.transform.GetChild(1).gameObject;

        // initialize p1
        p1 = transform.GetChild(0).gameObject;
        p1Units = new GameObject[p1.transform.childCount];
        p1Stats = new Character[p1.transform.childCount];
        // initialize p2
        p2 = transform.GetChild(1).gameObject;
        p2Units = new GameObject[p2.transform.childCount];
        p2Stats = new Character[p2.transform.childCount];
        //Initialize map features
        mapFeatures = transform.GetChild(2).gameObject;
        featureUnits = new GameObject[mapFeatures.transform.childCount];
        featuresFeatures = new mapFeatures[mapFeatures.transform.childCount];

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

        //For map features
        i = 0;
        foreach (Transform child in mapFeatures.transform)
        {
            featureUnits[i] = child.gameObject;
            featuresFeatures[i] = featureUnits[i].GetComponent<mapFeatures>();
            featureUnits[i].transform.position = featuresStartPos[i];
            featuresFeatures[i].changeFeature(featureTypeList[i]);

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

        clickLock = 3;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
    }

    public void turnOffGrids()
    {
        for (int i = 0; i < gridParent.transform.childCount; i++)
        {
            gridParent.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    [ServerRpc]
    public void InitializeGameServerRpc()
    {
        //if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
           // return;
        
        if (clientsConnected != 2)
            return;

        // grab lobbyData
        lobbyData = GameObject.Find("LobbyData").GetComponent<LobbyData>();
        
        // pick map
        currGrid = gridParent.transform.GetChild((int)lobbyData.getMap()).GetComponent<Grid>();
        overlayMap = currGrid.transform.GetChild(2).GetComponent<Tilemap>();
        collisionMap = currGrid.transform.GetChild(1).GetComponent<Tilemap>();
        pathfinding = new MPathfinding(17, 11, collisionMap);
        turnOffGrids();
        currGrid.gameObject.SetActive(true);
        
        // assign lobby spring amount
        giveGearNum(lobbyData.getSprings(), false); ;
        giveGearNum(lobbyData.getSprings(), true);
        
        // create lobby character number
        for (int j = 0; j < (10 - lobbyData.getUnits()); j++)
        {
            p1Units[9 - j].transform.parent = null; 
            p2Units[9 - j].transform.parent = null; 
            Destroy(p1Units[9 - j]);
            Destroy(p2Units[9 - j]);
        }

        // reinitialize p1
        p1Units = new GameObject[p1.transform.childCount];
        p1Stats = new Character[p1.transform.childCount];
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

        // reinitialize p2
        p2Units = new GameObject[p2.transform.childCount];
        p2Stats = new Character[p2.transform.childCount];
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
        InitializeGameClientRpc(lobbyData.getUnits(), (int)lobbyData.getMap());
        
        changeTurn(turnMode.Player1Turn);
        passTurnModeClientRpc(currTurnMode);
        
        changeMode(gameMode.MapMode);

        updateTurnText();
        
        clickLock = 0;
        passClickLockClientRpc(clickLock);
    }

    [ClientRpc]
    private void InitializeGameClientRpc(int unitNum, int mapNum)
    {
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
            return;
        
        // pick map
        currGrid = gridParent.transform.GetChild(mapNum).GetComponent<Grid>();
        overlayMap = currGrid.transform.GetChild(2).GetComponent<Tilemap>();
        collisionMap = currGrid.transform.GetChild(1).GetComponent<Tilemap>();
        pathfinding = new MPathfinding(17, 11, collisionMap);
        turnOffGrids();
        currGrid.gameObject.SetActive(true);
        
        // create lobby character number
        for (int j = 0; j < (10 - unitNum); j++)
        {
            p1Units[9 - j].transform.parent = null; 
            p2Units[9 - j].transform.parent = null; 
            Destroy(p1Units[9 - j]);
            Destroy(p2Units[9 - j]);
        }

        // reinitialize p1
        p1Units = new GameObject[p1.transform.childCount];
        p1Stats = new Character[p1.transform.childCount];
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

        // reinitialize p2
        p2Units = new GameObject[p2.transform.childCount];
        p2Stats = new Character[p2.transform.childCount];
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
    }
    
    private void OnClientConnectedCallback(ulong clientId)
    {
        clientsConnected = clientsConnected + 1;
        InitializeGameServerRpc();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (NetworkManager.Singleton.LocalClientId == (ulong)player1 && (clickLock == 1 || clickLock == 3))
                return;
            
            if (NetworkManager.Singleton.LocalClientId == (ulong)player2 && (clickLock == 2 || clickLock == 3))
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
                return;
            }

            else if (moveActive == true)
            {
                validatePathServerRpc(mousePos, new ServerRpcParams());
                return;
            }
            
            clickedCharServerRpc(mousePos, new ServerRpcParams());
        }
    }

    public void changeJoinCode(string joinCode)
    {
        if (joinCode != null)
            joinCodeTXT.text = joinCode; 
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
                    attackActive = false;
                    passAttackActiveClientRpc(attackActive);
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
                    attackActive = false;
                    passAttackActiveClientRpc(attackActive);
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
            attackActive = false;
            passAttackActiveClientRpc(attackActive);
            return;
        }
        
        if ((int)serverRpcParams.Receive.SenderClientId == player1)
        { 
            if (p1TargetedStats.getCanAttack() == true && inAttackRange(mousePos, p1Targeted)) 
            {
                startBattle(p1Targeted.gameObject, target, serverRpcParams);
                return; 
            }

        }
        else if ((int)serverRpcParams.Receive.SenderClientId == player2)
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
        if (serverRpcParams.Receive.SenderClientId == (ulong)player1)
        {
            p1TargetedStats.movLeft = 0;
            passMovStatClientRpc(p1Targeted.name, p1TargetedStats.movLeft, p1TargetedStats.baseMOV);
        }
        else if (serverRpcParams.Receive.SenderClientId == (ulong)player2)
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

    [ClientRpc]
    private void startBattlemodeClientRpc(string leftName, string rightName, Vector3 lPos, Vector3 rPos, Quaternion lQua, Quaternion rQua)
    {
        GameObject leftChar = GameObject.Find(leftName);
        GameObject rightChar = GameObject.Find(rightName);
        
        turnPanel.SetActive(false);
        gearNumPanel.SetActive(false);
        settingsPanel.SetActive(false);
        deactivateAllChildren();
        // reactivate participants
        leftChar.SetActive(true);
        rightChar.SetActive(true);
        Mapmode.SetActive(false);
        Battlemode.SetActive(true);
        mainCamera.orthographicSize = camBattleSize;
        mainCamera.transform.position = camBattlePos;
        charInfoPanel.SetActive(true);
        charInfoPanelR.SetActive(true);
        // copy initial position
        leftChar.transform.position = lPos;
        rightChar.transform.position = rPos;
        leftChar.transform.rotation = lQua;
        rightChar.transform.rotation = rQua;
        //
        leftChar.GetComponent<Character>().statReveal = true;
        rightChar.GetComponent<Character>().statReveal = true;
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
        deselectTargetServerRpc(new ServerRpcParams()); // leaves target active for one player on server, might cause a bug later
        p1DeselectClientRpc();
        p2DeselectClientRpc();
        deactivateAllChildren();
        // reactivate participants
        leftChar.SetActive(true);
        rightChar.SetActive(true);
        Mapmode.SetActive(false);
        Battlemode.SetActive(true);
        savedCamSize = mainCamera.orthographicSize;
        mainCamera.orthographicSize = camBattleSize;
        charInfoPanel.SetActive(true);
        charInfoPanelR.SetActive(true);
        updateBattleStats(leftChar, rightChar);
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

        startBattlemodeClientRpc(leftChar.name, rightChar.name, leftChar.transform.position, rightChar.transform.position
        , leftBattleQua, rightBattleQua);
        
        // delay for 1.5s so user can see before battle starts
        yield return new WaitForSeconds(inbetweenAttackDelay);

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
            updateBattleStats(leftChar, rightChar);
        }
 
        // delay
        yield return new WaitForSeconds(inbetweenAttackDelay * 3);

        // second attack
        if (secondStats.getAttackRange() >= battleRange)
        {
            yield return StartCoroutine(LerpPosition(secondAttacker, secondAttacker.transform.position + secondAttacker.transform.right, animationDuration));
            yield return new WaitForSeconds(inbetweenAttackDelay);
            yield return StartCoroutine(updateDamageTXT(firstAttacker, Attack(secondStats, firstStats))); // person taking damage and damage value
            updateBattleStats(leftChar, rightChar);
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

        leaveBattlemodClientRpc(leftChar.name, rightChar.name, leftChar.transform.position, rightChar.transform.position, 
            mainCamera.transform.position, mainCamera.orthographicSize, savedQuaLeft, savedQuaRight);
        
        // return to either player or enemy turn
        if (currTurnMode == turnMode.Player1Turn)
        {
            if (leftChar.transform.IsChildOf(p1.transform))
            {
                leftStats.setAttack(false);
                passAtkStatClientRpc(leftChar.name, false);
            }

            else {
                rightStats.setAttack(false);
                passAtkStatClientRpc(rightChar.name,false);
            }
        }
        else
        {
            if (leftChar.transform.IsChildOf(p2.transform))
            {
                leftStats.setAttack(false);
                passAtkStatClientRpc(leftChar.name, false);
            }

            else {
                rightStats.setAttack(false);
                passAtkStatClientRpc(rightChar.name,false);
            }
        }
        clickLock = 0;
        passClickLockClientRpc(clickLock);

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

    [ClientRpc]
    private void copyPositionClientRpc(string target , Vector3 tPos)
    {
        GameObject targetChar = GameObject.Find(target);
        targetChar.transform.position = tPos;
    }
    
    [ClientRpc]
    private void leaveBattlemodClientRpc(string leftName, string rightName, Vector3 lPos, Vector3 rPos, Vector3 camPos, float camSize, Quaternion lQua, Quaternion rQua)
    {
        GameObject leftChar = GameObject.Find(leftName);
        GameObject rightChar = GameObject.Find(rightName);
        
        leftChar.transform.position = lPos;
        rightChar.transform.position = rPos;
        mainCamera.transform.position = camPos;
        mainCamera.orthographicSize = camSize;
        leftChar.transform.rotation = savedQuaLeft;
        rightChar.transform.rotation = savedQuaRight;
        
        charInfoPanel.SetActive(false);
        charInfoPanelR.SetActive(false);
        turnPanel.SetActive(true);
        gearNumPanel.SetActive(true);
        settingsPanel.SetActive(true);
        activateAllChildren();
        Mapmode.SetActive(true);
        Battlemode.SetActive(false);
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
        
        passTurnModeClientRpc(currTurnMode);

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
        
        passGameModeClientRpc(currGameMode);
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
        turnPopupClientRpc();
    }

    [ClientRpc]
    private void turnPopupClientRpc()
    {
        turnPopupPanel.SetActive(true);
        Image img = turnPopupPanel.GetComponent<Image>();
        img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
        GameObject turnPopupText = turnPopupPanel.transform.GetChild(0).gameObject;
        TextMeshProUGUI tmp = turnPopupText.GetComponent<TextMeshProUGUI>();
        tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, 1f);

        if (currTurnMode == turnMode.Player1Turn)
            tmp.text = "Player 1's Turn";
        else
            tmp.text = "Player 2's Turn";
        
        StartCoroutine(turnPopup());
    }

    private IEnumerator turnPopup()
    {
        Image img = turnPopupPanel.GetComponent<Image>();
        TextMeshProUGUI tmp = turnPopupPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        for (int i = 0; i < 40; i++)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, img.color.a - 0.025f);
            tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, tmp.color.a - 0.025f);
        
            if (img.color.a <= 0.05f)
            {
                turnPopupPanel.SetActive(false);
                yield break;
            }
        
            yield return new WaitForSeconds(0.05f);
        }
    }

    Vector3Int GetMousePosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return currGrid.WorldToCell(mouseWorldPos);
    }

    [ServerRpc(RequireOwnership = false)]
    public void clickedCharServerRpc(Vector3Int mousePos, ServerRpcParams serverRpcParams)
    {
        Debug.Log("clickedCharServerRpc on " + NetworkManager.Singleton.LocalClientId);

        // if player 1
        if ((int)serverRpcParams.Receive.SenderClientId == player1)
        {
            for (int i = 0; i < p1Units.Length; i++)
            {
                // an ally was clicked
                if (mousePos == p1Units[i].transform.position && p1Stats[i].getIsDead() == false)
                {
                    GameObject target = p1Units[i];
                    deselectTargetServerRpc(serverRpcParams);
                    p1TargetServerRpc(target.transform.name, serverRpcParams);                                   
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
                    p1TargetServerRpc(target.transform.name, serverRpcParams);                                   
                    P1openContextMenuClientRpc(mousePos);
                    return;
                }
            }

            // else, clicked nothing
            deselectTargetServerRpc(serverRpcParams);
            contextMenu.SetActive(false);
        }
        // player 2
        else if ((int)serverRpcParams.Receive.SenderClientId == player2)
        {
            for (int i = 0; i < p2Units.Length; i++)
            {
                // an ally was clicked
                if (mousePos == p2Units[i].transform.position && p2Stats[i].getIsDead() == false)
                {                     
                    GameObject target = p2Units[i];
                    deselectTargetServerRpc(serverRpcParams);
                    p2TargetServerRpc(target.transform.name, serverRpcParams);                                   
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
                    p2TargetServerRpc(target.transform.name, serverRpcParams);                                   
                    P2openContextMenuClientRpc(mousePos);
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
        {
            Debug.Log("tried to end turn while NOT in Mapmode");
            return;
        }
            

        // player 1 turn
        if (currTurnMode == turnMode.Player1Turn)
        {
            // make sure player 1 sent this request
            if ((int)serverRpcParams.Receive.SenderClientId != player1)
            {
                Debug.Log("Player 2 tried to end Player 1s turn");
            }
            else
            {
                p1DeselectClientRpc();
                changeTurn(turnMode.Player2Turn);
                resetAllMoveServerRpc();
                resetAllAttackServerRpc();
            }
        }
        // player 2 turn
        if (currTurnMode == turnMode.Player2Turn)
        {
            // make sure player 2 sent this request
            if ((int)serverRpcParams.Receive.SenderClientId != player2)
            {
                Debug.Log("Player 1 tried to end Player 2s turn");
            }
            else
            {
                p2DeselectClientRpc();
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
        if ((int)serverRpcParams.Receive.SenderClientId == player1)
        {
            if (p1Targeted == null)
                return;

            p1Targeted.transform.GetChild(0).gameObject.SetActive(false);
            moveActive = false;
            attackActive = false;
            p1Targeted = null;
            p1TargetedStats = null;
            charInfoPanel.gameObject.SetActive(false);
            moveAreas[0].gameObject.SetActive(false);
            moveAreas[1].gameObject.SetActive(false);
            contextMenu.SetActive(false);
            overlayMap.ClearAllTiles();
            p1DeselectClientRpc();
        }
        else if ((int)serverRpcParams.Receive.SenderClientId == player2)
        {
            if (p2Targeted == null)
                return;

            //p2Targeted.transform.GetChild(0).gameObject.SetActive(false);
            moveActive = false;
            attackActive = false;
            p2Targeted = null;
            p2TargetedStats = null;
            //charInfoPanel.gameObject.SetActive(false); // dont do on server
            //moveAreas[0].gameObject.SetActive(false);
            //moveAreas[1].gameObject.SetActive(false);
            //contextMenu.SetActive(false); // dont do on server
            //overlayMap.ClearAllTiles();
            p2DeselectClientRpc();
        }
    }

    
    [ServerRpc(RequireOwnership = false)]
    void p1TargetServerRpc(String targetName, ServerRpcParams serverRpcParams)
    {
     
        GameObject target = GameObject.Find(targetName);
        moveActive = false;
        attackActive = false;
        if (target.GetComponent<Character>().getIsDead() == true)
            return;
        p1Targeted = target;
        p1TargetedStats = p1Targeted.GetComponent<Character>();
        p1Targeted.transform.GetChild(0).gameObject.SetActive(true);
        //updateUpgradeMenuServerRpc(p1Targeted.name);
        p1hideAreaClientRpc();  
        p1SelectClientRpc(p1Targeted.transform.name);
    }

    [ServerRpc(RequireOwnership = false)]
     void p2TargetServerRpc(String targetName, ServerRpcParams serverRpcParams)
    {
    
        GameObject target = GameObject.Find(targetName);
        moveActive = false;
        attackActive = false;
        if (target.GetComponent<Character>().getIsDead() == true)
            return;
        p2Targeted = target;
        p2TargetedStats = p2Targeted.GetComponent<Character>();
        //p2Targeted.transform.GetChild(0).gameObject.SetActive(true);
        //updateUpgradeMenuServerRpc(p2Targeted.name);
        p2hideAreaClientRpc();
        p2SelectClientRpc(p2Targeted.transform.name);
    }

    [ClientRpc]
    void p1SelectClientRpc(String name)
    {
        //Debug.Log("p1SelectClientRpc on " + NetworkManager.Singleton.LocalClientId);
        //if player 2 ignore 
        if (NetworkManager.Singleton.LocalClientId != (ulong)player1)
        {
           // Debug.Log("Returning P1SelectRPC");
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
        if (NetworkManager.Singleton.LocalClientId != (ulong)player1)
        {
            return;
        }
        
        attackAreas[0].gameObject.SetActive(false);
        attackAreas[1].gameObject.SetActive(false);
        if (p1Targeted != null) 
            p1Targeted.transform.GetChild(0).gameObject.SetActive(false);
        p1Targeted = null;
        p1TargetedStats = null;
        contextMenu.SetActive(false);
        overlayMap.ClearAllTiles(); // turn off movement highlighting
        moveActive = false;
        attackActive = false;
        charInfoPanel.SetActive(false);
        p1hideAreaClientRpc();
    }

    [ClientRpc]
    void p2SelectClientRpc(String name)
    {
        
        //if player 1 ignore 
        if (NetworkManager.Singleton.LocalClientId != (ulong)player2)
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
        if (NetworkManager.Singleton.LocalClientId != (ulong)player2)
        {
            return;
        }
        attackAreas[0].gameObject.SetActive(false);
        attackAreas[1].gameObject.SetActive(false);
        if (p2Targeted != null)
            p2Targeted.transform.GetChild(0).gameObject.SetActive(false);
        p2Targeted = null;
        p2TargetedStats = null;
        contextMenu.SetActive(false);
        overlayMap.ClearAllTiles(); // turn off movement highlighting
        moveActive = false;
        attackActive = false;
        charInfoPanel.SetActive(false);
        p2hideAreaClientRpc();
    }

    [ClientRpc]
    public void P1openContextMenuClientRpc(Vector3 mousePos)
    {
        //Ignore for player 2. 
        if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
        {
            return;
        }
        
        if (p1Targeted == null)
            return;
        
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
            p1hideAreaClientRpc();
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
            p1hideAreaClientRpc();            //showArea(currTargeted);

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
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
        {
            return;
        }
        
        if (p2Targeted == null)
            return;
        
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
            p2hideAreaClientRpc();   
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
            p2hideAreaClientRpc();
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
         if (p1Targeted == null)
             return;

         // if an enemy and not yet revealed
         if (p1Targeted.transform.IsChildOf(p2.transform) && p1TargetedStats.statReveal == false)
         {
             LcharNameTXT.text = "Name: " + p1TargetedStats.getCharName();
             LhpNUM.text = "" + p1TargetedStats.hpLeft + " / " + p1TargetedStats.HP;
             LstrNUM.text = "??";
             LmagNUM.text = "??";
             LdefNUM.text = "??";
             LresNUM.text = "??";
             LspdNUM.text = "??";
             LmovNUM.text = "??";
             LmovLeftTXT.SetActive(false);
             LmovLeftNUMObj.SetActive(false);
         }
         else
         {
             LcharNameTXT.text = "Name: " + p1TargetedStats.getCharName();
             LhpNUM.text = "" + p1TargetedStats.hpLeft + " / " + p1TargetedStats.HP;
             LstrNUM.text = "" + p1TargetedStats.STR;
             LmagNUM.text = "" + p1TargetedStats.MAG;
             LdefNUM.text = "" + p1TargetedStats.DEF;
             LresNUM.text = "" + p1TargetedStats.RES;
             LspdNUM.text = "" + p1TargetedStats.SPD;

             if (!p1Targeted.transform.IsChildOf(p2.transform))
             {
                 LmovNUM.text = "" + p1TargetedStats.MOV;
                 LmovLeftNUM.text = "" + p1TargetedStats.movLeft;
                 LmovLeftTXT.SetActive(true);
                 LmovLeftNUMObj.SetActive(true);
             }
             else
             {
                 LmovNUM.text = "" + p1TargetedStats.MOV;
                 LmovLeftTXT.SetActive(false);
                 LmovLeftNUMObj.SetActive(false);
             }
         }
     }
    
    public void P2updateCharInfo()
    {
        if (p2Targeted == null)
            return;

        // if an enemy and not yet revealed
        if (p2Targeted.transform.IsChildOf(p1.transform) && p2TargetedStats.statReveal == false)
        {
            LcharNameTXT.text = "Name: " + p2TargetedStats.getCharName();
            LhpNUM.text = "" + p2TargetedStats.hpLeft + " / " + p2TargetedStats.HP;
            LstrNUM.text = "??";
            LmagNUM.text = "??";
            LdefNUM.text = "??";
            LresNUM.text = "??";
            LspdNUM.text = "??";
            LmovNUM.text = "??";
            LmovLeftTXT.SetActive(false);
            LmovLeftNUMObj.SetActive(false);
        }
        else
        {
            LcharNameTXT.text = "Name: " + p2TargetedStats.getCharName();
            LhpNUM.text = "" + p2TargetedStats.hpLeft + " / " + p2TargetedStats.HP;
            LstrNUM.text = "" + p2TargetedStats.STR;
            LmagNUM.text = "" + p2TargetedStats.MAG;
            LdefNUM.text = "" + p2TargetedStats.DEF;
            LresNUM.text = "" + p2TargetedStats.RES;
            LspdNUM.text = "" + p2TargetedStats.SPD;

            if (!p2Targeted.transform.IsChildOf(p1.transform))
            {
                LmovNUM.text = "" + p2TargetedStats.MOV;
                LmovLeftNUM.text = "" + p2TargetedStats.movLeft;
                LmovLeftTXT.SetActive(true);
                LmovLeftNUMObj.SetActive(true);
            }
            else
            {
                LmovNUM.text = "" + p2TargetedStats.MOV;
                LmovLeftTXT.SetActive(false);
                LmovLeftNUMObj.SetActive(false);
            }
        }
    }
    
    public void highlightMove()
    {
        //For Player 1 movement 
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1 && currTurnMode == turnMode.Player1Turn)
        {
            if (p1Targeted == null)
                return;
            
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
        if (NetworkManager.Singleton.LocalClientId == (ulong)player2 && currTurnMode == turnMode.Player2Turn)
        {
            if (p2Targeted == null)
                return;
            
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
    
    public void upgradeButtonPressed()
    {
        // if not in mapmode dont do anything
        if (currGameMode != gameMode.MapMode)
            return;
        
        upgradeServerRpc(new ServerRpcParams());
    }

    [ServerRpc(RequireOwnership = false)]
    public void upgradeServerRpc(ServerRpcParams serverRpcParams)
    {
        changeMode(gameMode.MenuMode);

        if (serverRpcParams.Receive.SenderClientId == (ulong)player1)
        {
            p1UpgradeClientRpc();
            
        }
        else if (serverRpcParams.Receive.SenderClientId == (ulong)player2)
        {
            p2UpgradeClientRpc();
        }
    }

    [ServerRpc(RequireOwnership=false)]
    public void closeUpgradeMenuServerRpc(ServerRpcParams serverRpcParams)
    {
        changeMode(gameMode.MapMode);
        if (serverRpcParams.Receive.SenderClientId == (ulong)player1)
        {
            p1closeUpgradeMenuClientRpc();
            
        }
        else if (serverRpcParams.Receive.SenderClientId == (ulong)player2)
        {
            p2closeUpgradeMenuClientRpc();
        }
    }
    
    public void closeUpgradeMenu()
    {
       closeUpgradeMenuServerRpc(new ServerRpcParams());
    }
    
    [ClientRpc]
    public void p1closeUpgradeMenuClientRpc()
    {
        if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
        {
            return;
        }
        if (p1Targeted == null)
            return;
        if (upgradeMenu.activeSelf == true)
        {
               
            changeMode(gameMode.MapMode);
            upgradeMenu.gameObject.SetActive(false);
            P1updateCharInfo();
            P1openContextMenuClientRpc(p1Targeted.transform.position);
            clickLock = 0;
            passClickLockClientRpc(0);
            changeMode(gameMode.MapMode);
        }
    }
    
    [ClientRpc]
    public void p2closeUpgradeMenuClientRpc()
    {
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
        {
            return;
        }
        if (p2Targeted == null)
            return;
        if (upgradeMenu.activeSelf == true)
        {
               
            changeMode(gameMode.MapMode);
            upgradeMenu.gameObject.SetActive(false);
            P2updateCharInfo();
            P2openContextMenuClientRpc(p2Targeted.transform.position);
            clickLock = 0;
            passClickLockClientRpc(0);
            changeMode(gameMode.MapMode);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
     public void updateUpgradeMenuServerRpc(string name)
    {
        GameObject character = GameObject.Find(name);

        if (character == null)
            return;
        
        Character charStats = character.GetComponent<Character>();
        charName.text = charStats.getCharName();
        charImage.sprite = character.GetComponent<SpriteRenderer>().sprite;
        charImage.transform.localScale = character.transform.localScale;

        weaponIMG.sprite = character.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite;
        weaponIMG.transform.localScale = charStats.transform.GetChild(1).localScale;
        charStats.updateCosts();

        if (bodyDropdown.options.Count == 0)
        {
            string[] bodyNames = Enum.GetNames(typeof(Character.bodyType));
            List<string> body = new List<string>(bodyNames);
            bodyDropdown.AddOptions(body);
        }
        else
        {
            bodyDropdown.value = (int)charStats.GetBodyType();
        }

        if (weaponDropdown.options.Count == 0)
        {
            List<string> weapons = new List<string>();

            for (int i = 0; i < weaponSprites.transform.childCount; i++)
            {
                weapons.Add(weaponSprites.transform.GetChild(i).name);
            }

            weaponDropdown.AddOptions(weapons);
        }
        else
        {
            weaponDropdown.value = (int)charStats.GetWeaponType();
        }

        hpNUM.text = "" + charStats.baseHP;
        strNUM.text = "" + charStats.baseSTR;
        magNUM.text = "" + charStats.baseMAG;
        defNUM.text = "" + charStats.baseDEF;
        resNUM.text = "" + charStats.baseRES;
        spdNUM.text = "" + charStats.baseSPD;
        movNUM.text = "" + charStats.baseMOV;

        weaponRange.text = "RNG: " + charStats.getAttackRange();
        weaponStats1.text = "";
        weaponStats2.text = "";

        if (charStats.HPMOD == 0 && charStats.STRMOD == 0 && charStats.MAGMOD == 0 && charStats.DEFMOD == 0
            && charStats.RESMOD == 0 && charStats.SPDMOD == 0 && charStats.MOVMOD == 0)
        {
            //weaponStatsPanel.gameObject.SetActive(false);
            weaponStats1.text = "";
            weaponStats2.text = "";
        }

        if (charStats.HPMOD == 0)
            hpMOD.text = "";
        else if (charStats.HPMOD > 0)
        {
            hpMOD.color = Color.green;
            hpMOD.text = "+ " + charStats.HPMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            weaponStats1.color = Color.green;
            weaponStats1.text = "+" + charStats.HPMOD + " HP";
        }
        else
        {
            hpMOD.color = Color.red;
            hpMOD.text = "- " + Mathf.Abs(charStats.HPMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            weaponStats1.color = Color.red;
            weaponStats1.text = "-" + Mathf.Abs(charStats.HPMOD) + " HP";
        }

        if (charStats.STRMOD == 0)
            strMOD.text = "";
        else if (charStats.STRMOD > 0)
        {
            strMOD.color = Color.green;
            strMOD.text = "+ " + charStats.STRMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.STRMOD + " STR";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.STRMOD + " STR";
            }
        }
        else
        {
            strMOD.color = Color.red;
            strMOD.text = "- " + Mathf.Abs(charStats.STRMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.STRMOD) + " STR";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.STRMOD) + " STR";
            }
        }

        if (charStats.MAGMOD == 0)
            magMOD.text = "";
        else if (charStats.MAGMOD > 0)
        {
            magMOD.color = Color.green;
            magMOD.text = "+ " + charStats.MAGMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.MAGMOD + " MAG";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.MAGMOD + " MAG";
            }
        }
        else
        {
            magMOD.color = Color.red;
            magMOD.text = "- " + Mathf.Abs(charStats.MAGMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.MAGMOD) + " MAG";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.MAGMOD) + " MAG";
            }
        }

        if (charStats.DEFMOD == 0)
            defMOD.text = "";
        else if (charStats.DEFMOD > 0)
        {
            defMOD.color = Color.green;
            defMOD.text = "+ " + charStats.DEFMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.DEFMOD + " DEF";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.DEFMOD + " DEF";
            }
        }
        else
        {
            defMOD.color = Color.red;
            defMOD.text = "- " + Mathf.Abs(charStats.DEFMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.DEFMOD) + " DEF";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.DEFMOD) + " DEF";
            }
        }

        if (charStats.RESMOD == 0)
            resMOD.text = "";
        else if (charStats.RESMOD > 0)
        {
            resMOD.color = Color.green;
            resMOD.text = "+ " + charStats.RESMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.RESMOD + " RES";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.RESMOD + " RES";
            }
        }
        else
        {
            resMOD.color = Color.red;
            resMOD.text = "- " + Mathf.Abs(charStats.RESMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.RESMOD) + " RES";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.RESMOD) + " RES";
            }
        }

        if (charStats.SPDMOD == 0)
            spdMOD.text = "";
        else if (charStats.SPDMOD > 0)
        {
            spdMOD.color = Color.green;
            spdMOD.text = "+ " + charStats.SPDMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.SPDMOD + " SPD";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.SPDMOD + " SPD";
            }
        }
        else
        {
            spdMOD.color = Color.red;
            spdMOD.text = "- " + Mathf.Abs(charStats.SPDMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.SPDMOD) + " SPD";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.SPDMOD) + " SPD";
            }
        }

        if (charStats.MOVMOD == 0)
            movMOD.text = "";
        else if (charStats.MOVMOD > 0)
        {
            movMOD.color = Color.green;
            movMOD.text = "+ " + charStats.MOVMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.MOVMOD + " MOV";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.MOVMOD + " MOV";
            }
        }
        else
        {
            movMOD.color = Color.red;
            movMOD.text = "- " + Mathf.Abs(charStats.MOVMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.MOVMOD) + " MOV";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.MOVMOD) + " MOV";
            }
        }

        if (charStats.baseHP < charStats.getHPMAX())
        {
            hpButton.gameObject.SetActive(true);
            hpCOST.text = "x" + charStats.HPCost;
        }
        else
        {
            hpCOST.text = "MAX";
            hpButton.gameObject.SetActive(false);
        }


        if (charStats.baseSTR < charStats.getSTRMAX())
        {
            strButton.gameObject.SetActive(true);
            strCOST.text = "x" + charStats.STRCost;
        }        
        else
        {
            strCOST.text = "MAX";
            strButton.gameObject.SetActive(false);
        }

        if (charStats.baseMAG < charStats.getMAGMAX())
        {
            magButton.gameObject.SetActive(true);
            magCOST.text = "x" + charStats.MAGCost;
        }
        else
        {
            magCOST.text = "MAX";
            magButton.gameObject.SetActive(false);
        }

        if (charStats.baseDEF < charStats.getDEFMAX())
        {
            defButton.gameObject.SetActive(true);
            defCOST.text = "x" + charStats.DEFCost;
        }   
        else
        {
            defCOST.text = "MAX";
            defButton.gameObject.SetActive(false);
        }

        if (charStats.baseRES < charStats.getRESMAX())
        {
            resButton.gameObject.SetActive(true);
            resCOST.text = "x" + charStats.RESCost;
        } 
        else
        {
            resCOST.text = "MAX";
            resButton.gameObject.SetActive(false);
        }

        if (charStats.baseSPD < charStats.getSPDMAX())
        {
            spdButton.gameObject.SetActive(true);
            spdCOST.text = "x" + charStats.SPDCost;
        }       
        else
        {
            spdCOST.text = "MAX";
            spdButton.gameObject.SetActive(false);
        }

        if (charStats.baseMOV < charStats.getMOVMAX())
        {
            movButton.gameObject.SetActive(true);
            movCOST.text = "x" + charStats.MOVCost;
        }         
        else
        {
            movCOST.text = "MAX";
            movButton.gameObject.SetActive(false);
        }

        updateUpgradeMenuClientRpc(character.name);
    }

     [ServerRpc(RequireOwnership = false)]
     public void changedNameServerRpc(string s, ServerRpcParams serverRpcParams)
     {
         if (serverRpcParams.Receive.SenderClientId == (ulong)player1)
         {
             p1TargetedStats.setCharName(s);
             changedNameClientRpc(s,p1Targeted.name);
             
         }
         else if (serverRpcParams.Receive.SenderClientId == (ulong)player2)
         {
             p2TargetedStats.setCharName(s);
             changedNameClientRpc(s,p2Targeted.name);
         }
     }

     [ClientRpc]
     public void changedNameClientRpc(string s, string name)
     {
         GameObject target = GameObject.Find(name);
         Character targetStats = target.GetComponent<Character>();
         targetStats.setCharName(s);
         updateUpgradeMenuServerRpc(target.name);
     }
     
     public void changedName()
     {
         changedNameServerRpc(charName.text,new ServerRpcParams());
     }

     public void changedBody(Dropdown d)
     {
         if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
         {
             if (p1Targeted != null)
                changedBodyServerRpc(d.value, p1Targeted.name);
         }
         else if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
         {
             if (p2Targeted != null)
                changedBodyServerRpc(d.value, p2Targeted.name);
         }
     }
     
     [ClientRpc]
     public void changedBodyClientRpc(int val, string name)
     {
         GameObject unit = GameObject.Find(name);
         unit.GetComponent<Character>().changeBody((Character.bodyType)val);
         updateUpgradeMenuServerRpc(unit.name);
     }
     
     [ServerRpc(RequireOwnership = false)]
     public void changedBodyServerRpc(int val, string name)
     {
         GameObject unit = GameObject.Find(name);
         Character unitStats = unit.GetComponent<Character>();
         unitStats.changeBody((Character.bodyType)val);
         changedBodyClientRpc(val, unit.name);
         updateUpgradeMenuServerRpc(unit.name);
     }

     [ClientRpc]
     public void changedWeaponClientRpc(int val, string name)
     {
         GameObject unit = GameObject.Find(name);
         unit.GetComponent<Character>().changeWeapon((Character.weaponType)val);
         updateUpgradeMenuServerRpc(unit.name);
     }

     [ServerRpc(RequireOwnership = false)]
     public void changedWeaponServerRpc(int val, string name)
     {
         GameObject unit = GameObject.Find(name);
         Character unitStats = unit.GetComponent<Character>();
         unitStats.changeWeapon((Character.weaponType)val);
         changedWeaponClientRpc(val, unit.name);
         updateUpgradeMenuServerRpc(unit.name);
     }
     
     public void changedWeapon(Dropdown d)
     {
         if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
         {
             if (p1Targeted != null)
                changedWeaponServerRpc(d.value, p1Targeted.name);
         }
         else if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
         {
             if (p2Targeted != null)
                changedWeaponServerRpc(d.value, p2Targeted.name);
         }
     }

    [ClientRpc]
    public void p1UpgradeClientRpc()
    {
        
        if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
        {
            return;
        }
        contextMenu.SetActive(false);
        upgradeMenu.gameObject.SetActive(true);
        updateUpgradeMenuServerRpc(p1Targeted.name);

    }

    public void hpButtonPressed()
    {
        hpButtonPressedServerRpc(new ServerRpcParams());
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
        {
            updateUpgradeMenuServerRpc(p1Targeted.name);
            
        }
        else if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
        {
            updateUpgradeMenuServerRpc(p2Targeted.name);
        }
    }

    [ClientRpc]
    public void updateUpgradeMenuClientRpc(string name)
    {
        GameObject character = GameObject.Find(name);
        
        if (character == null)
            return;
        
        Character charStats = character.GetComponent<Character>();
        charName.text = charStats.getCharName();
        charImage.sprite = character.GetComponent<SpriteRenderer>().sprite;
        charImage.transform.localScale = character.transform.localScale;

        weaponIMG.sprite = character.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite;
        weaponIMG.transform.localScale = charStats.transform.GetChild(1).localScale;
        charStats.updateCosts();

        if (bodyDropdown.options.Count == 0)
        {
            string[] bodyNames = Enum.GetNames(typeof(Character.bodyType));
            List<string> body = new List<string>(bodyNames);
            bodyDropdown.AddOptions(body);
        }
        else
        {
            bodyDropdown.value = (int)charStats.GetBodyType();
        }

        if (weaponDropdown.options.Count == 0)
        {
            List<string> weapons = new List<string>();

            for (int i = 0; i < weaponSprites.transform.childCount; i++)
            {
                weapons.Add(weaponSprites.transform.GetChild(i).name);
            }

            weaponDropdown.AddOptions(weapons);
        }
        else
        {
            weaponDropdown.value = (int)charStats.GetWeaponType();
        }

        hpNUM.text = "" + charStats.baseHP;
        strNUM.text = "" + charStats.baseSTR;
        magNUM.text = "" + charStats.baseMAG;
        defNUM.text = "" + charStats.baseDEF;
        resNUM.text = "" + charStats.baseRES;
        spdNUM.text = "" + charStats.baseSPD;
        movNUM.text = "" + charStats.baseMOV;

        weaponRange.text = "RNG: " + charStats.getAttackRange();
        weaponStats1.text = "";
        weaponStats2.text = "";

        if (charStats.HPMOD == 0 && charStats.STRMOD == 0 && charStats.MAGMOD == 0 && charStats.DEFMOD == 0
            && charStats.RESMOD == 0 && charStats.SPDMOD == 0 && charStats.MOVMOD == 0)
        {
            //weaponStatsPanel.gameObject.SetActive(false);
            weaponStats1.text = "";
            weaponStats2.text = "";
        }

        if (charStats.HPMOD == 0)
            hpMOD.text = "";
        else if (charStats.HPMOD > 0)
        {
            hpMOD.color = Color.green;
            hpMOD.text = "+ " + charStats.HPMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            weaponStats1.color = Color.green;
            weaponStats1.text = "+" + charStats.HPMOD + " HP";
        }
        else
        {
            hpMOD.color = Color.red;
            hpMOD.text = "- " + Mathf.Abs(charStats.HPMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            weaponStats1.color = Color.red;
            weaponStats1.text = "-" + Mathf.Abs(charStats.HPMOD) + " HP";
        }

        if (charStats.STRMOD == 0)
            strMOD.text = "";
        else if (charStats.STRMOD > 0)
        {
            strMOD.color = Color.green;
            strMOD.text = "+ " + charStats.STRMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.STRMOD + " STR";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.STRMOD + " STR";
            }
        }
        else
        {
            strMOD.color = Color.red;
            strMOD.text = "- " + Mathf.Abs(charStats.STRMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.STRMOD) + " STR";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.STRMOD) + " STR";
            }
        }

        if (charStats.MAGMOD == 0)
            magMOD.text = "";
        else if (charStats.MAGMOD > 0)
        {
            magMOD.color = Color.green;
            magMOD.text = "+ " + charStats.MAGMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.MAGMOD + " MAG";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.MAGMOD + " MAG";
            }
        }
        else
        {
            magMOD.color = Color.red;
            magMOD.text = "- " + Mathf.Abs(charStats.MAGMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.MAGMOD) + " MAG";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.MAGMOD) + " MAG";
            }
        }

        if (charStats.DEFMOD == 0)
            defMOD.text = "";
        else if (charStats.DEFMOD > 0)
        {
            defMOD.color = Color.green;
            defMOD.text = "+ " + charStats.DEFMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.DEFMOD + " DEF";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.DEFMOD + " DEF";
            }
        }
        else
        {
            defMOD.color = Color.red;
            defMOD.text = "- " + Mathf.Abs(charStats.DEFMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.DEFMOD) + " DEF";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.DEFMOD) + " DEF";
            }
        }

        if (charStats.RESMOD == 0)
            resMOD.text = "";
        else if (charStats.RESMOD > 0)
        {
            resMOD.color = Color.green;
            resMOD.text = "+ " + charStats.RESMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.RESMOD + " RES";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.RESMOD + " RES";
            }
        }
        else
        {
            resMOD.color = Color.red;
            resMOD.text = "- " + Mathf.Abs(charStats.RESMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.RESMOD) + " RES";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.RESMOD) + " RES";
            }
        }

        if (charStats.SPDMOD == 0)
            spdMOD.text = "";
        else if (charStats.SPDMOD > 0)
        {
            spdMOD.color = Color.green;
            spdMOD.text = "+ " + charStats.SPDMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.SPDMOD + " SPD";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.SPDMOD + " SPD";
            }
        }
        else
        {
            spdMOD.color = Color.red;
            spdMOD.text = "- " + Mathf.Abs(charStats.SPDMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.SPDMOD) + " SPD";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.SPDMOD) + " SPD";
            }
        }

        if (charStats.MOVMOD == 0)
            movMOD.text = "";
        else if (charStats.MOVMOD > 0)
        {
            movMOD.color = Color.green;
            movMOD.text = "+ " + charStats.MOVMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.MOVMOD + " MOV";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.MOVMOD + " MOV";
            }
        }
        else
        {
            movMOD.color = Color.red;
            movMOD.text = "- " + Mathf.Abs(charStats.MOVMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.MOVMOD) + " MOV";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.MOVMOD) + " MOV";
            }
        }

        if (charStats.baseHP < charStats.getHPMAX())
        {
            hpButton.gameObject.SetActive(true);
            hpCOST.text = "x" + charStats.HPCost;
        }
        else
        {
            hpCOST.text = "MAX";
            hpButton.gameObject.SetActive(false);
        }


        if (charStats.baseSTR < charStats.getSTRMAX())
        {
            strButton.gameObject.SetActive(true);
            strCOST.text = "x" + charStats.STRCost;
        }        
        else
        {
            strCOST.text = "MAX";
            strButton.gameObject.SetActive(false);
        }

        if (charStats.baseMAG < charStats.getMAGMAX())
        {
            magButton.gameObject.SetActive(true);
            magCOST.text = "x" + charStats.MAGCost;
        }
        else
        {
            magCOST.text = "MAX";
            magButton.gameObject.SetActive(false);
        }

        if (charStats.baseDEF < charStats.getDEFMAX())
        {
            defButton.gameObject.SetActive(true);
            defCOST.text = "x" + charStats.DEFCost;
        }   
        else
        {
            defCOST.text = "MAX";
            defButton.gameObject.SetActive(false);
        }

        if (charStats.baseRES < charStats.getRESMAX())
        {
            resButton.gameObject.SetActive(true);
            resCOST.text = "x" + charStats.RESCost;
        } 
        else
        {
            resCOST.text = "MAX";
            resButton.gameObject.SetActive(false);
        }

        if (charStats.baseSPD < charStats.getSPDMAX())
        {
            spdButton.gameObject.SetActive(true);
            spdCOST.text = "x" + charStats.SPDCost;
        }       
        else
        {
            spdCOST.text = "MAX";
            spdButton.gameObject.SetActive(false);
        }

        if (charStats.baseMOV < charStats.getMOVMAX())
        {
            movButton.gameObject.SetActive(true);
            movCOST.text = "x" + charStats.MOVCost;
        }         
        else
        {
            movCOST.text = "MAX";
            movButton.gameObject.SetActive(false);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void hpButtonPressedServerRpc(ServerRpcParams serverRpcParams)
    {
        if (serverRpcParams.Receive.SenderClientId == (ulong)player1)
        {
            if (p1GearAmount >= p1TargetedStats.HPCost && p1TargetedStats.baseHP < p1TargetedStats.getHPMAX())
            {
                giveGearNum(-p1TargetedStats.HPCost,false);
                p1TargetedStats.baseHP = p1TargetedStats.baseHP + 1;
                p1TargetedStats.hpLeft = p1TargetedStats.hpLeft + 1;
                p1TargetedStats.updateStats();
                passHpStatClientRpc(p1Targeted.name,p1TargetedStats.hpLeft,p1TargetedStats.baseHP);
                updateUpgradeMenuServerRpc(p1Targeted.name);
            }
        }
        else if (serverRpcParams.Receive.SenderClientId == (ulong)player2)
        {
            if (p2GearAmount >= p2TargetedStats.HPCost && p2TargetedStats.baseHP < p2TargetedStats.getHPMAX())
            {
                giveGearNum(-p2TargetedStats.HPCost,true);
                p2TargetedStats.baseHP = p2TargetedStats.baseHP + 1;
                p2TargetedStats.hpLeft = p2TargetedStats.hpLeft + 1;
                p2TargetedStats.updateStats();
                passHpStatClientRpc(p2Targeted.name,p2TargetedStats.hpLeft,p2TargetedStats.baseHP);
                updateUpgradeMenuServerRpc(p2Targeted.name);
            }
        }
    }
    
    public void strButtonPressed()
    {
        strButtonPressedServerRpc(new ServerRpcParams());
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
        {
            updateUpgradeMenuServerRpc(p1Targeted.name);
            
        }
        else if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
        {
            updateUpgradeMenuServerRpc(p2Targeted.name);
        }
    }
    

    [ServerRpc(RequireOwnership = false)]
    public void strButtonPressedServerRpc(ServerRpcParams serverRpcParams)
    {
        if (serverRpcParams.Receive.SenderClientId == (ulong)player1)
        {
            if (p1GearAmount >= p1TargetedStats.STRCost && p1TargetedStats.baseSTR < p1TargetedStats.getSTRMAX())
            {
                giveGearNum(-p1TargetedStats.STRCost,false);
                p1TargetedStats.baseSTR = p1TargetedStats.baseSTR + 1;
                p1TargetedStats.updateStats();
                passStrClientRpc(p1Targeted.name,p1TargetedStats.baseSTR);
                updateUpgradeMenuServerRpc(p1Targeted.name);
            }
        }
        else if (serverRpcParams.Receive.SenderClientId == (ulong)player2)
        {
            if (p2GearAmount >= p2TargetedStats.STRCost && p2TargetedStats.baseSTR < p2TargetedStats.getSTRMAX())
            {
                giveGearNum(-p2TargetedStats.STRCost,true);
                p2TargetedStats.baseSTR = p2TargetedStats.baseSTR + 1;
                p2TargetedStats.updateStats();
                passStrClientRpc(p2Targeted.name,p2TargetedStats.baseSTR);
                updateUpgradeMenuServerRpc(p2Targeted.name);
            }
        }
    }
    
    public void magButtonPressed()
    {
        magButtonPressedServerRpc(new ServerRpcParams());
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
        {
            updateUpgradeMenuServerRpc(p1Targeted.name);
            
        }
        else if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
        {
            updateUpgradeMenuServerRpc(p2Targeted.name);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void magButtonPressedServerRpc(ServerRpcParams serverRpcParams)
    {
        if (serverRpcParams.Receive.SenderClientId == (ulong)player1)
        {
            if (p1GearAmount >= p1TargetedStats.MAGCost && p1TargetedStats.baseMAG < p1TargetedStats.getMAGMAX())
            {
                giveGearNum(-p1TargetedStats.MAGCost,false);
                p1TargetedStats.baseMAG = p1TargetedStats.baseMAG + 1;
                p1TargetedStats.updateStats();
                passMagClientRpc(p1Targeted.name,p1TargetedStats.baseMAG);
                updateUpgradeMenuServerRpc(p1Targeted.name);

            }
        }
        else if (serverRpcParams.Receive.SenderClientId == (ulong)player2)
        {
            if (p2GearAmount >= p2TargetedStats.MAGCost && p2TargetedStats.baseMAG < p2TargetedStats.getMAGMAX())
            {
                giveGearNum(-p2TargetedStats.MAGCost,true);
                p2TargetedStats.baseMAG = p2TargetedStats.baseMAG + 1;
                p2TargetedStats.updateStats();
                passMagClientRpc(p2Targeted.name,p2TargetedStats.baseMAG);
                updateUpgradeMenuServerRpc(p2Targeted.name);
            }
        }
    }
    
    public void defButtonPressed()
    {
        defButtonPressedServerRpc(new ServerRpcParams());
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
        {
            updateUpgradeMenuServerRpc(p1Targeted.name);
            
        }
        else if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
        {
            updateUpgradeMenuServerRpc(p2Targeted.name);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void defButtonPressedServerRpc(ServerRpcParams serverRpcParams)
    {
        if (serverRpcParams.Receive.SenderClientId == (ulong)player1)
        {
            if (p1GearAmount >= p1TargetedStats.DEFCost && p1TargetedStats.baseDEF < p1TargetedStats.getDEFMAX())
            {
                giveGearNum(-p1TargetedStats.DEFCost,false);
                p1TargetedStats.baseDEF = p1TargetedStats.baseDEF + 1;
                p1TargetedStats.updateStats();
                passDefClientRpc(p1Targeted.name,p1TargetedStats.baseDEF);
                updateUpgradeMenuServerRpc(p1Targeted.name);

            }
        }
        else if (serverRpcParams.Receive.SenderClientId == (ulong)player2)
        {
            if (p2GearAmount >= p2TargetedStats.DEFCost && p2TargetedStats.baseDEF < p2TargetedStats.getDEFMAX())
            {
                giveGearNum(-p2TargetedStats.DEFCost,true);
                p2TargetedStats.baseDEF = p2TargetedStats.baseDEF + 1;
                p2TargetedStats.updateStats();
                passDefClientRpc(p2Targeted.name,p2TargetedStats.baseDEF);
                updateUpgradeMenuServerRpc(p2Targeted.name);

            }
        }
    }
    public void resButtonPressed()
    {
        resButtonPressedServerRpc(new ServerRpcParams());
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
        {
            updateUpgradeMenuServerRpc(p1Targeted.name);
            
        }
        else if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
        {
            updateUpgradeMenuServerRpc(p2Targeted.name);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void resButtonPressedServerRpc(ServerRpcParams serverRpcParams)
    {
        if (serverRpcParams.Receive.SenderClientId == (ulong)player1)
        {
            if (p1GearAmount >= p1TargetedStats.RESCost && p1TargetedStats.baseRES < p1TargetedStats.getRESMAX())
            {
                giveGearNum(-p1TargetedStats.RESCost,false);
                p1TargetedStats.baseRES = p1TargetedStats.baseRES + 1;
                p1TargetedStats.updateStats();
                passResClientRpc(p1Targeted.name,p1TargetedStats.baseRES);
                updateUpgradeMenuServerRpc(p1Targeted.name);
            }
        }
        else if (serverRpcParams.Receive.SenderClientId == (ulong)player2)
        {
            if (p2GearAmount >= p2TargetedStats.RESCost && p2TargetedStats.baseRES < p2TargetedStats.getRESMAX())
            {
                giveGearNum(-p2TargetedStats.RESCost,true);
                p2TargetedStats.baseRES = p2TargetedStats.baseRES + 1;
                p2TargetedStats.updateStats();
                passResClientRpc(p2Targeted.name,p2TargetedStats.baseRES);
                updateUpgradeMenuServerRpc(p2Targeted.name);
            }
        }
    }
    
    public void spdButtonPressed()
    {
        spdButtonPressedServerRpc(new ServerRpcParams());
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
        {
            updateUpgradeMenuServerRpc(p1Targeted.name);
        }
        else if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
        {
            updateUpgradeMenuServerRpc(p2Targeted.name);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void spdButtonPressedServerRpc(ServerRpcParams serverRpcParams)
    {
        if (serverRpcParams.Receive.SenderClientId == (ulong)player1)
        {
            if (p1GearAmount >= p1TargetedStats.SPDCost && p1TargetedStats.SPDCost < p1TargetedStats.getSPDMAX())
            {
                giveGearNum(-p1TargetedStats.SPDCost,false);
                p1TargetedStats.baseSPD = p1TargetedStats.baseSPD + 1;
                p1TargetedStats.updateStats();
                passSpdClientRpc(p1Targeted.name,p1TargetedStats.baseSPD);
                updateUpgradeMenuServerRpc(p1Targeted.name);
            }
        }
        else if (serverRpcParams.Receive.SenderClientId == (ulong)player2)
        {
            if (p2GearAmount >= p2TargetedStats.SPDCost && p2TargetedStats.SPDCost < p2TargetedStats.getSPDMAX())
            {
                giveGearNum(-p2TargetedStats.SPDCost,true);
                p2TargetedStats.baseSPD = p2TargetedStats.baseSPD + 1;
                p2TargetedStats.updateStats();
                passSpdClientRpc(p2Targeted.name,p2TargetedStats.baseSPD);
                updateUpgradeMenuServerRpc(p2Targeted.name);
            }
        }
    }

    public void movButtonPressed()
    {
        movButtonPressedServerRpc(new ServerRpcParams());
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
        {
            updateUpgradeMenuServerRpc(p1Targeted.name);
        }
        else if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
        {
            updateUpgradeMenuServerRpc(p2Targeted.name);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void movButtonPressedServerRpc(ServerRpcParams serverRpcParams)
    {
        if (serverRpcParams.Receive.SenderClientId == (ulong)player1)
        {
            if (p1GearAmount >= p1TargetedStats.MOVCost && p1TargetedStats.baseMOV < p1TargetedStats.getMOVMAX())
            {
                giveGearNum(-p1TargetedStats.MOVCost,false);
                p1TargetedStats.baseMOV = p1TargetedStats.baseMOV + 1;
                p1TargetedStats.movLeft = p1TargetedStats.movLeft + 1;
                p1TargetedStats.updateStats(); 
                passMovStatClientRpc(p1Targeted.name,p1TargetedStats.movLeft,p1TargetedStats.baseMOV);
                updateUpgradeMenuServerRpc(p1Targeted.name);
            }
        }
        else if (serverRpcParams.Receive.SenderClientId == (ulong)player2)
        {
            if (p2GearAmount >= p2TargetedStats.MOVCost && p2TargetedStats.baseMOV < p2TargetedStats.getMOVMAX())
            {
                giveGearNum(-p2TargetedStats.MOVCost,true);
                p2TargetedStats.baseMOV = p2TargetedStats.baseMOV + 1;
                p2TargetedStats.movLeft = p2TargetedStats.movLeft + 1;
                p2TargetedStats.updateStats();          
                passMovStatClientRpc(p2Targeted.name,p2TargetedStats.movLeft,p2TargetedStats.baseMOV);
                updateUpgradeMenuServerRpc(p2Targeted.name);
            }
        }
    }
    [ClientRpc]
    public void p2UpgradeClientRpc()
    {
        
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
        {
            return;
        }
        contextMenu.SetActive(false);
        upgradeMenu.gameObject.SetActive(true);
        updateUpgradeMenuServerRpc(p2Targeted.name);

    }

    [ServerRpc(RequireOwnership = false)]
    public void attackActiveServerRpc(bool cond)
    {
        attackActive = cond;
    }

    public void highlightAttack()
    {
        
        attackActive = true;
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
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
        else if(NetworkManager.Singleton.LocalClientId == (ulong)player2)
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
        if ((int)serverRpcParams.Receive.SenderClientId == player1 && currTurnMode == turnMode.Player1Turn)
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
        if ((int)serverRpcParams.Receive.SenderClientId == player2 && currTurnMode == turnMode.Player2Turn)
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

    public mapFeatures checkOnCrate(Vector3 pos)
    {
        for (int i = 0; i < mapFeatures.transform.childCount; i++)
        {
            if (featuresFeatures[i].GetFeatureType() == global::mapFeatures.featureType.SupplyCache 
                && pos == featureUnits[i].transform.position)
            {
                return featuresFeatures[i];
            }
        }

        return null; 
    }

    [ClientRpc]
    public void collectCrateClientRpc(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
        {
            mapFeatures feature = obj.GetComponent<mapFeatures>();
            feature.getCollected();
        }
    }
    
    public IEnumerator movePathServer(List<PathNode> vectorPath, ServerRpcParams serverRpcParams)
    {
        if ((int)serverRpcParams.Receive.SenderClientId == player1 && currTurnMode == turnMode.Player1Turn)
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
                p1hideAreaClientRpc();
                //showArea(currTargeted);
                P1updateCharInfo();
                
                yield return new WaitForSeconds(delay);
            }

            mapFeatures maybeFeature = checkOnCrate(p1Targeted.transform.position);
            // check if unit ended turn on supply cache
            if (maybeFeature != null)
            {
                maybeFeature.getCollected();
                collectCrateClientRpc(maybeFeature.name);
                giveGearNum(crateGearNum, false);
            }
            
            changeMode(gameMode.MapMode);
            clickLock = 0;
            passClickLockClientRpc(0);

            moveActiveServerRpc(false);
            P1openContextMenuClientRpc(p1Targeted.transform.position);
        }

        if ((int)serverRpcParams.Receive.SenderClientId == player2 && currTurnMode == turnMode.Player2Turn)
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
                p2hideAreaClientRpc();
                //showArea(currTargeted);
                P2updateCharInfo();
                yield return new WaitForSeconds(delay);
            }
            
            mapFeatures maybeFeature = checkOnCrate(p2Targeted.transform.position);
            // check if unit ended turn on supply cache
            if (maybeFeature != null)
            {
                maybeFeature.getCollected();
                collectCrateClientRpc(maybeFeature.name);
                giveGearNum(crateGearNum, true);
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
            if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
            {
                return;
            }
        }
        // player 2
        else
        {
            if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
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

    [ClientRpc]
    public void passGameModeClientRpc(gameMode newGameMode)
    {
        currGameMode = newGameMode;
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
        for (int i = 0; i < p1.transform.childCount; i++)
        {
            p1Stats[i].resetMove();
            passMovStatClientRpc(p1Stats[i].name,p1Stats[i].movLeft,p1Stats[i].baseMOV);
        }
        
        for (int i = 0; i < p2.transform.childCount; i++)
        {
            p2Stats[i].resetMove();
            passMovStatClientRpc(p2Stats[i].name,p2Stats[i].movLeft,p2Stats[i].baseMOV);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void resetAllAttackServerRpc()
    {
        for (int i = 0; i < p1.transform.childCount; i++)
        {
            p1Stats[i].setAttack(true);
            passAtkStatClientRpc(p1Stats[i].name,p1Stats[i].getCanAttack());
        }
        
        for (int i = 0; i < p2.transform.childCount; i++)
        {
            p2Stats[i].setAttack(true);
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
        targetStats.updateStats();
        targetStats.updateCosts();
    }
    
    [ClientRpc]
    public void passStrClientRpc(string name,int str)
    {
        GameObject targetUnit = GameObject.Find(name);
        Character targetStats = targetUnit.GetComponent<Character>();
        targetStats.baseSTR = str;
        targetStats.updateStats();
        targetStats.updateCosts();

    }
        
    [ClientRpc]
    public void passMagClientRpc(string name,int num)
    {
        GameObject targetUnit = GameObject.Find(name);
        Character targetStats = targetUnit.GetComponent<Character>();
        targetStats.baseMAG= num;
        targetStats.updateStats();
        targetStats.updateCosts();

    }
    [ClientRpc]
    public void passDefClientRpc(string name,int num)
    {
        GameObject targetUnit = GameObject.Find(name);
        Character targetStats = targetUnit.GetComponent<Character>();
        targetStats.baseDEF= num;
        targetStats.updateStats();
        targetStats.updateCosts();

    }
    
    [ClientRpc]
    public void passResClientRpc(string name,int num)
    {
        GameObject targetUnit = GameObject.Find(name);
        Character targetStats = targetUnit.GetComponent<Character>();
        targetStats.baseRES= num;
        targetStats.updateStats();
        targetStats.updateCosts();

    }
    [ClientRpc]
    public void passSpdClientRpc(string name,int num)
    {
        GameObject targetUnit = GameObject.Find(name);
        Character targetStats = targetUnit.GetComponent<Character>();
        targetStats.baseSPD= num;
        targetStats.updateStats();
        targetStats.updateCosts();

    }
    
    [ClientRpc]
    public void passAtkStatClientRpc(string name, bool canAttack)
    {
        GameObject targetUnit = GameObject.Find(name);
        Character targetStats = targetUnit.GetComponent<Character>();
        targetStats.setAttack(canAttack);
        targetStats.updateStats();
        targetStats.updateCosts();
    }
    
    [ClientRpc]
    public void passHpStatClientRpc(string name, int hpleft, int hp)
    {
        GameObject targetUnit = GameObject.Find(name);
        Character targetStats = targetUnit.GetComponent<Character>();
        targetStats.hpLeft = hpleft;
        targetStats.baseHP = hp;
        targetStats.updateStats();
        targetStats.updateCosts();
    }
    
    [ClientRpc]
    public void passGearNumberClientRpc(int p1G, int p2G)
    {
        p1GearAmount = p1G;
        p2GearAmount = p2G;
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
    
    
    public void updateBattleStats(GameObject lChar, GameObject rChar)
    {
        Character leftStats = lChar.GetComponent<Character>();
        Character rightStats = rChar.GetComponent<Character>();
        
        Debug.Log("lName: " + lChar.name + ", rName: " + rChar.name);
        
        LcharNameTXT.text = "Name: " + leftStats.getCharName();
        LhpNUM.text = "" + leftStats.hpLeft + " / " + leftStats.HP;
        LstrNUM.text = "" + leftStats.STR;
        LmagNUM.text = "" + leftStats.MAG;
        LdefNUM.text = "" + leftStats.DEF;
        LresNUM.text = "" + leftStats.RES;
        LspdNUM.text = "" + leftStats.SPD;
        LmovNUM.text = "" + leftStats.MOV; 
        LmovLeftTXT.SetActive(false);
        LmovLeftNUMObj.SetActive(false);

        RcharNameTXT.text = "Name: " + rightStats.getCharName();
        RhpNUM.text = "" + rightStats.hpLeft + " / " + rightStats.HP;
        RstrNUM.text = "" + rightStats.STR;
        RmagNUM.text = "" + rightStats.MAG;
        RdefNUM.text = "" + rightStats.DEF;
        RresNUM.text = "" + rightStats.RES;
        RspdNUM.text = "" + rightStats.SPD;
        RmovNUM.text = "" + rightStats.MOV;
        RmovLeftTXT.SetActive(false);
        RmovLeftNUMObj.SetActive(false);
        
        passBattleStatsClientRpc(lChar.name, rChar.name);
    }

    [ClientRpc]
    private void passBattleStatsClientRpc(string lName, string rName)
    {
        Debug.Log("lName: " + lName + ", rName: " + rName);
        Character leftStats = GameObject.Find(lName).GetComponent<Character>();
        Character rightStats = GameObject.Find(rName).GetComponent<Character>();
        
        LcharNameTXT.text = "Name: " + leftStats.getCharName();
        LhpNUM.text = "" + leftStats.hpLeft + " / " + leftStats.HP;
        LstrNUM.text = "" + leftStats.STR;
        LmagNUM.text = "" + leftStats.MAG;
        LdefNUM.text = "" + leftStats.DEF;
        LresNUM.text = "" + leftStats.RES;
        LspdNUM.text = "" + leftStats.SPD;
        LmovNUM.text = "" + leftStats.MOV; 
        LmovLeftTXT.SetActive(false);
        LmovLeftNUMObj.SetActive(false);

        RcharNameTXT.text = "Name: " + rightStats.getCharName();
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
            copyPositionClientRpc(theObject.name, theObject.transform.position);
            yield return null;
        }

        time = 0;   
        theObject.transform.position = targetPosition;

        while (time < duration)
        {
            theObject.transform.position = Vector2.Lerp(targetPosition, startPosition, time / duration);
            time += Time.deltaTime;
            copyPositionClientRpc(theObject.name, theObject.transform.position);
            yield return null;
        }

        theObject.transform.position = startPosition;
    }
    
      
    // false == left hurt, true == right hurt
    public IEnumerator updateDamageTXT(GameObject unitHurt, int damageNum)
    {
        damageTXTPanel.SetActive(true);
        toggleDamageTXTClientRpc(true);
        damageTXTPanel.transform.position = mainCamera.WorldToScreenPoint(unitHurt.transform.position + damageTextOffset);

        // dead char attack number
        if (damageNum == -999)
            yield return null;
        else if (damageNum == 0)
        {
            damageTXT.color = Color.yellow;
            damageTXT.text = "(" + damageNum + ")";
            updateDamageTXTClientRpc(unitHurt.transform.position, damageTXT.text);
            
            yield return new WaitForSeconds(inbetweenAttackDelay * 3);

            damageTXT.text = "";
            updateDamageTXTClientRpc(unitHurt.transform.position, damageTXT.text);
        }
        else
        {
            damageTXT.color = Color.red;
            damageTXT.text = "(-" + damageNum + ")";
            updateDamageTXTClientRpc(unitHurt.transform.position, damageTXT.text);

            yield return new WaitForSeconds(inbetweenAttackDelay * 3);

            damageTXT.text = "";
            updateDamageTXTClientRpc(unitHurt.transform.position, damageTXT.text);
        }

        damageTXTPanel.SetActive(false);
        toggleDamageTXTClientRpc(false);
    }

    [ClientRpc]
    private void toggleDamageTXTClientRpc(bool cond)
    {
        damageTXTPanel.SetActive(cond);
    }
    
    [ClientRpc]
    private void updateDamageTXTClientRpc(Vector3 pos, string text)
    {
        damageTXTPanel.transform.position = mainCamera.WorldToScreenPoint(pos + damageTextOffset);
        damageTXT.text = text;
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
            takeDamageClientRpc(damageMinusDefense, damageTaker.name);
        }
        // attacker has magic weapon
        else
        {
            damageMinusDefense = attacker.MAG - damageTaker.RES;
            // make sure you cant do negative damage
            if (damageMinusDefense < 0)
                damageMinusDefense = 0;

            damageTaker.takeDamage(damageMinusDefense);
            takeDamageClientRpc(damageMinusDefense, damageTaker.name);
        }

        // all player characters dead
        if (p1allDead()) 
        {
            //Debug.Log("All allies dead you lose");
            p2VictoryClientRpc();
        }
        // all enemy characters dead
        else if (p2allDead())
        {
            //Debug.Log("All enemies dead you win");
            p1VictoryClientRpc();
        }

        return damageMinusDefense;
    }
    
    [ClientRpc]
    private void p1VictoryClientRpc()
    {
        p1Victory.SetActive(true);
        Mapmode.SetActive(false);
        turnPanel.SetActive(false);
        gearNumPanel.SetActive(false);
        charInfoPanel.SetActive(false);
        charInfoPanelR.SetActive(false);
    }

    [ClientRpc]
    private void p2VictoryClientRpc()
    {
        p2Victory.SetActive(true);
        Mapmode.SetActive(false);
        turnPanel.SetActive(false);
        gearNumPanel.SetActive(false);
        charInfoPanel.SetActive(false);
        charInfoPanelR.SetActive(false);
    }
    

    [ClientRpc]
    public void takeDamageClientRpc(int damagenumber, string name)
    {
        if (NetworkManager.Singleton.LocalClientId == 0)
            return;
        
        GameObject target = GameObject.Find(name);
        Character targetStats = target.GetComponent<Character>();
        targetStats.takeDamage(damagenumber);
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
    
    public void giveGearNum(int amount, bool player)
    {
        if (!player)
        {
            p1GearAmount = p1GearAmount + amount;
        }
        else
        {
            p2GearAmount = p2GearAmount + amount;
        }
        
        passGearNumberClientRpc(p1GearAmount, p2GearAmount);
        updateGearNumPanelClientRpc();
    }
    
    [ClientRpc]
    public void updateGearNumPanelClientRpc()
    {
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
        {
            gearNumPanel.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + p1GearAmount;
        }
        else if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
        {
            gearNumPanel.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + p2GearAmount;
        }
    }

    public IEnumerator plusAnimation()
    {
        float time = 0;
        RawImage rI = gearNumPlus.GetComponent<RawImage>();
        Vector3 originalPos = gearNumPlus.transform.position;
        gearNumPlus.SetActive(true);
        togglePlusClientRpc(true);
        if (!NetworkManager.IsClient)
            plusAnimationCLientRpc();
        while (time <= 0.2f)
        {
            gearNumPlus.transform.position = gearNumPlus.transform.position + new Vector3(0, 0.25f, 0);
            if (time > 0.1f)
                rI.color = new Color(1, 1, 1, rI.color.a * 0.80f);
            yield return new WaitForSeconds(0.01f);
            time = time + Time.smoothDeltaTime;
            //Debug.Log("time: " + time);
        }

        gearNumPlus.transform.position = originalPos;
        rI.color = new Color(1, 1, 1, 1);
        gearNumPlus.SetActive(false);
        togglePlusClientRpc(false);
    }

    [ClientRpc]
    public void plusAnimationCLientRpc()
    {
        StartCoroutine(plusAnimation()); 
        return;
    }
    [ClientRpc]
    public void togglePlusClientRpc(bool toogle)
    {
        gearNumPlus.SetActive(toogle);
    }
    
    public void deselectButtonPressed()
    {
        deselectButtonPressedServerRpc(new ServerRpcParams());
    }

    [ServerRpc(RequireOwnership = false)]

    public void deselectButtonPressedServerRpc(ServerRpcParams serverRpcParams)
    {
        deselectTargetServerRpc(serverRpcParams); 
        //contextMenu.SetActive(false);
    }

    public void inspectButtonPressed()
    {
        inspectButtonPressedServerRpc(new ServerRpcParams());
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void inspectButtonPressedServerRpc(ServerRpcParams serverRpcParams)
    {
        if (serverRpcParams.Receive.SenderClientId == (ulong)player1)
        {
            charInfoPanel.gameObject.SetActive(true);
            P1updateCharInfo();
            p1hideAreaClientRpc();
            p1showAreaClientRpc(p1Targeted.name);
            contextMenu.SetActive(false);
        }
        else if (serverRpcParams.Receive.SenderClientId == (ulong)player2)
        {
            charInfoPanel.gameObject.SetActive(true);
            P2updateCharInfo();
            p2hideAreaClientRpc();
            p2showAreaClientRpc(p2Targeted.name);
            contextMenu.SetActive(false);
        }
    }
    
    [ClientRpc]
    public void p1hideAreaClientRpc()
    {
        if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
        {
            return;
        }
        for (int i = 0; i < moveAreas.Length; i++)
            moveAreas[i].SetActive(false);

        for (int i = 0; i < attackAreas.Length; i++)
            attackAreas[i].SetActive(false);
    }
    
    [ClientRpc]
    public void p2hideAreaClientRpc()
    {
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
        {
            return;
        }
        for (int i = 0; i < moveAreas.Length; i++)
            moveAreas[i].SetActive(false);

        for (int i = 0; i < attackAreas.Length; i++)
            attackAreas[i].SetActive(false);
    }
    
    [ClientRpc]
    public void p1showAreaClientRpc(String unitString)
    {
        if (NetworkManager.Singleton.LocalClientId == (ulong)player2)
        {
            return;
        }
        GameObject unit = GameObject.Find(unitString);
        Character unitStats = unit.GetComponent<Character>();

        // 1 Range
        if (unitStats.getAttackRange() == 1)
        {
            if (unitStats.movLeft < 0 || unitStats.movLeft > moveAreas.Length || unitStats.movLeft >= attackAreas.Length)
            {
                //Debug.Log("movLeft out of range in showArea!!!");
                p1hideAreaClientRpc();
                
            }
            else if (unitStats.movLeft == 0)
            {
                p1hideAreaClientRpc();

                attackAreas[unitStats.movLeft].SetActive(true);
                attackAreas[unitStats.movLeft].transform.position = unit.transform.position;
            }
            else
            {
                moveAreas[unitStats.movLeft - 1].SetActive(true);
                moveAreas[unitStats.movLeft - 1].transform.position = unit.transform.position;
                attackAreas[unitStats.movLeft].SetActive(true);
                attackAreas[unitStats.movLeft].transform.position = unit.transform.position;
            }
        }
        // 2 Range
        else if (unitStats.getAttackRange() == 2)
        {
            if (unitStats.movLeft > moveAreas.Length || unitStats.movLeft + 1 >= attackAreas.Length)
            {
                Debug.Log("movLeft out of range in showArea!!!");
                p1hideAreaClientRpc();

            }
            else if (unitStats.movLeft == 0)
            {
                p1hideAreaClientRpc();

                attackAreas[unitStats.movLeft + 1].SetActive(true);
                attackAreas[unitStats.movLeft + 1].transform.position = unit.transform.position;
                attackAreas[unitStats.movLeft].SetActive(true);
                attackAreas[unitStats.movLeft].transform.position = unit.transform.position;
            }
            else
            {
                moveAreas[unitStats.movLeft - 1].SetActive(true);
                moveAreas[unitStats.movLeft - 1].transform.position = unit.transform.position;
                attackAreas[unitStats.movLeft].SetActive(true);
                attackAreas[unitStats.movLeft].transform.position = unit.transform.position;
                attackAreas[unitStats.movLeft + 1].SetActive(true);
                attackAreas[unitStats.movLeft + 1].transform.position = unit.transform.position;
            }
        }
    }
    
    [ClientRpc]
    public void p2showAreaClientRpc(String unitString)
    {
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
        {
            return;
        }
        
        GameObject unit = GameObject.Find(unitString);
        Character unitStats = unit.GetComponent<Character>();

        // 1 Range
        if (unitStats.getAttackRange() == 1)
        {
            if (unitStats.movLeft < 0 || unitStats.movLeft > moveAreas.Length || unitStats.movLeft >= attackAreas.Length)
            {
                //Debug.Log("movLeft out of range in showArea!!!");
                p2hideAreaClientRpc();

            }
            else if (unitStats.movLeft == 0)
            {
                p2hideAreaClientRpc();
                attackAreas[unitStats.movLeft].SetActive(true);
                attackAreas[unitStats.movLeft].transform.position = unit.transform.position;
            }
            else
            {
                moveAreas[unitStats.movLeft - 1].SetActive(true);
                moveAreas[unitStats.movLeft - 1].transform.position = unit.transform.position;
                attackAreas[unitStats.movLeft].SetActive(true);
                attackAreas[unitStats.movLeft].transform.position = unit.transform.position;
            }
        }
        // 2 Range
        else if (unitStats.getAttackRange() == 2)
        {
            if (unitStats.movLeft > moveAreas.Length || unitStats.movLeft + 1 >= attackAreas.Length)
            {
                Debug.Log("movLeft out of range in showArea!!!");
                p2hideAreaClientRpc();            }
            else if (unitStats.movLeft == 0)
            {
                p2hideAreaClientRpc();          
                attackAreas[unitStats.movLeft + 1].SetActive(true);
                attackAreas[unitStats.movLeft + 1].transform.position = unit.transform.position;
                attackAreas[unitStats.movLeft].SetActive(true);
                attackAreas[unitStats.movLeft].transform.position = unit.transform.position;
            }
            else
            {
                moveAreas[unitStats.movLeft - 1].SetActive(true);
                moveAreas[unitStats.movLeft - 1].transform.position = unit.transform.position;
                attackAreas[unitStats.movLeft].SetActive(true);
                attackAreas[unitStats.movLeft].transform.position = unit.transform.position;
                attackAreas[unitStats.movLeft + 1].SetActive(true);
                attackAreas[unitStats.movLeft + 1].transform.position = unit.transform.position;
            }
        }
    }

    public void openJoinCodePanel()
    {
        joinCodePanel.SetActive(true);
        joinCodeL.SetActive(false);
        joinCodeR.SetActive(true);
    }

    public void closeJoinCodePanel()
    {
        joinCodePanel.SetActive(false);
        joinCodeL.SetActive(true);
        joinCodeR.SetActive(false);
    }

    public void copyJoinCodeButton()
    {
        Debug.Log("copied join code to clipboard");
        GUIUtility.systemCopyBuffer = joinCodeTXT.text;
    }

    public void openPauseMenu()
    {
        changeMode(gameMode.MenuMode);
        pauseMenu.SetActive(true);
    }

    public void closePauseMenu()
    {
        changeMode(gameMode.MapMode);
        pauseMenu.SetActive(false);
    }

    public void openTutorial()
    {
        SceneManager.LoadScene("TutorialScene", LoadSceneMode.Additive);
    }

    public void returnMainMenu()
    {
        SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
    }

    public void rematch()
    {
        SceneManager.LoadScene("NewLobby", LoadSceneMode.Additive);
        StartCoroutine(rematchcourotine());
    }

    private IEnumerator rematchcourotine()
    {
        yield return new WaitForSeconds(.25f);
        AuthenticateUI.Instance.Hide();
        LobbySelect.Instance.Hide();
        LobbyListUI.Instance.Hide();
        LobbyUI.Instance.Show();

        // player 1 is host
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
        {
            LobbyManager.Instance.CreateLobby("newLobby", true, lobbyData.getMap(), lobbyData.getUnits(), lobbyData.getSprings());
            yield return new WaitForSeconds(.5f);
            //passLobbyClientRpc(LobbyManager.Instance.GetJoinedLobby().LobbyCode);
        }

        yield return new WaitForSeconds(.25f);
        SceneManager.UnloadSceneAsync("MultiplayerScene");
    }

    [ClientRpc]
    private void passLobbyClientRpc(string lobbyCode)
    {
        if (NetworkManager.Singleton.LocalClientId == (ulong)player1)
            return;
        
        LobbyManager.Instance.JoinLobbyByCode(lobbyCode);
    }

}

