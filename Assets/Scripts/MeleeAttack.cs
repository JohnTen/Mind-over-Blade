using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtility;

public class MeleeAttack : MonoBehaviour
{
	[SerializeField] BoxCollider2D meleeZone;
	[SerializeField] float meleeAttackRate;
	[SerializeField, MinMaxSlider(0, 1)] Vector2 meleeAttackDuration;
	[SerializeField] Animator meleeAnimator;
	[SerializeField] Transform attackShape;
	[SerializeField] float bladeLength;
	[Space(30)]
	[SerializeField] float meleeAttackTimer;
	[SerializeField] Vector3 lastAttackPosition;
	[SerializeField] GameObject attackedEnemy;

	private void LateUpdate()
	{
		if (meleeAttackTimer > 0)
		{
			meleeAttackTimer -= Time.deltaTime * meleeAttackRate;
		}

		if (Input.GetKey(KeyCode.F) && meleeAttackTimer <= 0)
		{
			meleeAnimator.SetBool("Attack", true);
			meleeAttackTimer = 1;
			lastAttackPosition = attackShape.position + attackShape.right * bladeLength;
		}
		else
		{
			meleeAnimator.SetBool("Attack", false);
		}

		if (meleeAttackDuration.IsIncluded(meleeAttackTimer))
		{
			var bladeTip = attackShape.position + attackShape.right * bladeLength;
			var hits = Physics2D.RaycastAll(attackShape.position, attackShape.right, bladeLength);
			Debug.DrawRay(attackShape.position, attackShape.right * bladeLength, Color.red);
			foreach (var hit in hits)
			{
				if (hit.collider.tag == "Enemy" && attackedEnemy != hit.collider.gameObject)
				{
					hit.collider.GetComponent<Enemy>().Hit(-hit.normal, 2);
					attackedEnemy = hit.collider.gameObject;
				}
			}

			var toLastPos = lastAttackPosition - bladeTip;
			hits = Physics2D.RaycastAll(bladeTip, toLastPos, toLastPos.magnitude);
			Debug.DrawRay(bladeTip, toLastPos, Color.red);
			foreach (var hit in hits)
			{
				if (hit.collider.tag == "Enemy" && attackedEnemy != hit.collider.gameObject)
				{
					hit.collider.GetComponent<Enemy>().Hit(-hit.normal, 2);
					attackedEnemy = hit.collider.gameObject;
				}
			}

			lastAttackPosition = bladeTip;
		}

		if (meleeAttackTimer <= 0)
		{
			attackedEnemy = null;
		}
	}
}
