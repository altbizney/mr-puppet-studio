using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MrPuppet;

public class IKTag : MonoBehaviour
{

    #region Static Variables

    #endregion

    #region Public Variables
    public IKTagId iKTagId = IKTagId.Unknown;
    public int chainId;
    #endregion

    #region Private Variables

    #endregion

    #region Unity Methods
    private void Awake()
    {
        IKButtPuppet root = this.transform.root.GetComponent<IKButtPuppet>();
        if (root != null)
        {
            switch (iKTagId)
            {
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

public enum IKTagId
{
    Unknown = -1,
    Head = 0,
    Neck = 1,
    Spine = 2,
    Hip = 3,
    LeftArm = 4,
    RightArm = 5,
    Grounder = 6,
    LeftLeg = 7,
    RightLeg = 8
}
