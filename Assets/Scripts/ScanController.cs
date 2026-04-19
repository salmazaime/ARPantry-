using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ScanController : MonoBehaviour
{
    [Header("UI References")]
    public RawImage cameraFeed;
    public TextMeshProUGUI statusText;

    [Header("Simulated Shopping List")]
    public List<string> idealShoppingList = new List<string> {
        "lait", "pain", "oeuf", "beurre", "pomme", "riz", "pates", "coca"
    };

    private bool isScanning = false;

    void Start()
    {
        UpdateStatus("Appuyez sur SCAN TICKET pour analyser.");
    }

    public void OnScanButtonPressed()
    {
        if (isScanning) return;
        StartCoroutine(SimulateOCRProcess());
    }

    IEnumerator SimulateOCRProcess()
    {
        isScanning = true;

        UpdateStatus("Capture du ticket...");
        yield return new WaitForSeconds(1.0f);

        UpdateStatus("Analyse des articles...");
        yield return new WaitForSeconds(1.5f);

        if (ProductManager.Instance == null)
        {
            UpdateStatus("Erreur: ProductManager introuvable!");
            isScanning = false;
            yield break;
        }

        List<string> missingItems = new List<string>();
        List<string> currentInventory = ProductManager.Instance.Products;

        // Éléments présents en inventaire mais absents du ticket scanné
        foreach (string item in currentInventory)
        {
            if (!idealShoppingList.Contains(item.ToLower()))
                missingItems.Add(item);
        }

        if (missingItems.Count > 0)
        {
            string missingString = string.Join(", ", missingItems);
            UpdateStatus($"<color=red>MANQUANT:</color>\n{missingString}");
            Debug.Log("Missing items: " + missingString);
        }
        else
        {
            UpdateStatus("<color=green>BRAVO!</color>\nTout est dans l'inventaire.");
        }

        isScanning = false;
    }

    private void UpdateStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log($"SIM_STATUS: {msg}");
    }

    public void GoBack()
    {
        SceneManager.LoadScene("HomeScene");
    }
}
