using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMotor : MonoBehaviour {

    public CharacterStates myState = CharacterStates.walking;

    public bool drawGizmo = false;

    public Transform mesh;

    [Header("Movement")]
    public float walkSpeed = 15;
    public float walkAcceleration = 0.3f;
    public float walkDeacceleration = 0.3f;
    public float jumpPower = 25;
    public float rotateSpeedStationary = 0.5f;
    public float rotateSpeedMoving = 0.1f;
    public float maxAngle = 45;
    public float maxStep = 0.2f;

    [Header("Check If Grounded")]
    public bool grounded;
    public float groundCheckDistance = 0.2f;
    private static float saved_groundCheckDistance = 0.2f;
    public float groundCheckSize = 0.5f;
    RaycastHit groundHit;

    [Header("Components")]
    public CapsuleCollider myCollider;
    public Rigidbody myRigidbody;
    public CharacterCamera myCharCamera;
    public PhysicMaterial myPhysicMat;

    private Vector2 moveInput = Vector2.zero;

    private float jumpTime = 0;
    private bool jumpInput = false;

    private void Start()
    {
        saved_groundCheckDistance = groundCheckDistance;
    }

 
    void Update () {


        if (Input.GetButtonDown("Jump"))
            jumpInput = true;

        CheckIfGrounded();
    }
    void FixedUpdate()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (grounded)
        {
            MoveOnGround(moveInput, jumpInput);
        }
        else
        {

        }

        jumpInput = false;
    }


    void MoveOnGround(Vector3 direction, bool jump)
    {
        // Direction to local
        direction = myCharCamera.followForward * direction.y + myCharCamera.followRight * direction.x;
        if (direction.magnitude > 1)
            direction.Normalize();

        // Rotate capsule smoothly
        RotatePlayerToDir(direction.normalized, Mathf.Lerp(rotateSpeedStationary, rotateSpeedMoving, myRigidbody.velocity.magnitude / walkSpeed));
        
        //Rotate mesh to velocity
        Vector3 meshDir = new Vector3(myRigidbody.velocity.x, 0, myRigidbody.velocity.z).normalized;
        if (meshDir.magnitude > 0)
            mesh.rotation = Quaternion.LookRotation(meshDir, Vector3.up);

        //Physics
        if (direction.magnitude == 0)//Standing- deaccelerating
        {
            // Calculate AntyForce
            Vector3 antyForce = -myRigidbody.velocity.normalized * walkDeacceleration;
            if (antyForce.magnitude <= walkDeacceleration)
                antyForce = Vector3.zero;
            // Apply force
            myRigidbody.velocity = antyForce;
        }
        else//Moving- accelerating
        {
            // Calculate Force
            Vector3 force = Vector3.ProjectOnPlane(direction.normalized, groundHit.normal) * walkAcceleration;
            force = Vector3.ClampMagnitude(force + new Vector3(myRigidbody.velocity.x, 0, myRigidbody.velocity.z), walkSpeed);
            // Apply force
            myRigidbody.velocity = force + Vector3.up * myRigidbody.velocity.y;
        }

        // Standard Jumping
        if (jump)
        {
            // Rotate capsule to jump Dir
            RotatePlayerToDir(direction.normalized, 1);

            grounded = false;
            groundCheckDistance = 0.01f;

            // Apply jump Force
            myRigidbody.AddForce(Vector3.up * jumpPower + direction.normalized * jumpPower / 5, ForceMode.Impulse); 
        }

        // Cancel Gravity
        myRigidbody.AddForce(-Physics.gravity, ForceMode.Force);
    } 

    void Jumping(bool jump)
    {
        if(grounded)
        {
            if (jump)
            {
                grounded = false;
                groundCheckDistance = 0.01f;
                myRigidbody.AddForce(Vector3.up*jumpPower + transform.forward * jumpPower/5, ForceMode.Impulse); // Apply jump Force
            }
        }
    }

    void RotatePlayerToDir(Vector3 dir, float turnSpeed = 1)
    {
        if (dir.magnitude != 0)
        {
            float turnAmount = Vector3.Angle(transform.forward, dir);
            if (Vector3.Cross(transform.forward, dir).y < 0)
                turnAmount = -turnAmount;
            transform.Rotate(new Vector3(0, turnAmount * turnSpeed, 0));
        }
    }

    private static float groundedCastOffset = 0.05f;
    void CheckIfGrounded()
    {
        // If falling reset check Distance
        if (myRigidbody.velocity.y < 0)
            groundCheckDistance = saved_groundCheckDistance;

        float sphereRadius = myCollider.radius * groundCheckSize;
        if (Physics.SphereCast(transform.position + Vector3.up*(sphereRadius + groundedCastOffset), sphereRadius, Vector3.down, out groundHit, groundCheckDistance + groundedCastOffset))
        {
            if (grounded == false)
            {
                //transform.position = groundHit.point;
                myRigidbody.velocity = new Vector3(myRigidbody.velocity.x, 0 , myRigidbody.velocity.z);// zapobiega wejściu colliderem w ziemie i podskoczeniu postaci;
                grounded = true;
            }
        }
        else
        {
            grounded = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (drawGizmo)
        {
            if (grounded)
            {
                Gizmos.color = new Color(0, 1, 0, 0.6f);
                Gizmos.DrawSphere(groundHit.point, myCollider.radius * groundCheckSize);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + Vector3.ProjectOnPlane(transform.forward, groundHit.normal));
            }
            else
            {
                float sphereRadius = myCollider.radius * groundCheckSize;
                Vector3 startPos = transform.position + Vector3.up * (sphereRadius + groundedCastOffset);
                Gizmos.color = new Color(1, 1, 1, 0.8f);
                Gizmos.DrawSphere(startPos, myCollider.radius * groundCheckSize);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(startPos, transform.position + Vector3.up * - groundCheckDistance);
            }
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + myRigidbody.velocity);
        }
    }

}

public enum CharacterStates
{
    walking, inair
};
