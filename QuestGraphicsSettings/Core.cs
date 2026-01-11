using MelonLoader;
using MelonLoader.Preferences;
using BoneLib.BoneMenu;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[assembly: MelonInfo(typeof(QuestGraphicsSettings.Core), "QuestGraphicsSettings", "1.1.0", "jorink")]
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
        MelonPreferences_Entry<bool> FSREnabledEntry;
        MelonPreferences_Entry<float> FSRSharpnessEntry;
        MelonPreferences_Entry<bool> FSROverideEntry;

        private Camera playerCamera;
        private GameObject fogObject;
        
        public override void OnInitializeMelon() {
            MelonPrefs();
            GraphicsMenu();
        }

        private void GraphicsMenu() {
            Page defaultPage = Page.Root.CreatePage("Quest Graphics Settings", Color.yellow);
            defaultPage.CreateBool("Fog", Color.green, FogEntry.Value, (a) => { FogEntry.Value = a; });
            defaultPage.CreateFloat("Render Scale", Color.green, RenderScaleEntry.Value, 0.05f, 0.40f, 1.0f, (a) => { RenderScaleEntry.Value = a; });
            defaultPage.CreateFloat("Texture Streaming Budget", Color.yellow, TextureStreamingBudgetEntry.Value, 16f, 16f, 3072f, (a) => { TextureStreamingBudgetEntry.Value = a; });
            defaultPage.CreateFloat("LOD Bias", Color.yellow, LODBiasEntry.Value, 0.05f, 0.50f, 2.0f, (a) => { LODBiasEntry.Value = a; });
            defaultPage.CreateFloat("Render Distance", Color.green, RenderDistanceEntry.Value, 5f, 10f, 100f, (a) => { RenderDistanceEntry.Value = a; });
            defaultPage.CreateBool("FSR", Color.green, FSREnabledEntry.Value, (a) => { FSREnabledEntry.Value = a; });
            defaultPage.CreateBool("Manual FSR Sharpness", Color.green, FSROverideEntry.Value, (a) => { FSROverideEntry.Value = a; });
            defaultPage.CreateFloat("FSR Sharpness", Color.green, FSRSharpnessEntry.Value, 0.1f, 0f, 1f, (a) => { FSRSharpnessEntry.Value = a; });
            defaultPage.CreateFunction("Apply Settings", Color.cyan, () => { ApplySettings(); });
            
            Page advancedPage = defaultPage.CreatePage("Advanced Settings", Color.red);
            advancedPage.CreateFunction("PRESS ME", Color.red, () => { Warning(); });
            advancedPage.CreateBool("Texture Streaming", Color.red, TextureStreamingEntry.Value, (a) => { TextureStreamingEntry.Value = a; });
            advancedPage.CreateFunction("Apply Settings", Color.cyan, () => { ApplySettings(); });
        }

        private void MelonPrefs() {
            category = MelonPreferences.CreateCategory("QuestGraphicsSettings");
            FogEntry = category.CreateEntry("Fog Enabled", true);
            RenderScaleEntry = category.CreateEntry("Render Scale", 1.0f);
            TextureStreamingEntry = category.CreateEntry("Texture Streaming Enabled", true);
            TextureStreamingBudgetEntry = category.CreateEntry("Texture Streaming Budget", 512f);
            LODBiasEntry = category.CreateEntry("LOD Bias", 1.0f);
            RenderDistanceEntry = category.CreateEntry("Render Distance", 60f);
            FSREnabledEntry = category.CreateEntry("FSR Enabled", false);
            FSRSharpnessEntry = category.CreateEntry("FSR Sharpness", 0.5f);
            FSROverideEntry = category.CreateEntry("FSR Sharpness Override", false);
            MelonPreferences.Save();
            category.SaveToFile();
        }

        private void LogDefaults() {
            MelonLogger.Msg("Example Default: " + QualitySettings.streamingMipmapsActive);
        }

        private void Warning() {
             Menu.DisplayDialog(
            "WARNING",
            "Using high settings will cause lag or crashes. Texture Streaming should be kept on for most users. Also, don't forget to apply the settings after changing them!"
            );
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            //LogDefaults();
            ApplySettings();
        }

        private void ApplySettings() {
            SetMisc();
            SetFog();
            SetRenderDistance();
            SetFSR();
            MelonPreferences.Save();
        }

        private void SetMisc() {
            UnityEngine.XR.XRSettings.renderViewportScale = RenderScaleEntry.Value;
            QualitySettings.streamingMipmapsActive = TextureStreamingEntry.Value;
            QualitySettings.streamingMipmapsMemoryBudget = TextureStreamingBudgetEntry.Value;
            QualitySettings.lodBias = LODBiasEntry.Value;
        }

        private void SetFog()
        {
            if (fogObject == null)
            {
                fogObject = GameObject.Find("Volumetrics");
            }
            
            if (fogObject != null)
            {
                fogObject.SetActive(FogEntry.Value);
            }
        }

        private void SetRenderDistance()
		{
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

        private void SetFSR()
		{
			UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;
			if (FSREnabledEntry.Value)
			{
				asset.upscalingFilter = (UpscalingFilterSelection)3;
			}
			else
			{
				asset.upscalingFilter = (UpscalingFilterSelection)0;
			}

			asset.fsrOverrideSharpness = FSROverideEntry.Value;
			if (FSROverideEntry.Value)
			{
				asset.fsrSharpness = FSRSharpnessEntry.Value;
			}
		}    
    }
}