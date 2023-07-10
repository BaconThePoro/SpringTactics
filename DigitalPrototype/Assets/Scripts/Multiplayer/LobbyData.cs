using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyData : MonoBehaviour
{
    public static LobbyData Instance { get; private set; }
    
    private string playerName;
    private LobbyManager.Map map = LobbyManager.Map.map1;
    private int startingSprings = 20;
    private int unitNumber = 2;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string getName()
    {
        return playerName;
    }

    public void setName(string newName)
    {
        playerName = newName;
    }

    public LobbyManager.Map getMap()
    {
        return map;
    }

    public void setMap(LobbyManager.Map newMap)
    {
        map = newMap;
    }
    
    public int getSprings()
    {
        return startingSprings;
    }

    public void setSprings(int newSprings)
    {
        startingSprings = newSprings;
    }
    
    public int getUnits()
    {
        return unitNumber;
    }
    
    public void setUnits(int newUnits)
    {
        unitNumber = newUnits;
    }
}
