using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
            _mpPage.SubElements.Add(ui_text);
            
            ui_text.localizers.Clear();
            ui_text.localizersStr.Clear();
            
            text.text = label;
            UIStateManager.Inst.RefreshTextCache(text, label);
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
            
            toggle.localizers.Clear();
            toggle.localizersStr.Clear();
            UIStateManager.inst.RefreshTextCache(toggle.ToggleLabel, label);
            _mpPage.SubElements.Add(toggle);
            
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
            
            slider.localizers.Clear();
            slider.localizersStr.Clear();
            slider.originalNonLocalizedText.Clear();

            UIStateManager.inst.RefreshTextCache(slider.Label, label);
            _mpPage.SubElements.Add(slider);
            
            callback?.Invoke(value);
        }
    }
    private struct SettingsDefinition(string name, ConfigFile file, Action<SettingsBuilder> settingsBuilder)
    {
        public string Name = name;
        public ConfigFile File = file;
        public Action<SettingsBuilder> SettingsBuilder = settingsBuilder;
    }
    
    private static UI_Book.Page _mpPage;
    
    private static GameObject _sliderPrefab;
    private static GameObject _togglePrefab;
    private static GameObject _labelPrefab;
    private static GameObject _spacerPrefab;

    private static readonly Dictionary<string, SettingsDefinition> _settings = new();
    
    public static void RegisterModSettings(string modGuid, string SettingsName, ConfigFile file, Action<SettingsBuilder> SettingsBuilder)
    {
        if (_settings.ContainsKey(modGuid))
        {
            Plugin.Logger.LogFatal($"Tried to register mod with GUID {modGuid} more than once! This is not necessary.");
            return;
        }
        
        _settings.Add(modGuid, new SettingsDefinition(SettingsName, file, SettingsBuilder));
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

        foreach (var setting in _settings)
        {
            var settingsPanel = Object.Instantiate(book.transform.Find("Audio"), book.transform).Find("Right");
            for (int i = 0; i < settingsPanel.childCount; i++)
            {
                Object.Destroy(settingsPanel.GetChild(i).gameObject);
            }

            var titleCard = settingsPanel.parent.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
            titleCard.text = VGFunctions.LSText.ASCII.Get2HighASCII($"{setting.Value.Name} Settings", 80);
            //UIStateManager.Inst.RefreshTextCache(titleCard, VGFunctions.LSText.ASCII.Get2HighASCII("Multiplayer Settings", 80));

            _mpPage = new()
            {
                _ID = setting.Key,
                PageContainer = settingsPanel.parent.gameObject,
                SubElements = [settingsPanel.parent.GetChild(0).GetChild(0).GetComponent<UI_Button>()]
            };
            book.Pages.Add(_mpPage);

            //the button
            GameObject settingsTab = Object.Instantiate(settingTabPrefab.GetChild(1).gameObject, settingTabPrefab);

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

            uiButton.localizers.Clear();
            uiButton.localizersStr.Clear();

            //add our button to the Settings page in the UI_Book in the Canvas
            book.transform.parent.parent.parent.GetComponent<UI_Book>().Pages[6].SubElements.Add(uiButton);

            setting.Value.SettingsBuilder?.Invoke(new SettingsBuilder(settingsPanel));
        }
    }

    internal static void Save()
    {
        foreach (var setting in _settings)
        {
            ConfigFile file = setting.Value.File;
            if (file != null && !file.SaveOnConfigSet)
            {
                file.Save();
            }
        }
    }
}