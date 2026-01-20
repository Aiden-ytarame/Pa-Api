using HarmonyLib;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using VGFunctions;

namespace PaApi;

[HarmonyPatch(typeof(ShowChangeLog))]
internal static class UiPatch
{
    [HarmonyPatch(nameof(ShowChangeLog.Start))]
    [HarmonyPrefix]
    private static void PreStart()
    {
        SettingsHelper.SetupMenu();
    }
}

[HarmonyPatch(typeof(SettingsManager))]
internal static class SettingsPatch
{
    [HarmonyPatch(nameof(SettingsManager.UpdateSettingsFile))]
    [HarmonyPostfix]
    private static void PostSave()
    {
        SettingsHelper.Save();
    }
}

[HarmonyPatch(typeof(UI_Slider))]
internal static class SliderPatch
{
    [HarmonyPatch(nameof(UI_Slider.GetWidthOfBar))]
    [HarmonyPrefix]
    private static bool PreGetWidth(UI_Slider __instance, ref int __result)
    {
        if (__instance.Values.Length != 0)
        {
            __result = __instance.Values.Length;
            if (__instance.Type == UI_Slider.VisualType.line)
            {
                --__result;
            }
            return false;
        }
        
        float f = __instance.VisualRange.y - __instance.VisualRange.x;
        __result = f > 1.0 ? Mathf.RoundToInt(f) : Mathf.RoundToInt(f * 10f);
        return false;
    }

    [HarmonyPatch(nameof(UI_Slider.UpdateTextValue))]
    [HarmonyPrefix]
    private static bool PreUpdateValueText(UI_Slider __instance)
    {
        __instance.ValueText.gameObject.SetActive(__instance.ShowValue);
        if (!__instance.ShowValue)
        {
            return false;
        }

        if (__instance.Values.Length != 0)
        {
            int max = __instance.GetWidthOfBar();
            if (__instance.Type == UI_Slider.VisualType.dot)
            {
                max--;
            }
            
            int index = Mathf.CeilToInt(Mathf.Clamp(LSMath.SuperLerp(__instance.Range.x, __instance.Range.y, 0.0f, max, __instance.Value), 0.0f, __instance.Values.Length - 1));
            if (index < 0)
            {
                index = 0;
            }

            if (index >= __instance.Values.Length)
            {
                index = __instance.Values.Length - 1;
            }

            string value = __instance.Values[index];
            if (value.Contains("$"))
            {
                try
                {
                    value = value.Replace("$", "");
                    value = LocalizationSettings.StringDatabase.GetLocalizedString((TableReference) "General UI", (TableEntryReference) value, LocalizationSettings.SelectedLocale);
                }
                catch
                {
                    value = value.Replace("$", "");
                }
            }
            
            SingletonBase<UIStateManager>.Inst.RefreshTextCache(__instance.ValueText, value);
        }
        else
        {
            SingletonBase<UIStateManager>.Inst.RefreshTextCache(__instance.ValueText, __instance.Value.ToString("f1"));
        }

        return false;
    }
}