using Cursors;
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
		public static HUDReplacer Instance { get; private set; }
		internal static bool enableDebug = false;
		private static Dictionary<string, string> images;
		private static string filePathConfig = "HUDReplacer";
		private static string colorPathConfig = "HUDReplacerRecolor";
		private TextureCursor[] cursors;

		public static List<ConfigNode> additionalConfigNodes = new List<ConfigNode>();
		public static List<ConfigNode> additionalRecolorNodes = new List<ConfigNode>();
		public void Awake()
		{
			Instance = this;
			Debug.Log("HUDReplacer: Running scene change. " + HighLogic.LoadedScene);
			// No longer cache on first load, as new 'onScene' config option will require a per-scene reload
			//if (images == null)
			//{
			GetTextures();
			//}
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
				/*
				It might be possible for a future update to allow modifying the sprite borders.
				This would allow you to specify a "9-slice" size, which would fix the issue of improperly scaling the texture and causing blurry/distorted artifacts.
				Currently not feasible, as a workaround for the readonly border property needs to be found.
				More info: https://docs.unity3d.com/Manual/9SliceSprites.html

				if (Input.GetKeyUp(KeyCode.A))
				{
					Texture2D[] tex_array = (Texture2D[])(object)Resources.FindObjectsOfTypeAll(typeof(Texture2D));
					Sprite[] sprite_array = (Sprite[])(object)Resources.FindObjectsOfTypeAll(typeof(Sprite));
					foreach(Sprite sprite in sprite_array)
					{
						if(sprite.name == "rect_round_color")
						{
							sprite.border = new Vector4(1,1,1,1);
						}
					}
					Debug.Log("finished");
				}
				*/
			}

		}


		

		public void GetTextures()
		{
			images = new Dictionary<string, string>();
			ConfigNode[] configs = Array.ConvertAll(GameDatabase.Instance.GetConfigs(filePathConfig), x => x.config).ToArray();
			configs = configs.Concat(additionalConfigNodes).ToArray();
			if (configs.Length <= 0)
			{
				Debug.Log("HUDReplacer: No texture configs found.");
				return;
			}
			
			Debug.Log("HUDReplacer file paths found:");
			configs = configs.OrderByDescending(x => int.Parse(x.GetValue("priority"))).ToArray();
			foreach (ConfigNode configFile in configs)
			{
				string filePath = configFile.GetValue("filePath");
				string onScene = configFile.HasValue("onScene") ? configFile.GetValue("onScene") : "";

				if(onScene != "")
				{
					try
					{
						GameScenes scene = (GameScenes)Enum.Parse(typeof(GameScenes), onScene);
						if (HighLogic.LoadedScene != scene) continue;
					}
					catch (Exception e)
					{
						Debug.LogError("HUDReplacer: Error loading onScene variable '" + onScene + "' from filePath: " + filePath);
					}
				}
				
				int priority = int.Parse(configFile.GetValue("priority"));
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

		public void ReplaceTextures()
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
				string tex_name_stripped = tex.name;
				if(tex_name_stripped.Contains('/')) // weird RP1 case. May also happen with other mods
				{
					tex_name_stripped = tex_name_stripped.Split('/').Last();
				}
				foreach (KeyValuePair<string, string> image in images)
				{
					string key_stripped = image.Key;
					
					if (image.Value.Contains("#"))
					{
						// Some textures have multiple variants in varying sizes. We don't want to overwrite a texture with the wrong dimensions, as it will not render correctly.
						// For these special cases, we save the width and height in the filename, appended by a # to tell the program this is a multi-texture.
						key_stripped = image.Key.Substring(0, image.Key.IndexOf("#", StringComparison.Ordinal));
					}
					if(key_stripped == tex_name_stripped)
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
						// NavBall GaugeGee and GaugeThrottle needs special handling as well
						if(key_stripped == "GaugeGee")
						{
							HarmonyPatches.GaugeGeeFilePath = image.Value;
							continue;
						}
						if(key_stripped == "GaugeThrottle")
						{
							HarmonyPatches.GaugeThrottleFilePath = image.Value;
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

		public void LoadHUDColors()
		{
			ConfigNode[] configs = Array.ConvertAll(GameDatabase.Instance.GetConfigs(colorPathConfig), x => x.config).ToArray();
			configs = configs.Concat(additionalRecolorNodes).ToArray();
			if (configs.Length <= 0)
			{
				return;
			}
			configs = configs.OrderByDescending(x => int.Parse(x.GetValue("priority"))).ToArray();
			List<string> colorsSet = new List<string>();
			foreach (ConfigNode configFile in configs)
			{
				int priority = int.Parse(configFile.GetValue("priority"));

				string TumblerColorPositive = "tumblerColorPositive";
				if (configFile.HasValue(TumblerColorPositive))
				{
					if (!colorsSet.Contains(TumblerColorPositive))
					{
						colorsSet.Add(TumblerColorPositive);
						Color color = configFile.GetValue(TumblerColorPositive).ToRGBA();
						HarmonyPatches.TumblerColorReplacePositive = true;
						HarmonyPatches.TumblerColorPositive = color;
					}
				}
				string TumblerColorNegative = "tumblerColorNegative";
				if (configFile.HasValue(TumblerColorNegative))
				{
					if (!colorsSet.Contains(TumblerColorNegative))
					{
						colorsSet.Add(TumblerColorNegative);
						Color color = configFile.GetValue(TumblerColorNegative).ToRGBA();
						HarmonyPatches.TumblerColorReplaceNegative = true;
						HarmonyPatches.TumblerColorNegative = color;
					}
				}

				string PAWTitleBar = "PAWTitleBar";
				if (configFile.HasValue(PAWTitleBar))
				{
					if (!colorsSet.Contains(PAWTitleBar))
					{
						colorsSet.Add(PAWTitleBar);
						HarmonyPatches.PAWTitleBar_color = configFile.GetValue(PAWTitleBar).ToRGBA();
						HarmonyPatches.PAWTitleBar_replace = true;
					}
				}

				string PAWBlueButton = "PAWBlueButton";
				if (configFile.HasValue(PAWBlueButton))
				{
					if (!colorsSet.Contains(PAWBlueButton))
					{
						colorsSet.Add(PAWBlueButton);
						HarmonyPatches.PAWBlueButton_replace = true;
						HarmonyPatches.PAWBlueButton_color = configFile.GetValue(PAWBlueButton).ToRGBA();
					}
				}

				string PAWBlueButtonToggle = "PAWBlueButtonToggle";
				if (configFile.HasValue(PAWBlueButtonToggle))
				{
					if (!colorsSet.Contains(PAWBlueButtonToggle))
					{
						colorsSet.Add(PAWBlueButtonToggle);
						HarmonyPatches.PAWBlueButtonToggle_replace = true;
						HarmonyPatches.PAWBlueButtonToggle_color = configFile.GetValue(PAWBlueButtonToggle).ToRGBA();
					}
				}

				string PAWVariantSelectorNext = "PAWVariantSelectorNext";
				if (configFile.HasValue(PAWVariantSelectorNext))
				{
					if (!colorsSet.Contains(PAWVariantSelectorNext))
					{
						colorsSet.Add(PAWVariantSelectorNext);
						HarmonyPatches.PAWVariantSelectorNext_replace = true;
						HarmonyPatches.PAWVariantSelectorNext_color = configFile.GetValue(PAWVariantSelectorNext).ToRGBA();
					}
				}

				string PAWVariantSelectorPrevious = "PAWVariantSelectorPrevious";
				if (configFile.HasValue(PAWVariantSelectorPrevious))
				{
					if (!colorsSet.Contains(PAWVariantSelectorPrevious))
					{
						colorsSet.Add(PAWVariantSelectorPrevious);
						HarmonyPatches.PAWVariantSelectorPrevious_replace = true;
						HarmonyPatches.PAWVariantSelectorPrevious_color = configFile.GetValue(PAWVariantSelectorPrevious).ToRGBA();
					}
				}

				string PAWResourcePriorityIncrease = "PAWResourcePriorityIncrease";
				if (configFile.HasValue(PAWResourcePriorityIncrease))
				{
					if (!colorsSet.Contains(PAWResourcePriorityIncrease))
					{
						colorsSet.Add(PAWResourcePriorityIncrease);
						HarmonyPatches.PAWResourcePriorityIncrease_replace = true;
						HarmonyPatches.PAWResourcePriorityIncrease_color = configFile.GetValue(PAWResourcePriorityIncrease).ToRGBA();
					}
				}

				string PAWResourcePriorityDecrease = "PAWResourcePriorityDecrease";
				if (configFile.HasValue(PAWResourcePriorityDecrease))
				{
					if (!colorsSet.Contains(PAWResourcePriorityDecrease))
					{
						colorsSet.Add(PAWResourcePriorityDecrease);
						HarmonyPatches.PAWResourcePriorityDecrease_replace = true;
						HarmonyPatches.PAWResourcePriorityDecrease_color = configFile.GetValue(PAWResourcePriorityDecrease).ToRGBA();
					}
				}

				string PAWResourcePriorityReset = "PAWResourcePriorityReset";
				if (configFile.HasValue(PAWResourcePriorityReset))
				{
					if (!colorsSet.Contains(PAWResourcePriorityReset))
					{
						colorsSet.Add(PAWResourcePriorityReset);
						HarmonyPatches.PAWResourcePriorityReset_replace = true;
						HarmonyPatches.PAWResourcePriorityReset_color = configFile.GetValue(PAWResourcePriorityReset).ToRGBA();
					}
				}

				string PAWFuelSliderColor = "PAWFuelSliderColor";
				if (configFile.HasValue(PAWFuelSliderColor))
				{
					if (!colorsSet.Contains(PAWFuelSliderColor))
					{
						colorsSet.Add(PAWFuelSliderColor);
						HarmonyPatches.PAWFuelSliderColor_replace = true;
						HarmonyPatches.PAWFuelSliderColor = configFile.GetValue(PAWFuelSliderColor).ToRGBA();
					}
				}

				string PAWFuelSliderTextColor = "PAWFuelSliderTextColor";
				if (configFile.HasValue(PAWFuelSliderTextColor))
				{
					if (!colorsSet.Contains(PAWFuelSliderTextColor))
					{
						colorsSet.Add(PAWFuelSliderTextColor);
						HarmonyPatches.PAWFuelSliderTextColor_replace = true;
						HarmonyPatches.PAWFuelSliderTextColor = configFile.GetValue(PAWFuelSliderTextColor).ToRGBA();
					}
				}

				string KALTitleBar = "KALTitleBar";
				if (configFile.HasValue(KALTitleBar))
				{
					if (!colorsSet.Contains(KALTitleBar))
					{
						colorsSet.Add(KALTitleBar);
						HarmonyPatches.KALTitleBar_color = configFile.GetValue(KALTitleBar).ToRGBA();
						HarmonyPatches.KALTitleBar_replace = true;
					}
				}

				string gaugeNeedleYawPitchRoll = "gaugeNeedleYawPitchRoll";
				if (configFile.HasValue(gaugeNeedleYawPitchRoll))
				{
					if (!colorsSet.Contains(gaugeNeedleYawPitchRoll))
					{
						colorsSet.Add(gaugeNeedleYawPitchRoll);
						HarmonyPatches.gaugeNeedleYawPitchRollColor = configFile.GetValue(gaugeNeedleYawPitchRoll).ToRGBA();
						HarmonyPatches.gaugeNeedleYawPitchRollColor_replace = true;
					}
				}

				string gaugeNeedleYawPitchRollPrecision = "gaugeNeedleYawPitchRollPrecision";
				if (configFile.HasValue(gaugeNeedleYawPitchRollPrecision))
				{
					if (!colorsSet.Contains(gaugeNeedleYawPitchRollPrecision))
					{
						colorsSet.Add(gaugeNeedleYawPitchRollPrecision);
						HarmonyPatches.gaugeNeedleYawPitchRollPrecisionColor = configFile.GetValue(gaugeNeedleYawPitchRollPrecision).ToRGBA();
						HarmonyPatches.gaugeNeedleYawPitchRollPrecisionColor_replace = true;
					}
				}

				string METDisplayColorRed = "METDisplayColorRed";
				if (configFile.HasValue(METDisplayColorRed))
				{
					if (!colorsSet.Contains(METDisplayColorRed))
					{
						colorsSet.Add(METDisplayColorRed);
						HarmonyPatches.METDisplayColorRed = configFile.GetValue(METDisplayColorRed).ToRGBA();
					}
				}

				string METDisplayColorYellow = "METDisplayColorYellow";
				if (configFile.HasValue(METDisplayColorYellow))
				{
					if (!colorsSet.Contains(METDisplayColorYellow))
					{
						colorsSet.Add(METDisplayColorYellow);
						HarmonyPatches.METDisplayColorYellow = configFile.GetValue(METDisplayColorYellow).ToRGBA();
					}
				}

				string METDisplayColorGreen = "METDisplayColorGreen";
				if (configFile.HasValue(METDisplayColorGreen))
				{
					if (!colorsSet.Contains(METDisplayColorGreen))
					{
						colorsSet.Add(METDisplayColorGreen);
						HarmonyPatches.METDisplayColorGreen = configFile.GetValue(METDisplayColorGreen).ToRGBA();
					}
				}

				string SpeedDisplayColorTextReplace = "speedDisplayColorText";
				if (configFile.HasValue(SpeedDisplayColorTextReplace))
				{
					if (!colorsSet.Contains(SpeedDisplayColorTextReplace))
					{
						colorsSet.Add(SpeedDisplayColorTextReplace);
						HarmonyPatches.SpeedDisplayColorTextReplace = true;
						HarmonyPatches.SpeedDisplayColorText = configFile.GetValue(SpeedDisplayColorTextReplace).ToRGBA();
					}
				}

				string SpeedDisplayColorSpeedReplace = "speedDisplayColorSpeed";
				if (configFile.HasValue(SpeedDisplayColorSpeedReplace))
				{
					if (!colorsSet.Contains(SpeedDisplayColorSpeedReplace))
					{
						colorsSet.Add(SpeedDisplayColorSpeedReplace);
						HarmonyPatches.SpeedDisplayColorSpeedReplace = true;
						HarmonyPatches.SpeedDisplayColorSpeed = configFile.GetValue(SpeedDisplayColorSpeedReplace).ToRGBA();
					}
				}

				string NavBallHeadingColor = "navBallHeadingColor";
				if (configFile.HasValue(NavBallHeadingColor))
				{
					if (!colorsSet.Contains(NavBallHeadingColor))
					{
						colorsSet.Add(NavBallHeadingColor);
						HarmonyPatches.NavBallHeadingColorReplace = true;
						HarmonyPatches.NavBallHeadingColor = configFile.GetValue(NavBallHeadingColor).ToRGBA();
					}
				}

				string StageTotalDeltaVColor = "stageTotalDeltaVColor";
				if (configFile.HasValue(StageTotalDeltaVColor))
				{
					if (!colorsSet.Contains(StageTotalDeltaVColor))
					{
						colorsSet.Add(StageTotalDeltaVColor);
						HarmonyPatches.StageTotalDeltaVColorReplace = true;
						HarmonyPatches.StageTotalDeltaVColor = configFile.GetValue(StageTotalDeltaVColor).ToRGBA();
					}
				}

				string StageGroupDeltaVTextColor = "stageGroupDeltaVTextColor";
				if (configFile.HasValue(StageGroupDeltaVTextColor))
				{
					if (!colorsSet.Contains(StageGroupDeltaVTextColor))
					{
						colorsSet.Add(StageGroupDeltaVTextColor);
						HarmonyPatches.StageGroupDeltaVTextColorReplace = true;
						HarmonyPatches.StageGroupDeltaVTextColor = configFile.GetValue(StageGroupDeltaVTextColor).ToRGBA();
					}
				}

				string StageGroupDeltaVNumberColor = "stageGroupDeltaVNumberColor";
				if (configFile.HasValue(StageGroupDeltaVNumberColor))
				{
					if (!colorsSet.Contains(StageGroupDeltaVNumberColor))
					{
						colorsSet.Add(StageGroupDeltaVNumberColor);
						HarmonyPatches.StageGroupDeltaVNumberColorReplace = true;
						HarmonyPatches.StageGroupDeltaVNumberColor = configFile.GetValue(StageGroupDeltaVNumberColor).ToRGBA();
					}
				}

				string StageGroupDeltaVBackgroundColor = "stageGroupDeltaVBackgroundColor";
				if (configFile.HasValue(StageGroupDeltaVBackgroundColor))
				{
					if (!colorsSet.Contains(StageGroupDeltaVBackgroundColor))
					{
						colorsSet.Add(StageGroupDeltaVBackgroundColor);
						HarmonyPatches.StageGroupDeltaVBackgroundColorReplace = true;
						HarmonyPatches.StageGroupDeltaVBackgroundColor = configFile.GetValue(StageGroupDeltaVBackgroundColor).ToRGBA();
					}
				}

				string StageEngineFuelGaugeTextColor = "stageEngineFuelGaugeTextColor";
				if (configFile.HasValue(StageEngineFuelGaugeTextColor))
				{
					if (!colorsSet.Contains(StageEngineFuelGaugeTextColor))
					{
						colorsSet.Add(StageEngineFuelGaugeTextColor);
						HarmonyPatches.StageEngineFuelGaugeTextColor_replace = true;
						HarmonyPatches.StageEngineFuelGaugeTextColor_color = configFile.GetValue(StageEngineFuelGaugeTextColor).ToRGBA();
					}
				}

				string StageEngineHeatGaugeTextColor = "stageEngineHeatGaugeTextColor";
				if (configFile.HasValue(StageEngineHeatGaugeTextColor))
				{
					if (!colorsSet.Contains(StageEngineHeatGaugeTextColor))
					{
						colorsSet.Add(StageEngineHeatGaugeTextColor);
						HarmonyPatches.StageEngineHeatGaugeTextColor_replace = true;
						HarmonyPatches.StageEngineHeatGaugeTextColor_color = configFile.GetValue(StageEngineHeatGaugeTextColor).ToRGBA();
					}
				}

				string StageEngineFuelGaugeBackgroundColor = "stageEngineFuelGaugeBackgroundColor";
				if (configFile.HasValue(StageEngineFuelGaugeBackgroundColor))
				{
					if (!colorsSet.Contains(StageEngineFuelGaugeBackgroundColor))
					{
						colorsSet.Add(StageEngineFuelGaugeBackgroundColor);
						HarmonyPatches.StageEngineFuelGaugeBackgroundColor_replace = true;
						HarmonyPatches.StageEngineFuelGaugeBackgroundColor_color = configFile.GetValue(StageEngineFuelGaugeBackgroundColor).ToRGBA();
					}
				}

				string StageEngineHeatGaugeBackgroundColor = "stageEngineHeatGaugeBackgroundColor";
				if (configFile.HasValue(StageEngineHeatGaugeBackgroundColor))
				{
					if (!colorsSet.Contains(StageEngineHeatGaugeBackgroundColor))
					{
						colorsSet.Add(StageEngineHeatGaugeBackgroundColor);
						HarmonyPatches.StageEngineHeatGaugeBackgroundColor_replace = true;
						HarmonyPatches.StageEngineHeatGaugeBackgroundColor_color = configFile.GetValue(StageEngineHeatGaugeBackgroundColor).ToRGBA();
					}
				}

				string StageEngineFuelGaugeFillColor = "stageEngineFuelGaugeFillColor";
				if (configFile.HasValue(StageEngineFuelGaugeFillColor))
				{
					if (!colorsSet.Contains(StageEngineFuelGaugeFillColor))
					{
						colorsSet.Add(StageEngineFuelGaugeFillColor);
						HarmonyPatches.StageEngineFuelGaugeFillColor_replace = true;
						HarmonyPatches.StageEngineFuelGaugeFillColor_color = configFile.GetValue(StageEngineFuelGaugeFillColor).ToRGBA();
					}
				}

				string StageEngineHeatGaugeFillColor = "stageEngineHeatGaugeFillColor";
				if (configFile.HasValue(StageEngineHeatGaugeFillColor))
				{
					if (!colorsSet.Contains(StageEngineHeatGaugeFillColor))
					{
						colorsSet.Add(StageEngineHeatGaugeFillColor);
						HarmonyPatches.StageEngineHeatGaugeFillColor_replace = true;
						HarmonyPatches.StageEngineHeatGaugeFillColor_color = configFile.GetValue(StageEngineHeatGaugeFillColor).ToRGBA();
					}
				}

				string StageEngineFuelGaugeFillBackgroundColor = "stageEngineFuelGaugeFillBackgroundColor";
				if (configFile.HasValue(StageEngineFuelGaugeFillBackgroundColor))
				{
					if (!colorsSet.Contains(StageEngineFuelGaugeFillBackgroundColor))
					{
						colorsSet.Add(StageEngineFuelGaugeFillBackgroundColor);
						HarmonyPatches.StageEngineFuelGaugeFillBackgroundColor_replace = true;
						HarmonyPatches.StageEngineFuelGaugeFillBackgroundColor_color = configFile.GetValue(StageEngineFuelGaugeFillBackgroundColor).ToRGBA();
					}
				}

				string StageEngineHeatGaugeFillBackgroundColor = "stageEngineHeatGaugeFillBackgroundColor";
				if (configFile.HasValue(StageEngineHeatGaugeFillBackgroundColor))
				{
					if (!colorsSet.Contains(StageEngineHeatGaugeFillBackgroundColor))
					{
						colorsSet.Add(StageEngineHeatGaugeFillBackgroundColor);
						HarmonyPatches.StageEngineHeatGaugeFillBackgroundColor_replace = true;
						HarmonyPatches.StageEngineHeatGaugeFillBackgroundColor_color = configFile.GetValue(StageEngineHeatGaugeFillBackgroundColor).ToRGBA();
					}
				}

				string NavBallCursor = "navballCursor";
				if (configFile.HasValue(NavBallCursor))
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
								img.color = configFile.GetValue(NavBallCursor).ToRGBA();
							}
						}
					}
				}

				string VerticalSpeedGaugeNeedle = "verticalSpeedGaugeNeedleColor";
				if (configFile.HasValue(VerticalSpeedGaugeNeedle))
				{
					if (!colorsSet.Contains(VerticalSpeedGaugeNeedle))
					{
						colorsSet.Add(VerticalSpeedGaugeNeedle);
						HarmonyPatches.VerticalSpeedGaugeNeedleColorReplace = true;
						HarmonyPatches.VerticalSpeedGaugeNeedleColor = configFile.GetValue(VerticalSpeedGaugeNeedle).ToRGBA();
					}
				}

				string ManeuverNodeEditorTextColor = "maneuverNodeEditorTextColor";
				if (configFile.HasValue(ManeuverNodeEditorTextColor))
				{
					if (!colorsSet.Contains(ManeuverNodeEditorTextColor))
					{
						colorsSet.Add(ManeuverNodeEditorTextColor);
						HarmonyPatches.ManeuverNodeEditorTextColor_replace = true;
						HarmonyPatches.ManeuverNodeEditorTextColor = configFile.GetValue(ManeuverNodeEditorTextColor).ToRGBA();
					}
				}

				string SASDisplayOnColor = "SASDisplayOnColor";
				if (configFile.HasValue(SASDisplayOnColor))
				{
					if (!colorsSet.Contains(SASDisplayOnColor))
					{
						colorsSet.Add(SASDisplayOnColor);
						HarmonyPatches.SASDisplayColor_SAS_Replace_On = true;
						HarmonyPatches.SASDisplayColor_SAS_On_color = configFile.GetValue(SASDisplayOnColor).ToRGBA();
					}
				}

				string SASDisplayOffColor = "SASDisplayOffColor";
				if (configFile.HasValue(SASDisplayOffColor))
				{
					if (!colorsSet.Contains(SASDisplayOffColor))
					{
						colorsSet.Add(SASDisplayOffColor);
						HarmonyPatches.SASDisplayColor_SAS_Replace_Off = true;
						HarmonyPatches.SASDisplayColor_SAS_Off_color = configFile.GetValue(SASDisplayOffColor).ToRGBA();
					}
				}

				string RCSDisplayOnColor = "RCSDisplayOnColor";
				if (configFile.HasValue(RCSDisplayOnColor))
				{
					if (!colorsSet.Contains(RCSDisplayOnColor))
					{
						colorsSet.Add(RCSDisplayOnColor);
						HarmonyPatches.RCSDisplayColor_RCS_Replace_On = true;
						HarmonyPatches.RCSDisplayColor_RCS_On_color = configFile.GetValue(RCSDisplayOnColor).ToRGBA();
					}
				}

				string RCSDisplayOffColor = "RCSDisplayOffColor";
				if (configFile.HasValue(RCSDisplayOffColor))
				{
					if (!colorsSet.Contains(RCSDisplayOffColor))
					{
						colorsSet.Add(RCSDisplayOffColor);
						HarmonyPatches.RCSDisplayColor_RCS_Replace_Off = true;
						HarmonyPatches.RCSDisplayColor_RCS_Off_color = configFile.GetValue(RCSDisplayOffColor).ToRGBA();
					}
				}
				
				string EditorCategoryButtonColor = "EditorCategoryButtonColor";
				if (configFile.HasValue(EditorCategoryButtonColor))
				{
					if (!colorsSet.Contains(EditorCategoryButtonColor))
					{
						colorsSet.Add(EditorCategoryButtonColor);
						HarmonyPatches.EditorCategoryButtonColor_replace = true;
						HarmonyPatches.EditorCategoryButtonColor_color = configFile.GetValue(EditorCategoryButtonColor).ToRGBA();
					}
				}

				string EditorCategoryButtonColor_Module = "EditorCategoryModuleButtonColor";
				if (configFile.HasValue(EditorCategoryButtonColor_Module))
				{
					if (!colorsSet.Contains(EditorCategoryButtonColor_Module))
					{
						colorsSet.Add(EditorCategoryButtonColor_Module);
						HarmonyPatches.EditorCategoryButtonColor_Module_replace = true;
						HarmonyPatches.EditorCategoryButtonColor_Module_color = configFile.GetValue(EditorCategoryButtonColor_Module).ToRGBA();
					}
				}

				string EditorCategoryButtonColor_Resource = "EditorCategoryResourceButtonColor";
				if (configFile.HasValue(EditorCategoryButtonColor_Resource))
				{
					if (!colorsSet.Contains(EditorCategoryButtonColor_Resource))
					{
						colorsSet.Add(EditorCategoryButtonColor_Resource);
						HarmonyPatches.EditorCategoryButtonColor_Resource_replace = true;
						HarmonyPatches.EditorCategoryButtonColor_Resource_color = configFile.GetValue(EditorCategoryButtonColor_Resource).ToRGBA();
					}
				}

				string EditorCategoryButtonColor_Manufacturer = "EditorCategoryManufacturerButtonColor";
				if (configFile.HasValue(EditorCategoryButtonColor_Manufacturer))
				{
					if (!colorsSet.Contains(EditorCategoryButtonColor_Manufacturer))
					{
						colorsSet.Add(EditorCategoryButtonColor_Manufacturer);
						HarmonyPatches.EditorCategoryButtonColor_Manufacturer_replace = true;
						HarmonyPatches.EditorCategoryButtonColor_Manufacturer_color = configFile.GetValue(EditorCategoryButtonColor_Manufacturer).ToRGBA();
					}
				}

				string EditorCategoryButtonColor_Tech = "EditorCategoryTechButtonColor";
				if (configFile.HasValue(EditorCategoryButtonColor_Tech))
				{
					if (!colorsSet.Contains(EditorCategoryButtonColor_Tech))
					{
						colorsSet.Add(EditorCategoryButtonColor_Tech);
						HarmonyPatches.EditorCategoryButtonColor_Tech_replace = true;
						HarmonyPatches.EditorCategoryButtonColor_Tech_color = configFile.GetValue(EditorCategoryButtonColor_Tech).ToRGBA();
					}
				}

				string EditorCategoryButtonColor_Profile = "EditorCategoryProfileButtonColor";
				if (configFile.HasValue(EditorCategoryButtonColor_Profile))
				{
					if (!colorsSet.Contains(EditorCategoryButtonColor_Profile))
					{
						colorsSet.Add(EditorCategoryButtonColor_Profile);
						HarmonyPatches.EditorCategoryButtonColor_Profile_replace = true;
						HarmonyPatches.EditorCategoryButtonColor_Profile_color = configFile.GetValue(EditorCategoryButtonColor_Profile).ToRGBA();
					}
				}

				string EditorCategoryButtonColor_Subassembly = "EditorCategorySubassemblyButtonColor";
				if (configFile.HasValue(EditorCategoryButtonColor_Subassembly))
				{
					if (!colorsSet.Contains(EditorCategoryButtonColor_Subassembly))
					{
						colorsSet.Add(EditorCategoryButtonColor_Subassembly);
						HarmonyPatches.EditorCategoryButtonColor_Subassembly_replace = true;
						HarmonyPatches.EditorCategoryButtonColor_Subassembly_color = configFile.GetValue(EditorCategoryButtonColor_Subassembly).ToRGBA();
					}
				}

				string EditorCategoryButtonColor_Variants = "EditorCategoryVariantsButtonColor";
				if (configFile.HasValue(EditorCategoryButtonColor_Variants))
				{
					if (!colorsSet.Contains(EditorCategoryButtonColor_Variants))
					{
						colorsSet.Add(EditorCategoryButtonColor_Variants);
						HarmonyPatches.EditorCategoryButtonColor_Variants_replace = true;
						HarmonyPatches.EditorCategoryButtonColor_Variants_color = configFile.GetValue(EditorCategoryButtonColor_Variants).ToRGBA();
					}
				}

				string EditorCategoryButtonColor_Custom = "EditorCategoryCustomButtonColor";
				if (configFile.HasValue(EditorCategoryButtonColor_Custom))
				{
					if (!colorsSet.Contains(EditorCategoryButtonColor_Custom))
					{
						colorsSet.Add(EditorCategoryButtonColor_Custom);
						HarmonyPatches.EditorCategoryButtonColor_Custom_replace = true;
						HarmonyPatches.EditorCategoryButtonColor_Custom_color = configFile.GetValue(EditorCategoryButtonColor_Custom).ToRGBA();
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
