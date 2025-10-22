using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] private float moveUpSpeed = 1.5f;
    [SerializeField] private float lifetime = 1.2f;
    [SerializeField] private Vector3 moveOffset = new Vector3(0, 1.5f, 0);

    private TextMeshPro textMesh;
    private float timer;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Initialize(float damageAmount, Color color)
    {
        textMesh.text = Mathf.RoundToInt(damageAmount).ToString();
        textMesh.color = color;
        timer = lifetime;
    }

    private void Update()
    {
        transform.position += Vector3.up * moveUpSpeed * Time.deltaTime;

        timer -= Time.deltaTime;
        float alpha = Mathf.Clamp01(timer / lifetime);
        var c = textMesh.color;
        c.a = alpha;
        textMesh.color = c;

        if (timer <= 0f)
            Destroy(gameObject);
    }
}
