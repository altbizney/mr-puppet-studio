using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MrPuppet;

public class IKTag : MonoBehaviour {

    #region Static Variables

    #endregion

    #region Public Variables
    public IKTagId iKTagId;
    public int chainId;
    #endregion

    #region Private Variables

    #endregion

    #region Unity Methods
    private void Awake() {
        IKButtPuppet root = this.transform.root.GetComponent<IKButtPuppet>();
        if (root != null) {
            switch (iKTagId) {
                case IKTagId.LeftArm:
                    this.gameObject.SetActive(root.enableLeftArmLimb);
                    break;
                case IKTagId.RightArm:
                    this.gameObject.SetActive(root.enableRightArmLimb);
                    break;
            }
        }
    }
    #endregion

    #region Callback Methods

    #endregion

    #region Static Methods

    #endregion

    #region Public Methods

    #endregion

    #region Local Methods

    #endregion
}

public enum IKTagId { Head, Neck, Spine, Hip, LeftArm, RightArm, Grounder, LeftLeg, RightLeg };