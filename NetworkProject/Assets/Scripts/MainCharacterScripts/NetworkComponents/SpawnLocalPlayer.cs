using Mirror;
using UnityEngine;

public class SpawnLocalPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject m_cinematicGo;
    [SerializeField] private GameObject m_localPlayerPrefab;
    [SerializeField] private bool isRunner;

    [field: Header("RUNNERS")]
    [SerializeField] private Animator m_animator;
    [SerializeField] private Rigidbody m_rb;

    [field: Header("SHOOTERS")]
    [SerializeField] private NetworkLevelPlayerController m_controller;
    [SerializeField] private Shooter m_shooter;



    private void Start()
    {
        NetworkManagerCustom.Instance.Identifier.AssignSingleId(transform);
        
        if (!isLocalPlayer)
        {
            return;
        }

        if (isRunner)
        {
            var go = Instantiate(m_localPlayerPrefab, transform);
            
            var childSM = go.GetComponentInChildren<RunnerSM>();
            childSM.SetAnimator(m_animator);
            childSM.SetRigidbody(m_rb);
            childSM.SetParentGo(gameObject);
            childSM.SetCinematicCamera(m_cinematicGo);


        }
        else
        {
            var go = Instantiate(m_localPlayerPrefab, transform);
            
            var childController = go.GetComponentInChildren<LocalLevelPlayerController>();
            childController.SetNetworkController(m_controller);
            childController.SetParentGo(gameObject);

            var camera = go.GetComponentInChildren<Camera>();
            m_shooter.SetCamera(camera);

            childController.SetCinematicCamera(m_cinematicGo);
            childController.SetCooldownDisplay(m_shooter);

        }
    }
}
