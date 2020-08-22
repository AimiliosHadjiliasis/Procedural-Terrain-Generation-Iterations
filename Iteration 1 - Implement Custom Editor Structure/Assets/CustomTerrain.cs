using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;


[ExecuteInEditMode] //allow us to use the methos in Edit mode and not only when we press the play button
public class CustomTerrain : MonoBehaviour
{
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
