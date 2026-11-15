using UnityEngine;
using System.Collections;

// ステージの壁を透明化するクラス
public class StageWall : MonoBehaviour
{
    [SerializeField] private Material selfMaterial;
    [SerializeField] private Transform player;

    private void Start()
    {
        selfMaterial = GetComponent<MeshRenderer>().material;
        StartCoroutine(setWallTransparent());
    }

    // 距離に応じて壁を透明化するコルーチン
    private IEnumerator setWallTransparent()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        // プレイヤーが近づくほど不透明にする
        // 10(distance) / 10 - 1 = 0 (透明), 2(distance) / 10 - 1 = 0.8 (不透明)
        float alpha = Mathf.Clamp01(1.0f - (distance / 10.0f)); // 距離が10以上で透明、0で不透明
        selfMaterial.color = new Color(
            selfMaterial.color.r,
            selfMaterial.color.g,
            selfMaterial.color.b,
            alpha
        );

        yield return new WaitForSeconds(0.1f);
        StartCoroutine(setWallTransparent());
    }
}
