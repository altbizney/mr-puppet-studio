using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKController : MonoBehaviour {

    #region Static Variables

    #endregion

    #region Public Variables
    
    #endregion

    #region Private Variables
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private bool isActive;
    #endregion

    #region Unity Methods
    private void Start() {
        
    }

    private void Update() {
        OnCustomAnimatorIK();
    }
    #endregion

    #region Callback Methods

    #endregion

    #region Static Methods

    #endregion

    #region Public Methods

    #endregion

    #region Local Methods
    void OnCustomAnimatorIK() {
        if (isActive) {

        }
    }
    #endregion
}
