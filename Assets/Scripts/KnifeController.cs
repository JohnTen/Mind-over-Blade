using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeController : MonoBehaviour
{
	[SerializeField] float fireRate;
	[SerializeField] float allLaunchTime;
	[SerializeField] float allLaunchRange;
	[SerializeField] float allLaunchRate;
	[SerializeField] float pullingForce;
	[SerializeField] float autoPickupDistance;
	[SerializeField] float autoReturnDistance;
	[Space(30)]
	[SerializeField] float allLaunchTimer;
	[SerializeField] Sheath sheath;
	[SerializeField] PlayerMover player;
	[SerializeField] List<Knife> knifesInAirList;

	PlayerInput input;
	Vector3 pointerDownPosition;

	private void Awake()
	{
		knifesInAirList = new List<Knife>();
		sheath.OnRecievedKnife += Sheath_OnRecievedKnife;
		player.OnChangeDirection += Player_OnChangeDirection;

		input = GetComponent<PlayerInput>();
		input.OnRangePressed += Input_OnRangePressed;
		input.OnChargedRangePressed += Input_OnChargedRangePressed;
		input.OnWithdrawAirPressed += Input_OnWithdrawAirPressed;
		input.OnWithdrawStuckPressed += Input_OnWithdrawStuckPressed;
	}

	private void Input_OnWithdrawStuckPressed()
	{
		if (player.Frozen) return;

		var minDistance = float.PositiveInfinity;
		var knifeIndex = -1;
		for (int i = 0; i < knifesInAirList.Count; i++)
		{
			var distance = (knifesInAirList[i].transform.position - sheath.transform.position).sqrMagnitude;
			if (distance > minDistance
			|| !knifesInAirList[i].StuckedOnClimbable
			|| knifesInAirList[i].IsReturning)
				continue;

			minDistance = distance;
			knifeIndex = i;
		}

		if (knifeIndex >= 0)
		{
			player.Pull((knifesInAirList[knifeIndex].transform.position - transform.position).normalized * pullingForce);
			knifesInAirList[knifeIndex].Withdraw();
		}
	}

	private void Input_OnWithdrawAirPressed()
	{
		if (player.Frozen) return;
		
		for (int i = 0; i < knifesInAirList.Count; i++)
		{
			if (knifesInAirList[i].StuckedOnClimbable
			|| knifesInAirList[i].IsReturning)
				continue;
			
			knifesInAirList[i].Withdraw();
		}
	}

	private void Input_OnRangePressed()
	{
		if (player.Frozen) return;

		var knife = sheath.TakeKnife(false);

		if (knife != null)
		{
			knifesInAirList.Add(knife);
			knife.Launch(input.GetAimingDirection(), true);
		}
	}

	private void Input_OnChargedRangePressed()
	{
		if (player.Frozen) return;

		StartCoroutine(LaunchAllKnife(input.GetAimingDirection()));
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

		foreach (var knife in knifesInAirList)
		{
			var dir = knife.transform.position - sheath.transform.position;
			if (dir.sqrMagnitude > autoReturnDistance * autoReturnDistance)
			{
				knife.Withdraw();
			}

			if (knife.State == KnifeState.Stuck && !knife.StuckedOnClimbable && dir.sqrMagnitude < autoPickupDistance * autoPickupDistance)
			{
				//knife.transform.position = sheath.transform.position;
				knife.Withdraw();
			}
		}
	}

	IEnumerator LaunchAllKnife(Vector3 direction)
	{
		var time = 0f;

		player.Frozen = true;
		while (sheath.knifeCount > 0)
		{
			time += Time.deltaTime * allLaunchRate;
			if (time < 1)
			{
				yield return null;
				continue;
			}
			time--;

			var knife = sheath.TakeKnife(true);
			knifesInAirList.Add(knife);
			knife.Launch(direction, true);
		}
		player.Frozen = false;
	}

	public void SetInputModel(IInputModel model)
	{
		throw new System.NotImplementedException();
	}
}
