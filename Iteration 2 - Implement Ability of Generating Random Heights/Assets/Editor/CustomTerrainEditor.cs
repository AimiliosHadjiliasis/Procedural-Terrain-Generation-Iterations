using UnityEngine;
using UnityEditor;
using EditorGUITable;

//links editor code and Custom terrain script
//so it knows when we add anyting to an object in the hierarchy when we are looking in the inspector
//then come back to this code and get all the details on how it should be displayed
[CustomEditor(typeof(CustomTerrain))]   
[CanEditMultipleObjects]    //can apply to multiple objects
public class CustomTerrainEditor : Editor
{
    /**************************************************/
    //                  Properties:
    /**************************************************/
    //References (Link with other script):
    SerializedProperty randomHeightRange;

    /**************************************************/
    //                  Foldouts:
    /**************************************************/
    bool showRandom = false;


    //method that allouw us to setup anything when we are setting something new into our editor
    //it disapbles and re-enables everything everytime we make a change without pressing play (same as awake method)
    void OnEnable()
    {
        //Syncronised with the custom terrain script:
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
    }

    //Method that overrides the GUI 
    public override void OnInspectorGUI()
    {
        /*******************************************************************/
        //UPDATES all serialized properties between this 
        //script and the Custom Terrain script
        serializedObject.Update();
        
        //Link with the custom terrain script
        CustomTerrain terrain = (CustomTerrain)target;

        /*************************************************************/
        //  Random Heights:
        /*************************************************************/
        showRandom = EditorGUILayout.Foldout(showRandom, "Random"); //set true false to foldout
       
        //show if foldout is true
        if (showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);  //Generate slider
            GUILayout.Label("Set Heights between Random Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);

            //Create button that generates random terrain:
            if (GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }
        }

        /************************************************************/
        // Reset Button that resets the terrain back to normal(flat)
        /************************************************************/
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrain();
        }

        //Apply all the changes:
        serializedObject.ApplyModifiedProperties();
        /*******************************************************************/
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
