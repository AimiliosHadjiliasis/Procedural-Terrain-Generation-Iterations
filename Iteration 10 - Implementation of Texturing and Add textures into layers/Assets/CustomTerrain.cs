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

    //Data that are used to implement Mltiple mountain Peaks using Voironoi Tesselation:
    public float voronoiFallOff = 0.02f;
    public float voronoiDropOff = 0.06f;
    public float voronoiMinHeight= 0.1f;
    public float voronoiMaxHeight= 0.4f;
    public int voronoiPeaks = 5;
    public enum VoronoiType
    {
        Linear = 0,
        Power = 1,
        SinPow = 2,
        Combined = 3
    }
    public VoronoiType voronoiType = VoronoiType.Linear;

    //Data that are used to generate mountains using MPD
    //Midpoint Displacement
    public float MPDheightMin = -2f;
    public float MPDheightMax = 2f;
    public float MPDheightDampenerPower = 2.0f;
    public float MPDroughness = 2.0f;

    //Data that sre used in smoothing algorithm
    public int smoothAmount = 2;

    //Data that are used in Texturing:
    /*****************************************************/
    // SPLATMAPS: We create a class since we are going to 
    // add a lot of textures in our terrain
    /*****************************************************/
    [System.Serializable]
    public class SplatHeights
    {
        public Texture2D texture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public Vector2 tileOffset = new Vector2(0, 0);
        public Vector2 tileSize = new Vector2(50, 50);
        public bool remove = false;
    }

    //List of our texture splat maps
    public List<SplatHeights> splatHeights = new List<SplatHeights>()
    {
        new SplatHeights()
    };

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

    //Method that generates the neighbours of of a verext so we can use it in smoothing
    List<Vector2> GenerateNeighbours(Vector2 pos, int width, int height)
    {
        //Initialise the neighbours inside a list
        List<Vector2> neighbours = new List<Vector2>();

        //Loop thorough to find the neighbours
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                //when the vertex is not the mid one
                if (!(x == 0 && y == 0))
                {
                    //Initialise the possition of it 
                    Vector2 nPos = new Vector2(Mathf.Clamp(pos.x + x, 0, width - 1),
                                                Mathf.Clamp(pos.y + y, 0, height - 1));

                    //And if is not contained in the neighbour list then it add it inside that list
                    if (!neighbours.Contains(nPos))
                    {
                        neighbours.Add(nPos);
                    }
                }
            }
        }
        return neighbours;
    }

    //Method that smooths the terrain peaks so they will look more realistic
    //this method is based on Blurring effect. It is averaging the neighbour vertices 
    //and generates a new value that in our case will make the mountain peaks look smoother (not like triangles)
    public void Smooth()
    {
        //2D array of floats that hold the height map
        //and initialises the height map equal to the values that we have 
        //currently on our terrain when we dont have the reset button on
        //otherwise we get a new heihgt map
        float[,] heightMap = GetHeightMap();

        //Amount of smooth that we want to make
        float smoothProgress = 0;

        //Create a smoothing bar to show us the progress while we are waiting in the editor
        EditorUtility.DisplayProgressBar("Smoothing Terrain",
                                 "Progress",
                                 smoothProgress);

        //Loop through the amount of smoothing that we want to apply:
        for (int s = 0; s < smoothAmount; s++)
        {
            //Loop through the height map
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                for (int x = 0; x < terrainData.heightmapWidth; x++)
                {
                    //Initialise the avereage height variable
                    float avgHeight = heightMap[x, y];

                    //Initialise the list of neighbours
                    List<Vector2> neighbours = GenerateNeighbours(new Vector2(x, y),
                                                                  terrainData.heightmapWidth,
                                                                  terrainData.heightmapHeight);

                    //Loop through the neighbours
                    foreach (Vector2 n in neighbours)
                    {
                        //and increase/decrease the average height
                        avgHeight += heightMap[(int)n.x, (int)n.y];
                    }

                    //Set heights in height map
                    heightMap[x, y] = avgHeight / ((float)neighbours.Count + 1);
                }
            }

            //Increasethe progress of smoothing
            smoothProgress++;

            //increase smoothing par as the algorithm finishes an iteration
            EditorUtility.DisplayProgressBar("Smoothing Terrain",
                                             "Progress",
                                             smoothProgress / smoothAmount);

        }

        //set height to the terrain data
        terrainData.SetHeights(0, 0, heightMap);

        //When it smoothing finish it close the progress bar
        EditorUtility.ClearProgressBar();
    }

    //Mwthod that generates a mesh with the use of Mid Point Displacement Algorithm
    public void MidPointDisplacement()
    {
        //2D array of floats that hold the height map
        //and initialises the height map equal to the values that we have 
        //currently on our terrain when we dont have the reset button on
        //otherwise we get a new heihgt map
        float[,] heightMap = GetHeightMap();

        //Initialise the width of the height map (remove the last line)
        int width = terrainData.heightmapWidth - 1;

        //Specifiying the current square that we are working with
        //used in for loop sinece we are on a square 
        int squareSize = width;

        //Initialises the heighmap min and max values 
        float heightMin = MPDheightMin;
        float heightMax = MPDheightMax;

        //Dampmening value that smooths the terrain
        float heightDampener = (float)Mathf.Pow(MPDheightDampenerPower, -1 * MPDroughness);

        //Secify exactly where each vertex is relative to the mid point
        //and since we dont know where exactly these variables are we calculate
        //them later in the algorithm
        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        //Loop while we are in the square
        while (squareSize > 0)
        {
            /*******************/
            //  Diamond Step:
            /*******************/
            //Loop through the square and initialise the values for the variables
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    //Initialise the valUes of the square:
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    //Set the heights in height map:
                    heightMap[midX, midY] = (float)((heightMap[x, y] +
                                                     heightMap[cornerX, y] +
                                                     heightMap[x, cornerY] +
                                                     heightMap[cornerX, cornerY]) / 4.0f +
                                                     UnityEngine.Random.Range(heightMin, heightMax));   //Add randomness to the possition
                }
            }

            /***********************/
            //  Square Step:
            /***********************/
            //Loop through the square and initialise the values for the variables
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    //Initialise the valies of the square:
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    pmidXR = (int)(midX + squareSize);
                    pmidYU = (int)(midY + squareSize);
                    pmidXL = (int)(midX - squareSize);
                    pmidYD = (int)(midY - squareSize);
                    
                    //In case that we are outside the bounds:
                    if (pmidXL <= 0 || pmidYD <= 0 || pmidXR >= width - 1 || pmidYU >= width - 1)
                    {
                        continue;
                    }

                    //Calculate the square value for the bottom side  
                    heightMap[midX, y] = (float)((heightMap[midX, midY] +
                                                  heightMap[x, y] +
                                                  heightMap[midX, pmidYD] +
                                                  heightMap[cornerX, y]) / 4.0f +
                                                  UnityEngine.Random.Range(heightMin, heightMax));
                    //Calculate the square value for the top side   
                    heightMap[midX, cornerY] = (float)((heightMap[x, cornerY] +
                                                            heightMap[midX, midY] +
                                                            heightMap[cornerX, cornerY] +
                                                            heightMap[midX, pmidYU]) / 4.0f +
                                                            UnityEngine.Random.Range(heightMin, heightMax));

                    //Calculate the square value for the left side   
                    heightMap[x, midY] = (float)((heightMap[x, y] +
                                                            heightMap[pmidXL, midY] +
                                                            heightMap[x, cornerY] +
                                                            heightMap[midX, midY]) / 4.0f +
                                                            UnityEngine.Random.Range(heightMin, heightMax));
                    //Calculate the square value for the right side   
                    heightMap[cornerX, midY] = (float)((heightMap[midX, y] +
                                                            heightMap[midX, midY] +
                                                            heightMap[cornerX, cornerY] +
                                                            heightMap[pmidXR, midY]) / 4.0f +
                                                            UnityEngine.Random.Range(heightMin, heightMax));

                }
            }

            squareSize = (int)(squareSize / 2.0f);  //Reduce square size each time we loop throught the height map
            heightMin *= heightDampener;            //Reduce the min height value
            heightMax *= heightDampener;            //Reduce the max height value
        }

        //set heights to terrain data
        terrainData.SetHeights(0, 0, heightMap);
    }

    //Method that generates Random terrain
    public void RandomTerrain()
    {
        //2D array of floats that hold the height map
        //and initialises the height map equal to the values that we have 
        //currently on our terrain when we dont have the reset button on
        //otherwise we get a new heihgt map
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
        //2D array of floats that hold the height map
        //and initialises the height map equal to the values that we have 
        //currently on our terrain when we dont have the reset button on
        //otherwise we get a new heihgt map
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
        //currently on our terrain when we dont have the reset button on
        //otherwise we get a new heihgt map
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
        //currently on our terrain when we dont have the reset button on
        //otherwise we get a new heihgt map
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
        //currently on our terrain when we dont have the reset button on
        //otherwise we get a new heihgt map
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

    //Method that draws random Icy Peaks using Voronoi Tessalation
    public void VoronoiIcyPeaks()
    {
        //2D array of floats that hold the height map
        //and initialises the height map equal to the values that we have 
        //currently on our terrain when we dont have the reset button on
        //otherwise we get a new heihgt map
        float[,] heightMap = GetHeightMap();

       // Random location to lift it up(so we store it in Vector3) and set a random range for x, z
       Vector3 peak = new Vector3(UnityEngine.Random.Range(0, terrainData.heightmapWidth),     //x
                                  UnityEngine.Random.Range(0.01f, 1.0f),                       //y
                                  UnityEngine.Random.Range(0, terrainData.heightmapHeight));   //z


       //Lift the height in the Random location (index x,z)
       heightMap[(int)peak.x, (int)peak.z] = peak.y;

        //Set the height
        terrainData.SetHeights(0, 0, heightMap);
    }

    //Method that draws multiple, random Mountain Peaks using Voronoi Tessalation
    public void VoronoiMultipleMountainPeak()
    {
        //2D array of floats that hold the height map
        //and initialises the height map equal to the values that we have 
        //currently on our terrain when we dont have the reset button on
        //otherwise we get a new heihgt map
        float[,] heightMap = GetHeightMap();

        //Loop through all the peaks:
        for (int p = 0; p < voronoiPeaks; p++)
        {
            //Set peak to random location with y value between min and max heihgt:
            Vector3 peak = new Vector3(UnityEngine.Random.Range(0, terrainData.heightmapWidth),
                                        UnityEngine.Random.Range(voronoiMinHeight, voronoiMaxHeight),
                                        UnityEngine.Random.Range(0, terrainData.heightmapHeight));

            //In case we are in a peak we are moving to the next
            //so we dont add a new peak otherwise if its not a peak then
            //we add a peak to the terrain
            if (heightMap[(int)peak.x, (int)peak.z] < peak.y)
            {
                heightMap[(int)peak.x, (int)peak.z] = peak.y;
            }
            else
            {
                continue;
            }


            //Initialise the peak location (x,z position in height map)
            Vector2 peakLocation = new Vector2(peak.x, peak.z);
            
            //Max distance possibly can be from peak
            float maxDst = Vector2.Distance(new Vector2(0, 0), new Vector2(terrainData.heightmapWidth, terrainData.heightmapHeight));

            //Loop through the height map
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                for (int x = 0; x < terrainData.heightmapWidth; x++)
                {
                    //stop processing the peak (if vertex is the peak then dont process it)
                    if (!(x == peak.x && y == peak.z))
                    {
                        //Calculate distance of a vertex from the peak
                        //float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) * fallOff;
                        float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDst;
                        float h;    //type of voronoi funvtion that we use to generate the mountains


                        //Select the voronoi type that we want to make the mountains look like
                        if (voronoiType == VoronoiType.Combined)
                        {
                            h = peak.y - distanceToPeak * voronoiFallOff -
                                Mathf.Pow(distanceToPeak, voronoiDropOff); //combined
                        }
                        else if (voronoiType == VoronoiType.Power)
                        {
                            h = peak.y - Mathf.Pow(distanceToPeak, voronoiDropOff) * voronoiFallOff; //power
                        }
                        else if (voronoiType == VoronoiType.SinPow)
                        {
                            h = peak.y - Mathf.Pow(distanceToPeak * 3, voronoiFallOff) -
                                    Mathf.Sin(distanceToPeak * 2 * Mathf.PI) / voronoiDropOff; //sinpow
                        }
                        else
                        {
                            h = peak.y - distanceToPeak * voronoiFallOff; //linear
                        }

                        //Allow multiple peaks to grow in the scene
                        if (heightMap[x,y] < h)
                        {
                            //each location on height map height will be smaller as 
                            //is located away from the peak of the mountain
                            heightMap[x, y] += h;
                        }

                    }
                }
            }
        }

        //Set the height
        terrainData.SetHeights(0, 0, heightMap);
    }

    //Method that add SplatMaps in the terrain
    public void SplatMaps()
    {
        //Array that store the textures and initialise it
        SplatPrototype[] newSplatPrototypes;
        newSplatPrototypes = new SplatPrototype[splatHeights.Count];

        //create index
        int spIndex = 0;

        //for every texture apply the texture to a specific height
        foreach (SplatHeights s in splatHeights)
        {
            //Create a new prototype
            newSplatPrototypes[spIndex] = new SplatPrototype();

            //Set texture of that with the same that we set in height maps
            newSplatPrototypes[spIndex].texture = s.texture;

            //Set the tile offset
            newSplatPrototypes[spIndex].tileOffset = s.tileOffset;

            //Set the size of each tile
            newSplatPrototypes[spIndex].tileSize = s.tileSize;

            //Apply that texture
            newSplatPrototypes[spIndex].texture.Apply(true);

            //Increaseto move to next texture
            spIndex++;
        }

        //Set the texture to the list of painting textures
        terrainData.splatPrototypes = newSplatPrototypes;

        //Height of each splat map so we can add diferent splatmaps in different heights
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        //Loop through every position in splatmaps 
        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                //Array of splatmaps
                float[] splat = new float[terrainData.alphamapLayers];

                //Loop around all tectures:
                for (int i = 0; i < splatHeights.Count; i++)
                {
                    //Set min and max height:
                    float thisHeightStart = splatHeights[i].minHeight;
                    float thisHeightStop = splatHeights[i].maxHeight;

                    if ((heightMap[x,y] >= thisHeightStart && heightMap[x,y] <= thisHeightStop))
                    {
                        splat[i] = 1;
                    }
                }

                NormalizeVector(splat);
                for (int j = 0; j < splatHeights.Count; j++)
                {
                    splatmapData[x, y, j] = splat[j];
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    //Method that used to normalize vectors
    void NormalizeVector(float[] vecArray)
    {
        float total = 0;
        //Loop through and find what the total is:
        for (int i = 0; i < vecArray.Length; i++)
        {
            total += vecArray[i];
        }

        //And then loops through and devide the vector array with the total so 
        //we get a normalised vector
        for (int i = 0; i < vecArray.Length; i++)
        {
            vecArray[i] /= total;
        }
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
    //      ADD AND REMOVE METHODS FOR SPLAT MAPS:
    /**************************************************************/
    //Method that adds a new Splatmap in the table when
    //we press the + sign in the table
    public void AddNewSplatHeight()
    {
        //create new item in list (new row)
        splatHeights.Add(new SplatHeights());
    }

    //Method that removes the selected splat map from the table
    //when we press the - sign
    public void RemoveSplatHeight()
    {
        //Create new list with the items that we will have
        List<SplatHeights> keptSplatHeights = new List<SplatHeights>();

        //Go through all the items and if its not ticked then add it in the kept list
        for (int i = 0; i < splatHeights.Count; i++)
        {
            if (!splatHeights[i].remove)
            {
                keptSplatHeights.Add(splatHeights[i]);
            }

        }

        //Make sure that we have at least one item
        if (keptSplatHeights.Count == 0)
        {
            keptSplatHeights.Add(splatHeights[0]);
        }
        splatHeights = keptSplatHeights;
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
