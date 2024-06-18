using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InGamePlayerIKHandler : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private Transform leftHandIKTarget;
    private Transform rightHandIKTarget;

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
    }

    public void SetHandIKTarget(Transform left, Transform right)
    {
        leftHandIKTarget = left;
        rightHandIKTarget = right;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if(animator)
        {
            if(leftHandIKTarget)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKTarget.rotation);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            }

            if(rightHandIKTarget)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandIKTarget.rotation);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            }
        }
    }

    private void Update()
    {
        animator.Update(0);
    }
}
