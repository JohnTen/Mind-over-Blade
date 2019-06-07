using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtility;

public enum KnifeState
{
	InSheath,
	Flying,
	Hover,
	Stuck,
}

public class Knife : BaseWeapon
{
	[SerializeField] Sheath sheath;
	[SerializeField] float bladeLength;
	[SerializeField] float speed;
	[SerializeField] float actionDelay;
	[SerializeField] float backDistance;
	[SerializeField] float sinkingLength;
	[SerializeField] float hoveringDistance;
	[SerializeField] float hoveringRotateSpeed;
	[SerializeField] float hoveringDuration;
	[SerializeField] float dragForce;
	[Space(30)]
	[SerializeField] float traveledDistance;
	[SerializeField] bool changedDirection;
	[SerializeField] bool hovered;
	[SerializeField] bool returning;
	[SerializeField] float actionTimer;
	[SerializeField] float hoverTimer;
	[SerializeField] KnifeState state;
	[SerializeField] Enemy hittedEnemy;
	[SerializeField] bool piercing;

	bool activated;
	AttackPackage basicAttack;

	public KnifeState State
	{
		get { return state; }
		set { state = value; }
	}

	public Enemy HittedEnemy => hittedEnemy;

	public bool Hovered => hovered;

	public bool StuckedOnClimbable { get; private set; }

	public bool IsReturning => returning;

	private void Update()
	{
		if (actionTimer > 0)
			actionTimer -= Time.deltaTime;

		if (state == KnifeState.Flying)
		{
			if (returning)
				Returning();
			else
				Flying();
		}

		if (state == KnifeState.Hover)
		{
			Hovering();
		}
	}

	public void SetSheath(Sheath sheath)
	{
		this.sheath = sheath;
	}

	public bool Launch(Vector3 direction, bool isPiercing = false)
	{
		if (state != KnifeState.InSheath) return false;

		piercing = isPiercing;
		actionTimer = actionDelay;
		state = KnifeState.Flying;

		transform.position = sheath.LaunchPosition.position;
		transform.right = direction;

		basicAttack = AttackPackage.CreateNewPackage();

		return true;
	}

	public bool Hover()
	{
		if (hovered ||
			returning ||
			state != KnifeState.Flying ||
			actionTimer > 0) return false;

		piercing = false;
		hovered = true;
		actionTimer = actionDelay;
		state = KnifeState.Hover;
		
		basicAttack = AttackPackage.CreateNewPackage();

		return true;
	}

	public bool Withdraw()
	{
		if (state == KnifeState.InSheath || actionTimer > 0 || returning)
			return false;

		returning = true;
		actionTimer = actionDelay;

		transform.parent = null;
		state = KnifeState.Flying;

		if (hittedEnemy != null)
		{
			var dir = (sheath.transform.position - transform.position).normalized;
			hittedEnemy.Hit(dir, 0.8f);
			hittedEnemy.AddForce(dir * dragForce);
		}

		basicAttack = AttackPackage.CreateNewPackage();
		hittedEnemy = null;
		Returning();

		return true;
	}

	private void Flying()
	{
		RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, speed * Time.deltaTime);
		transform.position += transform.right * speed * Time.deltaTime;
		traveledDistance += speed * Time.deltaTime;

		if (hit.collider != null)
		{
			print(hit.collider.name);
			var attackable = hit.collider.GetComponent<IAttackable>();

			if (hit.collider.tag == "Climable")
			{
				StuckedOnClimbable = true;
				state = KnifeState.Stuck;
				transform.position = (Vector3)hit.point + transform.right * sinkingLength;
				return;
			}
			else if (hit.collider.tag == "Player") { }
			else if (attackable != null)
			{
				print("Enemy fly");
				var attack = Process(basicAttack);
				attack._fromDirection = hit.collider.transform.position - transform.position;
				RaiseOnHitEvent(attackable, attackable.ReceiveAttack(ref attack), attack);
			}
			else
			{
				transform.position = hit.point;
				Hover();
				return;
			}
		}

		if (traveledDistance >= hoveringDistance)
			Hover();
	}

	private void Returning()
	{
		var dir = sheath.transform.position - transform.position;

		if (dir.sqrMagnitude <= backDistance * backDistance)
		{
			sheath.PutBackKnife(this);
			state = KnifeState.InSheath;

			hoverTimer = 0;
			returning = false;
			changedDirection = false;
			hovered = false;
			StuckedOnClimbable = false;
			traveledDistance = 0;
			return;
		}

		transform.right = dir;

		Debug.DrawRay(transform.position, transform.right * speed * Time.deltaTime, Color.red);

		RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, speed * Time.deltaTime);
		if (hit.collider != null)
		{
			var attackable = hit.collider.GetComponent<IAttackable>();

			if (attackable != null)
			{
				print("Enemy return");
				var attack = Process(basicAttack);
				attack._fromDirection = hit.collider.transform.position - transform.position;
				RaiseOnHitEvent(attackable, attackable.ReceiveAttack(ref attack), attack);
			}
		}

		transform.position += transform.right * speed * Time.deltaTime;
	}

	private void Hovering()
	{
		hoverTimer += Time.deltaTime;
		transform.Rotate(0, 0, hoveringRotateSpeed * Time.deltaTime);

		if (hoverTimer > hoveringDuration)
			Withdraw();

		Debug.DrawRay(transform.position, transform.right * bladeLength, Color.red);
		RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, bladeLength);
		if (hit.collider != null)
		{
			var attackable = hit.collider.GetComponent<IAttackable>();

			if (attackable != null)
			{
				print("Enemy hover");
				var attack = Process(basicAttack);
				attack._fromDirection = hit.collider.transform.position - transform.position;
				RaiseOnHitEvent(attackable, attackable.ReceiveAttack(ref attack), attack);
			}
		}
	}

	public override void Activate(AttackPackage attack)
	{
		basicAttack = attack;
		activated = true;
	}

	public override void Deactivate()
	{
		activated = false;
	}

	public override AttackPackage Process(AttackPackage target)
	{
		target._isMeleeAttack = false;
		target._isChargedAttack = false;
		target._hitPointDamage += _baseHitPointDamage;
		target._enduranceDamage += _baseEnduranceDamage;
		target._hitBackDistance += 0.7f;

		return target;
	}
}
