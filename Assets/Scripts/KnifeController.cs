using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeController : MonoBehaviour
{
	[SerializeField] float fireRate;
	[SerializeField] float allLaunchTime;
	[SerializeField] float allLaunchRange;
	[SerializeField] float withDrawHeldTime;
	[SerializeField] float pauseTimeDuration;
	[SerializeField] float hoverHeldTime;
	[SerializeField] float pullingForce;
	[SerializeField] float pullingForceLimit;
	[SerializeField] float autoPickupDistance;
	[SerializeField] float autoReturnDistance;
	[SerializeField] float dispersion;
	[SerializeField] SpriteRenderer allLaunchIndicator;
	[Space(30)]
	[SerializeField] float allLaunchTimer;
	[SerializeField] float withDrawHeldTimer;
	[SerializeField] float hoverHeldTimer;
	[SerializeField] bool changedDirection;
	[SerializeField] float pausedTime;
	[SerializeField] Sheath sheath;
	[SerializeField] PlayerMover player;
	[SerializeField] LinkedList<Knife> knifesInAirList;
	

	Vector3 pointerDownPosition;

	private void Awake()
	{
		knifesInAirList = new LinkedList<Knife>();
		sheath.OnRecievedKnife += Sheath_OnRecievedKnife;
		player.OnChangeDirection += Player_OnChangeDirection;
	}

	private void Sheath_OnRecievedKnife(Knife knife)
	{
		knifesInAirList.Remove(knife);
	}

	private void Player_OnChangeDirection(bool right)
	{
		sheath.UpdateFacingDirection(right);
	}

	private void Update()
	{
		sheath.ReloadSpeed = fireRate;

		if (Input.GetMouseButton(0))
		{
			var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			position.z = 0;

			allLaunchTimer += Time.deltaTime;
			if (Input.GetMouseButtonDown(0))
			{
				pointerDownPosition = position;
			}
			
			if (allLaunchTimer > allLaunchTime)
			{
				if (!allLaunchIndicator.enabled)
				{
					allLaunchIndicator.enabled = true;
					allLaunchIndicator.transform.position = position;
					allLaunchIndicator.transform.localScale = Vector3.one * allLaunchRange;
				}

				pausedTime += Time.deltaTime;

				if (pausedTime < pauseTimeDuration)
				{
					Time.timeScale = 0.2f;
				}
				else
				{
					Time.timeScale = 1f;
				}
			}
		}
		else
		{
			if (Input.GetMouseButtonUp(0))
			{
				var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				position.z = 0;
				var toPosition = position - pointerDownPosition;

				if (allLaunchTimer < allLaunchTime)
				{
					var knife = sheath.TakeKnife(false);

					if (knife != null)
					{
						knifesInAirList.AddLast(knife);
						knife.Launch(position - sheath.LaunchPosition.position);
					}
				}
				else
				{
					if (toPosition.sqrMagnitude > allLaunchRange * allLaunchRange)
					{
						foreach (var knife in knifesInAirList)
						{
							if (knife.State != KnifeState.InSheath)
							{
								knife.ChangeDirection(toPosition);
							}
						}
					}
					else
					{
						var directions = GenerateDirections(position - sheath.LaunchPosition.position, sheath.knifeCount);
						print(directions.Length);
						for (int i = 0; i < directions.Length; i++)
						{
							var knife = sheath.TakeKnife(true);
							knifesInAirList.AddLast(knife);
							knife.Launch(directions[i], true);
						}
					}
				}

				Time.timeScale = 1f;
				allLaunchIndicator.enabled = false;
				allLaunchTimer = 0;
			}
		}

		
		if (Input.GetMouseButton(1) && knifesInAirList.Count > 0)
		{
			withDrawHeldTimer += Time.deltaTime;
			if (withDrawHeldTimer > withDrawHeldTime)
			{
				foreach (var knife in knifesInAirList)
				{
					knife.Withdraw();
					if (knife.StuckedOnClimbable)
					{
						GetComponent<Rigidbody2D>().AddForce((knife.transform.position - transform.position).normalized * pullingForce, ForceMode2D.Impulse);
					}
				}
				withDrawHeldTimer = 0;
			}
		}
		else
		{
			if (Input.GetMouseButtonUp(1) && knifesInAirList.Count > 0)
			{
				var knife = knifesInAirList.First.Value;
					
				knife.Withdraw();
				if (knife.StuckedOnClimbable)
				{
					GetComponent<Rigidbody2D>().AddForce((knife.transform.position - transform.position).normalized * pullingForce, ForceMode2D.Impulse);
				}
			}

			withDrawHeldTimer = 0;
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

					foreach (var knife in knifesInAirList)
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
				foreach (var knife in knifesInAirList)
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

		foreach (var knife in knifesInAirList)
		{
			var dir = knife.transform.position - sheath.transform.position;
			if (dir.sqrMagnitude > autoReturnDistance * autoReturnDistance)
			{
				knife.Withdraw();
			}

			if (knife.State == KnifeState.Stuck && dir.sqrMagnitude < autoPickupDistance * autoPickupDistance)
			{
				//knife.transform.position = sheath.transform.position;
				knife.Withdraw();
			}
		}
	}

	Vector3[] GenerateDirections(Vector3 baseDirection, int numberOfDirection)
	{
		if (numberOfDirection <= 0) return new Vector3[0];
		Vector3[] directions = new Vector3[numberOfDirection];

		float angle = (numberOfDirection - 1) * dispersion * 0.5f;
		for (int i = 0; i < numberOfDirection; i++)
		{
			directions[i] = Quaternion.Euler(0, 0, angle) * baseDirection;
			angle -= dispersion;
		}

		return directions;
	}
}
