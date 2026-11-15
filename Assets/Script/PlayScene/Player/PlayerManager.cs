using UnityEngine;
using UnityEngine.UI; // UI関連のクラスを使用するために必要

// 他スクリプトの関数を実行・Playerのステータスを管理するクラス
// 25/10/09 リファクタリング
public class PlayerManager : MonoBehaviour
{
    [Header("スクリプト参照(共通)")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private CameraShake shake;
    [SerializeField] private PlayerPOV POV;

    [Header("スクリプト参照(Play)")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameUI gameUI;
    public PlayerShooting shoot;
    public PlayerMoving move;
    public DamageEffects damageEffects;

    // Sceneごとに同じクラスを使用するが、変数の参照先が異なるため分ける
    [Header("スクリプト参照(Tutorial)")]
    [SerializeField] private TutorialManager tutorialManager;

    [Header("コンポーネント参照")]
    public Slider HPBar;
    public Slider ReloadBar;

    [Header("ステータス")]
    public int level { get; private set; } = 1;
    public float levelUpThreshold { get; private set; } = 50; // レベルアップの閾値
    private float levelUpThresholdDefault = 50;
    public int HP = 100;
    public int MaxHP = 100;
    [SerializeField] private int HPDefault = 100;
    [SerializeField] public int HPRegenRate = 0; // HPのリジェネレートレート
    private float HPRegenTimer = 0f; // HPリジェネのタイマー
    [SerializeField] public int DamageReduction = 0; // ダメージ軽減率
    [SerializeField] public float expMultiplier = 1.0f; // 経験値の倍率(manager.addscore内で使用)
    public bool noDamage = false; // 無敵状態かどうか
    [SerializeField] private float noDamageDuration = 3.0f; // wave更新後の無敵状態の持続時間
    [SerializeField] private float airborneDamageMultiplier = 1.3f; // 空中にいるときに受けるダメージが増加する倍率

    [Header("演算用変数")]
    [SerializeField] private Vector3 StanbyPos = new Vector3(0, 100, 0); // スタンバイ時の位置
    [SerializeField] public bool isTutorial; // チュートリアルモードかどうか

    private void Start()
    {
        int LayMask = LayerMask.GetMask("Enemy"); // レイヤーマスクを取得(Colliderごとにインスペクターから設定が必要)
    }

    private void OnEnable()
    {
        shoot.InitAllStatus();
        move.InitAllStatus();
        POV.Initialize();
    }

    // ゲーム中実行されるUpdate関数
    void Update()
    {
        if (!isTutorial)
        {
            switch (gameManager.mode)
            {
                case GameManager.Mode.STANDBY:
                    break;
                case GameManager.Mode.PLAY:
                    // 射撃・タイマー管理
                    shoot.ShootingControl();
                    shoot.TimerControl();

                    // アニメーション・移動
                    move.PlayAnimaion();
                    move.Move();

                    // 視点移動
                    POV.CameraMove();

                    // 毎フレームHP管理
                    HPManagement(); 

                    // レベルアップ処理
                    if (levelUpThreshold <= gameManager.score) LevelUp();
                    break;

                case GameManager.Mode.STOP:
                    break;
            }
        } // if (!isTutorial)*******************************************************************************************
        else
        {
            switch (tutorialManager.mode)
            {
                case TutorialManager.TutorialoMode.START:
                    break;

                case TutorialManager.TutorialoMode.MOVE:
                    // 射撃・タイマー管理
                    shoot.ShootingControl();
                    shoot.TimerControl();

                    // アニメーション・移動
                    move.PlayAnimaion();
                    move.Move();

                    // 視点移動
                    POV.CameraMove();
                    break;

                case TutorialManager.TutorialoMode.PAUSE:
                    break;
            }
        }
    }

    // プレイヤーが敵や敵の弾に当たったときの処理
    private void OnTriggerEnter(Collider other)
    {
        if (noDamage || gameManager.mode != GameManager.Mode.PLAY) return;

        if (other.gameObject.CompareTag("Enemy"))
        {
            OnDamage(other.gameObject.GetComponent<EnemyManager>().ATK);
        }
    }

    // ダメージ処理(計算・エフェクト・SE)
    public void OnDamage(int EnemyATK)
    {
        int damage = EnemyATK;
        damage -= DamageReduction;
        if(!move.isGrounded) damage = (int)(damage * airborneDamageMultiplier); // 空中にいるときは被ダメ増加
        damage = Mathf.Max(damage, 0);
        HP -= damage; // 敵の攻撃力でプレイヤーのHPを減少
        HP = Mathf.Clamp(HP, 0, MaxHP); // HPを0からMaxHPの範囲に制限
        HPBar.value = (float)HP / MaxHP;

        if (gameManager.isCamShake) shake.GiveDamageShake();
        damageEffects.OnHit(HP, MaxHP);
        audioManager.PlayDamageSound();
    }

    /// <summary>
    /// ステータス初期化(Wave更新時に呼び出し)
    /// </summary>
    public void InitStatus()
    {
        HP = MaxHP; // HP
        HPBar.value = (float)HP / MaxHP;
        noDamage = true;
        Invoke("DisableNoDamage", noDamageDuration); // 無敵状態を一定時間後に解除
        shoot.InitStatus();
        move.InitStatus();
        damageEffects.Initialize(); // ポストエフェクト効果を初期化
    }

    /// <summary>
    /// ステータス完全初期化
    /// </summary>
    public void InitAllStatus()
    {
        MaxHP = HPDefault;
        HP = MaxHP;
        InitStatus();
        level = 1;
        levelUpThreshold = levelUpThresholdDefault;
        HPRegenRate = 0;
        DamageReduction = 0;
        expMultiplier = 1.0f;

        shoot.InitAllStatus();
        move.InitAllStatus();
    }

    /// <summary>
    ///  レベルアップ
    /// </summary>
    public void LevelUp()
    {
        level++;
        levelUpThreshold *= 1.7f;
        MaxHP += 10;
        HP += MaxHP / 10;
        HP = Mathf.Clamp(HP, 0, MaxHP);
        shoot.ATK += 10;
        move.moveSpeed += 0.1f;
        move.NotDashSpeed = move.moveSpeed;
        gameUI.Txt_Level.color = Color.yellow;
        Invoke("SetLvTextColor", 1f);
    }

    // 毎フレームHP管理
    private void HPManagement()
    {
        // HPリジェネ
        HPRegenTimer += Time.deltaTime;
        if (HPRegenTimer >= 1f)
        {
            HPRegenTimer = 0f;
            if (HP < MaxHP)
            {
                HP += HPRegenRate;
                if (HP > MaxHP) HP = MaxHP;
                HPBar.value = (float)HP / MaxHP;
                HP = Mathf.Clamp(HP, 0, MaxHP);
                damageEffects.OnHeal(HP, MaxHP);
            }
        }
        // プレイヤーのHPが0になったらゲーム終了
        if (HP <= 0)
        {
            gameManager.mode = GameManager.Mode.GAMEOVER;
            gameUI.GameOverUI(0);
        }

        // 奈落に落ちている間HPが徐々に減少
        if (transform.position.y < -20f)
        {
            HP--;
            HPBar.value = (float)HP / MaxHP;
            HP = Mathf.Clamp(HP, 0, MaxHP);
            damageEffects.OnHit(HP, MaxHP);
        }

    }

    /// <summary>
    /// スポナーの判定(canSpawn)を完全に初期化するため、一時的に遠くに移動させる関数
    /// </summary>
    public void Standby()
    {
        this.transform.position = StanbyPos; // スタンバイ位置に移動
    }

    // 無敵状態を解除(Invoke用)
    public void DisableNoDamage()
    {
        noDamage = false;
    }

    // テキストカラーを変更(Invoke・他スクリプトからの呼び出し用)
    public void SetLvTextColor()
    {
        gameUI.Txt_Level.color = Color.white;
    }
}
