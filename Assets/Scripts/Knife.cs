using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KnifeState
{
	InSheath,
	Flying,
	Hover,
	Stuck,
}

public class Knife : MonoBehaviour
{
	[SerializeField] Sheath sheath;
	[SerializeField] float speed;
	[SerializeField] float actionDelay;
	[SerializeField] float backDistance;
	[SerializeField] float sinkingLength;
	[SerializeField] float hoveringRotateSpeed;
	[SerializeField] float hoveringDuration;
	[SerializeField] float dragForce;
	[Space(30)]
	[SerializeField] bool changedDirection;
	[SerializeField] bool hovered;
	[SerializeField] bool returning;
	[SerializeField] float actionTimer;
	[SerializeField] float hoverTimer;
	[SerializeField] KnifeState state;
	[SerializeField] Enemy hittedEnemy;

	public KnifeState State
	{
		get { return state; }
		set { state = value; }
	}

	public Enemy HittedEnemy => hittedEnemy;

	public bool Hovered => hovered;

	public bool StuckedOnClimbable { get; private set; }

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

		if (state == KnifeState.Stuck)
		{
			Stuck();
		}
	}

	public void SetSheath(Sheath sheath)
	{
		this.sheath = sheath;
	}

	public bool Launch(Vector3 direction)
	{
		if (state != KnifeState.InSheath) return false;

		actionTimer = actionDelay;
		state = KnifeState.Flying;

		transform.position = sheath.LaunchPosition.position;
		transform.right = direction;

		return true;
	}

	public bool ChangeDirection(Vector3 direction)
	{
		if (changedDirection || 
			(state != KnifeState.Flying &&
			state != KnifeState.Hover))
			return false;
		
		changedDirection = true;
		actionTimer = actionDelay;
		state = KnifeState.Flying;

		transform.right = direction;

		return true;
	}

	public bool Hover()
	{
		if (hovered ||
			state != KnifeState.Flying ||
			actionTimer > 0) return false;

		hovered = true;
		actionTimer = actionDelay;
		state = KnifeState.Hover;
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
			hittedEnemy.Hit(dir);
			hittedEnemy.AddForce(dir * dragForce);
		}

		hittedEnemy = null;
		Returning();

		return true;
	}

	private void Flying()
	{
		RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, speed * Time.deltaTime);
		if (hit.collider != null)
		{
			transform.position = (Vector3)hit.point + transform.right * sinkingLength;
			if (hit.collider.tag == "Climable")
			{
				StuckedOnClimbable = true;
			}
			if (hit.collider.tag == "Enemy")
			{
				print("Enemy fly");
				hittedEnemy = hit.collider.GetComponent<Enemy>();
				hittedEnemy.Hit(-hit.normal);
				transform.parent = hittedEnemy.transform;
			}
			state = KnifeState.Stuck;
		}
		else
		{
			transform.position += transform.right * speed * Time.deltaTime;
		}
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
			return;
		}

		transform.right = dir;

		RaycastHit hit;
		if (Physics.Raycast(transform.position, transform.right, out hit, speed * Time.deltaTime))
		{
			if (hit.collider.tag == "Enemy")
			{
				print("Enemy return");
				var enemy = hit.collider.GetComponent<Enemy>();
				enemy.Hit(-hit.normal);
			}
		}
		else
		{
			transform.position += transform.right * speed * Time.deltaTime;
		}
	}

	private void Hovering()
	{
		hoverTimer += Time.deltaTime;
		transform.Rotate(0, 0, hoveringRotateSpeed * Time.deltaTime);

		if (hoverTimer > hoveringDuration)
			Withdraw();

		RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right);
		if (hit.collider != null)
		{
			if (hit.collider.tag == "Enemy")
			{
				print("Enemy hover");
				hittedEnemy = hit.collider.GetComponent<Enemy>();
				hittedEnemy.Hit(-hit.normal);
			}
		}
	}
	
	private void Stuck()
	{
		
	}
}
