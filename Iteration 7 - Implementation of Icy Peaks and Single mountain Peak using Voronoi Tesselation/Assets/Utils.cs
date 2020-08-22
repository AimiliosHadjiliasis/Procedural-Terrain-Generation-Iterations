using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class that holds the utilities that are used for the methods that 
//are used to genereate the height map
public static class Utils
{
    //Method that calculates the fractal Brownian Motion:
    //static so we can use it from outside
    //More about fbm: https://en.wikipedia.org/wiki/Fractional_Brownian_motion
    //x,y -> location
    //persistance: how many successive adding of the curve will get smaller/bigger
    public static float fBM(float x, float y, int octaves, float persistance)
    {
        //Data:
        float total = 0;        //total height value
        float frequency = 1;    //how close the waves are together
        float amplitude = 1;    //what persistance mult and scale up for every successive addition
        float maxValue = 0;     //Addition of each amplitude and octave

        //Loop through the octaves and modify the values
        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(x  * frequency,
                                        y  * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistance;
            frequency *= 2;
        }

        //return the fbm
        return total / maxValue;
    }
}
