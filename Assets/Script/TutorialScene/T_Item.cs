using UnityEngine;

public class T_Item : MonoBehaviour
{
    public enum ItemType
    {
        Health,
        Ammo,
    }
    public ItemType itemType;

    [Header("スクリプト参照")]
    public AudioManager audioManager; // Spawnerから注入される

    // アイテムの効果を適用するメソッド
    public void ApplyEffect(PlayerManager player)
    {
        if (player == null) { Debug.LogWarning("item.playerがnull"); return; }

        switch (itemType)
        {
            case ItemType.Health: // 10%回復
                int addHP = (int)(player.MaxHP * 0.1f);
                player.HP += addHP;
                player.HPBar.value = (float)player.HP / player.MaxHP;
                audioManager.PlayItemHealthSound();
                break;

            case ItemType.Ammo: // 弾薬を30%補充 or リロード
                int addAmmo = (int)(player.shoot.magazineSize * 0.3f);
                player.shoot.currentAmmo += addAmmo;
                // リロード中なら満タンに
                if (player.shoot.isReloading)
                {
                    player.shoot.currentAmmo = player.shoot.magazineSize;
                    player.shoot.isReloading = false; // リロード完了
                }
                player.shoot.currentAmmo = Mathf.Clamp(player.shoot.currentAmmo, 0, player.shoot.magazineSize);
                audioManager.PlayItemAmmoSound();
                break;
        }
    }

    // Playerに触れたときに効果を適用してアイテムを消す
    private void OnTriggerEnter(Collider other)
    {
        // 対象がplayerタグを持ち、PlayerManagerコンポーネントが存在する場合にアイテムの効果を適用
        if (other.CompareTag("Player"))
        {
            var pm = other.GetComponent<PlayerManager>();
            if (pm != null)
            {
                ApplyEffect(pm);
                Destroy(gameObject);
            }
        }
    }
}
