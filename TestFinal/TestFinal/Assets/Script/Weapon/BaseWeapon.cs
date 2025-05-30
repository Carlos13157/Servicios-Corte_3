using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

[Serializable]
public abstract class BaseWeapon : NetworkBehaviour{
    [HideInInspector]
    public AnimatorClipInfo[ ] animationClip;
    [HideInInspector]
    public AnimatorStateInfo stateInfo;
    public Animator animator {get; set;}
    [SerializeField] protected AudioClip actionateWeaponSound;
    [SerializeField] protected float damage;
    [SerializeField] protected TransformData hipTransform;
    public abstract void ActionateWeapon();

    protected void Awake(){
        animator = GetComponent<Animator>();
        transform.localPosition = hipTransform.position;
    }

    public void UpdateCurrentAnimation(){
        animationClip = animator.GetCurrentAnimatorClipInfo(0);
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        try{
            Debug.Log(animationClip[0].clip != null ? animationClip[0].clip.name : null);
        }catch (Exception){
            Debug.LogWarning("No animation clip found");
        }
    }

    public int GetAnimationLength(){
        var clipLength = stateInfo.length;
        var clipFrameRate = 0.0f;
        try{
            clipFrameRate = animationClip[0].clip.frameRate;
        }catch (Exception){
            Debug.LogWarning("Not animation clip found");
        }
        return Mathf.FloorToInt(clipLength * clipFrameRate);
    }

    public int GetAnimationCurentFrame(){
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        var time = stateInfo.normalizedTime;
        return Mathf.FloorToInt(time * GetAnimationLength());
    }
}

[Serializable]
public struct TransformData{
    [SerializeField] public Vector3 position;
    [SerializeField] public Quaternion rotation;
}

