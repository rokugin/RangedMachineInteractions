using HarmonyLib;
using RangedMachineInteractions.Patches;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Mods;

namespace RangedMachineInteractions;

internal class ModEntry : Mod {

    public static ModHooks Hooks = new ModHooks();
    public static IMonitor SMonitor = null!;
    public static ModConfig Config = new();

    public override void Entry(IModHelper helper) {
        I18n.Init(helper.Translation);
        SMonitor = Monitor;
        Config = helper.ReadConfig<ModConfig>();

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;

        var harmony = new Harmony(ModManifest.UniqueID);

        harmony.Patch(
            AccessTools.Method(typeof(Game1), nameof(Game1.tryToCheckAt)),
            prefix: new HarmonyMethod(typeof(Game1Patch), nameof(Game1Patch.TryToCheckAt_Prefix))
        );
    }

    private void OnGameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e) {
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is null) return;

        configMenu.Register(ModManifest, () => Config = new ModConfig(), () => Helper.WriteConfig(Config));
        configMenu.AddSectionTitle(ModManifest, I18n.ConfigMachinePriority_Title);
        configMenu.AddBoolOption(
            ModManifest,
            () => Config.MachinePriority,
            value => Config.MachinePriority = value,
            I18n.ConfigMachinePriority_Name,
            I18n.ConfigMachinePriority_Tooltip
        );
    }

}