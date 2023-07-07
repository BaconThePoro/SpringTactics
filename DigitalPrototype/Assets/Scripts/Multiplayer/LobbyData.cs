using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyData : MonoBehaviour
{
    private LobbyManager.Map map = LobbyManager.Map.map1;
    private int startingSprings = 20;
    private int unitNumber = 2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
