using UnityEngine;
using UnityEngine.AI;

// 敵のスポーンを管理するクラス
// 25/10/06 リファクタリング
public class EnemySpawner : MonoBehaviour
{
    [Header("スクリプト参照")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private EnemyDamageUI damageUI;
    [SerializeField] private EnemyBulletPool bulletPool;

    [Header("コンポーネント参照")]
    [SerializeField] private SphereCollider spawnArea; // スポーンエリアのコライダー

    [Header("スポーン設定")]
    [SerializeField] public float spawnInterval = 10.0f;
    [SerializeField] private float spawnIntervalCurrent = 10.0f; // 現在のスポーン間隔カウンター
    [SerializeField] public bool canSpawning { get; set; } = false; // スポーン可能かどうかのフラグ

    private GameObject enemyObj;
    [SerializeField] private Vector3 spawnPosition; // スポーン位置を保持するフィールド

    void Start()
    {
        spawnPosition = transform.position; // スポーン位置を初期化
        spawnIntervalCurrent = Random.Range(1,5); // スポーンまでの時間をランダムに設定
        ResetSpawner(); // スポーン設定をリセット
    }

    void Update()
    {
        switch (gameManager.mode)
        {
            case GameManager.Mode.PLAY:
                spawnIntervalCurrent -= Time.deltaTime;
                if (spawnIntervalCurrent <= 0 && canSpawning)
                {
                    SpawnEnemy();
                    spawnIntervalCurrent = spawnInterval; // リセット
                }
                break;
        }
    }

    // スポーン処理
    private void SpawnEnemy()
    {
        enemyObj = enemyPool.GetEnemy(); // プールから敵を取得
        enemyObj.transform.position = spawnPosition; // スポーン位置を設定
        enemyObj.GetComponent<NavMeshAgent>().Warp(spawnPosition); // NavMeshAgentを使用してスポーン位置にワープ

        EnemyManager enemy = enemyObj.GetComponent<EnemyManager>();

        // 敵スポーン時の参照探索を避けるために、必要なスクリプトをスポナーから直接渡す
        enemy.Init(
                playerManager,
                gameManager,
                this,
                damageUI,
                enemyPool,
                bulletPool
            );
        enemyObj.SetActive(true);
    }

    /// <summary>
    /// wave更新時に呼び出されるリセット関数
    /// </summary>
    public void ResetSpawner()
    {
        canSpawning = false; // 一時的に無効化
        spawnInterval -= 0.05f; // ウェーブ数に応じてスポーン間隔を短縮
        if (spawnInterval < 1.0f) spawnInterval = 1.0f; // 最小スポーン間隔を1秒に制限
        spawnIntervalCurrent = Random.Range(1, 5);

        // waveを更新するたびにスポーンエリアを拡大
        spawnArea.radius++;
        spawnArea.radius = Mathf.Clamp(spawnArea.radius, 30.0f, 100.0f);
    }

    /// <summary>
    /// スポナーの完全初期化
    /// </summary>
    public void InitSpawner()
    {
        canSpawning = false; // スポーン不可にする
        spawnInterval = 10.0f; // スポーン間隔を初期化
        spawnIntervalCurrent = spawnInterval; // 現在のスポーン間隔カウンターを初期化
    }

    //判定内にプレイヤーが入っていて、距離が一定以上離れている場合にスポーン可能にする
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // プレイヤーとの距離が一定以上離れている場合にスポーン可能にする
            // Distance : 2つの位置ベクトル間の距離(三次元空間の距離)を返す
            float distance = Vector3.Distance(other.transform.position, transform.position);
            if (distance >= 10)
            {
                canSpawning = true;
            }
            else
            {
                canSpawning = false;
            }
        }
    }

    // 判定外にプレイヤーが出たらスポーン不可にする
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canSpawning = false; // プレイヤーがスポーンエリアから出たらスポーン不可にする
        }
    }
}

