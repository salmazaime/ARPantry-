using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SceneManagement;
using TMPro;

public class ARManager : MonoBehaviour
{
    public GameObject arLabelPrefab;
    public ARRaycastManager arRaycastManager;

    private List<string> itemsToPlace = new List<string>();
    private List<GameObject> placedLabels = new List<GameObject>();
    private int currentIndex = 0;
    private bool ready = false;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        string saved = PlayerPrefs.GetString("MissingItems", "");
        Debug.Log($"ARManager loaded items: '{saved}'");

        if (!string.IsNullOrEmpty(saved))
        {
            itemsToPlace = new List<string>(saved.Split(','));
            ready = true;
            Debug.Log($"Ready to place {itemsToPlace.Count} items");
        }
        else
        {
            Debug.Log("No items found in PlayerPrefs!");
        }
    }

    void Update()
    {
        Debug.Log($"Update running, ready={ready}, index={currentIndex}, touchCount={Input.touchCount}");

        if (!ready || currentIndex >= itemsToPlace.Count) return;
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        Debug.Log("TOUCH BEGAN!");

        Camera cam = Camera.main;
        if (cam == null) { Debug.Log("NO CAMERA"); return; }

        Vector3 pos = cam.transform.position + cam.transform.forward * 1.5f;
        GameObject label = Instantiate(arLabelPrefab, pos, Quaternion.LookRotation(cam.transform.forward));
        var tmp = label.GetComponent<TextMeshPro>();
        if (tmp != null) tmp.text = itemsToPlace[currentIndex];
        placedLabels.Add(label);
        Debug.Log($"PLACED: {itemsToPlace[currentIndex]} at {pos}");
        currentIndex++;
        if (currentIndex >= itemsToPlace.Count) ready = false;
    }

    public void GoBack()
    {
        PlayerPrefs.DeleteKey("MissingItems");
        SceneManager.LoadScene("HomeScene");
    }
}
