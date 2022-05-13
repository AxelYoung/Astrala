using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Biome", menuName = "Biome")]
public class Biome : ScriptableObject {

    [Header("Biome Parameters")]
    public int solidGroundHeight;
    public NoiseSettings biomeSettings;
    public float heightMultiplier;
    public AnimationCurve heightMultiplierCurve;

    [Header("Tree Parameters")]
    public NoiseSettings treeZoneSettings;
    public float treeZoneThreshold;
    public NoiseSettings treePlacementSettings;
    public float treePlacementThreshold;
    public NoiseSettings treeHeightSettings;
    public int minTreeHeight;
    public int maxTreeHeight;

    [Header("Foliage Parameters")]
    public NoiseSettings grassSettings;
    public float grassThreshold;
    public NoiseSettings flowerSettings;
    public float flowerThreshold;
}