using System;
using BepInEx;
using UnityEngine;
using ImprovedInput;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RWCustom;
using System.Runtime.CompilerServices;
using static Nuktils.Utils;
using static Nuktils.Extensions;

namespace TheVessel;

//[BepInDependency("fisobs", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("improved-input-config", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("slime-cubed.slugself", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("nc.Nuktils", BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin(MOD_ID, "The Vessel", "0.1.0")]
class Plugin : BaseUnityPlugin
{
    public static BepInEx.Logging.ManualLogSource Beplogger;

    public static bool isInit = false;

    public const string MOD_ID = "nc.TheVessel";

    public static readonly SlugcatStats.Name Vessel = new("nc.vessel");

    public static bool RotundWorldEnabled;

    //public static bool ImprovedInputConfigEnabled;

    public bool isFinished = true;


    public static PlayerKeybind SlowTime;

    public static PlayerKeybind RecallSpear;


    public static ConditionalWeakTable<Player, Spear> thrownSpears = new();
    public static ConditionalWeakTable<Creature, CreaturePoison> poisonedCreatures = new();

    public void OnEnable()
    {
        Beplogger = Logger;

        try
        {
            SlowTime = PlayerKeybind.Register("nc_vessel:slowtime", "The Vessel", "Slow Time", KeyCode.T, KeyCode.JoystickButton3);
            RecallSpear = PlayerKeybind.Register("nc_vessel:recallspear", "The Vessel", "Recall Spear", KeyCode.Y, KeyCode.JoystickButton3);
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }

        On.RainWorld.OnModsInit += RainWorld_LoadOptions;

        On.RainWorld.OnModsInit += RainWorld_SetControlDescriptions;

        On.RainWorld.PostModsInit += RainWorld_PostModsInit;

        On.Player.Update += Player_Update;

        On.Player.MaulingUpdate += Player_MaulingUpdate;

        On.Player.ObjectEaten += Player_ObjectEaten;

        On.Spear.Thrown += Spear_Thrown;

        On.Creature.Update += Creature_Update;
    }

    #region poisonMaul
    private void Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        orig(self, eu);

        if (poisonedCreatures.TryGetValue(self, out CreaturePoison poison))
        {
            if (poison.Decrement() || self.dead)
            {
                poisonedCreatures.Remove(self);
            }
            else
            {
                poison.ApplyToCreature(self);
            }
        }
    }

    private void Player_MaulingUpdate(On.Player.orig_MaulingUpdate orig, Player self, int graspIndex)
    {
        orig(self, graspIndex);
        if (self.IsScug(Vessel) && self.grasps[graspIndex].grabbed is Creature)
        {
            if (!poisonedCreatures.TryGetValue(self.grasps[graspIndex].grabbed as Creature, out var _))
                poisonedCreatures.Add(self.grasps[graspIndex].grabbed as Creature, new CreaturePoison(Ticks.Second * 5, 2f));
        }
    }
    #endregion

    private void Spear_Thrown(On.Spear.orig_Thrown orig, Spear self, Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
        if (thrownBy.IsScug(Vessel))
        {
            if (thrownSpears.TryGetValue((thrownBy as Player), out var _))
                thrownSpears.Remove((thrownBy as Player));
            thrownSpears.Add((thrownBy as Player), self);
        }
        orig(self, thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
    }

    private void Player_ObjectEaten(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible edible)
    {
        orig(self, edible);

        if (edible is Mushroom)
        {
            self.mushroomCounter = 0;
            self.mushroomEffect = 0;
            self.Die();
        }
    }

    private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (self.IsScug(Vessel))
        {
            if (Options.slowTime.Value && self.IsPressed(SlowTime))
                self.mushroomCounter = 10;

            if (Options.recallSpear.Value && self.IsPressed(RecallSpear) && thrownSpears.TryGetValue(self, out Spear spear))
            {
                int freeHand = self.FreeHand();
                if (self.GraspsHasType(AbstractPhysicalObject.AbstractObjectType.Spear) != -1 && freeHand != -1)
                {
                    //spear.firstChunk.pos = self.firstChunk.pos;
                    spear.thrownPos = self.bodyChunks[0].pos;
                    self.SlugcatGrab(self, freeHand);
                    //AbstractPhysicalObject x = new(self.room.world, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null, self.coord, self.room.game.GetNewID());
                    //var y = new FlareBomb(x, self.room.world);
                    //y.StartBurn();
                }
            }
        }
    }

    private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        RotundWorldEnabled = ModManager.ActiveMods.Exists((ModManager.Mod mod) => mod.id == "willowwisp.bellyplus");
        //ImprovedInputConfigEnabled = ModManager.ActiveMods.Exists((ModManager.Mod mod) => mod.id == "improved-input-config");
        orig(self);
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
