using MelonLoader;
using BoneLib;
using BoneLib.BoneMenu;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Il2CppSLZ.Bonelab;

namespace QuestGraphicsSettings
{
	public partial class QuestGraphicsSettingsMod : MelonMod
	{
    	public const string Title = "QuestGraphicsSettings";
    	public const string Description = "A BoneLab mod that brings some graphics settings to Quest Standalone.";
    	public const string Version = "3.1.0";

    	private static BoneLib.BoneMenu.Page defaultPage;

		private static MelonPreferences_Entry<float> RenderScale;
		private static MelonPreferences_Entry<bool> enableCulling;
		private static MelonPreferences_Entry<float> farClipPlane;
		private static MelonPreferences_Entry<bool> enableFarClipPlane;
		private static MelonPreferences_Entry<float> LODBias;
		private static MelonPreferences_Entry<int> FFR;
		private static MelonPreferences_Entry<bool> dynamicFFR;

		private static UniversalRenderPipelineAsset asset;
		private static Camera playerCamera;

		private static UIRig uiRig;

		private static bool needsApply = false;

		public override void OnInitializeMelon()
		{
			SetupMelonPreferences();
			SetupBoneMenu();
			SetupHooks();
		}

		private void SetupMelonPreferences()
		{
			var category = MelonPreferences.CreateCategory("QuestGraphicsSettings");

            RenderScale = category.CreateEntry("Render Scale", 1.0f);
            enableCulling = category.CreateEntry("Enable Culling", false);
            enableFarClipPlane = category.CreateEntry("Enable farClipPlane", false);
            farClipPlane = category.CreateEntry("farClipPlane (Render Distance)", 100f);
            LODBias = category.CreateEntry("LOD Bias", 1.25f);
            FFR = category.CreateEntry("FFR Level", 3);
            dynamicFFR = category.CreateEntry("Dynamic FFR", false);

            MelonPreferences.Save();
		}

		private void SetupBoneMenu()
		{
			defaultPage = BoneLib.BoneMenu.Page.Root.CreatePage("Jorink", Color.red).CreatePage("QuestGraphicsSettings", Color.yellow);

            defaultPage.CreateFloat("Render Scale", Color.yellow, RenderScale.Value, 0.05f, 0.50f, 2.0f, (a) => { RenderScale.Value = a; SetRenderScale(); });
            defaultPage.CreateFloat("LOD Bias", Color.yellow, LODBias.Value, 0.05f, 0.50f, 2.0f, (a) => { LODBias.Value = a; SetLODBias(); });
            defaultPage.CreateInt("FFR Level", Color.green, FFR.Value, 1, 0, 3, (a) => { FFR.Value = a; SetFFR(); });
            defaultPage.CreateBool("Dynamic FFR", Color.green, dynamicFFR.Value, (a) => { dynamicFFR.Value = a; SetDynamicFFR(); });
            defaultPage.CreateFunction("Save Settings", Color.cyan, () => { MelonPreferences.Save(); });

            var experimentalPage = defaultPage.CreatePage("Experimental", Color.yellow);
            
            experimentalPage.CreateBool("Enable farClipPlane", Color.cyan, enableFarClipPlane.Value, (a) => { enableFarClipPlane.Value = a; SetFarClipPlane(); });
            experimentalPage.CreateFloat("farClipPlane (Render Distance)", Color.green, farClipPlane.Value, 5f, 5f, 200f, (a) => { farClipPlane.Value = a; SetFarClipPlane(); });
            experimentalPage.CreateBool("Enable Occlusion Culling", Color.cyan, enableCulling.Value, (a) => { enableCulling.Value = a; SetOcclusionCulling(); });
		}

		private static void SetupHooks()
		{
			Hooking.OnLevelLoaded += OnLevelLoaded;
		}

		private static void OnLevelLoaded(LevelInfo levelInfo)
		{
			playerCamera = UnityEngine.Object.FindObjectOfType<Camera>();
			asset = UniversalRenderPipeline.asset;
			uiRig = Player.UIRig;
			needsApply = true;
		}

		public override void OnUpdate()
		{
			if (!isModAllowed()) return;
			needsApply = false;
			SetRenderScale();
			SetOcclusionCulling();
			SetFarClipPlane();
			SetLODBias();
			SetFFR();
			
			if (menuButton != null) return;
			CreateMenuButton();
		}

		private static bool isModAllowed()
		{
			if (!needsApply || playerCamera == null || asset == null || uiRig == null || BoneLib.HelperMethods.IsLoading()) return false;
			return true;
		}

		private static void SetRenderScale() => asset.renderScale = RenderScale.Value;
		private static void SetLODBias() => QualitySettings.lodBias = LODBias.Value;
		private static void SetOcclusionCulling() => playerCamera.useOcclusionCulling = enableCulling.Value;
		private static void SetFFR() => Unity.XR.Oculus.Utils.foveatedRenderingLevel = FFR.Value;
		private static void SetDynamicFFR() => Unity.XR.Oculus.Utils.useDynamicFoveatedRendering = dynamicFFR.Value;

		private static void SetFarClipPlane()
		{
			if (enableFarClipPlane.Value)
			{
				playerCamera.farClipPlane = farClipPlane.Value;
				return;
			}
			
			playerCamera.farClipPlane = 1000f;
		}
	}
}
