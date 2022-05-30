using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData {

    public static int seed = 100;

    public static readonly float voxelSize = 0.5f;

    public static readonly int worldChunkSize = 32;
    public static readonly int chunkSizeInVoxels = 16;
    public static readonly int viewDistance = 8;
    public static readonly int worldSizeInVoxels = worldChunkSize * chunkSizeInVoxels;
    public static readonly float chunkSize = VoxelData.chunkSizeInVoxels * VoxelData.voxelSize * 2;
    public static readonly Vector2 worldSize = new Vector2(chunkSize * worldChunkSize, chunkSize * worldChunkSize);
    public static readonly int chunkHeight = 64;

    public static readonly Vector2 spriteSheetSize = new Vector2(Voxels.voxelAmount, 3);


    public static Vector2[] hexVerticies = new Vector2[] {
        new Vector2(0f, 0f),
        new Vector2(1f, 0f),
        new Vector2(0f, 1f),
        new Vector2(1f, 1f)
    };

    public static Vector2[] hexUVs = new Vector2[] {
        new Vector2(0f, 0f),
        new Vector2(1f, 0f),
        new Vector2(0f, 1f),
        new Vector2(1f, 1f)
    };

    public static int[] hexSideOrder = new int[] {
        0,
        1,
        3,
        2
    };

    public readonly static int[] topHexagonalFace = new int[] {
        2, 3, 0,
        3, 1, 0
    };

    public readonly static int[] bottomHexagonalFace = new int[] {
        0, 3, 2,
        0, 1, 3
    };

    public readonly static int[] sideTriangles = new int[] {
        0, 3, 1,
        0, 2, 3
    };

    public static Vector3[] relativeVoxelVerticies(Vector3 center) {
        Vector3[] verticies = new Vector3[4];
        for (int i = 0; i < 4; i++) {
            verticies[i] = new Vector3(hexVerticies[i].x + center.x, center.y, hexVerticies[i].y + center.z);
        }
        return verticies;
    }

    public static int[] relativeVoxelTriangles(bool top, int meshVertCount) {
        int[] triangles = new int[VoxelData.topHexagonalFace.Length];
        for (int i = 0; i < triangles.Length; i++) {
            triangles[i] = (meshVertCount - 4) + (top ? topHexagonalFace[i] : bottomHexagonalFace[i]);
        }
        return triangles;
    }

    public static Vector2[] uvsFromIndex(Vector2 index, Vector2 sheetSize) {
        Vector2[] uvs = new Vector2[4];
        for (int i = 0; i < 4; i++) {
            uvs[i] = new Vector2(((hexUVs[i].x + index.x) / sheetSize.x), ((hexUVs[i].y + index.y) / sheetSize.y));
        }
        return uvs;
    }
}