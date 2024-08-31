using Menu.Remix.MixedUI;
using UnityEngine;
using static Nuktils.Options;

namespace TheVessel;

sealed class Options : OptionInterface
{
    //taken from https://github.com/Dual-Iron/no-damage-rng/blob/master/src/Options.cs

    public static Configurable<bool> slowTime;
    //public static Configurable<int> mushroomEffect;
    public static Configurable<bool> recallSpear;
    public static Configurable<bool> canPoisonMaul;
    public static Configurable<bool> noExplosiveOrElectricDamage;
    public static Configurable<bool> electricuteLizardsOnGrab;
    public static Configurable<bool> canDash;

    public Options()
    {
        slowTime = config.Bind("nc_slowTime", true);
        //mushroomEffect = config.Bind("nc_mushroomEffect", 10, new ConfigAcceptableRange<int>(1, 100));
        recallSpear = config.Bind("nc_recallSpear", true);
        canPoisonMaul = config.Bind("nc_canPoisonMaul", true);
        noExplosiveOrElectricDamage = config.Bind("nc_noExplosiveOrElectricDamage", true);
        electricuteLizardsOnGrab = config.Bind("nc_electricuteLizardsOnGrab", true);
        canDash = config.Bind("nc_canDash", true);
    }

    public override void Initialize()
    {
        base.Initialize();

        Tabs = new OpTab[] { new(this) };

        var labelTitle = new OpLabel(20, 600 - 30, "The Vessel Options", true);

        var top = 550;
        ILabeledPair[] labelCheckboxPairs =
        {
            new LabeledCheckboxPair("Slow time", "If true, allows The Vessel to slow time on a keypress", slowTime),
            //new LabeledIntSliderPair("Time slow effect magnitude", "How strong the time slow effect is", mushroomEffect, 100),
            new LabeledCheckboxPair("Recall spear", "If true, allows The Vessel to recall her last thrown spear on a keypress", recallSpear),
            new LabeledCheckboxPair("Poison maul", "If true, allows The Vessel to poison a creature by mauling it", canPoisonMaul),
            new LabeledCheckboxPair("No explosive or electric damage", "If true, prevents the The Vessel from taking damage from explosions or electrical hazards(zap coils, centipedes, etc)", noExplosiveOrElectricDamage),
            new LabeledCheckboxPair("Electricute lizards on grab", "If true, lizards are electricuted when they grab The Vessel", electricuteLizardsOnGrab),
            new LabeledCheckboxPair("Dash", "If true, The Vessel is able to dash", canDash),
        };

        Tabs[0].AddItems(
            labelTitle
        );

        int yOffset = 0;
        for (int i = 0; i < labelCheckboxPairs.Length; i++)
        {
            var res = labelCheckboxPairs[i].Generate(new(30, top - (i * 42) - yOffset));
            yOffset += res.Item2;
            Tabs[0].AddItems(res.Item1);
        }
    }
}
