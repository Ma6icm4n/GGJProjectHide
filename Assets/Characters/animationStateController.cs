using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateController : MonoBehaviour
{
    Animator animator;
    int velocityHash;
    float velocity;

    public GameObject characterContainer;
    public GameObject character;
    public Camera cameraToLookAt;

    public float acceleration = 2f;
    public float deceleration = 5f;
    public float velocityMultiplier = 0.4f;

    // Start is called before the first frame update
    void Start()
    {
        animator = character.GetComponent<Animator>();
        velocityHash = Animator.StringToHash("Velocity");
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInputs();

        // The sprite is always facing the camera
        characterContainer.transform.LookAt(cameraToLookAt.transform);
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

        }
        else if (velocity > 0) { // Otherwise decrease the velocity
            velocity -= Time.deltaTime * deceleration;
        }

        // Update the blending parameter
        updateVelocityBlend();
    }

    void updateVelocityBlend() {
        animator.SetFloat(velocityHash, velocity);
    }

    void moveCharacter(Vector3 direction) {
        transform.Translate(direction * velocity * velocityMultiplier);
    }
}
