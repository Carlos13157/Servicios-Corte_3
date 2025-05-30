    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ZombieStateMachine : StateMachine<ZombieStateMachine.EStates>{
    public enum EStates{
        Idle,
        Chase,
        Search,
        Attack,
        Damaged,
        Dead
    }
    private GameObject contextObject;
    
    public void Initialize(){
        contextObject = this.gameObject;
        InitializeStates();
    }

    private void InitializeStates()
    {
        foreach (EStates state in Enum.GetValues(typeof(EStates)))
        {
            try
            {
                Type stateType = Type.GetType($"Zombie{state}State");
                if (stateType != null)
                {
                    if (stateType.IsSubclassOf(typeof(ZombieState)))
                    {
                        BaseState<ZombieStateMachine.EStates> stateInstance = Activator.CreateInstance(stateType, contextObject, state) as BaseState<ZombieStateMachine.EStates>;
                        states.Add(state, stateInstance);
                    }
                    else
                    {
                        Debug.Log($"La clase \"{stateType.Name}\" no es una instancia de ZombieState.");
                    }
                }
                else
                {
                    throw new Exception($"La clase \"Zombie{state}State\" no está definida.");
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"No se pudo cargar el estado \"{state}\": {ex}");
            }
        }

        currentState = states[EStates.Idle];
        InvokeOnStatesInitialized();
    }
    
}
