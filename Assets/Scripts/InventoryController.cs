using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public TMP_InputField addInputField;
    public Transform contentParent;
    public GameObject itemButtonPrefab;

    void Start()
    {
        // Safety check: if the manager doesn't exist yet, we can't show anything
        if (ProductManager.Instance == null)
        {
            Debug.LogError("ProductManager Instance is missing! Make sure it started in HomeScene.");
            return;
        }
        RefreshList();
    }

    public void AddProduct()
    {
        string name = addInputField.text.Trim();
        if (string.IsNullOrEmpty(name)) return;

        // Use the Instance instead of searching the scene
        if (!ProductManager.Instance.Products.Contains(name))
        {
            ProductManager.Instance.Products.Add(name);
        }

        addInputField.text = "";
        RefreshList();
    }

    void RefreshList()
    {
        if (ProductManager.Instance == null) return;

        // Clear the old UI elements
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Create a new button for every product in the Master List
        foreach (string product in ProductManager.Instance.Products)
        {
            string captured = product;
            GameObject btn = Instantiate(itemButtonPrefab, contentParent);

            // Set the text on the button
            btn.GetComponentInChildren<TextMeshProUGUI>().text = captured;

            // Setup the remove button logic
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                ProductManager.Instance.Products.Remove(captured);
                RefreshList();
            });
        }
    }

    public void GoBack()
    {
        SceneManager.LoadScene("HomeScene");
    }
}
