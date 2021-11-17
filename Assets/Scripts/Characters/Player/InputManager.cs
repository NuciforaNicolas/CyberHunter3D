using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.Player
{
    public class InputManager : MonoBehaviour
    {
        // Player infos
        [SerializeField] float moveSpeed, rotationSpeed, dashSpeed, dashDuration, timeToDash, jumpHeight, jumpMultiplier, wallJumpSpeed, wallFallingMultiplier, gravity, normalGravityMultiplier, gravityMultiplier;
        [SerializeField] float timeToEnableInput;
        bool isGrounded, canDash, isContactingWall, inputEnabled;
        Vector3 velocity;

        PlayerInput playerInput; // Input system actions
        [SerializeField] InputController controller; // For input system
        CharacterController characterController; // used to move player
        Animator anim;

        // Particle system
        [SerializeField] ParticleSystem charghingPS, fullChargePS, dashPS;
        [SerializeField] float chargeRateFrom, chargeRateTo, timeToChargedShoot;
        bool chargedShootReady;

        // Input system variables
        Vector2 inputMove; // Input system use a 2d vector where z = y -> (x, y) = (x, 0, y)

        static public InputManager instance;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;

            playerInput = GetComponent<PlayerInput>();
            controller = new InputController();
            characterController = GetComponent<CharacterController>();
            anim = GetComponent<Animator>();

            inputEnabled = true;

            // Setting Input system actions
            controller.Player.Move.performed += ctx => inputMove = ctx.ReadValue<Vector2>();
            controller.Player.Move.canceled += ctx => inputMove = Vector2.zero;

            controller.Player.Jump.performed += ctx => { if(isGrounded) Jump(); };

            controller.Player.NormalShoot.performed += ctx => NormalShoot();
            controller.Player.NormalShoot.canceled += ctx => StopNormalShooting();

            controller.Player.ChargedShoot.performed += ctx => StartCoroutine("StartChargedShootCR");
            controller.Player.ChargedShoot.canceled += ctx => ChargedShoot();

            controller.Player.Dash.performed += ctx => { if (isGrounded && canDash) Dash(); };

            // TO-DO: shooting and others
            chargedShootReady = false;
            canDash = true;
        }

        // Update is called once per frame
        void Update()
        {
            
            CheckIfGrounded();

            if (characterController.collisionFlags == CollisionFlags.None)
                isContactingWall = false;

            Move();

            ApplayGravity();
        }

        void ApplayGravity()
        {
            if(isContactingWall && velocity.y < 0)
            {
                gravityMultiplier = wallFallingMultiplier;
            }
            else
            {
                gravityMultiplier = normalGravityMultiplier;
            }
            
            // Let the player go down for gravity
            velocity.y -= gravity * gravityMultiplier * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        void Move()
        {
            if(inputEnabled)
            {
                var move = new Vector3(0, 0, inputMove.x);
                var forward = move * moveSpeed * Time.deltaTime;
                characterController.Move(forward);

                if (move != Vector3.zero)
                {
                    Quaternion rot = Quaternion.LookRotation(move, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, rotationSpeed * Time.deltaTime);
                }


                if (inputMove.magnitude > 0)
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
            isContactingWall = false;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = 0;
                if(anim != null)
                    anim.SetBool("isJumping", false);
            }
        }

        void Jump()
        {
            velocity.y = Mathf.Sqrt(jumpHeight * jumpMultiplier * gravity);
            if(anim != null)
            {
                anim.SetBool("isJumping", true);
                anim.SetBool("isJumping", false);
            }
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
            dashPS.Play();
            canDash = false;
            float t = 0f;
            float dir = transform.forward.z > 0 ? 1 : -1;
            while (t < dashDuration)
            {
                t += Time.deltaTime;
                velocity.z = dashSpeed * dir;
                yield return null;
            }
            dashPS.Stop();

            t = 0f;
            float finalVel = velocity.z;
            while (t < dashDuration)
            {
                var lerpTime = t / dashDuration;
                velocity.z = Mathf.Lerp(finalVel, 0, lerpTime);
                t += Time.deltaTime;
                yield return null;
            }
            velocity.z = 0;
            
            yield return new WaitForSeconds(timeToDash);
            canDash = true;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            //if (isGrounded || !hit.collider.CompareTag("wall")) return;
            if(!isGrounded && hit.normal.y < 0.1f)
            {
                isContactingWall = true;
                if (playerInput.actions["Jump"].triggered)
                {
                    StartCoroutine("WallJump", hit.normal.z);
                    
                }
            }
        }

        IEnumerator WallJump(float hitNormal)
        {
            Debug.Log("Walljump");
            inputEnabled = false;
            Jump();
            velocity.z = hitNormal * wallJumpSpeed;
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