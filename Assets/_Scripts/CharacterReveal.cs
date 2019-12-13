using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class CharacterReveal : MonoBehaviour
    {
        [Title("Scale")]
        public bool Scale = true;
        [ShowIf("Scale")]
        public KeyCode ScaleKey = KeyCode.Space;
        [ShowIf("Scale")]
        public Transform ScaleTarget;
        [ShowIf("Scale")]
        public float ScaleFrom = 0f;
        [ShowIf("Scale")]
        public float ScaleTo = 1f;
        [ShowIf("Scale")]
        public float ScaleSmoothTime = 0.1f;

        private bool ScaleIsActive = true;
        private float ScaleKnobCurrent, ScaleKnobTarget, ScaleKnobVelocity;

        [Title("Camera Background")]
        public bool FadeCamera = true;
        [ShowIf("FadeCamera")]
        public KeyCode FadeCameraKey = KeyCode.Space;
        [ShowIf("FadeCamera")]
        public Camera FadeCameraTarget;
        [ShowIf("FadeCamera")]
        public Color FadeCameraFrom = Color.black;
        [ShowIf("FadeCamera")]
        public float FadeCameraSmoothTime = 0.1f;

        private Color FadeCameraTo = Color.black;
        private bool FadeCameraIsActive = true;
        private float FadeCameraKnobCurrent, FadeCameraKnobTarget, FadeCameraKnobVelocity;

        private void Awake()
        {
            if (FadeCamera && FadeCameraTarget) FadeCameraTo = FadeCameraTarget.backgroundColor;
        }

        private void Update()
        {
            if (Scale)
            {
                if (ScaleTarget && Input.GetKeyUp(ScaleKey))
                {
                    ScaleKnobTarget = (ScaleIsActive ? ScaleFrom : ScaleTo);
                    ScaleIsActive = !ScaleIsActive;
                }

                ScaleKnobCurrent = Mathf.SmoothDamp(ScaleKnobCurrent, ScaleKnobTarget, ref ScaleKnobVelocity, ScaleSmoothTime);
                ScaleTarget.localScale = Vector3.one * ScaleKnobCurrent;
            }

            if (FadeCamera)
            {
                if (FadeCameraTarget && Input.GetKeyUp(FadeCameraKey))
                {
                    // TODO: Mathf.SmoothDamp + Color.Lerp
                    FadeCameraKnobTarget = (FadeCameraIsActive ? 0f : 1f);
                    FadeCameraIsActive = !FadeCameraIsActive;
                }

                FadeCameraKnobCurrent = Mathf.SmoothDamp(FadeCameraKnobCurrent, FadeCameraKnobTarget, ref FadeCameraKnobVelocity, FadeCameraSmoothTime);
                FadeCameraTarget.backgroundColor = Color.Lerp(FadeCameraFrom, FadeCameraTo, FadeCameraKnobCurrent);
            }
        }
    }
}