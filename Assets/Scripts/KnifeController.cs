using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeController : MonoBehaviour
{
	[SerializeField] float fireRate;
	[SerializeField] float withDrawHeldTime;
	[SerializeField] float pauseTimeDuration;
	[SerializeField] float hoverHeldTime;
	[SerializeField] float pullingForce;
	[SerializeField] float pullingForceLimit;
	[SerializeField] float autoPickupDistance;
	[SerializeField] float autoReturnDistance;
	[Space(30)]
	[SerializeField] float withDrawHeldTimer;
	[SerializeField] float hoverHeldTimer;
	[SerializeField] bool changedDirection;
	[SerializeField] float pausedTime;
	[SerializeField] Sheath sheath;
	[SerializeField] PlayerMover player;
	[SerializeField] Queue<Knife> knifesInAir;

	Vector3 pointerDownPosition;

	private void Awake()
	{
		knifesInAir = new Queue<Knife>();
		player.OnChangeDirection += Player_OnChangeDirection;
	}

	private void Player_OnChangeDirection(bool right)
	{
		sheath.UpdateFacingDirection(right);
	}

	private void Update()
	{
		sheath.ReloadSpeed = fireRate;

		if (Input.GetMouseButtonDown(0) && hoverHeldTimer < hoverHeldTime)
		{
			var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			position.z = 0;

			var knife = sheath.TakeKnife();
			
			if (knife != null)
			{
				knifesInAir.Enqueue(knife);
				knife.Launch(position - sheath.transform.position);
			}
		}
		else
		{
			if (Input.GetMouseButton(1) && knifesInAir.Count > 0)
			{
				withDrawHeldTimer += Time.deltaTime;
				if (withDrawHeldTimer > withDrawHeldTime)
				{
					while (knifesInAir.Count > 0)
					{
						var knife = knifesInAir.Dequeue();
						if (knife.State != KnifeState.InSheath)
						{
							knife.Withdraw();
							if (knife.StuckedOnClimbable)
							{
								GetComponent<Rigidbody2D>().AddForce((knife.transform.position - transform.position).normalized * pullingForce, ForceMode2D.Impulse);
							}
						}
					}
					withDrawHeldTimer = 0;
				}
			}
			else
			{
				if (Input.GetMouseButtonUp(1) && knifesInAir.Count > 0)
				{
					var knife = knifesInAir.Dequeue();
					while (knife.State == KnifeState.InSheath && knifesInAir.Count > 0)
						knife = knifesInAir.Dequeue();

					if (knife.State != KnifeState.InSheath)
					{
						knife.Withdraw();
						if (knife.StuckedOnClimbable)
						{
							GetComponent<Rigidbody2D>().AddForce((knife.transform.position - transform.position).normalized * pullingForce, ForceMode2D.Impulse);
						}
					}
				}

				withDrawHeldTimer = 0;
			}
		}

		if (Input.GetKey(KeyCode.Q))
		{
			hoverHeldTimer += Time.deltaTime;
			if (hoverHeldTimer > hoverHeldTime)
			{
				pausedTime += Time.unscaledDeltaTime;

				if (pausedTime < pauseTimeDuration)
				{
					Time.timeScale = 0.2f;
					if (Input.GetMouseButtonDown(0))
					{
						changedDirection = true;
						pointerDownPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
						pointerDownPosition.z = 0;
					}
				}
				else
				{
					Time.timeScale = 1f;

				}

				if (Input.GetMouseButtonUp(0) && changedDirection)
				{
					var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
					pos.z = 0;
					pos -= pointerDownPosition;

					foreach (var knife in knifesInAir)
					{
						print(pos);
						if (knife.State != KnifeState.InSheath)
						{
							knife.ChangeDirection(pos);
						}
					}
				}
			}
		}
		else
		{
			if (hoverHeldTimer > hoverHeldTime)
			{
				Time.timeScale = 1f;
			}

			if (!changedDirection && Input.GetKeyUp(KeyCode.Q))
			{
				foreach (var knife in knifesInAir)
				{
					if (!knife.Hovered)
					{
						knife.Hover();
						break;
					}
				}
			}

			pausedTime = 0;
			hoverHeldTimer = 0;
			changedDirection = false;
		}

		foreach (var knife in knifesInAir)
		{
			var dir = knife.transform.position - sheath.transform.position;
			if (dir.sqrMagnitude > autoReturnDistance * autoReturnDistance)
			{
				knife.Withdraw();
			}

			if (knife.State == KnifeState.Stuck && dir.sqrMagnitude < autoPickupDistance * autoPickupDistance)
			{
				knife.transform.position = sheath.transform.position;
				knife.Withdraw();
			}
		}
	}
}
