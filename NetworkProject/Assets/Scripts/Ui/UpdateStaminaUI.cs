using UnityEngine;
using UnityEngine.UI;

public class UpdateStaminaUI : MonoBehaviour
{
    [SerializeField]
    private Image m_staminaImage;
    [SerializeField]
    private RunnerSM m_runnerSM;

    void Update()
    {
        m_staminaImage.fillAmount = m_runnerSM.Stamina / 100;
    }
}
