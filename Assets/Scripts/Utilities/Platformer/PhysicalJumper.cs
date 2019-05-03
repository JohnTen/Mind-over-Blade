using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUtility.Platformer
{
	[RequireComponent(typeof(Rigidbody2D))]
	[RequireComponent(typeof(BaseGroundDetector))]
	public class PhysicalJumper : BaseCharacterJumper
	{
		protected Rigidbody2D rigidBody;
		protected BaseGroundDetector groundDetector;

		protected override void Jumping()
		{
			if (groundDetector.OnGround)
			{
				var vel = rigidBody.velocity;

				// Concluded from S = Vi * t + 1/2 * a * t^2 and t = (Vf - Vi)/a
				vel.y = Mathf.Sqrt(19.62f * jumpHeight * rigidBody.gravityScale);
				rigidBody.velocity = vel;
			}
		}

		protected void Update()
		{
			if (GetJumpingCommand())
			{
				Jumping();
			}
		}

		protected virtual void Awake()
		{
			rigidBody = GetComponent<Rigidbody2D>();
			groundDetector = GetComponent<BaseGroundDetector>();
		}
	}
}
