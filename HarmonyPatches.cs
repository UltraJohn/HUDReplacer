using Expansions.Serenity;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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


		// PAW Title bar patch
		internal static bool PAWTitleBar_replace = false;
		internal static Color PAWTitleBar_color;

		[HarmonyPatch(typeof(UIPartActionController), "CreatePartUI")]
		class Patch1
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
		[HarmonyPatch(typeof(RoboticControllerWindow), nameof(RoboticControllerWindow.Spawn))]
		class Patch2
		{
			static void Postfix(ref RoboticControllerWindow __result)
			{
				if (!__result) return;
				Image[] image_array = __result.gameObject.GetComponentsInChildren<Image>();
				List<Texture2D> textures = new List<Texture2D>();
				foreach (Image img in image_array)
				{
					textures.Add((Texture2D)img.mainTexture);
				}
				Texture2D[] tex_array = textures.ToArray();
				if (tex_array.Length > 0) HUDReplacer.instance.ReplaceTextures(tex_array);
			}
		}
	}


}
