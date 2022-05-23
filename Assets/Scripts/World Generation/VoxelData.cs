using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData {

    public static int seed = 100;

    public static readonly float voxelSize = 0.5373f;

    public static readonly int worldChunkSize = 32;
    public static readonly int chunkSizeInVoxels = 16;
    public static readonly int viewDistance = 8;
    public static readonly int worldSizeInVoxels = worldChunkSize * chunkSizeInVoxels;
    public static readonly float chunkWidth = VoxelData.chunkSizeInVoxels * VoxelData.voxelSize * 1.5f;
    public static readonly float chunkDepth = VoxelData.chunkSizeInVoxels * VoxelData.voxelSize * Mathf.Sqrt(3);
    public static readonly Vector2 worldSize = new Vector2(chunkWidth * worldChunkSize, chunkDepth * worldChunkSize);
    public static readonly int chunkHeight = 64;

    public static readonly Vector2 spriteSheetSize = new Vector2(Voxels.voxelAmount, 3);


    public static Vector2[] hexVerticies = new Vector2[] {
        new Vector2(0.5373f, 0),
        new Vector2(0.26865f, 0.4653155f),
        new Vector2(-0.26865f, 0.4653154f),
        new Vector2(-0.5373f, 0),
        new Vector2(-0.2686499f, -0.46543155f),
        new Vector2(0.2686499f, -0.4653155f)
    };

    public static Vector2[] hexUVs = new Vector2[] {
        new Vector2(0.5f, 0),
        new Vector2(0.25f, 0.4330127f),
        new Vector2(-0.25f, 0.4330127f),
        new Vector2(-0.5f, 0),
        new Vector2(-0.25f, -0.4330127f),
        new Vector2(0.25f, -0.4330127f)
    };

    public static Vector2[] hexSideUVs = new Vector2[] {
            new Vector2(-0.25f, 0),
            new Vector2(0.25f, 0),
            new Vector2(-0.25f, 1),
            new Vector2(0.25f, 1)
    };


    // Returns verticies of a hexagon
    public static Vector2[] CalculateHexagon(float size) {
        Vector2[] points = new Vector2[6];
        for (int i = 0; i < 6; i++) {
            float angleDegrees = 60 * i;
            float angleRadians = Mathf.PI / 180 * angleDegrees;
            Vector3 point = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
            points[i] = point * size;
            Debug.Log(i + ", " + points[i].x + ", " + points[i].y + ", " + size);
        }
        return points;
    }

    // Returns triangle order of hexagon
    public readonly static int[] topHexagonalFace = new int[] {
        5, 4, 0,
        4, 3, 0,
        3, 2, 0,
        2, 1, 0
    };

    public readonly static int[] bottomHexagonalFace = new int[] {
        0, 4, 5,
        0, 3, 4,
        0, 2, 3,
        0, 1, 2
    };

    public readonly static int[] sideTriangles = new int[] {
        0, 3, 1,
        0, 2, 3
    };

    public static Vector3[] relativeVoxelVerticies(Vector3 center) {
        Vector3[] verticies = new Vector3[6];
        for (int i = 0; i < 6; i++) {
            verticies[i] = new Vector3(hexVerticies[i].x + center.x, center.y, hexVerticies[i].y + center.z);
        }
        return verticies;
    }

    public static int[] relativeVoxelTriangles(bool top, int meshVertCount) {
        int[] triangles = new int[VoxelData.topHexagonalFace.Length];
        for (int i = 0; i < triangles.Length; i++) {
            triangles[i] = (meshVertCount - 6) + (top ? topHexagonalFace[i] : bottomHexagonalFace[i]);
        }
        return triangles;
    }

    public static Vector2[] faceUVsFromIndex(Vector2 index, Vector2 sheetSize) {
        Vector2[] uvs = new Vector2[6];
        for (int i = 0; i < 6; i++) {
            uvs[i] = new Vector2(((hexUVs[i].x + 0.5f + index.x) / sheetSize.x), ((hexUVs[i].y + 0.5f + index.y) / sheetSize.y));
        }
        return uvs;
    }

    public static Vector2[] sideUVsFromIndex(Vector2 index, Vector2 sheetSize) {
        Vector2[] uvs = new Vector2[4];
        for (int i = 0; i < 4; i++) {
            uvs[i] = new Vector2(((hexSideUVs[i].x + 0.5f + index.x) / sheetSize.x), ((hexSideUVs[i].y + index.y) / sheetSize.y));
        }
        return uvs;
    }
}