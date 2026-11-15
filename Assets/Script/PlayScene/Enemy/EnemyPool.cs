using UnityEngine;
using System.Collections.Generic;

// 敵のObjectPool
// 25/10/06 リファクタリング
public class EnemyPool : MonoBehaviour
{
    [Header("プールパラメータ")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int poolSize = 30;

    private Queue<GameObject> enemyPool = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewEnemy();
        }
    }

    // プール内の敵を生成
    private GameObject CreateNewEnemy()
    {
        GameObject newEnemy = Instantiate(enemyPrefab);
        newEnemy.SetActive(false);
        enemyPool.Enqueue(newEnemy); // Enqueue : キューに要素を追加
        return newEnemy;
    }

    // プール内の敵を取得
    public GameObject GetEnemy()
    {
        if (enemyPool.Count <= 0)
        {
            CreateNewEnemy();
        }
        // Dequeue : キューから要素を取り出す
        GameObject enemy = enemyPool.Dequeue();
        enemy.SetActive(true);
        return enemy;
    }

    // 敵をプールに戻す
    public void ReturnEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        enemyPool.Enqueue(enemy);
    }
}
