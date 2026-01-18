using MelonLoader;
using UnityEngine;

namespace QuestGraphicsSettings {
    public partial class Core : MelonMod {

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

        // FFR Entries
        MelonPreferences_Entry<bool> FFRAutoEntry;
        MelonPreferences_Entry<int> FFRLevelEntry;

        // FFR Variables
        private bool PresetFFRAuto;
        private int PresetFFRLevel;

        // Other Variables
        private Camera playerCamera;
        private GameObject fogObject;

        private float presetdelay = 5f;
        private float lastpresettime = 0f;

        // FPS tracking
        private float[] frameTimeSamples = new float[30];
        private int frameIndex = 0;

        private float GetAverageFPS() {
            float total = 0f;
            for (int i = 0; i < frameTimeSamples.Length; i++) {
                total += frameTimeSamples[i];
            }
            float avgDeltaTime = total / frameTimeSamples.Length;
            return avgDeltaTime > 0 ? 1f / avgDeltaTime : 0f;
        }
    }
}