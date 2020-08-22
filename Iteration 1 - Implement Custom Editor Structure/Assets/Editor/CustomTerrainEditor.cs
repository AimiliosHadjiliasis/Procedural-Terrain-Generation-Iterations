using UnityEngine;
using UnityEditor;
using EditorGUITable;

//links editor code and Custom terrain script
//so it knows when we add anyting to an object in the hierarchy when we are looking in the inspector
//then come back to this code and get all the details on how it should be displayed
[CustomEditor(typeof(CustomTerrain))]   
[CanEditMultipleObjects]    //can apply to multiple objects
public class CustomTerrainEditor : Editor {

    //method that allouw us to setup anything when we are setting something new into our editor
    //it disapbles and re-enables everything everytime we make a change without pressing play (same as awake method)
    void OnEnable()
    {

    }

    //Method that overrides the GUI 
    public override void OnInspectorGUI()
    {

    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
