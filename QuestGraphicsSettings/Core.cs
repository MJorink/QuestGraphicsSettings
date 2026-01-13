using MelonLoader;
using MelonLoader.Preferences;
using BoneLib.BoneMenu;
using BoneLib.Notifications;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(QuestGraphicsSettings.Core), "QuestGraphicsSettings", "1.1.2", "jorink")]
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
        private int PresetFPS;
        private float PresetRenderScale;
        private float PresetRenderDistance;
        private float PresetLODBias;        
        private float PresetTextureStreamingBudget;
        private bool PresetFog;
        private bool PresetTextureStreaming;
        private bool PresetLowPhysics;
        private bool PresetExperimental;

        // Other Variables
        private Camera playerCamera;
        private GameObject fogObject;
        private GameObject testfogObject;

        private float testfogdelay = 5f;
        private float lasttesttime = 0f;
        private int fogtestamount = 1;
        private float presetdelay = 2f;
        private float lastpresettime = 0f;

        public override void OnInitializeMelon() {
            SetupMelonPreferences();
            SetupBoneMenu();
            Preset = "Custom";
        }

        private void SetupBoneMenu() {
            Page defaultPage = Page.Root.CreatePage("QuestGraphicsSettings", Color.yellow);
            defaultPage.CreateInt("Target FPS", Color.green, FPSEntry.Value, 10, 10, 120, (a) => { FPSEntry.Value = a; });
            defaultPage.CreateFloat("Render Scale", Color.green, RenderScaleEntry.Value, 0.05f, 0.50f, 2.0f, (a) => { RenderScaleEntry.Value = a; });
            defaultPage.CreateFloat("Render Distance", Color.yellow, RenderDistanceEntry.Value, 5f, 5f, 150f, (a) => { RenderDistanceEntry.Value = a; });
            defaultPage.CreateFloat("LOD Bias", Color.yellow, LODBiasEntry.Value, 0.05f, 0.50f, 3.0f, (a) => { LODBiasEntry.Value = a; });
            defaultPage.CreateFloat("Texture Streaming Budget", Color.yellow, TextureStreamingBudgetEntry.Value, 32f, 32f, 3072f, (a) => { TextureStreamingBudgetEntry.Value = a; });
            defaultPage.CreateBool("Fog", Color.green, FogEntry.Value, (a) => { FogEntry.Value = a; });
            defaultPage.CreateFunction("Apply Settings", Color.cyan, () => { ApplySettings(); });

            Page presetsPage = defaultPage.CreatePage("Presets", Color.magenta);
            presetsPage.CreateFunction("PRESS ME", Color.red, () => { PresetsWarning(); });
            presetsPage.CreateFunction("Jorink's Preset", Color.magenta, () => { JorinksPreset(); ApplySettings(); });
            presetsPage.CreateFunction("Very Low", Color.green, () => { VeryLowPreset(); ApplySettings(); });
            presetsPage.CreateFunction("Low", Color.green, () => { LowPreset(); ApplySettings(); });
            presetsPage.CreateFunction("Medium", Color.yellow, () => { MediumPreset(); ApplySettings(); });
            presetsPage.CreateFunction("High", Color.red, () => { HighPreset(); ApplySettings(); });
            presetsPage.CreateFunction("Default", Color.blue, () => { DefaultPreset(); ApplySettings(); });
            presetsPage.CreateFunction("Custom", Color.cyan, () => { CustomPreset(); ApplySettings(); });

            Page autopresetPage = presetsPage.CreatePage("Auto Preset (WIP)", Color.magenta);
            autopresetPage.CreateFunction("Toggle Auto Preset", Color.cyan, () => { ToggleAutoPreset();});
            autopresetPage.CreateInt("Target FPS", Color.green, FPSEntry.Value, 10, 10, 120, (a) => { FPSEntry.Value = a; });

            Page advancedPage = defaultPage.CreatePage("Advanced Settings", Color.red);
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
            if (Preset == "Custom") {
                ApplySettings();
            }
            else {
                DefaultPreset();
                ApplySettings();
            }
            fogtestamount = 1;
        }

        private void ApplySettings() {
            SetFPS();
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
            PresetFPS = 90;
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
            PresetFPS = 90;
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
            PresetFPS = 90;
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
            PresetFPS = 90;
            PresetFog = true;
            PresetRenderScale = 1.2f;
            PresetTextureStreaming = true;
            PresetTextureStreamingBudget = 512f;
            PresetLODBias = 1.25f;
            PresetRenderDistance = 100f;
            PresetLowPhysics = false;
            PresetExperimental = false;
        }
        
        private void JorinksPreset() {
            Preset = "Jorink";
            PresetFPS = 90;
            PresetFog = false;
            PresetRenderScale = 0.85f;
            PresetTextureStreaming = true;
            PresetTextureStreamingBudget = 256f;
            PresetLODBias = 0.85f;
            PresetRenderDistance = 85f;
            PresetLowPhysics = true;
            PresetExperimental = true;
        }     

        private void DefaultPreset() {
            Preset = "Default";
            PresetFPS = 90;
            PresetFog = true;
            PresetRenderScale = 1.0f;
            PresetTextureStreaming = true;
            PresetTextureStreamingBudget = 512f;
            PresetLODBias = 1f;
            PresetRenderDistance = 90f;
            PresetLowPhysics = false;
            PresetExperimental = false;
        }  

        private void CustomPreset() {
            Preset = "Custom";
        } 
        //

        public override void OnUpdate() {
            base.OnUpdate();
            AutoPreset();
            TestFog();   
        }

        private void ToggleAutoPreset() {
            AutoPresetState = !AutoPresetState;
            if (AutoPresetState) {
                DefaultPreset();
                ApplySettings();
            }
        }

        private void AutoPreset() {
            if (!AutoPresetState) {
                return;
            }

            if (Time.time - lastpresettime < presetdelay) {
                return;
            }
            
            lastpresettime = Time.time;

            // Performance Drop, Lower Preset
            if (OnDemandRendering.effectiveRenderFrameRate < FPSEntry.Value - 5) {
                MelonLogger.Msg("Performance drop detected:" + OnDemandRendering.effectiveRenderFrameRate + "|" + FPSEntry.Value);

                if (Preset == "VeryLow") {
                    return;
                }
                else if (Preset == "Low") {
                    VeryLowPreset();
                }
                else if (Preset == "Medium") {
                    LowPreset();
                }
                else if (Preset == "Default") {
                    MediumPreset();
                }
                else if (Preset == "High") {
                    DefaultPreset();
                }
                else {
                    MediumPreset();
                }
            }
        }

        private void TestFog() {
            // Repeat 3 times per scene load
            if (fogtestamount > 3) {
                return;
            }

            // 5 Second delay after each test.
            if (Time.time - lasttesttime < testfogdelay) {
                return;
            }

            lasttesttime = Time.time;
            testfogObject = null;

            // Try to find fog objects, keep first match found
            testfogObject = GameObject.Find("Volumetrics");
            if (testfogObject == null) testfogObject = GameObject.Find("Volumetric");
            if (testfogObject == null) testfogObject = GameObject.Find("VolumetricFog");
            if (testfogObject == null) testfogObject = GameObject.Find("Fog");
            if (testfogObject == null) testfogObject = GameObject.Find("fog");

            if (testfogObject != null) {
                MelonLogger.Msg("Fog object found: " + testfogObject.name + " Scene: " + SceneManager.GetActiveScene().name);
            }
            else {
                MelonLogger.Msg("No fog object found in scene: " + SceneManager.GetActiveScene().name + ".");
            }

            fogtestamount += 1;
        }



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

        private void SetFPS() {
            QualitySettings.vSyncCount = 0;
            if (Preset == "Custom") {
            Application.targetFrameRate = FPSEntry.Value;
            Time.fixedDeltaTime = 1f / FPSEntry.Value;
            }
            else {
            Application.targetFrameRate = PresetFPS;
            Time.fixedDeltaTime = 1f / PresetFPS;
            }
        }

        private void SetFog() {
            if (fogObject == null)
            {
                fogObject = GameObject.Find("Volumetrics");
            }
            
            if (fogObject != null)
            {
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