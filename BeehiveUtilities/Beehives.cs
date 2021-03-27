using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;

namespace BeehiveUtilities
{
    [BepInPlugin("smallo.mods.beehiveutilities", "Beehive Utilities", "1.0.1")]
    [HarmonyPatch]
    class FireplaceUtilitiesPlugin : BaseUnityPlugin
    {
        private static ConfigEntry<bool> enableMod;
        private static ConfigEntry<bool> proxCheck;
        private static ConfigEntry<bool> showAlternateText;
        private static ConfigEntry<int> maxHoney;
        private static ConfigEntry<int> minsCreation;

        private static int creationTime = 0;

        void Awake()
        {
            enableMod = Config.Bind("1 - Global", "Enable Mod", true, "Enable or disable this mod");
            if (!enableMod.Value) return;

            proxCheck = Config.Bind("2 - General", "Disable Proximity Check", true, "Disables the \"Bees need more space\" check");
            showAlternateText = Config.Bind("2 - General", "Show Alternate Text", true, "Show the amount of honey generated next to the Beehive name \"Honey ( 0/4 )\" instead of \"( EMPTY )\" or \"( Honey x honeyAmount )\"");
            maxHoney = Config.Bind("2 - General", "Max Honey", 4, "The maximum amount of honey a beehive can generate (default is 4)");
            minsCreation = Config.Bind("2 - General", "Minutes Per Creation", 20, "The amount of minutes it takes to generate 1 piece of honey (default is 20)");

            creationTime = minsCreation.Value * 60;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Beehive), "Awake")]
        public static void BeehiveAwake_Patch(Beehive __instance)
        {
            if (proxCheck.Value) __instance.m_maxCover = 1000f;
            if (maxHoney.Value != __instance.m_maxHoney) __instance.m_maxHoney = maxHoney.Value;
            if (creationTime != __instance.m_secPerUnit) __instance.m_secPerUnit = creationTime;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Beehive), "GetHoverText")]
        public static string BeehiveGetHoverText_Patch(string __result, Beehive __instance)
        {
            if (!showAlternateText.Value) return __result;

            string honeyString = Localization.instance.Localize(__instance.m_honeyItem.m_itemData.m_shared.m_name);
            string EMPTY = $"( {Localization.instance.Localize("$piece_container_empty")} )";
            string honeyCount = $"\n{honeyString} ( {__instance.GetHoneyLevel()} / {__instance.m_maxHoney} )";
            string hasHoney = $"( {honeyString} x {__instance.GetHoneyLevel()} )";

            if (__result.Contains(EMPTY)) return __result.Replace(EMPTY, honeyCount);
            if (__result.Contains(hasHoney)) return __result.Replace(hasHoney, honeyCount);

            return __result;
        }
    }
}