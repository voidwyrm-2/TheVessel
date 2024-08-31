using System;
using BepInEx;
using UnityEngine;
using ImprovedInput;
using RWCustom;
using System.Runtime.CompilerServices;
using static Nuktils.Utils;
using static Nuktils.Extensions;

namespace TheVessel;

//[BepInDependency("fisobs", BepInDependency.DependencyFlags.HardDependency)]
//[BepInDependency("improved-input-config", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("slime-cubed.slugbase", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("nc.Nuktils", BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin(MOD_ID, "The Vessel", "0.1.0")]
class Plugin : BaseUnityPlugin
{
    internal const string MOD_ID = "nc.TheVessel";

    internal static readonly SlugcatStats.Name Vessel = new("nc.vessel");

    internal static BepInEx.Logging.ManualLogSource Blogger;

    private static bool isInit = false;


    internal static PlayerKeybind SlowTime;
    internal static PlayerKeybind RecallSpear;
    internal static PlayerKeybind Dash;

    public void OnEnable()
    {
        Blogger = Logger;

        try
        {
            SlowTime = PlayerKeybind.Register("nc_vessel:slowtime", "The Vessel", "Slow Time", KeyCode.T, KeyCode.JoystickButton3);
            RecallSpear = PlayerKeybind.Register("nc_vessel:recallspear", "The Vessel", "Recall Spear", KeyCode.Y, KeyCode.JoystickButton3);
            Dash = PlayerKeybind.Register("nc_vessel:dash", "The Vessel", "Dash", KeyCode.N, KeyCode.JoystickButton3);
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }

        On.RainWorld.OnModsInit += RainWorld_LoadOptions;

        On.RainWorld.OnModsInit += RainWorld_SetControlDescriptions;

        Hooks.PlayerHooks.ApplyPlayerHooks();
        Hooks.WorldHooks.ApplyWorldHooks();
    }

    private void RainWorld_SetControlDescriptions(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        try
        {
            if (!isInit)
            {
                isInit = true;
                SlowTime.Description = "The key held to slow time.";
                RecallSpear.Description = "The key held to recall the last spear you threw.";
                Dash.Description = "The key pressed to dash.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
        finally
        {
            orig.Invoke(self);
        }
    }

    private void RainWorld_LoadOptions(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        Logger.LogInfo("Loading remix options...");
        MachineConnector.SetRegisteredOI(MOD_ID, new Options());
        Logger.LogInfo("Loading complete!");
    }
}