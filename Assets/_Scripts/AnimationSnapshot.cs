using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor.Animations;



public class AnimationSnapshot : MonoBehaviour
{
    [Required]
    public AnimationClip SampleClip;

    [Button(ButtonSizes.Large)]
    private void SetPoseToAnimation()
    {
        float FirstFrameIndex = SampleClip.frameRate / SampleClip.length;

        //if (SampleClip)
        SampleClip.SampleAnimation(gameObject, FirstFrameIndex);
    }
}
