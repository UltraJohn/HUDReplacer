using Expansions.Serenity;
using HarmonyLib;
using KSP.UI;
using KSP.UI.Screens;
using KSP.UI.Screens.Flight;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
		internal static Color StageGroupDeltaVTextColor = Color.white;
		internal static Color StageGroupDeltaVNumberColor = Color.white;

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
				}
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

	}


}
