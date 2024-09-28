using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseHuman : MonoBehaviour
{
    /* 
     * Protected internal access modifier is used to specify 
     * that access is limited to the current assembly or types
     * derived from the containing class
     */

    // is the character launching an attack 
    protected internal bool isAttacking = false;
    protected internal float attackTime = float.MinValue;
    // prevent attack animation frames from getting stuck
    protected internal bool DisableAttackOnce { get; set; } = false;

    // is the character moving
    protected internal bool isMoving = false;
    // the character position
    private Vector3 targetPosition;
    // moving speed
    public float speed = 0.3f;
    // Animator component
    protected Animator animator;
    // Desciption
    public string description = "";

    // variable used for managing HP 
    internal Slider HP1F;
    internal Slider HP2F;
    internal Text text1;
    internal Text text2;

    private bool isDecreasingForHP2F = false;
    internal float preHP = 100;
    internal int currentHP = 100;

    // for DamageHP prefab
    private GameObject damageHP;

    // dead or alive
    internal bool isDead = false;

    // Attack action
    public void Attack()
    {
        isAttacking = true;
        attackTime = Time.time ;
        animator.SetBool("isAttacking", true);
    }

    // Moving to somewhere
    public void MoveTo(Vector3 pos)
    {
        targetPosition = pos;
        isMoving = true;
        animator.SetBool("isMoving", true);
    }

    // Attack Update
    public void AttackUpdate()
    {
        if (!isAttacking) return;
        // attack cooling time is 1.2 second
        if ((Time.time - attackTime) < 1.2f) return;
        isAttacking = false;
        animator.SetBool("isAttacking", false);
        DisableAttackOnce = true;
    }

    // Move Update
    public void MoveUpdate()
    {
        if (!isMoving ) return;
        Vector3 pos = transform.position;
        transform.position = Vector3.MoveTowards(pos, targetPosition, 
            speed * Time.deltaTime);
        transform.LookAt(targetPosition);
        if (Vector3.Distance(pos, targetPosition) <= 0.5f)
        {
            isMoving = false;
            animator.SetBool("isMoving", false);
        }
    }

    public void SetHP(int HP, bool isDamageImplemented, float attackAngleY)
    {
        int damage = currentHP - HP;
        if (HP < 0) HP = 0;
        currentHP = HP;
        HP1F.value = HP / 100.0f;
        text2.text = HP + " /100";
        isDecreasingForHP2F = true;
        // conditionally execute DamageHP effect according to the value
        // of isDamageImplemented 
        if (isDamageImplemented)
        {
            // Quaternion.euler (0,attackAngleY,0) * danmageDirection 
            // represents the result of attackAngleY degree rotation of vector 
            // danmageDirection around the y axis
            Vector3 danmageDirection = new Vector3(0, 6.5f, damage/6.0f);
            danmageDirection = 
                Quaternion.Euler(0, attackAngleY, 0) * danmageDirection;

            GameObject damageHPGo = Instantiate(damageHP, transform.position +
                new Vector3(0, 5, 0), HP1F.transform.rotation);

            damageHPGo.GetComponent<TextMesh>().text = $"{damage}";

            damageHPGo.GetComponent<TextMesh>().characterSize = damage / 10.0f;
            damageHPGo.GetComponent<DamageHP>().SetMove(danmageDirection);
        }
    }

    private void HPUpdate()
    {
        if (isDecreasingForHP2F)
        {
            HP2F.value = preHP / 100.0f;
            preHP -= 20f * Time.deltaTime;
            if (preHP < currentHP)
            {
                preHP = currentHP;
                HP2F.value = currentHP / 100.0f;
                isDecreasingForHP2F = false;
                if (currentHP <= 0)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    // Use this for initialization
    protected void Start()
    {
        animator = GetComponent<Animator>();
        GameObject Canvas = transform.Find("Canvas").gameObject;
        HP1F = Canvas.transform.Find("HP1F").GetComponent<Slider>();
        HP2F = Canvas.transform.Find("HP2F").GetComponent<Slider>();
        text1 = Canvas.transform.Find("Text1").GetComponent<Text>();
        text2 = Canvas.transform.Find("Text2").GetComponent<Text>();

        if (this.name == "EthanCtr")
        {
            HP1F.transform.Find("Fill Area/Fill").GetComponent<Image>().color =
                Color.green;
            text1.text = "EthanCtr";
        }
        // initialize the DamageHP prefab
        damageHP = (GameObject)Resources.Load("Prefabs/3DText");
    }

    // Update is called once per frame
    protected void Update()
    {
        MoveUpdate();
        AttackUpdate();
        HPUpdate();
    }
}
