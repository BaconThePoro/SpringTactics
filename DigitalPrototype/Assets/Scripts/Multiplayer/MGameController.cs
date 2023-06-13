using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Tilemaps;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using UnityEngine.SceneManagement;
using System;

public class MGameController : MonoBehaviour
{
    // enum for whose turn it is currently, the players or the enemies.
    public enum turnMode { Player1Turn, Player2Turn };
    private turnMode currTurnMode;

    // enum for what the game is currently doing/displayhing, a menu, the map, or a battle. 
    public enum gameMode { MenuMode, MapMode, BattleMode };
    private gameMode currGameMode;

    private TMPro.TMP_Text turnModeTXT = null;

    // p1 stuff
    private GameObject p1; 
    [System.NonSerialized] public GameObject[] p1Units;
    [System.NonSerialized] public Character[] p1Stats;
    public Vector3[] p1StartPos;
    public Character.bodyType[] p1bodysList;
    public Character.weaponType[] p1weaponsList;
    // p2 stuff
    private GameObject p2;
    [System.NonSerialized] public GameObject[] p2Units;
    [System.NonSerialized] public Character[] p2Stats;
    public Vector3[] p2StartPos;
    public Character.bodyType[] p2bodysList;
    public Character.weaponType[] p2weaponsList;

    void Start()
    {
        turnModeTXT = GameObject.Find("currentTurnTXT").GetComponent<TMPro.TextMeshProUGUI>();

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

            i += 1;
        }

        changeTurn(turnMode.Player1Turn);
        changeMode(gameMode.MapMode);
        updateTurnText();
    }

    void Update()
    {

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
        }
        else
        {
            turnModeTXT.text = "Player 2 Turn";
        }
    }

    public bool clickedAlly(Vector3Int mousePos)
    {
        for (int i = 0; i < p1Units.Length; i++)
        {
            // an ally was clicked
            if (mousePos == p1Units[i].transform.position && p1Stats[i].getIsDead() == false)
            {
                //deselectTarget();
                //targetAlly(i);
                //openContextMenu(mousePos);
                return true;
            }
        }

        return false;
    }
}
