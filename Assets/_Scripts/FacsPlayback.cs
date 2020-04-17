using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;


//ignore first top line completely
//ignore first items in every line
//get second item in every line - this is our key
//get 36th item in every line - this is our value

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
#endif

namespace MrPuppet
{
#if UNITY_EDITOR
    public class FacsPlayback : OdinEditorWindow
    {
        [MenuItem("Tools/Facs Playback")]
        private static void OpenWindow()
        {
            GetWindow<FacsPlayback>().Show();
        }
        private Dictionary<int, float> FacsData;
        private bool Playback;
        private float Timer;

        public GameObject Actor;

        [Button(ButtonSizes.Large)]
        private void Test()
        {
            FacsData = new Dictionary<int, float>();
            var filePath = @"Assets/Recordings/DOJO-E029-A001.txt";
            var data = File.ReadLines(filePath).Skip(21).Select(x => x.Split(',')).ToArray();

            for (int i = 1; i < data.Count(); i++)
            {
                FacsData.Add(int.Parse(data[i][1]), float.Parse(data[i][36]));
            }
            //foreach(KeyValuePair<int, float> item in dict){ Debug.Log("Time: " + item.Key + " Jaw: " + item.Value); }
            Playback = true;
            Timer = 0;
        }

        private void Update()
        {
            if (EditorApplication.isPlaying)
            {
                if (Playback)
                {
                    Timer += Time.deltaTime * 1000;

                    foreach (KeyValuePair<int, float> item in FacsData)
                    {
                        if (Timer >= item.Key)
                        {
                            continue;
                        }

                        DebugGraph.Log(item.Value);
                        Actor.GetComponent<JawTransformMapper>().JawPercentOverride = item.Value;

                        // TODO: Better way to check when animation is over
                        // Jacob: "frame loop you can store the value of the last timestamp. then you know once your time accumulation is >= that its done"
                        if (item.Key == FacsData.Keys.Last())
                        {
                            Playback = false;
                            Timer = 0;
                        }

                        break;
                    }

                }
            }
            else
            {
                Playback = false;
                Timer = 0;
            }
        }
    }
#else
    public class AnimationPlayback : MonoBehaviour
    {
    }
#endif
}
