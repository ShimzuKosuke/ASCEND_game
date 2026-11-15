using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// Playerの射撃・リロード・ダメージ計算を管理するクラス
public class PlayerShooting : MonoBehaviour
{
    [Header("ステータス")]
    [SerializeField] public int ATK = 10;
    [SerializeField] public float range = 100f;
    [SerializeField] public int CriticalRate = 0; // パーセンテージ
    [SerializeField] public float fireRate = 0.5f;
    [SerializeField] private float fireRateTimer = 0f;
    [SerializeField] public int magazineSize = 30;
    [SerializeField] public float reloadTime = 2f;
    [SerializeField] private float reloadTimer = 2f;
    [SerializeField] public int currentAmmo;
    [SerializeField] public float recoilAngle = -2.5f;
    [SerializeField] public int hitLimit = 2; // チャージショットの貫通数
    [SerializeField] public float chargeSpeed = 1.0f; // 速度倍率
    [SerializeField] public float burstInterval = 0.7f; // バースト射撃の間隔

    // デフォルト値
    [HideInInspector] public int ATKDefault = 10;
    [HideInInspector] public float fireRateDefault = 0.1f;
    [HideInInspector] public int magazineSizeDefault = 15;
    [HideInInspector] public float reloadTimeDefault = 2.0f;
    [HideInInspector] public float rangeDefault = 100f;
    [HideInInspector] public float recoilAngleDefault = -2.5f;
    [HideInInspector] public float chargeSpeedDefault = 1.0f;
    [HideInInspector] public int hitLimitDefault = 2;
    [HideInInspector] public float burstIntervalDefault = 0.7f;

    [Header("演算用変数")]
    // ダメージ計算用
    [SerializeField] private bool isCritical = false;
    [SerializeField] private int damage;
    private RaycastHit hit;
    private Vector3 origin; // 発射位置
    private bool isHit = false; // SEの多重再生防止フラグ
    [SerializeField] private float CriticalMultiplier = 2.0f; // クリティカル時のダメージ倍率(バフ化も検討)
    [SerializeField] public int AerialAttackBonus = 0; // 空中攻撃のボーナスダメージ
    [SerializeField] public int   fireCnt { get; private set; } = 0;
    [SerializeField] public int   hitCnt  { get; private set; } = 0;
    [SerializeField] public float accuracy { get; private set; } = 0f; 

    // 状態管理用(burst)
    private float burstTimer = 0f; // バースト射撃のタイマー
    private float burstDelay = 0.05f;

    // 状態管理用(charge)
    private float chargeTime = 0f; // チャージ時間
    public int chargeLevel = 0; // チャージレベル
    public int maxChargeLevel = 3; // 最大チャージレベル(現在未使用)
    private float chargeLvThreshold_1 = 0.5f; // チャージレベル1の閾値
    private float chargeLvThreshold_2 = 1.5f; // チャージレベル2の閾値
    private float chargeLvThreshold_3 = 2.5f; // チャージレベル3の閾値

    // 状態管理用(その他)
    [SerializeField] public bool isReloading = false;   // リロードの処理を行っているか
    [SerializeField] public bool isReloadAnim = false;  // リロードアニメーションが再生中か
    [SerializeField] private float lineDuration = 0.05f; // レイキャストの可視化の持続時間
    [SerializeField] private float muzzleFlashDuration = 0.1f; // マズルフラッシュの表示時間

    [Header("コンポーネント参照")]
    [SerializeField] private LineRenderer bulletLine;
    [SerializeField] private GameObject Muzzle;
    [SerializeField] private Slider ReloadBar;
    private GameObject flash;
    [SerializeField] private GameObject MainCamera;
    [SerializeField] private Animator anim_weapon;

    [Header("スクリプト参照(共通)")]
    [SerializeField] private MuzzleFlashPool muzzleFlashPool;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private PlayerPOV POV;
    [SerializeField] private CameraShake shake;
    [SerializeField] private PlayerManager player;
    [SerializeField] private PlayerMoving move;

    [Header("スクリプト参照(Play)")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameUI UI;

    [Header("スクリプト参照(Tutorial)")]
    [SerializeField] private TutorialUI tutorialUI;

    // 射撃モード
    public enum FireMode
    {
        FULLAUTO,
        BURST,
        CHARGE,
        TOTAL // モードの数
    } public FireMode fireMode;

    /// <summary>
    /// ステータス完全初期化
    /// </summary>
    public void InitAllStatus()
    {
        ATK = ATKDefault;
        fireRate = fireRateDefault;
        magazineSize = magazineSizeDefault;
        reloadTime = reloadTimeDefault;
        range = rangeDefault;
        recoilAngle = recoilAngleDefault;
        currentAmmo = magazineSize;
        reloadTimer = reloadTime;
        CriticalRate = 0;
        isReloading = false;
        chargeSpeed = chargeSpeedDefault;
        hitLimit = hitLimitDefault;
        burstInterval = burstIntervalDefault;
        fireMode = FireMode.FULLAUTO;
        AerialAttackBonus = 0;

        if (tutorialUI != null) tutorialUI.setFireInfoText();
    }

    /// <summary>
    /// ステータス初期化(Wave開始時)
    /// </summary>
    public void InitStatus()
    {
        // リロード中にwaveが終了した場合、finishアニメーションが再生されないため、wave開始時に強制的にfinishアニメーションを再生する
        if (anim_weapon && isReloading)
        {
            anim_weapon.SetTrigger("finish");
            Debug.Log("[PlayerShootsing] InitStatus: finishアニメーションを再生");
        }

        currentAmmo = magazineSize;
        reloadTimer = reloadTime;
        burstTimer = burstInterval;
        isReloading = false;
    }

    /// <summary>
    /// 入力を受け付けて射撃・リロードを制御
    /// </summary>
    public void ShootingControl()
    {
        // L1 or Bキーで射撃モードの切り替え
        if ((Input.GetButtonDown("FireModeChange") || Input.GetKeyDown(KeyCode.B)))
        {
            fireMode++;
            if (fireMode == FireMode.TOTAL) fireMode = 0;
            audioManager.PlayUIClick();

            if(anim_weapon && !isReloading) anim_weapon.SetTrigger("modeChange");
            else Debug.LogWarning("[PlayerShooting] Weaponアニメーターが設定されていません");

            // player.isTutorialでUIを分岐
            if (!player.isTutorial) UI.showSetFireModeCange();
            if (player.isTutorial)  tutorialUI.showSetFireModeCange();


        }

        // R2 or 左クリック and 連射速度制限 and 弾薬がある and リロード中でない + モードで分岐
        switch(fireMode)
        {
            //！！条件式の構文に注意！！
            case FireMode.FULLAUTO:
                if((Input.GetButton("Fire_Pad") || Input.GetKey(KeyCode.Mouse0))
                    && CanFire())
                {
                    Fire();
                }
                break;

            case FireMode.BURST:
                if ((Input.GetButtonDown("Fire_Pad") || Input.GetKeyDown(KeyCode.Mouse0))
                    && CanFire() && burstTimer <= 0f)
                {
                    Fire();
                    Invoke("Fire", burstDelay);
                    Invoke("Fire", burstDelay * 2);
                    Invoke("Fire", burstDelay * 3);
                    burstTimer = burstInterval; // バーストタイマーをリセット
                }
                break;

            case FireMode.CHARGE:
                // 長押しでチャージ、離すと発射
                if ((Input.GetButton("Fire_Pad") || Input.GetKey(KeyCode.Mouse0))
                    && CanFire())
                {
                    chargeTime += Time.deltaTime * chargeSpeed;
                    if (chargeTime >= chargeLvThreshold_1 && chargeLevel == 0)
                    {
                        chargeLevel = 1;
                        audioManager.PlayChargeSound();
                    }
                    if (chargeTime >= chargeLvThreshold_2 && chargeLevel == 1)
                    {
                        chargeLevel = 2;
                        audioManager.PlayChargeSound();
                    }
                    if (chargeTime >= chargeLvThreshold_3 && chargeLevel == 2)
                    {
                        chargeLevel = 3;
                        audioManager.PlayChargeSound();
                    }

                }
                if (Input.GetButtonUp("Fire_Pad") || Input.GetKeyUp(KeyCode.Mouse0)
                    && CanFire())
                {
                    Fire();
                }
                break;
        }

        // L1 or R and リロード中でない
        if ((Input.GetButton("Reload") || Input.GetKeyDown(KeyCode.R)) && !isReloading)
        {
            Reload(); // リロード処理
        }

        if (isReloading)
        {
            ReloadBar.gameObject.SetActive(true);
            ReloadBar.value = 1 - (reloadTimer / reloadTime);
        }
        else ReloadBar.gameObject.SetActive(false);

    }

    //射撃
    private void Fire()
    {
        if (currentAmmo <= 0) return;

        // カメラシェイク
        if (!player.isTutorial && POV.isCamShake) shake.FireShake();
        if (player.isTutorial && POV.isCamShake) shake.FireShake();

        // 反動
        POV.AddRecoil(recoilAngle);

        currentAmmo--;
        currentAmmo = Mathf.Clamp(currentAmmo, 0, magazineSize);
        if (currentAmmo <= 0) Reload();

        // マズルフラッシュの表示
        flash = muzzleFlashPool.GetMuzzleFlash();
        flash.transform.position = Muzzle.transform.position;
        flash.transform.rotation = Muzzle.transform.rotation;
        flash.transform.SetParent(Muzzle.transform);
        StartCoroutine(ReturnToPoolAfterSeconds(flash, muzzleFlashDuration));

        if (FireMode.FULLAUTO == fireMode) fireRateTimer = fireRate;

        origin = Muzzle.transform.position;
        Camera camera = MainCamera.GetComponent<Camera>();
        Vector3 direction = POV.neck.forward; // 首の向きから発射

        isHit = false;
        Vector3 hitPoint = origin + direction * range;

        // チャージショットの貫通処理
        if (fireMode == FireMode.CHARGE && chargeLevel == 3)
        {
            // 貫通: RaycastAll
            RaycastHit[] hits = Physics.RaycastAll(origin, direction, range);
            // ソートして手前から順番に当たるようにする
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            float maxDistance = 0f; // 最も遠いヒット点の距離
            int hitValue = Mathf.Min(hits.Length, hitLimit); // ヒット点の最大数を制限

            fireCnt++;

            for (int i = 0; i< hitValue; i++)
            {
                RaycastHit hitInfo = hits[i];
                // 最も遠いヒット点を更新
                float dist = (hitInfo.point - origin).magnitude;
                if (dist > maxDistance)
                {
                    maxDistance = dist;
                    hitPoint = hitInfo.point;
                }

                if (!player.isTutorial && hitInfo.transform.CompareTag("Enemy"))
                {
                    isHit = true;
                    hitCnt += hitValue;
                    damage = ATK;
                    if(!move.isGrounded) damage += AerialAttackBonus; // 空中攻撃ボーナス加算
                    isCritical = false;
                    int criticalChance = UnityEngine.Random.Range(0, 100);
                    if (CriticalRate > criticalChance)
                    {
                        damage *= 2;
                        isCritical = true;
                    }
                    // チャージレベルに応じてダメージを増加
                    switch (chargeLevel)
                    {
                        case 3: damage = damage * 10; break;
                    }
                    hitInfo.transform.GetComponent<EnemyManager>().OnDamage(damage, isCritical);
                    gameManager.AddScore(false);
                }
                else if (player.isTutorial && hitInfo.transform.CompareTag("Dummy"))
                {
                    isHit = true;
                    hitCnt += hitValue;
                    hitInfo.transform.GetComponent<Dummy>().OnDamage(ATK, isCritical);
                    audioManager.PlayHitSound();
                }
            }
            ShowBulletLine(origin, hitPoint, chargeLevel == 3); // 可視化: 最遠のヒット点まで
            Debug.Log("hitValue: " + hitValue);
        } // if (fireMode == FireMode.CHARGE && chargeLevel == 3)
        else // 通常射撃
        {
            fireCnt++;

            // 通常: Raycast（単体ヒット）
            if (Physics.Raycast(origin, direction, out hit, range))
            {
                hitPoint = hit.point;
                ShowBulletLine(origin, hitPoint, false);

                if (!player.isTutorial && hit.transform.CompareTag("Enemy"))
                {
                    isHit = true;
                    hitCnt++;
                    damage = ATK;
                    if (!move.isGrounded) damage += AerialAttackBonus; // 空中攻撃ボーナス加算
                    isCritical = false;
                    int criticalChance = UnityEngine.Random.Range(0, 100);
                    if (CriticalRate > criticalChance)
                    {
                        damage *= 2;
                        isCritical = true;
                    }
                    // チャージレベルに応じてダメージを増加
                    switch (chargeLevel)
                    {
                        case 0: break; // 通常ショット
                        case 1: damage = damage * 3; break;
                        case 2: damage = damage * 5; break;
                    }

                    hit.transform.GetComponent<EnemyManager>().OnDamage(damage, isCritical);
                    gameManager.AddScore(false);
                }
                else if (player.isTutorial && hit.transform.CompareTag("Dummy"))
                {
                    isHit = true;
                    hitCnt++;
                    hit.transform.GetComponent<Dummy>().OnDamage(ATK, isCritical);
                    audioManager.PlayHitSound();
                }
            }
            else
            {
                ShowBulletLine(origin, hitPoint, false);
            }
        }

        // SE再生
        if (isHit)
        {
            if (chargeLevel >= 3) audioManager.PlayChargeLv3HitSound();
            else audioManager.PlayHitSound();
        }
        else
        {
            if (chargeLevel >= 3) audioManager.PlayChargeLv3UnhitSound();
            else audioManager.PlayFireSound();
        }

        isHit = false;
        chargeTime = 0f;
        chargeLevel = 0;
        accuracy = ((float)hitCnt / (float)fireCnt) * 100f;
        if (tutorialUI != null) tutorialUI.setFireInfoText();
    }

    // 射撃可能か判定
    private bool CanFire()
    {
        return fireRateTimer <= 0 && currentAmmo > 0 && !isReloading;
    }

    // リロード
    private void Reload()
    {
        if (isReloading || currentAmmo >= magazineSize) return;
        isReloading = true;
        reloadTimer = reloadTime;
        audioManager.PlayReloadSound();
        if(anim_weapon) anim_weapon.SetTrigger("reload");
        else Debug.LogWarning("[PlayerShooting] Weaponアニメーターが設定されていません");
    }

    // マズルフラッシュをプールに戻すコルーチン
    private IEnumerator ReturnToPoolAfterSeconds(GameObject obj, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        muzzleFlashPool.ReturnMuzzleFlash(obj); // マズルフラッシュをプールに戻す
    }

    // レイキャストの可視化
    private void ShowBulletLine(Vector3 start, Vector3 end, bool isFullCharge)
    {
        bulletLine.SetPosition(0, start); // レイキャストの開始位置を設定  
        bulletLine.SetPosition(1, end); // レイキャストの終了位置を設定  
        bulletLine.enabled = true;

        if (isFullCharge)
        {
            // フルチャージ時は太く表示し、赤色にする
            bulletLine.startWidth = 0.1f;
            bulletLine.endWidth = 0.1f;
            bulletLine.startColor = Color.red;
        }
        else
        {
            bulletLine.startWidth = 0.05f;
            bulletLine.endWidth = 0.05f;
            bulletLine.startColor = Color.yellow;
            bulletLine.endColor = Color.red;

        }
        Invoke(nameof(HideBulletLine), lineDuration);
    }

    // レイキャストの可視化を非表示にする
    void HideBulletLine()
    {
        bulletLine.enabled = false;
    }


    /// <summary>
    /// 各種タイマーの制御
    /// </summary>
    public void TimerControl()
    {

        if (fireRateTimer > 0) fireRateTimer -= Time.deltaTime; // タイマーを更新
        if (reloadTimer > 0) reloadTimer -= Time.deltaTime; // リロードタイマーを更新
        if (reloadTimer <= 0 && isReloadAnim) // リロードタイマーが0以下になったらリロード完了
        {
            currentAmmo = magazineSize;
            currentAmmo = Mathf.Clamp(currentAmmo, 0, magazineSize);
            isReloadAnim = false; // ここでfalseにしないとリロード完了アニメーションが重複して再生される
            if (anim_weapon) anim_weapon.SetTrigger("finish");
            else Debug.LogWarning("[PlayerShooting] Weaponアニメーターが設定されていません");
        }
        if (burstTimer > 0f && fireMode == FireMode.BURST)
        {
            burstTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// リロード完了時にReloadFinishTriggerから呼ばれる
    /// </summary>
    public void ReloadFinish()
    {
        isReloading = false; // 念のためここでもfalseにする
        isReloadAnim = false;
    }
}
