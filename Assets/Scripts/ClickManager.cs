using UnityEngine;
using UnityEngine.EventSystems;

public class ClickManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                TowerSelector selector = hit.collider.GetComponent<TowerSelector>();
                if (selector != null)
                {
                    selector.SelectTower();
                    return;
                }
            }

            TowerSelector.DeselectAll();
        }
    }
}
