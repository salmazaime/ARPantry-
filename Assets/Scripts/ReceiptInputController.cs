using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ReceiptInputController : MonoBehaviour
{
    public TMP_InputField inputField;

    public void OnButtonClick()
    {
        if (inputField == null) return;

        string raw = inputField.text;
        if (string.IsNullOrEmpty(raw)) return;

        ProductManager pm = FindObjectOfType<ProductManager>();
        if (pm == null) return;

        pm.Products.Clear();

        foreach (var item in raw.Split(','))
        {
            string trimmed = item.Trim();
            if (!string.IsNullOrEmpty(trimmed))
                pm.Products.Add(trimmed);
        }

        SceneManager.LoadScene("SampleScene");
    }
}
