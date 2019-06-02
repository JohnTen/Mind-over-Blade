using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAnimationEventHandler : MonoBehaviour
{
	[SerializeField] PlayerMover mover;
	[SerializeField] BaseWeapon weapon;

    public void StepForward(float stepLength)
	{
		mover.StepForward(stepLength);
	}

	public void SetStepSpeed(float speed)
	{
		mover.SetStepSpeed(speed);
	}

	public void Activate()
	{
		AttackPackage attack = AttackPackage.CreateNewPackage();
		weapon.Activate(attack);
	}

	public void Deactivate()
	{
		weapon.Deactivate();
	}
}
