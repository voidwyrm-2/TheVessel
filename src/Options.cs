using Menu.Remix.MixedUI;
using UnityEngine;
using static Nuktils.Options;

namespace TheVessel;

sealed class Options : OptionInterface
{
    //taken from https://github.com/Dual-Iron/no-damage-rng/blob/master/src/Options.cs

    public static Configurable<bool> slowTime;
    public static Configurable<int> mushroomEffect;
    public static Configurable<bool> recallSpear;
    public static Configurable<bool> canPoisonMaul;

    public Options()
    {
        slowTime = config.Bind("nc_slowTime", true);
        mushroomEffect = config.Bind("nc_mushroomEffect", 10, new ConfigAcceptableRange<int>(1, 100));
        recallSpear = config.Bind("nc_recallSpear", true);
        canPoisonMaul = config.Bind("nc_canPoisonMaul", true);
    }

    public override void Initialize()
    {
        base.Initialize();

        Tabs = new OpTab[] { new(this) };

        var labelTitle = new OpLabel(20, 600 - 30, "The Vessel Options", true);

        var top = 200;
        ILabeledPair[] labelCheckboxPairs =
        {
            new LabeledCheckboxPair("Slow time", "If true, allows The Vessel to slow time on a keypress", slowTime),
            new LabeledIntSliderPair("Time slow effect magnitude", "How strong the time slow effect is", mushroomEffect, 100),
            new LabeledCheckboxPair("Recall spear", "If true, allows The Vessel to recall her last thrown spear on a keypress", recallSpear),
            new LabeledCheckboxPair("Poison maul", "If true, allows The Vessel to poison a creature by mauling it", canPoisonMaul)
        };

        Tabs[0].AddItems(
            labelTitle
        );

        int yOffset = 0;
        for (int i = 0; i < labelCheckboxPairs.Length; i++)
        {
            var res = labelCheckboxPairs[i].Generate(new(20, top + (i * 30) - yOffset));
            yOffset += res.two;
            Tabs[0].AddItems(res.one);
        }
    }
}
