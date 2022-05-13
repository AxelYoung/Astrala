
// Declare all Tools here

public static class Tools {
    public static readonly Tool WoodPick = new Tool(Tool.Type.Pick, 201, 0.33f);
    public static readonly Tool WoodShovel = new Tool(Tool.Type.Shovel, 202, 0.6f);
    public static readonly Tool WoodAxe = new Tool(Tool.Type.Axe, 203, 0.4f);
    public static readonly Tool WoodHoe = new Tool(Tool.Type.Hoe, 204, 0.5f);

    public static readonly Tool StonePick = new Tool(Tool.Type.Pick, 205, 0.15f);
    public static readonly Tool StoneShovel = new Tool(Tool.Type.Shovel, 206, 0.4f);
    public static readonly Tool StoneAxe = new Tool(Tool.Type.Axe, 207, 0.2f);
    public static readonly Tool StoneHoe = new Tool(Tool.Type.Hoe, 208, 0.33f);

    public static Tool[] tools = new Tool[] { WoodPick, WoodShovel, WoodAxe, WoodHoe, StonePick, StoneShovel, StoneAxe, StoneHoe };
}

public struct Tool {
    public enum Type {
        Empty,
        Pick,
        Shovel,
        Axe,
        Hoe
    }

    public Type type;
    public byte ID;
    public float multiplier;

    public Tool(Type type, byte ID, float multiplier) {
        this.type = type;
        this.ID = ID;
        this.multiplier = multiplier;
    }
}