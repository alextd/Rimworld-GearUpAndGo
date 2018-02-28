using System.Reflection;
using Verse;
using Harmony;

namespace GearUpAndGo
{
    public class GearUpAndGo
    {
        public class Mod : Verse.Mod
        {
            public Mod(ModContentPack content) : base(content)
            {
                //// initialize settings
                //GetSettings<Settings>();
#if DEBUG
                HarmonyInstance.DEBUG = true;
#endif
                HarmonyInstance harmony = HarmonyInstance.Create("uuugggg.rimworld.GearUpAndGo.main");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }

            //public override void DoSettingsWindowContents(Rect inRect)
            //{
            //    base.DoSettingsWindowContents(inRect);
            //    GetSettings<Settings>().DoWindowContents(inRect);
            //}

            //public override string SettingsCategory()
            //{
            //    return "Gear Up And Go";
            //}
        }
    }
}