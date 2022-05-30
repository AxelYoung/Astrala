using UnityEngine;

public struct Coordinates {

    public int x { get; private set; }
    public int y { get; private set; }
    public int z { get; private set; }

    public Coordinates(int x = 0, int y = 0, int z = 0) {
        this.x = x;
        this.y = y;
        this.z = z;
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
            Coordinates[] neighbors = new Coordinates[4];
            neighbors[0] = new Coordinates(x, y, z - 1);
            neighbors[1] = new Coordinates(x + 1, y, z);
            neighbors[2] = new Coordinates(x, y, z + 1);
            neighbors[3] = new Coordinates(x - 1, y, z);
            return neighbors;
        }
    }

    public Vector3 worldPosition {
        get {
            return new Vector3(x, y, z);
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

    public static Coordinates WorldToCoordinates(Vector3 worldCoordinates) {
        return new Coordinates(Mathf.FloorToInt(worldCoordinates.x), Mathf.FloorToInt(worldCoordinates.y), Mathf.FloorToInt(worldCoordinates.z));
    }

    public bool Equals(Coordinates comparedCoordinates) {
        if (this.x != comparedCoordinates.x || this.y != comparedCoordinates.y || this.z != comparedCoordinates.z) return false;
        else return true;
    }
}