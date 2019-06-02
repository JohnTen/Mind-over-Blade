using System.Collections;
using System.Collections.Generic;
using UnityUtility;
using UnityEngine;

public class Sword : BaseWeapon
{
	[SerializeField] float _maxChargeAttack;
	[SerializeField] float _maxChargeTime;
	[SerializeField] AnimationCurve _chargeAttackMultiplierCurve;

	bool _activated;
	bool _charged;
	float _chargeAttackMultiplier;
	AttackPackage basePackage;

	public void Charge(float time)
	{
		_charged = true;
		_chargeAttackMultiplier = _chargeAttackMultiplierCurve.Evaluate(time / _maxChargeTime) * _maxChargeAttack;
	}

	public override void Activate(AttackPackage attack)
	{
		print("activate");
		basePackage = attack;
		_activated = true;
	}

	public override void Deactivate()
	{
		print("deactivate");
		_activated = false;
		_charged = false;
	}

	public override AttackPackage Process(AttackPackage target)
	{
		target._isMeleeAttack = true;
		target._isChargedAttack = _charged;
		target._hitPointDamage += _baseHitPointDamage;
		target._hitPointDamage *= _chargeAttackMultiplier;
		target._enduranceDamage += _baseEnduranceDamage;
		target._hitBackDistance += 2;

		return target;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		print(other.name);
		var attackable = other.GetComponent<IAttackable>();
		if (!_activated || attackable == null) return;

		var package = Process(basePackage);
		package._fromDirection = other.transform.position - this.transform.position;

		var result = attackable.ReceiveAttack(ref package);
		RaiseOnHitEvent(attackable, result, package);
	}
}
