using StardewValley.Buildings;
using StardewValley;
using xTile.Dimensions;
using SObject = StardewValley.Object;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;

namespace RangedMachineInteractions.Patches;

static class Game1Patch {

    static bool CheckMachinePriority() {
        GameLocation location = Game1.currentLocation;
        Vector2 tile = Game1.player.GetGrabTile();

        Building building = location.getBuildingAt(tile);
        bool noBuilding = building == null;

        bool buildingActionable = false;
        if (building != null) buildingActionable = building.isActionableTile((int)tile.X, (int)tile.Y, Game1.player);

        string tileProperty = location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Action", "Buildings");
        bool noTileProperty = tileProperty == null;

        NPC character = location.isCharacterAtTile(tile);
        bool noCharacter = character == null;

        SObject obj = null;//location.getObjectAtTile((int)tile.X, (int)tile.Y);
        bool noObject = obj == null;

        bool flag = (noBuilding || !buildingActionable) && noTileProperty && noCharacter && noObject;
        ModEntry.SMonitor.Log($"Data output:\nBuilding: {(noBuilding ? "none" : building.GetIndoorsName())}\n" +
            $"Building actionable: {buildingActionable}\n" +
            $"Tile property: {(noTileProperty ? "none" : tileProperty)}\n" +
            $"Character: {(noCharacter ? "none" : character.Name)}\n" +
            $"Object: {(noObject ? "none" : obj.Name)}\n"
            , StardewModdingAPI.LogLevel.Trace);
        if (ModEntry.Config.MachinePriority) flag = true;
        return flag;
    }

    public static bool TryToCheckAt_Prefix(ref bool __result, Vector2 grabTile, Farmer who) {
        try {
            if (who.onBridge.Value) {
                return true;
            }

            bool flag = false;
            grabTile = Game1.currentCursorTile;
            SObject obj = Game1.currentLocation.getObjectAtTile((int)grabTile.X, (int)grabTile.Y);

            if (obj != null) flag = CheckMachinePriority() && obj.HasTypeBigCraftable() && obj.GetMachineData() != null;

            Game1.haltAfterCheck = true;
            if (!flag && !Utility.tileWithinRadiusOfPlayer((int)grabTile.X, (int)grabTile.Y, 1, who)) {
                grabTile = who.GetGrabTile();
            }

            if (ModEntry.Hooks.OnGameLocation_CheckAction(Game1.currentLocation, new Location((int)grabTile.X, (int)grabTile.Y), Game1.viewport, who,
            () => Game1.currentLocation.checkAction(new Location((int)grabTile.X, (int)grabTile.Y), Game1.viewport, who))) {
                Game1.updateCursorTileHint();
                who.lastGrabTile = grabTile;

                if (who.CanMove && Game1.haltAfterCheck) {
                    who.faceGeneralDirection(grabTile * 64f);
                    who.Halt();
                }

                Game1.oldKBState = Game1.GetKeyboardState();
                Game1.oldMouseState = Game1.input.GetMouseState();
                Game1.oldPadState = Game1.input.GetGamePadState();

                __result = true;
                return false;
            }

            __result = false;
            return false;
        }
        catch (Exception ex) {
            ModEntry.SMonitor.Log($"\nFailed in {nameof(TryToCheckAt_Prefix)}:\n{ex}", StardewModdingAPI.LogLevel.Error);

            return true;
        }
    }

}