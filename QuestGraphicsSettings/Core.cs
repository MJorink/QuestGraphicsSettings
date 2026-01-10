using MelonLoader;
using MelonLoader.Preferences;
using BoneLib.BoneMenu;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[assembly: MelonInfo(typeof(QuestGraphicsSettings.Core), "QuestGraphicsSettings", "1.0.0", "jorink")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace QuestGraphicsSettings {
    public class Core : MelonMod {

        MelonPreferences_Category category;
        MelonPreferences_Entry<bool> FogEntry;
        MelonPreferences_Entry<float> RenderScaleEntry;
        MelonPreferences_Entry<bool> TextureStreamingEntry;
        MelonPreferences_Entry<float> TextureStreamingBudgetEntry;

        private GameObject fogObject;
        
        public override void OnInitializeMelon() {
            MelonPrefs();
            GraphicsMenu();
        }

        private void GraphicsMenu() {
            Page page = Page.Root.CreatePage("Quest Graphics Settings", Color.yellow);
            page.CreateFunction("PRESS ME", Color.red, () => { Warning(); });
            page.CreateBool("Fog", Color.yellow, FogEntry.Value, (a) => { FogEntry.Value = a; });
            page.CreateFloat("Render Scale", Color.yellow, RenderScaleEntry.Value, 0.05f, 0.40f, 1.0f, (a) => { RenderScaleEntry.Value = a; });
            page.CreateBool("Texture Streaming", Color.yellow, TextureStreamingEntry.Value, (a) => { TextureStreamingEntry.Value = a; });
            page.CreateFloat("Texture Streaming Budget", Color.yellow, TextureStreamingBudgetEntry.Value, 16f, 16f, 3072f, (a) => { TextureStreamingBudgetEntry.Value = a; });
            page.CreateFunction("Apply Settings", Color.green, () => { ApplySettings(); });
        }

        private void MelonPrefs() {
            category = MelonPreferences.CreateCategory("QuestGraphicsSettings");
            FogEntry = category.CreateEntry("Fog Enabled", true);
            RenderScaleEntry = category.CreateEntry("Render Scale", 1.0f);
            TextureStreamingEntry = category.CreateEntry("Texture Streaming Enabled", true);
            TextureStreamingBudgetEntry = category.CreateEntry("Texture Streaming Budget", 3072f);
            MelonPreferences.Save();
            category.SaveToFile();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            ApplySettings();
        }

        private void Warning() {
             Menu.DisplayDialog(
            "WARNING",
            "Using high settings will cause lag or crashes. Texture Streaming should be kept on for most users. Also, don't forget to apply the settings after changing them!"
            );
        }

        private void ApplySettings() {
            SetFog();
            SetRenderScale();
            SetTextureStreaming();
            MelonPreferences.Save();
        }

        private void SetRenderScale() {
        UnityEngine.XR.XRSettings.renderViewportScale = RenderScaleEntry.Value;
        }

        private void SetTextureStreaming() {
            QualitySettings.streamingMipmapsActive = TextureStreamingEntry.Value;
            QualitySettings.streamingMipmapsMemoryBudget = TextureStreamingBudgetEntry.Value;
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
            
    }
}