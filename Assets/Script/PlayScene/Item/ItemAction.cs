using UnityEngine;

// アイテムの単振動を管理するクラス
// 25/10/07 リファクタリング(確認)
public class ItemAction : MonoBehaviour
{
    private Vector3 initPos;
    void OnEnable()
    {
        initPos = transform.position; //初期位置を記憶
    }

    void Update()
    {
        //初期位置を中心に上下の単振動を行う
        transform.position = initPos +
        new Vector3(0, Mathf.Sin(Time.time * Mathf.PI) * 0.3f, 0);
        // 上下の振幅を変更したい場合は、Mathf.Sinの結果に係数を掛ける
    }
}
