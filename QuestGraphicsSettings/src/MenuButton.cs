using MelonLoader;
using BoneLib.BoneMenu;
using BoneLib.BoneMenu.UI;
using UnityEngine;
using UnityEngine.UI;
using Il2CppSLZ.Bonelab;
using Il2CppTMPro;

namespace QuestGraphicsSettings
{
	public partial class QuestGraphicsSettingsMod : MelonMod
	{
		private static GameObject menuButton;
		
		private static void CreateMenuButton()
		{
		    var uiRig = UIRig.Instance;
		    if (uiRig == null) return;

		    // Add the button
		    var panelView = uiRig.popUpMenu.preferencesPanelView;

		    var gridOptions = panelView.transform.Find("page_OPTIONS/grid_Options");
		    var controlButton = gridOptions.Find("button_Control").gameObject;

		    var graphicsButton = GameObject.Instantiate(controlButton, controlButton.transform.parent, false);
		    graphicsButton.name = "button_Graphics";

		    var quitButtonIndex = gridOptions.Find("button_Quit").GetSiblingIndex();
		    graphicsButton.transform.SetSiblingIndex(quitButtonIndex - 1);

		    // Modify the button
		    var buttonText = graphicsButton.transform.Find("text_Control").GetComponent<TMP_Text>();
		    buttonText.text = "Graphics";
		    buttonText.gameObject.name = "text_Graphics";

		    var buttonScript = graphicsButton.GetComponent<Button>();
		    buttonScript.onClick = new Button.ButtonClickedEvent();
		    
		    Button buttonComponent = graphicsButton.GetComponent<Button>();
		    buttonComponent.onClick.AddListener((UnityEngine.Events.UnityAction)(() => OpenFromMenu(panelView)));

		    menuButton = graphicsButton;
		}

		private static void OpenFromMenu(PreferencesPanelView panelView)
		{
			int menuPageIndex = FindMenuPageIndex(panelView);
			if (menuPageIndex >= 0)
			{
				panelView.PAGESELECT(menuPageIndex);
			}

			Menu.OpenPage(defaultPage);
		}

		private static int FindMenuPageIndex(PreferencesPanelView panelView)
		{
			var guiMenuObject = GUIMenu.Instance?.gameObject;
			if (guiMenuObject == null) return -1;

			for (int i = 0; i < panelView.pages.Length; i++)
			{
				if (panelView.pages[i] == guiMenuObject) return i;
			}

			return -1;
		}
	}
}
