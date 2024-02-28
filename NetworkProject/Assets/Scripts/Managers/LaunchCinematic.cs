using Mirror;
using UnityEngine;


//public enum EPlayerType
//{
//    Runner,
//    Shooter
//}
public class LaunchCinematic : NetworkBehaviour
{
    [SerializeField] private Camera m_camera;
    [SerializeField] private GameObject m_player;
    [SerializeField] private EPlayerType m_playerType;
    [SerializeField] private Transform m_objectToLookAt;
    [SerializeField] private AnimationCurve m_horizontalCurve;
    [SerializeField] private AnimationCurve m_verticalCurve;
    [SerializeField] private Vector3 m_startPosition;
    [SerializeField] private float m_lerpF = 0.1f;
    [SerializeField] private float m_rotationSpeed = 5.0f;
    [SerializeField] private float m_duration = 5.0f;
    [SerializeField] private float m_timer = 0.0f;
    private float m_lerpedAngleX;

    private bool m_hasStarted = false;


    private void Start()
    {
        transform.position = m_startPosition;
    }

    [ClientRpc]
    public void RPC_Launch()
    {
        //Debug.Log("rpc_launch");

        m_timer = 0.0f;
        m_hasStarted = true;
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            m_camera.enabled = false;
            return;
        }
            
        if (!m_hasStarted)
        {
            return;
        }
        if (m_timer > m_duration)
        {
            if (isServer)
            {
                NetworkMatchManager._Instance.StartGameTimer();
            }
            EndPlayerCinematic();
            return;
        }

        m_timer += Time.fixedDeltaTime;
        
        RotateAroundObjectHorizontal();
        MoveVertically();
    }


    private void RotateAroundObjectHorizontal()
    {
        float currentAngleX = m_horizontalCurve.Evaluate(m_timer / m_duration) * m_rotationSpeed;
        m_lerpedAngleX = Mathf.Lerp(m_lerpedAngleX, currentAngleX, m_lerpF);

        transform.RotateAround(m_objectToLookAt.position, m_objectToLookAt.up, m_lerpedAngleX);
    }

    private void MoveVertically()
    {
        float y = m_verticalCurve.Evaluate(m_timer / m_duration);
        Vector3 movement = new Vector3(0 ,y/2, 0);

        transform.position += movement;
    }

    private void EndPlayerCinematic()
    {
        switch (m_playerType)
        {
            case EPlayerType.Runner:
                var runnerSM = m_player.GetComponentInChildren<RunnerSM>();
                if (runnerSM != null)
                {
                    runnerSM.SetIsInNonGameplay(false);
                }
                break;
            case EPlayerType.Shooter:
                var controller = m_player.GetComponentInChildren<LocalLevelPlayerController>();
                if (controller != null)
                {
                    controller.SetIsInNonGameplay(false);
                }
                break;
            default:
                break;
        }
    }

    public void SetPlayer(GameObject go, bool isRunner)
    {
        m_player = go;
        if (isRunner)
            m_playerType = EPlayerType.Runner;
        else
            m_playerType = EPlayerType.Shooter;
    }
}
