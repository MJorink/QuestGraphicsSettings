using MelonLoader;
using BoneLib.BoneMenu;
using BoneLib.Notifications;
using UnityEngine;

[assembly: MelonInfo(typeof(QuestGraphicsSettings.Core), "QuestGraphicsSettings", "1.2.0", "jorink")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace QuestGraphicsSettings {
    public partial class Core : MelonMod {

        public override void OnInitializeMelon() {
            SetupMelonPreferences();
            SetupBoneMenu();
            Preset = "Custom";
        }

        private void SetupBoneMenu() {
            Page defaultPage = Page.Root.CreatePage("QuestGraphicsSettings", Color.yellow);

            Page customPage = defaultPage.CreatePage("Settings (Custom)", Color.cyan);
            customPage.CreateFloat("Render Scale", Color.green, RenderScaleEntry.Value, 0.05f, 0.50f, 2.0f, (a) => { RenderScaleEntry.Value = a; });
            customPage.CreateFloat("Render Distance", Color.yellow, RenderDistanceEntry.Value, 5f, 5f, 150f, (a) => { RenderDistanceEntry.Value = a; });
            customPage.CreateFloat("LOD Bias", Color.yellow, LODBiasEntry.Value, 0.05f, 0.50f, 3.0f, (a) => { LODBiasEntry.Value = a; });
            customPage.CreateFloat("Texture Streaming Budget", Color.yellow, TextureStreamingBudgetEntry.Value, 32f, 32f, 3072f, (a) => { TextureStreamingBudgetEntry.Value = a; });
            customPage.CreateBool("Fog", Color.green, FogEntry.Value, (a) => { FogEntry.Value = a; });
            customPage.CreateBool("Auto FFR", Color.cyan, FFRAutoEntry.Value, (a) => { FFRAutoEntry.Value = a; ApplySettings(); });
            customPage.CreateInt("FFR Level (Manual)", Color.green, FFRLevelEntry.Value, 1, 0, 4, (a) => { FFRLevelEntry.Value = a; if (!FFRAutoEntry.Value) ApplySettings(); });

            customPage.CreateFunction("Apply Settings", Color.cyan, () => { ApplySettings(); });
            customPage.CreateFunction("Save Settings", Color.cyan, () => { MelonPreferences.Save(); });
            customPage.CreateFunction("Enable Custom Preset", Color.cyan, () => { CustomPreset(); ApplySettings(); });

            Page presetsPage = defaultPage.CreatePage("Presets", Color.blue);
            presetsPage.CreateFunction("No Preset (Custom)", Color.cyan, () => { CustomPreset(); ApplySettings(); });
            presetsPage.CreateFunction("Jorink's Preset", Color.magenta, () => { JorinksPreset(); ApplySettings(); });
            presetsPage.CreateFunction("Default", Color.blue, () => { DefaultPreset(); ApplySettings(); });
            presetsPage.CreateFunction("High", Color.red, () => { HighPreset(); ApplySettings(); });
            presetsPage.CreateFunction("Medium", Color.yellow, () => { MediumPreset(); ApplySettings(); });
            presetsPage.CreateFunction("Low", Color.green, () => { LowPreset(); ApplySettings(); });                        
            presetsPage.CreateFunction("Very Low", Color.green, () => { VeryLowPreset(); ApplySettings(); });
            presetsPage.CreateFunction("Show Current Preset", Color.cyan, () => { CurrentPreset();});

            Page autopresetPage = defaultPage.CreatePage("Auto Preset (WIP)", Color.magenta);
            autopresetPage.CreateFunction("Toggle Auto Preset", Color.cyan, () => { ToggleAutoPreset();});
            autopresetPage.CreateInt("Target FPS", Color.green, FPSEntry.Value, 10, 60, 120, (a) => { FPSEntry.Value = a; });
            autopresetPage.CreateFunction("Show Current Preset", Color.cyan, () => { CurrentPreset();});

            Page advancedPage = customPage.CreatePage("Advanced Settings", Color.red);
            advancedPage.CreateFunction("PRESS ME", Color.red, () => { AdvancedWarning(); });
            advancedPage.CreateBool("Texture Streaming (!)", Color.red, TextureStreamingEntry.Value, (a) => { TextureStreamingEntry.Value = a; });
            advancedPage.CreateBool("Debug Info", Color.magenta, DebugInfoEntry.Value, (a) => { DebugInfoEntry.Value = a; });            
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
            FFRAutoEntry = category.CreateEntry("FFR Auto", true);
            FFRLevelEntry = category.CreateEntry("FFR Level", 1);
            DebugInfoEntry = category.CreateEntry("Debug Info", false);
            MelonPreferences.Save();
            category.SaveToFile();
        }

        private void AdvancedWarning() {
             Menu.DisplayDialog(
            "WARNING",
            "These settings are experimental and minimally tested, and may cause bugs or crashes. Proceed with caution!"
            );
        }

        private void DebugInfo() {
            if (!DebugInfoEntry.Value) return;
            var notif = new Notification {
                Title = "Debug Info",
                Message = "Preset: " + Preset + "\n" +
                          "Render Scale: " + RenderScaleEntry.Value.ToString("F2") + "\n" +
                          "Render Distance: " + RenderDistanceEntry.Value.ToString("F2") + "\n" +
                          "LOD Bias: " + LODBiasEntry.Value.ToString("F2") + "\n" +
                          "Texture Streaming Budget: " + TextureStreamingBudgetEntry.Value.ToString("F2") + " MB\n" +
                          "Fog: " + (FogEntry.Value ? "Enabled" : "Disabled") + "\n" +
                          "FFR: " + (FFRAutoEntry.Value ? "Auto" : ("Level " + FFRLevelEntry.Value)),
                Type = NotificationType.Success,
                PopupLength = 2.0f,
                ShowTitleOnPopup = true
            };
            Notifier.Send(notif);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            if (AutoPresetState) {
                HighPreset();
                ApplySettings();
            }
            else {
                ApplySettings();
            }
        }

        private void ApplySettings() {
            SetTextureStreaming();
            SetRenderScale();
            SetRenderDistance();
            SetLODBias();
            SetFog();
            SetFFR();
            DebugInfo();
        }

        public override void OnUpdate() {
            base.OnUpdate();
            frameTimeSamples[frameIndex] = Time.unscaledDeltaTime;
            frameIndex = (frameIndex + 1) % frameTimeSamples.Length;
            AutoPreset();
        }
    }
}
