using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeController : MonoBehaviour
{
    public void GoToInventory()
    {
        SceneManager.LoadScene("InventoryScene");
    }

    public void GoToScan()
    {
        SceneManager.LoadScene("ScanScene");
    }
}
