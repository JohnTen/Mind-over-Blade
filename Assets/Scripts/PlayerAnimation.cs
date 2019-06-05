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
	private DragonBones.Armature _armature;
	private DragonBones.AnimationState _walkState;

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
	}

	private void Update()
	{
		UpdateAnimation();
	}

	public void SetSpeed(float x, float y)
	{
		_xSpeed = x;
		_ySpeed = y;
	}

	private void OnAnimationEventHandler(string type, EventObject eventObject)
	{
		if (eventObject.animationState.name == "Droping_Buffering")
		{
			_armature.animation.FadeIn("Idle_Stand", 0.2f, 0, 0, null).resetToPose = false;
			print("Idle_Stand");

			OnFrozenMovement?.Invoke(false);
			_landing = false;
		}
	}

	private void UpdateAnimation()
	{
		if (_jump && _airborne == false)
		{
			var anim = _armature.animation.FadeIn("jump_ground", 0.1f, 1, 0, null);
			anim.timeScale = 5f;
			anim.resetToPose = false;

			print("jump_ground");
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
					var anim = _armature.animation.FadeIn("jump_sky", 0.2f, 1, 0, null);
					anim.timeScale = 2f;
					anim.resetToPose = false;
					print("jump_sky");
				}
				_dropping = false;
			}
			else if (!_dropping)
			{
				_armature.animation.FadeIn("Droping", 0.2f, 0, 0, null).resetToPose = false;
				print("Droping");
				_dropping = true;
			}
			_armatureComponent.armature.flipX = _xSpeed < 0;
			return;
		}

		if (_armature.animation.lastAnimationName == "Droping_Buffering") return;

		if (_xSpeed == 0)
		{
			if (_walkState != null)
			{
				_armature.animation.FadeIn("Idle_Stand", 0.1f, 0, 0, null).resetToPose = false;
				print("Idle_Stand");
				_walkState = null;
			}
		}
		else
		{
			if (_walkState == null)
			{
				_walkState = _armature.animation.FadeIn("Run", 0.1f, 0, 0,  null);
				print("Run");
				_walkState.resetToPose = false;
				_walkState.timeScale = 1.5f;
			}
			_armatureComponent.armature.flipX = _xSpeed < 0;
		}
	}
}
