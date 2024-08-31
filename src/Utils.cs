using ImprovedInput;
using UnityEngine;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;

namespace TheVessel
{
    public static class Utils
    {
        public static void CopyStats(this Spear self, Spear other)
        {
            self.abstractSpear.electric = other.abstractSpear.electric;
            self.abstractSpear.electricCharge = other.abstractSpear.electricCharge;
            self.abstractSpear.needle = other.abstractSpear.needle;
            self.abstractSpear.hue = other.abstractSpear.hue;
            self.abstractSpear.type = other.abstractSpear.type;
            self.buoyancy = other.buoyancy;
            self.spearDamageBonus = other.spearDamageBonus;
            self.spearmasterNeedle = other.spearmasterNeedle;
            self.spearmasterNeedleType = other.spearmasterNeedleType;
            self.spearmasterNeedle_fadecounter = other.spearmasterNeedle_fadecounter;
            self.spearmasterNeedle_fadecounter_max = other.spearmasterNeedle_fadecounter_max;
        }

        public static void PlayRandomSoundInRoom(this Room room, Vector2 pos, float vol, float pitch, params SoundID[] sounds)
        {
            if (room == null || sounds.Length == 0)
                return;

            System.Random rand = new();
            room.PlaySound(sounds[rand.Next(sounds.Length)], pos, vol, pitch);
        }
    }
}
