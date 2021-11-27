using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.Player
{
    public class InputManager : MonoBehaviour
    {
        // Player infos
        [SerializeField] float moveSpeed, rotationSpeed, dashSpeed, dashDuration, timeToDash, jumpHeight, jumpMultiplier, wallJumpSpeed, wallSlideSpeed, gravity, normalGravityMultiplier, gravityMultiplier;
        [SerializeField] float timeToEnableInput;
        [SerializeField] bool isGrounded, canDash, isContactingWall, inputEnabled, canJump, canDoubleJump;
        [SerializeField] Vector3 velocity, moveDir;
        float wallNormal;

        PlayerInput playerInput; // Input system actions
        [SerializeField] InputController controller; // For input system
        CharacterController characterController; // used to move player
        Animator anim;

        // Particle system
        [SerializeField] ParticleSystem charghingPS, fullChargePS, dashPS;
        [SerializeField] float chargeRateFrom, chargeRateTo, timeToChargedShoot;
        bool chargedShootReady;

        // Input system variables
        Vector2 inputDirection; // Input system use a 2d vector where z = y -> (x, y) = (x, 0, y)

        static public InputManager instance;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;

            playerInput = GetComponent<PlayerInput>();
            controller = new InputController();
            characterController = GetComponent<CharacterController>();
            anim = GetComponent<Animator>();

            // Setting Input system actions
            controller.Player.Move.performed += ctx => inputDirection = ctx.ReadValue<Vector2>();
            controller.Player.Move.canceled += ctx => inputDirection = Vector2.zero;

            controller.Player.Jump.performed += ctx => {
                if (isGrounded && canJump && !isContactingWall)
                    Jump();
                else if (!isGrounded && canDoubleJump && !isContactingWall)
                    DoubleJump();
                else if(isContactingWall)
                {
                    StartCoroutine("WallJump", wallNormal);
                }
            };

            controller.Player.NormalShoot.performed += ctx => NormalShoot();
            controller.Player.NormalShoot.canceled += ctx => StopNormalShooting();

            controller.Player.ChargedShoot.performed += ctx => StartCoroutine("StartChargedShootCR");
            controller.Player.ChargedShoot.canceled += ctx => ChargedShoot();

            controller.Player.Dash.performed += ctx => { if (isGrounded && canDash) Dash(); };

            // TO-DO: shooting and others
            chargedShootReady = false;
            canDash = true;
            canJump = true;
            canDoubleJump = true;
            inputEnabled = true;
        }

        // Update is called once per frame
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
                ApplayGravity();
        }

        void ApplayGravity()
        {
            //if(isContactingWall && velocity.y < 0)
            //{
            //    gravityMultiplier = wallSlideSpeed;
            //}
            //else
            //{
            //    gravityMultiplier = normalGravityMultiplier;
            //}

            gravityMultiplier = normalGravityMultiplier;
            // Let the player go down for gravity
            velocity.y -= gravity * gravityMultiplier * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        void WallSlide()
        {
            characterController.Move(-Vector3.up * wallSlideSpeed * Time.deltaTime);
        }

        void Move()
        {
            if(inputEnabled)
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

        void CheckIfGrounded()
        {
            // Simulate gravity on player
            isGrounded = characterController.isGrounded;
            if (isGrounded)
            {
                isContactingWall = false;
                canJump = true;
                canDoubleJump = true;
                if(velocity.y < 0)
                    velocity.y = 0;
                if(anim != null)
                    anim.SetBool("isJumping", false);
            }
        }

        void Jump()
        {
            canJump = false;
            velocity.y = Mathf.Sqrt(jumpHeight * jumpMultiplier * gravity);
            if (anim != null)
            {
                anim.SetBool("isJumping", true);
                anim.SetBool("isJumping", false);
            }
        }

        void DoubleJump()
        {
            canDoubleJump = false;
            Jump();
        }

        void NormalShoot()
        {
            // TODO: cambiare nome bool con "isNormalShooting"
            if(anim != null)
                anim.SetBool("isShooting", true);
            GunManager.instance.NormalShoot();
        }

        void StopNormalShooting()
        {
            if(anim != null)
                anim.SetBool("isShooting", false);
        }

        IEnumerator StartChargedShootCR()
        {
            charghingPS.Play();
            var t = 0f;
            while(t < timeToChargedShoot)
            {
                t += Time.deltaTime;
                var emission = charghingPS.emission;
                emission.rateOverTime = Mathf.Lerp(chargeRateFrom, chargeRateTo, t / timeToChargedShoot);
                yield return null;
            }

            if(t >= timeToChargedShoot)
            {
                chargedShootReady = true;
                //charghingPS.Stop();
                fullChargePS.Play();
            }
                
        }

        void ChargedShoot()
        {
            if(chargedShootReady)
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

        void Dash()
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
            if(!isGrounded && hit.normal.y < 0.1f)
            {
                isContactingWall = true;
                wallNormal = hit.normal.z;
                //if (playerInput.actions["Jump"].triggered)
                //{
                //    StartCoroutine("WallJump", hit.normal.z);

                //}
            }
        }

        IEnumerator WallJump(float hitNormal)
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

        private void OnEnable()
        {
            controller.Enable();
        }

        private void OnDisable()
        {
            controller.Disable();
        }

    }

}