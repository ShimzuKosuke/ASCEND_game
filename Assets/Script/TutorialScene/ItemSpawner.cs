using UnityEngine;
using System.Collections;

public class ItemSpawner : MonoBehaviour
{
    [Header("コンポーネント参照")]
    public GameObject itemPrefab;

    [Header("スクリプト参照")]
    public AudioManager audioManager;

    [Header("変数")]
    [SerializeField] private float spawnInterval = 5f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= spawnInterval)
        {
            SpawnItem();
            timer = 0f;
        }
    }

    // アイテムをスポーンさせるメソッド
    void SpawnItem()
    {
        // すでに子オブジェクトがある場合はスポーンしない
        if (GetComponentInChildren<Transform>().childCount > 0) {
            return;
        }

        GameObject item =Instantiate(itemPrefab, 
            transform.position, 
            Quaternion.identity);
        // itemを自身の子オブジェクトに設定
        item.transform.parent = this.transform;

        // 依存関係の注入
        item.GetComponent<T_Item>().audioManager = audioManager;

    }
}
