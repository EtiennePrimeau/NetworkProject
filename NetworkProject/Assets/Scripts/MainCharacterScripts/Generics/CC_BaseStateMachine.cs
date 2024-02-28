using System.Collections.Generic;
using UnityEngine;

public abstract class CC_BaseStateMachine<T> : MonoBehaviour where T : CC_IState
{
    protected T m_currentState;
    protected List<T> m_possibleStates;

    protected virtual void Awake()
    {
        CreatePossibleStates();
    }

    protected virtual void Start()
    {
        foreach (CC_IState state in m_possibleStates)
        {
            state.OnStart();
        }

        m_currentState = m_possibleStates[0];
        m_currentState.OnEnter();
    }

    protected virtual void Update()
    {
        m_currentState.OnUpdate();
        TryTransitionningState();
    }

    protected virtual void FixedUpdate()
    {
        m_currentState.OnFixedUpdate();
    }

    protected virtual void CreatePossibleStates()
    {
    }

    protected void TryTransitionningState()
    {
        if (!m_currentState.CanExit())
        {
            return;
        }

        foreach (var state in m_possibleStates)
        {
            if (m_currentState.Equals(state))
            {
                continue;
            }

            if (state.CanEnter(m_currentState))
            {
                m_currentState.OnExit();
                m_currentState = state;
                m_currentState.OnEnter();
                return;
            }
        }
    }
}
