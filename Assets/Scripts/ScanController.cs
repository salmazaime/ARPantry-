using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ScanController : MonoBehaviour
{
    [Header("UI References")]
    public RawImage cameraFeed; // Still keep this for the "look" of the app
    public TextMeshProUGUI statusText;

    [Header("Simulated Shopping List")]
    // This is what the user *should* have bought
    public List<string> idealShoppingList = new List<string> {
        "lait", "pain", "oeuf", "beurre", "pomme", "riz", "pates", "coca"
    };

    private WebCamTexture webCamTexture;
    private bool isScanning = false;

    void Start()
    {
        UpdateStatus("Camera Simulation Active.");
        StartCamera();
    }

    void StartCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            webCamTexture = new WebCamTexture(devices[0].name);
            cameraFeed.texture = webCamTexture;
            webCamTexture.Play();
        }
    }

    // THIS IS LINKED TO YOUR BUTTON
    public void OnScanButtonPressed()
    {
        if (isScanning) return;
        StartCoroutine(SimulateOCRProcess());
    }

    IEnumerator SimulateOCRProcess()
    {
        isScanning = true;

        // 1. Visual Feedback
        UpdateStatus("Capturing Ticket...");
        yield return new WaitForSeconds(1.0f);

        UpdateStatus("Analyzing Items (Simulated)...");
        yield return new WaitForSeconds(1.5f);

        // 2. Logic: Compare Inventory with Ideal List
        if (ProductManager.Instance == null)
        {
            UpdateStatus("Error: ProductManager Instance not found!");
            isScanning = false;
            yield break;
        }

        List<string> missingItems = new List<string>();
        List<string> currentInventory = ProductManager.Instance.Products;

        // Check which items from our hardcoded list are NOT in the inventory
        foreach (string item in idealShoppingList)
        {
            // Simple check (converted to lowercase to be safe)
            if (!currentInventory.Contains(item.ToLower()))
            {
                missingItems.Add(item);
            }
        }

        // 3. Display Result
        if (missingItems.Count > 0)
        {
            string missingString = string.Join(", ", missingItems);
            UpdateStatus($"<color=red>MANQUANT:</color>\n{missingString}");
            Debug.Log("Missing Items: " + missingString);
        }
        else
        {
            UpdateStatus("<color=green>BRAVO!</color>\nTout est dans l'inventaire.");
        }

        // 4. Wait so user can read, then go back
        yield return new WaitForSeconds(5.0f);
        GoBack();
    }

    private void UpdateStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log($"SIM_STATUS: {msg}");
    }

    public void GoBack()
    {
        if (webCamTexture != null) webCamTexture.Stop();
        SceneManager.LoadScene("HomeScene");
    }

    void OnDestroy()
    {
        if (webCamTexture != null) webCamTexture.Stop();
    }
}
