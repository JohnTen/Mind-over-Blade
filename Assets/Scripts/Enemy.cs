using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtility;
using UnityUtility.Platformer;

[RequireComponent(typeof(PhysicalJumper))]
public class Enemy : PhysicalMover
{
	[SerializeField] int hitPoint;
	[SerializeField] float hitBackForce;
	[SerializeField] float stunDuration;
	[SerializeField] Transform player;
	[SerializeField] SpriteRenderer sprite;
	[SerializeField, MinMaxSlider(0, 5)] Vector2 flashRed;
	[SerializeField, MinMaxSlider(0, 5)] Vector2 actionChange;
	[SerializeField] int state;
	[SerializeField] float stateChangeTimer;
	[SerializeField] bool stuned;

	public bool Stuned => stuned;
	public int State => state;

	public void Hit(Vector3 hitDirection)
	{
		if (stuned) return;

		hitPoint--;
		if (hitPoint <= 0)
			Destroy(gameObject);

		hitDirection.x = hitDirection.x > 0 ? 1 : -1;
		hitDirection.y = 1;
		hitDirection.z = 0;

		stuned = true;
		StartCoroutine(BeenHit());
		rigidBody.AddForce(hitDirection * hitBackForce, ForceMode2D.Impulse);
	}

	private void Update()
	{
		stateChangeTimer -= Time.deltaTime;
		if (stateChangeTimer <= 0)
		{
			state = RandomState();
			stateChangeTimer = actionChange.RandomBetween();
		}
	}

	public void AddForce(Vector2 force)
	{
		rigidBody.AddForce(force, ForceMode2D.Impulse);
	}

	protected override void Moving(Vector3 vector)
	{
		if (stuned) return;

		if (vector.x > 0)
		{
			sprite.transform.rotation = Quaternion.Euler(0, 0, 0);
		}
		else if (vector.x < 0)
		{
			sprite.transform.rotation = Quaternion.Euler(0, 180, 0);
		}

		base.Moving(vector);
	}

	protected override Vector3 GetMovingDirection()
	{
		if (stuned)
			return Vector3.zero;

		var dir = player.position - transform.position;
		switch (state)
		{
			case 0:
				dir.z = dir.y = 0;
				dir.x = dir.x > 0 ? 1 : -1;
				break;
			case 1:
				dir.z = dir.y = 0;
				dir.x = dir.x > 0 ? -1 : 1;
				break;
			case 2:
				dir = Vector3.zero;
				break;
		}

		return dir;
	}

	int RandomState()
	{
		return Random.Range(0, 3);
	}

	IEnumerator BeenHit()
	{
		float time = stunDuration;

		while (time > 0)
		{
			time -= Time.deltaTime;
			if (flashRed.IsIncluded(stunDuration - time))
			{
				sprite.color = Color.red;
			}
			else
			{
				sprite.color = Color.white;
			}
			yield return null;
		}

		stuned = false;
	}
}
