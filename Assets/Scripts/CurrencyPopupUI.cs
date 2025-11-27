using TMPro;
using UnityEngine;
using System.Collections;

public class CurrencyPopupUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;

    // Duration of the travel time
    [SerializeField] private float animationDuration = 0.8f;

    public void Initialize(int amount, RectTransform targetDestination)
    {
        if (textMesh != null)
        {
            textMesh.text = $"+{amount}";
            textMesh.alpha = 1f;
        }

        StartCoroutine(AnimateToTarget(targetDestination));
    }

    private IEnumerator AnimateToTarget(RectTransform target)
    {
        float timer = 0f;
        Vector3 startPos = transform.position;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animationDuration;

            // CHANGED: Direct Linear or Ease-Out movement (no Up/Down drift)
            // Using a simple Sin ease-out for smooth arrival
            float moveT = Mathf.Sin(t * Mathf.PI * 0.5f);

            if (target != null)
            {
                // Lerp directly from Fixed Start Position to the Coin Text
                transform.position = Vector3.Lerp(startPos, target.position, moveT);
            }

            // Visuals: Fade out and shrink as it merges into the target
            if (textMesh != null)
                textMesh.alpha = Mathf.Lerp(1f, 0f, t); // Fade out over time

            // Shrink to 0 to look like it's being absorbed
            transform.localScale = Vector3.Lerp(Vector3.one * 0.7f, Vector3.zero, t);

            yield return null;
        }

        Destroy(gameObject);
    }
}