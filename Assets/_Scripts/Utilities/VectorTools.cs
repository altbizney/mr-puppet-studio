using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class VectorTools : MonoBehaviour
    {
        public static ValueDropdownList<Vector3> VectorDirectionValues = new ValueDropdownList<Vector3>()
        {
            { "Forward", Vector3.forward },
            { "Back",    Vector3.back    },
            { "Up",      Vector3.up      },
            { "Down",    Vector3.down    },
            { "Right",   Vector3.right   },
            { "Left",    Vector3.left    },
        };
    }
}
