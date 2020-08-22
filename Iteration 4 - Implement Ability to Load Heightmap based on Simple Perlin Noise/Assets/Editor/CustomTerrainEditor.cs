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
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;
    SerializedProperty perlinXScale;
    SerializedProperty perlinYScale;
    SerializedProperty perlinOffsetX;
    SerializedProperty perlinOffsetY;

    /**************************************************/
    //                  Foldouts:
    /**************************************************/
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showSimplePerlinNoise = false;


    //method that allouw us to setup anything when we are setting something new into our editor
    //it disapbles and re-enables everything everytime we make a change without pressing play (same as awake method)
    void OnEnable()
    {
        //Syncronised with the custom terrain script:
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
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
        //                      Random Heights:
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

        /*************************************************************/
        //                  Load Heights from Image:
        /*************************************************************/
        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "Load Heights");
        if (showLoadHeights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Heights From Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);

            //Create button that load the texture from an image
            if (GUILayout.Button("Load Texture"))
            {
                terrain.LoadTexture();
            }
        }

        /*************************************************************/
        //          Load Heigths based on Simple Perlin Noise:
        /*************************************************************/
        showSimplePerlinNoise = EditorGUILayout.Foldout(showSimplePerlinNoise, "Simple Perlin Noise");
        if (showSimplePerlinNoise)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Perlin Noise", EditorStyles.boldLabel);
            
            //Add a slider as for scale X and Y values:
            EditorGUILayout.Slider(perlinXScale, 0, 1, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale, 0, 1, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinOffsetX, 0, 10000, new GUIContent("Offset X"));
            EditorGUILayout.IntSlider(perlinOffsetY, 0, 10000, new GUIContent("Offset Y"));

            //Create a button that generates a height map based on Perlin noise
            if (GUILayout.Button("Simple Perlin Noise"))
            {
                terrain.Perlin();
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
