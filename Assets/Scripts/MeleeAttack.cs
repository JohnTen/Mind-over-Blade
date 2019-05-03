using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtility;

public class MeleeAttack : MonoBehaviour
{
	[SerializeField] float meleeAttackRate;
	[SerializeField, MinMaxSlider(0, 1)] Vector2 meleeAttackDuration;
	[SerializeField] Animator meleeAnimator;
	[SerializeField] Transform attackShape;
	[SerializeField] float bladeLength;
	[Space(30)]
	[SerializeField] float meleeAttackTimer;

	private void Update()
	{
		if (meleeAttackTimer > 0)
		{
			meleeAttackTimer -= Time.deltaTime * meleeAttackRate;
		}

		if (Input.GetKey(KeyCode.F) && meleeAttackTimer <= 0)
		{
			meleeAnimator.SetBool("Attack", true);
			meleeAttackTimer = 1;
		}
		else
		{
			meleeAnimator.SetBool("Attack", false);
		}

		if (meleeAttackDuration.IsIncluded(meleeAttackTimer))
		{
			var hits = Physics2D.RaycastAll(attackShape.position, attackShape.right, bladeLength);
			Debug.DrawRay(attackShape.position, attackShape.right * bladeLength, Color.red);
			foreach (var hit in hits)
			{
				if (hit.collider.tag == "Enemy")
				{
					hit.collider.GetComponent<Enemy>().Hit(-hit.normal);
				}
			}
		}
	}
}
