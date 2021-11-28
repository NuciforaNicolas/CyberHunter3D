using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    public class Character : MonoBehaviour
    {
        [SerializeField]
        protected float moveSpeed, rotationSpeed, jumpSpeed, jumpMultiplier, gravity, gravityMultiplier;
        [SerializeField]
        public bool isGrounded { get; set; }
        [SerializeField]
        public bool canJump {get; set;}

        protected Vector3 velocity, moveDir;

        protected Animator anim;

        protected CharacterController characterController; // used to move player

        protected void Awake()
        {
            characterController = GetComponent<CharacterController>();
            anim = GetComponent<Animator>();

            canJump = true;

        }

        public void Hit(float damage)
        {
            
        }

        public void Die()
        {
            
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
            if (anim != null)
                anim.SetBool("isShooting", false);
        }
    }
}