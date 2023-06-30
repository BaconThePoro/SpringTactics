using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class mapFeatures : MonoBehaviour
{

    public enum featureType{ SupplyCache, Placeholder };
    private featureType currFeatureType; 
    private GameObject featureSprites = null;
    private bool isActive = false; 
    
    // Start is called before the first frame update
    void Start()
    {
        featureSprites = GameObject.Find("FeatureSprites");
        setActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool getActive()
    {
        return isActive;
    }

    public void setActive(bool b)
    {
        isActive = b;
    }
    
    public featureType GetFeatureType()
    {
        return currFeatureType;
    }
    
    public void changeFeature(featureType choice)
    {
        currFeatureType = choice;
        setFeatureVisuals();
    }
    
    public void setFeatureVisuals()
    {
        GameObject featurePrefab = featureSprites.transform.GetChild((int)currFeatureType).gameObject;
        GetComponent<SpriteRenderer>().sprite = featurePrefab.GetComponent<SpriteRenderer>().sprite;

        transform.localScale = featurePrefab.transform.localScale;
        transform.position = transform.position +  featurePrefab.transform.position;
    }

    public void getCollected()
    {
        setActive(false);
        GetComponent<SpriteRenderer>().enabled = false; 
    }

    public void unCollect()
    {
        setActive(true);
        GetComponent<SpriteRenderer>().enabled = true; 
    }
}
