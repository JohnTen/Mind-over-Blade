using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtility;
using UnityUtility.Platformer;

public class PlayerJumper : PhysicalJumper
{
	[SerializeField] float dashingDistance;
	[SerializeField] float dashingDuration;
	[SerializeField] Vector2 direction;
	public event Action<Vector3> OnDashingBegin;
	public event Action OnDashingEnd;
	public event Action<float> OnJump;
	public bool dashing;

	PlayerInput input;
	Vector2 MoveCommond;

	protected override void Awake()
	{
		base.Awake();
		input = GetComponent<PlayerInput>();
	}

	protected override bool GetJumpingCommand()
	{
		if (!input.IsJumpPressed())
			return false;

		MoveCommond = input.GetMovingDirection();
		return true;
	}

	protected override void Jumping()
	{
		MoveCommond = new Vector2(Mathf.Round(MoveCommond.x), Mathf.Round(MoveCommond.y));
		direction = MoveCommond;
		if (groundDetector.OnGround)
		{
			base.Jumping();
			OnJump?.Invoke(jumpHeight);
		}
	}

	IEnumerator Dashing()
	{
		float speed = dashingDistance / dashingDuration;
		float time = 0;


		MoveCommond = new Vector2(Mathf.Round(MoveCommond.x), Mathf.Round(MoveCommond.y));
		if (MoveCommond != Vector2.zero && !dashing)
		{
			OnDashingBegin?.Invoke(MoveCommond);
			dashing = true;
			MoveCommond.Normalize();

			while (time < dashingDuration)
			{
				time += Time.deltaTime;
				print(MoveCommond * Time.deltaTime * speed);
				transform.position += (Vector3)(MoveCommond * Time.deltaTime * speed);
				yield return null;
			}

			OnDashingEnd?.Invoke();
			dashing = false;
		}

	}
}
