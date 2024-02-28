using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class DisplayLoseScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject m_player;
    [SerializeField]
    private Image m_panelImage;
    [SerializeField]
    private TMP_Text m_deadText;
    [SerializeField]
    private float m_boundaryY = -10f;
    [SerializeField]
    private GameObject m_staminaBar;

    private int m_targetFontSize = 120;
    private Color m_targetedColor = Color.black;
    private float m_lerpValue = 0.15f;

    void Start()
    {
        m_panelImage.color = Color.clear;
        m_deadText.gameObject.SetActive(false);
    }

    void Update()
    {
        if(DidPlayerLose())
        {
            ActivateLoseScreen();
        }
    }

    private bool DidPlayerLose()
    {
        if (m_player.transform.position.y < m_boundaryY)
        {
            m_staminaBar.gameObject.SetActive(false);
            return true;
        }

        return false;
    }

    private void ActivateLoseScreen()
    {
        m_panelImage.color = Color.Lerp(m_panelImage.color, m_targetedColor, m_lerpValue);

        m_deadText.gameObject.SetActive(m_panelImage.color == m_targetedColor);

        if (m_deadText.IsActive())
        {
            m_deadText.fontSize = Mathf.Lerp(m_deadText.fontSize, m_targetFontSize, m_lerpValue);
        }
    }
}
