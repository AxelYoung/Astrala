using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelModification {
    public Coordinates coordinates { get; private set; }
    public byte id { get; private set; }

    public VoxelModification(Coordinates coordinates, byte id) {
        this.coordinates = coordinates;
        this.id = id;
    }
}
