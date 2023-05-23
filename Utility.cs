using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HUDReplacer
{
	internal static class Utility
	{
		public static void Invoke(this MonoBehaviour mb, Action f, float delay)
		{
			mb.StartCoroutine(InvokeRoutine(f, delay));
		}

		private static IEnumerator InvokeRoutine(Action f, float delay)
		{
			yield return new WaitForSeconds(delay);
			f();
		}
	}
}
