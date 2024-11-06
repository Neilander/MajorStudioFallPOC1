using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gemScript : MonoBehaviour
{
    public float launchForce = 5f;            // 施加的力大小
    public Rigidbody2D rb;
    public Transform groundCheck;             // 地面检测的位置
    public LayerMask groundLayer;             // 地面图层
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f); // 地面检测区域大小（长方形）

    private bool isGrounded;
    private bool isFloating = false;          // 是否进入浮动状态
    public float floatAmplitude = 0.1f;      // 浮动的幅度
    public float floatFrequency = 2f;        // 浮动的频率
    private Vector3 originalPosition;         // 初始位置，用于浮动基准


    public float destroyY = -12f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
    }

    void Update()
    {
        // 检测是否在地面上
        isGrounded = CheckIfGrounded();

        if (isGrounded && !isFloating)
        {
            // 进入浮动状态
            isFloating = true;
            rb.velocity = Vector2.zero; // 停止施加的力
            rb.isKinematic = true;      // 设置为 kinematic 模式以防止受重力影响
            originalPosition = transform.position;
        }

        if (isFloating)
        {
            // 使用 sin 函数实现上下浮动效果
            float newY = originalPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            transform.position = new Vector3(originalPosition.x, newY, transform.position.z);
        }

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    private bool CheckIfGrounded()
    {
        // 如果 Rigidbody2D 的垂直速度大于 0，直接返回 false
        if (rb != null && rb.velocity.y > 0)
        {
            return false;
        }

        // 第一种情况：检测是否在地面上
        Collider2D groundCollider = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        if (groundCollider != null)
        {
            return true;
        }

        // 第二种情况：检测是否有带有 "corpseOnGround" 标签的物体
        Collider2D[] colliders = Physics2D.OverlapBoxAll(groundCheck.position, groundCheckSize, 0f);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("corpseOnGround"))
            {
                return true;
            }
        }

        // 如果以上条件都不满足，则返回 false
        return false;
    }

    public void initializeJem()
    {
        // 随机生成一个角度在 -45 到 45 度之间
        float angle = Random.Range(-45f, 45f);

        // 将角度转换为方向向量
        Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;

        // 施加力
        rb.AddForce(direction * launchForce, ForceMode2D.Impulse);
    }

    public void initializeJem(float chooseAngle, float launchForceTimer)
    {
        float angle = Random.Range(-chooseAngle, chooseAngle);
        Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
        rb.AddForce(direction * launchForce*launchForceTimer, ForceMode2D.Impulse);
    }

    // 在编辑器中绘制检测区域的 Gizmo
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red; // 设置 Gizmo 的颜色为红色
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize); // 绘制检测区域的长方形边框
        }
    }
}