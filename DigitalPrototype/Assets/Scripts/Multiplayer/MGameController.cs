using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MGameController : MonoBehaviour
{
    // enum for whose turn it is currently, the players or the enemies.
    public enum turnMode { PlayerTurn, EnemyTurn };
    private turnMode currTurnMode;

    // enum for what the game is currently doing/displayhing, a menu, the map, or a battle. 
    public enum gameMode { MenuMode, MapMode, BattleMode };
    private gameMode currGameMode;

    public GameObject playerControllerObj = null;
    private MPlayerController playerController = null;
    public GameObject enemyControllerObj = null;
    private MPlayerController enemyController = null;
    public TMPro.TMP_Text turnModeTXT = null;
    public Button endTurnButton = null;

    // Start is called before the first frame update
    void Start()
    {
        playerController = playerControllerObj.GetComponent<MPlayerController>();
        enemyController = enemyControllerObj.GetComponent<MPlayerController>();
    }

    // Update is called once per frame
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

            // if player turn
            if (currTurnMode == turnMode.PlayerTurn)
            {
                if (playerController.ourTurn == false)
                {
                    playerController.ourTurn = true;
                }

                // give player back their end turn button
                endTurnButton.gameObject.SetActive(true);
            }
            // if enemy turn
            else
            {
                if (enemyController.ourTurn == false)
                {
                    enemyController.ourTurn = true;
                }

                // give player back their end turn button
                endTurnButton.gameObject.SetActive(true);
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
        if (currTurnMode == turnMode.PlayerTurn)
        {
            turnModeTXT.text = "Player 1 Turn";
        }
        else
        {
            turnModeTXT.text = "Player 2 Turn";
        }
    }

    public gameMode getGameMode()
    {
        return currGameMode;
    }

    public turnMode getTurnMode()
    {
        return currTurnMode;
    }

    public void endTurn()
    {

    }

    public void moveRpc()
    {

    }
}
