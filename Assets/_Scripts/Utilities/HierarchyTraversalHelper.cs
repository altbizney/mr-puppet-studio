using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HierarchyTraversalHelpers
{

    public static Transform GetChildGameObjectWithName(Transform parent, string childName)
    {

        Transform[] ts = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform transform in ts) if (transform.gameObject.name == childName) return transform;
        return null;

    }
    public static int CountChildren(Transform t)
    {
        int count = 0;

        foreach (Transform child in t)
        {

            count += CountChildren(child);    // add the number of children the child has to total
            ++count;                           // add the child itself to total

        }

        return count;

    }
    public static Transform GetDeepestChild(Transform parent)
    {

        Transform deepestChild = null;

        foreach (Transform child in parent)
        {

            if (child.childCount == 0)
            {
                deepestChild = child;
                return deepestChild;

            }
            else if (child.childCount > 0)
            {

                deepestChild = GetDeepestChild(child);

                if (deepestChild)
                {
                    return deepestChild;
                }

            }

        }

        return deepestChild;

    }

}
