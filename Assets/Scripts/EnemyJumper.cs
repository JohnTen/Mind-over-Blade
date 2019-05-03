using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtility.Platformer;

[RequireComponent(typeof(Enemy))]
public class EnemyJumper : PhysicalJumper
{
	[SerializeField] Enemy mover;
	[SerializeField] float jumpRate;

	void Start ()
	{
		mover = GetComponent<Enemy>();
	}

	protected override bool GetJumpingCommand()
	{
		if (!groundDetector.OnGround)
			return false;
		if (mover.Stuned || mover.State == 2)
			return false;

		return jumpRate > Random.value;
	}
}
