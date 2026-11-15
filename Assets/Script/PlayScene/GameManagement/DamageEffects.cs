using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition; // HDRP用

// ポストエフェクトを管理するクラス
// 25/10/06 リファクタリング
public class DamageEffects : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Volume volume; // HDRPのVolumeコンポーネント

    [Header("低体力時の設定")]
    [SerializeField] private float lowHpThreshold = 0.3f;          // 低体力とみなす閾値(割合)
    [SerializeField] private Color lowHpVignetteColor = Color.red; // 低体力時のビネットカラー
    [SerializeField] private float lowHpVignetteIntensity = 0.4f;  // 低体力時のビネット強度

    [Header("被弾時の設定")]
    [SerializeField] private float hitChromaticIntensity = 1.0f; // 被弾時の色収差強度
    [SerializeField] private float hitVignetteExtra = 0.5f;      // 被弾時のビネット強度上乗せ分
    [SerializeField] private float hitEffectDuration = 0.2f;     // 被弾エフェクトの持続時間

    private Vignette vignette; // ビネットエフェクト
    private ChromaticAberration chromatic; // 色収差エフェクト
    private float hitTimer = 0f; // 被弾エフェクトのタイマー
    private bool isLowHP = false;

    // 外部から渡される想定(割合計算用)
    private float currentHP = 1.0f;
    private float maxHP = 1.0f;

    void Start()
    {
        // TryGet : 成功すればtrueを返し、outパラメータに取得したコンポーネントを格納
        if(volume.profile.TryGet(out vignette) && volume.profile.TryGet(out chromatic))
        {
            vignette.intensity.value = 0f;
            chromatic.intensity.value = 0f;
            vignette.color.value = lowHpVignetteColor;
        }
        else
        {
            Debug.LogError("VolumeプロファイルにVignetteまたはChromaticAberrationがありません。");
        }
    }

    private void Update()
    {
        if (vignette == null || chromatic == null)
        {
            Debug.LogWarning("vignetteまたはchromaticがnullです");
        }

        // ↓コルーチン化を検討
        // 低体力判定
        float HPRate = currentHP / maxHP;
        if (HPRate <= lowHpThreshold) isLowHP = true;
        else isLowHP = false;

        // ===== 低体力時エフェクト =====
        if (isLowHP)
        {
            // Lerp : aからbへtの割合で線形補間
            // HPRateが0.3→0に近づくほど1に近づくので、1 - (HPRate / lowHpThreshold)とする
            vignette.intensity.value = Mathf.Lerp(
                0f, lowHpVignetteIntensity, 1 - (HPRate / lowHpThreshold));
        }
        else // 低体力でない場合は徐々に弱める
        {
            vignette.intensity.value = Mathf.Lerp(
                vignette.intensity.value, 0f, Time.deltaTime * 5f);
        }

        // ===== 被弾エフェクト =====
        if (hitTimer > 0f)
        {
            hitTimer -= Time.deltaTime;
            float t = hitTimer / hitEffectDuration;
            chromatic.intensity.value = Mathf.Lerp(0f, hitChromaticIntensity, t);
            vignette.intensity.value += Mathf.Lerp(0f, hitVignetteExtra, t);
        }
    }

    /// <summary>
    /// 被弾時に呼ぶ
    /// </summary>
    public void OnHit(float playerHP, float maxHP)
    {
        hitTimer = hitEffectDuration; 
        currentHP = playerHP;
        this.maxHP = maxHP;
    }

    /// <summary>
    /// 回復時に呼ぶ
    /// </summary>
    public void OnHeal(float playerHP, float maxHP)
    {
        // 被弾エフェクトは出さずに体力だけ更新
        currentHP = playerHP;
        this.maxHP = maxHP;
    }

    /// <summary>
    /// ポストエフェクト効果を初期化する
    /// </summary>
    public void Initialize()
    {
        if (vignette == null || chromatic == null) return;
        vignette.intensity.value = 0f;
        chromatic.intensity.value = 0f;
        hitTimer = 0f;
        isLowHP = false;
        currentHP = 1.0f;
        maxHP = 1.0f;
    }
}
