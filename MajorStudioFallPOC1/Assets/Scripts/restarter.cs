using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class restarter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            string currentSceneName = SceneManager.GetActiveScene().name;

            // 使用场景名称重新加载场景
            SceneManager.LoadScene(currentSceneName);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
            
    }
}
