using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtility.Platformer;

namespace UnityUtility
{
	[RequireComponent(typeof(Collider2D))]
	public class ContactGroundDetector : BaseGroundDetector
	{
		protected new Collider2D collider;
		protected ContactPoint2D[] contactPoints = new ContactPoint2D[10];

		protected virtual void Awake()
		{
			collider = GetComponent<Collider2D>();
		}

		protected override bool IsOnGround()
		{
			var count = Physics2D.GetContacts(collider, contactPoints);

			for (int i = 0; i < count; i++)
			{
				if (Vector2.Angle(contactPoints[i].normal, Vector2.up) < 30)
				{
					Normal = contactPoints[i].normal;
					return true;
				}
			}

			return false;
		}
	}
}
