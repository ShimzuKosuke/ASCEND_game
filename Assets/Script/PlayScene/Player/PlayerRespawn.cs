using UnityEngine;
using System.Collections.Generic;

// プレイヤーのリスポーンポイントを管理するクラス
// 25/10/08 リファクタリング
public class PlayerRespawn : MonoBehaviour
{
    [Header("プレイヤーのリスポーンポイント")]
    public List<GameObject> respawnPointsList; // リスポーンポイントのリスト  

    private void Start()
    {
        // リスポーンポイントのリストが空でないことを確認
        if (respawnPointsList == null || respawnPointsList.Count == 0)
        {
            Debug.LogError("リスポーンポイントのリストが設定されていません。");
        }
    }

    // リスポーンポイントをランダムに選択して返すメソッド  
    public Vector3 Respawn()
    {
        //リスポーンポイント[0 ~ 要素数-1].座標
        Vector3 point = 
            respawnPointsList[Random.Range(0, respawnPointsList.Count)].transform.position;

        return point;
    }
}
