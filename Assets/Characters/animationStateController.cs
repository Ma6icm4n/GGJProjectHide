using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkTransform))]
public class animationStateController : NetworkBehaviour
{
    public Animator animator;
    int velocityHash;
    float velocity;

    public GameObject characterContainer;
    public GameObject character;

    public float acceleration = 2f;
    public float deceleration = 5f;
    public float velocityMultiplier = 0.4f;

    // Start is called before the first frame update
    void Start()
    {
        animator = character.GetComponent<Animator>();
        velocityHash = Animator.StringToHash("Velocity");
    }

    public override void OnStartLocalPlayer()
    {
        Camera.main.orthographic = false;
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = new Vector3(0f, 3f, -8f);
        Camera.main.transform.localEulerAngles = new Vector3(10f, 0f, 0f);
    }

    void OnDisable()
    {
        if (isLocalPlayer && Camera.main != null)
        {
            Camera.main.orthographic = true;
            Camera.main.transform.SetParent(null);
            Camera.main.transform.localPosition = new Vector3(0f, 70f, 0f);
            Camera.main.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasAuthority) { return; }

        ProcessInputs();

        // The sprite is always facing the camera
        characterContainer.transform.LookAt(Camera.main.transform);
    }

    void ProcessInputs() {
        bool leftPressed = Input.GetKey(KeyCode.LeftArrow);
        bool rightPressed = Input.GetKey(KeyCode.RightArrow);
        bool upPressed = Input.GetKey(KeyCode.UpArrow);
        bool downPressed = Input.GetKey(KeyCode.DownArrow);

        if (leftPressed || rightPressed || upPressed || downPressed) {
            if (velocity < 1) {
              velocity += Time.deltaTime * acceleration;
            }

            // Initialize direction
            Vector3 direction = Vector3.zero;

            // Check for key pressed
            if (leftPressed) {
                // Unflip the sprite
                characterContainer.transform.localScale = new Vector3(-1, 1, 1);

                direction += Vector3.left;
            }

            if (rightPressed) {
                // Flip the sprite
                characterContainer.transform.localScale = Vector3.one;

                direction += Vector3.right;
            }

            if (upPressed) {
                direction += Vector3.forward;
            }

            if (downPressed) {
                direction += Vector3.back;
            }

            // Move the player in that direction
            moveCharacter(direction.normalized);
            animator.SetBool("isWalking", true);
        }
        else if (velocity > 0) { // Otherwise decrease the velocity
            velocity -= Time.deltaTime * deceleration;
            animator.SetBool("isWalking", false);
        }

        


        // Update the blending parameter
    }


    [ClientRpc]
    void updateVelocityBlend() {
        animator.SetFloat(velocityHash, velocity);
    }

    void moveCharacter(Vector3 direction) {
        transform.Translate(direction * velocity * velocityMultiplier);
    }
}
