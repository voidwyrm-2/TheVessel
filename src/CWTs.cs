using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Nuktils.Utils;

namespace TheVessel
{
    internal static class CWTs
    {
        //public static ConditionalWeakTable<Player, Dictionary<string, int>> playerTimers = new();
        internal readonly static ConditionalWeakTable<Player, Spear> thrownSpears = new(); // keeps track of spears thrown
        internal class Empty { }
        internal readonly static ConditionalWeakTable<AbstractPhysicalObject, Empty> spearsToRemoveFromCreatures = new(); // keeps track of spears that need to be removed from creatures
        internal readonly static ConditionalWeakTable<Creature, CreaturePoison> poisonedCreatures = new(); // keeps track of which creatures are poisoned
        internal readonly static ConditionalWeakTable<Player, Timer> dashCharges = new(); // keeps track of which players can dash
        internal readonly static ConditionalWeakTable<Player, Timer> talkTimers = new(); // keeps track of player talk cooldowns
    }
}
