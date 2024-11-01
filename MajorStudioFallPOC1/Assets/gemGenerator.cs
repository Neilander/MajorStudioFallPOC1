using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gemGenerator : MonoBehaviour
{
    public GameObject gemPrefab;  // 宝石预制体
    public Transform spawnPoint;  // 宝石生成的位置
    private int gemCount = 20;    // 生成宝石的数量
    private float spawnInterval = 0.2f; // 每个宝石生成的时间间隔

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
            StartGeneratingGems();
    }

    // 开始生成宝石的函数
    public void StartGeneratingGems()
    {
        StartCoroutine(GenerateGems());
    }

    private IEnumerator GenerateGems()
    {
        for (int i = 0; i < gemCount; i++)
        {
            // 实例化宝石并设置位置
            GameObject gem = Instantiate(gemPrefab, spawnPoint.position, Quaternion.identity);

            // 调用宝石的 initializeJem 方法
            gemScript gemScript = gem.GetComponent<gemScript>();
            if (gemScript != null)
            {
                gemScript.initializeJem();
            }

            // 等待指定的间隔时间
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
