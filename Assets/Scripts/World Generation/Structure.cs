using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure {
    public static Queue<VoxelModification> GenerateTree(Coordinates coordinates, int minHeight, int maxHeight, NoiseSettings treeHeightSettings, byte logID, byte leavesID) {
        Queue<VoxelModification> modifications = new Queue<VoxelModification>();

        int height = Mathf.RoundToInt(maxHeight * Noise.GetNoiseValue(coordinates.x, coordinates.z, treeHeightSettings));
        if (height < minHeight) {
            height = minHeight;
        }
        for (int i = 1; i < height; i++) {
            modifications.Enqueue(new VoxelModification(new Coordinates(coordinates.x, coordinates.y + i, coordinates.z), logID));
        }

        for (int x = -height / 2; x <= height / 2; x++) {
            for (int y = 0; y < height / 2; y++) {
                for (int z = -height / 2; z <= height / 2; z++) {
                    if (x == 0 && z == 0 && y < height / 3) {
                        modifications.Enqueue(new VoxelModification(new Coordinates(coordinates.x + x, coordinates.y + height + y, coordinates.z + z), logID));
                    } else {
                        modifications.Enqueue(new VoxelModification(new Coordinates(coordinates.x + x, coordinates.y + height + y, coordinates.z + z), leavesID));
                    }
                }
            }
        }

        for (int x = (-height / 2) + 1; x <= (height / 2) - 1; x++) {
            for (int y = height / 2; y < height - 1; y++) {
                for (int z = (-height / 2) + 1; z <= (height / 2) - 1; z++) {
                    modifications.Enqueue(new VoxelModification(new Coordinates(coordinates.x + x, coordinates.y + height + y, coordinates.z + z), leavesID));
                }
            }
        }

        return modifications;
    }
}
