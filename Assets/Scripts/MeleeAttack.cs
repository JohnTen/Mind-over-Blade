using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtility;

public class MeleeAttack : MonoBehaviour
{
	[SerializeField] float meleeAttackRate;
	[SerializeField, MinMaxSlider(0, 1)] Vector2 meleeAttackDuration;
	[SerializeField] Animator meleeAnimator;
	[SerializeField] Transform bladeStart;
	[SerializeField] Transform bladeEnd;
	[Space(30)]
	[SerializeField] float meleeAttackTimer;
	[SerializeField] Vector3 lastAttackPositionStart;
	[SerializeField] Vector3 lastAttackPositionEnd;
	[SerializeField] GameObject attackedEnemy;
	[SerializeField] PlayerMover player;
	[SerializeField] PlayerAnimation animation;

	PlayerInput input;
	[SerializeField] bool repeatAttack;

	private void Awake()
	{
		input = GetComponent<PlayerInput>();
	}

	private void LateUpdate()
	{
		if (player.Frozen && meleeAttackTimer < 0)
			return;

		if (meleeAttackTimer > 0)
		{
			meleeAttackTimer -= Time.deltaTime * meleeAttackRate;
			if (meleeAttackTimer <= 0)
			{
				player.Frozen = false;
			}

			if (input.IsMeleePressed())
				repeatAttack = true;
		}

		if ((input.IsMeleePressed() || repeatAttack) && meleeAttackTimer <= 0)
		{
			repeatAttack = false;
			player.Frozen = true;
			animation.Attack();
			//meleeAnimator.SetBool("Attack", true);
			meleeAttackTimer = 1;
			lastAttackPositionStart = bladeStart.position;
			lastAttackPositionEnd = bladeEnd.position;
		}
		else
		{
			//meleeAnimator.SetBool("Attack", false);
		}

		if (meleeAttackDuration.IsIncluded(meleeAttackTimer))
		{
			var toBladeTop = bladeEnd.position - bladeStart.position;
			var hits = Physics2D.RaycastAll(bladeStart.position, toBladeTop, toBladeTop.magnitude);
			Debug.DrawRay(bladeStart.position, toBladeTop, Color.red);
			foreach (var hit in hits)
			{
				if (hit.collider.tag == "Enemy" && attackedEnemy != hit.collider.gameObject)
				{
					hit.collider.GetComponent<Enemy>().Hit(-hit.normal, 2);
					attackedEnemy = hit.collider.gameObject;
				}
			}

			ExtraHitTest();
		}

		if (meleeAttackTimer <= 0)
		{
			attackedEnemy = null;
		}
	}

	private void ExtraHitTest()
	{
		var toLastPos = lastAttackPositionEnd - bladeEnd.position;
		var hits = Physics2D.RaycastAll(bladeEnd.position, toLastPos, toLastPos.magnitude);
		Debug.DrawRay(bladeEnd.position, toLastPos, Color.red);
		foreach (var hit in hits)
		{
			if (hit.collider.tag == "Enemy" && attackedEnemy != hit.collider.gameObject)
			{
				hit.collider.GetComponent<Enemy>().Hit(-hit.normal, 2);
				attackedEnemy = hit.collider.gameObject;
			}
		}

		toLastPos = lastAttackPositionStart - bladeStart.position;
		hits = Physics2D.RaycastAll(bladeStart.position, toLastPos, toLastPos.magnitude);
		Debug.DrawRay(bladeStart.position, toLastPos, Color.red);
		foreach (var hit in hits)
		{
			if (hit.collider.tag == "Enemy" && attackedEnemy != hit.collider.gameObject)
			{
				hit.collider.GetComponent<Enemy>().Hit(-hit.normal, 2);
				attackedEnemy = hit.collider.gameObject;
			}
		}

		toLastPos = lastAttackPositionStart - bladeEnd.position;
		hits = Physics2D.RaycastAll(bladeEnd.position, toLastPos, toLastPos.magnitude);
		Debug.DrawRay(bladeEnd.position, toLastPos, Color.red);
		foreach (var hit in hits)
		{
			if (hit.collider.tag == "Enemy" && attackedEnemy != hit.collider.gameObject)
			{
				hit.collider.GetComponent<Enemy>().Hit(-hit.normal, 2);
				attackedEnemy = hit.collider.gameObject;
			}
		}

		toLastPos = lastAttackPositionEnd - bladeStart.position;
		hits = Physics2D.RaycastAll(bladeStart.position, toLastPos, toLastPos.magnitude);
		Debug.DrawRay(bladeStart.position, toLastPos, Color.red);
		foreach (var hit in hits)
		{
			if (hit.collider.tag == "Enemy" && attackedEnemy != hit.collider.gameObject)
			{
				hit.collider.GetComponent<Enemy>().Hit(-hit.normal, 2);
				attackedEnemy = hit.collider.gameObject;
			}
		}

		lastAttackPositionStart = bladeStart.position;
		lastAttackPositionEnd = bladeEnd.position;
	}
}
