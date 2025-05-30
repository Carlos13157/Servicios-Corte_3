using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Fusion;

public abstract class StateMachine<EState> : NetworkBehaviour where EState : Enum
{
    protected Dictionary<EState, BaseState<EState>> states = new();
    protected BaseState<EState> currentState;

    protected bool isTransitioningState = false;

    protected Queue<EState> statesHistory = new Queue<EState>();

    protected event Action OnStatesInitialized;

    void Start()
    {
        OnStatesInitialized += () =>
        {
            addStateToHistory();
            currentState.EnterState();
        };
    }

    public override void FixedUpdateNetwork(){
        if (currentState == null) return;

        EState nextStateKey = currentState.GetNextState();

        if (!isTransitioningState && nextStateKey.Equals(currentState.stateKey))
        {
            currentState.UpdateState();
        }
        else
        {
            TransitionToState(nextStateKey);
        }
    }

    public void TransitionToState(EState stateKey)
    {
        isTransitioningState = true;
        currentState.ExitState();
        currentState = states[stateKey];
        currentState.EnterState();
        this.addStateToHistory();
        isTransitioningState = false;
    }

    public TEnum GetStateWithString<TEnum>(string value) where TEnum : struct, Enum
    {
        Enum.TryParse<TEnum>(value, out TEnum foundState);
        return foundState;
    }

    private void addStateToHistory()
    {
        statesHistory.Enqueue(currentState.stateKey);
        if (statesHistory.Count > 5) cleanStatesHistory();
    }

    public Queue<EState> getStateHistory()
    {
        return statesHistory;
    }

    private void cleanStatesHistory()
    {
        statesHistory.Dequeue();
    }

    public EState GetCurrentState()
    {
        return currentState.stateKey;
    }

    void OnTriggerEnter(Collider other)
    {
        currentState.OnTriggerEnter(other);
    }

    void OnTriggerStay(Collider other)
    {
        currentState.OnTriggerStay(other);
    }

    void OnTriggerExit(Collider other)
    {
        currentState.OnTriggerExit(other);
    }

    protected void InvokeOnStatesInitialized(){
        OnStatesInitialized?.Invoke();
    }
}