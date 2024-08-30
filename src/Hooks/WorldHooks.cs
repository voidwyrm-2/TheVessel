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

namespace TheVessel.Hooks;

internal static class WorldHooks
{
    public static void ApplyWorldHooks()
    {
        On.Spear.Thrown += Spear_Thrown;

        On.Creature.Update += Creature_Update;
    }

    private static void Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        orig(self, eu);

        if (poisonedCreatures.TryGetValue(self, out CreaturePoison poison))
        {
            poison.Tick();

            if (poison.Ended() || self.dead)
            {
                poisonedCreatures.Remove(self);
            }
            else
            {
                poison.ApplyToCreature(self);
            }
        }

        for (int i = 0; i < self.abstractPhysicalObject.stuckObjects.Count; i++)
        {
            if (self.abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick)
            {
                if (spearsToRemoveFromCreatures.TryGetValue((self.abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).Spear, out var _))
                    self.abstractPhysicalObject.stuckObjects[i].Deactivate();
            }
        }
    }

    private static void Spear_Thrown(On.Spear.orig_Thrown orig, Spear self, Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
        if (thrownBy.IsScug(Vessel) && !self.abstractSpear.explosive)
        {
            if (thrownSpears.TryGetValue(thrownBy as Player, out var _))
                thrownSpears.Remove(thrownBy as Player);
            thrownSpears.Add(thrownBy as Player, self);
        }
        orig(self, thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
    }
}

