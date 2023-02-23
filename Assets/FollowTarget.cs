using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    // camera will follow this object
    public Transform target;
    //camera transform
    public Transform camTransform;
    // offset between camera and target
    public Vector3 offset;

    [Tooltip("change this value to get desired smoothness")]
    public float smoothTime = 0.3f;

    [Tooltip("point at x unit in the forward direction of the target")]
    public float anticipateDistance = 1f;

    [Tooltip("camera rotation may be annoying for certain settings")]
    public bool lookAtTarget = true;
    
    public bool unparentAtStart = true;

    // This value will change at the runtime depending on target movement. Initialize with zero vector.
    private Vector3 velocity = Vector3.zero;


    private void Start()
    {   
        //if unassigned assume this transform
        if(camTransform == null)
            camTransform = transform;

        //save the current offset for easy editor setup
        offset = camTransform.position - target.position;

        if(unparentAtStart)
            transform.SetParent(null);
    }
    
    //late update runs after the update and in this case is preferable
    private void LateUpdate()
    {
        if (target != null)
        {
            // update position
            
            Vector3 targetPosition = target.position + anticipateDistance * target.forward + offset;

            camTransform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            if (lookAtTarget)
            {
                // update rotation, for certain types of movement this may not be a good idea
                transform.LookAt(target);
            }
        }
    }
}