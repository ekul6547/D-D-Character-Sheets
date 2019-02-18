using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_Manager : MonoBehaviour {

    public static GameObject NetworkManager;

    void Awake()
    {
        if(NetworkManager == null)
        {
            NetworkManager = GameObject.FindGameObjectWithTag("GameController");
        }
    }

    void Update()
    {
        if (NetworkManager != null)
        {
            NetworkManager.SetActive(SceneManager.GetActiveScene().name == "Lobby");
        }
    }

	public void GotoScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
