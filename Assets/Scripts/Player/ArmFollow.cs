using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmFollow : MonoBehaviour {
    public Transform target;
    public float smoothRot = 0.01f;
    public float smoothTime = 0.05f;
    float velocity = 0f;
    Vector3 vel = Vector3.zero;

    void LateUpdate() {
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref vel, smoothTime);
        Quaternion currentRotation = transform.rotation;
        Quaternion goalRotation = target.rotation;
        float smoothX = Mathf.SmoothDamp(currentRotation.x, goalRotation.x, ref velocity, smoothRot);
        float smoothY = Mathf.SmoothDamp(currentRotation.y, goalRotation.y, ref velocity, smoothRot);
        float smoothZ = Mathf.SmoothDamp(currentRotation.z, goalRotation.z, ref velocity, smoothRot);
        float smoothW = Mathf.SmoothDamp(currentRotation.w, goalRotation.w, ref velocity, smoothRot);
        Quaternion smoothQuaternion = new Quaternion(smoothX, smoothY, smoothZ, smoothW);
        transform.rotation = smoothQuaternion;
    }

}
