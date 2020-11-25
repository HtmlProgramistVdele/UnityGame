using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSensor : MonoBehaviour
{
    private bool m_collided;
    private GameObject hitTarget;
    private float m_DisableTimer;

    private void OnEnable()
    {
        m_collided = false;
    }

    public bool State()
    {
        if (m_DisableTimer > 0)
            return false;
        return m_collided;
    }

    public void Attack()
    {
        if (m_collided)
        {
            if (hitTarget.CompareTag("Enemy"))
            {
                var target = hitTarget.gameObject.GetComponent<Bandit>();
                target.Hurt();
            }
            else if (hitTarget.CompareTag("Player"))
            {
               var target =  hitTarget.gameObject.GetComponent<HeroKnightCore>();
               
               target.Hurt(this.transform.parent.gameObject);
                
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((!other.gameObject.CompareTag("Enemy") && !other.gameObject.CompareTag("Player")) || other.isTrigger == true)
            return;
        
        hitTarget = other.gameObject;
        m_collided = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if ((!other.gameObject.CompareTag("Enemy") && !other.gameObject.CompareTag("Player"))|| other.isTrigger == true)
            return;
        m_collided = false;
    }


    void Update()
    {
        m_DisableTimer -= Time.deltaTime;
    }

    public void Disable(float duration)
    {
        m_DisableTimer = duration;
    }
}