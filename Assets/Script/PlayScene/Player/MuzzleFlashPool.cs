using UnityEngine;
using System.Collections.Generic;

// マズルフラッシュのオブジェクトプールを管理するクラス
// 25/10/08 リファクタリング
public class MuzzleFlashPool : MonoBehaviour
{
    [Header("オブジェクト参照")]
    [SerializeField] private GameObject muzzleFlashPrefab;

    [Header("プール設定")]
    [SerializeField] private int poolSize = 10;

    private Queue<GameObject> muzzleFlashPool = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewFlash();
        }
    }

    // プール内のマズルフラッシュを生成
    private void CreateNewFlash()
    {
        GameObject newFlash = Instantiate(muzzleFlashPrefab);
        newFlash.SetActive(false);
        muzzleFlashPool.Enqueue(newFlash); // Enqueue : キューに要素を追加
        //return newFlash;
    }

    /// <summary>
    /// プール内のマズルフラッシュを取得
    /// </summary>
    public GameObject GetMuzzleFlash()
    {
        if (muzzleFlashPool.Count <= 0)
        {
            CreateNewFlash();
        }

        // Dequeue : キューから要素を取り出す
        GameObject flash = muzzleFlashPool.Dequeue();
        flash.SetActive(true);
        return flash;
    }

    /// <summary>
    /// マズルフラッシュをプールに戻す
    /// </summary>
    public void ReturnMuzzleFlash(GameObject flash)
    {
        flash.SetActive(false);
        muzzleFlashPool.Enqueue(flash);
    }
}
