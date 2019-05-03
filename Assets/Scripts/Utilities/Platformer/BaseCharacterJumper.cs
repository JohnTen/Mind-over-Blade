using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUtility.Platformer
{
	public abstract class BaseCharacterJumper : MonoBehaviour
	{
		[SerializeField] protected float jumpHeight = 3;

		protected virtual bool GetJumpingCommand()
		{
			return Input.GetButton("Jump");
		}

		protected virtual bool IsJumpable(Vector3 vector)
		{
			return true;
		}

		protected abstract void Jumping();
	}
}
