
// Declare all Voxels here

public static class Voxels {

    public static readonly byte voxelAmount = 18;

    public static readonly Voxel Air = new Voxel(0, false, true, 0);
    public static readonly Voxel Grass = new Voxel(1, true, false, 2, 2, false, Tool.Type.Shovel);
    public static readonly Voxel Dirt = new Voxel(2, true, false, 2, 2, false, Tool.Type.Shovel);
    public static readonly Voxel Stone = new Voxel(3, true, false, 4, 9, true, Tool.Type.Pick);
    public static readonly Voxel WoodLog = new Voxel(4, true, false, 3, 4, false, Tool.Type.Axe);
    public static readonly Voxel WoodPlank = new Voxel(5, true, false, 3, 5, false, Tool.Type.Axe);
    public static readonly Voxel OakLeaves = new Voxel(6, true, true, 1, 6, true, Tool.Type.Hoe);
    public static readonly Voxel Glass = new Voxel(7, true, true, 1);
    public static readonly Voxel CraftingTable = new Voxel(8, true, false, 3, 8, false, Tool.Type.Axe);
    public static readonly Voxel Cobblestone = new Voxel(9, true, false, 4, 9, true, Tool.Type.Pick);
    public static readonly Voxel Gravel = new Voxel(10, true, false, 2, 10, false, Tool.Type.Shovel);
    public static readonly Voxel IronOre = new Voxel(11, true, false, 4, Items.RawIronOre, true, Tool.Type.Pick);
    public static readonly Voxel DiamondOre = new Voxel(12, true, false, 4, Items.Diamond, true, Tool.Type.Pick);
    public static readonly Voxel BirchLog = new Voxel(13, true, false, 3, 13, false, Tool.Type.Axe);
    public static readonly Voxel BirchPlank = new Voxel(14, true, false, 3, 14, false, Tool.Type.Axe);
    public static readonly Voxel BirchLeaves = new Voxel(15, true, true, 1, 15, true, Tool.Type.Hoe);

    public static readonly Voxel Rose = new Voxel(16, true, true, 0, 16, false, Tool.Type.Hoe, true);
    public static readonly Voxel TallGrass = new Voxel(17, true, true, 0, 17, true, Tool.Type.Hoe, true);
    public static readonly Voxel ShortGrass = new Voxel(18, true, true, 0, 18, true, Tool.Type.Hoe, true);

    public static Voxel[] types = { Air, Grass, Dirt, Stone, WoodLog, WoodPlank, OakLeaves, Glass, CraftingTable, Cobblestone, Gravel, IronOre, DiamondOre, BirchLog, BirchPlank, BirchLeaves, Rose, TallGrass, ShortGrass };
}

public struct Voxel {
    public byte ID { get; private set; }
    public bool isSolid { get; private set; }
    public bool isTransparent { get; private set; }
    public byte breakSpeed { get; private set; }
    public byte blockDrop { get; private set; }
    public bool toolRequired { get; private set; }
    public Tool.Type effectiveTool { get; private set; }
    public bool isFoliage { get; private set; }

    public Voxel(byte ID, bool isSolid, bool isTransparent, byte breakSpeed = 0, byte blockDrop = 0, bool toolRequired = false, Tool.Type effectiveTool = Tool.Type.Empty, bool isFoliage = false) {
        this.ID = ID;
        this.isSolid = isSolid;
        this.isTransparent = isTransparent;
        this.breakSpeed = breakSpeed;
        this.blockDrop = blockDrop;
        this.toolRequired = toolRequired;
        this.effectiveTool = effectiveTool;
        this.isFoliage = isFoliage;
    }
}