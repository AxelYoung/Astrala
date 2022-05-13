
// Declare all Recipes for abstract crafting here

public static class Recipies {

    static byte[,] woodPlankRecipe = { { Voxels.WoodLog.ID } };
    public static Recipe woodPlanks = new Recipe(woodPlankRecipe, Voxels.WoodPlank.ID, 4);

    static byte[,] birchWoodPlankRecipe = { { Voxels.BirchLog.ID } };
    public static Recipe birchWoodPlanks = new Recipe(birchWoodPlankRecipe, Voxels.BirchPlank.ID, 4);

    static byte[,] stickRecipe = { { Voxels.WoodPlank.ID }, { Voxels.WoodPlank.ID } };
    public static Recipe sticks = new Recipe(stickRecipe, Items.Stick, 4);
    static byte[,] birchStickRecipe = { { Voxels.BirchPlank.ID }, { Voxels.BirchPlank.ID } };
    public static Recipe birchSticks = new Recipe(birchStickRecipe, Items.Stick, 4);

    static byte[,] craftingTableRecipe = { { Voxels.WoodPlank.ID, Voxels.WoodPlank.ID }, { Voxels.WoodPlank.ID, Voxels.WoodPlank.ID } };
    public static Recipe craftingTable = new Recipe(craftingTableRecipe, Voxels.CraftingTable.ID, 1);

    static byte[,] birchCraftingTableRecipe = { { Voxels.BirchPlank.ID, Voxels.BirchPlank.ID }, { Voxels.BirchPlank.ID, Voxels.BirchPlank.ID } };
    public static Recipe birchCraftingTable = new Recipe(birchCraftingTableRecipe, Voxels.CraftingTable.ID, 1);

    static byte[,] woodPickRecipe = { { Voxels.WoodPlank.ID, Voxels.WoodPlank.ID, Voxels.WoodPlank.ID }, { 0, Items.Stick, 0 }, { 0, Items.Stick, 0 } };
    public static Recipe woodPick = new Recipe(woodPickRecipe, Tools.WoodPick.ID, 1);


    static byte[,] birchWoodPickRecipe = { { Voxels.BirchPlank.ID, Voxels.BirchPlank.ID, Voxels.BirchPlank.ID }, { 0, Items.Stick, 0 }, { 0, Items.Stick, 0 } };
    public static Recipe birchWoodPick = new Recipe(birchWoodPickRecipe, Tools.WoodPick.ID, 1);

    static byte[,] stonePickRecipe = { { Voxels.Cobblestone.ID, Voxels.Cobblestone.ID, Voxels.Cobblestone.ID }, { 0, Items.Stick, 0 }, { 0, Items.Stick, 0 } };
    public static Recipe stonePick = new Recipe(stonePickRecipe, Tools.StonePick.ID, 1);

    static byte[,] woodShovelRecipe = { { Voxels.WoodPlank.ID }, { Items.Stick }, { Items.Stick } };
    public static Recipe woodShovel = new Recipe(woodShovelRecipe, Tools.WoodShovel.ID, 1);

    static byte[,] birchWoodShovelRecipe = { { Voxels.BirchPlank.ID }, { Items.Stick }, { Items.Stick } };
    public static Recipe birchWoodShovel = new Recipe(birchWoodShovelRecipe, Tools.WoodShovel.ID, 1);

    static byte[,] stoneShovelRecipe = { { Voxels.Cobblestone.ID }, { Items.Stick }, { Items.Stick } };
    public static Recipe stoneShovel = new Recipe(stoneShovelRecipe, Tools.StoneShovel.ID, 1);

    static byte[,] woodAxeRecipe = { { Voxels.WoodPlank.ID, Voxels.WoodPlank.ID }, { Voxels.WoodPlank.ID, Items.Stick }, { 0, Items.Stick } };
    public static Recipe woodAxe = new Recipe(woodAxeRecipe, Tools.WoodAxe.ID, 1);

    static byte[,] birchWoodAxeRecipe = { { Voxels.BirchPlank.ID, Voxels.BirchPlank.ID }, { Voxels.BirchPlank.ID, Items.Stick }, { 0, Items.Stick } };
    public static Recipe birchWoodAxe = new Recipe(birchWoodAxeRecipe, Tools.WoodAxe.ID, 1);

    static byte[,] stoneAxeRecipe = { { Voxels.Cobblestone.ID, Voxels.Cobblestone.ID }, { Voxels.Cobblestone.ID, Items.Stick }, { 0, Items.Stick } };
    public static Recipe stoneAxe = new Recipe(stoneAxeRecipe, Tools.StoneAxe.ID, 1);

    static byte[,] woodHoeRecipe = { { Voxels.WoodPlank.ID, Voxels.WoodPlank.ID }, { 0, Items.Stick }, { 0, Items.Stick } };
    public static Recipe woodHoe = new Recipe(woodHoeRecipe, Tools.WoodHoe.ID, 1);

    static byte[,] birchWoodHoeRecipe = { { Voxels.BirchPlank.ID, Voxels.BirchPlank.ID }, { 0, Items.Stick }, { 0, Items.Stick } };
    public static Recipe birchWoodHoe = new Recipe(birchWoodHoeRecipe, Tools.WoodHoe.ID, 1);

    static byte[,] stoneHoeRecipe = { { Voxels.Cobblestone.ID, Voxels.Cobblestone.ID }, { 0, Items.Stick }, { 0, Items.Stick } };
    public static Recipe stoneHoe = new Recipe(stoneHoeRecipe, Tools.StoneHoe.ID, 1);

    public static readonly Recipe[] recipes = { woodPlanks, sticks, craftingTable,
    woodPick, woodShovel, woodAxe, woodHoe,
    birchWoodPlanks, birchSticks, birchCraftingTable, birchWoodPick, birchWoodShovel, birchWoodAxe, birchWoodHoe,
    stonePick, stoneShovel, stoneAxe, stoneHoe };
}

public struct Recipe {
    public byte[,] itemIDs { get; private set; }
    public byte outputID { get; private set; }
    public byte quantity { get; private set; }

    public Recipe(byte[,] itemIDs, byte outputID, byte quantity) {
        this.itemIDs = itemIDs;
        this.outputID = outputID;
        this.quantity = quantity;
    }
}