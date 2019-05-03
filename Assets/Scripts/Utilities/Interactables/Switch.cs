using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUtility.Interactables
{
	public class Switch : MonoInteractable
	{
		public override void StartInteracting()
		{
			SetActiveStatus(!isActivated);

			base.StartInteracting();
		}
	}
}
