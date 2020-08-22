﻿using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;


[ExecuteInEditMode] //allow us to use the methos in Edit mode and not only when we press the play button
public class CustomTerrain : MonoBehaviour
{
    //Vector2 that holds 2 random values that are used as the height
    //and initially set it to values o and 0.1f
    public Vector2 randomHeightRange = new Vector2(0, 0.1f);

    public Terrain terrain; //Reference terrain
    public TerrainData terrainData;
    
    //Method that generates Random terrain
    public void RandomTerrain()
    {
        //2D array of floats that hold the height map
        //and initialises the height map equal to the values that we have 
        //currently on our terrain
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                         terrainData.heightmapHeight);
       
        //Loop through the map and set random values for x and y
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                //x -> min  y -> max (between 0 and 1)
                heightMap[x, z] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }

        //Set heights starting from point 0,0
        terrainData.SetHeights(0, 0, heightMap);
    }

    //Method that Reset the terrain
    //flatens the entire terrain by setting all the values equal to 0
    public void ResetTerrain()
    {
        //Generate a new heihgt map
        float[,] heightMap;
        heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];

        //Loop through the map and set all the values in the terrain to 0
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                //flat the terrain
                heightMap[x, z] = 0;
            }
        }

        //Set heights starting from point 0,0
        terrainData.SetHeights(0, 0, heightMap);
    }

    //Method that runs everytime we update the script
    //It basiclly initialise the data of the terrain
    void OnEnable()
    {
        Debug.Log("Initialising Terrain Data");
        terrain = this.GetComponent<Terrain>();
        terrainData = Terrain.activeTerrain.terrainData;
    }

    //Method that on when we press the play button add tags 
    //so we can manage our objeccts
    void Awake()
    {
        //Initialise tag manager in the path that we set:
        SerializedObject tagManager = new SerializedObject(
                             AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        //Pickup the particular tags:
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        //Add tags to foldout:
        AddTag(tagsProp, "Terrain");
        AddTag(tagsProp, "Cloud");
        AddTag(tagsProp, "Shore");

        //apply all the tag changes to tag database
        //update database to add tags in
        tagManager.ApplyModifiedProperties();

        //take the selected object:
        this.gameObject.tag = "Terrain";
    }

    //Method that looks around all the existing tags
    //Search to see if the tag we want to add exist so we dont add it again
    //to do that we use the bool found and if we have it we set it to true
    //if we dont find it we add the tag to the array of tags
    void AddTag(SerializedProperty tagsProp, string newTag)
    {
        bool found = false;

        //ensure the tag doesn't already exist
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag)) { found = true; break; }
        }

        //add your new tag
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
