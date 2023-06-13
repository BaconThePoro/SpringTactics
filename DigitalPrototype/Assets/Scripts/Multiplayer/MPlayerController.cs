using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.Netcode;

public class MPlayerController : NetworkBehaviour
{
    private GameObject gameControllerObj = null;
    private MGameController gameController = null;
    private Grid currGrid = null;

    // Start is called before the first frame update
    void Start()
    {
        gameControllerObj = GameObject.Find("GameController");
        gameController = gameControllerObj.GetComponent<MGameController>();
        currGrid = GameObject.Find("Grid").gameObject.GetComponent<Grid>();
    }

    // Update is called once per frame
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

    Vector3Int GetMousePosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return currGrid.WorldToCell(mouseWorldPos);
    }

    [ServerRpc]
    public void clickedAllyServerRpc(Vector3Int mousePos, ServerRpcParams serverRpcParams)
    {
        Debug.Log("request from client " + serverRpcParams.Receive.SenderClientId);
        return;
    }
}

