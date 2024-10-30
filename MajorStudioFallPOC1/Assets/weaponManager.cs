using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponManager : MonoBehaviour
{

    public List<GameObject> allCorpse;
    public stickScript stick;
    private int curIndex = 0;
    private bool canDamage = false;
    public float waitTimePerBlock = 0.12f;
    public playerControl control;

    private Dictionary<playerControl, int> damageRecord = new Dictionary<playerControl, int>();
    // Start is called before the first frame update
    void Start()
    {
        int n = 0;
        foreach (GameObject gmo in allCorpse)
        {
            if (gmo.GetComponent<corpseCode>() != null)
            {
                gmo.GetComponent<corpseCode>().manager = this;
                gmo.GetComponent<corpseCode>().index = n;
                n += 1;
            }
        }
        stick.manager = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
            addOne(Color.white);
    }

    public void resetWeapon()
    {
        curIndex = 0;
        foreach (GameObject gmo in allCorpse)
        {
            if(gmo.activeInHierarchy)
                control.genCorpse(gmo.transform.position);
            gmo.SetActive(false);
            

        }
            
    }

    public void addOne(Color c)
    {
        if (curIndex >= allCorpse.Count)
            return;
        allCorpse[curIndex].SetActive(true);
        allCorpse[curIndex].GetComponent<SpriteRenderer>().color = Color.white;
        curIndex += 1;
    }

    public void reportAttack(playerControl tar, int n)
    {
        if (tar == control)
            return;
        if ((!damageRecord.ContainsKey(tar))&& canDamage)
        {
            damageRecord.Add(tar, n);
            tar.takeDamage(n);
        }
        
    }

    public void startAttackInitialize()
    {
        damageRecord = new Dictionary<playerControl, int>();
        canDamage = true;
        
    }

    public void finishAttack()
    {
        canDamage = false;
    }

    public float reportTime()
    {
        return 0.1f + curIndex *waitTimePerBlock;
    }
    
}
