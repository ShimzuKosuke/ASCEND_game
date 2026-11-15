using UnityEngine;
using System.Collections.Generic;

// 敵の弾のプールを管理するクラス
// 25/10/05 リファクタリング
public class EnemyBulletPool : MonoBehaviour
{
    [Header("プール設定")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private int poolSize = 30;
    [SerializeField] private GameObject Pool; // 弾をまとめる親オブジェクト

    private Queue<GameObject> enemyBulletPool = new Queue<GameObject>();

    [Header("スクリプト参照")]
    public GameManager gameManager; // 依存性の注入に使用

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewBullet();
        }
    }

    // プール内の敵弾を生成
    private GameObject CreateNewBullet()
    {
        GameObject newBullet = Instantiate(enemyBulletPrefab);
        newBullet.SetActive(false);
        enemyBulletPool.Enqueue(newBullet); // Enqueue : キューに要素を追加

        // 親オブジェクトを設定
        if (Pool != null)
        {
            newBullet.gameObject.transform.SetParent(Pool.transform);
        }
        return newBullet;
    }

    /// <summary>
    /// プール内の敵弾を取得
    /// </summary>
    /// <returns></returns>
    public GameObject GetEnemyBullet()
    {
        if (enemyBulletPool.Count <= 0)
        {
            CreateNewBullet();
        }
        // Dequeue : キューから要素を取り出す
        GameObject bullet = enemyBulletPool.Dequeue();
        bullet.SetActive(true);
        return bullet;
    }

    /// <summary>
    /// 敵弾をプールに戻す
    /// </summary>
    /// <param name="bullet"></param>
    public void ReturnEnemyBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        enemyBulletPool.Enqueue(bullet);
    }
}
