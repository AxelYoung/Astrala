using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading;

public class WorldGeneration : MonoBehaviour {

    public Biome biome;
    public int seed;
    public Material worldMaterial;
    public Material worldMaterialTransparent;
    public Chunk[,] chunkMap = new Chunk[VoxelData.worldChunkSize, VoxelData.worldChunkSize];

    List<ChunkCoordinates> activeChunks = new List<ChunkCoordinates>();
    List<ChunkCoordinates> chunksToCreate = new List<ChunkCoordinates>();
    public List<Chunk> chunksToUpdate = new List<Chunk>();

    bool applyingModifications = false;

    public Transform player;

    ChunkCoordinates previousPlayerChunkCoordinates;
    ChunkCoordinates currentPlayerChunkCoordinates;

    Queue<Queue<VoxelModification>> modifications = new Queue<Queue<VoxelModification>>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    Thread ChunkUpdateThread;
    public object ChunkUpdateThreadLock = new object();

    public int meshLayer;
    public int foliageLayer;

    bool doneCreatingWorld = false;

    // Start is called before the first frame update
    void Awake() {
        VoxelData.seed = seed;

        ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
        ChunkUpdateThread.Start();

        GenerateWorld();
        previousPlayerChunkCoordinates = new ChunkCoordinates(Coordinates.WorldToCoordinates(player.position));
    }

    void GenerateWorld() {
        for (int x = VoxelData.worldChunkSize / 2 - VoxelData.viewDistance / 2; x < VoxelData.worldChunkSize / 2 + VoxelData.viewDistance / 2; x++) {
            for (int z = VoxelData.worldChunkSize / 2 - VoxelData.viewDistance / 2; z < VoxelData.worldChunkSize / 2 + VoxelData.viewDistance / 2; z++) {
                ChunkCoordinates chunk = new ChunkCoordinates(x, z);
                chunkMap[x, z] = new Chunk(this, chunk);
                chunksToCreate.Add(chunk);
            }
        }
        Vector3 spawn = new Vector3(VoxelData.worldSize.x / 2, VoxelData.chunkHeight * 2, VoxelData.worldSize.y / 2);
        player.position = spawn;
        CheckViewDistance();
        doneCreatingWorld = true;
    }

    void CreateChunk() {
        ChunkCoordinates chunk = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        chunkMap[chunk.x, chunk.z].Initialize();
    }

    void UpdateChunks() {
        bool updated = false;
        int index = 0;
        lock (ChunkUpdateThreadLock) {
            while (!updated && index < chunksToUpdate.Count - 1) {
                if (chunksToUpdate[index].isEditable) {
                    chunksToUpdate[index].UpdateChunk();
                    activeChunks.Add(chunksToUpdate[index].chunkCoordinates);
                    chunksToUpdate.RemoveAt(index);
                    updated = true;
                } else {
                    index++;
                }
            }
        }
    }


    void ApplyModifications() {
        applyingModifications = true;

        while (modifications.Count > 0) {
            Queue<VoxelModification> modificationsQueue = modifications.Dequeue();
            while (modificationsQueue.Count > 0) {
                VoxelModification mod = modificationsQueue.Dequeue();
                ChunkCoordinates chunk = new ChunkCoordinates(mod.coordinates);
                if (chunkInWorld(chunk)) {
                    if (chunkMap[chunk.x, chunk.z] == null) {
                        chunkMap[chunk.x, chunk.z] = new Chunk(this, chunk);
                        chunksToCreate.Add(chunk);
                    }

                    chunkMap[chunk.x, chunk.z].modifications.Enqueue(mod);
                }
            }
        }
        applyingModifications = false;
    }


    void Update() {
        currentPlayerChunkCoordinates = new ChunkCoordinates(Coordinates.WorldToCoordinates(player.position));
        if (!currentPlayerChunkCoordinates.Equals(previousPlayerChunkCoordinates))
            CheckViewDistance();
        if (chunksToCreate.Count > 0) {
            CreateChunk();
        }
        if (chunksToDraw.Count > 0) {
            if (chunksToDraw.Peek().isEditable) {
                chunksToDraw.Dequeue().GenerateMesh();
            }
        }
    }

    void OnDisable() {
        ChunkUpdateThread.Abort();
    }

    void ThreadedUpdate() {
        while (true) {
            if (!applyingModifications) {
                ApplyModifications();
            }
            if (chunksToUpdate.Count > 0) {
                UpdateChunks();
            }
        }
    }

    public bool solidVoxelAtCoordinates(Coordinates coordinates, ChunkCoordinates originChunkCoordinates) {
        Coordinates globalCoordinates = Coordinates.LocalToGlobalOffset(coordinates, originChunkCoordinates);

        ChunkCoordinates chunkCoordinates = new ChunkCoordinates(globalCoordinates);
        Coordinates localCoordinates = Coordinates.GlobalToLocalOffset(globalCoordinates, chunkCoordinates);

        if (!chunkInWorld(chunkCoordinates) || coordinates.y < 0 || coordinates.y >= VoxelData.chunkHeight) return false;

        if (chunkMap[chunkCoordinates.x, chunkCoordinates.z] != null && chunkMap[chunkCoordinates.x, chunkCoordinates.z].isEditable) {
            return Voxels.types[chunkMap[chunkCoordinates.x, chunkCoordinates.z].voxelMap[localCoordinates.x, localCoordinates.y, localCoordinates.z]].isSolid;
        }

        return Voxels.types[GenerateVoxelID(globalCoordinates)].isSolid;
    }

    public bool transparentVoxelAtCoordinates(Coordinates coordinates, ChunkCoordinates originChunkCoordinates) {
        Coordinates globalCoordinates = Coordinates.LocalToGlobalOffset(coordinates, originChunkCoordinates);

        ChunkCoordinates chunkCoordinates = new ChunkCoordinates(globalCoordinates);
        Coordinates localCoordinates = Coordinates.GlobalToLocalOffset(globalCoordinates, chunkCoordinates);

        if (!chunkInWorld(chunkCoordinates) || coordinates.y < 0 || coordinates.y >= VoxelData.chunkHeight) return false;

        if (chunkMap[chunkCoordinates.x, chunkCoordinates.z] != null && chunkMap[chunkCoordinates.x, chunkCoordinates.z].isEditable) {
            return Voxels.types[chunkMap[chunkCoordinates.x, chunkCoordinates.z].voxelMap[localCoordinates.x, localCoordinates.y, localCoordinates.z]].isTransparent;
        }

        return Voxels.types[GenerateVoxelID(globalCoordinates)].isTransparent;
    }

    public byte blockSpeedAtCoordinates(Coordinates coordinates) {
        if (!voxelInWorld(coordinates)) return Voxels.Air.breakSpeed;

        ChunkCoordinates chunkCoordinates = new ChunkCoordinates(coordinates);
        Coordinates localCoordinates = Coordinates.GlobalToLocalOffset(coordinates, chunkCoordinates);

        if (!chunkInWorld(chunkCoordinates) || coordinates.y < 0 || coordinates.y >= VoxelData.chunkHeight) return Voxels.Air.breakSpeed;

        if (chunkMap[chunkCoordinates.x, chunkCoordinates.z] != null && chunkMap[chunkCoordinates.x, chunkCoordinates.z].isEditable) {
            return Voxels.types[chunkMap[chunkCoordinates.x, chunkCoordinates.z].voxelMap[localCoordinates.x, localCoordinates.y, localCoordinates.z]].breakSpeed;
        }

        return Voxels.types[GenerateVoxelID(coordinates)].breakSpeed;
    }

    public byte blockIDAtCoordinates(Coordinates coordinates) {
        if (!voxelInWorld(coordinates)) return Voxels.Air.ID;

        ChunkCoordinates chunkCoordinates = new ChunkCoordinates(coordinates);
        Coordinates localCoordinates = Coordinates.GlobalToLocalOffset(coordinates, chunkCoordinates);

        if (!chunkInWorld(chunkCoordinates) || coordinates.y < 0 || coordinates.y >= VoxelData.chunkHeight) return Voxels.Air.ID;

        if (chunkMap[chunkCoordinates.x, chunkCoordinates.z] != null && chunkMap[chunkCoordinates.x, chunkCoordinates.z].isEditable) {
            return Voxels.types[chunkMap[chunkCoordinates.x, chunkCoordinates.z].voxelMap[localCoordinates.x, localCoordinates.y, localCoordinates.z]].ID;
        }

        return Voxels.types[GenerateVoxelID(coordinates)].ID;
    }

    bool chunkInWorld(ChunkCoordinates chunkCoordinates) {
        if (chunkCoordinates.x < 0 || chunkCoordinates.x >= VoxelData.worldChunkSize || chunkCoordinates.z < 0 || chunkCoordinates.z >= VoxelData.worldChunkSize) return false;
        else return true;
    }

    public bool voxelInWorld(Coordinates coordinates) {
        if (coordinates.x < 0 || coordinates.x >= VoxelData.worldSizeInVoxels || coordinates.y < 0 || coordinates.y >= VoxelData.chunkHeight || coordinates.z < 0 || coordinates.z >= VoxelData.worldSizeInVoxels) return false;
        else return true;
    }

    void CheckViewDistance() {

        ChunkCoordinates playerChunk = new ChunkCoordinates(player.position);
        previousPlayerChunkCoordinates = playerChunk;

        List<ChunkCoordinates> previouslyActiveChunks = new List<ChunkCoordinates>(activeChunks);
        activeChunks.Clear();

        for (int x = playerChunk.x - VoxelData.viewDistance / 2; x < playerChunk.x + VoxelData.viewDistance / 2; x++) {
            for (int z = playerChunk.z - VoxelData.viewDistance / 2; z < playerChunk.z + VoxelData.viewDistance / 2; z++) {

                ChunkCoordinates chunk = new ChunkCoordinates(x, z);

                // If the chunk is within the world bounds and it has not been created.
                if (chunkInWorld(chunk)) {

                    if (chunkMap[x, z] == null) {
                        chunkMap[x, z] = new Chunk(this, chunk);
                        chunksToCreate.Add(chunk);
                    } else if (!chunkMap[x, z].isActive) {
                        chunkMap[x, z].isActive = true;
                    }
                    activeChunks.Add(chunk);
                }

                // Check if this chunk was already in the active chunks list.
                for (int i = 0; i < previouslyActiveChunks.Count; i++) {
                    if (previouslyActiveChunks[i].x == x && previouslyActiveChunks[i].z == z)
                        previouslyActiveChunks.RemoveAt(i);
                }
            }
        }

        foreach (ChunkCoordinates coord in previouslyActiveChunks)
            chunkMap[coord.x, coord.z].isActive = false;

    }

    public byte GenerateVoxelID(Coordinates coordinates) {
        byte voxelID = Voxels.Air.ID;

        if (!voxelInWorld(coordinates)) voxelID = Voxels.Air.ID;

        int noiseValue = Mathf.RoundToInt(biome.heightMultiplierCurve.Evaluate(Noise.GetNoiseValue(coordinates.x, coordinates.z, biome.biomeSettings)) * biome.heightMultiplier) + biome.solidGroundHeight;
        if (noiseValue == coordinates.y) {
            voxelID = Voxels.Grass.ID;
        } else if (coordinates.y < noiseValue && coordinates.y > noiseValue - 3) {
            voxelID = Voxels.Dirt.ID;
        } else if (coordinates.y <= noiseValue - 3) {
            voxelID = Voxels.Stone.ID;
        }

        if (coordinates.y == noiseValue) {
            float treeZoneNoiseValue = Noise.GetNoiseValue(coordinates.x, coordinates.z, biome.treeZoneSettings);
            if (treeZoneNoiseValue > biome.treeZoneThreshold) {
                float treePlacementNoiseValue = Noise.GetNoiseValue(coordinates.x, coordinates.z, biome.treePlacementSettings);
                if (treePlacementNoiseValue > biome.treePlacementThreshold) {
                    voxelID = Voxels.Dirt.ID;
                    modifications.Enqueue(Structure.GenerateTree(coordinates, biome.minTreeHeight, biome.maxTreeHeight, biome.treeHeightSettings, Voxels.WoodLog.ID, Voxels.OakLeaves.ID));
                }
            } else {
                float birchTreeZoneNoiseValue = Noise.GetNoiseValue(coordinates.x + 1000, coordinates.z + 1000, biome.treeZoneSettings);
                if (birchTreeZoneNoiseValue > biome.treeZoneThreshold) {
                    float treePlacementNoiseValue = Noise.GetNoiseValue(coordinates.x + 1000, coordinates.z + 1000, biome.treePlacementSettings);
                    if (treePlacementNoiseValue > biome.treePlacementThreshold) {
                        voxelID = Voxels.Dirt.ID;
                        modifications.Enqueue(Structure.GenerateTree(coordinates, biome.minTreeHeight, biome.maxTreeHeight, biome.treeHeightSettings, Voxels.BirchLog.ID, Voxels.BirchLeaves.ID));
                    }
                }
            }
        }

        if (coordinates.y - 1 == noiseValue) {
            float flowerNoiseValue = Noise.GetNoiseValue(coordinates.x, coordinates.z, biome.flowerSettings);
            if (flowerNoiseValue > biome.flowerThreshold) {
                voxelID = Voxels.Rose.ID;
            } else {
                float grassNoiseValue = Noise.GetNoiseValue(coordinates.x, coordinates.z, biome.grassSettings);
                if (grassNoiseValue > biome.grassThreshold) {
                    if (grassNoiseValue > biome.grassThreshold * 1.5f) {
                        voxelID = Voxels.TallGrass.ID;
                    } else {
                        voxelID = Voxels.ShortGrass.ID;
                    }
                }
            }
        }

        return voxelID;
    }
}
