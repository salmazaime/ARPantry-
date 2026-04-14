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
        RefreshList();
    }

    public void AddProduct()
    {
        string name = addInputField.text.Trim();
        if (string.IsNullOrEmpty(name)) return;
        ProductManager pm = FindObjectOfType<ProductManager>();
        if (pm == null) return;
        if (!pm.Products.Contains(name))
            pm.Products.Add(name);
        addInputField.text = "";
        RefreshList();
    }

    void RefreshList()
    {
        ProductManager pm = FindObjectOfType<ProductManager>();
        if (pm == null) return;
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);
        foreach (string product in pm.Products)
        {
            string captured = product;
            GameObject btn = Instantiate(itemButtonPrefab, contentParent);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = captured;
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                pm.Products.Remove(captured);
                RefreshList();
            });
        }
    }

    public void GoBack()
    {
        SceneManager.LoadScene("HomeScene");
    }
}
