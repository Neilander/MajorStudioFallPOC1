using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class corpseCode : MonoBehaviour
{

    public weaponManager manager;
    public int index;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("I touch someone");
        if (other.gameObject.GetComponent<playerControl>() != null)
        {
            //Debug.Log("I hit someone");
            manager.reportAttack(other.gameObject.GetComponent<playerControl>(), (index+1) * 5);
        }
    }

}