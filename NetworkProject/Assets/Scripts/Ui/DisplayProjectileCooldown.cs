using UnityEngine;
using UnityEngine.UI;

public class DisplayProjectileCooldown : MonoBehaviour
{
    [SerializeField]
    private Image m_bombCooldownImage;
    [SerializeField]
    private Image m_bigBombCooldownImage;
    private Shooter m_shooterScript;

    void Update()
    {
        UpdateBombCooldownImage(m_shooterScript.GetBulletRemainingPercentage());
        UpdateBigBombCooldownImage(NetworkMatchManager._Instance.GetBombRemainingPercentage());
    }

    private void UpdateBombCooldownImage(float cooldown)
    {
        m_bombCooldownImage.fillAmount = cooldown;
    }

    private void UpdateBigBombCooldownImage(float cooldown)
    {
        m_bigBombCooldownImage.fillAmount = cooldown;
    }

    public void SetShooterScript(Shooter script)
    {
        m_shooterScript = script;
    }
}
