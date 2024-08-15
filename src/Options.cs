using Menu.Remix.MixedUI;
using UnityEngine;
using Menu;

namespace TheVessel;

sealed class Options : OptionInterface
{
    //taken from https://github.com/Dual-Iron/no-damage-rng/blob/master/src/Plugin.cs
    //thanks dual, you're a life saver

    public static Configurable<bool> slowTime;
    public static Configurable<bool> recallSpear;

    public Options()
    {
        slowTime = config.Bind("nc_SlowTime", true);
        recallSpear = config.Bind("nc_RecallSpear", true);
    }

    public override void Initialize()
    {
        base.Initialize();

        Tabs = new OpTab[] { new(this) };

        var labelTitle = new OpLabel(20, 600 - 30, "The Vessel Options", true);

        var top = 200;
        var labelSlowTime = new OpLabel(new(100, 590 - top), Vector2.zero, "Allow the slowing of time", FLabelAlignment.Left);
        var checkSlowTime = new OpCheckBox(slowTime, new Vector2(20, (590 - top) - 6));

        var labelRecallSpear = new OpLabel(new(100, 580 - top), Vector2.zero, "Allow for the recalling of the last spear you threw", FLabelAlignment.Left);
        var checkRecallSpear = new OpCheckBox(recallSpear, new Vector2(20, (580 - top) - 6));

        Tabs[0].AddItems(
            labelTitle,

            labelSlowTime,
            checkSlowTime,
            labelRecallSpear,
            checkRecallSpear
        );
    }
}
