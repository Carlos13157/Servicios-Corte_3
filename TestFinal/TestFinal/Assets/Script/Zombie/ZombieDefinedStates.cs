using System.Collections;
using UnityEngine;

public class ZombieIdleState : ZombieState{
    public ZombieIdleState(GameObject ZombieInfo, ZombieStateMachine.EStates key): base(ZombieInfo, key){}

    public override void EnterState(){
        zombieController.animator.SetBool("isIdle", true);
        PlayAudio("IdleSounds", 1f, 2f, false);
    }
    public override void ExitState(){
        zombieController.animator.SetBool("isIdle", false);
    }
    public override void UpdateState(){
        if(zombieController.playerInSight) zombieStateMachine.TransitionToState(ZombieStateMachine.EStates.Chase);
        PlayAudio("IdleSounds", 1f, 2f, false);
    }
    public override ZombieStateMachine.EStates GetNextState(){
        return stateKey;
    }

    public override void OnTriggerEnter(Collider other){}

    public override void OnTriggerStay(Collider other){}

    public override void OnTriggerExit(Collider other){}
}

public class ZombieChaseState : ZombieState{
    private AnimatorStateInfo stateInfo;
    public ZombieChaseState(GameObject ZombieInfo, ZombieStateMachine.EStates state): base(ZombieInfo, state){}

    public override void EnterState(){
        zombieController.animator.SetBool("isChasing", true);
        stateInfo = zombieController.animator.GetCurrentAnimatorStateInfo(0);
        PlayAudio("ChaseSounds", 1f, 1f, true);
    }
    public override void ExitState(){
        zombieController.animator.SetBool("isChasing", false);
        zombieController.navMeshAgent.SetDestination(zombieController.transform.position);
    }
    public override void UpdateState(){
        stateInfo = zombieController.animator.GetCurrentAnimatorStateInfo(0);

        if(zombieController.GetDistanceToPlayer() <= zombieController.attackDistance){
            zombieStateMachine.TransitionToState(ZombieStateMachine.EStates.Attack);
        }else if(!zombieController.playerInSight){
            zombieController.targetLastSeenPosition = zombieController.GetPlayerTransform().position;
            zombieStateMachine.TransitionToState(ZombieStateMachine.EStates.Search);
        }else if(stateInfo.IsName("Zombie Running")){
            zombieController.navMeshAgent.SetDestination(zombieController.GetPlayerTransform().position);
            PlayAudio("ChaseSounds", 1f, 1f, true);
        }
    }
    public override ZombieStateMachine.EStates GetNextState(){
        return stateKey;
    }

    public override void OnTriggerEnter(Collider other){}

    public override void OnTriggerStay(Collider other){}

    public override void OnTriggerExit(Collider other){}

}

public class ZombieSearchState : ZombieState{
    private bool reachedPlayerLastSeenPosition;
    public ZombieSearchState(GameObject ZombieInfo, ZombieStateMachine.EStates state): base(ZombieInfo, state){}

    public override void EnterState(){
        zombieController.animator.SetBool("isSearching", true);
        reachedPlayerLastSeenPosition = false;
    }
    public override void ExitState(){
        zombieController.animator.SetBool("isSearching", false);
        zombieController.targetLastSeenPosition = null;
        reachedPlayerLastSeenPosition = false;
    }
    public override void UpdateState(){
        if(zombieController.playerInSight){
            zombieController.targetLastSeenPosition = Vector3.zero;
            zombieStateMachine.TransitionToState(ZombieStateMachine.EStates.Chase);
        }

        zombieController.animator.SetBool("hasReachedPlayerLastSeenPosition", reachedPlayerLastSeenPosition);

        if (!zombieController.targetLastSeenPosition.HasValue) return;

        if (Vector3.Distance(zombieController.transform.position, zombieController.targetLastSeenPosition.Value) <= 4 && !zombieController.playerInSight)
        {
            reachedPlayerLastSeenPosition = true;
            zombieController.navMeshAgent.SetDestination(zombieController.transform.position);
            PlayAudio("IdleSounds", 1f, 2f, false);
        }
        else if (!reachedPlayerLastSeenPosition)
        {
            zombieController.navMeshAgent.SetDestination(zombieController.targetLastSeenPosition.Value);
            PlayAudio("ChaseSounds", 1f, 0f, true);
        }

        if (reachedPlayerLastSeenPosition){
            zombieStateMachine.TransitionToState(ZombieStateMachine.EStates.Idle);
        }
    }
    public override ZombieStateMachine.EStates GetNextState(){
        return stateKey;
    }

    public override void OnTriggerEnter(Collider other){}

    public override void OnTriggerStay(Collider other){}

    public override void OnTriggerExit(Collider other){}

}

public class ZombieAttackState : ZombieState{
    
    public ZombieAttackState(GameObject ZombieInfo, ZombieStateMachine.EStates state): base(ZombieInfo, state){}

    public override void EnterState(){
        zombieController.animator.SetBool("isAttacking", true);
    }
    public override void ExitState(){
        zombieController.animator.SetBool("isAttacking", false);
    }
    public override void UpdateState(){
        PlayAudio("AttackSounds", 1f, 1f, true);
        if(zombieController.GetDistanceToPlayer() > zombieController.attackDistance) zombieStateMachine.TransitionToState(ZombieStateMachine.EStates.Chase);
    }
    public override ZombieStateMachine.EStates GetNextState(){
        return stateKey;
    }

    public override void OnTriggerEnter(Collider other){}

    public override void OnTriggerStay(Collider other){}

    public override void OnTriggerExit(Collider other){}

}

public class ZombieDamagedState : ZombieState{
    public ZombieDamagedState(GameObject ZombieInfo, ZombieStateMachine.EStates state): base(ZombieInfo, state){}

    public override void EnterState(){}
    public override void ExitState(){}
    public override void UpdateState(){}
    public override ZombieStateMachine.EStates GetNextState(){
        return stateKey;
    }

    public override void OnTriggerEnter(Collider other){}

    public override void OnTriggerStay(Collider other){}

    public override void OnTriggerExit(Collider other){}

}

public class ZombieDeadState : ZombieState{
    public ZombieDeadState(GameObject ZombieInfo, ZombieStateMachine.EStates state): base(ZombieInfo, state){}

    public override void EnterState(){
        zombieController.animator.SetBool("isDead", true);
        PlayAudio("DeathsSounds", 1f, 2f, true);
    }
    public override void ExitState(){}
    public override void UpdateState(){}
    public override ZombieStateMachine.EStates GetNextState(){
        return stateKey;
    }

    public override void OnTriggerEnter(Collider other){}

    public override void OnTriggerStay(Collider other){}

    public override void OnTriggerExit(Collider other){}

}
