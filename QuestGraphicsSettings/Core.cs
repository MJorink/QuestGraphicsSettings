using MelonLoader;
using MelonLoader.Preferences;
using BoneLib.BoneMenu;
using UnityEngine;

[assembly: MelonInfo(typeof(QuestGraphicsSettings.Core), "QuestGraphicsSettings", "1.0.0", "jorink")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace QuestGraphicsSettings {
    public class Core : MelonMod {

        MelonPreferences_Category category;
        MelonPreferences_Entry<bool> FogEntry;
        private GameObject fogObject;
        
        public override void OnInitializeMelon() {
            MelonPrefs();
            GraphicsMenu();
        }

        private void GraphicsMenu() {
            Page page = Page.Root.CreatePage("Quest Graphics Settings", Color.yellow);
            page.CreateBool("Fog", Color.gray, FogEntry.Value, (a) => { FogEntry.Value = a; });
            page.CreateFunction("Apply Settings", Color.cyan, () => { ApplySettings(); });
        }

        private void MelonPrefs() {
            category = MelonPreferences.CreateCategory("QuestGraphicsSettings");
            FogEntry = category.CreateEntry("Fog Enabled", true);
            MelonPreferences.Save();
            category.SaveToFile();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            base.OnSceneWasLoaded(buildIndex, sceneName);
        }

        private void ApplySettings() {
            ToggleFog();
        }

        private void ToggleFog()
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