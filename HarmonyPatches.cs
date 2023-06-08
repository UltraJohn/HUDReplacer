using Expansions.Serenity;
using HarmonyLib;
using KSP.UI;
using KSP.UI.Screens.Flight;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;

namespace HUDReplacer
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class HarmonyPatches : MonoBehaviour
	{
		public void Awake()
		{
			// NOTE: A Harmony patcher should be placed in a run once Startup addon. The patch is kept between scene changes.
			var harmony = new Harmony("UltraJohn.Mods.HUDReplacer");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}


		// Tumbler colors
		internal static bool TumblerColorsLoaded = false;
		internal static bool TumblerColorReplacePositive = false;
		internal static bool TumblerColorReplaceNegative = false;
		internal static Color TumblerColorPositive = Color.black;
		internal static Color TumblerColorNegative = new Color(0.6f, 0f, 0f);

		[HarmonyPatch(typeof(KSP.UI.Screens.Tumbler), "Awake")]
		class Patch1
		{
			static void Prefix(ref Color ___positiveColor, ref Color ___negativeColor)
			{
				if (!TumblerColorsLoaded)
				{
					HUDReplacer.LoadTumblerColors();
				}
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

		
		[HarmonyPatch(typeof(UIPlanetariumDateTime), "Start")]
		class Patch7
		{
			static void Postfix(ref UIPlanetariumDateTime __instance)
			{
				__instance.textDate.color = METDisplayColorGreen;
			}
		}
		

		[HarmonyPatch(typeof(UIPlanetariumDateTime), "onGameUnPause")]
		class Patch8
		{
			static void Postfix(ref UIPlanetariumDateTime __instance)
			{
				__instance.textDate.color = METDisplayColorGreen;
			}
		}


		/*
		// GaugePitchPointer, GaugeRollPointer, GaugeYAW

		// Vertical Speed Gauge
		// GaugeNeedle & GaugeKnob


		[HarmonyPatch(typeof(KSP.UI.Screens.LinearGauge), "Awake")]
		class Patch4
		{
			static void Prefix(ref KSP.UI.Screens.LinearGauge __instance)
			{
				if (!__instance) return;
				if(__instance.gameObject.name == "GaugeVerticalSpeed11111111111111111111111")
				{
					try
					{
						Image[] images = __instance.gameObject.GetComponentsInChildren<Image>();
						foreach (Image img in images)
						{
							if (img.mainTexture.name == "GaugeKnob")
							{
								img.color = new Color(1,0,0,1);
							}
							if (img.mainTexture.name == "GaugePointer")
							{
								img.color = new Color(0,1,0,1);
							}
						}
					}
					catch (Exception e)
					{
						Debug.Log(e.ToString());
					}
				}
			}
		}
		*/
	}


}
