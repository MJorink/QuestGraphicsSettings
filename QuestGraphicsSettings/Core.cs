using MelonLoader;
using MelonLoader.Preferences;
using BoneLib.BoneMenu;
using UnityEngine;
using Il2Cpp;

[assembly: MelonInfo(typeof(QuestGraphicsSettings.Core), "QuestGraphicsSettings", "1.0.0", "jorink")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace QuestGraphicsSettings {
    public class Core : MelonMod {

        MelonPreferences_Category category;

        private static bool FogState;
        
        public override void OnInitializeMelon() {
            MelonPrefs();
            GraphicsMenu();
        }

        private void GraphicsMenu() {
            Page page = Page.Root.CreatePage("Quest Graphics Settings", Color.yellow);
            page.CreateFunction("Toggle Fog", Color.green, () => { ToggleFog(); });
        }

        private void MelonPrefs() {
            category = MelonPreferences.CreateCategory("QuestGraphicsSettings");
            MelonPreferences.Save();
            category.SaveToFile();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            //example = exampleEntry.Value;
        }

        private static void ToggleFog()
        {
            FogState = !FogState;
            foreach (VolumetricRendering item in GameObject.FindObjectsOfType<VolumetricRendering>()) {
                if (!FogState)
                {
                    item.disable();
                }
                else
                {
                    item.enable();
                }
            }
        }
            
    }
}