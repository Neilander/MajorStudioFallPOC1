using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerControl : MonoBehaviour
{

    public float moveSpeed = 5f;        
    public float jumpForce = 10f;      
    public Transform groundCheck;       
    public LayerMask groundLayer;
    public Transform rotatingObject;
    public int HpValue = 100;
    public weaponManager manager;
    public bool inControl = true;
    public Vector2 dropX;
    public bool isLeftSide = true;
    public GameObject corpse;
    public SpriteRenderer renderer;
    public float detectionRadius = 5f;     // 检测半径
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public float curAtkSign;
    public float takeDamageForceScale;
    public float waitTimeToRecover;


    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isRotating = false;
    private float curHp;
    private float scaleX;
    private float scaleY;
    private float scaleZ;

    private Coroutine recoverCor;
    
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        curHp = HpValue;
        scaleX = transform.localScale.x;
        scaleY = transform.localScale.y;
        scaleZ = transform.localScale.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (inControl)
        {
            // 检测是否在地面上
            isGrounded = CheckIfGrounded();//Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

            

            // 根据角色侧面区分输入
            if (isLeftSide)
            {
                // 左侧角色使用 A/D 移动，W 跳跃，空格攻击
                float horizontalInput = 0;
                if (Input.GetKey(KeyCode.A)) horizontalInput = -1;
                else if (Input.GetKey(KeyCode.D)) horizontalInput = 1;

                HandleMovement(horizontalInput);
                HandleJump(Input.GetKeyDown(KeyCode.W));
                HandleAttack(Input.GetKeyDown(KeyCode.Space));
                // 左侧角色使用 Q 键消除最近的"corpseOnGround"物体
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    RemoveClosestCorpse();
                }
            }
            else
            {
                // 右侧角色使用 左/右箭头移动，上箭头跳跃，K 键攻击
                float horizontalInput = 0;
                if (Input.GetKey(KeyCode.LeftArrow)) horizontalInput = -1;
                else if (Input.GetKey(KeyCode.RightArrow)) horizontalInput = 1;

                HandleMovement(horizontalInput);
                HandleJump(Input.GetKeyDown(KeyCode.UpArrow));
                HandleAttack(Input.GetKeyDown(KeyCode.K));
                // 右侧角色使用 L 键消除最近的"corpseOnGround"物体
                if (Input.GetKeyDown(KeyCode.L))
                {
                    RemoveClosestCorpse();
                }
            }
        }
        else
        {
            isGrounded = CheckIfGrounded();
            if (isGrounded)
            {
                rb.gravityScale = 2;
                inControl = true;
            }
        }

    }

    private void HandleMovement(float horizontalInput)
    {
        // 移动角色
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        if(horizontalInput!=0 && !isRotating)
            transform.localScale = new Vector3(((horizontalInput>0)?1:-1) * scaleX, scaleY, scaleZ);
    }

    private void HandleJump(bool jumpKeyPressed)
    {
        // 检测跳跃输入和地面状态
        if (jumpKeyPressed && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void HandleAttack(bool attackKeyPressed)
    {
        // 检测攻击输入
        if (attackKeyPressed)
        {
            TriggerRotation(); // 攻击行为（这里是旋转）
        }
    }

   
    public void TriggerRotation()
    {
        if (!isRotating)
        {
            Debug.Log("StartRotating");
            StartCoroutine(RotateObject());
            manager.startAttackInitialize();
        }
    }

    private IEnumerator RotateObject()
    {
        isRotating = true;
        float rotationDuration = manager.reportTime();     // 每次180度旋转的持续时间（秒）
        float elapsed;
        curAtkSign = Mathf.Sign(transform.localScale.x);

        // 第一步：旋转180度
        Quaternion initialRotation = rotatingObject.rotation;
        Quaternion halfRotation = initialRotation * Quaternion.Euler(0, 0, 180*curAtkSign);
        elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            rotatingObject.rotation = Quaternion.Slerp(initialRotation, halfRotation, elapsed / rotationDuration);
            yield return null;
        }

        // 确保完成第一个180度旋转
        rotatingObject.rotation = halfRotation;

        // 第二步：再旋转180度
        Quaternion fullRotation = halfRotation * Quaternion.Euler(0, 0, 180*curAtkSign);
        elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            rotatingObject.rotation = Quaternion.Slerp(halfRotation, fullRotation, elapsed / rotationDuration);
            yield return null;
        }

        // 确保完成整个360度旋转
        rotatingObject.rotation = fullRotation;

        isRotating = false;  // 重置状态
        manager.finishAttack();
    }

    public void takeDamage(int n, Vector2 dir)
    {
        Debug.Log(name + "收到了"+n+"点伤害,朝向"+dir.x+", "+dir.y);
        curHp -= n;

        if (curHp <= 0)
        {
            GameObject gmo = Instantiate(corpse, transform.position, Quaternion.identity);
            gmo.GetComponent<SpriteRenderer>().color = isLeftSide ? Color.green : Color.red;
            gmo.GetComponent<deathOnGroundScript>().ifLeft = isLeftSide;
            manager.resetWeapon();
            transform.position = new Vector3(Random.Range(dropX.x, dropX.y), 50, 0);
            rb.gravityScale = 1;
            inControl = false;
            curHp = HpValue;
            if (recoverCor != null)
            {
                StopCoroutine(recoverCor);
            }

        }
        else
        {
            //dir.normalized*takeDamageForceScale
            Debug.Log("被打飞了！");
            rb.AddForce(dir.normalized*takeDamageForceScale*n, ForceMode2D.Impulse);
            inControl = false;
            if (recoverCor != null)
            {
                StopCoroutine(recoverCor);
            }
            recoverCor = StartCoroutine(recoverFromNoControl(waitTimeToRecover));
        }
    }

    private void RemoveClosestCorpse()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        float closestDistance = Mathf.Infinity;
        Collider2D closestCorpse = null;

        // 查找最近的"corpseOnGround"对象
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("corpseOnGround") && (collider.gameObject.GetComponent<deathOnGroundScript>().ifLeft == isLeftSide))
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCorpse = collider;
                }
            }
        }

        // 如果找到最近的"corpseOnGround"，则销毁它
        if (closestCorpse != null)
        {
            manager.addOne(closestCorpse.gameObject.GetComponent<SpriteRenderer>().color);
            Destroy(closestCorpse.gameObject);
        }
    }

    public void genCorpse(Vector3 pos)
    {
        GameObject gmo = Instantiate(corpse, pos, Quaternion.identity);
        gmo.GetComponent<SpriteRenderer>().color = isLeftSide ? Color.green : Color.red;
        gmo.GetComponent<deathOnGroundScript>().ifLeft = isLeftSide;
    }

    private bool CheckIfGrounded()
    {
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

        // 第二种情况和第三种情况：检测长方形区域内的所有碰撞物体
        Collider2D[] colliders = Physics2D.OverlapBoxAll(groundCheck.position, groundCheckSize, 0f);
        foreach (var collider in colliders)
        {
            // 第二种情况：检测是否有带有 "corpseOnGround" 标签的物体
            if (collider.CompareTag("corpseOnGround"))
            {
                return true;
            }

            // 第三种情况：检测是否有带有 "PlayerControl" 组件且不等于自己
            playerControl otherPlayer = collider.GetComponent<playerControl>();
            if (otherPlayer != null && otherPlayer != this)
            {
                return true;
            }
        }

        // 如果以上条件都不满足，则返回 false
        return false;
    }

    // 绘制检测区域（仅在编辑器中可见，方便调试）
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }

    IEnumerator recoverFromNoControl(float t)
    {
        yield return new WaitForSeconds(t);
        inControl = true;
    }
}
