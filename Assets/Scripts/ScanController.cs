using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ScanController : MonoBehaviour
{
    public RawImage cameraFeed;
    public TextMeshProUGUI statusText;

    private WebCamTexture webCamTexture;
    private float scanTimer = 0f;
    private bool scanned = false;

    private string[] foodKeywords = {
        "lait", "pain", "oeuf", "oeufs", "eau", "jus", "beurre", "fromage",
        "yaourt", "viande", "poulet", "poisson", "riz", "pates", "farine",
        "sucre", "sel", "huile", "tomate", "pomme", "banane", "orange",
        "cafe", "the", "chocolat", "biscuit", "chips", "soda", "biere",
        "energie", "energy", "drink", "boisson", "cereales", "confiture",
        "miel", "sauce", "ketchup", "mayonnaise", "coca", "pepsi", "sprite"
    };

    void Start()
    {
        StartCamera();
    }

    void StartCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            statusText.text = "Aucune camera trouvee";
            return;
        }
        string camName = devices[0].name;
        foreach (var d in devices)
            if (!d.isFrontFacing) camName = d.name;

        webCamTexture = new WebCamTexture(camName, 1280, 720, 30);
        cameraFeed.texture = webCamTexture;
        webCamTexture.Play();
        statusText.text = "Pointez vers votre ticket...\nAppuyez pour scanner";
    }

    void Update()
    {
        if (scanned) return;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            StartCoroutine(SimulateScan());
        }
    }

    IEnumerator SimulateScan()
    {
        scanned = true;
        statusText.text = "Analyse en cours...";
        yield return new WaitForSeconds(1.5f);

        // Get current inventory and pretend we scanned some of them
        ProductManager pm = FindObjectOfType<ProductManager>();
        if (pm == null || pm.Products.Count == 0)
        {
            statusText.text = "Inventaire vide!";
            yield return new WaitForSeconds(2f);
            GoBack();
            yield break;
        }

        // Simulate: mark first half of products as "bought"
        var bought = new System.Collections.Generic.List<string>();
        int count = Mathf.Max(1, pm.Products.Count / 2);
        for (int i = 0; i < count; i++)
            bought.Add(pm.Products[i]);

        foreach (var item in bought)
            pm.Products.Remove(item);

        statusText.text = "Achetes: " + string.Join(", ", bought) +
                         "\n\nRestant: " + string.Join(", ", pm.Products);

        yield return new WaitForSeconds(3f);
        if (webCamTexture != null) webCamTexture.Stop();
        SceneManager.LoadScene("SampleScene");
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
