using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;


[ExecuteInEditMode] //allow us to use the methos in Edit mode and not only when we press the play button
public class CustomTerrain : MonoBehaviour
{
    /***********************************************************************/
    //                         References and Data:
    /***********************************************************************/
    //Reset tick box that resets the terrain everytime we want to 
    //apply changes in the terrain when is set to true
    public bool resetTerrain = true;

    //Vector2 that holds 2 random values that are used as the height
    //and initially set it to values o and 0.1f
    public Vector2 randomHeightRange = new Vector2(0, 0.1f);

    //Data that used to load the heihgt map from an Image:
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);

    //Data that are used to implement Simple Perlin Noise and fBM Perlin Noise:
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinOffsetX = 0;
    public int perlinOffsetY = 0;
    public int perlinOctaves = 3;
    public float perlinPersistance = 8;
    public float perlinHeightScale = 0.09f;
    public float perlinfBMXScale = 0.01f;
    public float perlinfBMYScale = 0.01f;
    public int perlinfBMOffsetX = 0;
    public int perlinfBMOffsetY = 0;

    //Data for Multiple Perlin Noise:
    //All the data are the same as the Simple Perlin Noise 
    //but we add them in a class so we can have an array of multiple perlin noises
    [System.Serializable]
    public class MultiplePerlinParameters
    {
        public float mPerlinXScale = 0.01f;
        public float mPerlinYScale = 0.01f;
        public int mPerlinOffsetX = 0;
        public int mPerlinOffsetY = 0;
        public int mPerlinOctaves = 3;
        public float mPerlinPersistance = 8;
        public float mPerlinHeightScale = 0.09f;
        public bool remove = false;
    }

    //List that holds perlin Parameters so we add them together and generate a terrain
    //with the use of multiple perlin noises
    public List<MultiplePerlinParameters> multiplePerlinParameters = new List<MultiplePerlinParameters>()
    {
        new MultiplePerlinParameters()  //Add at least one perlin parameter inside so we dont get compiler error
    };

    //Reference terrain
    public Terrain terrain; 
    public TerrainData terrainData;
    
    //Method that gets the heightmap
    float[,] GetHeightMap()
    {
        //when the reset box is false then it gets the heights of the terrain data
        //otherwise it genereates new terrain data
        if (!resetTerrain)
        {
            return terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        }
        else
        {
            return new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        }
    }

    //Method that generates Random terrain
    public void RandomTerrain()
    {
        //2D array of floats that hold the height map
        //and initialises the height map equal to the values that we have 
        //currently on our terrain
        float[,] heightMap = GetHeightMap();

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

    //Method that load the height map from a Texture (image)
    public void LoadTexture()
    {
        //Create new height map using the dimensions of our terrain:
        float[,] heightMap = GetHeightMap();

        //Loop through the terrain:
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                //set heightmap values based on the heightmaps that come from an image:
                //Get pixel -> gives you the pixel colour at the location(x,y)
                heightMap[x, z] += heightMapImage.GetPixel((int)(x * heightMapScale.x),
                                                         (int)(z * heightMapScale.z)).grayscale
                                                         * heightMapScale.y;
            }
        }

        //Set the heights:
        terrainData.SetHeights(0, 0, heightMap);
    }

    //Method that generates the height map based on Perlin Noise 
    public void PerlinNoise()
    {
        //2D array of floats that hold the height map
        //and initialises the height map equal to the values that we have 
        //currently on our terrain
        float[,] heightMap = GetHeightMap();

        //Loop through the map and set values to height map based on perlin noise
        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                //set values to height map based on Perlin Noise
                //multiply by scale so we get a small value between 0 and 1
                heightMap[x, y] += Mathf.PerlinNoise((x + perlinOffsetX) * perlinXScale,
                                                    (y + perlinOffsetY) * perlinYScale);
            }
        }

        //Set heights starting from point 0,0
        terrainData.SetHeights(0, 0, heightMap);
    }
 
    //Method that generates height map based Simple Perlin Noise and the use of fbm 
    public void fBMPerlinNoise()
    {
        //2D array of floats that hold the height map
        //and initialises the height map equal to the values that we have 
        //currently on our terrain
        float[,] heightMap = GetHeightMap();

        //Loop through the map and set values to height map based on perlin noise
        //that uses the fractal Brownian Motion
        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                //set heigh map with valuse that are calculated with the use of fbm method
                heightMap[x, y] += Utils.fBM((x + perlinOffsetX) * perlinXScale,
                                            (y + perlinOffsetY) * perlinYScale,
                                            perlinOctaves,
                                            perlinPersistance) * perlinHeightScale;
            }
        }

        //Set heights starting from point 0,0
        terrainData.SetHeights(0, 0, heightMap);
    }

    //Method that generates height with the use of multiple perlin noises
    public void MultiplePerlinNoiseTerrain()
    {
        //2D array of floats that hold the height map
        //and initialises the height map equal to the values that we have 
        //currently on our terrain
        float[,] heightMap = GetHeightMap();

        //Loop through the terrain and add values to the height map 
        //with the use of multiple perlin noises (foreach loop). This method uses the fBM method
        for (int y= 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                foreach (MultiplePerlinParameters par in multiplePerlinParameters)
                {
                     heightMap[x,y] += Utils.fBM((x + par.mPerlinOffsetX) * par.mPerlinXScale,
                                            (y + par.mPerlinOffsetY) * par.mPerlinYScale,
                                            par.mPerlinOctaves,
                                            par.mPerlinPersistance) * par.mPerlinHeightScale;
                }
            }
        }

        //Set heights starting from point 0,0
        terrainData.SetHeights(0, 0, heightMap);
    }

    /**************************************************************/
    //      ADD AND REMOVE METHODS FOR MULTIPLE PERLIN NOISE
    /**************************************************************/
    //Method that adds a new Perlin Noise in the table when
    //we press the + sign in the table
    public void  AddNewPerlin()
    { 
        //create new item in list (new row)
        multiplePerlinParameters.Add(new MultiplePerlinParameters());
    }

    //Method that removes the selected perlin noise from the table
    //when we press the - sign
    public void RemovePerlin()
    {
        //Create new list with the items that we will have
        List<MultiplePerlinParameters> keptPerlinParameters = new List<MultiplePerlinParameters>();

        //Go through all the items and if its not ticked then add it in the kept list
        for (int i = 0; i < multiplePerlinParameters.Count; i++)
        {
            if (!multiplePerlinParameters[i].remove)
            {
                keptPerlinParameters.Add(multiplePerlinParameters[i]);
            }
        }

        //Make sure that we have at least one item
        if (keptPerlinParameters.Count == 0)    
        {
            keptPerlinParameters.Add(multiplePerlinParameters[0]); //add at lreast 1
        }

        multiplePerlinParameters = keptPerlinParameters;
    }

    /**************************************************************/
    //  Method that Reset the terrain flatens the entire terrain 
    //  by setting all the values equal to 0
    /**************************************************************/
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
