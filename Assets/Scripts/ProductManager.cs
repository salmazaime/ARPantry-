using System.Collections.Generic;
using UnityEngine;

public class ProductManager : MonoBehaviour
{
    public static ProductManager Instance;
    public List<string> Products = new List<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps data alive between scenes
        }
        else
        {
            Destroy(gameObject); // Prevents duplicates
        }
    }
}
