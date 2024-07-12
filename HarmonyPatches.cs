using Expansions.Serenity;
using HarmonyLib;
using KSP.UI;
using KSP.UI.Screens;
using KSP.UI.Screens.Flight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Kerbal;

namespace HUDReplacer
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class HarmonyPatches : MonoBehaviour
	{
		public void Awake()
		{
			// NOTE: A Harmony patcher should be placed in a run once Startup addon. The patch is kept between scene changes.
			var harmony = new Harmony("UltraJohn.Mods.HUDReplacer");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}


		// Tumbler colors
		internal static bool TumblerColorReplacePositive = false;
		internal static bool TumblerColorReplaceNegative = false;
		internal static Color TumblerColorPositive = Color.black;
		internal static Color TumblerColorNegative = new Color(0.6f, 0f, 0f);

		[HarmonyPatch(typeof(KSP.UI.Screens.Tumbler), "Awake")]
		class Patch1
		{
			static void Prefix(ref Color ___positiveColor, ref Color ___negativeColor)
			{
				if (TumblerColorReplacePositive)
				{
					___positiveColor = TumblerColorPositive;
				}
				if (TumblerColorReplaceNegative)
				{
					___negativeColor = TumblerColorNegative;
				}
			}
		}

		[HarmonyPatch(typeof(StageTumbler), "Awake")]
		class Patch1_2
		{
			static void Postfix(StageTumbler __instance)
			{
				if (TumblerColorReplacePositive)
				{
					MeshRenderer[] meshes = __instance.gameObject.GetComponentsInChildren<MeshRenderer>();
					foreach(MeshRenderer mesh in meshes)
					{
						mesh.material.color = TumblerColorPositive;
					}
				}
			}
		}

		// PAW Title bar patch
		internal static bool PAWTitleBar_replace = false;
		internal static Color PAWTitleBar_color;

		[HarmonyPatch(typeof(UIPartActionController), "CreatePartUI")]
		class Patch2
		{
			static void Postfix(ref UIPartActionWindow __result)
			{
				if (!PAWTitleBar_replace) return;
				try
				{

					Image[] images = __result.gameObject.GetComponentsInChildren<Image>();
					foreach (Image img in images)
					{
						if (img.mainTexture.name == "app_divider_pulldown_header_over")
						{
							img.color = PAWTitleBar_color;
						}
					}
				}
				catch (Exception e)
				{
					Debug.Log(e.ToString());
				}
			}
		}

		// PAW Blue button
		internal static bool PAWBlueButton_replace = false;
		internal static Color PAWBlueButton_color;

		[HarmonyPatch(typeof(UIPartActionButton), "Awake")]
		class Patch2_1
		{
			static void Postfix(ref UIPartActionButton __instance)
			{
				if (!PAWBlueButton_replace) return;
				__instance.button.GetComponent<Image>().color = PAWBlueButton_color;
			}
		}

		// PAW Blue button Toggle & Fuel Flow Buttons
		internal static bool PAWBlueButtonToggle_replace = false;
		internal static Color PAWBlueButtonToggle_color;

		[HarmonyPatch(typeof(UIButtonToggle), "Awake")]
		class Patch2_2
		{
			static void Postfix(ref UIButtonToggle __instance)
			{
				if (PAWBlueButtonToggle_replace)
				{
					if (__instance.toggleImage.mainTexture.name.Contains("Blue_Btn"))
					{
						__instance.toggleImage.color = PAWBlueButtonToggle_color;
					}
				}
			}
		}

		// PAW Variant Selector Previous/Next Button
		internal static bool PAWVariantSelectorNext_replace = false;
		internal static Color PAWVariantSelectorNext_color;
		internal static bool PAWVariantSelectorPrevious_replace = false;
		internal static Color PAWVariantSelectorPrevious_color;

		[HarmonyPatch(typeof(UIPartActionVariantSelector), nameof(UIPartActionVariantSelector.Setup))]
		class Patch2_3
		{
			static void Postfix(ref UIPartActionVariantSelector __instance)
			{
				if (PAWVariantSelectorNext_replace)
				{
					__instance.buttonNext.GetComponent<Image>().color = PAWVariantSelectorNext_color;
				}
				if (PAWVariantSelectorPrevious_replace)
				{
					__instance.buttonPrevious.GetComponent<Image>().color = PAWVariantSelectorPrevious_color;
				}

			}
		}

		// PAW Resource Priority Increase/Decrease/Reset Button
		internal static bool PAWResourcePriorityIncrease_replace = false;
		internal static Color PAWResourcePriorityIncrease_color;
		internal static bool PAWResourcePriorityDecrease_replace = false;
		internal static Color PAWResourcePriorityDecrease_color;
		internal static bool PAWResourcePriorityReset_replace = false;
		internal static Color PAWResourcePriorityReset_color;

		[HarmonyPatch(typeof(UIPartActionResourcePriority), "Awake")]
		class Patch2_4
		{
			static void Postfix(ref Button ___btnInc, ref Button ___btnDec, ref Button ___btnReset)
			{
				if (PAWResourcePriorityIncrease_replace)
				{
					___btnInc.GetComponent<Image>().color = PAWResourcePriorityIncrease_color;
				}
				if (PAWResourcePriorityDecrease_replace)
				{
					___btnDec.GetComponent<Image>().color = PAWResourcePriorityDecrease_color;
				}
				if (PAWResourcePriorityReset_replace)
				{
					___btnReset.GetComponent<Image>().color = PAWResourcePriorityReset_color;
				}
			}
		}

		// PAW Fuel Slider
		internal static bool PAWFuelSliderColor_replace = false;
		internal static Color PAWFuelSliderColor;
		internal static bool PAWFuelSliderTextColor_replace = false;
		internal static Color PAWFuelSliderTextColor;

		[HarmonyPatch(typeof(UIPartActionResourceEditor), "Setup")]
		class Patch2_5
		{
			static void Postfix(ref UIPartActionResourceEditor __instance)
			{
				if (PAWFuelSliderColor_replace)
				{
					__instance.slider.image.color = PAWFuelSliderColor;
				}
				if (PAWFuelSliderTextColor_replace)
				{
					__instance.resourceName.color = PAWFuelSliderTextColor;
					__instance.resourceAmnt.color = PAWFuelSliderTextColor;
					__instance.resourceMax.color = PAWFuelSliderTextColor;
					foreach (Transform child in __instance.sliderContainer.transform)
					{
						if (child.name == "Slash")
						{
							child.GetComponent<TextMeshProUGUI>().color = PAWFuelSliderTextColor;
						}
						/* re-enable if background can't be colored through the texture itself
						if (child.name == "Background")
						{
							child.GetComponent<Image>().color = Color.yellow;
							break;
						}
						*/
					}
				}
				

			}
		}

		// KAL-1000 Editor patch
		internal static bool KALTitleBar_replace = false;
		internal static Color KALTitleBar_color;

		[HarmonyPatch(typeof(RoboticControllerWindow), nameof(RoboticControllerWindow.Spawn))]
		class Patch3
		{
			static void Postfix(ref RoboticControllerWindow __result)
			{
				if (!__result) return;
				Image[] image_array = __result.gameObject.GetComponentsInChildren<Image>();
				List<Texture2D> textures = new List<Texture2D>();
				foreach (Image img in image_array)
				{
					textures.Add((Texture2D)img.mainTexture);
					if(img.mainTexture.name == "app_divider_pulldown_header_over" && KALTitleBar_replace)
					{
						img.color = KALTitleBar_color;
					}
				}
				Texture2D[] tex_array = textures.ToArray();
				if (tex_array.Length > 0) HUDReplacer.instance.ReplaceTextures(tex_array);
			}
		}

		// Yaw/Pitch/Roll + Precision mode
		// gaugeNeedleYawPitchRoll
		internal static bool gaugeNeedleYawPitchRollColor_replace = false;
		internal static bool gaugeNeedleYawPitchRollPrecisionColor_replace = false;
		internal static Color gaugeNeedleYawPitchRollColor;
		internal static Color gaugeNeedleYawPitchRollPrecisionColor;

		[HarmonyPatch(typeof(LinearControlGauges), nameof(LinearControlGauges.Start))]
		class Patch4
		{
			static void Postfix(ref LinearControlGauges __instance, ref List<Image> ___inputGaugeImages)
			{
				if (!__instance) return;
				if (!gaugeNeedleYawPitchRollColor_replace) return;
				try
				{
					foreach (Image img in ___inputGaugeImages)
					{
						img.color = gaugeNeedleYawPitchRollColor;
					}
				}
				catch (Exception e)
				{
					Debug.Log(e.ToString());
				}
			}
		}

		[HarmonyPatch(typeof(LinearControlGauges), "onPrecisionModeToggle")]
		class Patch5
		{
			static void Postfix(ref LinearControlGauges __instance, ref bool precisionMode, ref List<Image> ___inputGaugeImages)
			{
				if (!__instance) return;
				if (gaugeNeedleYawPitchRollColor_replace == false && gaugeNeedleYawPitchRollPrecisionColor_replace == false) return;
				try
				{
					foreach(Image img in ___inputGaugeImages)
					{
						if (!precisionMode)
						{
							if(gaugeNeedleYawPitchRollColor_replace)
							{
								img.color = gaugeNeedleYawPitchRollColor;
							}
							
						}
						else
						{
							if(gaugeNeedleYawPitchRollPrecisionColor_replace)
							{
								img.color = gaugeNeedleYawPitchRollPrecisionColor;
							}
							
						}
					}
				}
				catch (Exception e)
				{
					Debug.Log(e.ToString());
				}
			}
		}

		// Top left clock widget

		internal static Color METDisplayColorRed = Color.red;
		internal static Color METDisplayColorYellow = Color.yellow;
		internal static Color METDisplayColorGreen = Color.green;

		internal static Color METDisplayRedColorOverride()
		{
			return METDisplayColorRed;
		}
		internal static Color METDisplayYellowColorOverride()
		{
			return METDisplayColorYellow;
		}
		internal static Color METDisplayGreenColorOverride()
		{
			return METDisplayColorGreen;
		}

		[HarmonyPatch(typeof(METDisplay), "LateUpdate")]
		class Patch6
		{
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var getColorRed = AccessTools.Method(typeof(Color), "get_red");
				var getColorYellow = AccessTools.Method(typeof(Color), "get_yellow");
				var getColorGreen = AccessTools.Method(typeof(Color), "get_green");
				foreach (var instruction in instructions)
				{
					if (instruction.Calls(getColorRed))
					{
						instruction.operand = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.METDisplayRedColorOverride));
					}
					else if (instruction.Calls(getColorYellow))
					{
						instruction.operand = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.METDisplayYellowColorOverride));
					}
					else if (instruction.Calls(getColorGreen))
					{
						instruction.operand = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.METDisplayGreenColorOverride));
					}
					yield return instruction;
				}
			}
		}

		// Top left clock widget

		[HarmonyPatch(typeof(UIPlanetariumDateTime), "Start")]
		class Patch7
		{
			static void Postfix(ref UIPlanetariumDateTime __instance)
			{
				__instance.textDate.color = METDisplayColorGreen;
			}
		}

		// Top left clock widget

		[HarmonyPatch(typeof(UIPlanetariumDateTime), "onGameUnPause")]
		class Patch8
		{
			static void Postfix(ref UIPlanetariumDateTime __instance)
			{
				__instance.textDate.color = METDisplayColorGreen;
			}
		}

		// NavBall speed display unit

		internal static bool SpeedDisplayColorTextReplace = false;
		internal static bool SpeedDisplayColorSpeedReplace = false;
		internal static Color SpeedDisplayColorText = Color.green;
		internal static Color SpeedDisplayColorSpeed = Color.green;

		[HarmonyPatch(typeof(SpeedDisplay), "Start")]
		class Patch9
		{
			static void Postfix(ref SpeedDisplay __instance)
			{
				if (SpeedDisplayColorTextReplace)
				{
					__instance.textTitle.color = SpeedDisplayColorText;
				}
				if (SpeedDisplayColorSpeedReplace)
				{
					__instance.textSpeed.color = SpeedDisplayColorSpeed;
				}
			}
		}

		// NavBall heading unit

		internal static bool NavBallHeadingColorReplace = false;
		internal static Color NavBallHeadingColor = Color.green;

		[HarmonyPatch(typeof(NavBall), "Start")]
		class Patch10
		{
			static void Postfix(ref NavBall __instance)
			{
				if (NavBallHeadingColorReplace)
				{
					__instance.headingText.color = NavBallHeadingColor;
				}
			}
		}

		// Stage Total deltaV
		internal static bool StageTotalDeltaVColorReplace = false;
		internal static Color StageTotalDeltaVColor = Color.white;

		[HarmonyPatch(typeof(StageManager), "Awake")]
		class Patch11
		{
			static void Postfix(ref StageManager __instance)
			{
				if (__instance && StageTotalDeltaVColorReplace)
				{
					__instance.deltaVTotalText.color = StageTotalDeltaVColor;
				}
			}
		}

		// Stage Group deltaV
		internal static bool StageGroupDeltaVTextColorReplace = false;
		internal static bool StageGroupDeltaVNumberColorReplace = false;
		internal static bool StageGroupDeltaVBackgroundColorReplace = false;
		internal static Color StageGroupDeltaVTextColor = Color.white;
		internal static Color StageGroupDeltaVNumberColor = Color.white;
		internal static Color StageGroupDeltaVBackgroundColor = Color.white;

		[HarmonyPatch(typeof(StageGroup), "Awake")]
		class Patch12
		{
			static void Postfix(ref StageGroup __instance, ref TextMeshProUGUI ___DeltaVHeadingText, ref TextMeshProUGUI ___uiStageIndex)
			{
				if (__instance)
				{
					if (StageGroupDeltaVTextColorReplace)
					{
						___DeltaVHeadingText.color = StageGroupDeltaVTextColor;
					}
					if (StageGroupDeltaVNumberColorReplace)
					{
						___uiStageIndex.color = StageGroupDeltaVNumberColor;
					}
					if(StageGroupDeltaVBackgroundColorReplace)
					{
						Image[] images = __instance.GetComponentsInChildren<Image>();
						foreach (Image img in images)
						{
							if(img.mainTexture.name == "StageDV")
							{
								img.color = StageGroupDeltaVBackgroundColor;
							}
						}
					}
				}
			}
		}

		// Stage Engine Fuel & Heat gauge (Text color)
		internal static bool StageEngineFuelGaugeTextColor_replace = false;
		internal static Color StageEngineFuelGaugeTextColor_color;
		internal static bool StageEngineHeatGaugeTextColor_replace = false;
		internal static Color StageEngineHeatGaugeTextColor_color;

		[HarmonyPatch(typeof(ProtoStageIconInfo), nameof(ProtoStageIconInfo.SetMsgTextColor))]
		class Patch12_1
		{
			static bool Prefix(ref Color c)
			{
				if (StageEngineFuelGaugeTextColor_replace)
				{
					// Engine & RCS gauge
					if (c == XKCDColors.ElectricLime.A(0.6f))
					{
						c = StageEngineFuelGaugeTextColor_color;
						return true;
					}
				}
				if (StageEngineHeatGaugeTextColor_replace)
				{
					// Heat gauge
					if (c == XKCDColors.OrangeYellow.A(0.6f))
					{
						c = StageEngineHeatGaugeTextColor_color;
						return true;
					}
				}
				
				return true;
			}
		}

		// Stage Engine Fuel & Heat gauge (Background color)
		internal static bool StageEngineFuelGaugeBackgroundColor_replace = false;
		internal static Color StageEngineFuelGaugeBackgroundColor_color;
		internal static bool StageEngineHeatGaugeBackgroundColor_replace = false;
		internal static Color StageEngineHeatGaugeBackgroundColor_color;

		[HarmonyPatch(typeof(ProtoStageIconInfo), nameof(ProtoStageIconInfo.SetMsgBgColor))]
		class Patch12_2
		{
			static bool Prefix(ref Color c)
			{
				if (StageEngineFuelGaugeBackgroundColor_replace)
				{
					// Engine & RCS gauge
					if (c == XKCDColors.DarkLime.A(0.6f))
					{
						c = StageEngineFuelGaugeBackgroundColor_color;
						return true;
					}
				}
				if (StageEngineHeatGaugeBackgroundColor_replace)
				{
					// Heat gauge
					if (c == XKCDColors.DarkRed.A(0.6f))
					{
						c = StageEngineHeatGaugeBackgroundColor_color;
						return true;
					}
				}
				return true;
			}
		}

		// Stage Engine Fuel & Heat gauge (Fill color)
		internal static bool StageEngineFuelGaugeFillColor_replace = false;
		internal static Color StageEngineFuelGaugeFillColor_color;
		internal static bool StageEngineHeatGaugeFillColor_replace = false;
		internal static Color StageEngineHeatGaugeFillColor_color;

		[HarmonyPatch(typeof(ProtoStageIconInfo), nameof(ProtoStageIconInfo.SetProgressBarColor))]
		class Patch12_3
		{
			static bool Prefix(ref Color c)
			{
				if (StageEngineFuelGaugeFillColor_replace)
				{
					// Engine & RCS gauge
					if (c == XKCDColors.Yellow.A(0.6f))
					{
						c = StageEngineFuelGaugeFillColor_color;
						return true;
					}
				}
				if (StageEngineHeatGaugeFillColor_replace)
				{
					// Heat gauge
					if (c == XKCDColors.OrangeYellow.A(0.6f))
					{
						c = StageEngineHeatGaugeFillColor_color;
						return true;
					}
				}
				return true;
			}
		}

		// Stage Engine Fuel & Heat gauge (Fill background color)
		internal static bool StageEngineFuelGaugeFillBackgroundColor_replace = false;
		internal static Color StageEngineFuelGaugeFillBackgroundColor_color;
		internal static bool StageEngineHeatGaugeFillBackgroundColor_replace = false;
		internal static Color StageEngineHeatGaugeFillBackgroundColor_color;

		[HarmonyPatch(typeof(ProtoStageIconInfo), nameof(ProtoStageIconInfo.SetProgressBarBgColor))]
		class Patch12_4
		{
			static bool Prefix(ref Color c)
			{
				if (StageEngineFuelGaugeFillBackgroundColor_replace)
				{
					// Engine & RCS gauge
					if (c == XKCDColors.DarkLime.A(0.6f))
					{
						c = StageEngineFuelGaugeFillBackgroundColor_color;
						return true;
					}
				}
				if (StageEngineHeatGaugeFillBackgroundColor_replace)
				{
					// Heat gauge
					if (c == XKCDColors.DarkRed.A(0.6f))
					{
						c = StageEngineHeatGaugeFillBackgroundColor_color;
						return true;
					}
				}
				return true;
			}
		}

		internal static bool VerticalSpeedGaugeNeedleColorReplace = false;
		internal static Color VerticalSpeedGaugeNeedleColor;

		[HarmonyPatch(typeof(VerticalSpeedGauge), nameof(VerticalSpeedGauge.Start))]
		class Patch13
		{
			static void Postfix(ref VerticalSpeedGauge __instance)
			{
				if (VerticalSpeedGaugeNeedleColorReplace)
				{
					__instance.gauge.pointer.gameObject.GetComponentInChildren<Image>().color = VerticalSpeedGaugeNeedleColor;
				}
			}
		}

		internal static bool ManeuverNodeEditorTextColor_replace = false;
		internal static Color ManeuverNodeEditorTextColor;

		[HarmonyPatch(typeof(ManeuverNodeEditorTabOrbitBasic), "Start")]
		class Patch14
		{
			static void Postfix(ref TextMeshProUGUI ___apoapsisAltitude, ref TextMeshProUGUI ___apoapsisTime, ref TextMeshProUGUI ___periapsisAltitude, ref TextMeshProUGUI ___periapsisTime, ref TextMeshProUGUI ___orbitPeriod)
			{
				if (ManeuverNodeEditorTextColor_replace)
				{
					___apoapsisAltitude.color = ManeuverNodeEditorTextColor;
					___apoapsisTime.color = ManeuverNodeEditorTextColor;
					___periapsisAltitude.color = ManeuverNodeEditorTextColor;
					___periapsisTime.color = ManeuverNodeEditorTextColor;
					___orbitPeriod.color = ManeuverNodeEditorTextColor;
				}
			}
		}

		[HarmonyPatch(typeof(ManeuverNodeEditorTabOrbitAdv), "Start")]
		class Patch14_1
		{
			static void Postfix(ref TextMeshProUGUI ___orbitArgumentOfPeriapsis, ref TextMeshProUGUI ___orbitLongitudeOfAscendingNode, ref TextMeshProUGUI ___ejectionAngle, ref TextMeshProUGUI ___orbitEccentricity, ref TextMeshProUGUI ___orbitInclination)
			{
				if (ManeuverNodeEditorTextColor_replace)
				{
					___orbitArgumentOfPeriapsis.color = ManeuverNodeEditorTextColor;
					___orbitLongitudeOfAscendingNode.color = ManeuverNodeEditorTextColor;
					___ejectionAngle.color = ManeuverNodeEditorTextColor;
					___orbitEccentricity.color = ManeuverNodeEditorTextColor;
					___orbitInclination.color = ManeuverNodeEditorTextColor;
				}
			}
		}

		[HarmonyPatch(typeof(ManeuverNodeEditorTabVectorInput), "Start")]
		class Patch14_2
		{
			static void Postfix(ref TMP_InputField ___proRetrogradeField, ref TMP_InputField ___normalField, ref TMP_InputField ___radialField, ref TMP_InputField ___timeField)
			{
				if (ManeuverNodeEditorTextColor_replace)
				{
					___proRetrogradeField.textComponent.color = ManeuverNodeEditorTextColor;
					___normalField.textComponent.color = ManeuverNodeEditorTextColor;
					___radialField.textComponent.color = ManeuverNodeEditorTextColor;
					___timeField.textComponent.color = ManeuverNodeEditorTextColor;
				}
			}
		}

		[HarmonyPatch(typeof(ManeuverNodeEditorTabVectorHandles), "Start")]
		class Patch14_3
		{
			static void Postfix(ref TextMeshProUGUI ___sliderTimeDVString)
			{
				if (ManeuverNodeEditorTextColor_replace)
				{
					___sliderTimeDVString.color = ManeuverNodeEditorTextColor;
				}
			}
		}

		internal static string GaugeGeeFilePath = "";
		internal static string GaugeThrottleFilePath = "";

		[HarmonyPatch(typeof(NavBall), "Start")]
		class Patch15
		{
			static void Postfix(ref NavBall __instance)
			{
				if(GaugeGeeFilePath != "")
				{
					Texture2D tex = (Texture2D)__instance.sideGaugeGee.mainTexture;
					ImageConversion.LoadImage(tex, File.ReadAllBytes(GaugeGeeFilePath));
				}
				if(GaugeThrottleFilePath != "")
				{
					Texture2D tex = (Texture2D)__instance.sideGaugeThrottle.mainTexture;
					ImageConversion.LoadImage(tex, File.ReadAllBytes(GaugeThrottleFilePath));
				}
			}
		}

		internal static bool SASDisplayColor_SAS_Replace_On = false;
		internal static bool SASDisplayColor_SAS_Replace_Off = false;
		internal static bool RCSDisplayColor_RCS_Replace_On = false;
		internal static bool RCSDisplayColor_RCS_Replace_Off = false;
		internal static Color SASDisplayColor_SAS_On_color;
		internal static Color SASDisplayColor_SAS_Off_color;
		internal static Color RCSDisplayColor_RCS_On_color;
		internal static Color RCSDisplayColor_RCS_Off_color;

		[HarmonyPatch(typeof(SASDisplay), "Start")]
		class Patch16
		{
			static void Postfix(ref SASDisplay __instance)
			{
				UIStateText.TextState[] states = __instance.stateText.states;
				if(states == null)
				{
					Debug.LogError("HUDReplacer: no states found for SASDisplay.stateText.states");
					return;
				}
				int num = states.Length;
				while (num-- > 0)
				{
					if (states[num].name == "On")
					{
						if (SASDisplayColor_SAS_Replace_On)
						{
							states[num].textColor = SASDisplayColor_SAS_On_color;
						}
					}
					if (states[num].name == "Off")
					{
						if (SASDisplayColor_SAS_Replace_Off)
						{
							states[num].textColor = SASDisplayColor_SAS_Off_color;
						}
					}
				}
			}
		}
		[HarmonyPatch(typeof(RCSDisplay), "Start")]
		class Patch16_2
		{
			static void Postfix(ref RCSDisplay __instance)
			{
				UIStateText.TextState[] states = __instance.stateText.states;
				if (states == null)
				{
					Debug.LogError("HUDReplacer: no states found for RCSDisplay.stateText.states");
					return;
				}
				int num = states.Length;
				while (num-- > 0)
				{
					if (states[num].name == "On")
					{
						if (RCSDisplayColor_RCS_Replace_On)
						{
							states[num].textColor = RCSDisplayColor_RCS_On_color;
						}
					}
					if (states[num].name == "Off")
					{
						if (RCSDisplayColor_RCS_Replace_Off)
						{
							states[num].textColor = RCSDisplayColor_RCS_Off_color;
						}
					}
				}
			}
		}

		internal static bool EditorCategoryButtonColor_replace = false;
		internal static bool EditorCategoryButtonColor_Module_replace = false;
		internal static bool EditorCategoryButtonColor_Resource_replace = false;
		internal static bool EditorCategoryButtonColor_Manufacturer_replace = false;
		internal static bool EditorCategoryButtonColor_Tech_replace = false;
		internal static bool EditorCategoryButtonColor_Profile_replace = false;
		internal static bool EditorCategoryButtonColor_Subassembly_replace = false;
		internal static bool EditorCategoryButtonColor_Variants_replace = false;
		internal static bool EditorCategoryButtonColor_Custom_replace = false;
		internal static Color EditorCategoryButtonColor_color;
		internal static Color EditorCategoryButtonColor_Module_color;
		internal static Color EditorCategoryButtonColor_Resource_color;
		internal static Color EditorCategoryButtonColor_Manufacturer_color;
		internal static Color EditorCategoryButtonColor_Tech_color;
		internal static Color EditorCategoryButtonColor_Profile_color;
		internal static Color EditorCategoryButtonColor_Subassembly_color;
		internal static Color EditorCategoryButtonColor_Variants_color;
		internal static Color EditorCategoryButtonColor_Custom_color;

		[HarmonyPatch(typeof(PartCategorizer), "Setup")]
		class Patch17 {
			static void Prefix(ref PartCategorizer __instance)
			{
				if (EditorCategoryButtonColor_replace)
				{
					__instance.colorFilterFunction = EditorCategoryButtonColor_color;
				}
				if (EditorCategoryButtonColor_Module_replace)
				{
					__instance.colorFilterModule = EditorCategoryButtonColor_Module_color;
				}
				if (EditorCategoryButtonColor_Resource_replace)
				{
					__instance.colorFilterResource = EditorCategoryButtonColor_Resource_color;
				}
				if (EditorCategoryButtonColor_Manufacturer_replace)
				{
					__instance.colorFilterManufacturer = EditorCategoryButtonColor_Manufacturer_color;
				}
				if (EditorCategoryButtonColor_Tech_replace)
				{
					__instance.colorFilterTech = EditorCategoryButtonColor_Tech_color;
				}
				if (EditorCategoryButtonColor_Profile_replace)
				{
					__instance.colorFilterProfile = EditorCategoryButtonColor_Profile_color;
				}
				if (EditorCategoryButtonColor_Subassembly_replace)
				{
					__instance.colorSubassembly = EditorCategoryButtonColor_Subassembly_color;
				}
				if (EditorCategoryButtonColor_Variants_replace)
				{
					__instance.colorVariants = EditorCategoryButtonColor_Variants_color;
				}
				if (EditorCategoryButtonColor_Custom_replace)
				{
					__instance.colorCategory = EditorCategoryButtonColor_Custom_color;
				}
			}
		}
		/*
		Perhaps at some point we might tackle orbital lines
		
		[HarmonyPatch(typeof(OrbitRenderer), "GetNodeColour")]
		class Patch15
		{
			static void Postfix()
			{
				Debug.Log("test");
			}
		}

		[HarmonyPatch(typeof(OrbitRenderer), "GetOrbitColour")]
		class Patch15_1
		{
			static void Postfix(ref OrbitRenderer __instance)
			{
				Debug.Log("test");
			}
		}
		*/
	}


}
