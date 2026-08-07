using MelonLoader;
using BoneLib;
using BoneLib.BoneMenu;
using BoneLib.BoneMenu.UI;
using Page = BoneLib.BoneMenu.Page;
using System;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Il2CppSLZ.Bonelab;
using Il2CppTMPro;

[assembly: MelonInfo(typeof(QuestGraphicsSettings.Core), "QuestGraphicsSettings", "3.0.0", "Jorink")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace QuestGraphicsSettings
{
    public class Core : MelonMod
    {

        private sealed class GraphicsPreset
        {
            public float RenderScale { get; set; }
            public float RenderDistance { get; set; }
            public bool DisableRenderDistanceTweaks { get; set; }
            public float LODBias { get; set; }
            public int FFRLevel { get; set; }
        }

        MelonPreferences_Category category;
        
        MelonPreferences_Entry<float> RenderScaleEntry;
        MelonPreferences_Entry<float> RenderDistanceEntry;
        MelonPreferences_Entry<bool> DisableRenderDistanceTweaksEntry;
        MelonPreferences_Entry<float> LODBiasEntry;
        MelonPreferences_Entry<int> FFRLevelEntry;
        MelonPreferences_Entry<string> CustomPresetsEntry;

        private int ffrLevel;
        private Camera playerCamera;
        private Camera defaultRenderDistanceCamera;
        private bool hasDefaultRenderDistance = false;
        private float defaultRenderDistance;
        private float TimerStart;
        private bool ApplyNeeded = true;
        private Page defaultPage;
        private Page presetsPage;
        private GameObject pauseMenuButton;
        private string customPresetNameInput = string.Empty;
        private readonly Dictionary<string, FunctionElement> customPresetElements = new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, GraphicsPreset> builtInPresets = new(StringComparer.OrdinalIgnoreCase)
        {
            ["low"] = new GraphicsPreset
            {
                RenderScale = 0.80f,
                RenderDistance = 60f,
                DisableRenderDistanceTweaks = true,
                LODBias = 1.00f,
                FFRLevel = 3,
            },
            ["medium"] = new GraphicsPreset
            {
                RenderScale = 1.00f,
                RenderDistance = 120f,
                DisableRenderDistanceTweaks = true,
                LODBias = 1.25f,
                FFRLevel = 3,
            },
            ["high"] = new GraphicsPreset
            {
                RenderScale = 1.50f,
                RenderDistance = 200f,
                DisableRenderDistanceTweaks = true,
                LODBias = 1.50f,
                FFRLevel = 3,
            },
        };

        private readonly Dictionary<string, GraphicsPreset> customPresets = new(StringComparer.OrdinalIgnoreCase);

        public override void OnInitializeMelon()
        {
        	base.OnInitializeMelon();
            SetupMelonPreferences();
            SetupBoneMenu();
            Hooking.OnLevelLoaded += OnLevelLoaded;
            Hooking.OnUIRigCreated += SetupPauseMenuButton;
        }

        public override void OnDeinitializeMelon()
        {
        	base.OnDeinitializeMelon();
            Hooking.OnLevelLoaded -= OnLevelLoaded;
            Hooking.OnUIRigCreated -= SetupPauseMenuButton;
        }

        private void OnLevelLoaded(LevelInfo levelInfo)
        {
            ApplySettings();
            TimerStart = Time.time;
            ApplyNeeded = true;
            SetupPauseMenuButton();
        }

        private void SetupBoneMenu()
        {
            defaultPage = Page.Root.CreatePage("Jorink", Color.red).CreatePage("QuestGraphicsSettings", Color.red);
            defaultPage.CreateFloat("Render Scale", Color.yellow, RenderScaleEntry.Value, 0.05f, 0.50f, 2.0f, (a) => { RenderScaleEntry.Value = a; SetRenderScale(); });
            defaultPage.CreateFloat("LOD Bias", Color.yellow, LODBiasEntry.Value, 0.05f, 0.50f, 2.0f, (a) => { LODBiasEntry.Value = a; SetLODBias(); });
            defaultPage.CreateInt("FFR Level", Color.green, FFRLevelEntry.Value, 1, 0, 3, (a) => { FFRLevelEntry.Value = a; SetFFR(); });
            defaultPage.CreateFunction("Save Settings", Color.cyan, () => { MelonPreferences.Save(); });

            Page experimentalPage = defaultPage.CreatePage("Experimental", Color.yellow);
            experimentalPage.CreateFloat("Render Distance", Color.green, RenderDistanceEntry.Value, 5f, 5f, 300f, (a) => { RenderDistanceEntry.Value = a; SetRenderDistance(); });
            experimentalPage.CreateBool("Disable Render Distance Tweaks", Color.cyan, DisableRenderDistanceTweaksEntry.Value, (a) => { DisableRenderDistanceTweaksEntry.Value = a; SetRenderDistance(); });

            presetsPage = defaultPage.CreatePage("Presets", Color.magenta);
            presetsPage.CreateString("Custom Preset Name", Color.white, customPresetNameInput, (value) => { customPresetNameInput = NormalizePresetName(value); });
            presetsPage.CreateFunction("Save Manual Settings As Preset", Color.cyan, SaveCurrentAsCustomPreset);
            presetsPage.CreateFunction("Delete Preset", Color.red, RemoveCustomPreset);

            CreatePresetMenuEntry("low", true);
            CreatePresetMenuEntry("medium", true);
            CreatePresetMenuEntry("high", true);

            foreach (string presetName in customPresets.Keys)
            {
                CreatePresetMenuEntry(presetName, false);
            }
        }

        private void SetupPauseMenuButton()
        {
            if (pauseMenuButton != null) return;

            try
            {
                TrySetupPauseMenuButton();
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"Failed to set up pause menu button: {ex.Message}");
            }
        }

        private void TrySetupPauseMenuButton()
        {

            UIRig uiRig = Player.UIRig;
            if (uiRig == null) return;

            PreferencesPanelView panelView = uiRig.popUpMenu.preferencesPanelView;
            if (panelView == null) return;

            GameObject optionsPage = panelView.pages[panelView.defaultPage];

            Transform grid = optionsPage.transform.Find("grid_Options");
            Transform controlButtonTransform = grid.Find("button_Control");

            GameObject button = UnityEngine.Object.Instantiate(controlButtonTransform.gameObject, grid, false);
            button.name = "button_QuestGraphicsSettings";

            Transform quitButtonTransform = grid.Find("button_Quit");
            if (quitButtonTransform != null)
            {
                button.transform.SetSiblingIndex(quitButtonTransform.GetSiblingIndex());
            }

            TMP_Text buttonText = button.transform.Find("text_Control")?.GetComponent<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = "Graphics";
                buttonText.gameObject.name = "text_QuestGraphicsSettings";
            }
            Button buttonComponent = button.GetComponent<Button>();
            buttonComponent.onClick = new Button.ButtonClickedEvent();
            buttonComponent.onClick.AddListener((UnityEngine.Events.UnityAction)(() => OpenSettingsFromPauseMenu(panelView)));

            pauseMenuButton = button;
        }

        private void OpenSettingsFromPauseMenu(PreferencesPanelView panelView)
        {
            int guiMenuPageIndex = FindGUIMenuPageIndex(panelView);
            if (guiMenuPageIndex >= 0)
            {
                panelView.PAGESELECT(guiMenuPageIndex);
            }

            Menu.OpenPage(defaultPage);
        }

        private int FindGUIMenuPageIndex(PreferencesPanelView panelView)
        {
            GameObject guiMenuObject = GUIMenu.Instance?.gameObject;
            if (guiMenuObject == null) return -1;

            for (int i = 0; i < panelView.pages.Length; i++)
            {
                if (panelView.pages[i] == guiMenuObject) return i;
            }

            return -1;
        }

        private void SetupMelonPreferences()
        {
            category = MelonPreferences.CreateCategory("QuestGraphicsSettings");
            RenderScaleEntry = category.CreateEntry("Render Scale", 1.0f);
            RenderDistanceEntry = category.CreateEntry("Render Distance", 200f);
            DisableRenderDistanceTweaksEntry = category.CreateEntry("Disable Render Distance Tweaks", true);
            LODBiasEntry = category.CreateEntry("LOD Bias", 1.5f);
            FFRLevelEntry = category.CreateEntry("FFR Level", 3);
            CustomPresetsEntry = category.CreateEntry("Custom Presets", "{}");

            LoadCustomPresets();

            MelonPreferences.Save();
            category.SaveToFile();
        }
        
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            ApplySettings();
            ApplyNeeded = true;
        }

        public override void OnUpdate()
        {
        	base.OnUpdate();
            if (!ApplyNeeded) return;
            if (Time.time - TimerStart < 5f) return;
            ApplySettings();
            ApplyNeeded = false;
            TimerStart = Time.time;
        }

        private void ApplySettings()
        {
            SetRenderScale();
            SetRenderDistance();
            SetLODBias();
            SetFFR();
        }

        private void SetRenderScale()
        {
            UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;
            if (asset == null) return;

            asset.renderScale = RenderScaleEntry.Value;
        }

        private void SetRenderDistance()
        {
            if (playerCamera == null)
            {
                playerCamera = UnityEngine.Object.FindObjectOfType<Camera>();
            }
            if (playerCamera == null) return;
            if (!hasDefaultRenderDistance || defaultRenderDistanceCamera != playerCamera)
            {
                defaultRenderDistanceCamera = playerCamera;
                defaultRenderDistance = playerCamera.farClipPlane;
                hasDefaultRenderDistance = true;
            }
            if (DisableRenderDistanceTweaksEntry.Value)
            {
                playerCamera.farClipPlane = defaultRenderDistance;
                playerCamera.useOcclusionCulling = false;
            }
            else
            {
                playerCamera.farClipPlane = RenderDistanceEntry.Value;
                playerCamera.useOcclusionCulling = true;
            }
        }

        private void SetLODBias()
        {
            QualitySettings.lodBias = LODBiasEntry.Value;
        }

        private void SetFFR()
        {
            ffrLevel = FFRLevelEntry.Value;
            Unity.XR.Oculus.Utils.useDynamicFoveatedRendering = false;
            Unity.XR.Oculus.Utils.foveatedRenderingLevel = ffrLevel;
        }

        private void ApplyPreset(string presetName)
        {
            if (string.Equals(presetName, "manual", StringComparison.OrdinalIgnoreCase))
            {
                ApplySettings();
                return;
            }

            if (!TryGetPreset(presetName, out GraphicsPreset preset))
            {
                MelonLogger.Warning($"Preset '{presetName}' was not found.");
                return;
            }

            RenderScaleEntry.Value = preset.RenderScale;
            RenderDistanceEntry.Value = preset.RenderDistance;
            DisableRenderDistanceTweaksEntry.Value = preset.DisableRenderDistanceTweaks;
            LODBiasEntry.Value = preset.LODBias;
            FFRLevelEntry.Value = preset.FFRLevel;

            ApplySettings();
            MelonPreferences.Save();
            category.SaveToFile();
        }

        private void SaveCurrentAsCustomPreset()
        {
            string presetName = NormalizePresetName(customPresetNameInput);
            if (string.IsNullOrWhiteSpace(presetName))
            {
                MelonLogger.Warning("Custom preset name cannot be empty.");
                return;
            }

            if (IsReservedPresetName(presetName))
            {
                MelonLogger.Warning($"Preset name '{presetName}' is reserved.");
                return;
            }

            bool isOverwrite = customPresets.ContainsKey(presetName);

            customPresets[presetName] = CreateCurrentPreset();
            CustomPresetsEntry.Value = JsonSerializer.Serialize(customPresets);
            MelonPreferences.Save();
            category.SaveToFile();

            if (!isOverwrite)
            {
                CreatePresetMenuEntry(presetName, false);
            }

            MelonLogger.Msg(isOverwrite ? $"Overwrote custom preset '{presetName}'." : $"Saved custom preset '{presetName}'.");
        }

        private void LoadCustomPresets()
        {
            customPresets.Clear();
            if (string.IsNullOrWhiteSpace(CustomPresetsEntry.Value)) return;

            try
            {
                Dictionary<string, GraphicsPreset> presets = JsonSerializer.Deserialize<Dictionary<string, GraphicsPreset>>(CustomPresetsEntry.Value);
                if (presets == null) return;

                foreach ((string presetName, GraphicsPreset preset) in presets)
                {
                    if (IsReservedPresetName(presetName) || preset == null)
                    {
                        continue;
                    }

                    customPresets[presetName] = preset;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"Failed to load custom presets: {ex.Message}");
                CustomPresetsEntry.Value = "{}";
            }
        }

        private void RemoveCustomPreset()
        {
            string presetName = NormalizePresetName(customPresetNameInput);
            if (string.IsNullOrWhiteSpace(presetName))
            {
                MelonLogger.Warning("Preset name cannot be empty.");
                return;
            }

            if (!customPresets.ContainsKey(presetName))
            {
                MelonLogger.Warning($"Custom preset '{presetName}' does not exist.");
                return;
            }

            customPresets.Remove(presetName);
            CustomPresetsEntry.Value = JsonSerializer.Serialize(customPresets);
            MelonPreferences.Save();
            category.SaveToFile();

            if (customPresetElements.TryGetValue(presetName, out FunctionElement element))
            {
                presetsPage.Remove(element);
                customPresetElements.Remove(presetName);
            }
        }

        private void CreatePresetMenuEntry(string presetName, bool isBuiltIn)
        {
            if (presetsPage == null) return;

            Color presetColor = isBuiltIn ? Color.yellow : Color.green;
            FunctionElement element = presetsPage.CreateFunction(presetName, presetColor, () =>
            {
                ApplyPreset(presetName);
            });

            if (!isBuiltIn)
            {
                customPresetElements[presetName] = element;
            }
        }

        private GraphicsPreset CreateCurrentPreset()
        {
            return new GraphicsPreset
            {
                RenderScale = RenderScaleEntry.Value,
                RenderDistance = RenderDistanceEntry.Value,
                DisableRenderDistanceTweaks = DisableRenderDistanceTweaksEntry.Value,
                LODBias = LODBiasEntry.Value,
                FFRLevel = FFRLevelEntry.Value,
            };
        }

        private bool TryGetPreset(string presetName, out GraphicsPreset preset)
        {
            string normalizedPresetName = NormalizePresetName(presetName);
            if (builtInPresets.TryGetValue(normalizedPresetName, out preset))
            {
                return true;
            }

            return customPresets.TryGetValue(normalizedPresetName, out preset);
        }

        private bool IsReservedPresetName(string presetName)
        {
            string normalizedPresetName = NormalizePresetName(presetName);
            return string.Equals(normalizedPresetName, "manual", StringComparison.OrdinalIgnoreCase) || builtInPresets.ContainsKey(normalizedPresetName);
        }

        private string NormalizePresetName(string presetName)
        {
            return presetName?.Trim() ?? string.Empty;
        }
    }
}
