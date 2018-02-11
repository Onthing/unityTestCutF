using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] float movingTurnSpeed = 360;
    [SerializeField] float stationaryTurnSpeed = 180;
    [SerializeField] float groundCheckDistance = 0.1f;

    public UnityEvent OnDeath;

    public AudioClip dieSound;

    Rigidbody rigid;
    Animator anim;
    CapsuleCollider capsule;

    float turnAmount;
    float forwardAmount;
    Vector3 groundNormal;
    float capsuleHeight;
    Vector3 capsuleCenter;

    bool isCrouching;
    bool isGrounded;
    public bool isDead = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        capsuleHeight = capsule.height;
        capsuleCenter = capsule.center;

        rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        UpdateInput();
        UpdateMovement();
        UpdateAnimator();
    }

    void UpdateInput() {
        if (isDead) return;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool crouch = Input.GetKey(KeyCode.C);
        var move = v * Vector3.forward + h * Vector3.right;
        Move(move);
        Crouching(crouch);
    }

    void UpdateMovement()
    {
        CheckGroundStatus();

        //转向控制
        float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
        transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
    }

    void UpdateAnimator() {
        anim.SetFloat("Forward", forwardAmount, 0.01f, Time.deltaTime);
        anim.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
        anim.SetBool("Crouch", isCrouching);
        anim.SetBool("OnGround", isGrounded);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(OnDieBehaviour());
    }

    IEnumerator OnDieBehaviour() {
        Move(Vector3.zero);
        anim.Play("Dead");
        capsule.height = 0.2f;
        capsule.center = new Vector3(0, 0.3f, 0);
        AudioSource.PlayClipAtPoint(dieSound, transform.position);
        OnDeath.Invoke();

        yield return new WaitForSeconds(3);
        GameplayStatics.RestartLevel();
    }

    public void Move(Vector3 move)
    {
        if (move.magnitude > 1f) move.Normalize();
        //将move从世界空间转向本地空间
        move = transform.InverseTransformDirection(move);
        //将move投影在地板的2D平面上
        move = Vector3.ProjectOnPlane(move, groundNormal);
        //返回值为x轴和一个（在零点起始，在(x, y)结束）的2D向量的之间夹角
        turnAmount = Mathf.Atan2(move.x, move.z);
        forwardAmount = move.z;
    }

    public void Crouching(bool crouch)
    {
        if (isGrounded && crouch)
        {
            if (isCrouching) return;
            capsule.height = capsule.height / 2f;
            capsule.center = capsule.center / 2f;
            isCrouching = true;
        }
        else
        {
            //限制头顶有遮挡时，必须蹲下
            Ray crouchRay = new Ray(rigid.position + Vector3.up * capsule.radius * 0.5f, Vector3.up);
            float crouchRayLength = capsuleHeight - capsule.radius * 0.5f;
            if (Physics.SphereCast(crouchRay, capsule.radius * 0.5f, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                isCrouching = true;
                return;
            }
            capsule.height = capsuleHeight;
            capsule.center = capsuleCenter;
            isCrouching = false;
        }
    }

    void CheckGroundStatus()
    {
        RaycastHit hitInfo;

        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * groundCheckDistance));

        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, groundCheckDistance))
        {
            groundNormal = hitInfo.normal;
            isGrounded = true;
            anim.applyRootMotion = true;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
            anim.applyRootMotion = false;
        }
    }
}

