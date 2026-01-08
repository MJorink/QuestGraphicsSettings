using MelonLoader;
using UnityEngine;
using BoneLib;
using BoneLib.BoneMenu;

[assembly: MelonInfo(typeof(QuestGraphicsSettings.Core), "QuestGraphicsSettings", "1.0.0", "jorink", null)]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace QuestGraphicsSettings {
    public class Core : MelonMod {

        MelonPreferences_Category category;
        
        public override void OnInitializeMelon() {
            MelonPrefs();
            GraphicsMenu();
        }

        private void GraphicsMenu() {
            Page page = Page.Root.CreatePage("Quest Graphics Settings", Color.yellow);
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
	}
}