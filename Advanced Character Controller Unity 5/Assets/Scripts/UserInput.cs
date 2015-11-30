using UnityEngine;
using System.Collections;


public class UserInput : MonoBehaviour {

    public bool walkByDefault = false;

    private CharMove character;
    private Transform cam;
    private Vector3 camForward;
    private Vector3 move;

	public bool aim;
	public float aimingWeight;

	public bool lookInCameraDirection;
	Vector3 lookPos;

	Animator anim;

	//IK stuff. These values will need to be changed for different rigs...
	public Transform spine;
	public float aimingZ = 213.46f;
	public float aimingX = -65.93f;
	public float aimingY = 20.1f;
	public float point = 30;

    void Start() {

        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        character = GetComponent<CharMove>();
		anim = GetComponent<Animator>();
    }

	// This is where we handle the aiming on right-click
	// Because the right click (mouse button 1) does not work on the mac simulator, 
	// I changed it to regular click. The original is commented out.
	void LateUpdate() {
	
		aim = Input.GetMouseButton (0);
		//aim = Input.GetMouseButton (1);

		aimingWeight = Mathf.MoveTowards (aimingWeight, (aim) ? 1.0f : 0.0f, Time.deltaTime * 5);

		Vector3 normalState = new Vector3 (0, 0, -2f);
		Vector3 aimingState = new Vector3 (0, 0, -0.5f);

		Vector3 pos = Vector3.Lerp (normalState, aimingState, aimingWeight);

		cam.transform.localPosition = pos;

		// if aiming, make character look and orient itself to the correct position
		if (aim) {
		
			Vector3 eulerAngleOffset = Vector3.zero;

			eulerAngleOffset = new Vector3(aimingX, aimingY, aimingZ);

			Ray ray = new Ray(cam.position, cam.forward);

			Vector3 lookPosition = ray.GetPoint(point);

			spine.LookAt(lookPosition);
			spine.Rotate (eulerAngleOffset, Space.Self);
		}
	}

    void FixedUpdate()
    {

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

		if (!aim) {

			if (cam != null) {

				camForward = Vector3.Scale (cam.forward, new Vector3 (1, 0, 1)).normalized;
				move = vertical * camForward + horizontal * cam.right;

			} else {

				move = vertical * Vector3.forward + horizontal * Vector3.right;
			}

		} else {

			move = Vector3.zero;

			Vector3 dir = lookPos - transform.position;
			dir.y = 0;

			transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (dir), 20 * Time.deltaTime);

			anim.SetFloat ("Forward", vertical);
			anim.SetFloat ("Turn", horizontal);
		}

        if (move.magnitude > 1)
            move.Normalize();

        bool walkToggle = Input.GetKey(KeyCode.LeftShift);

        float walkMultiplier = 1;

        if (walkByDefault)
        {
            if (walkToggle)
            {
                walkMultiplier = 1;
            }
            else
            {
                walkMultiplier = 0.5f;
            }
        }
        else
        {
            if (walkToggle)
            {
                walkMultiplier = 0.5f;
            }
            else
            {
                walkMultiplier = 1;
            }
        }

		lookPos = lookInCameraDirection && cam != null ? transform.position + cam.forward * 100 : transform.position + transform.forward * 100;

        move *= walkMultiplier;
        character.Move(move, aim, lookPos);
    }

}