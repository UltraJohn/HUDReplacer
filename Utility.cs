using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace HUDReplacer
{
	internal static class Utility
	{
		internal static void Invoke(this MonoBehaviour mb, Action f, float delay)
		{
			mb.StartCoroutine(InvokeRoutine(f, delay));
		}

		private static IEnumerator InvokeRoutine(Action f, float delay)
		{
			yield return new WaitForSeconds(delay);
			f();
		}

		internal static Color ToRGBA(this string color)
		{
			string[] values = color.Split(',');
			return new Color(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
		}

		// Might be useful in the future, if it's possible to read textures from the game's data.
		// It's currently blocked by the ImportFormat IsReadable flag.
		// As it stands right now this is the problem:
		// "Texture is not readable. The texture memory cannot be accessed from scripts."
		private static void DumpTexture(Texture2D tex)
		{
			string path = KSPUtil.ApplicationRootPath + "GameData/HUDReplacer/PluginData/Dump";
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			File.WriteAllBytes(path, tex.EncodeToPNG());
		}
	}
}
