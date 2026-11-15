using TMPro;
using UnityEngine;

// 敵にダメージを与えた際に、ダメージ量を表示するクラス
// 各エネミーが自身のEnemyDamageUIを持つ
// 25/10/05 リファクタリング
public class EnemyDamageUI : MonoBehaviour
{
    [Header("ダメージ表示設定")]
    [SerializeField] private TextMeshProUGUI enemyDamageUI; //prefab
    private TextMeshProUGUI dmg;
    [SerializeField] private GameObject canvas; 
    [SerializeField] private new Camera camera; 
    [SerializeField] private float displayDuration = 1f;

    // ダメージ表示のパラメータ
    private Color normalColor = Color.yellow; 
    private float normalFontSize = 16; 
    private Color criticalColor = Color.red; 
    private float criticalFontSize = 32;
    private Vector3 offset = new Vector3(0, 2f, 0);

    private int totalDamage = 0; // 累積ダメージ
    private float displayTimer = 0f; // ダメージ表示のタイマー

    void Start()
    {
        canvas = GameObject.Find("Canvas_world");
        camera = Camera.main;
        dmg = Instantiate(enemyDamageUI, canvas.transform); // canvasの子に
        dmg.gameObject.SetActive(false); // 初期は非表示
    }

    /// <summary>
    /// ダメージを表示するメソッド
    /// </summary>
    public void ShowDamage(int amount, bool isCrithical, Vector3 position)
    {
        if (dmg == null)
        {
            dmg = Instantiate(enemyDamageUI, canvas.transform);
        }
        dmg.gameObject.SetActive(true);

        totalDamage += amount; // 累積ダメージを加算
        dmg.text = totalDamage.ToString(); // 累積表示（単発ならamount.ToString()）

        displayTimer = 0f;

        dmg.color = isCrithical ? criticalColor : normalColor;
        dmg.fontSize = isCrithical ? criticalFontSize : normalFontSize;

        Vector3 randPos = Vector3.zero;
        // 1hit目だけランダム位置に表示
        if (totalDamage == 0)
        {
            // UI座標をワールド座標で設定(+乱数位置)  
            randPos = new Vector3(
                UnityEngine.Random.Range(-0.3f, 0.3f),     // X
                UnityEngine.Random.Range(-0.3f, 0.3f), 0); // Y,Z

        }
        dmg.transform.position = position + offset + randPos;
        dmg.transform.LookAt(camera.transform);
        dmg.transform.Rotate(0, 180, 0); // カメラの向きに合わせて180度回転
    }

    /// <summary>
    /// ダミー用のダメージ表示メソッド
    /// </summary>
    public void ShowDamage(Vector3 position)
    {
        //TextMeshProUGUI dmg = Instantiate(enemyDamageUI, transform);

        if (dmg == null)
        {
            dmg = Instantiate(enemyDamageUI, canvas.transform);
        }
        dmg.gameObject.SetActive(true);

        dmg.text = "hit!";
        displayTimer = 0f;

        Vector3 randPos = Vector3.zero;
        // 1hit目だけランダム位置に表示
        if (totalDamage == 0)
        {
            // UI座標をワールド座標で設定  
            randPos = new Vector3(
                UnityEngine.Random.Range(-0.3f, 0.3f),     // X
                UnityEngine.Random.Range(-0.3f, 0.3f), 0); // Y,Z

        }
        dmg.color = normalColor;
        dmg.fontSize = criticalFontSize;
        dmg.transform.position = position + (offset * 2) + randPos; // ダミーと被らないようにOffsetを2倍
        dmg.transform.LookAt(camera.transform); // カメラの方向を向くように設定  
        dmg.transform.Rotate(0, 180, 0); // カメラの向きに合わせて180度回転
    }

    private void Update()
    {
        if (!dmg.gameObject.activeSelf) return;

        // ダメージ表示のタイマー更新
        displayTimer += Time.deltaTime;
        if (displayTimer >= displayDuration)
        {
            ResetDamage();
        }

        // コルーチン化しても良い
        if (dmg != null)
        {
            // UIがカメラの方向を向くように更新
            dmg.transform.LookAt(camera.transform);
            dmg.transform.Rotate(0, 180, 0); // カメラの向きに合わせて180度回転
        }
    }

    // ダメージを非表示にしてリセット
    private void ResetDamage()
    {
        dmg.gameObject.SetActive(false);
        totalDamage = 0; // 累積ダメージをリセット
        displayTimer = 0f; // タイマーをリセット
    }
}
