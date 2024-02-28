using UnityEngine;

public class CameraController_CharacterPlayer : MonoBehaviour
{

    [SerializeField] private Transform m_objectToLookAt;
    private Vector3 m_targetPosition;
    [SerializeField] private float m_startDistance = 5.0f;
    private float m_targetDistance;
    private float m_lerpedDistance;
    [SerializeField] private float m_scrollSpeed = 0.5f;
    [SerializeField] private float m_lerpF = 0.1f;
    [SerializeField] private float m_rotationSpeed = 5.0f;
    private float m_lerpedAngleX;
    private float m_lerpedAngleY;
    [SerializeField] private Vector2 m_clampingXRotationValues;
    [SerializeField] private Vector2 m_clampingCameraDistance;


    void Start()
    {
        m_targetDistance = m_startDistance;
    }

    private void FixedUpdate()
    {
        CalculateDistance();
        CalculateTargetPosition();

        RotateAroundObjectVertical();
        transform.position = Vector3.Lerp(transform.position, m_targetPosition, m_lerpF);

        HardSetCameraZRotation();
        MoveCameraInFrontOfObstructionsFUpdate();
    }
    void CalculateDistance()
    {
        float mouseInput = Input.mouseScrollDelta.y * m_scrollSpeed;

        if ((mouseInput < 0 && m_targetDistance > m_clampingCameraDistance.x) ||
            (mouseInput > 0 && m_targetDistance < m_clampingCameraDistance.y))
        {
            m_targetDistance += mouseInput;
        }

        m_lerpedDistance = Mathf.Lerp(m_lerpedDistance, m_targetDistance, m_lerpF);
    }

    void CalculateTargetPosition()
    {
        Vector3 CameraForwardVec = transform.forward;
        CameraForwardVec.Normalize();
        Vector3 desiredCameraOffset = CameraForwardVec * m_lerpedDistance;

        m_targetPosition = m_objectToLookAt.position - desiredCameraOffset;
    }

    void RotateAroundObjectHorizontal()
    {
        float currentAngleX = Input.GetAxis("Mouse X") * m_rotationSpeed;
        m_lerpedAngleX = Mathf.Lerp(m_lerpedAngleX, currentAngleX, m_lerpF);

        transform.RotateAround(m_objectToLookAt.position, m_objectToLookAt.up, m_lerpedAngleX);
    }

    void RotateAroundObjectVertical()
    {
        float currentAngleY = Input.GetAxis("Mouse Y") * m_rotationSpeed;
        var xRotationValue = transform.rotation.eulerAngles.x;
        float comparisonAngle = xRotationValue + currentAngleY;

        if (comparisonAngle > 180)
        {
            comparisonAngle -= 360;
        }
        if ((currentAngleY < 0 && comparisonAngle < m_clampingXRotationValues.x) ||
            (currentAngleY > 0 && comparisonAngle > m_clampingXRotationValues.y))
        {
            return;
        }

        m_lerpedAngleY = Mathf.Lerp(m_lerpedAngleY, currentAngleY, m_lerpF);

        if (comparisonAngle > m_clampingXRotationValues.x && comparisonAngle < m_clampingXRotationValues.y)
        {
            transform.RotateAround(m_objectToLookAt.position, transform.right, m_lerpedAngleY);
        }
    }

    private void HardSetCameraZRotation()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0.0f);
    }

    void MoveCameraInFrontOfObstructionsFUpdate()
    {
        int layerMask = 1 << 8;
        RaycastHit hit;

        Vector3 vDiff = transform.position - m_objectToLookAt.position;
        float distance = vDiff.magnitude;

        if (Physics.Raycast(m_objectToLookAt.position, vDiff, out hit, distance, layerMask))
        {
            //Objet détecté
            Debug.DrawRay(m_objectToLookAt.position, vDiff.normalized * hit.distance, Color.yellow);

            transform.SetPositionAndRotation(hit.point, transform.rotation);
        }
        else
        {
            //Objet non détecté
            Debug.DrawRay(m_objectToLookAt.position, vDiff, Color.white);
        }
    }

}