using Mirror;
using UnityEngine;

public class NetworkPlatformManager : NetworkBehaviour
{    
    [SerializeField] private GameObject m_platform = null;
    [SerializeField] private GameObject m_previewObject = null;
    [SerializeField] private GameObject m_previewAngleObject = null;    
    [SerializeField] private float m_rotationSpeed = 25.0f;
    [SerializeField] private float m_angleLimit = 15.0f;    
    [SerializeField] private float m_dampingSpeed = 10.0f;
    [SerializeField] private float m_pivotRadius = 0.01f;

    private Vector3 m_playersInputs = Vector3.zero;
    private Vector3 m_rotationAxis = Vector3.zero;
    private float m_noInputTimer = 0.0f;
    private float m_noInputTimerLimit = 0.1f;
    
    public static NetworkPlatformManager _Instance { get; private set; }

    private void Awake()
    {
        if (_Instance != null && _Instance != this)
        {
            Destroy(this);
        }
        _Instance = this;
    }

    private void Update()
    {
        if (isServer)
        {
            ServerUpdate();
        }

    }

    //[Server]
    private void ServerUpdate()
    {       
        Debug.DrawRay(transform.position + new Vector3(0, 2, 0), m_playersInputs * 7, Color.blue);

        m_playersInputs = m_playersInputs.normalized;
        int goIdx = NetworkManagerCustom.Instance.Identifier.GetIndex(m_platform);        
        m_noInputTimer += Time.deltaTime;

        if (m_playersInputs != Vector3.zero)
        {
            m_noInputTimer = 0;
            m_rotationAxis = Quaternion.Euler(0, 90, 0) * m_playersInputs;   

            ApplyRotatePreview(m_previewObject);            

            if (CalculatePreviewAngleFromPivot() >= m_angleLimit)
                return;

            RPC_ApplyRotate(goIdx, m_rotationAxis);            
        }
                
        if(m_playersInputs == Vector3.zero && m_noInputTimer > m_noInputTimerLimit)
        {
            RPC_ResetPosition(goIdx);
        }

        m_playersInputs = Vector3.zero;
    }
    
    private void ApplyRotatePreview(GameObject gO)
    {        
        //Debug.DrawRay(transform.position, m_rotationAxis * 10, Color.magenta);

        if (gO == null) return;

        m_previewObject.transform.position = m_platform.transform.position;
        m_previewObject.transform.rotation = m_platform.transform.rotation;

        gO.transform.position = transform.position - (-transform.up * m_pivotRadius);        
        gO.transform.RotateAround(transform.position, m_rotationAxis, m_rotationSpeed * Time.deltaTime);                
    }    

    [ClientRpc]
    private void RPC_ApplyRotate(int goIdx, Vector3 rotationAxis)
    {        
        var go = NetworkManagerCustom.Instance.Identifier.GetObjectAtIndex(goIdx);        

        go.transform.position = transform.position - (-transform.up * m_pivotRadius);
        go.transform.RotateAround(go.transform.position, rotationAxis, m_rotationSpeed * Time.deltaTime);
    }    

    [ClientRpc]
    private void RPC_ResetPosition(int goIdx)
    {        
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, transform.up);
        var go = NetworkManagerCustom.Instance.Identifier.GetObjectAtIndex(goIdx);

        go.transform.rotation = Quaternion.Slerp(go.transform.rotation, targetRotation, m_dampingSpeed * Time.deltaTime);
    }

    private float CalculatePreviewAngleFromPivot()
    {
        Vector3 pivotToObjectPreviewDir = transform.position - m_previewAngleObject.transform.TransformPoint(Vector3.zero);       
        float previewObjectToPivotDirAngle = Vector3.Angle(-Vector3.up, pivotToObjectPreviewDir);
        //Debug.Log("Preview Angle is " + previewObjectToPivotDirAngle);
        return previewObjectToPivotDirAngle;
    }

    [Server]
    public void ReceivePlayersInputs(Vector3 inputs)
    {
        //Debug.Log("ReceiveWorldInputs " + worldInputs);        
        if (inputs == Vector3.zero)
        {
            m_playersInputs = Vector3.zero;
            return;
        }

        m_playersInputs += inputs; 
    }
}