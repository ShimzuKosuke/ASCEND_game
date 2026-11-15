using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

// 敵の移動・攻撃・被弾・死亡処理を管理するクラス
// 25/10/06 リファクタリング
public class EnemyManager : MonoBehaviour
{
    [Header("スクリプト参照")]
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private EnemyDamageUI damageUI;
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private EnemyItemDroper itemDroper;

    [Header("コンポーネント参照")]
    [SerializeField] private Transform target;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private GameObject killEffectPrefab;
    [SerializeField] private Slider enemyHPBar;
    [SerializeField] private GameObject Muzzle;
    [SerializeField] private LineRenderer bulletLine;

    [Header("敵のステータス")]
    [SerializeField] public int HP = 50;
    [SerializeField] public int HPMax = 50;
    [SerializeField] public int HPDefault { get; private set; } = 50;
    [SerializeField] private float SPD = 4.0f;
    [SerializeField] private float SPDDefault = 4.0f;
    [SerializeField] public int ATK = 5;
    [SerializeField] private int ATKDefault = 5;
    [SerializeField] private float fireRate = 3; // 弾を発射する間隔
    [SerializeField] private float fireRateDefault = 3; 
    [SerializeField] private float fireTimer = 0; // 弾を発射するタイマー
    [SerializeField] private bool canFire = true;
    [SerializeField] private float attackTime = 0.5f;
    [SerializeField] private int level = 1; // 敵のレベル

    // LineRendererの設定
    private float lineWidth_aim = 0.02f;         // 予測線の幅
    private Color startColor_aim = Color.yellow; // 予測線の開始色
    private Color endColor_aim = Color.yellow;   // 予測線の終了色
    private float lineWidth = 0.05f;      // 攻撃線の幅 
    private Color startColor = Color.red; // 攻撃線の開始色
    private Color endColor   = Color.red; // 攻撃線の終了色

    void Start()
    {
        damageUI = GetComponentInChildren<EnemyDamageUI>();
    }

    void Update()
    {
        switch (gameManager.mode)
        {
            case GameManager.Mode.PLAY:
                fireTimer += Time.deltaTime;

                if (fireTimer >= fireRate) canFire = true;

                if (animator.speed == 0)
                {
                    // アニメーションと移動を再開
                    animator.speed = 1;
                    navMeshAgent.speed = SPD;
                }
                break;

            case GameManager.Mode.PAUSE:
                if (animator != null && animator == true)
                {
                    // アニメーションと移動を一時停止
                    animator.speed = 0;
                    navMeshAgent.speed = 0;
                }
                break;

            default:
                if (animator != null) animator.speed = 0; // その他のモードではアニメーションを停止
                break;
        }
    }

    // ステータス完全初期化
    private void SetStatus()
    {
        HP = HPDefault;
        SPD = SPDDefault;
        ATK = ATKDefault;
        fireRate = fireRateDefault; // 弾を発射する間隔
        fireTimer = 0f; // 弾を発射するタイマーを初期化
    }

    /// <summary>
    /// ステータスリセット(ウェーブ数に応じてステータスを増加)
    /// </summary>
    public void ResetStatus()
    {
        if (level >= 1)
        {
            HP += (level * 50);
            HPMax = HP;
            SPD += 0.05f;
            ATK += level * 5;
            fireRate = 3f - 0.01f;
            fireRate = Mathf.Max(fireRate, 0.1f, 3f);
            attackTime = 0.5f;
        }
        else
        {
            HP = HPDefault;
            HPMax = HP;
            SPD = SPDDefault;
            ATK = ATKDefault;
            fireRate = fireRateDefault;
        }
        enemyHPBar.value = HP / (float)HPMax;
    }

    // プレイヤーが前方の判定に接触したときに弾を発射
    private void OnTriggerStay(Collider other)
    {
        if (gameManager.mode != GameManager.Mode.PLAY) return;
        if (other.CompareTag("Player") && canFire)
        {
            if (!animator.enabled) animator.enabled = true; // Animatorが無効な場合は有効化                  
            animator.SetBool("isAttack", true); // 攻撃アニメーションを再生

            StartCoroutine(ShootPause()); // NavMeshAgentを停止して1秒待機
            Invoke("StopAttackAnimation", attackTime); // 1秒後に攻撃アニメーションを終了

            canFire = false;
            fireTimer = 0f;

            Vector3 lineStartPos = Muzzle.transform.position;
            Vector3 lineDir = other.transform.position - lineStartPos;
            float lineLength = 50f;
            StartCoroutine(Aiming(lineStartPos, lineStartPos + lineDir * lineLength));
        }
    }

    private IEnumerator Aiming(Vector3 lineStart, Vector3 lineEnd)
    {
        bulletLine.enabled = true;
        bulletLine.SetPosition(0, lineStart);
        bulletLine.SetPosition(1, lineEnd);
        bulletLine.startColor = startColor_aim;
        bulletLine.endColor = endColor_aim;
        bulletLine.startWidth = lineWidth_aim;
        bulletLine.endWidth = lineWidth_aim;
        // 予測線の点滅
        for (int i = 0; i < 6; i++)
        {
            bulletLine.enabled = !bulletLine.enabled;
            yield return new WaitForSeconds(attackTime / 6); 
        }
        bulletLine.enabled = true;
        bulletLine.startColor = startColor;
        bulletLine.endColor = endColor;
        bulletLine.startWidth = lineWidth;
        bulletLine.endWidth = lineWidth;
        AttackProcess(lineEnd - lineStart);
        yield return new WaitForSeconds(0.2f);
        bulletLine.enabled = false;
        yield return null;
    }

    private void AttackProcess(Vector3 rayDir)
    {
        Physics.Raycast(Muzzle.transform.position, rayDir, out RaycastHit hit, 100f);
        if(hit.collider != null && hit.collider.CompareTag("Player"))
        {
            PlayerManager player = hit.collider.GetComponent<PlayerManager>();
            if (!player) return;
            player.OnDamage(ATK);
        }
    }

    // 攻撃アニメーションを停止
    private void StopAttackAnimation()
    {
        animator.SetBool("isAttack", false);
    }

    // NavMeshAgentを停止して1秒待機するコルーチン
    private IEnumerator ShootPause()
    {
        navMeshAgent.isStopped = true;
        yield return new WaitForSeconds(attackTime); // 待機
        navMeshAgent.isStopped = false;
    }

    /// <summary>
    /// 被弾したとき
    /// </summary>
    public void OnDamage(int damage, bool isCritical)
    {
        if (gameManager.mode != GameManager.Mode.PLAY) return;
        HP -= damage; // 敵のHPを減少(ダメージ計算はPlayerで行う)
        enemyHPBar.value = HP / (float)HPMax;

        damageUI.ShowDamage(damage, isCritical, transform.position); // ダメージUIを表示
        if (HP <= 0)
        {
            gameManager.AddScore(true);
            OnDestroyEnemy();
        }
    }

    //　死亡処理
    private void OnDestroyEnemy()
    {
        // エフェクトを生成
        GameObject killEffect = Instantiate(killEffectPrefab, transform.position, Quaternion.identity);
        Destroy(killEffect, 1f);
        StartCoroutine(ReturnToPool());
        itemDroper.DropItem(transform.position);
        damageUI.Invoke("ResetDamage", 1f);
    }

    /// <summary>
    /// spawnerから呼び出すための初期化メソッド(依存性注入)
    /// </summary>
    public void Init(
        PlayerManager player,
        GameManager gm,
        EnemySpawner spawner,
        EnemyDamageUI damageUI,
        EnemyPool enemyPool,
        EnemyBulletPool bulletPool)
    {
        this.playerManager = player;
        this.gameManager = gm;
        this.enemySpawner = spawner;
        //this.damageUI = damageUI;
        this.enemyPool = enemyPool;

        target = player.transform;

        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        level = gameManager.waveCount;
        SetStatus();
        ResetStatus(); // ステータスをリセット
        navMeshAgent.speed = SPD;
        navMeshAgent.updateRotation = true;
        navMeshAgent.updateUpAxis = true;

        // ゲームオブジェクトが有効になったときにパスを更新するコルーチンを開始
        // 一連の処理は、whileによって自動的に呼び出される
        StartCoroutine(UpdatePath());
    }

    // enemyをプールに戻す
    private IEnumerator ReturnToPool()
    {
        yield return new WaitForSeconds(0.1f); // 死亡エフェクトが見えるように少し待機
        enemyPool.ReturnEnemy(gameObject);
        gameObject.SetActive(false);
    }

    // 0.3秒ごとにターゲットの位置を更新するコルーチン(+HPBarが常にカメラを向く)
    private IEnumerator UpdatePath()
    {
        while (true)
        {
            if ((gameManager.mode == GameManager.Mode.PLAY && target != null))
            {
                navMeshAgent.SetDestination(target.position);
            }
            else if (gameManager.mode != GameManager.Mode.PLAY)
            {
                navMeshAgent.ResetPath();
            }

            if(enemyHPBar != null)
            {
                enemyHPBar.transform.LookAt(Camera.main.transform);
                enemyHPBar.transform.Rotate(0, 180, 0); // カメラの向きに合わせて180度回転
            }

            yield return new WaitForSeconds(0.3f);
        }
    }
}