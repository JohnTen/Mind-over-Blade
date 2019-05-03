using System;
using System.Collections;
using UnityEngine;

namespace UnityUtility
{
	public static class ExtensionMethods
	{
		/// <summary>
		/// Checks whether a Unity object exists; i.e. it hasn't been destroyed
		/// </summary>
		/// <remarks>
		/// Unlike the normal null check, this will return <c>true</c> for objects
		/// that have been merely disabled.
		/// </remarks>
		/// <param name="obj"></param>
		/// <returns><c>true</c> if the object exists, otherwise <c>false</c></returns>
		public static bool IsExists(this object obj)
		{
			if (obj == null)
				return false;
			bool exists = false;
			var o = obj as UnityEngine.Object;
			if ((object)o != null)
				try
				{
					o.hideFlags.Equals(null);
					exists = true;
				}
				catch { }
			return exists;
		}

		/// <summary>
		/// Gets component from another component if it has one, and adds one if it doesn't
		/// </summary>
		/// <param name="obj"></param>
		/// <returns><c>true</c> if the object exists, otherwise <c>false</c></returns>
		public static T GetOrAddComponent<T>(this Component c) where T : Component
		{
			if (c == null) throw new ArgumentNullException("The operational component is null!");
			var part = c.GetComponent<T>();
			if (part == null)
				part = c.gameObject.AddComponent<T>();

			return part;
		}

		#region Maths

		/// <summary>
		/// Returns the square of the given number
		/// </summary>
		/// <param name="num"></param>
		/// <returns></returns>
		public static int Square(this int num)
		{
			return num * num;
		}

		/// <summary>
		/// Returns the square of the given number
		/// </summary>
		/// <param name="num"></param>
		/// <returns></returns>
		public static float Square(this float num)
		{
			return num * num;
		}

		/// <summary>
		/// Returns the square of the given number
		/// </summary>
		/// <param name="num"></param>
		/// <returns></returns>
		public static double Square(this double num)
		{
			return num * num;
		}

		public static float Clamp01(this float t)
		{
			return t > 1? 1: (t < 0? 0: t);
		}

		/// <summary>
		/// Clamps an angle between -180 and +180
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		public static float Clamp180(this float angle)
		{
			if (angle > 180f)
				angle -= 360f;
			else if (angle < -180f)
				angle += 360f;
			return angle;
		}

		/// <summary>
		/// Clamps an euler rotation between -180 and +180
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		public static Vector3 Clamp180(this Vector3 angle)
		{
			return new Vector3(angle.x.Clamp180(),
				angle.y.Clamp180(),
				angle.z.Clamp180());
		}

		/// <summary>
		/// Clamps an euler rotation between -180 and +180
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		public static Vector2 Clamp180(this Vector2 angle)
		{
			return new Vector2(angle.x.Clamp180(),
				angle.y.Clamp180());
		}

		public static Vector3 Rotate(this Vector3 vector, Vector3 angles)
		{
			return Quaternion.Euler(angles) * vector;
		}

		public static Vector3 Rotate(this Vector3 vector, float x, float y, float z)
		{
			return Quaternion.Euler(x, y, z) * vector;
		}

		public static float Cross(this Vector2 v, Vector2 o)
		{
			return v.x * o.y - v.y * o.x;
		}
		#endregion

		#region Randoms
		public static float RandomBetween(this Vector2 vec)
		{
			return UnityEngine.Random.Range(vec.x, vec.y);
		}

		public static int RandomBetween(this Vector2Int vec)
		{
			return UnityEngine.Random.Range(vec.x, vec.y);
		}

		public static int RandomBetweenIncluded(this Vector2Int vec)
		{
			return UnityEngine.Random.Range(vec.x, vec.y + 1);
		}
		#endregion

		public static float LerpBetween(this Vector2 vec, float t)
		{
			return Mathf.Lerp(vec.x, vec.y, t);
		}

		public static Vector3Int ToVector3Int(this Vector3 vec)
		{
			vec.x += vec.x < 0 ? -1 : 0;
			vec.y += vec.y < 0 ? -1 : 0;
			vec.z += vec.z < 0 ? -1 : 0;
			return new Vector3Int((int)vec.x, (int)vec.y, (int)vec.z);
		}
		
		public static bool IsIncluded(this Vector2 bound, float value)
		{
			return value >= bound.x && value <= bound.y;
		}

		public static bool IsIncluded(this Vector2Int bound, float value)
		{
			return value >= bound.x && value <= bound.y;
		}

		public static bool IsWithInBound(this Bounds self, Bounds other)
		{
			return other.Contains(self.max) && other.Contains(self.min);
		}

		// From TOMS...
		// TODO: Figure out those 'reflection' stuffs.
		/*
		static readonly char[] dotSplit = { '.' };
		static readonly Regex indexRegex = new Regex(@"data\[(\d+)\]", RegexOptions.Compiled);

		public static object GetObject(this SerializedProperty property)
		{
			object parentObject;
			return GetObject(property, out parentObject);
		}

		public static object GetObject(this SerializedProperty property, out object parentObject)
		{
			const BindingFlags bf =
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic;
			if (property == null)
				throw new ArgumentNullException();
			object obj = property.serializedObject.targetObject;
			parentObject = null;
			var pathTokens = property.propertyPath.Split(dotSplit);
			for (int i = 0; i < pathTokens.Length; i++)
			{
				parentObject = obj;
				if (obj == null)
					return null;
				var field = obj.GetType().GetField(pathTokens[i], bf);
				if (field == null)
				{
					if (pathTokens[i] != "Array")
					{
						Debug.LogError("Unable to find field " + pathTokens[i] +
									   ". Maybe it's private? (fix this)");
						return null;
					}
					var match = indexRegex.Match(pathTokens[++i]);
					if (!match.Success)
					{
						Debug.LogError("Regex was not a match: " + pathTokens[i]);
						return null;
					}
					var index = Int32.Parse(match.Groups[1].Value);
					var list = obj as IList;
					if (list == null || index < 0 || index >= list.Count)
					{
						Debug.LogError("Unable to index: " + obj);
						return null;
					}
					obj = list[index];
				}
				else
				{
					obj = field.GetValue(obj);
				}
			}
			return obj;
		}
		*/
	}
}
