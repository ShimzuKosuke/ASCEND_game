using UnityEngine;
using System.Collections;

// BGM, SEの管理クラス
// 25/10/06 リファクタリング
public class AudioManager : MonoBehaviour
{
    [Header("スクリプト参照")]
    [SerializeField] private GameManager manager;
    [SerializeField] private TutorialManager tutorialManager; 

    [Header("BGM設定")]
    [SerializeField] private AudioClip bgmClip;

    [Header("SE設定")]
    [SerializeField] private AudioClip UI;
    [SerializeField] private AudioClip Fire;
    [SerializeField] private AudioClip Reload;
    [SerializeField] private AudioClip Hit;
    [SerializeField] private AudioClip Damage; // プレイヤーがダメージを受けたとき
    [SerializeField] private AudioClip Item_Ammo;
    [SerializeField] private AudioClip Item_Health;
    [SerializeField] private AudioClip ChargeLv3_hit;
    [SerializeField] private AudioClip ChargeLv3_unhit;
    [SerializeField] private AudioClip Charge; // チャージレベルアップ時

    [Header("Volume設定")]
    [SerializeField] private float bgmVolume = 1f;
    [SerializeField] private float uiVolume = 1f;
    [SerializeField] private float reloadVolume = 1f;
    [SerializeField] private float fireVolume = 1f;

    [Header("PlayerPrefsのキー")]
    private const string BGMVolume = "BGMVolume";
    private const string UIVolume = "UIVolume";
    private const string FireVolume = "FireVolume";

    [Header("AudioSource設定")]
    [SerializeField] private AudioSource bgmAudioSource;

    void Start()
    {
        // AudioSourceコンポーネントを取得または追加
        if (bgmAudioSource == null)
        {
            bgmAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        ApplyVolumeSettings(); // PlayerPrefsから音量を取得
        StartCoroutine(SetVolume(bgmAudioSource, bgmVolume, 0.1f)); // 音量を自動更新
    }

    private void Update()
    {
        if (manager != null)
        {
            switch (manager.mode)
            {
                case GameManager.Mode.PLAY:
                    // BGM再生
                    if (!bgmAudioSource.isPlaying)
                    {
                        bgmAudioSource.clip = bgmClip;
                        bgmAudioSource.Play();
                    }
                    break;

                case GameManager.Mode.PAUSE:
                case GameManager.Mode.STOP:
                    break;

                case GameManager.Mode.GAMEOVER:
                    if(bgmAudioSource && bgmAudioSource.isPlaying)
                    {
                        bgmAudioSource.Stop();
                    }
                    break;

            }
        }
        if(tutorialManager != null)
        {
            switch (tutorialManager.mode)
            {
                case TutorialManager.TutorialoMode.MOVE:
                    // BGM再生
                    if (!bgmAudioSource.isPlaying)
                    {
                        bgmAudioSource.clip = bgmClip;
                        bgmAudioSource.Play();
                    }
                    break;
                case TutorialManager.TutorialoMode.START:
                case TutorialManager.TutorialoMode.PAUSE:
                    bgmAudioSource.volume = bgmVolume / 2;
                    break;
            }
        }
    }

    /// <summary>
    ///  UIクリック
    /// </summary>
    public void PlayUIClick()
    {
        if (UI != null) bgmAudioSource.PlayOneShot(UI, uiVolume);
    }

    /// <summary>
    ///  射撃音
    /// </summary>
    public void PlayFireSound()
    {
        if (Fire != null) bgmAudioSource.PlayOneShot(Fire, fireVolume);
    }

    /// <summary>
    /// リロード音
    /// </summary>
    public void PlayReloadSound()
    {
        if (Reload != null) bgmAudioSource.PlayOneShot(Reload, reloadVolume);
    }

    /// <summary>
    ///  ヒット音
    /// </summary>
    public void PlayHitSound()
    {
        if (Hit != null) bgmAudioSource.PlayOneShot(Hit, fireVolume);
    }

    /// <summary>
    ///  プレイヤーダメージ音
    /// </summary>
    public void PlayDamageSound()
    {
        if (Damage != null) bgmAudioSource.PlayOneShot(Damage, fireVolume);
    }

    /// <summary>
    ///  アイテム取得音（弾薬）
    /// </summary>
    public void PlayItemAmmoSound()
    {
        if (Item_Ammo != null) bgmAudioSource.PlayOneShot(Item_Ammo, fireVolume);
    }

    /// <summary>
    ///  アイテム取得音（体力）
    /// </summary>
    public void PlayItemHealthSound()
    {
        if (Item_Health != null) bgmAudioSource.PlayOneShot(Item_Health, fireVolume);
    }

    /// <summary>
    ///  チャージLv3ヒット音
    /// </summary>
    public void PlayChargeLv3HitSound()
    {
        if (ChargeLv3_hit != null) bgmAudioSource.PlayOneShot(ChargeLv3_hit, fireVolume);
    }
    /// <summary>
    ///  チャージLv3非ヒット音
    /// </summary>
    public void PlayChargeLv3UnhitSound()
    {
        if (ChargeLv3_unhit != null) bgmAudioSource.PlayOneShot(ChargeLv3_unhit, fireVolume);
    }

    /// <summary>
    ///  チャージレベルアップ音
    /// </summary>
    public void PlayChargeSound()
    {
        if (Charge != null) bgmAudioSource.PlayOneShot(Charge, fireVolume);
    }

    // 音量設定を反映
    private void ApplyVolumeSettings()
    {
        bgmVolume = PlayerPrefs.GetFloat(BGMVolume, 1f);
        uiVolume = PlayerPrefs.GetFloat(UIVolume, 1f);
        fireVolume = PlayerPrefs.GetFloat(FireVolume, 1f);
        
    }

    // 音量を一定間隔で更新するコルーチン
    private IEnumerator SetVolume(AudioSource source, float volume, float intervalSec)
    {
        if (manager != null)
        {
            switch (manager.mode)
            {
                case GameManager.Mode.PLAY:
                    source.volume = volume;
                    break;

                case GameManager.Mode.PAUSE:
                case GameManager.Mode.STOP:
                    source.volume = volume / 2;
                    break;

                case GameManager.Mode.GAMEOVER:
                    source.volume = 0;
                    break;
            }
        }
        if (tutorialManager != null)
        {
            switch (tutorialManager.mode)
            {
                case TutorialManager.TutorialoMode.MOVE:
                    source.volume = volume;
                    break;
                case TutorialManager.TutorialoMode.START:
                case TutorialManager.TutorialoMode.PAUSE:
                    source.volume = volume / 2;
                    break;
            }
        }
        yield return new WaitForSeconds(intervalSec);
    }
}
