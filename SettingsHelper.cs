using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PaApi;


public static class SettingsHelper
{
    public class SettingsBuilder(string guid, Transform settingsPanel, Color color, int residingPage)
    {
        private Transform _settingsPanel = settingsPanel;

        private int _itemCount = 0;
        private int _pageCount = 0;

        public void EndPage()
        {
            if (_itemCount == 0)
            {
                return;
            }

            if (_pageCount == 0)
            {
                RectTransform layout = AddPageSwapButtons(_settingsPanel.parent.GetChild(0), _settingsBook, _modPage,
                    null,
                    $"{guid} page 1");

                layout.pivot = new Vector2(0, 0.5f);
                layout.anchorMin = Vector2.zero;
                layout.anchorMax = Vector2.up;
                layout.anchoredPosition = new Vector2(0, -135);
                layout.sizeDelta = new Vector2(372, 80);
            }
            else
            {
                SetupSwapPageButtons($"{guid} page {_pageCount - 1}", $"{guid} page {_pageCount + 1}");
            }

            _itemCount = 0;
            CreateNewModSettingsPage(_settingsPanel.parent, ++_pageCount);
        }

        private void CheckPageEnd()
        {
            if (_itemCount >= 10)
            {
                EndPage();
            }

            _itemCount++;
        }

        internal void Finished()
        {
            if (_pageCount == 0)
            {
                return;
            }
            
            SetupSwapPageButtons($"{guid} page {_pageCount - 1}", null);
        }

        public void Label(string label)
        {
            CheckPageEnd();

            var text = Object.Instantiate(_labelPrefab, _settingsPanel).GetComponentInChildren<TextMeshProUGUI>();

            var ui_text = text.transform.parent.GetComponent<UI_Text>();
            _modPage.SubElements.Add(ui_text);

            text.text = label;
            UIStateManager.Inst.RefreshTextCache(text, label);
            ui_text.SetLocalization(text, guid, label, label);
        }

        public void Spacer()
        {
            CheckPageEnd();

            Object.Instantiate(_spacerPrefab, _settingsPanel);
        }
        
        public void Toggle(string label, ConfigEntry<bool> config, Action<bool> callback = null) => Toggle(label, null, null, config, callback);
     
        public void Toggle(string label, string onDescription, string offDescription, ConfigEntry<bool> config, Action<bool> callback = null)
        {
            CheckPageEnd();

            var toggle = Object.Instantiate(_togglePrefab, _settingsPanel).GetComponent<UI_Toggle>();
            toggle.Value = config.Value;
            toggle.DataID = null;
            toggle.OnValueChanged.AddListener(x =>
            {
                config.Value = x;
                callback?.Invoke(x);
            });

            TextMeshProUGUI text = null;
            foreach (Graphic graphics in toggle.multiGraphics.subGraphics)
            {
                TextMeshProUGUI component = graphics?.GetComponent<TextMeshProUGUI>();
                if (component != null)
                {
                    text = component;
                    break;
                }
            }

            if (text == null)
            {
                Plugin.Logger.LogFatal("Could not find text for Toggle");
                return;
            }
         
            text.text = label;
            UIStateManager.inst.RefreshTextCache(text, label);

            if (!string.IsNullOrEmpty(onDescription) && !string.IsNullOrEmpty(offDescription))
            {
                toggle.OnValueChanged.AddListener(x =>
                {
                    toggle.RefreshToggleDescription();
                    if (toggle.Description)
                    {
                        SingletonBase<UIStateManager>.Inst.RefreshTextCache(toggle.Description, x ? onDescription : offDescription);
                    }
                });
            }
            else
            {
                if (toggle.Description)
                {
                    toggle.Description.enabled = false;
                }
            }
            
            if (toggle.Description)
            {
                string desc =  toggle.Value ? onDescription : offDescription;
                if (string.IsNullOrEmpty(desc))
                {
                    desc = "Missing";
                }
                toggle.Description.text = desc;
                UIStateManager.inst.RefreshTextCache(toggle.Description, desc);
                
                //if (string.IsNullOrEmpty(description))
                {
                    //toggle.SetLocalization(toggle.Description, guid, "null", "null");
                }
               // else
                {
                    //toggle.SetLocalization(toggle.Description, guid, description, description);
                }
            }
            
            _modPage.SubElements.Add(toggle);
            toggle.SetLocalization(text, guid, label, label);

            toggle.OverrideNormalColor = color;
            callback?.Invoke(config.Value);
        }

        public void Slider(string label, ConfigEntry<int> config, UI_Slider.VisualType visualType,
            params string[] values)
        {
            Slider(label, config.Value, 1, f => { config.Value = (int)f; }, visualType, values);
        }

        public void Slider(string label, float changeAmount, ConfigEntry<float> config, UI_Slider.VisualType visualType,
            params string[] values)
        {
            Slider(label, config.Value, changeAmount, f => { config.Value = f; }, visualType, values);
        }


        public void Slider(string label, ConfigEntry<int> config, UI_Slider.VisualType visualType, Vector2 range)
        {
            Slider(label, config.Value, 1, f => { config.Value = (int)f; }, visualType, range);
        }

        public void Slider(string label, float changeAmount, ConfigEntry<float> config, UI_Slider.VisualType visualType,
            Vector2 range)
        {
            Slider(label, config.Value, changeAmount, f => { config.Value = f; }, visualType, range);
        }

        public void Slider(string label, float value, float changeAmount, UnityAction<float> callback,
            UI_Slider.VisualType visualType, params string[] values)
        {
            CheckPageEnd();

            UI_Slider slider = Object.Instantiate(_sliderPrefab, _settingsPanel).GetComponent<UI_Slider>();
            slider.Values = values;
            slider.Range = new Vector2(0, values.Length - 1);
            slider.VisualRange = slider.Range;
            Internal_InstantiateSlider(slider, label, value, changeAmount, callback, visualType);
        }

        public void Slider(string label, float value, float changeAmount, UnityAction<float> callback,
            UI_Slider.VisualType visualType, Vector2 range)
        {
            CheckPageEnd();

            UI_Slider slider = Object.Instantiate(_sliderPrefab, _settingsPanel).GetComponent<UI_Slider>();
            slider.Range = range;
            slider.VisualRange = range;
            Internal_InstantiateSlider(slider, label, value, changeAmount, callback, visualType);
        }

        private void Internal_InstantiateSlider(UI_Slider slider, string label, float value, float changeAmount,
            UnityAction<float> callback, UI_Slider.VisualType visualType)
        {
            slider.DataID = null;
            slider.DataIDType = UI_Slider.DataType.Runtime;

            slider.Value = value;
            slider.Label.text = label;
            slider.ChangeAmount = changeAmount;
            slider.Type = visualType;

            slider.OnValueChanged.AddListener(callback);

            slider.originalNonLocalizedText.Clear();

            UIStateManager.inst.RefreshTextCache(slider.Label, label);
            _modPage.SubElements.Add(slider);
            slider.SetLocalization(slider.Label, guid, label, label);

            slider.CenterText.color = color;
            callback?.Invoke(value);
        }

        private void CreateNewModSettingsPage(Transform panel, int page)
        {
            _settingsPanel = Object.Instantiate(panel, _settingsBook.transform).Find("Right");
            _settingsPanel.parent.name = $"{guid}  page {page}";

            for (int i = 0; i < _settingsPanel.childCount; i++)
            {
                Object.Destroy(_settingsPanel.GetChild(i).gameObject);
            }

            _modPage = new()
            {
                _ID = $"{guid} page {page}",
                PageContainer = _settingsPanel.parent.gameObject,
                SubElements = [_settingsPanel.parent.GetChild(0).GetChild(0).GetComponent<UI_Button>()],
                BottomTitleLocalized = new LocalizedString(),
                TitleLocalized = new LocalizedString(Plugin.Guid, "ModSettings")
            };
            _settingsBook.Pages.Add(_modPage);

            var titleCard = _settingsPanel.parent.GetChild(0).GetChild(1).GetComponent<UI_Text>();
            _modPage.SubElements.Add(titleCard);

            var filler = _settingsPanel.parent.GetChild(2).GetComponent<UI_Text>();
            _modPage.SubElements.Add(filler);
            
            var backButton = _settingsPanel.parent.GetChild(0).GetChild(0).GetComponent<MultiElementButton>();
            backButton.onClick.AddListener(() =>
            {
                _settingsBook.ForceSwapPage($"PaApi.ModSettings.{residingPage}");
            });
        }

        private void SetupSwapPageButtons(string leftPage, string rightPage)
        {
            var layout = _settingsPanel.parent.GetChild(0).GetChild(2);
            var leftButton = layout.GetChild(0).GetComponent<MultiElementButton>();
            var rightButton = layout.GetChild(1).GetComponent<MultiElementButton>();

            var leftUi = leftButton.GetComponent<UI_Button>();
            leftButton.uiElement = leftUi;

            var rightUi = rightButton.GetComponent<UI_Button>();
            rightButton.uiElement = rightUi;

            _modPage.SubElements.Add(leftUi);
            _modPage.SubElements.Add(rightUi);

            if (string.IsNullOrEmpty(leftPage))
            {
                leftButton.LockButtonState(true);
            }
            else
            {
                leftButton.onClick = new();
                leftButton.onClick.AddListener(() => { _settingsBook.ForceSwapPage(leftPage); });
            }

            if (string.IsNullOrEmpty(rightPage))
            {
                rightButton.LockButtonState(true);
            }
            else
            {
                rightButton.onClick = new();
                rightButton.onClick.AddListener(() => { _settingsBook.ForceSwapPage(rightPage); });
            }
        }
    }

    internal struct SettingsDefinition(string name, Color color, ConfigFile file, Action<SettingsBuilder> settingsBuilder)
    {
        public string Name = name;
        public Color Color = color;
        public ConfigFile File = file;
        public Action<SettingsBuilder> SettingsBuilder = settingsBuilder;
    }
    
    private static UI_Book.Page _modSettingsPage;
    private static UI_Book.Page _modPage;
    
    private static UI_Book _settingsBook;
    private static Transform _settingTabButtonPrefab;
    
    private static GameObject _sliderPrefab;
    private static GameObject _togglePrefab;
    private static GameObject _labelPrefab;
    private static GameObject _spacerPrefab;

    internal static readonly Dictionary<string, SettingsDefinition> ModSettingsDefinitions = new();
    private static readonly Color DefaultColor = new Color(0, 0.6824f, 0.9373f, 1);
    private static Transform _blankPage;


    public static void RegisterModSettings(string modGuid, string SettingsName, Color? color, ConfigFile file, Action<SettingsBuilder> SettingsBuilder)
    {
        if (ModSettingsDefinitions.ContainsKey(modGuid))
        {
            Plugin.Logger.LogFatal($"Tried to register mod with GUID {modGuid} more than once! This is not necessary.");
            return;
        }
        
        ModSettingsDefinitions.Add(modGuid, new SettingsDefinition(SettingsName, color ?? DefaultColor, file, SettingsBuilder));
    }

    /// <summary>
    /// instantiates the new settings tab, but be called everytime the MENU scene is loaded.
    /// </summary>
    internal static void SetupMenu()
    {
        _settingTabButtonPrefab = GameObject.Find("Canvas/Window/Content/Settings/Blank").transform.GetChild(0).GetChild(0);

        if (!_settingTabButtonPrefab)
        {
            Plugin.Logger.LogError("No settings tab found, SetupMenu was called at a incorrect time or scene changed");
            return;
        }

        _settingsBook = _settingTabButtonPrefab.parent.parent.parent.GetComponent<UI_Book>();
        
        //get 'prefabs'
        
        Transform prefabsParent = _settingsBook.transform.Find("Accessibility/Right");
        _sliderPrefab = _settingsBook.transform.Find("Audio/Right/Music").gameObject;
        _togglePrefab = prefabsParent.Find("High Contrast").gameObject;
        _labelPrefab = prefabsParent.Find("General Title").gameObject;
        _spacerPrefab = prefabsParent.Find("spacer").gameObject;

        GameObject modSettingsTab = Object.Instantiate(_settingTabButtonPrefab.GetChild(1).gameObject, _settingTabButtonPrefab);
        modSettingsTab.name = "Mod Settings";
        
        var modButton = modSettingsTab.GetComponent<MultiElementButton>();
        modButton.onClick = new();

        var modUiButton = modSettingsTab.GetComponent<UI_Button>();
        
        modButton.onClick.AddListener(() =>
        {
            _settingsBook.ForceSwapPage("PaApi.ModSettings.0");
            modUiButton.OnClick();
        });
        modUiButton.Text.text = "Mod Settings";
        UIStateManager.Inst.RefreshTextCache(modUiButton.Text, "Mod Settings");
        modUiButton.SetLocalization(modUiButton.Text, Plugin.Guid, "ModSettings", "Mod Settings");
    
 
        //add our button to the Settings page in the UI_Book in the Canvas
        _settingsBook.transform.parent.parent.parent.GetComponent<UI_Book>().Pages[6].SubElements.Add(modUiButton);

        _blankPage = _settingsBook.transform.Find("Blank");
        var blankFiller = _blankPage.GetChild(3).GetComponent<UI_Text>();
        blankFiller.Text.text = """
                                ▒
                                ▒▒<space=32px>▒
                                ▒▓▒▒<space=64px>▒▒
                                ▒<space=32px>▒▒<space=32px>▒
                                ▒▒▒<space=32px>▒
                                ▒▒▓▒▒<space=64px>▒
                                ▒▒▒▒▓▒▒
                                ▒▒<space=64px>▒▒▒<space=64px>▒
                                ▓▒▒▓▒▒<space=32px>▒▒▒▒▒
                                """;
        
        
        Transform buttonList = CreateSettingsListPage(0);

        int index = 0;
        int pages = 0;
        foreach (var setting in ModSettingsDefinitions)
        {
            index++;
            SetupModSettings(setting, buttonList, pages);

            if (index % 5 != 0 || index + 1 > ModSettingsDefinitions.Count)
            {
                continue;
            }

            if (pages == 0)
            {
                AddPageSwapButtons(buttonList, _settingsBook, _modSettingsPage, null, $"PaApi.ModSettings.1");
            }
            else if (index + 5 < ModSettingsDefinitions.Count)
            {
                AddPageSwapButtons(buttonList, _settingsBook, _modSettingsPage, $"PaApi.ModSettings.{pages - 1}", $"PaApi.ModSettings.{pages + 1}");
            }
            else
            {
                AddPageSwapButtons(buttonList, _settingsBook, _modSettingsPage, $"PaApi.ModSettings.{pages - 1}", null);
            }
            
            pages++;
            buttonList = CreateSettingsListPage(pages);
        }
    }

    private static void SetupModSettings(KeyValuePair<string, SettingsDefinition> setting, Transform modSettingsButtonList, int page)
    {
        var settingsPanel = Object.Instantiate(_settingsBook.transform.Find("Audio"), _settingsBook.transform).Find("Right");
        settingsPanel.parent.name = $"{setting.Key} page 0";
            
        for (int i = 0; i < settingsPanel.childCount; i++)
        {
            Object.Destroy(settingsPanel.GetChild(i).gameObject);
        }
            
        _modPage = new()
        {
            _ID = $"{setting.Key} page 0",
            PageContainer = settingsPanel.parent.gameObject,
            SubElements = [settingsPanel.parent.GetChild(0).GetChild(0).GetComponent<UI_Button>()],
            BottomTitleLocalized = new LocalizedString(),
            TitleLocalized = new LocalizedString(Plugin.Guid, "ModSettings")
        };
        _settingsBook.Pages.Add(_modPage);
            
        var backButton = settingsPanel.parent.GetChild(0).GetChild(0).GetComponent<MultiElementButton>();

        backButton.onClick = new();
        backButton.onClick.AddListener(() =>
        {
            _settingsBook.ForceSwapPage($"PaApi.ModSettings.{page}");
        });

        string asciiTitle = VGFunctions.LSText.ASCII.Get2HighASCII($"{setting.Value.Name} Settings", 80);
        
        var titleCard = settingsPanel.parent.GetChild(0).GetChild(1).GetComponent<UI_Text>();
        titleCard.Text = titleCard.GetComponent<TextMeshProUGUI>();
        titleCard.Text.text = asciiTitle;
        UIStateManager.Inst.RefreshTextCache(titleCard.Text, asciiTitle);
        titleCard.originalNonLocalizedText[titleCard.Text] = asciiTitle;
        
        _modPage.SubElements.Add(titleCard);

        var filler = settingsPanel.parent.GetChild(2).GetComponent<UI_Text>();
        filler.Text.color = setting.Value.Color;
        _modPage.SubElements.Add(filler);
            
        //the button
        GameObject settingsTab = Object.Instantiate(_settingTabButtonPrefab.GetChild(1).gameObject, modSettingsButtonList);

        var button = settingsTab.GetComponent<MultiElementButton>();
        button.onClick = new();

        var uiButton = settingsTab.GetComponent<UI_Button>();
        button.onClick.AddListener(() =>
        {
            _settingsBook.ForceSwapPage($"{setting.Key} page 0");
            uiButton.OnClick();
        });
        uiButton.Text.text = setting.Value.Name;
        UIStateManager.Inst.RefreshTextCache(uiButton.Text, setting.Value.Name);
        uiButton.SetLocalization(uiButton.Text, setting.Key, "PaApi Setting Tab Button", setting.Value.Name);
        
        //add our button to the Settings page in the UI_Book in the Canvas
        _modSettingsPage.SubElements.Add(uiButton);

        SettingsBuilder builder = new(setting.Key, settingsPanel, setting.Value.Color, page);
        setting.Value.SettingsBuilder?.Invoke(builder);
        builder.Finished();
    }

    private static Transform CreateSettingsListPage(int page)
    {
        var modSetingsPanel = Object.Instantiate(_blankPage, _settingsBook.transform);
        var modSettingsButtonList = modSetingsPanel.GetChild(0).GetChild(0);
        modSetingsPanel.gameObject.name = $"PaApi Mod Settings {page}";

        for (int i = 1; i < modSettingsButtonList.childCount; i++)
        {
            Object.Destroy(modSettingsButtonList.GetChild(i).gameObject);
        }
      
        var modSettingsBackButton = modSettingsButtonList.GetChild(0).GetComponent<MultiElementButton>();
        modSettingsBackButton.onClick = new();
        modSettingsBackButton.onClick.AddListener(() =>
        {
            _settingsBook.ForceSwapPage("Blank");
        });
        
        _modSettingsPage = new()
        {
            _ID = $"PaApi.ModSettings.{page}",
            PageContainer = modSetingsPanel.gameObject,
            SubElements = [modSettingsButtonList.GetChild(0).GetComponent<UI_Button>(), modSetingsPanel.GetChild(3).GetComponent<UI_Text>()],
            BottomTitleLocalized = new LocalizedString(),
            TitleLocalized = new LocalizedString(Plugin.Guid, "ModSettings")
        };
        _settingsBook.Pages.Add(_modSettingsPage);

        return modSettingsButtonList;
    }

    private static RectTransform AddPageSwapButtons(Transform buttonList, UI_Book containingBook, UI_Book.Page containingPage, string leftPage, string rightPage)
    {
        var layoutGO = new GameObject();
        layoutGO.transform.SetParent(buttonList, false);
        layoutGO.name = "Page Swap PaApi";
        
        var layout = layoutGO.AddComponent<FlexibleGridLayout>();
        layout.spacing = new Vector2(10, 0);
        layout.childAlignment = TextAnchor.MiddleCenter;
        
        RectTransform rect = (layout.transform as RectTransform);
        rect!.sizeDelta = rect.sizeDelta with {y = 160};
        var leftButton = Object.Instantiate(_settingTabButtonPrefab.GetChild(1).gameObject, layoutGO.transform).GetComponent<MultiElementButton>();
        var rightButton = Object.Instantiate(_settingTabButtonPrefab.GetChild(1).gameObject, layoutGO.transform).GetComponent<MultiElementButton>();

        var leftUi = leftButton.GetComponent<UI_Button>();
        leftButton.uiElement = leftUi;
        
        var rightUi = rightButton.GetComponent<UI_Button>();
        rightButton.uiElement = rightUi;
            
        leftUi!.transform.localScale = Vector3.one;
        leftUi.SetLocalization(leftUi.Text, Plugin.Guid, "PrevPage", "prev");
        
        rightUi!.transform.localScale = Vector3.one;
        rightUi.SetLocalization(rightUi.Text, Plugin.Guid, "NextPage", "next");
        
        containingPage.SubElements.Add(leftUi);
        containingPage.SubElements.Add(rightUi);

        if (string.IsNullOrEmpty(leftPage))
        {
            leftButton.LockButtonState(true);
        }
        else
        {
            leftButton.onClick = new();
            leftButton.onClick.AddListener(() =>
            {
                containingBook.ForceSwapPage(leftPage);
            });
        }
        
        if (string.IsNullOrEmpty(rightPage))
        {
            rightButton.LockButtonState(true);
        }
        else
        {
            rightButton.onClick = new();
            rightButton.onClick.AddListener(() =>
            {
                containingBook.ForceSwapPage(rightPage);
            });
        }

        return rect;
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