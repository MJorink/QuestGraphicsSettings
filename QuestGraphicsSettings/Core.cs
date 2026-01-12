using MelonLoader;
using MelonLoader.Preferences;
using BoneLib.BoneMenu;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[assembly: MelonInfo(typeof(QuestGraphicsSettings.Core), "QuestGraphicsSettings", "1.1.2", "jorink")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace QuestGraphicsSettings {
    public class Core : MelonMod {

        MelonPreferences_Category category;
        MelonPreferences_Entry<bool> FogEntry;
        MelonPreferences_Entry<float> RenderScaleEntry;
        MelonPreferences_Entry<bool> TextureStreamingEntry;
        MelonPreferences_Entry<float> TextureStreamingBudgetEntry;
        MelonPreferences_Entry<float> LODBiasEntry;
        MelonPreferences_Entry<float> RenderDistanceEntry;
        MelonPreferences_Entry<bool> ExperimentalEntry;
        MelonPreferences_Entry<bool> LowPhysicsEntry;

        private Camera playerCamera;
        private GameObject fogObject;

        private GameObject testfogObject;
        private float testfogdelay = 5f;
        private float lasttesttime = 0f;
        private int fogtestamount = 1;
        
        public override void OnInitializeMelon() {
            MelonPrefs();
            GraphicsMenu();
        }

        private void GraphicsMenu() {
            Page defaultPage = Page.Root.CreatePage("Quest Graphics Settings", Color.yellow);
            defaultPage.CreateBool("Fog", Color.green, FogEntry.Value, (a) => { FogEntry.Value = a; });
            defaultPage.CreateFloat("Render Scale", Color.green, RenderScaleEntry.Value, 0.05f, 0.50f, 2.0f, (a) => { RenderScaleEntry.Value = a; });
            defaultPage.CreateFloat("Texture Streaming Budget", Color.yellow, TextureStreamingBudgetEntry.Value, 32f, 32f, 3072f, (a) => { TextureStreamingBudgetEntry.Value = a; });
            defaultPage.CreateFloat("LOD Bias", Color.yellow, LODBiasEntry.Value, 0.05f, 0.50f, 3.0f, (a) => { LODBiasEntry.Value = a; });
            defaultPage.CreateFloat("Render Distance", Color.yellow, RenderDistanceEntry.Value, 5f, 5f, 150f, (a) => { RenderDistanceEntry.Value = a; });
            defaultPage.CreateFunction("Apply Settings", Color.cyan, () => { ApplySettings(); });
            
            Page advancedPage = defaultPage.CreatePage("Advanced Settings", Color.red);
            advancedPage.CreateFunction("PRESS ME", Color.red, () => { Warning(); });
            advancedPage.CreateBool("Texture Streaming (!)", Color.red, TextureStreamingEntry.Value, (a) => { TextureStreamingEntry.Value = a; });
            advancedPage.CreateBool("Low Physics (!)", Color.red, LowPhysicsEntry.Value, (a) => { LowPhysicsEntry.Value = a; });
            advancedPage.CreateBool("Experimental Tweaks (!)", Color.red, ExperimentalEntry.Value, (a) => { ExperimentalEntry.Value = a; });
            advancedPage.CreateFunction("Apply Settings", Color.cyan, () => { ApplySettings(); });
        }

        private void MelonPrefs() {
            category = MelonPreferences.CreateCategory("QuestGraphicsSettings");
            FogEntry = category.CreateEntry("Fog Enabled", false);
            RenderScaleEntry = category.CreateEntry("Render Scale", 0.85f);
            TextureStreamingEntry = category.CreateEntry("Texture Streaming Enabled", true);
            TextureStreamingBudgetEntry = category.CreateEntry("Texture Streaming Budget", 192f);
            LODBiasEntry = category.CreateEntry("LOD Bias", 0.85f);
            RenderDistanceEntry = category.CreateEntry("Render Distance", 85f);
            ExperimentalEntry = category.CreateEntry("Experimental Tweaks", true);
            LowPhysicsEntry = category.CreateEntry("Low Physics", true);
            MelonPreferences.Save();
            category.SaveToFile();
        }

        private void Warning() {
             Menu.DisplayDialog(
            "WARNING",
            "These settings are experimental and minimally tested, and may cause bugs or crashes. Proceed with caution!"
            );
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            ApplySettings();
            fogtestamount = 1;
        }

        private void ApplySettings() {
            SetMisc();
            SetFog();
            SetRenderDistance();
            SetRenderScale();
            SetPhysics();
            SetExperimental();
        }

        private void SetMisc() {
            QualitySettings.streamingMipmapsActive = TextureStreamingEntry.Value;
            QualitySettings.streamingMipmapsMemoryBudget = TextureStreamingBudgetEntry.Value;
            QualitySettings.lodBias = LODBiasEntry.Value;
        }

        public override void OnUpdate() {
            base.OnUpdate();
            TestFog();
        }

        private void TestFog() {
            // Repeat 3 times per scene load
            if (fogtestamount > 3) {
                return;
            }

            // 5 Second delay after each test.
            testfogdelay = 5f;
            lasttesttime = Time.time;

            if (Time.time - lasttesttime < testfogdelay) {
                return;
            }

            testfogObject = null;

            testfogObject = GameObject.Find("Volumetrics");
            testfogObject = GameObject.Find("Volumetric");
            testfogObject = GameObject.Find("VolumetricFog");
            testfogObject = GameObject.Find("Fog");
            testfogObject = GameObject.Find("fog");

            if (testfogObject != null) {
                MelonLogger.Msg("Fog object found: " + testfogObject.name);
            }

            fogtestamount += 1;
        }


        private void SetFog() {
            if (fogObject == null)
            {
                fogObject = GameObject.Find("Volumetrics");
            }
            
            if (fogObject != null)
            {
                fogObject.SetActive(FogEntry.Value);
            }
        }

        private void SetRenderDistance() {
			if ((UnityEngine.Object)(object)playerCamera == (UnityEngine.Object)null)
			{
				playerCamera = UnityEngine.Object.FindObjectOfType<Camera>();
			}
			if ((UnityEngine.Object)(object)playerCamera != (UnityEngine.Object)null)
			{
				playerCamera.farClipPlane = RenderDistanceEntry.Value;
			}
			playerCamera.useOcclusionCulling = true;
		}

        private void SetRenderScale() {
			UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;
			asset.renderScale = RenderScaleEntry.Value;
        }   

        private void SetPhysics() {
            if (LowPhysicsEntry.Value) {
                Physics.defaultSolverIterations = 1;
                Physics.defaultSolverVelocityIterations = 1;
                Physics.sleepThreshold = 0.04f;
                Physics.defaultContactOffset = 0.01f;
            }

            // Default BoneLab settings found by logging
            if (!LowPhysicsEntry.Value) {
                Physics.defaultSolverIterations = 6;
                Physics.defaultSolverVelocityIterations = 2;
                Physics.sleepThreshold = 0.01f;
                Physics.defaultContactOffset = 0.0055f;
            }

        }

        private void SetExperimental() {
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
                }
        }
    }
}