using UnityEngine;

// いらんかも
public class BulletManager : MonoBehaviour
{
    [Header("オブジェクト参照")]
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private GameObject MainCamera; // メインカメラのGameObject

    [Header("スクリプト参照")]
    [SerializeField] private GameManager gameManager;

    [Header("パラメータ")]
    [SerializeField] private float lifeTime = 10f;

    private Vector3 shootDirection; // 弾の発射方向

    void Start()
    {
        //Player = GameObject.FindWithTag("Player"); // タグ"Player"を持つオブジェクトを取得

        MainCamera = GameObject.FindWithTag("MainCamera"); // タグ"MainCamera"を持つオブジェクトを取得
        if (MainCamera != null)
        {
            // 弾の発射方向をプレイヤーの向き（forward）に設定
            shootDirection = MainCamera.transform.forward;
            transform.rotation = Quaternion.LookRotation(shootDirection); // 弾の向きを揃える
        }
        else
        {
            shootDirection = transform.forward; // 念のための初期化
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if(gameManager.mode != GameManager.Mode.PLAY) return;
        transform.position += shootDirection * bulletSpeed * Time.deltaTime; // 弾を発射方向に移動
    }

    // オブジェクトに衝突したら弾を削除
    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
