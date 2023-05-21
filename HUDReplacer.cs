﻿using Cursors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HUDReplacer
{

	[KSPAddon(KSPAddon.Startup.MainMenu, false)]
	public class HUDReplacerMainMenu : HUDReplacer
	{

	}
	[KSPAddon(KSPAddon.Startup.FlightEditorAndKSC, false)]
	public class HUDReplacerFEKSC : HUDReplacer
	{

	}
	[KSPAddon(KSPAddon.Startup.TrackingStation, false)]
	public class HUDReplacerTrackingStation : HUDReplacer
	{

	}
	[KSPAddon(KSPAddon.Startup.Settings, false)]
	public class HUDReplacerSettings : HUDReplacer
	{

	}
	public partial class HUDReplacer : MonoBehaviour
	{
		private static Dictionary<string, string> images;
		private static string filePathConfig = "HUDReplacer";
		private TextureCursor[] cursors;
		public void Awake()
        {

			Debug.Log("HUDReplacer: Running scene change. " + HighLogic.LoadedScene);
			if (images == null)
			{
				GetImages();
			}
			if (images.Count > 0)
			{
				Debug.Log("HUDReplacer: Replacing textures...");
				ReplaceImages();
				Debug.Log("HUDReplacer: Textures have been replaced!");
			}
		}

#if DEBUG
		public void Update()
		{
			if(Input.GetKeyUp(KeyCode.E))
			{
				Debug.Log("HUDReplacer: Dumping list of loaded texture2D objects...");
				Texture2D[] tex_array = (Texture2D[])(object)Resources.FindObjectsOfTypeAll(typeof(Texture2D));
				foreach(Texture2D tex in tex_array)
				{
					Debug.Log(tex.name + " - WxH=" + tex.width + "x" + tex.height);
				}
				Debug.Log("HUDReplacer: Dumping finished.");
			}
			if (Input.GetKeyUp(KeyCode.Q))
			{
				GetImages();
				ReplaceImages();
				Debug.Log("HUDReplacer: Refreshed.");
			}
			if (Input.GetKeyUp(KeyCode.D))
			{

				PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
				eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

				List<RaycastResult> results = new List<RaycastResult>();
				EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
				
				Debug.Log("HUDReplacer: [][][][][][][][][][][][][][][][][]");
				foreach(RaycastResult result in results)
				{
					try
					{
						Image img = result.gameObject.GetComponent<Image>();
						//Debug.Log("HUDReplacer: ------");
						Debug.Log("Image.mainTexture.name: " + img.mainTexture.name + " - WxH=" + img.mainTexture.width + "x" + img.mainTexture.height);
						Debug.Log("Image.sprite.texture.name: " + img.sprite.texture.name + " - WxH=" + img.sprite.texture.width + "x" + img.sprite.texture.height);
						Debug.Log("HUDReplacer: ------");
					}catch(Exception e)
					{
						Debug.Log(e.ToString());
					}
				}
			}
			if (Input.GetKeyUp(KeyCode.G))
			{

				PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
				eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

				List<RaycastResult> results = new List<RaycastResult>();
				EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

				Debug.Log("HUDReplacer: [][][][][][][][][][][][][][][][][]");
				foreach (RaycastResult result in results)
				{
					try
					{
						Debug.Log("GameObject '" + result.gameObject.name + "' contains these components:");
						string list = "";
						foreach(Component comp in result.gameObject.GetComponents(typeof(Component))){
							list+= comp.name+" & ";
						}
						list = list.TrimEnd(' ').TrimEnd('&');
						Debug.Log(list);
					}
					catch (Exception e)
					{
						Debug.Log(e.ToString());
					}
				}
			}
		}
#endif
		

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
				//string[] files = Directory.GetFiles(filePath, "*.png");
				string[] files = Directory.GetFiles(KSPUtil.ApplicationRootPath + filePath, "*.png");
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

			foreach (Texture2D tex in tex_array)
			{
				foreach (KeyValuePair<string, string> image in images)
				{
					string key_stripped = image.Key;
					
					if (image.Value.Contains("#"))
					{
						// Some textures have multiple variants in varying sizes. We don't want to overwrite a texture with the wrong dimensions, as it will not render correctly.
						// For these special cases, we save the width and height in the filename, appended by a # to tell the program this is a multi-texture.
						key_stripped = image.Key.Substring(0, image.Key.IndexOf("#", StringComparison.Ordinal));
					}
					if(key_stripped == tex.name)
					{
						if(key_stripped == "basicNeutral")
						{
							if(cursors == null)
							{
								cursors = new TextureCursor[3];
							}
							TextureCursor tc = CreateCursor(image.Value);
							cursors[0] = tc;
							continue;
						}
						if (key_stripped == "basicElectricLime")
						{
							if (cursors == null)
							{
								cursors = new TextureCursor[3];
							}
							TextureCursor tc = CreateCursor(image.Value);
							cursors[1] = tc;
							continue;
						}
						if (key_stripped == "basicDisabled")
						{
							if (cursors == null)
							{
								cursors = new TextureCursor[3];
							}
							TextureCursor tc = CreateCursor(image.Value);
							cursors[2] = tc;
							continue;
						}
						if (key_stripped != image.Key)
						{
							// Special case texture
							string size = image.Key.Substring(image.Key.LastIndexOf("#")+1);
							int width = int.Parse(size.Substring(0, size.IndexOf("x")));
							int height = int.Parse(size.Substring(size.IndexOf("x")+1));
							if(tex.width == width && tex.height == height)
							{
								//Debug.Log("HUDReplacer: Replacing texture " + image.Value);
								ImageConversion.LoadImage(tex, File.ReadAllBytes(image.Value));
								continue;
							}
						}
						else
						{
							// Regular texture
							//Debug.Log("HUDReplacer: Replacing texture " + image.Value);
							ImageConversion.LoadImage(tex, File.ReadAllBytes(image.Value));
							continue;
						}
					}
				}
			}

			if(cursors != null)
			{
				CursorController.Instance.AddCursor("HUDReplacerCursor", cursors[0], cursors[1], cursors[2]);
				CursorController.Instance.ChangeCursor("HUDReplacerCursor");
				Debug.Log("HUDReplacer: Changed Cursor!");
			}

		}

		private TextureCursor CreateCursor(string value)
		{
			Texture2D cursor = new Texture2D(2, 2);
			cursor.LoadImage(File.ReadAllBytes(value));
			//Cursor.SetCursor(cursor, new Vector2(6,0), CursorMode.ForceSoftware);
			TextureCursor tc = new TextureCursor();
			tc.texture = cursor;
			tc.hotspot = new Vector2(6, 0);
			return tc;
		}
	}
}
