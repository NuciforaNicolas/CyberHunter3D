using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.Player
{
    public class InputManager : MonoBehaviour
    {
        PlayerInput playerInput; // Input system actions
        [SerializeField] InputController controller; // For input system

        [SerializeField] Player player;

        // Start is called before the first frame update
        void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            controller = new InputController();

            // Setting Input system actions
            controller.Player.Move.performed += ctx => player.inputDirection = ctx.ReadValue<Vector2>();
            controller.Player.Move.canceled += ctx => player.inputDirection = Vector2.zero;

            controller.Player.Jump.performed += ctx => {
                if (player.isGrounded && player.canJump && !player.isContactingWall)
                    player.Jump();
                else if (!player.isGrounded && player.canDoubleJump && !player.isContactingWall)
                    player.DoubleJump();
                else if(player.isContactingWall)
                {
                    player.WallJump();
                }
            };

            controller.Player.NormalShoot.performed += ctx => player.NormalShoot();
            controller.Player.NormalShoot.canceled += ctx => player.StopNormalShooting();

            controller.Player.ChargedShoot.performed += ctx => player.StartChargedShoot();
            controller.Player.ChargedShoot.canceled += ctx => player.ChargedShoot();

            controller.Player.Dash.performed += ctx => { if (player.isGrounded && player.canDash) player.Dash(); };
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