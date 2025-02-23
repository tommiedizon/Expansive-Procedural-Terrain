﻿using Unity.VisualScripting;
using UnityEngine;
public static class Noise {
    public static float[,] GenerateNoiseMap(GenerateNoiseMapParams parameters) {

        Vector2[] octaveOffsets = GenerateOctaveOffsets(parameters);
        float[,] noiseMap = new float[parameters.mapChunkSize, parameters.mapChunkSize];

        // Variables for state
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        float halfWidth = parameters.mapChunkSize / 2;
        float halfHeight = parameters.mapChunkSize / 2;

        // Loop through each pixel
        for (int y = 0; y < parameters.mapChunkSize; y++) {
            for (int x = 0; x < parameters.mapChunkSize; x++) {
                
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                // loop through octaves
                for (int i = 0; i < parameters.octaves; i++) {

                    float sampleX = (x - halfWidth) / parameters.noiseScale * frequency + octaveOffsets[i].x * frequency;
                    float sampleY = (y - halfHeight) / parameters.noiseScale * frequency - octaveOffsets[i].y * frequency;

                    // Shift and scale to be in range [-1,1]
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    noiseHeight += perlinValue * amplitude;
                    amplitude *= parameters.persistance;
                    frequency *= parameters.lacunarity;

                    noiseMap[x, y] = noiseHeight;
                }

                // Update state variables 
                if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;
            }
        }

        RenormalizeNoiseMap(parameters, ref noiseMap, maxNoiseHeight, minNoiseHeight);
        return noiseMap;

    }
    private static void RenormalizeNoiseMap(GenerateNoiseMapParams parameters, ref float[,] noiseMap, float maxNoiseHeight, float minNoiseHeight) {
        /* Re-normalizes noise map to be between 0 and 1. */
        for (int y = 0; y < parameters.mapChunkSize; y++) 
            for (int x = 0; x < parameters.mapChunkSize; x++) 
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);         
    }
    private static Vector2[] GenerateOctaveOffsets(GenerateNoiseMapParams param) {
        /* */
        const int MIN = -100000;
        const int MAX = 100000;
        var prng = new System.Random(param.seed);
        var octaveOffsets = new Vector2[param.octaves];

        for (int i = 0; i < param.octaves; i++) {
            float offsetX = prng.Next(MIN, MAX) + param.offset.x;
            float offsetY = prng.Next(MIN, MAX) + param.offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        return octaveOffsets;
    }
}
