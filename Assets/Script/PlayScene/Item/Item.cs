using UnityEngine;

// アイテムの効果を管理するクラス
// 25/10/07 リファクタリング
public class Item : MonoBehaviour
{
    [Header("スクリプト参照")]
    private PlayerManager player;
    [SerializeField] private GameManager manager;

    [Header("アイテム設定")]
    [SerializeField] private float healValue = 0.1f; // 回復量(最大HPの割合)
    [SerializeField] private float ammoValue = 0.3f; // 補充量(マガジンサイズの割合)

    // 取得されたとき、アイテムの種類を識別するための列挙型
    private enum ItemType
    {
        Health,
        Ammo,
    }
    [SerializeField] private ItemType itemType;

    void Start()
    {
        if (manager == null) // nullチェック
        {
            manager = GameObject.Find("GameManager").GetComponent<GameManager>();
            if (manager == null)
            {
                Debug.LogError("Item: GameManagerが見つかりません。");
            }else Debug.Log("Item: GameManagerを取得しました。");
        }
        // wave更新時の削除アナウンスメントを受け取るためにGameManagerの子オブジェクトに設定
        transform.SetParent(manager.transform); 
    }

    // アイテムの効果を適用するメソッド
    private void ApplyEffect()
    {
        if (player == null) { Debug.LogWarning("item.playerがnull"); return; }

        switch (itemType)
        {
            case ItemType.Health: // 10%回復
                int addHP = (int)(player.MaxHP * healValue);
                player.HP += addHP;
                player.HP = Mathf.Clamp(player.HP, 0, player.MaxHP);
                player.HPBar.value = (float)player.HP / player.MaxHP;
                player.damageEffects.OnHeal(player.HP, player.MaxHP);
                manager.audioManager.PlayItemHealthSound();
                break;

            case ItemType.Ammo: // 弾薬を30%補充
                int addAmmo = (int)(player.shoot.magazineSize * ammoValue);
                player.shoot.currentAmmo += addAmmo;
                // リロード中なら満タンに
                if (player.shoot.isReloading)
                {
                    player.shoot.currentAmmo = player.shoot.magazineSize;
                    player.shoot.isReloading = false; // リロード完了
                }
                player.shoot.currentAmmo = Mathf.Clamp(player.shoot.currentAmmo, 0, player.shoot.magazineSize);
                manager.audioManager.PlayItemAmmoSound();
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 対象がplayerタグを持ち、PlayerManagerコンポーネントが存在する場合にアイテムの効果を適用
        if (other.CompareTag("Player"))
        {
            // 軽量化ポイント
            var pm = other.GetComponent<PlayerManager>();
            if (pm != null)
            {
                player = pm;
                ApplyEffect();
                Destroy(gameObject);
            }
        }
    }    
}
