using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class HeroKnightCore : MonoBehaviour
{
    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float m_rollForce = 6.0f;
    [SerializeField] bool m_noBlood = false;
    [SerializeField] GameObject m_slideDust;

    private int m_health
    {
        get { return int.Parse(transform.Find("Canvas").transform.Find("HealthCount").GetComponent<Text>().text); }
        set { transform.Find("Canvas").transform.Find("HealthCount").GetComponent<Text>().text = value.ToString(); }
    }

    private int _health = 4;

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_HeroKnight m_groundSensor;
    private Sensor_HeroKnight m_wallSensorR1;
    private Sensor_HeroKnight m_wallSensorR2;
    private Sensor_HeroKnight m_wallSensorL1;
    private Sensor_HeroKnight m_wallSensorL2;
    private AttackSensor m_attackSensorL;
    private AttackSensor m_attackSensorR;
    private List<AudioSource> audioSources;
    private bool m_grounded = false;
    private bool m_rolling = false;
    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;
    private bool m_isDead = false;
    private bool m_isInBlock = false;


    // Use this for initialization
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        audioSources = GetComponents<AudioSource>().ToList();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
        m_attackSensorL = transform.Find("AttackSensor_L").GetComponent<AttackSensor>();
        m_attackSensorR = transform.Find("AttackSensor_R").GetComponent<AttackSensor>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_isDead)
        {
            if (Input.GetKeyDown("r"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
                return;
        }

        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

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

        // -- Handle input and movement --
        float inputX = Input.GetAxis("Horizontal");


        // Swap direction of sprite depending on walk direction
        if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }

        else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        // Move
        if (!m_rolling)
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        // -- Handle Animations --
        //Wall Slide
        m_animator.SetBool("WallSlide",
            (m_wallSensorR1.State() && m_wallSensorR2.State()) || (m_wallSensorL1.State() && m_wallSensorL2.State()));


        //Attack
        if (Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f)
        {
            m_currentAttack++;
            audioSources.FirstOrDefault(x => x.clip.name == "SwordAttack")?.Play();
            // Loop back to one after third attack
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            m_animator.SetTrigger("Attack" + m_currentAttack);
            if (m_facingDirection == 1)
            {
                if (m_attackSensorR.State())
                {
                    m_attackSensorR.Attack();
                }
            }
            else if (m_facingDirection == -1)
            {
                if (m_attackSensorL.State())
                {
                    m_attackSensorL.Attack();
                }
            }


            // Reset timer
            m_timeSinceAttack = 0.0f;
        }

        // Block
        else if (Input.GetMouseButtonDown(1))
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
            m_isInBlock = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            m_animator.SetBool("IdleBlock", false);
            m_isInBlock = false;
        }

        //Jump
        else if (Input.GetKeyDown("space") && m_grounded)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }
        else if (Input.GetKeyDown("space") && ((m_wallSensorR1.State() && m_wallSensorR2.State()) ||
                                               (m_wallSensorL1.State() && m_wallSensorL2.State())))

        {
            m_animator.SetTrigger("Jump");

            var sideJumpSide = m_wallSensorR1.State() && m_wallSensorR2.State() ? -10f : 10f;
            m_body2d.velocity = new Vector2(sideJumpSide, m_jumpForce);
            m_body2d.AddForce(transform.right * sideJumpSide);
            m_groundSensor.Disable(0.2f);
        }

        //Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }

        //Idle
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }
    }

    public void Hurt(GameObject attacker)
    {
        if (m_isDead)
            return;

        if (m_isInBlock)
        {
            var attakerClass = attacker.GetComponent<Bandit>();
            attakerClass.Parry();
            return;
        }

        m_animator.SetTrigger("Hurt");
        m_health = m_health - 1;

        if (m_health == 0)
            Death();
    }

    private void Death()
    {
        if (!m_isDead)
            m_animator.SetTrigger("Death");


        audioSources.FirstOrDefault(x => x.clip.name == "Death")?.Play();

        this.GetComponent<BoxCollider2D>().enabled = false;
        m_body2d.bodyType = RigidbodyType2D.Static;
        m_isDead = !m_isDead;
    }
    
    void AE_SlideDust()
    {
        Vector3 spawnPosition;

        if (m_facingDirection == 1)
            spawnPosition = m_wallSensorR2.transform.position;
        else
            spawnPosition = m_wallSensorL2.transform.position;

        if (m_slideDust != null)
        {
            // Set correct arrow spawn position
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            // Turn arrow in correct direction
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }
}