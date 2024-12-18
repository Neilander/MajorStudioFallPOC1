using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class playerControl : EndAble
{

    public float moveSpeed = 5f;        
    public float jumpForce = 10f;      
    public Transform groundCheck;       
    public LayerMask groundLayer;
    public Transform rotatingObject;
    public int HpValue = 100;
    public weaponManager manager;
    public bool inControl = true;
    public bool canMove = true;
    public Vector2 dropX;
    public bool isLeftSide = true;
    public GameObject corpse;
    public SpriteRenderer renderer;
    public float detectionRadius = 5f;     // 检测半径
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public float curAtkSign;
    public float takeDamageForceScale;
    public float waitTimeToRecover;
    public float healthRecoverThreshold = 5f;
    [Header("grravity scale")]
    public float originalScale = 2f;
    public float quickFallDownScale = 5f;
    [Header("Score Ui")]
    public TextMeshProUGUI scoreText;
    public textScript txt;

    [Header("death check height")]
    public float destroyY = -8.5f;

    [Header("gem generator after death")]
    public GameObject generatorPrefab;

    public Color DamageColorLevel1;
    public Color DamageColorLevel2;

    public float invulTime = 3f;
    public GameObject invulLightObj;

    public Color deathColor;

    public GameObject dustEffectPrefab;
    public Vector3 posAdjust;


    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isRotating = false;
    private float curHp;
    private float scaleX;
    private float scaleY;
    private float scaleZ;
    private bool healthRecover = false;
    private float healthRecoverTimer = 0f;        // 恢复计时器
    

    private Coroutine recoverCor;
    private Coroutine healthRecoverCor;
    public int curScore = 0;

    private bool invulnerable = false;
    private Coroutine invulCor;
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        curHp = HpValue;
        scaleX = transform.localScale.x;
        scaleY = transform.localScale.y;
        scaleZ = transform.localScale.z;
        manager.addOne(deathColor);
    }

    // Update is called once per frame
    void Update()
    {
        if (curHp > 15)
        {
            renderer.color = Color.white;
        }
        else if (curHp > 10 && curHp <= 15)
        {
            renderer.color = DamageColorLevel1;
        }
        else if (curHp <= 10)
        {
            renderer.color = DamageColorLevel2;
        }

        if (ifEndGame)
        {
            if (!rb.isKinematic)
            {
                //rb.gravityScale = 0;
                StopAllCoroutines();
                rb.isKinematic = true;
                rb.velocity = Vector2.zero;
            }
            return;
        }

        if (inControl)
        {
            // 检测是否在地面上
            isGrounded = CheckIfGrounded();//Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

            if (isGrounded)
            {
                canMove = true;
                if (rb.gravityScale == quickFallDownScale)
                {
                    Instantiate(dustEffectPrefab, transform.position+posAdjust, Quaternion.Euler(-90,0,0)).GetComponent<ParticleSystem>().Play();

                }
                   
                rb.gravityScale = originalScale;
            }
                

            if (canMove)
            {
                // 根据角色侧面区分输入
                if (isLeftSide)
                {
                    // 左侧角色使用 A/D 移动，W 跳跃，空格攻击
                    float horizontalInput = 0;
                    if (Input.GetKey(KeyCode.A)) horizontalInput = -1;
                    else if (Input.GetKey(KeyCode.D)) horizontalInput = 1;

                    HandleMovement(horizontalInput);
                    HandleJump(Input.GetKeyDown(KeyCode.W));
                    HandFall(Input.GetKeyDown(KeyCode.S));
                }
                else
                {
                    // 右侧角色使用 左/右箭头移动，上箭头跳跃，K 键攻击
                    float horizontalInput = 0;
                    if (Input.GetKey(KeyCode.LeftArrow)) horizontalInput = -1;
                    else if (Input.GetKey(KeyCode.RightArrow)) horizontalInput = 1;

                    HandleMovement(horizontalInput);
                    HandleJump(Input.GetKeyDown(KeyCode.UpArrow));
                    HandFall(Input.GetKeyDown(KeyCode.DownArrow));

                }
            }

            if (isLeftSide)
            {
                HandleAttack(Input.GetKeyDown(KeyCode.Space));
                // 左侧角色使用 Q 键消除最近的"corpseOnGround"物体
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    RemoveClosestCorpse();
                }
            }
            else
            {
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
                rb.gravityScale = originalScale;
                inControl = true;
                canMove = true;
            }
        }

        if (healthRecover)
        {

            // 累加计时器
            healthRecoverTimer += Time.deltaTime;

            // 如果计时器达到阈值，恢复一点生命并重置计时器
            if (healthRecoverTimer >= healthRecoverThreshold)
            {
                
                curHp = Mathf.Min(curHp + 1, HpValue);// 恢复 1 点生命值
                Debug.Log("恢复了，现在是" + curHp);
                healthRecoverTimer = 0f; // 重置计时器
            }
        }
        else
        {
            // 如果不在恢复状态，重置计时器
            healthRecoverTimer = 0f;
        }

        if (transform.position.y < destroyY)
        {
            Instantiate(generatorPrefab, transform.position, Quaternion.identity).GetComponent<gemGenerator>().GenInOnce(curScore);
            curScore = 0;
            scoreText.text = "0";

            GameObject gmo = Instantiate(corpse, transform.position, Quaternion.identity);
            gmo.GetComponent<SpriteRenderer>().color = deathColor;
            gmo.GetComponent<deathOnGroundScript>().ifLeft = isLeftSide;
            manager.resetWeapon(false);
            transform.position = new Vector3(Random.Range(dropX.x, dropX.y), 50, 0);
            rb.gravityScale = 1;
            inControl = false;
            curHp = HpValue;
            invulnerable = true;
            invulLightObj.SetActive(true);
            startWaitForInvul();
            if (recoverCor != null)
            {
                StopCoroutine(recoverCor);
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

    private void HandFall(bool fallKeyPressed)
    {
        if (fallKeyPressed && !isGrounded && inControl)
        {
            rb.gravityScale = quickFallDownScale;
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
        if (invulnerable)
            return;

        Debug.Log(name + "收到了"+n+"点伤害,朝向"+dir.x+", "+dir.y);
        curHp -= n;
        healthRecover = false;

        if (healthRecoverCor != null)
        {
            StopCoroutine(healthRecoverCor);
        }

        if (curHp <= 0)
        {
            Instantiate(generatorPrefab, transform.position, Quaternion.identity).GetComponent<gemGenerator>().GenInOnce(curScore);
            curScore = 0;
            scoreText.text = "0";
            GameObject gmo = Instantiate(corpse, transform.position, Quaternion.identity);
            gmo.GetComponent<SpriteRenderer>().color = deathColor;
            gmo.GetComponent<deathOnGroundScript>().ifLeft = isLeftSide;
            manager.resetWeapon();
            transform.position = new Vector3(Random.Range(dropX.x, dropX.y), 50, 0);
            rb.gravityScale = 1;
            invulnerable = true;
            invulLightObj.SetActive(true);
            startWaitForInvul();
            inControl = false;
            canMove = true;
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
            canMove = false;
            if (recoverCor != null)
            {
                StopCoroutine(recoverCor);
            }
            recoverCor = StartCoroutine(recoverFromNoControl(waitTimeToRecover));

            healthRecoverCor = StartCoroutine(waitForRecover(waitTimeToRecover));
            
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
        gmo.GetComponent<SpriteRenderer>().color = deathColor;
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
        canMove = true;
    }

    IEnumerator waitForRecover(float t)
    {
        yield return new WaitForSeconds(t);
        healthRecover = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<gemScript>() != null&& inControl&& collision.GetComponent<gemScript>().ifCollectable)
        {
            Destroy(collision.gameObject);
            curScore += 1;
            scoreText.text = curScore.ToString();
            txt.lightUp();
        }
    }

    IEnumerator invulRe()
    {
        yield return new WaitForSeconds(invulTime);
        invulnerable = false;
        invulLightObj.SetActive(false);
    }

    private void startWaitForInvul()
    {
        if (invulCor != null)
        {
            StopCoroutine(invulCor);
        }
        invulCor = StartCoroutine(invulRe());
    }
}
