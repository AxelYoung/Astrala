using UnityEngine;

public class ChunkCoordinates {
    public int x;
    public int z;

    public ChunkCoordinates() {
        x = 0;
        z = 0;
    }

    public ChunkCoordinates(int x, int z) {
        this.x = x;
        this.z = z;
    }

    public ChunkCoordinates(Vector3 worldCoordinates) {
        x = Mathf.FloorToInt(worldCoordinates.x / VoxelData.chunkWidth);
        z = Mathf.FloorToInt(worldCoordinates.z / VoxelData.chunkDepth);
    }

    public ChunkCoordinates(Coordinates coordinates) {
        x = Mathf.FloorToInt((float)coordinates.x / VoxelData.chunkSizeInVoxels);
        z = Mathf.FloorToInt((float)coordinates.z / VoxelData.chunkSizeInVoxels);
    }
}