using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;
using UnityUtility;

public class PlayerAnimation : MonoBehaviour
{
	[SerializeField] bool _jump;
	[SerializeField] bool _airborne;
	[SerializeField] bool _dropping;
	[SerializeField] bool _landing;
	[SerializeField] float _xSpeed;
	[SerializeField] float _ySpeed;
	[SerializeField] private UnityArmatureComponent _armatureComponent;
	[SerializeField] MeleeAnimationEventHandler meleeHandler;
	private DragonBones.Armature _armature;
	private DragonBones.AnimationState _walkState;

	[SerializeField] private float attackTimer;

	int currentAttack = 0;
	string[] meleeAttacks = { "ATK_Melee_Ground_1", "ATK_Melee_Ground_2", "ATK_Melee_Ground_3" };

	public bool Jump
	{
		get => _jump;
		set => _jump = value;
	}

	public bool Landing
	{
		get => _landing;
		set => _landing = value;
	}

	public event Action<bool> OnFrozenMovement;

	private void Start()
	{
		_armature = this._armatureComponent.armature;
		_armatureComponent.AddDBEventListener(EventObject.COMPLETE, OnAnimationEventHandler);
		_armatureComponent.AddDBEventListener(EventObject.FRAME_EVENT, OnFrameEventHandler);
	}

	private void Update()
	{
		if (!IsPlayingMeleeAnimation() && attackTimer > 0)
		{
			attackTimer -= Time.deltaTime;
			if (attackTimer < 0)
				currentAttack = 0;
		}


		UpdateAnimation();
	}

	public void SetSpeed(float x, float y)
	{
		_xSpeed = x;
		_ySpeed = y;
	}

	public void Attack()
	{
		_walkState = null;
		_armature.animation.FadeIn(meleeAttacks[currentAttack], 0, 1);
	}

	private void OnAnimationEventHandler(string type, EventObject eventObject)
	{
		if (eventObject.animationState.name == "Droping_Buffering")
		{
			_armature.animation.FadeIn("Idle_Ground", 0.2f, 0, 0, null).resetToPose = false;
			print("Idle_Ground");

			OnFrozenMovement?.Invoke(false);
			_landing = false;
		}

		foreach (var attack in meleeAttacks)
		{
			if (attack != eventObject.animationState.name) continue;
			currentAttack++;
			currentAttack %= meleeAttacks.Length;
			attackTimer = 0.3f;
			_armature.animation.FadeIn("Idle_Ground", 0.2f, 0, 0, null).resetToPose = false;
			print("Idle_Ground");
		}
	}

	private void OnFrameEventHandler(string type, EventObject eventObject)
	{
		if (eventObject.name == "AttackBegin")
			meleeHandler.Activate();

		if (eventObject.name == "AttackEnd")
			meleeHandler.Deactivate();

		if (eventObject.name == "AttackStepDistance")
			meleeHandler.StepForward(eventObject.data.floats[0]);

		if (eventObject.name == "AttackStepSpeed")
			meleeHandler.SetStepSpeed(eventObject.data.floats[0]);
	}

	private void UpdateAnimation()
	{
		if (_jump && _airborne == false)
		{
			var anim = _armature.animation.FadeIn("Jump_Ground", 0.1f, 1, 0, null);
			anim.timeScale = 5f;
			anim.resetToPose = false;

			print("Jump_Ground");
			_walkState = null;
			_jump = false;
			_airborne = true;
			_landing = false;
			return;
		}
		_jump = false;

		if (_airborne)
		{
			if (_landing)
			{
				var anim = _armature.animation.FadeIn("Droping_Buffering", 0f, 1, 0, null);
				anim.timeScale = 3f;
				anim.resetToPose = false;
				print("Droping_Buffering");
				_airborne = false;
				_dropping = false;
				OnFrozenMovement?.Invoke(true);
				return;
			}

			if (_ySpeed >= 0)
			{
				if (_dropping)
				{
					var anim = _armature.animation.FadeIn("Jump_Air", 0.2f, 1, 0, null);
					anim.timeScale = 2f;
					anim.resetToPose = false;
					print("Jump_Air");
				}
				_dropping = false;
			}
			else if (!_dropping)
			{
				_armature.animation.FadeIn("Droping", 0.2f, 0, 0, null).resetToPose = false;
				print("Droping");
				_dropping = true;
			}

			if (_xSpeed > 0 && _armature.flipX
			 || _xSpeed < 0 && !_armature.flipX)
			{
				_armature.flipX = !_armature.flipX;
			}
				
			return;
		}

		if (_armature.animation.lastAnimationName == "Droping_Buffering") return;

		if (_xSpeed == 0)
		{
			if (_walkState != null)
			{
				_walkState = _armature.animation.FadeIn("Idle_Ground", 0.1f, 0, 0, null);
				_walkState.resetToPose = false;
				_walkState.timeScale = 0.5f;
				print("Idle_Ground");
				_walkState = null;
			}
		}
		else
		{
			if (_walkState == null)
			{
				_walkState = _armature.animation.FadeIn("Run_Ground", 0.1f, 0, 0,  null);
				print("Run_Ground");
				_walkState.resetToPose = false;
				_walkState.timeScale = 0.5f;
			}

			if (_xSpeed > 0 && _armature.flipX
			 || _xSpeed < 0 && !_armature.flipX)
			{
				_armature.flipX = !_armature.flipX;
			}
		}
	}

	private bool IsPlayingMeleeAnimation()
	{
		foreach (var attack in meleeAttacks)
		{
			if (attack == _armature.animation.lastAnimationName)
				return true;
		}

		return false;
	}
}
