using MelonLoader;
using BoneLib;
using BoneLib.BoneMenu;
using BoneLib.BoneMenu.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using Il2CppSLZ.Bonelab;
using Il2CppTMPro;
using jlib;

namespace questgraphicssettings
{
	public class QuestGraphicsSettings : MelonMod
	{
		public const string Version = "3.1.0";

		private MelonPreferences_Entry<float> renderScale;
		private MelonPreferences_Entry<float> lodBias;
		private MelonPreferences_Entry<int> ffrLevel;
		private MelonPreferences_Entry<bool> dynamicFFR;
		private MelonPreferences_Entry<bool> enableFarClipPlane;
		private MelonPreferences_Entry<float> farClipPlane;
		private MelonPreferences_Entry<bool> enableCulling;

		private Camera playerCamera;
		private GameObject menuButton;
		private bool needsApply;

		private ModPage menu; // OpenFromMenu() needs this too

		public override void OnInitializeMelon()
		{
			menu = JLib.Register("QuestGraphicsSettings", Color.yellow);

			renderScale = menu.Float("Render Scale", 1.0f, 0.05f, 0.50f, 2.0f, Color.yellow, _ => Apply());
			lodBias = menu.Float("LOD Bias", 1.25f, 0.05f, 0.50f, 2.0f, Color.yellow, _ => Apply());
			ffrLevel = menu.Int("FFR Level", 3, 1, 0, 3, Color.green, _ => Apply());
			dynamicFFR = menu.Bool("Dynamic FFR", false, Color.green, _ => Apply());

			var experimental = menu.SubPage("Experimental", Color.yellow);

			enableFarClipPlane = experimental.Bool("Enable farClipPlane", false, Color.cyan, _ => Apply());
			farClipPlane = experimental.Float("farClipPlane (Render Distance)", 100f, 5f, 5f, 200f, Color.green, _ => Apply());
			enableCulling = experimental.Bool("Enable Occlusion Culling", false, Color.cyan, _ => Apply());

			Hooking.OnLevelLoaded += OnLevelLoaded;
		}

		private void OnLevelLoaded(LevelInfo levelInfo)
		{
			playerCamera = UnityEngine.Object.FindObjectOfType<Camera>();
			needsApply = true;
		}

		public override void OnUpdate()
		{
			if (needsApply && !HelperMethods.IsLoading()) Apply();
			if (menuButton == null) CreateMenuButton();
		}

		private void Apply()
		{
			var asset = UniversalRenderPipeline.asset;
			if (playerCamera == null || asset == null) return;
			needsApply = false;

			asset.renderScale = renderScale.Value;
			QualitySettings.lodBias = lodBias.Value;
			Unity.XR.Oculus.Utils.foveatedRenderingLevel = ffrLevel.Value;
			Unity.XR.Oculus.Utils.useDynamicFoveatedRendering = dynamicFFR.Value;
			playerCamera.useOcclusionCulling = enableCulling.Value;
			playerCamera.farClipPlane = enableFarClipPlane.Value ? farClipPlane.Value : 1000f;
		}

		private void CreateMenuButton()
		{
			var uiRig = UIRig.Instance;
			if (uiRig == null) return;
			
			var panelView = uiRig.popUpMenu.preferencesPanelView;
			var gridOptions = panelView.transform.Find("page_OPTIONS/grid_Options");
			var controlButton = gridOptions.Find("button_Control").gameObject;

			menuButton = GameObject.Instantiate(controlButton, controlButton.transform.parent, false);
			menuButton.name = "button_Graphics";
			menuButton.transform.SetSiblingIndex(gridOptions.Find("button_Quit").GetSiblingIndex() - 1);

			var buttonText = menuButton.transform.Find("text_Control").GetComponent<TMP_Text>();
			buttonText.text = "Graphics";
			buttonText.gameObject.name = "text_Graphics";

			var button = menuButton.GetComponent<Button>();
			button.onClick = new Button.ButtonClickedEvent();
			button.onClick.AddListener((UnityEngine.Events.UnityAction)(() => OpenFromMenu(panelView)));
		}

		private void OpenFromMenu(PreferencesPanelView panelView)
		{
			var guiMenuObject = GUIMenu.Instance?.gameObject;

			for (int i = 0; guiMenuObject != null && i < panelView.pages.Length; i++)
			{
				if (panelView.pages[i] != guiMenuObject) continue;
				panelView.PAGESELECT(i);
				break;
			}

			Menu.OpenPage(menu.page);
		}
	}
}
