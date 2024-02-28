using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class ShooterUIManager : NetworkBehaviour
{

    [SerializeField]
    private Image[] m_playerArrows;
    [SerializeField]
    private Image[] m_platformArrows;

    private Vector3 m_arrowOneInitialPosition = new Vector3();

    private Vector3 m_arrowTwoInitialPosition = new Vector3();
    [SerializeField]
    private GameObject m_shooterCanvas;

    private void Start()
    {
        m_arrowOneInitialPosition = m_playerArrows[0].gameObject.transform.localPosition;
        m_arrowTwoInitialPosition = m_playerArrows[1].gameObject.transform.localPosition;
    }

    private void Update()
    {
    }

    public void MovePlayerArrow(int index, bool selectingBomb)
    {
        if (selectingBomb)
        {
            m_playerArrows[index].transform.localPosition = new Vector3(17.5f, 111, 0);
            CMDMovePlayerArrow(index, selectingBomb);
        }
        else
        {
            if (index == 0)
            {
                m_playerArrows[index].transform.localPosition = m_arrowOneInitialPosition;
            }
            else
            {
                m_playerArrows[index].transform.localPosition = m_arrowTwoInitialPosition;
            }
            CMDMovePlayerArrow(index, selectingBomb);
        }
    }

    [Command(requiresAuthority = false)]
    private void CMDMovePlayerArrow(int index, bool selectingBomb)
    {
        if (selectingBomb)
        {
            m_playerArrows[index].transform.localPosition = new Vector3(17.5f, 111, 0);
        }
        else
        {
            if (index == 0)
            {
                m_playerArrows[index].transform.localPosition = m_arrowOneInitialPosition;
            }
            else
            {
                m_playerArrows[index].transform.localPosition = m_arrowTwoInitialPosition;
            }
        }
    }

    //[ClientRpc]
    public void RPCMovePlayerArrow(int index, bool selectingBomb)
    {
        if (selectingBomb)
        {
            m_playerArrows[index].transform.localPosition = new Vector3(17.5f, 111, 0);
        }
        else
        {
            if (index == 0)
            {
                m_playerArrows[index].transform.localPosition = m_arrowOneInitialPosition;
            }
            else
            {
                m_playerArrows[index].transform.localPosition = m_arrowTwoInitialPosition;
            }
        }
    }
}