using MelonLoader;
using UnityEngine;

namespace QuestGraphicsSettings {
    public partial class Core : MelonMod {

        MelonPreferences_Category category;

        MelonPreferences_Entry<int> FPSEntry;
        MelonPreferences_Entry<float> RenderScaleEntry;
        MelonPreferences_Entry<float> RenderDistanceEntry;
        MelonPreferences_Entry<float> LODBiasEntry;        
        MelonPreferences_Entry<float> TextureStreamingBudgetEntry;
        MelonPreferences_Entry<bool> FogEntry;
        MelonPreferences_Entry<bool> TextureStreamingEntry;
        MelonPreferences_Entry<bool> DebugInfoEntry;
        MelonPreferences_Entry<bool> FFRAutoEntry;
        MelonPreferences_Entry<int> FFRLevelEntry;
        private Camera playerCamera;
        private GameObject fogObject;
        private bool PresetFFRAuto;
        private int PresetFFRLevel;
        private bool AutoPresetState;
        private string Preset;
        private float PresetRenderScale;
        private float PresetRenderDistance;
        private float PresetLODBias;        
        private float PresetTextureStreamingBudget;
        private bool PresetFog;
        private bool PresetTextureStreaming;
        private int PerformanceDrops;
        private float presetdelay = 5f;
        private float lastpresettime = 0f;
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