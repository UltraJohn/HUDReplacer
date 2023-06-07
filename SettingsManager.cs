using UnityEngine;

namespace HUDReplacer
{
	internal class SettingsManager
	{
		static SettingsManager instance;
		public bool showDebugToolbar;
		public static SettingsManager Instance
		{
			get {
				if (instance == null)
				{
					instance = new SettingsManager();
				}
				return instance;
			}
		}
		SettingsManager()
		{
			ConfigNode node = GameDatabase.Instance.GetConfigNode("HUDReplacer/HUDReplacerSettings");
			if(node == null)
			{
				Debug.LogError("HUDReplacer: Could not load settings. Settings.cfg may be missing.");
				return;
			}
			if (node.HasValue("showDebugToolbar"))
			{
				bool.TryParse(node.GetValue("showDebugToolbar"), out  showDebugToolbar);
			}
		}
	}
}
