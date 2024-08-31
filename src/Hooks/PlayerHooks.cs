using System;
using BepInEx;
using UnityEngine;
using ImprovedInput;
using RWCustom;
using System.Runtime.CompilerServices;
using static Nuktils.Utils;
using static Nuktils.Extensions;
using static TheVessel.Plugin;
using static TheVessel.CWTs;
using static TheVessel.Utils;
using MoreSlugcats;

namespace TheVessel.Hooks;

internal static class PlayerHooks
{
    public static void ApplyPlayerHooks()
    {
        On.Player.ctor += Player_ctor;

        On.Player.Update += Player_Update;

        On.Player.ObjectEaten += Player_ObjectEaten;

        On.Player.CanMaulCreature += Player_CanMaulCreature;

        On.Player.MaulingUpdate += Player_ApplyMaulPoison;

        On.Creature.Violence += Creature_Violence;

        On.Creature.Grab += Creature_Grab;
    }

    private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (self.IsScug(Vessel))
        {
            if (!dashCharges.TryGetValue(self, out var _))
                dashCharges.Add(self, new(0));
        }
    }

    private static bool Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        bool res = orig.Invoke(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
        if (self is Lizard && self.grasps[graspUsed].grabbed is Creature && res && Options.electricuteLizardsOnGrab.Value)
        {
            if ((self.grasps[graspUsed].grabbed as Creature).IsScug(Vessel))
            {
                Blogger.LogDebug("lizard has grabbed a Vessel instance, electrocuting...");
                self.Violence(self.firstChunk, new Vector2?(Custom.DirVec(self.firstChunk.pos, self.firstChunk.pos) * 5f), self.firstChunk, null, Creature.DamageType.Electric, 0.1f, 320f * Mathf.Lerp(self.Template.baseStunResistance, 1f, 0.5f));
                self.room.AddObject(new CreatureSpasmer(self, false, 10));
                self.room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, self.firstChunk.pos);
                self.room.AddObject(new Explosion.ExplosionLight(self.firstChunk.pos, 200f, 1f, 4, new Color(0.7f, 1f, 1f)));
                Blogger.LogDebug("electrocuted");
            }
        }
        return res;
    }

    private static void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
    {
        Blogger.LogDebug($"Creature.Violence invoked: is Vessel? {self.IsScug(Vessel)}(actual type: '{self}'), noExplosiveOrElectricDamage? {Options.noExplosiveOrElectricDamage.Value}, type? '{type}'");
        if (self.IsScug(Vessel) && Options.noExplosiveOrElectricDamage.Value && (type == Creature.DamageType.Electric || type == Creature.DamageType.Explosion))
        {
            //var prevDamage = damage;
            //var prevStunBonus = stunBonus;
            //damage = 0;
            //stunBonus = Math.Min(0, stunBonus - 6);
            //Blogger.LogDebug($"changed damage and stunBonus from {prevDamage}/{prevStunBonus} to {damage}/{stunBonus}");
            Blogger.LogDebug("Creature.Violence invocation stopped");
            return;
        }
        else
        {
            Blogger.LogDebug("Creature.Violence invocation ignored");
        }
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }

    private static void Player_ApplyMaulPoison(On.Player.orig_MaulingUpdate orig, Player self, int graspIndex)
    {
        orig(self, graspIndex);
        if (self.IsScug(Vessel) && self.grasps[graspIndex].grabbed is Creature && self.maulTimer > 15)
        {
            if (!poisonedCreatures.TryGetValue(self.grasps[graspIndex].grabbed as Creature, out var _))
                poisonedCreatures.Add(self.grasps[graspIndex].grabbed as Creature, new CreaturePoison(Intervals.Second * 5, 2f, 3));
        }
    }

    private static bool Player_CanMaulCreature(On.Player.orig_CanMaulCreature orig, Player self, Creature crit)
    {
        if (self.IsScug(Vessel) && !Options.canPoisonMaul.Value)
            return false;
        return orig.Invoke(self, crit);
    }

    private static void Player_ObjectEaten(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible edible)
    {
        orig(self, edible);

        if (edible is Mushroom)
        {
            self.mushroomCounter = 0;
            self.mushroomEffect = 0;
            self.Die();
        }
    }

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        //if (self.State is HealthState)
        //    Blogger.LogDebug($"player health: {(self.State as HealthState).health}");

        if (self.IsScug(Vessel))
        {
            if (Options.slowTime.Value && self.IsPressed(SlowTime))
                self.mushroomCounter = 1;
            else if (self.mushroomCounter != 0)
                self.mushroomCounter = 0;

            {
                if (Options.canDash.Value && self.IsPressed(Dash) && dashCharges.TryGetValue(self, out Timer dash) && dash.Ended())
                {
                    //var pastPos = self.bodyChunks[0].pos;

                    float num3 = self.input[0].x;
                    float num4 = self.input[0].y;
                    /*
                    while (num3 == 0f && num4 == 0f)
                    {
                        num3 = (((double)UnityEngine.Random.value <= 0.33) ? 0 : (((double)UnityEngine.Random.value <= 0.5) ? 1 : -1));
                        num4 = (((double)UnityEngine.Random.value <= 0.33) ? 0 : (((double)UnityEngine.Random.value <= 0.5) ? 1 : -1));
                    }
                    */

                    self.room.PlaySound(SoundID.Vulture_Jet_LOOP, self.bodyChunks[0].pos, 2.0f, 3.5f);
                    self.room.AddObject(new ExplosionSpikes(self.room, self.bodyChunks[0].pos, 10, 5.0f, 10, 10.0f, 40.0f, Color.white));

                    //for (int i = 0; i < 5; i++)
                    //    self.room.AddObject(new LightningBolt(self.bodyChunks[0].pos, self.bodyChunks[0].pos + new Vector2(UnityEngine.Random.value, UnityEngine.Random.value), 0, 1f, 2f));

                    var prevPos = self.bodyChunks[0].pos;

                    self.bodyChunks[0].vel.x = 24f * num3;
                    self.bodyChunks[0].vel.y = 24f * num4;
                    self.bodyChunks[1].vel.x = 23f * num3;
                    self.bodyChunks[1].vel.y = 23f * num4;

                    self.room.PlaySound(SoundID.Vulture_Jet_LOOP, self.bodyChunks[0].pos, 2.0f, 3.5f);
                    self.room.AddObject(new ExplosionSpikes(self.room, self.bodyChunks[0].pos, 10, 5.0f, 10, 10.0f, 40.0f, Color.white));

                    for (int i = 0; i < 5; i++)
                    {
                        self.room.AddObject(new LightningBolt(self.firstChunk.pos, prevPos, 0, 0.4f, 0.35f, 0.64f, 0.64f, true)
                        {
                            intensity = 1f,
                            color = Color.white
                        });
                    }

                    //self.room.AddObject(new LightningMachine.Impact(self.firstChunk.pos, 0.5f, Color.red, true));

                    for (int i = 0; i < 5; i++)
                        self.room.AddObject(new Spark(self.bodyChunks[0].pos,
                            new(UnityEngine.Random.value * (UnityEngine.Random.value * 10f),
                            UnityEngine.Random.value * (UnityEngine.Random.value * 10f)),
                            new(0.3f, 0.3f, 1f, 1f), null, 10, 20));

                    dashCharges.Remove(self);
                    dashCharges.Add(self, new(15));
                }
            }

            {
                if (Options.canDash.Value && dashCharges.TryGetValue(self, out Timer dash))
                    dash.Tick();
            }

            if (Options.recallSpear.Value && self.IsPressed(RecallSpear) && thrownSpears.TryGetValue(self, out Spear spear) && !self.inShortcut)
            {
                Blogger.LogDebug("a spear is being recalled...");
                // a lot of this was stolen from PearlCat: https://github.com/forthfora/pearlcat/blob/9038d81f6292d871ca1291067e7588e8b5d6557b/src/Hooks/World/World.cs#L536
                Vector2 prevPos = spear.room.Equals(self.room) ? spear.firstChunk.pos : self.firstChunk.pos;
                int freeHand = self.FreeHand();

                if (freeHand != -1 && self.GraspsHasType(AbstractPhysicalObject.AbstractObjectType.Spear) == -1)
                {
                    if (self.room != null && self.graphicsModule != null)
                    {
                        //self.room.AddObject(new ShockWave(prevPos, 50.0f, 0.8f, 10));
                        //self.room.AddObject(new ExplosionSpikes(self.room, prevPos, 10, 10.0f, 10, 10.0f, 80.0f, Color.white));

                        var handPos = ((PlayerGraphics)self.graphicsModule).hands[freeHand].pos;

                        //self.room.AddObject(new ShockWave(handPos, 15.0f, 0.8f, 10));
                        //self.room.AddObject(new ExplosionSpikes(self.room, handPos, 10, 5.0f, 10, 10.0f, 40.0f, Color.white));

                        //self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, prevPos, 1.0f, 3.5f);
                        self.room.AddObject(new LightningBolt(prevPos, handPos, 0, 2f, 2f));
                        //self.room.AddObject(new LightningMachine(handPos, prevPos, handPos, 90, false, false, 2f, 1f, 3f));
                        self.room.AddObject(new LightningMachine.Impact(handPos, 0.5f, Color.white, true));
                        self.room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, handPos, 1.0f, 3.5f);
                    }

                    try
                    {
                        Blogger.LogDebug("recreating spear...");

                        if (!spearsToRemoveFromCreatures.TryGetValue(spear.abstractPhysicalObject, out var _))
                            spearsToRemoveFromCreatures.Add(spear.abstractPhysicalObject, new Empty());
                        spear.abstractSpear.stuckInWallCycles = 0;

                        if (spear.IsNeedle) spear.Spear_NeedleDisconnect();
                        spear.PulledOutOfStuckObject();
                        spear.AllGraspsLetGoOfThisObject(true);
                        spear.ChangeMode(Weapon.Mode.Free);
                        spear.PickedUp(self);
                        spear.RemoveFromRoom();

                        Spear newSpear = new(
                                new AbstractSpear(
                                    self.room.world,
                                    null,
                                    self.room.GetWorldCoordinate(self.firstChunk.pos),
                                    self.room.game.GetNewID(),
                                    false
                                ),
                                self.room.world
                            );
                        Blogger.LogDebug("new spear created, copying original stats...");
                        newSpear.CopyStats(spear);
                        Blogger.LogDebug("stats copied");

                        self.room.AddObject(newSpear);
                        self.SlugcatGrab(newSpear, freeHand);
                        thrownSpears.Remove(self);
                        thrownSpears.Add(self, newSpear);

                        Blogger.LogDebug("successfully recalled spear");

                        // use this code if the room-inexclusive code doesn't work
                        /*
                        if (spear.room == self.room)
                        {
                            Blogger.LogDebug("the spear is in player's room, moving...");
                            spear.PulledOutOfStuckObject();
                            spear.AllGraspsLetGoOfThisObject(true);
                            spear.ChangeMode(Weapon.Mode.Free);
                            self.SlugcatGrab(spear, freeHand);
                            thrownSpears.Remove(self);
                            Blogger.LogDebug("successfully recalled spear");
                        }
                        else
                        {
                            Blogger.LogDebug("the spear is not in player's room, ignoring...");
                        }
                        */
                    }
                    catch (Exception e)
                    {
                        Blogger.LogError($"exception while recalling a spear: {e}");
                    }
                }
            }
        }
    }
}
