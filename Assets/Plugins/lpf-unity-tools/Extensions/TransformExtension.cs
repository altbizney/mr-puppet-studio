using System.Collections.Generic;
using UnityEngine;

public static class TransformExtension
{
	public static void DestroyChildren(this Transform transform)
	{
		var allChildren = new GameObject[transform.childCount];

		var i = 0;
		foreach (Transform child in transform)
		{
			allChildren[i] = child.gameObject;
			i += 1;
		}

		foreach (var child in allChildren)
		{
			Object.DestroyImmediate(child.gameObject);
		}
	}
	
	public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position)
	{
		var localToWorldMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
		return localToWorldMatrix.MultiplyPoint3x4(position);
	}
 
	public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
	{
		var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
		return worldToLocalMatrix.MultiplyPoint3x4(position);
	}
	
	public static string GetPath(this Transform current) 
	{
		if (current.parent == null)
			return "/" + current.name;
		return current.parent.GetPath() + "/" + current.name;
	}
	
	public static Transform GetDeepestChild(this Transform parent)
	{
		Transform deepestChild = null;
		foreach (Transform child in parent)
		{
			if (child.childCount == 0)
			{
				deepestChild = child;
				return deepestChild;
			}

			deepestChild = GetDeepestChild(child);
			if (deepestChild)
				return deepestChild;
		}

		return deepestChild;
	}
}
