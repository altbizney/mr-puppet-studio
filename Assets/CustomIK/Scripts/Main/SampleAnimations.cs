using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleAnimations : MonoBehaviour
{

    #region Static Variables

    #endregion

    #region Public Variables

    #endregion

    #region Private Variables
    [SerializeField]
    private Animator animator;
    #endregion

    #region Unity Methods
    private void Start()
    {

    }

    private void Update()
    {
        GetInput();
    }
    #endregion

    #region Callback Methods

    #endregion

    #region Static Methods

    #endregion

    #region Public Methods

    #endregion

    #region Local Methods
    void GetInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            animator.SetTrigger("doReset");
            animator.ResetTrigger("doAction");
            animator.SetInteger("action", 1);
            animator.SetTrigger("doAction");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            animator.SetTrigger("doReset");
            animator.ResetTrigger("doAction");
            animator.SetInteger("action", 2);
            animator.SetTrigger("doAction");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            animator.SetTrigger("doReset");
            animator.ResetTrigger("doAction");
            animator.SetInteger("action", 3);
            animator.SetTrigger("doAction");
        }
    }
    #endregion
}
