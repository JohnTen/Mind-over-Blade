using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtility;

public interface IAttackable
{
	AttackResult ReceiveAttack(ref AttackPackage attack);

	event Action<AttackPackage> OnHit;
}
