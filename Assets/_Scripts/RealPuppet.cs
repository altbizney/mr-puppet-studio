using UnityEngine;


namespace Thinko
{
    [RequireComponent(typeof(Animator))]
    public class RealPuppet : MonoBehaviour
    {
        [Header("Joints")]
        public Transform RootNode;
        public Transform HeadNode;
        public Transform ButtNode;
        public Transform JawNode;

        [Header("Physics")]
        [Range(0, 100)]
        public float SpineFloppiness;
    
        [Header("Jaw Configuration")]
        public float GloveOpen;
        public float GloveClose;
        [ReadOnly]
        public float LiveGloveValue;

        [Space]
        public float RigOpen;
        public float RigClose;

        private void Reset()
        {
            var children = gameObject.GetComponentsInChildren<Transform>();
            foreach(var child in children)
            {
                if (DamerauLevenshteinDistance(child.name.ToLower(), "skeleton") < 2 ||
                    DamerauLevenshteinDistance(child.name.ToLower(), "root") < 2)
                {
                    RootNode = child;
                    continue;
                }
                
                if (DamerauLevenshteinDistance(child.name.ToLower(), "head") < 2)
                {
                    HeadNode = child;
                    continue;
                }
                
                if (DamerauLevenshteinDistance(child.name.ToLower(), "butt") < 2 ||
                    DamerauLevenshteinDistance(child.name.ToLower(), "tail") < 2)
                {
                    ButtNode = child;
                    continue;
                }
                
                if (DamerauLevenshteinDistance(child.name.ToLower(), "jaw") < 2)
                {
                    JawNode = child;
                }
            }
        }
        
        public static int DamerauLevenshteinDistance(string string1, string string2)
        {
            if (string.IsNullOrEmpty(string1))
            {
                return !string.IsNullOrEmpty(string2) ? string2.Length : 0;
            }
 
            if (string.IsNullOrEmpty(string2))
            {
                return !string.IsNullOrEmpty(string1) ? string1.Length : 0;
            }
 
            var length1 = string1.Length;
            var length2 = string2.Length;
 
            var d = new int[length1 + 1, length2 + 1];

            for (var i = 0; i <= d.GetUpperBound(0); i++)
                d[i, 0] = i;
 
            for (var i = 0; i <= d.GetUpperBound(1); i++)
                d[0, i] = i;
 
            for (var i = 1; i <= d.GetUpperBound(0); i++)
            {
                for (var j = 1; j <= d.GetUpperBound(1); j++)
                {
                    var cost = string1[i - 1] == string2[j - 1] ? 0 : 1;
 
                    var del = d[i - 1, j] + 1;
                    var ins = d[i, j - 1] + 1;
                    var sub = d[i - 1, j - 1] + cost;
 
                    d[i, j] = Mathf.Min(del, Mathf.Min(ins, sub));
 
                    if (i > 1 && j > 1 && string1[i - 1] == string2[j - 2] && string1[i - 2] == string2[j - 1])
                        d[i, j] = Mathf.Min(d[i, j], d[i - 2, j - 2] + cost);
                }
            }
 
            return d[d.GetUpperBound(0), d.GetUpperBound(1)];
        }
    }
}