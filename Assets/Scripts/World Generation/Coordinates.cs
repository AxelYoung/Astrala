using UnityEngine;

public struct Coordinates {

    public int x { get; private set; }
    public int y { get; private set; }
    public int z { get; private set; }

    int q;
    int r;
    int s { get { return -q - r; } }

    public Coordinates(int x = 0, int y = 0, int z = 0) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.q = x;
        this.r = z - (x - (x & 1)) / 2;
    }

    public Coordinates topNeighbor {
        get {
            return new Coordinates(x, y + 1, z);
        }
    }

    public Coordinates bottomNeighbor {
        get {
            return new Coordinates(x, y - 1, z);
        }
    }

    // Returns all possible neighboring voxel directions
    public Coordinates[] sideNeighbors {
        get {
            Coordinates[] neighbors = new Coordinates[6];
            neighbors[0] = CubeToCoordinates(q + 1, y, r);
            neighbors[1] = CubeToCoordinates(q, y, r + 1);
            neighbors[2] = CubeToCoordinates(q - 1, y, r + 1);
            neighbors[3] = CubeToCoordinates(q - 1, y, r);
            neighbors[4] = CubeToCoordinates(q, y, r - 1);
            neighbors[5] = CubeToCoordinates(q + 1, y, r - 1);
            return neighbors;
        }
    }

    public Vector3 worldPosition {
        get {
            float worldX = VoxelData.voxelSize * ((3f / 2f) * q);
            float worldZ = VoxelData.voxelSize * (Mathf.Sqrt(3) / 2 * q + Mathf.Sqrt(3) * r);
            return new Vector3(worldX, y, worldZ);
        }
    }

    public static Coordinates zero = new Coordinates();

    public static Coordinates LocalToGlobalOffset(Coordinates localOffset, ChunkCoordinates chunkCoordinates) {
        int globalX = localOffset.x + (chunkCoordinates.x * VoxelData.chunkSizeInVoxels);
        int globalZ = localOffset.z + (chunkCoordinates.z * VoxelData.chunkSizeInVoxels);
        Coordinates globalOffset = new Coordinates(globalX, localOffset.y, globalZ);
        return globalOffset;
    }

    public static Coordinates GlobalToLocalOffset(Coordinates globalOffset, ChunkCoordinates chunkCoordinates) {
        int localX = globalOffset.x - (chunkCoordinates.x * VoxelData.chunkSizeInVoxels);
        int localZ = globalOffset.z - (chunkCoordinates.z * VoxelData.chunkSizeInVoxels);
        Coordinates localOffset = new Coordinates(localX, globalOffset.y, localZ);
        return localOffset;
    }

    static Coordinates CubeToCoordinates(int q, int y, int r) {
        int x = q;
        int z = r + (q - (q & 1)) / 2; ;
        return new Coordinates(x, y, z);
    }

    public static Coordinates WorldToCoordinates(Vector3 worldCoordinates) {
        float q = (2f / 3 * worldCoordinates.x) / VoxelData.voxelSize;
        float r = (-1f / 3 * worldCoordinates.x + Mathf.Sqrt(3) / 3 * worldCoordinates.z) / VoxelData.voxelSize;
        int y = Mathf.FloorToInt(worldCoordinates.y);
        float s = -q - r;

        int qInt = Mathf.RoundToInt(q);
        int rInt = Mathf.RoundToInt(r);
        int sInt = Mathf.RoundToInt(s);
        float qDiff = Mathf.Abs(qInt - q);
        float rDiff = Mathf.Abs(rInt - r);
        float sDiff = Mathf.Abs(sInt - s);
        if (qDiff > rDiff && qDiff > sDiff) {
            qInt = -rInt - sInt;
        } else if (rDiff > sDiff) {
            rInt = -qInt - sInt;
        } else {
            sInt = -qInt - rInt;
        }

        return CubeToCoordinates(qInt, y, rInt);
    }

    public bool Equals(Coordinates comparedCoordinates) {
        if (this.x != comparedCoordinates.x || this.y != comparedCoordinates.y || this.z != comparedCoordinates.z) return false;
        else return true;
    }
}