using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyData : MonoBehaviour
{
    public static LobbyData Instance { get; private set; }
    
    private string p1Name;
    private string p2Name; 
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

    public string getP1Name()
    {
        return p1Name;
    }
    
    public string getP2Name()
    {
        return p2Name;
    }
    
    public void setP1Name(string newName)
    {
        p1Name = newName;
    }
    
    public void setP2Name(string newName)
    {
        p2Name = newName;
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
