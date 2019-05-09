using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class CharacterCamera : MonoBehaviour {

    public Transform follow;

    public float mouseSensitivity = 1;

    public float distance;

    public Vector3 startPointOffset;
    Vector3 startPoint = Vector3.zero;

    public float collisionRadius = 0.2f;
    //public Vector3 focusPointOffset;
    //Vector3 focusPoint = Vector3.zero;

    public Vector2 angleX_MinMax = new Vector2(-45, 80);



    public Vector3 followForward;
    public Vector3 followRight;

    Vector2 input = Vector2.zero;
    Vector3 position = Vector3.zero;
    Vector3 armDirection = Vector3.zero;
    Vector3 armRotation = Vector3.zero;


    // Use this for initialization
    void Start()
    {
        //QualitySettings.vSyncCount = 3;
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();

        CalculateArmRotation();
        CalculateFollowForwardAndRight();
        CalculatePosition();

        SetPosition();
        SetRotation();
    }

    void SetPosition()
    {
        transform.position = position;
    }

    void SetRotation()
    {
        transform.eulerAngles = armRotation;
    }
    void GetInput()
    {
        input = new Vector2(-Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }
    void CalculateFollowForwardAndRight()
    {
        followForward = Quaternion.Euler(new Vector3(0, armRotation.y, 0)) * Vector3.forward;
        followRight = -new Vector3(-followForward.z, 0, followForward.x);
    }
    void CalculateArmRotation()
    {
        armRotation.y += -input.x * mouseSensitivity;
        armRotation.x += -input.y * mouseSensitivity;
        armRotation.x = Mathf.Clamp(armRotation.x, angleX_MinMax.x, angleX_MinMax.y);

        armDirection = Quaternion.Euler(armRotation) * -Vector3.forward;
    }
    void CalculatePosition()
    {
        startPoint = follow.transform.position + startPointOffset;
        position = startPoint + armDirection * CameraCollisionDistance();
    }
    void CalculateRotation()
    {

    }
    float CameraCollisionDistance()
    {
        RaycastHit hit;
        if (Physics.SphereCast(startPoint, collisionRadius, armDirection, out hit, distance, LayerMask.GetMask("Default")))
            return Vector3.Magnitude(hit.point - startPoint) - collisionRadius;
        else
            return distance;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(startPoint, startPoint + followForward);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(startPoint, startPoint + followRight);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(startPoint, startPoint + armDirection * distance);
    }
}

