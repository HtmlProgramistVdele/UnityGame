using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Bandit : MonoBehaviour
{
    [SerializeField] float m_speed = 2f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] int m_health = 3;

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private SensorHeroKnight m_groundSensor;
    private List<AudioSource> audioSources;
    private bool m_grounded = false;
    private bool m_combatIdle = false;
    private bool m_isDead = false;
    private int m_spriteFacing = 1;
    private Rigidbody2D enemyBody;
    private AttackSensor m_attackSensorL;
    private float m_attackCooldown = 0f;
    private bool m_attacked = false;

    private bool IsTriggered
    {
        get => m_isTriggered;
        set
        {
            m_xtarget = value ? 1f : 0f;
            m_isTriggered = value;
        }
    }

    private bool m_isTriggered;

    private float m_xtarget = 0f;

    // Use this for initialization
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        audioSources = GetComponents<AudioSource>().ToList();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<SensorHeroKnight>();
        m_attackSensorL = transform.Find("AttackSensor_L").GetComponent<AttackSensor>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_isDead)
            return;

        var spriteRend = GetComponent<SpriteRenderer>();
        if (m_attacked && spriteRend.sprite.name == "LightBandit_20")
        {
            var state = m_attackSensorL.State();

            if (state)
            {
                m_attackSensorL.Attack();

                m_attacked = false;
            }
        }

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        if (IsTriggered)
        {
            if (enemyBody.position.x > m_body2d.position.x && m_xtarget < 0)
            {
                m_xtarget *= -1;
            }
            else if (enemyBody.position.x < m_body2d.position.x && m_xtarget > 0)
            {
                m_xtarget *= -1;
            }
        }

        // Swap direction of sprite depending on walk direction
        if (m_xtarget > 0)
        {
            m_spriteFacing = 1;
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }
        else if (m_xtarget < 0)
        {
            m_spriteFacing = -1;
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }


        if ((m_attackSensorL.State()) && m_attackCooldown <= 0f)
        {
            Attack();

            m_attacked = true;
            m_attackCooldown = 0.57f;
            m_body2d.velocity = new Vector2(0, 0);
        }
        else if (!m_attackSensorL.State())
        {
            m_body2d.velocity = new Vector2(m_xtarget * m_speed, m_body2d.velocity.y);
        }

        if (m_attackCooldown > 0f)
        {
            m_attackCooldown -= Time.deltaTime;
        }

        // m_body2d.AddForce(transform.right * m_xtarget);
        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeed", m_body2d.velocity.y);

        if (Mathf.Abs(m_xtarget) > 0)
            m_animator.SetInteger("AnimState", 2);

        //Combat Idle
        else if (m_combatIdle)
            m_animator.SetInteger("AnimState", 1);

        //Idle
        else
            m_animator.SetInteger("AnimState", 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name != "HeroKnight")
            return;

        IsTriggered = true;
        enemyBody = GameObject.Find("HeroKnight").GetComponent<Rigidbody2D>();
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.collider.name != "HeroKnight")
            return;

        IsTriggered = false;
        enemyBody = null;
    }

    private void Jump()
    {
        m_animator.SetTrigger("Jump");
        m_grounded = false;
        m_animator.SetBool("Grounded", m_grounded);
        m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
        m_groundSensor.Disable(0.2f);
    }

    private void Attack()
    {
        audioSources.FirstOrDefault(x=>x.clip.name=="SwordAttack")?.Play();
        m_animator.SetTrigger("Attack");
    }

    public void Hurt()
    {
        if (m_isDead)
            return;

        m_animator.SetTrigger("Hurt");
        m_health--;

        if (m_health == 0)
            Death();
    }

    private void Death()
    {
        if (!m_isDead)
            m_animator.SetTrigger("Death");


        audioSources.FirstOrDefault(x=>x.clip.name=="Death").Play();
        this.GetComponent<BoxCollider2D>().enabled = false;
        m_body2d.bodyType = RigidbodyType2D.Static;
        m_isDead = !m_isDead;
    }

    public void Parry()
    {
        audioSources.FirstOrDefault(x=>x.clip.name=="Parry")?.Play();
        m_animator.SetTrigger("Hurt");
        m_attackCooldown = 2f;
    }
}