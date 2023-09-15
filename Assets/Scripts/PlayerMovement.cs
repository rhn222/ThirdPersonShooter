using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [HideInInspector]
    public CharacterController mCharacterController;
    public Animator mAnimator;  
    public float mWalkSpeed = 1.5f;
    public float mRunSpeed = 3.0f;
    public float mRotationSpeed = 50.0f;
    public float mGravity = -30.0f;
    public float mJumpHeight = 3.0f;
    [Tooltip("Only useful with Follow and Independent " + "Rotation - third - person camera control")]
    public bool mFollowCameraForward = false;
    public float mTurnRate = 200.0f;
    private Vector3 mVelocity = new Vector3(0.0f, 0.0f, 0.0f);  
    
    // Start is called before the first frame update
    void Start()
    {
      mCharacterController = GetComponent<CharacterController>();
    }   
    
    void Update()
    {
      //Move();
    }

    public void Move()
    {
#if UNITY_STANDALONE
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
#endif
#if UNITY_ANDROID
        float h = mJoystick.Horizontal;
        float v = mJoystick.Vertical;
#endif
#if UNITY_STANDALONE
        float speed = mWalkSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = mRunSpeed;
        }
#endif
#if UNITY_ANDROID
        float speed = mRunSpeed;
#endif
        if (mFollowCameraForward)
        {
            // Only allow aligning of player's direction when there is a movement.
            if (v > 0.1 || v < -0.1 || h > 0.1 || h < -0.1)
            {
                // rotate player towards the camera forward.
                Vector3 eu = Camera.main.transform.rotation.eulerAngles;
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    Quaternion.Euler(0.0f, eu.y, 0.0f),
                    mTurnRate * Time.deltaTime);
            }
        }
        else
        {
            transform.Rotate(0.0f, h * mRotationSpeed * Time.deltaTime, 0.0f);
        }
        mCharacterController.Move(transform.forward * v * speed * Time.deltaTime);
        // Move forward / backward
        Vector3 forward = transform.TransformDirection(Vector3.forward).normalized;
        forward.y = 0.0f;
        Vector3 right = transform.TransformDirection(Vector3.right).normalized;
        right.y = 0.0f;
        if (mAnimator != null)
        {
            if (mFollowCameraForward)
            {
                mCharacterController.Move(forward * v * speed * Time.deltaTime + right * h * speed * Time.deltaTime);
                mAnimator.SetFloat("PosX", h * speed / mRunSpeed);
                mAnimator.SetFloat("PosZ", v * speed / mRunSpeed);
            }
            else
            {
                mCharacterController.Move(forward * v * speed * Time.deltaTime);
                mAnimator.SetFloat("PosX", 0);
                mAnimator.SetFloat("PosZ", v * speed / mRunSpeed);
            }
        }
        // apply gravity.
        mVelocity.y += mGravity * Time.deltaTime;
        mCharacterController.Move(mVelocity * Time.deltaTime);
        if (mCharacterController.isGrounded && mVelocity.y < 0)
            mVelocity.y = 0f;
        if (Input.GetKey(KeyCode.Space))
        {
            Jump();
        }
    }

    void Jump()
    {
        if (mCharacterController.isGrounded)
        {
            mAnimator.SetTrigger("Jump");
            mVelocity.y += Mathf.Sqrt(mJumpHeight * -2f * mGravity);
        }
    }

}
