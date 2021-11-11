using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.Player
{
    public class InputManager : MonoBehaviour
    {
        // Player infos
        [SerializeField] float moveSpeed, rotationSpeed, jumpHeight, gravity;
        [SerializeField] bool isGrounded;
        [SerializeField] Vector3 velocity;
        
        //PlayerInput playerInput; // Input system actions
        [SerializeField] InputController controller; // For input system
        CharacterController characterController; // used to move player
        Animator anim;

        // Particle system
        [SerializeField] ParticleSystem particleSystem;
        [SerializeField] float rateFrom, rateTo, timeToChargedShoot;
        [SerializeField] bool chargedShootReady;

        // Input system variables
        Vector2 inputMove; // Input system use a 2d vector where z = y -> (x, y) = (x, 0, y)


        static public InputManager instance;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;

            //playerInput = GetComponent<PlayerInput>();
            controller = new InputController();
            characterController = GetComponent<CharacterController>();
            anim = GetComponent<Animator>();

            // Setting Input system actions
            controller.Player.Move.performed += ctx => inputMove = ctx.ReadValue<Vector2>();
            controller.Player.Move.canceled += ctx => inputMove = Vector2.zero;

            controller.Player.Jump.performed += ctx => Jump();

            controller.Player.NormalShoot.performed += ctx => NormalShoot();
            controller.Player.NormalShoot.canceled += ctx => StopNormalShooting();

            controller.Player.ChargedShoot.performed += ctx => StartCoroutine("StartChargedShootCR");
            controller.Player.ChargedShoot.canceled += ctx => ChargedShoot();

            // TO-DO: shooting and others
            chargedShootReady = false;
        }

        // Update is called once per frame
        void Update()
        {
            
            CheckIfGrounded();

            Move();

            // Let the player go down for gravity
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        void Move()
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
                anim.SetFloat("x", 1);
            else
                anim.SetFloat("x", 0);
        }

        void CheckIfGrounded()
        {
            // Simulate gravity on player
            isGrounded = characterController.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = 0;
                anim.SetBool("isJumping", false);
            }
        }

        void Jump()
        {
            if (isGrounded)
            {
                velocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);
                anim.SetBool("isJumping", true);
                anim.SetBool("isJumping", false);

            }
        }

        void NormalShoot()
        {
            // TODO: cambiare nome bool con "isNormalShooting"
            anim.SetBool("isShooting", true);
            GunManager.instance.NormalShoot();
        }

        void StopNormalShooting()
        {
            anim.SetBool("isShooting", false);
        }

        IEnumerator StartChargedShootCR()
        {
            Debug.Log("Starting charged shoot");
            particleSystem.Play();
            var t = 0f;
            while(t < timeToChargedShoot)
            {
                t += Time.deltaTime;
                var emission = particleSystem.emission;
                emission.rateOverTime = Mathf.Lerp(rateFrom, rateTo, t / timeToChargedShoot);
                Debug.Log("Charging: " + t);
                yield return null;
            }

            if(t >= timeToChargedShoot)
                chargedShootReady = true;
        }

        void ChargedShoot()
        {
            Debug.Log("Charged Shoot: " + chargedShootReady);
            if(chargedShootReady)
            {
                GunManager.instance.ChargedShoot();
            }
            else
            {
                Debug.Log("Stop charging");
                StopCoroutine("StartChargedShootCR");
            }
            particleSystem.Stop();
            chargedShootReady = false;
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