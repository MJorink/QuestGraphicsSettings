using MelonLoader;
using BoneLib.Notifications;
using UnityEngine;

namespace QuestGraphicsSettings {
    public partial class Core {

        private void VeryLowPreset() {
            Preset = "VeryLow";
            PresetFog = false;
            PresetRenderScale = 0.5f;
            PresetTextureStreaming = true;
            PresetTextureStreamingBudget = 128f;
            PresetLODBias = 0.5f;
            PresetRenderDistance = 50f;
            PresetFFRAuto = false;
            PresetFFRLevel = 3;
        }

        private void LowPreset() {
            Preset = "Low";
            PresetFog = false;
            PresetRenderScale = 0.85f;
            PresetTextureStreaming = true;
            PresetTextureStreamingBudget = 128f;
            PresetLODBias = 0.7f;
            PresetRenderDistance = 70f;
            PresetFFRAuto = false;
            PresetFFRLevel = 3;
        }

        private void MediumPreset() {
            Preset = "Medium";
            PresetFog = false;
            PresetRenderScale = 1.25f;
            PresetTextureStreaming = true;
            PresetTextureStreamingBudget = 256f;
            PresetLODBias = 0.85f;
            PresetRenderDistance = 85f;
            PresetFFRAuto = false;
            PresetFFRLevel = 3;
        }

        private void HighPreset() {
            Preset = "High";
            PresetFog = false;
            PresetRenderScale = 1.5f;
            PresetTextureStreaming = true;
            PresetTextureStreamingBudget = 512f;
            PresetLODBias = 1.25f;
            PresetRenderDistance = 100f;
            PresetFFRAuto = false;
            PresetFFRLevel = 3;
        }
        
        private void JorinksPreset() {
            Preset = "Jorink";
            PresetFog = false;
            PresetRenderScale = 1.0f;
            PresetTextureStreaming = true;
            PresetTextureStreamingBudget = 256f;
            PresetLODBias = 0.85f;
            PresetRenderDistance = 85f;
            PresetFFRAuto = false;
            PresetFFRLevel = 3;
        }     

        private void DefaultPreset() {
            Preset = "Default";
            PresetFog = true;
            PresetRenderScale = 1.0f;
            PresetTextureStreaming = true;
            PresetTextureStreamingBudget = 512f;
            PresetLODBias = 1f;
            PresetRenderDistance = 90f;
            PresetFFRAuto = false;
            PresetFFRLevel = 3;
        }  

        private void CustomPreset() {
            Preset = "Custom";
        } 

        private void CurrentPreset() {
            var notif = new Notification {
                Title = "Current Preset",
                Message = Preset,
                Type = NotificationType.Success,
                PopupLength = 1.25f,
                ShowTitleOnPopup = true
            };
            Notifier.Send(notif);
        }

        private void ToggleAutoPreset() {
            AutoPresetState = !AutoPresetState;
            if (AutoPresetState) {
                HighPreset();
                ApplySettings();
            }

            var notif = new Notification {
                Title = "Auto Preset",
                Message = AutoPresetState ? "Enabled" : "Disabled",
                Type = AutoPresetState ? NotificationType.Success : NotificationType.Warning,
                PopupLength = 1.25f,
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
                else if (Preset == "High") {
                    MediumPreset();
                    ApplySettings();
                }
                else {
                    HighPreset();
                    ApplySettings();
                }
            }
            }}        
    }
}