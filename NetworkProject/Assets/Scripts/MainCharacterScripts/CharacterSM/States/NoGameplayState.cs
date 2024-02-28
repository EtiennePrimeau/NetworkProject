using UnityEngine;

public class NoGameplayState : CharacterState
{


    public override void OnEnter()
    {
        //Debug.Log("Entered NoGameplayState");

        m_stateMachine.CinematicCamera.enabled = true;
        m_stateMachine.Camera.enabled = false;
    }

    public override void OnFixedUpdate()
    {
    }

    public override void OnUpdate()
    {
    }

    public override void OnExit()
    {
        //Debug.Log("Exited NoGameplayState");

        m_stateMachine.Camera.enabled = true;
        m_stateMachine.CinematicCamera.enabled = false;
    }

    public override bool CanEnter(CC_IState currentState)
    {
        return m_stateMachine.IsInNoGameplayState();
    }

    public override bool CanExit()
    {
        return !m_stateMachine.IsInNoGameplayState();
    }
}
