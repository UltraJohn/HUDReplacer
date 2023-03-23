using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HUDReplacer
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class HUDReplacer : MonoBehaviour
    {

		private static Dictionary<string, string> images;
		private static string imagesPath;


		/// <summary>
		/// <c>Start</c> runs on each scene change.
		/// </summary>
		public void Start()
        {
			Debug.Log("HUDReplacer running scene change");
			if(images == null)
			{
				// First time run, generate the list of textures to replace, then replace.
				GetImages();
				ReplaceImages();
			}
			else if(images.Count > 0)
			{
				// If there are still more to replace on scene change, replace them. Don't run GetImages again.
				ReplaceImages();
			}
			else
			{
				// All done!
				Debug.Log("No more HUD to replace, all done!");
			}


		}
		/// <summary>
		/// <c>GetImages</c> loads the image files from the GameData/HUDReplacer/Images directory into an array.
		/// </summary>
		private void GetImages()
		{
			imagesPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\PluginData";
			images = new Dictionary<string, string>();
			string[] files = Directory.GetFiles(imagesPath, "*.png");
			foreach (string text in files)
			{
				string filename = Path.GetFileNameWithoutExtension(text);
				images.Add(filename, text);
			}
		}
		/// <summary>
		/// <c>ReplaceImages</c> finds and replaces textures in the current scene that matches any of the loaded images from <c>GetImages</c>.
		/// Finishes up by removing already replaced images from the array. As they don't refresh on scene change, there is no need to replace them again.
		/// </summary>
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
						ImageConversion.LoadImage(tex, File.ReadAllBytes(image.Value));
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
