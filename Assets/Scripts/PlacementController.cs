using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class PlacementController : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public GameObject labelPrefab;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject spawnedLabel;

    void Update()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            if (spawnedLabel == null)
            {
                spawnedLabel = Instantiate(labelPrefab, hitPose.position, hitPose.rotation);
            }
            else
            {
                spawnedLabel.transform.position = hitPose.position;
            }

            // Update text with products
            TextMeshPro tmp = spawnedLabel.GetComponentInChildren<TextMeshPro>();
            if (tmp != null && ProductManager.Instance != null)
            {
                tmp.text = "🛒 Produits:\n" + string.Join("\n", ProductManager.Instance.Products);
            }
        }
    }
}

