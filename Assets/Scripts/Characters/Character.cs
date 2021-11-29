using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
    public class Character : MonoBehaviour
    {
        [Header("Character Attributes")]
        [Header("Movement attributes")]
        [SerializeField] protected float moveSpeed;
        [SerializeField] protected float rotationSpeed;
        [Header("Jump attributes")]
        [SerializeField] protected float jumpSpeed;
        [SerializeField] protected float jumpMultiplier;
        [Header("Gravity attributes")]
        [SerializeField] protected float gravity;
        [SerializeField] protected float gravityMultiplier;
        [Header("Health")]
        [SerializeField] protected float maxHealth;
        [Header("UI")]
        [SerializeField] protected Image healthBar;
        [SerializeField] protected Color redHealthColor;
        [SerializeField] protected Color greenHealthColor;

        // Booleans
        public bool isGrounded { get; set; }
        public bool canJump {get; set;}

        //Protected attributes
        public float health { get; protected set; }
        protected Vector3 velocity, moveDir;
        protected Animator anim;
        protected CharacterController characterController; // used to move player

        protected void Awake()
        {
            characterController = GetComponent<CharacterController>();
            anim = GetComponent<Animator>();

            canJump = true;

        }

        protected virtual void Start()
        {
            health = maxHealth;
        }

        public virtual void Hit(float amount)
        {
            health = ((health - amount) > 0f) ? health - amount : 0f;
            var fillAmount = health / maxHealth;
            healthBar.fillAmount = fillAmount;
            healthBar.color = Color.Lerp(greenHealthColor, redHealthColor, 1 - fillAmount);
            if (health <= 0f)
                Die();
        }

        public virtual void Die()
        {
            Debug.Log("" + gameObject.name + " is dead");
        }

        protected virtual void Move()
        {
            
        }

        protected virtual void ApplayGravity()
        {
            
        }

        public virtual void Jump()
        {

        }

        public virtual void NormalShoot()
        {
            
        }

        public virtual void StopNormalShooting()
        {
            
        }

        public float GetMaxHealth()
        {
            return maxHealth;
        }
    }
}