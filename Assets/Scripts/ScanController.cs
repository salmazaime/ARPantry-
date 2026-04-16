using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class ScanController : MonoBehaviour
{
    [Header("UI References")]
    public RawImage cameraFeed;
    public TextMeshProUGUI statusText;

    [Header("Settings")]
    public string apiKey = "helloworld";

    private WebCamTexture webCamTexture;
    private bool isScanning = false;

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
        Debug.Log("<color=cyan>SCANNER: Start sequence initiated.</color>");
        UpdateStatus("Checking permissions...");

#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Debug.Log("<color=orange>SCANNER: Requesting Camera Permission...</color>");
            Permission.RequestUserPermission(Permission.Camera);
            StartCoroutine(WaitForPermission());
        }
        else
        {
            Debug.Log("<color=green>SCANNER: Permission already granted.</color>");
            StartCamera();
        }
#else
        StartCamera();
#endif
    }

    IEnumerator WaitForPermission()
    {
        float timer = 0;
        while (!Permission.HasUserAuthorizedPermission(Permission.Camera) && timer < 5f)
        {
            timer += 0.5f;
            yield return new WaitForSeconds(0.5f);
        }
        StartCamera();
    }

    void StartCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        Debug.Log($"<color=cyan>SCANNER: Found {devices.Length} camera devices.</color>");

        if (devices.Length == 0)
        {
            UpdateStatus("FATAL: No cameras found!");
            return;
        }

        string camName = devices[0].name;
        foreach (var d in devices)
        {
            Debug.Log($"SCANNER: Camera found: {d.name} (Front: {d.isFrontFacing})");
            if (!d.isFrontFacing) camName = d.name;
        }

        webCamTexture = new WebCamTexture(camName);
        cameraFeed.texture = webCamTexture;
        webCamTexture.Play();

        UpdateStatus("Camera Live. Press SCAN.");
    }

    // CONNECT THIS TO YOUR BUTTON ONCLICK()
    public void OnScanButtonPressed()
    {
        Debug.Log("<color=yellow>BUTTON: Scan Button Clicked!</color>");

        if (isScanning)
        {
            Debug.LogWarning("BUTTON: Ignore - already scanning.");
            return;
        }

        if (webCamTexture == null)
        {
            Debug.LogError("BUTTON ERROR: webCamTexture is NULL!");
            UpdateStatus("Error: Camera null");
            return;
        }

        if (!webCamTexture.isPlaying)
        {
            Debug.LogWarning("BUTTON: Camera was not playing. Attempting Play()...");
            webCamTexture.Play();
        }

        StartCoroutine(ActualScan());
    }

    IEnumerator ActualScan()
    {
        isScanning = true;
        UpdateStatus("Capturing...");
        Debug.Log("SCAN: Coroutine started.");

        // Wait for camera to actually give data (Pixel 9 fix)
        float timeout = 0;
        while (webCamTexture.width < 100 && timeout < 2.0f)
        {
            timeout += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"SCAN: Camera Data Size: {webCamTexture.width}x{webCamTexture.height}");

        yield return new WaitForEndOfFrame();

        try
        {
            Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);
            photo.SetPixels(webCamTexture.GetPixels());
            photo.Apply();
            Debug.Log("SCAN: Texture2D created and pixels set.");

            byte[] bytes = photo.EncodeToJPG(75);
            string base64Image = System.Convert.ToBase64String(bytes);
            Destroy(photo);
            Debug.Log($"SCAN: Image encoded. Base64 length: {base64Image.Length}");

            StartCoroutine(PostToOCR(base64Image));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SCAN CRASH: {e.Message}");
            UpdateStatus("Capture Failed!");
            isScanning = false;
        }
    }

    IEnumerator PostToOCR(string base64)
    {
        UpdateStatus("Sending to API...");
        Debug.Log("API: Preparing Web Request.");

        WWWForm form = new WWWForm();
        form.AddField("base64Image", "data:image/jpg;base64," + base64);
        form.AddField("apikey", apiKey);
        form.AddField("language", "fre");

        using (UnityWebRequest www = UnityWebRequest.Post("https://api.ocr.space/parse/image", form))
        {
            www.SetRequestHeader("Origin", "https://api.ocr.space");
            www.timeout = 25;
            www.certificateHandler = new BypassCertificate();

            Debug.Log("API: Sending Request now...");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API ERROR: {www.error}");
                UpdateStatus($"Nettttttt");
                UpdateStatus($"Net Error: {www.error}");
                isScanning = false;
            }
            else
            {
                string result = www.downloadHandler.text;
                Debug.Log($"API SUCCESS: {result}");
                ParseAndAddProducts(result.ToLower());
            }
        }
    }

    void ParseAndAddProducts(string json)
    {
        if (ProductManager.Instance == null)
        {
            Debug.LogError("LOGIC: ProductManager.Instance is NULL!");
            UpdateStatus("Error: No ProductManager");
            isScanning = false;
            return;
        }

        List<string> detected = new List<string>();
        foreach (string keyword in foodKeywords)
        {
            if (json.Contains(keyword))
            {
                if (!ProductManager.Instance.Products.Contains(keyword))
                {
                    ProductManager.Instance.Products.Add(keyword);
                    detected.Add(keyword);
                }
            }
        }

        Debug.Log($"LOGIC: Found {detected.Count} keywords.");
        UpdateStatus(detected.Count > 0 ? $"Found: {string.Join(", ", detected)}" : "No products found.");

        if (detected.Count > 0) Invoke("GoBack", 3.0f);
        else isScanning = false;
    }

    private void UpdateStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log($"<color=white>STATUS: {msg}</color>");
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

// Ensure this is at the VERY BOTTOM of the file
public class BypassCertificate : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData) => true;
}
