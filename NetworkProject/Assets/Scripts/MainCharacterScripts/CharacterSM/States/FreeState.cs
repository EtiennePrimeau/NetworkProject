using UnityEngine;

public class FreeState : CharacterState
{
    private bool m_isMovingForward = false;
    private bool m_isMovingLateral = false;
    private bool m_isMovingBackward = false;    

    public override void OnEnter()
    {
        Debug.Log("Entering FreeState");
    }

    public override void OnFixedUpdate()
    {
        AddForceFromInputs();          

        ApplySlopeForce();
        
        CapMaximumSpeed();
    }

    void ApplySlopeForce()
    {
        RaycastHit hit;
        int layerMask = 1 << 8;

        if (Physics.Raycast(m_stateMachine.MC.transform.position + new Vector3(0, 2, 0), Vector3.down, out hit, Mathf.Infinity, layerMask))
        {            
            float groundAngle = Vector3.Angle(hit.normal, Vector3.up);                      

            if (groundAngle > m_stateMachine.SteepnessBeforeSlopeForce && groundAngle < 90)
            {
                float slopeMultiplier = m_stateMachine.SlopeForceAngleMultiplier + (groundAngle / 90.0f);
                float forceMagnitude = m_stateMachine.SlopeForceMagnitude * slopeMultiplier * Mathf.Sin(Mathf.Deg2Rad * groundAngle);

                //Debug.Log("The ground angle is " + groundAngle + " the slope multiplier is " + slopeMultiplier + " the force magnitude is " + forceMagnitude);

                Vector3 slopeDirection = Vector3.ProjectOnPlane(-hit.normal, Vector3.up).normalized;
                Vector3 slopeForce = -slopeDirection * forceMagnitude;
                
                //Debug.DrawRay(m_stateMachine.MC.transform.position + new Vector3(0, 2, 0), slopeForce, Color.blue);
                
                m_stateMachine.Rb.AddForce(slopeForce, ForceMode.Acceleration);
            }
        }
    }   

    private void AddForceFromInputs()
    {
        Vector2 inputs = Vector2.zero;

        m_isMovingForward = false;
        m_isMovingLateral = false;
        m_isMovingBackward = false;

        if (Input.GetKey(KeyCode.W))
        {
            inputs.y += 1;

            m_isMovingForward = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputs.y -= 1;

            m_isMovingBackward = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputs.x -= 1;

            m_isMovingLateral = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputs.x += 1;

            m_isMovingLateral = true;
        }

        inputs.Normalize();

        if (m_stateMachine.Stamina <= 0.0f)
        {
            return;
        }
        m_stateMachine.Rb.AddForce(inputs.y * m_stateMachine.ForwardVectorForPlayer * m_stateMachine.AccelerationValue,
                ForceMode.Acceleration);
        m_stateMachine.Rb.AddForce(inputs.x * m_stateMachine.RightVectorForPlayer * m_stateMachine.AccelerationValue,
                ForceMode.Acceleration);
    }

    private void CapMaximumSpeed()
    {
        FlatTerrainCapMaximumSpeed();
    }   

    private void SlopedTerrainCapMaximumSpeed(RaycastHit hit, float groundAngle)
    {
        float slopeMultiplier = 1.0f + (groundAngle / 90.0f);

        float forwardMaxVelocity = 0.0f;
        float lateralMaxVelocity = 0.0f;
        float backwardMaxVelocity = 0.0f;

        if (m_stateMachine.Rb.velocity.magnitude > 0)
        {
            float velocityDirection = Vector3.Dot(m_stateMachine.Rb.velocity.normalized, -hit.normal);

            if (velocityDirection > 0)
            {
                forwardMaxVelocity = Mathf.Lerp(m_stateMachine.ForwardMaxVelocity, m_stateMachine.ForwardMaxVelocity * 2, slopeMultiplier);
                lateralMaxVelocity = Mathf.Lerp(m_stateMachine.LateralMaxVelocity, m_stateMachine.LateralMaxVelocity, slopeMultiplier);
                backwardMaxVelocity = Mathf.Lerp(m_stateMachine.BackwardMaxVelocity, m_stateMachine.BackwardMaxVelocity * 2, slopeMultiplier);
            }
            else
            {
                forwardMaxVelocity = Mathf.Lerp(m_stateMachine.ForwardMaxVelocity, m_stateMachine.ForwardMaxVelocity / 2, slopeMultiplier);
                lateralMaxVelocity = Mathf.Lerp(m_stateMachine.LateralMaxVelocity, m_stateMachine.LateralMaxVelocity, slopeMultiplier);
                backwardMaxVelocity = Mathf.Lerp(m_stateMachine.BackwardMaxVelocity, m_stateMachine.BackwardMaxVelocity / 2, slopeMultiplier);
            }

            Debug.DrawRay(m_stateMachine.MC.transform.position + new Vector3(0, 2, 0), m_stateMachine.Rb.velocity, Color.green);
        }

        if (m_isMovingForward)
        {
            if (m_isMovingLateral)
            {
                float diagonalMaxVelocity = (forwardMaxVelocity + lateralMaxVelocity) / 2;

                if (m_stateMachine.Rb.velocity.magnitude < diagonalMaxVelocity)
                {
                    return;
                }
                m_stateMachine.Rb.velocity = Vector3.Normalize(m_stateMachine.Rb.velocity);
                m_stateMachine.Rb.velocity *= diagonalMaxVelocity;
                return;
            }
            if (m_stateMachine.Rb.velocity.magnitude < forwardMaxVelocity)
            {
                return;
            }

            m_stateMachine.Rb.velocity = Vector3.Normalize(m_stateMachine.Rb.velocity);
            m_stateMachine.Rb.velocity *= forwardMaxVelocity;
            return;
        }
        if (m_isMovingLateral)
        {
            if (m_isMovingBackward)
            {
                float diagonalMaxVelocity = (lateralMaxVelocity + backwardMaxVelocity) / 2;

                if (m_stateMachine.Rb.velocity.magnitude < diagonalMaxVelocity)
                {
                    return;
                }
                m_stateMachine.Rb.velocity = Vector3.Normalize(m_stateMachine.Rb.velocity);
                m_stateMachine.Rb.velocity *= diagonalMaxVelocity;
                return;
            }
            if (m_stateMachine.Rb.velocity.magnitude < lateralMaxVelocity)
            {
                return;
            }
            m_stateMachine.Rb.velocity = Vector3.Normalize(m_stateMachine.Rb.velocity);
            m_stateMachine.Rb.velocity *= lateralMaxVelocity;
            return;
        }
        if (m_isMovingBackward)
        {
            if (m_stateMachine.Rb.velocity.magnitude < backwardMaxVelocity)
            {
                return;
            }
            m_stateMachine.Rb.velocity = Vector3.Normalize(m_stateMachine.Rb.velocity);
            m_stateMachine.Rb.velocity *= backwardMaxVelocity;
        }
        else
        {
            if (m_stateMachine.Rb.velocity.magnitude > 0)
            {
                m_stateMachine.Rb.velocity *= m_stateMachine.SlowingVelocity;
            }
        }
    }

    private void FlatTerrainCapMaximumSpeed()
    {
        if (m_isMovingForward)
        {
            if (m_isMovingLateral)
            {
                float diagonalMaxVelocity = (m_stateMachine.ForwardMaxVelocity + m_stateMachine.LateralMaxVelocity) / 2;

                if (m_stateMachine.Rb.velocity.magnitude < diagonalMaxVelocity)
                {
                    return;
                }
                m_stateMachine.Rb.velocity = Vector3.Normalize(m_stateMachine.Rb.velocity);
                m_stateMachine.Rb.velocity *= diagonalMaxVelocity;
                return;
            }
            if (m_stateMachine.Rb.velocity.magnitude < m_stateMachine.ForwardMaxVelocity)
            {
                return;
            }

            m_stateMachine.Rb.velocity = Vector3.Normalize(m_stateMachine.Rb.velocity);
            m_stateMachine.Rb.velocity *= m_stateMachine.ForwardMaxVelocity;
            return;
        }
        if (m_isMovingLateral)
        {
            if (m_isMovingBackward)
            {
                float diagonalMaxVelocity = (m_stateMachine.LateralMaxVelocity + m_stateMachine.BackwardMaxVelocity) / 2;

                if (m_stateMachine.Rb.velocity.magnitude < diagonalMaxVelocity)
                {
                    return;
                }
                m_stateMachine.Rb.velocity = Vector3.Normalize(m_stateMachine.Rb.velocity);
                m_stateMachine.Rb.velocity *= diagonalMaxVelocity;
                return;
            }
            if (m_stateMachine.Rb.velocity.magnitude < m_stateMachine.LateralMaxVelocity)
            {
                return;
            }
            m_stateMachine.Rb.velocity = Vector3.Normalize(m_stateMachine.Rb.velocity);
            m_stateMachine.Rb.velocity *= m_stateMachine.LateralMaxVelocity;
            return;
        }
        if (m_isMovingBackward)
        {
            if (m_stateMachine.Rb.velocity.magnitude < m_stateMachine.BackwardMaxVelocity)
            {
                return;
            }
            m_stateMachine.Rb.velocity = Vector3.Normalize(m_stateMachine.Rb.velocity);
            m_stateMachine.Rb.velocity *= m_stateMachine.BackwardMaxVelocity;
        }
        else
        {
            if (m_stateMachine.Rb.velocity.magnitude > 0)
            {
                m_stateMachine.Rb.velocity *= m_stateMachine.SlowingVelocity;
            }
        }
    }


    public override void OnUpdate()
    {
        SendAnimatorValuesToSM();
    }

    private void SendAnimatorValuesToSM()
    {
        float forwardComponent = Vector3.Dot(m_stateMachine.Rb.velocity, m_stateMachine.ForwardVectorForPlayer); 
        float lateralComponent = Vector3.Dot(m_stateMachine.Rb.velocity, m_stateMachine.RightVectorForPlayer); 
        m_stateMachine.UpdateAnimatorMovementValues(new Vector2(lateralComponent, forwardComponent)); 
    }

    public override void OnExit()
    {
        Debug.Log("Exiting FreeState");
    }

    public override bool CanEnter(CC_IState currentState)
    {
        if (currentState is NoGameplayState)
        {
            return true;
        }
        if (currentState is JumpState)
        {
            return m_stateMachine.IsInContactWithFloor();
        }
        return false;
    }
    public override bool CanExit()
    {
        return true;
    }

}
