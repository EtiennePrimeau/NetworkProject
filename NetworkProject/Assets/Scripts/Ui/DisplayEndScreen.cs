using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class DisplayEndScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject m_player;
    [SerializeField]
    private Image m_losePanelImage;
    [SerializeField]
    private TMP_Text m_loseText;
    [SerializeField]
    private Image m_winPanelImage;
    [SerializeField]
    private TMP_Text m_winText;
    [SerializeField]
    private float m_boundaryY = -10f;
    [SerializeField]
    private GameObject m_staminaBar;
    [SerializeField]
    private GameObject m_bulletSlots;
    [SerializeField]
    private GameObject m_tiltIndicator;

    private int m_targetFontSize = 120;
    private Color m_targetedLoseColor = Color.black;
    private Color m_targetedWinColor = Color.white;
    private float m_lerpValue = 0.15f;

    void Start()
    {
        m_losePanelImage.color = Color.clear;
        m_loseText.gameObject.SetActive(false);
        m_winPanelImage.color = Color.clear;
        m_winText.gameObject.SetActive(false);
    }

    void Update()
    {
        if(DidPlayerLose())
        {
            ActivateLoseScreen();
        }
        else if(DidPlayerWin())
        {
            ActivateWinScreen();
        }
    }

    private bool DidPlayerLose()
    {
        return false;
    }

    private void ActivateLoseScreen()
    {
        m_losePanelImage.color = Color.Lerp(m_losePanelImage.color, m_targetedLoseColor, m_lerpValue);

        m_loseText.gameObject.SetActive(m_losePanelImage.color == m_targetedLoseColor);

        if (m_loseText.IsActive())
        {
            m_loseText.fontSize = Mathf.Lerp(m_loseText.fontSize, m_targetFontSize, m_lerpValue);
        }
    }

    private bool DidPlayerWin()
    {
        return false;
    }

    private void ActivateWinScreen()
    {
        m_winPanelImage.color = Color.Lerp(m_winPanelImage.color, m_targetedWinColor, m_lerpValue);

        m_winText.gameObject.SetActive(m_winPanelImage.color == m_targetedWinColor);

        if (m_winText.IsActive())
        {
            m_winText.fontSize = Mathf.Lerp(m_winText.fontSize, m_targetFontSize, m_lerpValue);
        }
    }
}
