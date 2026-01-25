using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PaApi;


public static class SettingsHelper
{
    public class SettingsBuilder(Transform settingsPanel)
    {
        public void InstantiateLabel(string label)
        {
            var text = Object.Instantiate(_labelPrefab, settingsPanel).GetComponentInChildren<TextMeshProUGUI>();

            var ui_text = text.transform.parent.GetComponent<UI_Text>();
            _modPage.SubElements.Add(ui_text);
            
            text.text = label;
            UIStateManager.Inst.RefreshTextCache(text, label);
            ui_text.SetLocalization(text, _modPage._ID, label, label);
        }

        public void InstantiateSpacer()
        {
            Object.Instantiate(_spacerPrefab, settingsPanel);
        }

        public void InstantiateToggle(string label, ConfigEntry<bool> config, Action<bool> callback = null)
        {
            var toggle = Object.Instantiate(_togglePrefab, settingsPanel).GetComponent<UI_Toggle>();
            toggle.Value = config.Value;
            toggle.DataID = null;
            toggle.ToggleLabel.text = label;
            toggle.OnValueChanged.AddListener(x =>
            {
                config.Value = x;
                callback?.Invoke(x);
            });

            toggle.ToggleLabel.text = label;
            UIStateManager.inst.RefreshTextCache(toggle.ToggleLabel, label);
            _modPage.SubElements.Add(toggle);
            toggle.SetLocalization(toggle.ToggleLabel, _modPage._ID, label, label);
            callback?.Invoke(config.Value);
        }

        public void InstantiateSlider(string label, ConfigEntry<int> config, UI_Slider.VisualType visualType, params string[] values)
        {
            InstantiateSlider(label, config.Value, 1, f => { config.Value = (int)f; }, visualType, values);
        }

        public void InstantiateSlider(string label, float changeAmount, ConfigEntry<float> config, UI_Slider.VisualType visualType, params string[] values)
        {
            InstantiateSlider(label, config.Value, changeAmount, f => { config.Value = f; }, visualType, values);
        }
        
        
        public void InstantiateSlider(string label, ConfigEntry<int> config, UI_Slider.VisualType visualType, Vector2 range)
        {
            InstantiateSlider(label, config.Value, 1, f => { config.Value = (int)f; }, visualType, range);
        }

        public void InstantiateSlider(string label, float changeAmount, ConfigEntry<float> config, UI_Slider.VisualType visualType, Vector2 range)
        {
            InstantiateSlider(label, config.Value, changeAmount, f => { config.Value = f; }, visualType, range);
        }
        
        public void InstantiateSlider(string label, float value, float changeAmount, UnityAction<float> callback, UI_Slider.VisualType visualType, params string[] values)
        {
            UI_Slider slider = Object.Instantiate(_sliderPrefab, settingsPanel).GetComponent<UI_Slider>();
            slider.Values = values;
            slider.Range = new Vector2(0, values.Length - 1);
            slider.VisualRange = slider.Range;
            Internal_InstantiateSlider(slider, label, value, changeAmount, callback, visualType);
        }

        public void InstantiateSlider(string label, float value, float changeAmount, UnityAction<float> callback, UI_Slider.VisualType visualType, Vector2 range)
        {
            UI_Slider slider = Object.Instantiate(_sliderPrefab, settingsPanel).GetComponent<UI_Slider>();
            slider.Range = range;
            slider.VisualRange = range;
            Internal_InstantiateSlider(slider, label, value, changeAmount, callback, visualType);
        }
        
        private void Internal_InstantiateSlider(UI_Slider slider, string label, float value, float changeAmount, UnityAction<float> callback, UI_Slider.VisualType visualType)
        {
            slider.DataID = "unused";
            slider.DataIDType = UI_Slider.DataType.Runtime;

            slider.Value = value;
            slider.Label.text = label;
            slider.ChangeAmount = changeAmount;
            slider.Type = visualType;
            
            slider.OnValueChanged.AddListener(callback);
            
            slider.originalNonLocalizedText.Clear();

            UIStateManager.inst.RefreshTextCache(slider.Label, label);
            _modPage.SubElements.Add(slider);
            slider.SetLocalization(slider.Label, _modPage._ID, label, label);
            
            callback?.Invoke(value);
        }
    }
    internal struct SettingsDefinition(string name, ConfigFile file, Action<SettingsBuilder> settingsBuilder)
    {
        public string Name = name;
        public ConfigFile File = file;
        public Action<SettingsBuilder> SettingsBuilder = settingsBuilder;
    }
    
    private static UI_Book.Page _modPage;
    
    private static GameObject _sliderPrefab;
    private static GameObject _togglePrefab;
    private static GameObject _labelPrefab;
    private static GameObject _spacerPrefab;

    internal static readonly Dictionary<string, SettingsDefinition> ModSettingsDefinitions = new();
    
    public static void RegisterModSettings(string modGuid, string SettingsName, ConfigFile file, Action<SettingsBuilder> SettingsBuilder)
    {
        if (ModSettingsDefinitions.ContainsKey(modGuid))
        {
            Plugin.Logger.LogFatal($"Tried to register mod with GUID {modGuid} more than once! This is not necessary.");
            return;
        }
        
        ModSettingsDefinitions.Add(modGuid, new SettingsDefinition(SettingsName, file, SettingsBuilder));
    }

    /// <summary>
    /// instantiates the new settings tab, but be called everytime the MENU scene is loaded.
    /// </summary>
    internal static void SetupMenu()
    {
        Transform settingTabPrefab =
            GameObject.Find("Canvas/Window/Content/Settings/Blank").transform.GetChild(0).GetChild(0);

        if (!settingTabPrefab)
        {
            Plugin.Logger.LogError("No settings tab found, SetupMenu was called at a incorrect time or scene changed");
            return;
        }

        UI_Book book = settingTabPrefab.parent.parent.parent.GetComponent<UI_Book>();
        //get 'prefabs'
        Transform prefabsParent = book.transform.Find("Audio/Right");
        _sliderPrefab = prefabsParent.Find("Music").gameObject;
        _togglePrefab = prefabsParent.Find("Checkpoint SFX").gameObject;
        _labelPrefab = prefabsParent.Find("General Title").gameObject;
        _spacerPrefab = prefabsParent.Find("spacer").gameObject;

        GameObject modSettingsTab = Object.Instantiate(settingTabPrefab.GetChild(1).gameObject, settingTabPrefab);
        modSettingsTab.name = "Mod Settings";
        
        var modButton = modSettingsTab.GetComponent<MultiElementButton>();
        modButton.onClick = new();

        var modUiButton = modSettingsTab.GetComponent<UI_Button>();
        
        modButton.onClick.AddListener(() =>
        {
            book.ForceSwapPage("PaApi.ModSettings");
            modUiButton.OnClick();
        });
        modUiButton.Text.text = "Mod Settings";
        UIStateManager.Inst.RefreshTextCache(modUiButton.Text, "Mod Settings");
        modUiButton.SetLocalization(modUiButton.Text, Plugin.Guid, "mod settings", "Mod Settings");
    
 
        //add our button to the Settings page in the UI_Book in the Canvas
        book.transform.parent.parent.parent.GetComponent<UI_Book>().Pages[6].SubElements.Add(modUiButton);

        var modSetingsPanel = Object.Instantiate(book.transform.Find("Blank"), book.transform);
        var modSettingsButtonList = modSetingsPanel.GetChild(0).GetChild(0);
        modSetingsPanel.gameObject.name = "Mod Settings";

        for (int i = 1; i < modSettingsButtonList.childCount; i++)
        {
            Object.Destroy(modSettingsButtonList.GetChild(i).gameObject);
        }
      
        var modSettingsBackButton = modSettingsButtonList.GetChild(0).GetComponent<MultiElementButton>();
        modSettingsBackButton.onClick = new();
        modSettingsBackButton.onClick.AddListener(() =>
        {
            book.ForceSwapPage("Blank");
        });
        
        UI_Book.Page modsPage = new()
        {
            _ID = "PaApi.ModSettings",
            PageContainer = modSetingsPanel.gameObject,
            SubElements = [modSettingsButtonList.GetChild(0).GetComponent<UI_Button>()]
        };
        book.Pages.Add(modsPage);
        
        foreach (var setting in ModSettingsDefinitions)
        {
            var settingsPanel = Object.Instantiate(book.transform.Find("Audio"), book.transform).Find("Right");
            settingsPanel.parent.name = $"{setting.Key} settings";
            
            for (int i = 0; i < settingsPanel.childCount; i++)
            {
                Object.Destroy(settingsPanel.GetChild(i).gameObject);
            }

            var backButton = settingsPanel.parent.GetChild(0).GetChild(0).GetComponent<MultiElementButton>();

            backButton.onClick = new();
            backButton.onClick.AddListener(() =>
            {
                book.ForceSwapPage("PaApi.ModSettings");
            });

            var titleCard = settingsPanel.parent.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
            titleCard.text = VGFunctions.LSText.ASCII.Get2HighASCII($"{setting.Value.Name} Settings", 80);
            //UIStateManager.Inst.RefreshTextCache(titleCard, VGFunctions.LSText.ASCII.Get2HighASCII("Multiplayer Settings", 80));

            _modPage = new()
            {
                _ID = setting.Key,
                PageContainer = settingsPanel.parent.gameObject,
                SubElements = [settingsPanel.parent.GetChild(0).GetChild(0).GetComponent<UI_Button>()]
            };
            book.Pages.Add(_modPage);

            //the button
            GameObject settingsTab = Object.Instantiate(settingTabPrefab.GetChild(1).gameObject, modSettingsButtonList);

            var button = settingsTab.GetComponent<MultiElementButton>();
            button.onClick = new();

            var uiButton = settingsTab.GetComponent<UI_Button>();
            button.onClick.AddListener(() =>
            {
                book.ForceSwapPage(setting.Key);
                uiButton.OnClick();
            });
            uiButton.Text.text = setting.Value.Name;
            UIStateManager.Inst.RefreshTextCache(uiButton.Text, setting.Value.Name);
            uiButton.SetLocalization(uiButton.Text, setting.Key, "PaApi Setting Tab Button", setting.Value.Name);
            //add our button to the Settings page in the UI_Book in the Canvas
            modsPage.SubElements.Add(uiButton);

            setting.Value.SettingsBuilder?.Invoke(new SettingsBuilder(settingsPanel));
        }
    }

    internal static void Save()
    {
        foreach (var setting in ModSettingsDefinitions)
        {
            ConfigFile file = setting.Value.File;
            if (file != null && !file.SaveOnConfigSet)
            {
                file.Save();
            }
        }
    }
}

public static class UIElementExtension
{
    /// <summary>
    /// Sets the localization for this TMPUgui for this UiElement
    /// </summary>
    /// <param name="text">Must be part of a GameObjectLocalizer and the first member of element.Localizers</param>
    public static void SetLocalization(this UIElement element, TextMeshProUGUI text, string table, string entry, string value)
    {
        element.InitGraphics();
        
        if (element.localizers.TryGetValue(text, out var outText) && outText.TrackedObjects[0].TrackedProperties[0] is LocalizedStringProperty str)
        {
            str.LocalizedString = new LocalizedString(table, entry);
            LocalizationSettings.StringDatabase.GetTable(table).AddEntry(entry, value);
        }
        else
        {
            Plugin.Logger.LogWarning($"Tried to set invalid locale for object [{element.name}], entry [{entry}]");
        }
    }
}