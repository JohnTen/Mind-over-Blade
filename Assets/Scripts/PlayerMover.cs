using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtility;
using UnityUtility.Platformer;

public class PlayerMover : PhysicalMover
{
	[SerializeField] float airSpeed;
	[SerializeField] float speedSmoothing;
	[SerializeField] SpriteRenderer sprite;
	[SerializeField] ContactGroundDetector groundDetector;
	[SerializeField] bool dashing;

	public event Action<bool> OnChangeDirection;
	public bool FacingRight => sprite.transform.eulerAngles.y == 0;

	protected override void Awake()
	{
		base.Awake();
		var jumper = GetComponent<PlayerJumper>();
		jumper.OnDashingBegin += Jumper_OnDashingBegin;
		jumper.OnDashingEnd += Jumper_OnDashingEnd;
	}

	private void Jumper_OnDashingEnd()
	{
		rigidBody.isKinematic = false;
	}

	private void Jumper_OnDashingBegin(Vector3 direction)
	{
		rigidBody.isKinematic = true;
		rigidBody.velocity = Vector2.zero;
	}

	protected override Vector3 GetMovingDirection()
	{
		var dir = base.GetMovingDirection();
		
		if (dir.x != 0)
		{
			OnChangeDirection.Invoke(dir.x > 0);
			sprite.transform.rotation = Quaternion.Euler(0, dir.x > 0? 0: 180, 0);
		}


		return dir;
	}

	protected override void Moving(Vector3 vector)
	{
		var vel = rigidBody.velocity;

		vector *= Time.deltaTime;
		
		if (groundDetector.OnGround && Vector3.Dot(groundDetector.Normal, vel.normalized) < 0.2f)
		{
			vel.x = vector.x * moveSpeed;
		}
		else if (!groundDetector.OnGround)
		{
			vel.x += vector.x * airSpeed;
		}
		
		vel.y += vector.y;
		rigidBody.velocity = vel;
	}
}
