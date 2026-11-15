using UnityEngine;

// 敵のアイテムドロップを管理するクラス
// 25/10/05 リファクタリング
public class EnemyItemDroper : MonoBehaviour
{
    [Header("アイテムプレハブ")]
    [SerializeField] private GameObject Item_heal;
    [SerializeField] private GameObject Item_ammo;

    [Header("変数")]
    [SerializeField] private int dropRate = 20; // パーセンテージ
    [SerializeField] private bool isDrop = false;
    private GameObject itm; // ドロップするアイテム

    private void OnEnable()
    {
        // ドロップするかどうかをランダムに決定
        if (Random.Range(0, 100) < dropRate)
        {
            isDrop = true;
            itm = Random.Range(0, 2) == 0 ? Item_heal : Item_ammo; // ランダムにアイテムを選択
        }
        else
        {
            isDrop = false;
        }
    }

    /// <summary>
    /// アイテムをドロップするメソッド
    /// </summary>
    public void DropItem(Vector3 position)
    {
        if (isDrop && itm != null)
        {
            Instantiate(itm, position, Quaternion.identity);

            isDrop = false; // ドロップフラグをリセット
        }
    }
}
