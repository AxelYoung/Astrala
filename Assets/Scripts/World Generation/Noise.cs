using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {

    public static float GetNoiseValue(int x, int y, NoiseSettings settings = new NoiseSettings()) {

        System.Random rng = new System.Random(VoxelData.seed);

        if (settings.Equals(NoiseSettings.empty)) {
            return Mathf.PerlinNoise(x * .5f, y * .5f);
        }

        float noiseValue;

        Vector2[] octaveOffsets = new Vector2[settings.octaves];


        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < settings.octaves; i++) {
            float offsetX = rng.Next(-100000, 100000);
            float offsetY = rng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= settings.persistance;
        }

        if (settings.scale <= 0) {
            settings.scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfSize = VoxelData.worldSizeInVoxels / 2f;

        amplitude = 1;
        frequency = 1;
        float noiseHeight = 0;


        for (int i = 0; i < settings.octaves; i++) {
            float sampleX = (x - halfSize + octaveOffsets[i].x) / settings.scale * frequency;
            float sampleY = (y - halfSize + octaveOffsets[i].y) / settings.scale * frequency;

            float perlinValue = (Mathf.PerlinNoise(sampleX, sampleY) * 2) - 1;
            noiseHeight += perlinValue * amplitude;

            amplitude *= settings.persistance;
            frequency *= settings.lacunarity;
        }

        if (noiseHeight > maxNoiseHeight) {
            maxNoiseHeight = noiseHeight;
        } else if (noiseHeight < minNoiseHeight) {
            minNoiseHeight = noiseHeight;
        }

        float normalizedHeight = (noiseHeight + 1) / (2f * maxPossibleHeight);
        noiseValue = normalizedHeight;

        return noiseValue;
    }
}

[System.Serializable]
public struct NoiseSettings {
    public float scale;
    public int octaves;
    public float persistance;
    public float lacunarity;

    public NoiseSettings(float scale = 0f, int octaves = 0, float persistance = 0f, float lacunarity = 0f) {
        this.scale = scale;
        this.octaves = octaves;
        this.persistance = persistance;
        this.lacunarity = lacunarity;
    }

    public static NoiseSettings empty = new NoiseSettings();

    public bool Equals(NoiseSettings comparedSettings) {
        if (this.scale != comparedSettings.scale || this.octaves != comparedSettings.octaves || this.persistance != comparedSettings.persistance || this.lacunarity != comparedSettings.lacunarity) return false;
        else return true;
    }
}