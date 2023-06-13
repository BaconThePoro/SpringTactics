using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MPlayerController : MonoBehaviour
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
            bool clickedAlly = gameController.clickedAlly(mousePos);
            Debug.Log("clickedAlly: " + clickedAlly);
        }
    }

    Vector3Int GetMousePosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return currGrid.WorldToCell(mouseWorldPos);
    }
}

