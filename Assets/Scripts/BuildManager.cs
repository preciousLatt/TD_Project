using System.Collections.Generic;
using UnityEngine;
using Singleton;

public class BuildManager : Singleton<BuildManager>
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask towerMask;
    [SerializeField] private float placementCheckPadding = 1f;

    private GameObject currentValidPreviewPrefab;
    private GameObject currentInvalidPreviewPrefab;
    private GameObject currentActualPrefab;

    private GameObject currentPreviewInstance;
    private bool lastCanPlaceState = true;

    private Renderer[] previewRenderersCache;
    private float previewCheckRadius = 0.5f;

    public override void Awake()
    {
        base.Awake();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    private void Update()
    {
        if (currentPreviewInstance == null) return;

        UpdatePreviewPositionAndValidity();
        HandleInput();
    }

    public void SellTower(GameObject towerInstance)
    {
        if (towerInstance == null) return;

        int cost = 100; 

        Tower towerScript = towerInstance.GetComponent<Tower>();
        if (towerScript != null)
        {
            cost = towerScript.cost;
        }

        int refundAmount = Mathf.FloorToInt(cost * 0.5f);

        GameManager.Instance.AddMoney(refundAmount);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGoldPopup(towerInstance.transform.position, refundAmount);

            if (UIManager.Instance.CurrentTower != null && UIManager.Instance.CurrentTower.gameObject == towerInstance)
            {
                UIManager.Instance.SetActiveTower(null);
            }
        }

        Destroy(towerInstance);
    }

    public void StartPlacing(GameObject validPreviewPrefab, GameObject invalidPreviewPrefab, GameObject actualPrefab)
    {
        CancelPlacement();

        currentValidPreviewPrefab = validPreviewPrefab;
        currentInvalidPreviewPrefab = invalidPreviewPrefab;
        currentActualPrefab = actualPrefab;

        if (currentValidPreviewPrefab == null || currentInvalidPreviewPrefab == null || currentActualPrefab == null)
            return;

        CreatePreviewInstance(currentValidPreviewPrefab);
        previewCheckRadius = ComputePreviewCheckRadius(currentPreviewInstance);
    }

    public void CancelPlacement()
    {
        if (currentPreviewInstance != null)
        {
            Destroy(currentPreviewInstance);
            currentPreviewInstance = null;
        }

        currentValidPreviewPrefab = null;
        currentInvalidPreviewPrefab = null;
        currentActualPrefab = null;
        previewRenderersCache = null;
    }

    private void UpdatePreviewPositionAndValidity()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundMask))
        {
            currentPreviewInstance.transform.position = hit.point;
            bool canPlace = CanPlaceAt(hit.point);

            if (canPlace != lastCanPlaceState)
            {
                SwapPreviewPrefabInstance(canPlace);
                lastCanPlaceState = canPlace;
            }
        }
        else
        {
            if (lastCanPlaceState != false)
            {
                SwapPreviewPrefabInstance(false);
                lastCanPlaceState = false;
            }
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 placePos = currentPreviewInstance.transform.position;
            if (CanPlaceAt(placePos))
            {
                ConfirmPlacement(placePos, currentPreviewInstance.transform.rotation);
            }
            else
            {
                Debug.Log("Cannot place here.");
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    private bool CanPlaceAt(Vector3 worldPos)
    {
        float checkRadius = previewCheckRadius + placementCheckPadding;
        Collider[] hits = Physics.OverlapSphere(worldPos, checkRadius, towerMask);
        return hits.Length == 0;
    }

    private void ConfirmPlacement(Vector3 worldPos, Quaternion rot)
    {
        if (currentActualPrefab == null) return;

        var cmd = new BuildTowerCommand(currentActualPrefab, worldPos, rot);
        CommandManager.Instance.ExecuteCommand(cmd);

        Destroy(currentPreviewInstance);
        currentPreviewInstance = null;

        currentValidPreviewPrefab = null;
        currentInvalidPreviewPrefab = null;
        currentActualPrefab = null;
        previewRenderersCache = null;
    }

    private void SwapPreviewPrefabInstance(bool wantValid)
    {
        GameObject prefabToUse = wantValid ? currentValidPreviewPrefab : currentInvalidPreviewPrefab;
        if (prefabToUse == null) return;

        Vector3 pos = currentPreviewInstance != null ? currentPreviewInstance.transform.position : Vector3.zero;
        Quaternion rot = currentPreviewInstance != null ? currentPreviewInstance.transform.rotation : Quaternion.identity;

        if (currentPreviewInstance != null)
            Destroy(currentPreviewInstance);

        CreatePreviewInstance(prefabToUse);
        currentPreviewInstance.transform.position = pos;
        currentPreviewInstance.transform.rotation = rot;

        previewCheckRadius = ComputePreviewCheckRadius(currentPreviewInstance);
    }

    private void CreatePreviewInstance(GameObject prefab)
    {
        currentPreviewInstance = Instantiate(prefab);
        DisableRuntimeBehaviours(currentPreviewInstance);

        int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreLayer >= 0) SetLayerRecursively(currentPreviewInstance, ignoreLayer);

        previewRenderersCache = currentPreviewInstance.GetComponentsInChildren<Renderer>(true);
    }

    private float ComputePreviewCheckRadius(GameObject preview)
    {
        if (preview == null) return 0.5f;

        float maxExtent = 0f;
        Renderer[] rends = preview.GetComponentsInChildren<Renderer>(true);
        foreach (var r in rends)
        {
            if (r == null) continue;
            float ext = r.bounds.extents.magnitude;
            if (ext > maxExtent) maxExtent = ext;
        }

        Collider[] cols = preview.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols)
        {
            if (c == null) continue;
            float ext = c.bounds.extents.magnitude;
            if (ext > maxExtent) maxExtent = ext;
        }

        if (maxExtent <= 0f) maxExtent = 0.5f;
        return maxExtent;
    }

    private void DisableRuntimeBehaviours(GameObject go)
    {
        if (go == null) return;

        var monos = go.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var mb in monos) mb.enabled = false;

        var cols = go.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols) c.enabled = false;

        var rbs = go.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;
        obj.layer = layer;
        foreach (Transform t in obj.transform)
            SetLayerRecursively(t.gameObject, layer);
    }
}