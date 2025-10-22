using UnityEngine;
using UnityEngine.UI;

public class TowerButton : MonoBehaviour
{
    public GameObject validPreviewPrefab;

    public GameObject invalidPreviewPrefab;

    public GameObject actualPrefab;

    [SerializeField] private Button button;

    private void Reset()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        if (button != null)
            button.onClick.AddListener(OnClick_StartPlacing);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClick_StartPlacing);
    }

    public void OnClick_StartPlacing()
    {
        BuildManager.Instance.StartPlacing(validPreviewPrefab, invalidPreviewPrefab, actualPrefab);
    }
}
