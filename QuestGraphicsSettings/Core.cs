using MelonLoader;
using UnityEngine;
using BoneLib;
using BoneLib.BoneMenu;
using Il2Cpp;

[assembly: MelonInfo(typeof(QuestGraphicsSettings.Core), "QuestGraphicsSettings", "1.0.0", "jorink", null)]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace QuestGraphicsSettings {
    public class Core : MelonMod {

        MelonPreferences_Category category;
        MelonPreferences_Entry<bool> FogEntry;

        public override void OnInitializeMelon() {
            MelonPrefs();
            GraphicsMenu();
        }

        private void GraphicsMenu() {
            Page page = Page.Root.CreatePage("Quest Graphics Settings", Color.yellow);
            page.CreateBool("Fog", Color.green, FogEntry.Value, (a) => {FogEntry.Value = a;});
        }

        private void MelonPrefs() {
            category = MelonPreferences.CreateCategory("QuestGraphicsSettings");
            FogEntry = category.CreateEntry<bool>("Fog", true);
            MelonPreferences.Save();
            category.SaveToFile();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            ToggleFog();
            //example = exampleEntry.Value;
        }

        private void ToggleFog()
		{
			foreach (VolumetricRendering item in UnityEngine.Object.FindObjectsOfType<VolumetricRendering>())
			{
				if (!FogEntry.Value)
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