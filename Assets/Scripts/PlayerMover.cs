using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtility;
using UnityUtility.Platformer;

public class PlayerMover : PhysicalMover
{
	[SerializeField] float maxXSpeed;
	[SerializeField] float maxYSpeed;
	[SerializeField] float airSpeed;
	[SerializeField] SpriteRenderer sprite;
	[SerializeField] ContactGroundDetector groundDetector;
	
	[Header("Attacking steps")]
	[SerializeField] float attackStepSpeed;
	[SerializeField] Vector2 attackStep;

	[Header("Airborne movement")]
	[SerializeField] float airborneXDecayRate = 0.02f;
	[SerializeField] float airborneXMovementTimeFactor = 0.5f;
	[SerializeField] AnimationCurve airborneXMovementCurve;

	Vector2 velocity = Vector2.zero;

	PlayerInput input;
	PlayerAnimation _animation;
	float airborneTime;
	List<ContactPoint2D> contacts;

	public bool FacingRight => sprite.transform.eulerAngles.y == 0;
	public bool Frozen { get; set; }

	public event Action<bool> OnChangeDirection;

	protected override void Awake()
	{
		base.Awake();
		var jumper = GetComponent<PlayerJumper>();
		jumper.OnJump += Jumper_OnJump;
		jumper.OnDashingBegin += Jumper_OnDashingBegin;
		jumper.OnDashingEnd += Jumper_OnDashingEnd;
		groundDetector.OnLanding += GroundDetector_OnLanding;

		input = GetComponent<PlayerInput>();
		contacts = new List<ContactPoint2D>();

		_animation = GetComponent<PlayerAnimation>();
		_animation.OnFrozenMovement += _animation_OnFrozenMovement;
	}

	private void _animation_OnFrozenMovement(bool t1)
	{
		Frozen = t1;
	}

	private void GroundDetector_OnLanding()
	{
		airborneTime = 0;
		_animation.Landing = true;
	}

	public void Pull(Vector3 force)
	{
		velocity = force;
		airborneTime = 0;
		_animation.Jump = true;
	}

	public void SetStepSpeed(float speed)
	{
		attackStepSpeed = speed;
	}

	public void StepForward(float stepLength)
	{
		attackStep = (FacingRight? Vector3.right: Vector3.left) * stepLength;
	}

	private void Jumper_OnJump(float height)
	{
		if (Frozen) return;

		velocity.y = Mathf.Sqrt(19.62f * height * rigidBody.gravityScale);
		_animation.Jump = true;
	}

	private void Jumper_OnDashingEnd()
	{
		rigidBody.isKinematic = false;
	}

	private void Jumper_OnDashingBegin(Vector3 direction)
	{
		rigidBody.isKinematic = true;
		rigidBody.velocity = Vector2.zero;
	}

	protected override Vector3 GetMovingDirection()
	{
		if (Frozen)
			return Vector3.zero;
		
		var dir = input.GetMovingDirection();
		dir.y = 0;


		if (dir.x != 0)
		{
			OnChangeDirection.Invoke(dir.x > 0);
			sprite.transform.rotation = Quaternion.Euler(0, dir.x > 0? 0: 180, 0);
		}


		return dir;
	}

	protected override void Moving(Vector3 vector)
	{
		velocity.y += rigidBody.gravityScale * Physics2D.gravity.y * Time.deltaTime;
		velocity = ApplyPhysicalContactEffects(velocity);

		if (groundDetector.OnGround && velocity.y <= 0)
		{
			velocity.x = vector.x * moveSpeed;
		}
		else
		{
			velocity.x *= 1 - airborneXDecayRate;
			if (Mathf.Sign(velocity.x) != Mathf.Sign(vector.x) ||
				Mathf.Abs(velocity.x) < moveSpeed)
				velocity.x += vector.x * airSpeed * airborneXMovementCurve.Evaluate(airborneTime / airborneXMovementTimeFactor);
		}

		velocity = ApplySpeedLimit(velocity);

		rigidBody.MovePosition((Vector2)transform.position + velocity * Time.deltaTime);

		if (attackStep.sqrMagnitude > 0)
		{
			var targetPos = Vector2.MoveTowards(transform.position, (Vector2)transform.position + attackStep, attackStepSpeed * Time.deltaTime);
			rigidBody.MovePosition(targetPos);
			if (targetPos == (Vector2)transform.position + attackStep)
			{
				attackStep = Vector3.zero;
			}
			else
			{
				attackStep -= attackStep.normalized * attackStepSpeed * Time.deltaTime;
			}
		}

		ResetPhysicalContacts();

		_animation.SetSpeed(velocity.x, velocity.y);
	}

	private Vector2 ApplySpeedLimit(Vector2 velocity)
	{
		velocity.x = Mathf.Clamp(velocity.x, -maxXSpeed, maxXSpeed);
		velocity.y = Mathf.Clamp(velocity.y, -maxYSpeed, maxYSpeed);

		return velocity;
	}

	private Vector2 ApplyPhysicalContactEffects(Vector2 velocity)
	{
		foreach (var contact in contacts)
		{
			var d = contact.normal * velocity;
			if (d.x < 0)
				velocity.x -= d.x * Mathf.Sign(contact.normal.x);
			if (d.y < 0)
				velocity.y -= d.y * Mathf.Sign(contact.normal.y);
		}

		return velocity;
	}

	private void UpdatePhysicalContacts(ContactPoint2D[] contactPoints)
	{
		contacts.AddRange(contactPoints);
	}

	private void ResetPhysicalContacts()
	{
		contacts.Clear();
	}

	protected override void FixedUpdate()
	{
		if (!groundDetector.OnGround)
			airborneTime += Time.fixedDeltaTime;

		base.FixedUpdate();
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		UpdatePhysicalContacts(collision.contacts);
	}
}
