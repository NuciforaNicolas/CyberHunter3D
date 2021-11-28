using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.Player
{
    public class Player : Character
    {
        // Player infos
        [SerializeField] float dashSpeed, dashDuration, wallJumpSpeed, wallSlideSpeed;
        [SerializeField] float timeToEnableInput;
        public bool canDash { get; private set; } 
        public bool isContactingWall { get; private set; } 
        public bool inputEnabled { get; private set; } 
        public bool canDoubleJump { get; private set; }
        
        public float wallNormal { get; private set; }

        // Particle system
        [SerializeField] ParticleSystem charghingPS, fullChargePS, dashPS;
        [SerializeField] float chargeRateFrom, chargeRateTo, timeToChargedShoot;
        bool chargedShootReady;

        // Input system variables
        public Vector2 inputDirection { get; set; } // Input system use a 2d vector where z = y -> (x, y) = (x, 0, y)

        private void Awake()
        {
            base.Awake();
            canDoubleJump = true;
            inputEnabled = true;
            chargedShootReady = false;
            canDash = true;
        }

        void Update()
        {

            CheckIfGrounded();

            if (characterController.collisionFlags == CollisionFlags.None)
            {
                isContactingWall = false;
            }

            Move();

            if (isContactingWall)
                WallSlide();
            else
                ApplyGravity();
        }

        void Move()
        {
            if (inputEnabled)
            {
                var inputDir = new Vector3(0, 0, inputDirection.x > 0 ? 1 : inputDirection.x < 0 ? -1 : 0);
                moveDir = inputDir * moveSpeed * Time.deltaTime;
                characterController.Move(moveDir);

                if (inputDir != Vector3.zero)
                {
                    Quaternion rot = Quaternion.LookRotation(inputDir, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, rotationSpeed * Time.deltaTime);
                }


                if (inputDirection.magnitude > 0)
                {
                    if (anim != null)
                        anim.SetFloat("x", 1);
                }
                else
                {
                    if (anim != null)
                        anim.SetFloat("x", 0);
                }
            }
        }

        void ApplyGravity()
        {
            gravityMultiplier = normalGravityMultiplier;
            // Let the player go down for gravity
            velocity.y -= gravity * gravityMultiplier * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        void WallSlide()
        {
            characterController.Move(-Vector3.up * wallSlideSpeed * Time.deltaTime);
        }

        void CheckIfGrounded()
        {
            // Simulate gravity on player
            isGrounded = characterController.isGrounded;
            if (isGrounded)
            {
                isContactingWall = false;
                canJump = true;
                canDoubleJump = true;
                if (velocity.y < 0)
                    velocity.y = 0;
                if (anim != null)
                    anim.SetBool("isJumping", false);
            }
        }

        public void Jump()
        {
            canJump = false;
            velocity.y = Mathf.Sqrt(jumpHeight * jumpMultiplier * gravity);
            if (anim != null)
            {
                anim.SetBool("isJumping", true);
                anim.SetBool("isJumping", false);
            }
        }

        public void DoubleJump()
        {
            canDoubleJump = false;
            Jump();
        }

        public void NormalShoot()
        {
            // TODO: cambiare nome bool con "isNormalShooting"
            if (anim != null)
                anim.SetBool("isShooting", true);
            GunManager.instance.NormalShoot();
        }

        public void StartChargedShoot()
        {
            StartCoroutine("StartChargedShootCR");
        }

        IEnumerator StartChargedShootCR()
        {
            charghingPS.Play();
            var t = 0f;
            while (t < timeToChargedShoot)
            {
                t += Time.deltaTime;
                var emission = charghingPS.emission;
                emission.rateOverTime = Mathf.Lerp(chargeRateFrom, chargeRateTo, t / timeToChargedShoot);
                yield return null;
            }

            if (t >= timeToChargedShoot)
            {
                chargedShootReady = true;
                //charghingPS.Stop();
                fullChargePS.Play();
            }

        }

        public void ChargedShoot()
        {
            if (chargedShootReady)
            {
                fullChargePS.Stop();
                GunManager.instance.ChargedShoot();
            }
            else
            {
                StopCoroutine("StartChargedShootCR");
                NormalShoot();
            }
            charghingPS.Stop();
            chargedShootReady = false;
        }

        public void Dash()
        {
            StartCoroutine("DashCR");
        }

        IEnumerator DashCR()
        {
            //dashPS.Play();
            canDash = false;
            float t = 0f;
            float dir = transform.forward.z > 0 ? 1 : -1;
            while (t < dashDuration || !isGrounded)
            {
                if (isContactingWall) break;

                t += Time.deltaTime;
                //velocity.z = dashSpeed * dir;
                characterController.Move(new Vector3(0, 0, inputDirection.x > 0 ? 1 : inputDirection.x < 0 ? -1 : 0) * dashSpeed * Time.deltaTime);
                yield return null;
            }
            dashPS.Stop();
            velocity.z = 0;
            canDash = true;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!isGrounded && hit.normal.y < 0.1f)
            {
                isContactingWall = true;
                wallNormal = hit.normal.z;
                //if (playerInput.actions["Jump"].triggered)
                //{
                //    StartCoroutine("WallJump", hit.normal.z);

                //}
            }
        }

        public void WallJump()
        {
            StartCoroutine("WallJumpCR", wallNormal);
        }

        IEnumerator WallJumpCR(float hitNormal)
        {
            Debug.Log("Walljump");
            inputEnabled = false;
            Jump();
            velocity.z = hitNormal * wallJumpSpeed;
            characterController.Move(velocity * Time.deltaTime);
            yield return new WaitForSeconds(timeToEnableInput);
            inputEnabled = true;
            velocity.z = 0;
        }
    }
}
