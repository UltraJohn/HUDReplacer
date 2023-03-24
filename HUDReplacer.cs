using Smooth.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UrlDir;

namespace HUDReplacer
{
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class HUDReplacer : MonoBehaviour
	{
		private static Dictionary<string, string> images;
		private static string filePathConfig = "HUDReplacer";
		private static bool firstTimeRun = false;
		private static GameScenes[] allowedScenes = new GameScenes[] { GameScenes.MAINMENU, GameScenes.SETTINGS, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION };

		public void Awake()
        {
			if(!allowedScenes.Contains(HighLogic.LoadedScene))
			{
				return;
			}
			if (!firstTimeRun)
			{
				firstTimeRun = true;
				Debug.Log("HUDReplacer: Performing initialization.");
				GetImages();
			}
			Debug.Log("HUDReplacer: Running scene change. " + HighLogic.LoadedScene);
			if(images.Count > 0)
			{
				Debug.Log("HUDReplacer: Replacing textures...");
				ReplaceImages();
			}

			Debug.Log("HUDReplacer: Textures have been replaced!");
		}

		private void GetImages()
		{
			images = new Dictionary<string, string>();
			UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs(filePathConfig);
			if(configs.Length <= 0)
			{
				Debug.Log("HUDReplacer: No configs found.");
				return;
			}
			
			Debug.Log("HUDReplacer file paths found:");
			configs = configs.OrderByDescending(x => int.Parse(x.config.GetValue("priority"))).ToArray();
			foreach(UrlDir.UrlConfig configFile in configs)
			{
				string filePath = configFile.config.GetValue("filePath");
				int priority = int.Parse(configFile.config.GetValue("priority"));
				Debug.Log("HUDReplacer: path " + filePath + " - priority: "+priority);
				string[] files = Directory.GetFiles(filePath, "*.png");
				foreach (string text in files)
				{
					Debug.Log("HUDReplacer: Found file " + text);
					string filename = Path.GetFileNameWithoutExtension(text);
					if (!images.ContainsKey(filename))
					{
						images.Add(filename, text);
					}
					
				}
			}
		}
		private void ReplaceImages()
		{
			if (images.Count == 0) return;

			Texture2D[] tex_array = (Texture2D[])(object)Resources.FindObjectsOfTypeAll(typeof(Texture2D));

			List<string> toRemove = new List<string>();

			foreach (Texture2D tex in tex_array)
			{
				foreach (KeyValuePair<string, string> image in images)
				{
					if(image.Key == tex.name)
					{
						Debug.Log("HUDReplacer: Replacing texture " + image.Value);
						bool success = ImageConversion.LoadImage(tex, File.ReadAllBytes(image.Value));
						toRemove.Add(image.Key);
						continue;
					}
				}
				
				
			}

			if (toRemove.Count > 0)
			{
				foreach (string key in toRemove)
				{
					images.Remove(key);
				}
			}

		}

		
	}
}
