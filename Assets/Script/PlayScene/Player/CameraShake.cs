using UnityEngine;
using Cinemachine;

// ƒJƒƒ‰U“®‚ğŠÇ—‚·‚éƒNƒ‰ƒX
// 25/10/08 ƒŠƒtƒ@ƒNƒ^ƒŠƒ“ƒO
public class CameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    private CinemachineBasicMultiChannelPerlin noise;
    private float shakeTimer;

    [Header("ƒJƒƒ‰U“®İ’è")]
    [SerializeField] private float giveDMGShakeDuration = 0.1f;  // ƒ_ƒ[ƒW‚ğó‚¯‚½‚Æ‚«‚ÌU“®ŠÔ
    [SerializeField] private float giveDMGShakeAmplitude = 1.5f; // ƒ_ƒ[ƒW‚ğó‚¯‚½‚Æ‚«‚ÌU“®‹­‚³
    [SerializeField] private float giveDMGShakeFrequency = 3.0f; // ƒ_ƒ[ƒW‚ğó‚¯‚½‚Æ‚«‚ÌU“®ü”g”
    [SerializeField] private float fireShakeDuration = 0.1f;  // ”­–C‚ÌU“®ŠÔ
    [SerializeField] private float fireShakeAmplitude = 1.0f; // ”­–C‚ÌU“®‹­‚³
    [SerializeField] private float fireShakeFrequency = 3.0f; // ”­–C‚ÌU“®ü”g”
    void Start()
    {
        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0f)
            {
                noise.m_AmplitudeGain = 0f;
                noise.m_FrequencyGain = 0f;
            }
        }
    }

    /// <summary>
    /// ‚»‚Ì‘¼‚ÌU“®Bˆø”: U“®‚Ì‘±ŠÔ, U“®‚Ì‹­‚³, U“®‚Ìü”g”
    /// </summary>
    public void OtherShake(float duration, float amplitude, float frequency)
    {
        if (noise == null) return;
        noise.m_AmplitudeGain = amplitude;
        noise.m_FrequencyGain = frequency;
        shakeTimer = duration;
    }

    /// <summary>
    /// ”í’e‚ÌƒJƒƒ‰U“®
    /// </summary>
    public void GiveDamageShake()
    {
        if (noise == null) return;
        noise.m_AmplitudeGain = giveDMGShakeAmplitude;
        noise.m_FrequencyGain = giveDMGShakeFrequency;
        shakeTimer = giveDMGShakeDuration;
    }

    /// <summary>
    /// ËŒ‚‚ÌƒJƒƒ‰U“®
    /// </summary>
    public void FireShake()
    {
        if (noise == null) return;
        noise.m_AmplitudeGain = fireShakeAmplitude;
        noise.m_FrequencyGain = fireShakeFrequency;
        shakeTimer = fireShakeDuration;
    }
}
