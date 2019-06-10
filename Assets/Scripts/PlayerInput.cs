using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtility;

public class PlayerInput : MonoBehaviour, IInputModelPlugable
{
	[SerializeField] float allLaunchTime;
	[SerializeField] float withdrawTime;
	[SerializeField] Sheath sheath;
	[SerializeField] LineRenderer aimingLine;
    [SerializeField] Transform aimingLineBase;

    bool throwPressedBefore;
	bool withdrawPressedBefore;
	bool usingController;
	IInputModel input;
	
	float allLaunchTimer;
	float withdrawTimer;


    public bool BlockInput { get; set; }

    public event Action OnMeleePressed;
	public event Action OnRangePressed;
	public event Action OnChargedRangePressed;
	public event Action OnJumpPressed;
	public event Action OnWithdrawAirPressed;
	public event Action OnWithdrawStuckPressed;

	public Vector2 GetMovingDirection()
	{
        if (BlockInput) return Vector2.zero;
        return new Vector2(input.GetAxis("MoveX"), input.GetAxis("MoveY"));
	}

	internal bool IsJumpPressed()
	{
        if (BlockInput) return false;
		return input.GetButtonDown("Jump");
	}

	public Vector2 GetAimingDirection()
	{
        if (BlockInput) return Vector2.zero;
        var aim = Vector2.zero;
		if (usingController)
		{
			aim = new Vector2(input.GetAxis("LookX"), input.GetAxis("LookY"));
		}
		else
		{
			aim = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - aimingLineBase.position);
			if (aim.SqrMagnitude() > 1)
				aim.Normalize();
		}

		return aim;
	}

	private void SetAimingLine()
	{
		Vector3[] positions = new Vector3[2];
		aimingLine.GetPositions(positions);

		positions[0] = sheath.LaunchPosition.position;
		positions[1] = positions[0] + (Vector3)GetAimingDirection().normalized * 20;
		aimingLine.SetPositions(positions);
	}

	private void HideAimingLine()
	{
		Vector3[] positions = new Vector3[2];
		aimingLine.SetPositions(positions);
	}

	public bool IsMeleePressed()
	{
        if (BlockInput) return false;
        return input.GetButtonDown("Melee");
	}

	private void Start()
	{
		InputManager.Instance.RegisterPluggable(0, this);
	}

	// Update is called once per frame
	void Update()
	{
        if (BlockInput) return;
        if (input.GetButtonDown("Melee"))
		{
			OnMeleePressed?.Invoke();
		}

		var throwPressed = false;
		if (usingController)
		{
			print(GetAimingDirection());
			if (GetAimingDirection().sqrMagnitude > 0.25f)
			{
				SetAimingLine();
				print(input.GetAxis("Throw"));
				if (input.GetAxis("Throw") > 0.3f)
				{
					throwPressed = true;
					throwPressedBefore = true;
				}
			}
			else
			{
				HideAimingLine();
			}
		}
		else
		{
			SetAimingLine();
			if (input.GetButton("Throw"))
			{
				throwPressed = true;
				throwPressedBefore = true;
			}
		}

		if (throwPressed)
		{
			allLaunchTimer += Time.deltaTime;
			if (allLaunchTimer >= allLaunchTime)
			{
				OnChargedRangePressed?.Invoke();
				allLaunchTimer = 0;
				throwPressedBefore = false;
			}
		}
		else
		{
			if (throwPressedBefore)
			{
				if (allLaunchTimer < allLaunchTime)
				{
					OnRangePressed?.Invoke();
				}

				allLaunchTimer = 0;
				throwPressedBefore = false;
			}
		}

		if (input.GetButtonDown("Jump"))
		{
			OnJumpPressed?.Invoke();
		}

        //if (input.GetButtonDown("WithdrawOnAir"))
        //      {
        //          OnWithdrawAirPressed?.Invoke();
        //      }

        //      if (input.GetButtonDown("WithdrawOnStuck"))
        //      {
        //          OnWithdrawStuckPressed?.Invoke();
        //      }

        if (input.GetButton("WithdrawOnAir") || input.GetButton("WithdrawOnStuck"))
        {
            withdrawPressedBefore = true;
            withdrawTimer += Time.deltaTime;
            if (withdrawTimer >= withdrawTime)
            {
                OnWithdrawAirPressed?.Invoke();
            }
        }
        else if (withdrawPressedBefore)
        {
            withdrawPressedBefore = false;
            if (withdrawTimer < withdrawTime)
            {
                OnWithdrawStuckPressed?.Invoke();
            }
            withdrawTimer = 0;
        }
    }

	public void SetInputModel(IInputModel model)
	{
		input = model;
		usingController = model is ControllerInputModel;

		if (usingController)
		{
			print("Using Controller");
		}
		else
		{
			print("Using keyboard & mouse");
		}
	}
}
