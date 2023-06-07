using KSP.UI.Screens;
using UnityEngine;

namespace HUDReplacer
{
	[KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
	internal class ToolbarManager : MonoBehaviour
	{
		private ApplicationLauncherButton ToolbarButton;
		private Texture OnTexture;
		private Texture OffTexture;
		private void Awake()
		{
			if (!SettingsManager.Instance.showDebugToolbar)
			{
				return;
			}
			OnTexture = GameDatabase.Instance.GetTexture("HUDReplacer/Assets/ToolbarOn", false);
			OffTexture = GameDatabase.Instance.GetTexture("HUDReplacer/Assets/ToolbarOff", false);
			if (!ToolbarButton)
			{
				ToolbarButton = ApplicationLauncher.Instance.AddModApplication(onToolbarToggleOn, onToolbarToggleOff, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, OffTexture);
			}
		}

		private void onToolbarToggleOn()
		{
			HUDReplacer.enableDebug = true;
			ToolbarButton.SetTexture(OnTexture);
			Debug.Log("HUDReplacer: Enabled debug mode.");
		}

		private void onToolbarToggleOff()
		{
			HUDReplacer.enableDebug = false;
			ToolbarButton.SetTexture(OffTexture);
			Debug.Log("HUDReplacer: Disabled debug mode.");
		}
	}
}
