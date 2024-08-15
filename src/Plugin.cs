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
using System.Xml.Linq;

namespace TheVessel;

//[BepInDependency("fisobs", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("improved-input-config", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("slime-cubed.slugself", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("nc.Nuktils", BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin(MOD_ID, "The Vessel", "0.1.0")]
class Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "nc.TheVessel";

    public static readonly SlugcatStats.Name Vessel = new("nc.vessel");

    public static BepInEx.Logging.ManualLogSource Beplogger;

    public static bool isInit = false;


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

        //On.RainWorld.PostModsInit += RainWorld_PostModsInit;

        On.Player.CanMaulCreature += Player_CanMaulCreature;

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

    private bool Player_CanMaulCreature(On.Player.orig_CanMaulCreature orig, Player self, Creature crit)
    {
        if (self.IsScug(Vessel) && !Options.canPoisonMaul.Value)
            return false;
        return orig.Invoke(self, crit);
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
                self.mushroomEffect = 20;

            if (Options.recallSpear.Value && self.IsPressed(RecallSpear) && thrownSpears.TryGetValue(self, out var _))
            {
                Vector2 prevPos = self.firstChunk.pos;

                int freeHand = self.FreeHand();

                if (freeHand != -1 && self.GraspsHasType(AbstractPhysicalObject.AbstractObjectType.Spear) == -1)
                {
                    if (self.room != null && self.graphicsModule != null)
                    {
                        self.room.AddObject(new ShockWave(prevPos, 50.0f, 0.8f, 10));
                        self.room.AddObject(new ExplosionSpikes(self.room, prevPos, 10, 10.0f, 10, 10.0f, 80.0f, Color.white));

                        var handPos = ((PlayerGraphics)self.graphicsModule).hands[freeHand].pos;

                        self.room.AddObject(new ShockWave(handPos, 15.0f, 0.8f, 10));
                        self.room.AddObject(new ExplosionSpikes(self.room, handPos, 10, 5.0f, 10, 10.0f, 40.0f, Color.white));

                        self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, prevPos, 1.0f, 3.5f);
                    }

                    self.AllGraspsLetGoOfThisObject(true);
                    self.SlugcatGrab(self, freeHand);
                    thrownSpears.Remove(self);
                }
            }
        }
    }

    /*
    private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        //RotundWorldEnabled = ModManager.ActiveMods.Exists((ModManager.Mod mod) => mod.id == "willowwisp.bellyplus");
        orig(self);
    }
    */

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
