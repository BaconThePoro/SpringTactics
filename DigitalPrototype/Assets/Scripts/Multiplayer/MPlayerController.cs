using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MPlayerController : MonoBehaviour
{

    public GameObject gameControllerObj = null;
    private MGameController gameController = null;
    private GameObject currTargeted = null;
    private Character currTargetedStats = null;
    public bool ourTurn = false;
    // context menu stuff
    public GameObject contextMenu = null;
    private Button moveButton = null;
    private Button attackButton = null;
    private Button upgradeButton = null;
    private Button inspectButton = null;
    private Button deselectButton = null;
    private Vector3 menuOffset = new Vector3(2.5f, -2f, 0);
    private Vector3 lastClickPos = Vector3.zero;
    // camera stuff
    // world limit is limit camera should be movable
    public GameObject mainCameraObj = null;
    private Camera mainCamera = null;
    public float worldLimPlusX = 18f;
    public float worldLimPlusY = 11f;
    public float worldLimMinusX = 18f;
    public float worldLimMinusY = 11f;
    public int springAmount = 0;
    float camMoveAmount = 0.02f;
    float targetZoom;
    float sensitivity = 1;
    float camSpeed = 3;
    float maxZoom = 11;
    float minZoom = 2;

    // Start is called before the first frame update
    void Start()
    {
        gameController = gameControllerObj.GetComponent<MGameController>();
        mainCamera = mainCameraObj.GetComponent<Camera>();
        // context menu
        moveButton = contextMenu.transform.GetChild(0).GetComponent<Button>();
        attackButton = contextMenu.transform.GetChild(1).GetComponent<Button>();
        upgradeButton = contextMenu.transform.GetChild(2).GetComponent<Button>();
        inspectButton = contextMenu.transform.GetChild(3).GetComponent<Button>();
        deselectButton = contextMenu.transform.GetChild(4).GetComponent<Button>();
    }
    
    // Update is called once per frame
    void Update()
    {
        // Map mode only
        if (gameController.getGameMode() == gameController.gameMode.MapMode)
        {
            // camera move up
            if (Input.GetKey(KeyCode.W) && mainCamera.transform.position.y < worldLimPlusY)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + camMoveAmount, mainCamera.transform.position.z);
            }
            // camera move down
            if (Input.GetKey(KeyCode.S) && mainCamera.transform.position.y > worldLimMinusY)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y - camMoveAmount, mainCamera.transform.position.z);
            }
            // camera move left
            if (Input.GetKey(KeyCode.A) && mainCamera.transform.position.x > worldLimMinusX)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x - camMoveAmount, mainCamera.transform.position.y, mainCamera.transform.position.z);
            }
            // camera move right
            if (Input.GetKey(KeyCode.D) && mainCamera.transform.position.x < worldLimPlusX)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x + camMoveAmount, mainCamera.transform.position.y, mainCamera.transform.position.z);
            }

            targetZoom -= Input.mouseScrollDelta.y * sensitivity;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            float newSize = Mathf.MoveTowards(mainCamera.orthographicSize, targetZoom, camSpeed * Time.deltaTime);
            mainCamera.orthographicSize = newSize;

            // mouse hover over effect
            Vector3Int mousePos = GetMousePosition();

            // adjust pos to z -5, see below
            mousePos = new Vector3Int(mousePos.x, mousePos.y, -6);

            if (!mousePos.Equals(previousMousePos))
            {
                // select layer is z: -6 (cause I randomly decided)
                Vector3Int mousePosZHover = new Vector3Int(mousePos.x, mousePos.y, -6);
                currTilemap.SetTile(previousMousePos, null); // Remove old hoverTile
                currTilemap.SetTile(mousePosZHover, hoverTile);
                previousMousePos = mousePos;
            }
        }

        if (ourTurn)
        {
            // if user presses Q click, end turn
            if (Input.GetKey(KeyCode.Q) && currTurnMode == turnMode.PlayerTurn)
            {
                endTurnButtonPressed();
            }

            // if player left clicks
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    //Debug.Log("Clicked UI, ignoring");
                    return;
                }

                // adjust to z level for units
                Vector3Int mousePos = GetMousePosition();
                mousePos = new Vector3Int(mousePos.x, mousePos.y, 0);

                // checking if an ally was clicked
                for (int i = 0; i < playerUnits.Length; i++)
                {
                    // an ally was clicked
                    if (mousePos == playerUnits[i].transform.position && playerStats[i].getIsDead() == false)
                    {
                        //deselectTarget();
                        //targetAlly(i);
                        //openContextMenu(mousePos);
                        return;
                    }
                }

/*                // checking if an enemy was clicked
                for (int i = 0; i < enemyController.enemyUnits.Length; i++)
                {
                    // an enemy was clicked
                    if (mousePos == enemyController.enemyUnits[i].transform.position && enemyController.enemyStats[i].getIsDead() == false)
                    {
                        // attackRing is active
                        if (attackActive == true)
                        {
                            if (currTargetedStats.getCanAttack() == true &&
                                inAttackRange(Vector3Int.FloorToInt(enemyController.enemyUnits[i].transform.position), currTargeted) == true)
                            {
                                beginBattle(i);
                                return;
                            }
                        }

                        deselectTarget();
                        targetEnemy(i);
                        openContextMenu(mousePos);
                        return;
                    }
                }

                // player clicked move
                if (moveActive == true)
                {
                    List<PathNode> vectorPath = new List<PathNode>();
                    vectorPath = pathfinding.FindPath((int)currTargeted.transform.position.x, (int)currTargeted.transform.position.y,
                    mousePos.x, mousePos.y, currTargetedStats.movLeft);

                    // valid moveable path
                    if (vectorPath != null)
                    {
                        //ourTurn = false;
                        StartCoroutine(movePath(vectorPath));
                        pathfinding.resetCollision();
                    }
                    // invalid path 
                    else
                    {
                        deselectTarget();
                    }

                    // unhighlight moveTiles
                    overlapGrid.ClearAllTiles();
                    moveActive = false;
                    return;
                }

                // clicked nothing
                deselectTarget();
                contextMenu.SetActive(false);*/
            }
        }
    }

    // for hovering effect
    Vector3Int GetMousePosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return currGrid.WorldToCell(mouseWorldPos);
    }

    public void endTurnButtonPressed()
    {

    }

/*    public void openContextMenu(Vector3 mousePos)
    {
        contextMenu.SetActive(true);

        menuOffset = new Vector3(Screen.width * 0.11f, -Screen.height * 0.16f, 0);
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
    }

    public void moveButtonPressed()
    {
        moveActive = true;
        attackActive = false;

        // call move RPC

        List<PathNode> vectorPath = new List<PathNode>();
        Vector3Int currPos = Vector3Int.FloorToInt(currTargeted.transform.position);
        int currMov = currTargetedStats.movLeft;

        //Debug.Log("movLeft: " + currMov + "   currPos: " + currPos.x + ", " + currPos.y);

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
                    overlapGrid.SetTile(newPos, moveTile);
                }
            }
        }

        contextMenu.SetActive(false);
    }*/
}
