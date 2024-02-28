using UnityEngine;

public class JumpState : CharacterState
{
    private const float STAMINA_DECREASE = 5.0f;
    private const float GROUNDCHECK_DELAY_TIMER = 1.0f;
    private float m_currentGCDelayTimer = 0.0f;
    private bool m_hasDoubleJumped = false;

    public override void OnEnter()
    {
        //Debug.Log("Entering JumpState");

        m_hasDoubleJumped = false;
        Jump();
        m_currentGCDelayTimer = GROUNDCHECK_DELAY_TIMER;
    }

    public override void OnFixedUpdate()
    {
        AddForceFromInputs();

        CapMaximumSpeed();
    }

    private void AddForceFromInputs()
    {
        Vector2 inputs = Vector2.zero;

        if (Input.GetKey(KeyCode.W))
        {
            inputs.y += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputs.y -= 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputs.x -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputs.x += 1;
        }
        inputs.Normalize();

        m_stateMachine.Rb.AddForce(inputs.y * m_stateMachine.ForwardVectorForPlayer * m_stateMachine.SlowedDownAccelerationValue,
                ForceMode.Acceleration);
        m_stateMachine.Rb.AddForce(inputs.x * m_stateMachine.RightVectorForPlayer * m_stateMachine.SlowedDownAccelerationValue,
                ForceMode.Acceleration);
    }

    private void CapMaximumSpeed()
    {
        if (m_stateMachine.Rb.velocity.magnitude < m_stateMachine.InAirMaxVelocity)
        {
            return;
        }

        m_stateMachine.Rb.velocity = Vector3.Normalize(m_stateMachine.Rb.velocity);
        m_stateMachine.Rb.velocity *= m_stateMachine.InAirMaxVelocity;
    }

    private void Jump()
    {
        m_stateMachine.Rb.AddForce(Vector3.up * m_stateMachine.JumpAccelerationValue,
        ForceMode.Impulse);

        m_stateMachine.UpdateStaminaWhileJumping(STAMINA_DECREASE);

    }

    public override void OnUpdate()
    {
        if (!m_hasDoubleJumped && 
            m_stateMachine.Stamina > STAMINA_DECREASE &&
            m_currentGCDelayTimer < GROUNDCHECK_DELAY_TIMER - 0.2f)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_stateMachine.Rb.velocity = new Vector3(m_stateMachine.Rb.velocity.x, 0, m_stateMachine.Rb.velocity.z);
                Jump();
                m_hasDoubleJumped = true;
            }
        }
        
        m_currentGCDelayTimer -= Time.deltaTime;
    }

    public override void OnExit()
    {
        //Debug.Log("Exiting JumpState");
    }

    public override bool CanEnter(CC_IState currentState)
    {
        if (currentState is FreeState)
        {
            if (!m_stateMachine.IsInContactWithFloor())
            {
                return false;
            }
            if (m_stateMachine.Stamina < STAMINA_DECREASE)
            {
                return false;
            }
            return Input.GetKeyDown(KeyCode.Space);
        }
        return false;
    }
    public override bool CanExit()
    {
        if (m_currentGCDelayTimer <= 0)
        {
            return m_stateMachine.IsInContactWithFloor(); 
        }
        return false;
    }

}
