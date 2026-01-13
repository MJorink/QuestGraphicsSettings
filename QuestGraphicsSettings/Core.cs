using MelonLoader;
using MelonLoader.Preferences;
using BoneLib.BoneMenu;
using BoneLib.Notifications;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(QuestGraphicsSettings.Core), "QuestGraphicsSettings", "1.2.0", "jorink")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace QuestGraphicsSettings {
    public class Core : MelonMod {

        MelonPreferences_Category category;

        // Default Page Entries
        MelonPreferences_Entry<int> FPSEntry;
        MelonPreferences_Entry<float> RenderScaleEntry;
        MelonPreferences_Entry<float> RenderDistanceEntry;
        MelonPreferences_Entry<float> LODBiasEntry;        
        MelonPreferences_Entry<float> TextureStreamingBudgetEntry;
        MelonPreferences_Entry<bool> FogEntry;

        // Advanced Page Entries
        MelonPreferences_Entry<bool> TextureStreamingEntry;
        MelonPreferences_Entry<bool> LowPhysicsEntry;
        MelonPreferences_Entry<bool> ExperimentalEntry;

        // Preset Variables
        private bool AutoPresetState;
        private string Preset;
        private float PresetRenderScale;
        private float PresetRenderDistance;
        private float PresetLODBias;        
        private float PresetTextureStreamingBudget;
        private bool PresetFog;
        private bool PresetTextureStreaming;
        private bool PresetLowPhysics;
        private bool PresetExperimental;
        private int PerformanceDrops;

        // Other Variables
        private Camera playerCamera;
        private GameObject fogObject;

        private float presetdelay = 5f;
        private float lastpresettime = 0f;

        // FPS tracking
        private float[] frameTimeSamples = new float[30];
        private int frameIndex = 0;

        public override void OnInitializeMelon() {
            SetupMelonPreferences();
            SetupBoneMenu();
            Preset = "Custom";
        }

        private void SetupBoneMenu() {
            Page defaultPage = Page.Root.CreatePage("QuestGraphicsSettings", Color.yellow);

            Page customPage = defaultPage.CreatePage("Custom Preset Settings", Color.cyan);
            customPage.CreateFloat("Render Scale", Color.green, RenderScaleEntry.Value, 0.05f, 0.50f, 2.0f, (a) => { RenderScaleEntry.Value = a; });
            customPage.CreateFloat("Render Distance", Color.yellow, RenderDistanceEntry.Value, 5f, 5f, 150f, (a) => { RenderDistanceEntry.Value = a; });
            customPage.CreateFloat("LOD Bias", Color.yellow, LODBiasEntry.Value, 0.05f, 0.50f, 3.0f, (a) => { LODBiasEntry.Value = a; });
            customPage.CreateFloat("Texture Streaming Budget", Color.yellow, TextureStreamingBudgetEntry.Value, 32f, 32f, 3072f, (a) => { TextureStreamingBudgetEntry.Value = a; });
            customPage.CreateBool("Fog", Color.green, FogEntry.Value, (a) => { FogEntry.Value = a; });
            customPage.CreateFunction("Apply Settings", Color.cyan, () => { ApplySettings(); });
            customPage.CreateFunction("Save Settings", Color.cyan, () => { MelonPreferences.Save(); });
            customPage.CreateFunction("Enable Custom Preset", Color.cyan, () => { CustomPreset(); ApplySettings(); });

            Page presetsPage = defaultPage.CreatePage("Presets", Color.blue);
            presetsPage.CreateFunction("PRESS ME", Color.red, () => { PresetsWarning(); });
            presetsPage.CreateFunction("Jorink's Preset", Color.magenta, () => { JorinksPreset(); ApplySettings(); });
            presetsPage.CreateFunction("Very Low", Color.green, () => { VeryLowPreset(); ApplySettings(); });
            presetsPage.CreateFunction("Low", Color.green, () => { LowPreset(); ApplySettings(); });
            presetsPage.CreateFunction("Medium", Color.yellow, () => { MediumPreset(); ApplySettings(); });
            presetsPage.CreateFunction("High", Color.red, () => { HighPreset(); ApplySettings(); });
            presetsPage.CreateFunction("Default", Color.blue, () => { DefaultPreset(); ApplySettings(); });
            presetsPage.CreateFunction("Custom", Color.cyan, () => { CustomPreset(); ApplySettings(); });
            presetsPage.CreateFunction("Current Preset", Color.cyan, () => { CurrentPreset();});

            Page autopresetPage = defaultPage.CreatePage("Auto Preset (WIP)", Color.magenta);
            autopresetPage.CreateFunction("Toggle Auto Preset", Color.cyan, () => { ToggleAutoPreset();});
            autopresetPage.CreateInt("Target FPS", Color.green, FPSEntry.Value, 10, 60, 120, (a) => { FPSEntry.Value = a; });
            autopresetPage.CreateFunction("Current Preset", Color.cyan, () => { CurrentPreset();});

            Page advancedPage = customPage.CreatePage("Advanced Settings", Color.red);
            advancedPage.CreateFunction("PRESS ME", Color.red, () => { AdvancedWarning(); });
            advancedPage.CreateBool("Texture Streaming (!)", Color.red, TextureStreamingEntry.Value, (a) => { TextureStreamingEntry.Value = a; });
            advancedPage.CreateBool("Low Physics (!)", Color.red, LowPhysicsEntry.Value, (a) => { LowPhysicsEntry.Value = a; });
            advancedPage.CreateBool("Experimental Tweaks (!)", Color.red, ExperimentalEntry.Value, (a) => { ExperimentalEntry.Value = a; });
            advancedPage.CreateFunction("Apply Settings", Color.cyan, () => { ApplySettings(); });
        }

        private void SetupMelonPreferences() {
            category = MelonPreferences.CreateCategory("QuestGraphicsSettings");
            FogEntry = category.CreateEntry("Fog Enabled", true);
            RenderScaleEntry = category.CreateEntry("Render Scale", 1.0f);
            TextureStreamingEntry = category.CreateEntry("Texture Streaming Enabled", true);
            TextureStreamingBudgetEntry = category.CreateEntry("Texture Streaming Budget", 512f);
            LODBiasEntry = category.CreateEntry("LOD Bias", 1f);
            RenderDistanceEntry = category.CreateEntry("Render Distance", 90f);
            FPSEntry = category.CreateEntry("Target FPS", 90);
            ExperimentalEntry = category.CreateEntry("Experimental Tweaks", false);
            LowPhysicsEntry = category.CreateEntry("Low Physics", false);
            MelonPreferences.Save();
            category.SaveToFile();
        }

        private void AdvancedWarning() {
             Menu.DisplayDialog(
            "WARNING",
            "These settings are experimental and minimally tested, and may cause bugs or crashes. Proceed with caution!"
            );
        }

        private void PresetsWarning() {
             Menu.DisplayDialog(
            "WARNING",
            "Some presets also use experimental options, these settings are experimental and minimally tested, and may cause bugs or crashes. Proceed with caution! Preset Custom will disable presets and use your own settings in the normal menu."
            );
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            if (AutoPresetState) {
                DefaultPreset();
                ApplySettings();
                PerformanceDrops = 0;
            }
            else {
                ApplySettings();
            }
        }

        private void ApplySettings() {
            SetPhysics();
            SetTextureStreaming();
            SetRenderScale();
            SetExperimental();
            SetRenderDistance();
            SetLODBias();
            SetFog();
        }

        // Presets
        private void VeryLowPreset() {
            Preset = "VeryLow";
            PresetFog = false;
            PresetRenderScale = 0.6f;
            PresetTextureStreaming = true;
            PresetTextureStreamingBudget = 64f;
            PresetLODBias = 0.5f;
            PresetRenderDistance = 50f;
            PresetLowPhysics = true;
            PresetExperimental = true;
        }

        private void LowPreset() {
            Preset = "Low";
            PresetFog = false;
            PresetRenderScale = 0.8f;
            PresetTextureStreaming = true;
            PresetTextureStreamingBudget = 128f;
            PresetLODBias = 0.7f;
            PresetRenderDistance = 70f;
            PresetLowPhysics = true;
            PresetExperimental = true;
        }

        private void MediumPreset() {
            Preset = "Medium";
            PresetFog = false;
            PresetRenderScale = 1.0f;
            PresetTextureStreaming = true;
            PresetTextureStreamingBudget = 256f;
            PresetLODBias = 0.8f;
            PresetRenderDistance = 80f;
            PresetLowPhysics = false;
            PresetExperimental = true;
        }

        private void HighPreset() {
            Preset = "High";
            PresetFog = false;
            PresetRenderScale = 1.5f;
            PresetTextureStreaming = true;
            PresetTextureStreamingBudget = 256f;
            PresetLODBias = 1.25f;
            PresetRenderDistance = 100f;
            PresetLowPhysics = false;
            PresetExperimental = false;
        }
        
        private void JorinksPreset() {
            Preset = "Jorink";
            PresetFog = false;
            PresetRenderScale = 0.9f;
            PresetTextureStreaming = true;
            PresetTextureStreamingBudget = 256f;
            PresetLODBias = 0.85f;
            PresetRenderDistance = 85f;
            PresetLowPhysics = true;
            PresetExperimental = true;
        }     

        private void DefaultPreset() {
            Preset = "Default";
            PresetFog = false;
            PresetRenderScale = 1.0f;
            PresetTextureStreaming = true;
            PresetTextureStreamingBudget = 256f;
            PresetLODBias = 1f;
            PresetRenderDistance = 90f;
            PresetLowPhysics = false;
            PresetExperimental = false;
        }  

        private void CustomPreset() {
            Preset = "Custom";
        } 

        private void CurrentPreset() {
            var notif = new Notification {
                Title = "Current Preset",
                Message = Preset,
                Type = NotificationType.Success,
                PopupLength = 2f,
                ShowTitleOnPopup = true
            };
            Notifier.Send(notif);
        }
        //

        public override void OnUpdate() {
            base.OnUpdate();
            // Track frame times
            frameTimeSamples[frameIndex] = Time.unscaledDeltaTime;
            frameIndex = (frameIndex + 1) % frameTimeSamples.Length;
            AutoPreset();
        }

        private float GetAverageFPS() {
            float total = 0f;
            for (int i = 0; i < frameTimeSamples.Length; i++) {
                total += frameTimeSamples[i];
            }
            float avgDeltaTime = total / frameTimeSamples.Length;
            return avgDeltaTime > 0 ? 1f / avgDeltaTime : 0f;
        }

        private void ToggleAutoPreset() {
            AutoPresetState = !AutoPresetState;
            if (AutoPresetState) {
                DefaultPreset();
                ApplySettings();
            }

            var notif = new Notification {
                Title = "Auto Preset",
                Message = AutoPresetState ? "Enabled" : "Disabled",
                Type = AutoPresetState ? NotificationType.Success : NotificationType.Warning,
                PopupLength = 2f,
                ShowTitleOnPopup = true
            };
            Notifier.Send(notif);
        }

        private void AutoPreset() {
            if (!AutoPresetState) {
                return;
            }

            if (Time.time - lastpresettime < presetdelay) {
                return;
            }
            
            float currentFPS = GetAverageFPS();
            lastpresettime = Time.time;

            // Performance Drop, Lower Preset
            if (currentFPS < FPSEntry.Value - 10) {
                
                PerformanceDrops += 1;
    
                MelonLogger.Msg("Performance drop detected " + PerformanceDrops + "/3");
                MelonLogger.Msg("Performance drop FPS:" + currentFPS + "|" + FPSEntry.Value);

                if (PerformanceDrops >= 3) {
                PerformanceDrops = 0;
                if (Preset == "VeryLow") {
                    return;
                }
                else if (Preset == "Low") {
                    VeryLowPreset();
                    ApplySettings();
                }
                else if (Preset == "Medium") {
                    LowPreset();
                    ApplySettings();
                }
                else if (Preset == "Default") {
                    MediumPreset();
                    ApplySettings();
                }
                else if (Preset == "High") {
                    DefaultPreset();
                    ApplySettings();
                }
                else {
                    MediumPreset();
                    ApplySettings();
                }
            }
            }}
        

        private void SetTextureStreaming() {
            if (Preset == "Custom") {
            QualitySettings.streamingMipmapsActive = TextureStreamingEntry.Value;
            QualitySettings.streamingMipmapsMemoryBudget = TextureStreamingBudgetEntry.Value;        
            }
            else {
            QualitySettings.streamingMipmapsActive = PresetTextureStreaming;
            QualitySettings.streamingMipmapsMemoryBudget = PresetTextureStreamingBudget;        
            }          
        }

        private void SetLODBias() {
            if (Preset == "Custom") {
            QualitySettings.lodBias = LODBiasEntry.Value;
            }
            else {
            QualitySettings.lodBias = PresetLODBias;
            }
        }

        private void SetFog() {
            if (fogObject == null) fogObject = GameObject.Find("Volumetrics");
            if (fogObject == null) fogObject = GameObject.Find("Volumetric");
            if (fogObject == null) fogObject = GameObject.Find("VolumetricFog");
            if (fogObject == null) fogObject = GameObject.Find("Fog");
            if (fogObject == null) fogObject = GameObject.Find("fog");
            if (fogObject == null) MelonLogger.Msg("No fog object found in scene: " + SceneManager.GetActiveScene().name);
            
            
            if (fogObject != null)
            {
                MelonLogger.Msg("Fog Object Found: " + fogObject.name + " Scene:" + SceneManager.GetActiveScene().name);
                if (Preset == "Custom")
                {
                    fogObject.SetActive(FogEntry.Value);
                }
                else
                {
                    fogObject.SetActive(PresetFog);
                }
            }
        }

        private void SetRenderDistance() {
			if ((UnityEngine.Object)(object)playerCamera == (UnityEngine.Object)null)
			{
				playerCamera = UnityEngine.Object.FindObjectOfType<Camera>();
			}
			if ((UnityEngine.Object)(object)playerCamera != (UnityEngine.Object)null)
			{
                if (Preset == "Custom") {
                    playerCamera.farClipPlane = RenderDistanceEntry.Value; }
                else {
				    playerCamera.farClipPlane = PresetRenderDistance; }
			}
			playerCamera.useOcclusionCulling = true;
		}

        private void SetRenderScale() {
			UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;
            if (Preset == "Custom") {
            asset.renderScale = RenderScaleEntry.Value; }
            else {
            asset.renderScale = PresetRenderScale; }
        }   

        private void SetPhysics() {
            if (Preset == "Custom") {
            if (LowPhysicsEntry.Value) {
                Physics.defaultSolverIterations = 1;
                Physics.defaultSolverVelocityIterations = 1;
                Physics.sleepThreshold = 0.04f;
                Physics.defaultContactOffset = 0.01f;
            }
            
            if (!LowPhysicsEntry.Value) {
                Physics.defaultSolverIterations = 6;
                Physics.defaultSolverVelocityIterations = 2;
                Physics.sleepThreshold = 0.01f;
                Physics.defaultContactOffset = 0.0055f;
            }}

            else {
            if (PresetLowPhysics) {
                Physics.defaultSolverIterations = 1;
                Physics.defaultSolverVelocityIterations = 1;
                Physics.sleepThreshold = 0.04f;
                Physics.defaultContactOffset = 0.01f;
            }

            if (!PresetLowPhysics) {
                Physics.defaultSolverIterations = 6;
                Physics.defaultSolverVelocityIterations = 2;
                Physics.sleepThreshold = 0.01f;
                Physics.defaultContactOffset = 0.0055f;
            }}
        }

        private void SetExperimental() {
            if (Preset == "Custom") {
            // Experimental settings for performance research
            if (ExperimentalEntry.Value) {
				RenderSettings.defaultReflectionResolution = 16;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                QualitySettings.pixelLightCount = 0;
                QualitySettings.softVegetation = false;
                QualitySettings.particleRaycastBudget = 0;
                }

            // Default BoneLab settings found by logging
            if (!ExperimentalEntry.Value) {
                RenderSettings.defaultReflectionResolution = 128;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                QualitySettings.pixelLightCount = 99;
                QualitySettings.softVegetation = true;
                QualitySettings.particleRaycastBudget = 512;
                }}

            else {
            if (PresetExperimental) {
				RenderSettings.defaultReflectionResolution = 16;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                QualitySettings.pixelLightCount = 0;
                QualitySettings.softVegetation = false;
                QualitySettings.particleRaycastBudget = 0;
                }

            if (!PresetExperimental) {
                RenderSettings.defaultReflectionResolution = 128;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                QualitySettings.pixelLightCount = 99;
                QualitySettings.softVegetation = true;
                QualitySettings.particleRaycastBudget = 512;
                }
            }
        }
    }
}
