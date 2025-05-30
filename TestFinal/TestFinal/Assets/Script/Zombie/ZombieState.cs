using System.Collections;
using UnityEngine;

public abstract class ZombieState : BaseState<ZombieStateMachine.EStates>{
    protected GameObject zombieInfo;
    protected ZombieController zombieController;
    protected ZombieStateMachine zombieStateMachine;

    protected float nextPlayTime = 0f;

    public ZombieState(GameObject zombieInfo, ZombieStateMachine.EStates key) : base(key){
        this.zombieInfo = zombieInfo;
        zombieController = zombieInfo.GetComponent<ZombieController>();
        zombieStateMachine = zombieInfo.GetComponent<ZombieStateMachine>();
    }

    protected void PlayAudio(string soundListName, float volume, float delay, bool canInterrupt){
        if (Time.time >= nextPlayTime){
            float clipLength = SoundFXManager.instance.PlayRandomSoundFXClip(zombieController.GetAudioFiles()[soundListName], zombieInfo.transform, volume, canInterrupt, zombieController.audioSource);
            nextPlayTime = Time.time + clipLength + delay;
        }
    }
}
