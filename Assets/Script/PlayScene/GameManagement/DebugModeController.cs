using UnityEngine;
using TMPro;

// デバッグモードを管理するクラス
// 25/10/06 リファクタリング
public class DebugModeController : MonoBehaviour
{
    [Header("スクリプト参照")]
    [SerializeField] private PlayerManager player;
    [SerializeField] private GameManager manager;

    [Header("コンポーネント参照")]
    [SerializeField] private TextMeshProUGUI debugText;

    void Update()
    {
        if(manager.mode != GameManager.Mode.PLAY) return;

        // 1, 無敵モード
        // 2, 武器性能最大化
        // 3, 移動性能・HP最大化
        // 4, レベル10アップ
        // 5, 敵全滅
        // 6, フィールドの敵のHP +100
        // 7, プレイヤーステータス初期化
        // 8, 制限時間のカウントダウンON/OFF

        // RShift + 1で無敵モードの切り替え
        if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            player.noDamage = !player.noDamage;

            manager.isUseDebugmode = true; // スコア無効化
            if (player.noDamage)
            {
                showDebugText("DebugMode : 無敵モードON");
            }
            else
            {
                showDebugText("DebugMode : 無敵モードOFF");
            }
        }
        // RShift + 2で武器性能を最大に
        if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            player.shoot.ATK = 9999;
            player.shoot.fireRate = 0.001f;
            player.shoot.reloadTime = 0.001f;
            player.shoot.magazineSize = 999;
            player.shoot.currentAmmo = player.shoot.magazineSize;
            player.shoot.recoilAngle = 0;
            player.shoot.burstInterval = 0.2f;
            player.shoot.chargeSpeed = 3f;
            player.shoot.hitLimit = 10;
            player.shoot.range = 300;

            manager.isUseDebugmode = true; // スコア無効化
            showDebugText("DebugMode : 武器性能最大化");
        }
        // RShift + 3で移動性能 + MAXHPを最大に
        if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha3))
        {
            player.move.moveSpeed = 50f;
            player.move.NotDashSpeed = player.move.moveSpeed;
            player.move.dashSpeed = 60f;
            player.move.jumpForce = 100f;
            player.MaxHP = 9999;
            player.HP = player.MaxHP;
            player.HPBar.value = (float)player.HP / player.MaxHP;
            player.move.StaminaMax = 9999;
            player.move.Stamina = player.move.StaminaMax;
            player.move.StaminaBar.value = (float)player.move.Stamina / player.move.StaminaMax;

            manager.isUseDebugmode = true; // スコア無効化
            showDebugText("DebugMode : 移動性能・HP最大化");
        }

        // RShift + 4でlvを10上げる
        if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha4))
        {
            for (int i = 0; i < 10; i++)
            {
                player.LevelUp();
            }

            manager.isUseDebugmode = true; // スコア無効化
            showDebugText("DebugMode : レベル10アップ");
        }
        // RShift + 5で敵を全滅させる
        if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha5))
        {
            manager.ReturnPoolAllEnemies();

            manager.isUseDebugmode = true; // スコア無効化
            showDebugText("DebugMode : 敵全滅");
        }
        // RShift + 6でフィールドに存在する敵のHPを+100
        if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha6))
        {
            SetAllEnemiesHP();

            manager.isUseDebugmode = true; // スコア無効化
            showDebugText("DebugMode : フィールドの敵のHP +100");
        }
        // RShift + 7でplayerのステータスを初期化
        if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha7))
        {
            player.InitAllStatus();

            manager.isUseDebugmode = true; // スコア無効化
            showDebugText("DebugMode : プレイヤーステータス初期化");
        }
        // RShift + 8で制限時間のカウントダウンを停止
        if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha8))
        {
            manager.isCountDown = !manager.isCountDown;
            if (manager.isCountDown)
            {
                showDebugText("DebugMode : 制限時間のカウントダウンON");
            }
            else
            {
                showDebugText("DebugMode : 制限時間のカウントダウンOFF");
            }

            manager.isUseDebugmode = true; // スコア無効化
        }
    }

    // フィールドの敵のHP +100する
    private void SetAllEnemiesHP()
    {
        var enemies = FindObjectsByType<EnemyManager>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.HP += 100;
            enemy.HPMax = 100;
        }
    }

    // デバッグテキストの表示
    private void showDebugText(string text)
    {
        debugText.text = text;
        Invoke("ClearDebugText", 2.0f); // 2秒後にテキストを消す
    }

    // デバッグテキストを消す
    private void ClearDebugText()
    {
        debugText.text = "";
    }

}
