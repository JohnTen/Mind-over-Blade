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
	public bool dashing;

	Vector2 MoveCommond;

	protected override bool GetJumpingCommand()
	{
		if (!base.GetJumpingCommand())
			return false;

		MoveCommond = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		return true;
	}

	protected override void Jumping()
	{
		MoveCommond = new Vector2(Mathf.Round(MoveCommond.x), Mathf.Round(MoveCommond.y));
		direction = MoveCommond;
		if (groundDetector.OnGround)
		{
			if (Mathf.Abs(MoveCommond.x) <= 0.1f || MoveCommond.y > 0.1f)
			{
				base.Jumping();
			}
			else
			{
				StartCoroutine(Dashing());
			}
		}
		else
		{
			if (MoveCommond == Vector2.zero)
				MoveCommond = GetComponent<PlayerMover>().FacingRight? Vector2.right : Vector2.left;
			StartCoroutine(Dashing());
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
