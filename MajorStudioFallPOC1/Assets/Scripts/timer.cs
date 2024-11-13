using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class timer : MonoBehaviour
{
    public TextMeshProUGUI timerText; // 引用 TextMeshProUGUI 组件
    public int currentTime = 60;     // 初始时间为 60 秒
    private float timerInterval = 1f; // 每秒倒计时一次
    private bool isCountingDown = true;
    public float basicSize;
    public float increaseSize;

    [Header("camMove")]
    public Transform target;                // 目标位置
    public float moveSpeed = 2f;            // 相机移动速度（控制进度百分比的变化速度）
    public float targetOrthographicSize = 3f; // 目标缩放大小
    private Camera cam;                     // 相机组件
    private float originalOrthographicSize; // 初始相机大小
    private bool isMoving = false;          // 是否正在移动的标识
    private float progress = 0f;            // 移动和缩放的进度百分比（0 到 1）

    [Header("two player")]
    public playerControl p1;
    public playerControl p2;


    public TextMeshProUGUI textMesh;
    public AnimationCurve movementCurve;
    public AnimationCurve colorCurve;
    public Vector3 textMoveOffset = new Vector3(0, 1, 0); // 移动的目标偏移量
    public float animationDuration = 1f;
    

    private void Start()
    {
        // 开始倒计时协程
        StartCoroutine(StartCountdown());
        cam = Camera.main;
        originalOrthographicSize = cam.orthographicSize;
    }

    private void Update()
    {
        if (isMoving && target != null)
        {
            // 更新进度百分比
            progress += moveSpeed * Time.deltaTime;
            progress = Mathf.Clamp01(progress); // 确保进度在 0 到 1 之间

            // 使用 Lerp 根据百分比进行位置插值
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, cam.transform.position.z);
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPosition, progress);

            // 使用 Lerp 根据百分比进行相机缩放插值
            cam.orthographicSize = Mathf.Lerp(originalOrthographicSize, targetOrthographicSize, progress);

            // 当进度达到 1 时，停止移动
            if (progress >= 1f)
            {
                isMoving = false;
                progress = 0f; // 重置进度百分比，方便下次使用
                cam.transform.position = new Vector3(target.position.x, target.position.y, cam.transform.position.z);
                StartCoroutine(TextMoveAndFadeEffect());
            }
        }
    }

    private IEnumerator StartCountdown()
    {
        while (currentTime > 0 && isCountingDown)
        {
            UpdateTimerDisplay(); // 更新显示
            yield return new WaitForSeconds(timerInterval); // 等待一秒
            currentTime--; // 减少时间
        }

        

        // 倒计时结束，处理逻辑
        if (currentTime <= 0)
        {
            currentTime = 0;
            endTheGame();
            UpdateTimerDisplay();
            // 可以在这里添加其他的倒计时结束逻辑
        }
    }

    private void UpdateTimerDisplay()
    {
        // 显示当前时间
        timerText.text = currentTime.ToString();

        // 如果时间小于等于 10 秒，改变字体颜色和大小
        if (currentTime <= 10)
        {
            timerText.color = Color.red;           // 设置颜色为红色
            timerText.fontSize = basicSize+(10-currentTime)*increaseSize;               // 增大字体大小

        }
        else
        {
            //timerText.color = Color.white;         // 设置颜色为白色
            timerText.fontSize = basicSize;               // 恢复原始字体大小
        }
    }

    private void endTheGame()
    {
        timerText.enabled = false;
        foreach (EndAble ed in FindObjectsOfType<EndAble>())
        {
            ed.doEndGame();
        }
        if (p1.curScore > p2.curScore)
        {
            MoveToTarget(p1.transform);
        }
        else
        {
            MoveToTarget(p2.transform);
        }
            
    }

    // 开始移动相机的函数
    public void MoveToTarget(Transform newTarget)
    {
        target = newTarget;                   // 设置新的目标位置
        isMoving = true;                      // 开始移动
        progress = 0f;                        // 重置进度百分比
        cam.orthographicSize = originalOrthographicSize; // 重置相机大小
    }

    private IEnumerator TextMoveAndFadeEffect()
    {
        
        Vector3 startPosition = textMesh.transform.position;
        Vector3 endPosition = startPosition + textMoveOffset;
        float elapsedTime = 0f;

        bool set = false;
        Color startColor = textMesh.color;
        Color endColor = startColor;
        endColor.a = 1f; // 最终颜色为完全不透明

        // 初始化透明度为 0
        startColor.a = 0f;
        textMesh.color = startColor;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;

            // 根据 AnimationCurve 计算移动进度
            float curveValue = movementCurve.Evaluate(t);
            float colorcurveValue = colorCurve.Evaluate(t);

            if (t > 0.2f && !set)
            {
                set = true;
                textMesh.gameObject.SetActive(true);
            }

            // 位置插值
            textMesh.transform.position = Vector3.Lerp(startPosition, endPosition, curveValue);

            // 透明度插值
            Color currentColor = Color.Lerp(startColor, endColor, colorcurveValue);
            textMesh.color = currentColor;

            yield return null;
        }

        // 确保在结束时完全设置到最终位置和透明度
        //textMesh.transform.position = endPosition;
        textMesh.color = endColor;
    }
}
