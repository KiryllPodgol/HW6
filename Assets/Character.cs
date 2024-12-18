using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float movementSpeed = 2.0f;
    public float sprintSpeed = 5.0f;
    public float rotationSpeed = 0.2f;
    public float animationBlendSpeed = 0.2f;
    public float jumpSpeed = 7.0f;
    public LayerMask Ground;
    private CharacterController controller;
    private Camera characterCamera;
    private Animator animator;
    private float rotationAngle = 0.0f;
    private float targetAnimationSpeed = 0.0f;
    private float speedY = 0.0f;
    private float gravity = -9.81f;
    private float currentHitValue = 0.0f;
    private float respawnDelay = 2.1f;

    private bool isSprint = false;
    private bool isDead = false;
    private bool isJumping = false;
    private bool StartMove = false;


    public CharacterController Controller
    {
        get { return controller = controller ?? GetComponent<CharacterController>(); }
    }


    public Camera CharacterCamera
    {
        get { return characterCamera = characterCamera ?? Camera.main; }
    }

    public Animator CharacterAnimator
    {
        get { return animator = animator ?? GetComponent<Animator>(); }
    }


    private void Start()
    {
        Respawn();
    }

    private void Respawn()
    {
        CharacterAnimator.SetTrigger("Spawn");
        isDead = false;
        StartMove = false;
        StartCoroutine(EnableMovementAfterDelay());
    }

    private IEnumerator EnableMovementAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        EnableMovement();
    }

    private void EnableMovement()
    {
        StartMove = true;
        CharacterAnimator.SetTrigger("StartMove");
    }

    void Update()
    {
        if (!StartMove) return;
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        // ������
        if ((Input.GetButtonDown("Jump") && Controller.isGrounded && !isJumping))
        {
            isJumping = true;
            CharacterAnimator.SetTrigger("Jump");
            speedY += jumpSpeed;
        }

        // ���������� � �����������
        if (!Controller.isGrounded)
        {
            speedY += gravity * Time.deltaTime;
        }
        else if (speedY < 0.0f)  // ����� �������� �� ����� � ������ ����
        {
            speedY = 0.0f;
        } 
        CharacterAnimator.SetFloat("SpeedY", speedY/jumpSpeed);
        if (isJumping && speedY < 0.0f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f, Ground))
            {
                isJumping= false;
                CharacterAnimator.SetTrigger("Land");
            }
        }
        if (Input.GetKeyDown(KeyCode.E) && !isDead)
        {
            CharacterAnimator.SetTrigger("Death");
            isDead = true;
            StartMove = false;
         
        }
        if (isDead)
        {

            Respawn();
        }

        //���

        if (Input.GetMouseButtonDown(0))
        {
            currentHitValue = Random.Range(1, 4); // ���������� ��������� ����� 1, 2 ��� 3
            CharacterAnimator.SetFloat("currentHitValue", currentHitValue);
            CharacterAnimator.SetTrigger("RandomHit");
        }

        // ������
        isSprint = Input.GetKey(KeyCode.LeftShift);
        // �������� ���������
        Vector3 movement = new Vector3(horizontal, 0.0f, vertical);
        Vector3 rotatedMovement = Quaternion.Euler(0.0f, CharacterCamera.transform.rotation.eulerAngles.y, 0.0f) * movement.normalized;
        Vector3 verticalMovement = Vector3.up * speedY;
        float currentSpeed = isSprint ? sprintSpeed : movementSpeed;

        // ��������� ������������ ��������
        //Vector3 finalMovement = rotatedMovement * currentSpeed + Vector3.up * speedY;
        Controller.Move((verticalMovement + rotatedMovement * currentSpeed) * Time.deltaTime);

        // �������� � ��������
        if (rotatedMovement.sqrMagnitude > 0.0f)
        {
            rotationAngle = Mathf.Atan2(rotatedMovement.x, rotatedMovement.z) * Mathf.Rad2Deg;
            targetAnimationSpeed = isSprint ? 1.0f : 0.5f;
        }
        else
        {
            targetAnimationSpeed = 0.0f;
        }

        // �������� �������� ��������
        CharacterAnimator.SetFloat("Speed", Mathf.Lerp(CharacterAnimator.GetFloat("Speed"), targetAnimationSpeed, animationBlendSpeed));

        // �������� ���������
        Quaternion currentRotation = Controller.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0.0f, rotationAngle, 0.0f);
        Controller.transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, rotationSpeed);
    }
}
