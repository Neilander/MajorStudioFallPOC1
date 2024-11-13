using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class textScript : MonoBehaviour
{
    public Color defaultColor;
    public Color lightColor;
    public float fadeDuration = 0.5f; // 渐变时间
    private Coroutine lightCoroutine; // 用于跟踪当前的渐变协程
    private TextMeshProUGUI textMesh; // 目标 TextMeshProUGUI 组件
    // Start is called before the first frame update
    void Start()
    {
        // 获取 TextMeshProUGUI 组件
        textMesh = GetComponent<TextMeshProUGUI>();

        // 设置初始颜色为 defaultColor
        if (textMesh != null)
        {
            textMesh.color = defaultColor;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            lightUp();
    }

    public void lightUp()
    {
        // 如果已有协程在运行，则停止它
        if (lightCoroutine != null)
        {
            StopCoroutine(lightCoroutine);
        }

        // 启动新的渐变协程
        lightCoroutine = StartCoroutine(LightUpCoroutine());
    }

    // 协程：实现颜色逐渐变亮再变回的效果
    private IEnumerator LightUpCoroutine()
    {
        // 渐变到 lightColor
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            if (textMesh != null)
            {
                textMesh.color = Color.Lerp(defaultColor, lightColor, elapsed / fadeDuration);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 确保颜色完全变为 lightColor
        if (textMesh != null)
        {
            textMesh.color = lightColor;
        }

        // 渐变回 defaultColor
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            if (textMesh != null)
            {
                textMesh.color = Color.Lerp(lightColor, defaultColor, elapsed / fadeDuration);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 确保颜色完全恢复为 defaultColor
        if (textMesh != null)
        {
            textMesh.color = defaultColor;
        }

        // 结束时将协程置空
        lightCoroutine = null;
    }
}
