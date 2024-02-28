using System.Collections.Generic;
using UnityEngine;


public class RunnerSM : CC_BaseStateMachine<CharacterState>
{

    [field: SerializeField] public Camera Camera { get; private set; }
    [field: SerializeField] public Camera CinematicCamera { get; private set; }
    [field: SerializeField] public Rigidbody Rb { get; private set; }
    [field: SerializeField] private Animator Animator { get; set; }
    [field: SerializeField] public BoxCollider HitBox { get; private set; }
    [field: SerializeField] public float AccelerationValue { get; private set; } = 20.0f;
    [field: SerializeField] public float JumpAccelerationValue { get; private set; } = 500.0f;
    [field: SerializeField] public float SlowedDownAccelerationValue { get; private set; } = 7.0f;
    [field: SerializeField] public float ForwardMaxVelocity { get; private set; } = 6.0f;
    [field: SerializeField] public float LateralMaxVelocity { get; private set; } = 5.0f;
    [field: SerializeField] public float BackwardMaxVelocity { get; private set; } = 3.0f;
    [field: SerializeField] public float SlowingVelocity { get; private set; } = 0.97f;
    [field: SerializeField] public float InAirMaxVelocity { get; private set; } = 6.0f;
    [field: SerializeField] public float MaxNoDamageFall { get; private set; } = 10.0f;
    [field: SerializeField] public float RotationSpeed { get; private set; } = 3.0f;
    [field: SerializeField] public float Stamina { get; private set; } = 100.0f;
    [field: SerializeField] public float StaminaRecovery { get; private set; } = 0.5f;
    [field: SerializeField] public float StaminaInverseMultiplier { get; private set; } = 500.0f;
    [field: SerializeField] public float StaminaMaxValue { get; private set; } = 100.0f;
    [field: SerializeField] public GameObject ObjectToLookAt { get; private set; }
    [field: SerializeField] public GameObject MC { get; private set; }

    // /////////////////
    public Vector3 ForwardVectorOnFloor { get; private set; }
    public Vector3 ForwardVectorForPlayer { get; private set; }
    public Vector3 RightVectorOnFloor { get; private set; }
    public Vector3 RightVectorForPlayer { get; private set; }
    public bool IsStunned { get; private set; } = false;

    public bool IsInNonGameplay { get; private set; } = true;

    // /////////////////

    [SerializeField] private GameObject m_groundCollider;

    private float m_lerpedAngleX = 0;
    private bool m_isGrounded = false;

    // /////////////////

    [field: Header("SLOPE FORCE MECANICS")]
    [field: SerializeField] public float SlopeForceMagnitude { get; private set; } = 10.0f;
    [field: SerializeField] public float SlopeForceAngleMultiplier { get; private set; } = 1.0f;
    [field: SerializeField] public float SteepnessBeforeSlopeForce { get; private set; } = 5.0f;

    protected override void CreatePossibleStates()
    {
        m_possibleStates = new List<CharacterState>();
        m_possibleStates.Add(new NoGameplayState());
        m_possibleStates.Add(new FreeState());
        m_possibleStates.Add(new JumpState());
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        foreach (CharacterState state in m_possibleStates)
        {
            state.OnStart(this);
        }

        m_currentState = m_possibleStates[0];
        m_currentState.OnEnter();

    }
    protected override void Update()
    {
        base.Update();

        SetForwardVectorFromGroundNormal();

        SetIsGroundedAnimationBool();

        UpdateStaminaWhileRunning();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    protected override void FixedUpdate()
    {
        RotatePlayer();
        base.FixedUpdate();
    }

    private void UpdateStaminaWhileRunning()
    {

        if (Rb.velocity.magnitude < 0.5f)
        {
            if (Stamina == StaminaMaxValue)
            {
                return;
            }

            Stamina += StaminaRecovery;

            if (Stamina > StaminaMaxValue)
            {
                Stamina = StaminaMaxValue;
            }
            return;
        }

        Stamina -= Rb.velocity.magnitude / StaminaInverseMultiplier;
        if (Stamina < 0.0f)
        {
            Stamina = 0.0f;
        }
    }

    public void UpdateStaminaWhileJumping(float value)
    {

        Stamina -= value;

        if (Stamina < 0.0f)
        {
            Stamina = 0.0f;
        }
    }

    private void RotatePlayer()
    {

        if (IsInNonGameplay)
        {
            return;
        }

        float currentAngleX = Input.GetAxis("Mouse X") * RotationSpeed;
        m_lerpedAngleX = Mathf.Lerp(m_lerpedAngleX, currentAngleX, 0.1f);
        MC.transform.RotateAround(ObjectToLookAt.transform.position, ObjectToLookAt.transform.up, m_lerpedAngleX);
    }

    private void SetForwardVectorFromGroundNormal()
    {
        int layerMask = 1 << 8;
        RaycastHit hit;
        Vector3 hitNormal = Vector3.up;
        float vDiffMagnitude = 3.0f;
        Vector3 vDiff = transform.position - new Vector3(transform.position.x, transform.position.y + vDiffMagnitude, transform.position.z);

        if (Physics.Raycast(transform.position, vDiff, out hit, vDiffMagnitude, layerMask))
        {
            hitNormal = hit.normal;
        }

        ForwardVectorOnFloor = Vector3.ProjectOnPlane(MC.transform.forward, Vector3.up);
        ForwardVectorForPlayer = Vector3.ProjectOnPlane(ForwardVectorOnFloor, hitNormal);
        ForwardVectorForPlayer = Vector3.Normalize(ForwardVectorForPlayer);

        RightVectorOnFloor = Vector3.ProjectOnPlane(MC.transform.right, Vector3.up);
        RightVectorForPlayer = Vector3.ProjectOnPlane(RightVectorOnFloor, hitNormal);
        RightVectorForPlayer = Vector3.Normalize(RightVectorForPlayer);

        Debug.DrawRay(transform.position + new Vector3(0, 1, 0), ForwardVectorForPlayer, Color.red);
    }

    public bool IsInContactWithFloor()
    {
        return m_groundCollider.GetComponent<GroundDetection>().IsGrounded;
    }

    public bool IsInNoGameplayState()
    {
        return IsInNonGameplay;
    }

    public bool IsTouchingGround()
    {
        return m_groundCollider.GetComponent<GroundDetection>().TouchingGround;
    }

    public void SetIsStunnedToFalse()
    {
        IsStunned = false;
    }

    public void SetIsStunnedToTrue()
    {
        IsStunned = true;
    }

    public void HandleAttackHitbox(bool isEnabled)
    {
        HitBox.enabled = isEnabled;
    }

    public void UpdateAnimatorMovementValues(Vector2 movement)
    {
        float lateralMovement = movement.x / LateralMaxVelocity;
        float forwardMovement = movement.y;

        if (forwardMovement >= 0)
        {
            forwardMovement /= ForwardMaxVelocity;
        }
        else
        {
            forwardMovement /= BackwardMaxVelocity;
        }

        Animator.SetFloat("MoveLR", lateralMovement);
        Animator.SetFloat("MoveFB", forwardMovement);

    }

    private void SetTouchingGroundAnimationBool()
    {
        Animator.SetBool("TouchingGround", IsTouchingGround());
    }

    private void SetIsGroundedAnimationBool()
    {
        Animator.SetBool("IsGrounded", IsInContactWithFloor());
    }

    public void AddImpulseForce(Vector3 direction, float impulseForce)
    {
        //Debug.Log("addimpulse");
        Rb.AddForce(direction * impulseForce, ForceMode.Impulse);
    }

    public void SetParentGo(GameObject go)
    {
        MC = go;
    }

    public void SetAnimator(Animator animator)
    {
        Animator = animator;
    }

    public void SetRigidbody(Rigidbody rb)
    {
        Rb = rb;
    }

    public void SetIsInNonGameplay(bool value)
    {
        IsInNonGameplay = value;
    }

    public void SetCinematicCamera(GameObject go)
    {
        CinematicCamera = go.GetComponent<Camera>();
    }
}
