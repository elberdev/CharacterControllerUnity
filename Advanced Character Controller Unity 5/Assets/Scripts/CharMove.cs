using UnityEngine;
using System.Collections;

public class CharMove : MonoBehaviour {

    float moveSpeedMultiplier = 1;
    float stationaryTurnSpeed = 180; 
    float movingTurnSpeed = 360;

    public bool onGround; 

    Animator animator;

    Vector3 moveInput; 
    float turnAmount; 
    float forwardAmount; 
    Vector3 velocity;

    float jumpPower = 10;
   
    IComparer rayHitComparer;

	float autoTurnThreshold = 10;
	float autoTurnSpeed = 20;
	bool aim;
	Vector3 currentLookPos;

    Rigidbody rigidBody;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        SetUpAnimator();

        rigidBody = GetComponent<Rigidbody>();   
    }

    void SetUpAnimator()
    {
        animator = GetComponent<Animator>();

        foreach (var childAnimator in GetComponentsInChildren<Animator>())
        {
            if (childAnimator != animator)
            {
                animator.avatar = childAnimator.avatar;
                Destroy(childAnimator);
                break; 
            }
        }
    }
 
    void OnAnimatorMove()
    {
        if (onGround && Time.deltaTime > 0)
        {
            Vector3 v = (animator.deltaPosition * moveSpeedMultiplier) / Time.deltaTime; 
            v.y = rigidBody.velocity.y;
            rigidBody.velocity = v;
        }
    }

    public void Move(Vector3 move, bool aim, Vector3 lookPos) {

        if (move.magnitude > 1)
            move.Normalize();
       
        this.moveInput = move; 
		this.aim = aim;
		this.currentLookPos = lookPos;

        velocity = GetComponent<Rigidbody>().velocity; 

        ConvertMoveInput();

		if (!aim) {

			TurnTowardsCameraForward ();
			ApplyExtraTurnRotation();
		}

        GroundCheck ();
        UpdateAnimator();
    }

    void ConvertMoveInput()
    {
        Vector3 localMove = transform.InverseTransformDirection(moveInput);

        turnAmount = Mathf.Atan2(localMove.x, localMove.z);

        forwardAmount = localMove.z;
    }

    void ApplyExtraTurnRotation()
    {
        float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
        transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
    }

    void UpdateAnimator()
    { 
       	animator.applyRootMotion = true;
       	animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
       	animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);

		//animator.SetBool ("Aim", aim);
    }
   
    void GroundCheck()
    {
        Ray ray = new Ray(transform.position + Vector3.up * .1f, -Vector3.up);

        RaycastHit[] hits = Physics.RaycastAll(ray, .1f);
        rayHitComparer = new RayHitComparer();
        System.Array.Sort(hits, rayHitComparer);

        if (velocity.y < jumpPower * .5f)
        { 	
            onGround = false;
            rigidBody.useGravity = true;

            foreach (var hit in hits)
            { 
                if (!hit.collider.isTrigger)
                {
                    if (velocity.y <= 0)
                    {
                          rigidBody.position = Vector3.MoveTowards(rigidBody.position, hit.point, Time.deltaTime * 100);
                    }

                    onGround = true; 
                    rigidBody.useGravity = false; 
                }
            }
        }
    }

	void TurnTowardsCameraForward() {
	
		if (Mathf.Abs (forwardAmount) < .01f) {
		
			Vector3 lookDelta = transform.InverseTransformDirection(currentLookPos - transform.position);

			float lookAngle = Mathf.Atan2 (lookDelta.x, lookDelta.z) * Mathf.Rad2Deg;

			if(Mathf.Abs (lookAngle) > autoTurnThreshold) {
			
				turnAmount += lookAngle * autoTurnSpeed * .001f;
			}
		}
	}

    class RayHitComparer : IComparer {

        public int Compare(object x, object y) {

            return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
        }
    }

}