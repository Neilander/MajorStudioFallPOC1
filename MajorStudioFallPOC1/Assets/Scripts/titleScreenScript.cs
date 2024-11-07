using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class titleScreenScript : MonoBehaviour
{

    public AudioSource buttonSFX;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
           if (Input.GetKeyDown(KeyCode.X))
        {
            StartCoroutine(loadDelay());
        }

   
    }

            private IEnumerator loadDelay()
    {
        buttonSFX.Play();
        yield return new WaitForSeconds(1f); 
        SceneManager.LoadScene("SampleScene");
    }
}
