﻿using UnityEngine;
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

    void Start() {

        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        character = GetComponent<CharMove>();
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