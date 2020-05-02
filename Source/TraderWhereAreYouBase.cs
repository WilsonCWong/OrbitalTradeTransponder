using HugsLib;
using HugsLib.Settings;

namespace TraderWhereAreYou
{
    public class TraderWhereAreYouBase : ModBase
    {
        
        public override string ModIdentifier => "OrbitalTraderTransponder";

        private SettingHandle<int> tradeCost;

        public override void DefsLoaded()
        {
            tradeCost = Settings.GetHandle<int>(
                "cost",
                "Trade Cost",
                "Set how much silver it costs to call a trader.",
                300);

            ModShared.Settings = Settings;
        }
    }
}
