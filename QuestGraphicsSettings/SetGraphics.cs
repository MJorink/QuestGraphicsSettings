using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace QuestGraphicsSettings {
    public partial class Core {

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
            if (fogObject == null) fogObject = GameObject.Find("Fog");
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

        private void SetFFR() {
            try {                
                bool autoFFR;
                int ffrLevel;

                if (Preset == "Custom") {
                    autoFFR = FFRAutoEntry.Value;
                    ffrLevel = FFRLevelEntry.Value;
                } else {
                    autoFFR = PresetFFRAuto;
                    ffrLevel = PresetFFRLevel;
                }

                // Apply FFR via Unity.XR.Oculus Performance API
                if (autoFFR) {
                    // Enable dynamic/automatic FFR
                    Unity.XR.Oculus.Utils.useDynamicFoveatedRendering = true;
                    MelonLogger.Msg("FFR: Automatic/Dynamic mode enabled");
                } else {
                    // Manual FFR level
                    Unity.XR.Oculus.Utils.useDynamicFoveatedRendering = false;
                    Unity.XR.Oculus.Utils.foveatedRenderingLevel = ffrLevel;
                    
                    switch (ffrLevel) {
                        case 0:
                            MelonLogger.Msg("FFR: Level 0 (Off)");
                            break;
                        case 1:
                            MelonLogger.Msg("FFR: Level 1 (Low)");
                            break;
                        case 2:
                            MelonLogger.Msg("FFR: Level 2 (Medium)");
                            break;
                        case 3:
                            MelonLogger.Msg("FFR: Level 3 (High)");
                            break;
                        case 4:
                            MelonLogger.Msg("FFR: Level 4 (Very High)");
                            break;
                    }
                }

            } catch (System.Exception e) {
                MelonLogger.Warning("FFR error: " + e.Message);
            }
        }
    }
}