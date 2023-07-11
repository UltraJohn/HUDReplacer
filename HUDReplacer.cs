using Cursors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
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
		internal static HUDReplacer instance;
		internal static bool enableDebug = false;
		private static Dictionary<string, string> images;
		private static string filePathConfig = "HUDReplacer";
		private static string colorPathConfig = "HUDReplacerRecolor";
		private TextureCursor[] cursors;
		public void Awake()
        {
			instance = this;
			Debug.Log("HUDReplacer: Running scene change. " + HighLogic.LoadedScene);
			if (images == null)
			{
				GetTextures();
			}
			if (images.Count > 0)
			{
				Debug.Log("HUDReplacer: Replacing textures...");
				ReplaceTextures();
				Debug.Log("HUDReplacer: Textures have been replaced!");
			}
			LoadHUDColors();
		}

		public void Update()
		{
			if(enableDebug)
			{
				if (Input.GetKeyUp(KeyCode.E))
				{
					Debug.Log("HUDReplacer: Dumping list of loaded texture2D objects...");
					Texture2D[] tex_array = (Texture2D[])(object)Resources.FindObjectsOfTypeAll(typeof(Texture2D));
					foreach (Texture2D tex in tex_array)
					{
						Debug.Log(tex.name + " - WxH=" + tex.width + "x" + tex.height);
					}
					Debug.Log("HUDReplacer: Dumping finished.");
				}
				if (Input.GetKeyUp(KeyCode.Q))
				{
					GetTextures();
					ReplaceTextures();
					LoadHUDColors();
					Debug.Log("HUDReplacer: Refreshed.");
				}
				if (Input.GetKeyUp(KeyCode.D))
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
							Image img = result.gameObject.GetComponent<Image>();
							//Debug.Log("HUDReplacer: ------");
							Debug.Log("Image.mainTexture.name: " + img.mainTexture.name + " - WxH=" + img.mainTexture.width + "x" + img.mainTexture.height);
							Debug.Log("Image.sprite.texture.name: " + img.sprite.texture.name + " - WxH=" + img.sprite.texture.width + "x" + img.sprite.texture.height);
							Debug.Log("HUDReplacer: ------");
							if(img.mainTexture.name == "app_divider_pulldown_header" || img.mainTexture.name == "app_divider_pulldown_header_over")
							{
								//result.gameObject.GetComponent<CanvasRenderer>().SetColor(Color.red);
							}
							//Texture2D tex = (Texture2D)img.mainTexture;
							//DumpTexture(tex);
						}
						catch (Exception e)
						{
							Debug.Log(e.ToString());
						}
					}
				}
			}

		}


		

		private void GetTextures()
		{
			images = new Dictionary<string, string>();
			UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs(filePathConfig);
			if(configs.Length <= 0)
			{
				Debug.Log("HUDReplacer: No texture configs found.");
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
		internal void ReplaceTextures()
		{
			Texture2D[] tex_array = (Texture2D[])(object)Resources.FindObjectsOfTypeAll(typeof(Texture2D));
			ReplaceTextures(tex_array);
		}
		internal void ReplaceTextures(Texture2D[] tex_array)
		{
			if (images.Count == 0) return;

			string[] cursor_names = new string[] { "basicNeutral", "basicElectricLime", "basicDisabled" };

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
						// For the mouse cursor
						if (cursor_names.Contains(key_stripped))
						{
							if (cursors == null)
							{
								cursors = new TextureCursor[3];
							}
							cursors[cursor_names.IndexOf(key_stripped)] = CreateCursor(image.Value);
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
			// Need to wait a small amount of time after scene load before you can set the cursor.
			this.Invoke(SetCursor, 1f);
		}

		internal void LoadHUDColors()
		{
			UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs(colorPathConfig);
			if (configs.Length <= 0)
			{
				return;
			}
			configs = configs.OrderByDescending(x => int.Parse(x.config.GetValue("priority"))).ToArray();
			List<string> colorsSet = new List<string>();
			foreach (UrlDir.UrlConfig configFile in configs)
			{
				int priority = int.Parse(configFile.config.GetValue("priority"));

				string TumblerColorPositive = "tumblerColorPositive";
				if (configFile.config.HasValue(TumblerColorPositive))
				{
					if (!colorsSet.Contains(TumblerColorPositive))
					{
						colorsSet.Add(TumblerColorPositive);
						Color color = configFile.config.GetValue(TumblerColorPositive).ToRGBA();
						HarmonyPatches.TumblerColorReplacePositive = true;
						HarmonyPatches.TumblerColorPositive = color;
					}
				}
				string TumblerColorNegative = "tumblerColorNegative";
				if (configFile.config.HasValue(TumblerColorNegative))
				{
					if (!colorsSet.Contains(TumblerColorNegative))
					{
						colorsSet.Add(TumblerColorNegative);
						Color color = configFile.config.GetValue(TumblerColorNegative).ToRGBA();
						HarmonyPatches.TumblerColorReplaceNegative = true;
						HarmonyPatches.TumblerColorNegative = color;
					}
				}

				string PAWTitleBar = "PAWTitleBar";
				if (configFile.config.HasValue(PAWTitleBar))
				{
					if (!colorsSet.Contains(PAWTitleBar))
					{
						colorsSet.Add(PAWTitleBar);
						HarmonyPatches.PAWTitleBar_color = configFile.config.GetValue(PAWTitleBar).ToRGBA();
						HarmonyPatches.PAWTitleBar_replace = true;
					}
				}

				string PAWBlueButton = "PAWBlueButton";
				if (configFile.config.HasValue(PAWBlueButton))
				{
					if (!colorsSet.Contains(PAWBlueButton))
					{
						colorsSet.Add(PAWBlueButton);
						HarmonyPatches.PAWBlueButton_replace = true;
						HarmonyPatches.PAWBlueButton_color = configFile.config.GetValue(PAWBlueButton).ToRGBA();
					}
				}

				string PAWBlueButtonToggle = "PAWBlueButtonToggle";
				if (configFile.config.HasValue(PAWBlueButtonToggle))
				{
					if (!colorsSet.Contains(PAWBlueButtonToggle))
					{
						colorsSet.Add(PAWBlueButtonToggle);
						HarmonyPatches.PAWBlueButtonToggle_replace = true;
						HarmonyPatches.PAWBlueButtonToggle_color = configFile.config.GetValue(PAWBlueButtonToggle).ToRGBA();
					}
				}

				string PAWVariantSelectorNext = "PAWVariantSelectorNext";
				if (configFile.config.HasValue(PAWVariantSelectorNext))
				{
					if (!colorsSet.Contains(PAWVariantSelectorNext))
					{
						colorsSet.Add(PAWVariantSelectorNext);
						HarmonyPatches.PAWVariantSelectorNext_replace = true;
						HarmonyPatches.PAWVariantSelectorNext_color = configFile.config.GetValue(PAWVariantSelectorNext).ToRGBA();
					}
				}

				string PAWVariantSelectorPrevious = "PAWVariantSelectorPrevious";
				if (configFile.config.HasValue(PAWVariantSelectorPrevious))
				{
					if (!colorsSet.Contains(PAWVariantSelectorPrevious))
					{
						colorsSet.Add(PAWVariantSelectorPrevious);
						HarmonyPatches.PAWVariantSelectorPrevious_replace = true;
						HarmonyPatches.PAWVariantSelectorPrevious_color = configFile.config.GetValue(PAWVariantSelectorPrevious).ToRGBA();
					}
				}

				string PAWResourcePriorityIncrease = "PAWResourcePriorityIncrease";
				if (configFile.config.HasValue(PAWResourcePriorityIncrease))
				{
					if (!colorsSet.Contains(PAWResourcePriorityIncrease))
					{
						colorsSet.Add(PAWResourcePriorityIncrease);
						HarmonyPatches.PAWResourcePriorityIncrease_replace = true;
						HarmonyPatches.PAWResourcePriorityIncrease_color = configFile.config.GetValue(PAWResourcePriorityIncrease).ToRGBA();
					}
				}

				string PAWResourcePriorityDecrease = "PAWResourcePriorityDecrease";
				if (configFile.config.HasValue(PAWResourcePriorityDecrease))
				{
					if (!colorsSet.Contains(PAWResourcePriorityDecrease))
					{
						colorsSet.Add(PAWResourcePriorityDecrease);
						HarmonyPatches.PAWResourcePriorityDecrease_replace = true;
						HarmonyPatches.PAWResourcePriorityDecrease_color = configFile.config.GetValue(PAWResourcePriorityDecrease).ToRGBA();
					}
				}

				string PAWResourcePriorityReset = "PAWResourcePriorityReset";
				if (configFile.config.HasValue(PAWResourcePriorityReset))
				{
					if (!colorsSet.Contains(PAWResourcePriorityReset))
					{
						colorsSet.Add(PAWResourcePriorityReset);
						HarmonyPatches.PAWResourcePriorityReset_replace = true;
						HarmonyPatches.PAWResourcePriorityReset_color = configFile.config.GetValue(PAWResourcePriorityReset).ToRGBA();
					}
				}

				string KALTitleBar = "KALTitleBar";
				if (configFile.config.HasValue(KALTitleBar))
				{
					if (!colorsSet.Contains(KALTitleBar))
					{
						colorsSet.Add(KALTitleBar);
						HarmonyPatches.KALTitleBar_color = configFile.config.GetValue(KALTitleBar).ToRGBA();
						HarmonyPatches.KALTitleBar_replace = true;
					}
				}

				string gaugeNeedleYawPitchRoll = "gaugeNeedleYawPitchRoll";
				if (configFile.config.HasValue(gaugeNeedleYawPitchRoll))
				{
					if (!colorsSet.Contains(gaugeNeedleYawPitchRoll))
					{
						colorsSet.Add(gaugeNeedleYawPitchRoll);
						HarmonyPatches.gaugeNeedleYawPitchRollColor = configFile.config.GetValue(gaugeNeedleYawPitchRoll).ToRGBA();
						HarmonyPatches.gaugeNeedleYawPitchRollColor_replace = true;
					}
				}

				string gaugeNeedleYawPitchRollPrecision = "gaugeNeedleYawPitchRollPrecision";
				if (configFile.config.HasValue(gaugeNeedleYawPitchRollPrecision))
				{
					if (!colorsSet.Contains(gaugeNeedleYawPitchRollPrecision))
					{
						colorsSet.Add(gaugeNeedleYawPitchRollPrecision);
						HarmonyPatches.gaugeNeedleYawPitchRollPrecisionColor = configFile.config.GetValue(gaugeNeedleYawPitchRollPrecision).ToRGBA();
						HarmonyPatches.gaugeNeedleYawPitchRollPrecisionColor_replace = true;
					}
				}

				string METDisplayColorRed = "METDisplayColorRed";
				if (configFile.config.HasValue(METDisplayColorRed))
				{
					if (!colorsSet.Contains(METDisplayColorRed))
					{
						colorsSet.Add(METDisplayColorRed);
						HarmonyPatches.METDisplayColorRed = configFile.config.GetValue(METDisplayColorRed).ToRGBA();
					}
				}
				string METDisplayColorYellow = "METDisplayColorYellow";
				if (configFile.config.HasValue(METDisplayColorYellow))
				{
					if (!colorsSet.Contains(METDisplayColorYellow))
					{
						colorsSet.Add(METDisplayColorYellow);
						HarmonyPatches.METDisplayColorYellow = configFile.config.GetValue(METDisplayColorYellow).ToRGBA();
					}
				}
				string METDisplayColorGreen = "METDisplayColorGreen";
				if (configFile.config.HasValue(METDisplayColorGreen))
				{
					if (!colorsSet.Contains(METDisplayColorGreen))
					{
						colorsSet.Add(METDisplayColorGreen);
						HarmonyPatches.METDisplayColorGreen = configFile.config.GetValue(METDisplayColorGreen).ToRGBA();
					}
				}
				string SpeedDisplayColorTextReplace = "speedDisplayColorText";
				if (configFile.config.HasValue(SpeedDisplayColorTextReplace))
				{
					if (!colorsSet.Contains(SpeedDisplayColorTextReplace))
					{
						colorsSet.Add(SpeedDisplayColorTextReplace);
						HarmonyPatches.SpeedDisplayColorTextReplace = true;
						HarmonyPatches.SpeedDisplayColorText = configFile.config.GetValue(SpeedDisplayColorTextReplace).ToRGBA();
					}
				}
				string SpeedDisplayColorSpeedReplace = "speedDisplayColorSpeed";
				if (configFile.config.HasValue(SpeedDisplayColorSpeedReplace))
				{
					if (!colorsSet.Contains(SpeedDisplayColorSpeedReplace))
					{
						colorsSet.Add(SpeedDisplayColorSpeedReplace);
						HarmonyPatches.SpeedDisplayColorSpeedReplace = true;
						HarmonyPatches.SpeedDisplayColorSpeed = configFile.config.GetValue(SpeedDisplayColorSpeedReplace).ToRGBA();
					}
				}
				string NavBallHeadingColor = "navBallHeadingColor";
				if (configFile.config.HasValue(NavBallHeadingColor))
				{
					if (!colorsSet.Contains(NavBallHeadingColor))
					{
						colorsSet.Add(NavBallHeadingColor);
						HarmonyPatches.NavBallHeadingColorReplace = true;
						HarmonyPatches.NavBallHeadingColor = configFile.config.GetValue(NavBallHeadingColor).ToRGBA();
					}
				}
				string StageTotalDeltaVColor = "stageTotalDeltaVColor";
				if (configFile.config.HasValue(StageTotalDeltaVColor))
				{
					if (!colorsSet.Contains(StageTotalDeltaVColor))
					{
						colorsSet.Add(StageTotalDeltaVColor);
						HarmonyPatches.StageTotalDeltaVColorReplace = true;
						HarmonyPatches.StageTotalDeltaVColor = configFile.config.GetValue(StageTotalDeltaVColor).ToRGBA();
					}
				}
				string StageGroupDeltaVTextColor = "stageGroupDeltaVTextColor";
				if (configFile.config.HasValue(StageGroupDeltaVTextColor))
				{
					if (!colorsSet.Contains(StageGroupDeltaVTextColor))
					{
						colorsSet.Add(StageGroupDeltaVTextColor);
						HarmonyPatches.StageGroupDeltaVTextColorReplace = true;
						HarmonyPatches.StageGroupDeltaVTextColor = configFile.config.GetValue(StageGroupDeltaVTextColor).ToRGBA();
					}
				}
				string StageGroupDeltaVNumberColor = "stageGroupDeltaVNumberColor";
				if (configFile.config.HasValue(StageGroupDeltaVNumberColor))
				{
					if (!colorsSet.Contains(StageGroupDeltaVNumberColor))
					{
						colorsSet.Add(StageGroupDeltaVNumberColor);
						HarmonyPatches.StageGroupDeltaVNumberColorReplace = true;
						HarmonyPatches.StageGroupDeltaVNumberColor = configFile.config.GetValue(StageGroupDeltaVNumberColor).ToRGBA();
					}
				}

				string NavBallCursor = "navballCursor";
				if (configFile.config.HasValue(NavBallCursor))
				{
					if (!colorsSet.Contains(NavBallCursor))
					{
						colorsSet.Add(NavBallCursor);
						GameObject go = GameObject.Find("_UIMaster/MainCanvas/Flight/NavballFrame/IVAEVACollapseGroup/NavBallCursor");
						if (go != null)
						{
							Image img = go.GetComponentInChildren<Image>();
							if (img != null)
							{
								img.color = configFile.config.GetValue(NavBallCursor).ToRGBA();
							}
						}
					}
				}

				string VerticalSpeedGaugeNeedle = "verticalSpeedGaugeNeedleColor";
				if (configFile.config.HasValue(VerticalSpeedGaugeNeedle))
				{
					if (!colorsSet.Contains(VerticalSpeedGaugeNeedle))
					{
						colorsSet.Add(VerticalSpeedGaugeNeedle);
						HarmonyPatches.VerticalSpeedGaugeNeedleColorReplace = true;
						HarmonyPatches.VerticalSpeedGaugeNeedleColor = configFile.config.GetValue(VerticalSpeedGaugeNeedle).ToRGBA();
					}
				}

			}
		}
		/*
		internal static void LoadTumblerColors()
		{
			UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs(colorPathConfig);
			if (configs.Length <= 0)
			{
				return;
			}
			configs = configs.OrderByDescending(x => int.Parse(x.config.GetValue("priority"))).ToArray();
			List<string> colorsSet = new List<string>();
			foreach (UrlDir.UrlConfig configFile in configs)
			{
				int priority = int.Parse(configFile.config.GetValue("priority"));


				string TumblerColorPositive = "tumblerColorPositive";
				if (configFile.config.HasValue(TumblerColorPositive))
				{
					if (!colorsSet.Contains(TumblerColorPositive))
					{
						colorsSet.Add(TumblerColorPositive);
						Color color = configFile.config.GetValue(TumblerColorPositive).ToRGBA();
						HarmonyPatches.TumblerColorReplacePositive = true;
						HarmonyPatches.TumblerColorPositive = color;
					}
				}
				string TumblerColorNegative = "tumblerColorNegative";
				if (configFile.config.HasValue(TumblerColorNegative))
				{
					if (!colorsSet.Contains(TumblerColorNegative))
					{
						colorsSet.Add(TumblerColorNegative);
						Color color = configFile.config.GetValue(TumblerColorNegative).ToRGBA();
						HarmonyPatches.TumblerColorReplaceNegative = true;
						HarmonyPatches.TumblerColorNegative = color;
					}
				}
			}
			HarmonyPatches.TumblerColorsLoaded = true;
		}
		*/
		private void SetCursor()
		{
			if (cursors != null && cursors[0] != null)
			{
				if (cursors[1] == null) cursors[1] = cursors[0];
				if (cursors[2] == null) cursors[2] = cursors[0];
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
