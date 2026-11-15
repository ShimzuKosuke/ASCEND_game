using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReticleColor : MonoBehaviour
{
    [Header("コンポーネント参照")]
    [SerializeField] private Image ReticleHorizontal;
    [SerializeField] private Image ReticleVertical;
    [SerializeField] private GameObject Muzzle;

    [Header("スクリプト参照")]
    [SerializeField] private PlayerPOV POV;
    [SerializeField] private PlayerShooting shoot;

    private Color originalColor = new(0.7f, 0.7f, 0.7f, 1f);
    private Color targetColor = new(1f, 0f, 0f, 1f);

    private void Start()
    {
        if (ReticleHorizontal == null)
        {
            ReticleHorizontal = GetComponent<Image>();
        }
        if (ReticleVertical == null)
        {
            ReticleVertical = GetComponent<Image>();
        }

        originalColor = ReticleHorizontal.color;
        ReticleHorizontal.color = originalColor;

        StartCoroutine(UpdateReticleColor());
    }

    // Rayを飛ばして経路上にEnemyタグがあれば赤色にする
    private IEnumerator UpdateReticleColor()
    {
        Vector3 origin = Muzzle.transform.position;
        Vector3 direction = POV.neck.forward;
        float upDateInterval = 0.1f;

        if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, shoot.range))
        {
            if (hitInfo.collider.CompareTag("Enemy") || hitInfo.collider.CompareTag("Dummy"))
            {
                ReticleHorizontal.color = targetColor;
                ReticleVertical.color = targetColor;
            }
            else
            {
                ReticleHorizontal.color = originalColor;
                ReticleVertical.color = originalColor;
            }
        }
        else
        {
            ReticleHorizontal.color = originalColor;
            ReticleVertical.color = originalColor;
        }
        yield return new WaitForSeconds(upDateInterval);
        StartCoroutine(UpdateReticleColor());
    }
}
