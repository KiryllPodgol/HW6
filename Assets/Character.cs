using UnityEngine;

public class Character : MonoBehaviour
{
    public float movementSpeed = 2.0f;
    public float sprintSpeed = 5.0f;
    public float rotationSpeed = 0.2f;
    public float animationBlendSpeed = 0.2f;
    public float jumpSpeed = 7.0f;
    private CharacterController controller;
    private Camera characterCamera;
    private Animator animator;
    private float rotationAngle = 0.0f;
    private float targetAnimationSpeed = 0.0f;
    private bool isSprint = false;
    private float speedY = 0.0f;
    private float gravity = -9.81f;
    private bool isJumping = false;
    private bool isDead = false;
    private float currentHitValue = 0.0f;

    private bool StartMove = false;


    public CharacterController Controller
    {
        get { return controller = controller ?? GetComponent<CharacterController>(); }
    }

    [System.Obsolete]
    public Camera CharacterCamera
    {
        get { return characterCamera = characterCamera ?? FindObjectOfType<Camera>(); }
    }

    public Animator CharacterAnimator
    {
        get { return animator = animator ?? GetComponent<Animator>(); }
    }


     void Start()
    {
        //CharacterAnimator.SetTrigger("Spawn");

        spawns();

        //Invoke("EnableMovement", 2.0f);
    }

    private void spawns()
    {
        CharacterAnimator.SetTrigger("Spawn");



        Invoke("EnableMovement", 2.0f);
    }
    private void Respawn()
    {
        
        CharacterAnimator.SetTrigger("Spawn");
        isDead = false;
        Invoke("EnableMovement", 2.0f);
    }
    private void EnableMovement()
    {
        StartMove = true;
        CharacterAnimator.SetTrigger("StartMove");
    }

    [System.Obsolete]
    void Update()
    {
        if (!StartMove) return;
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        // Прыжок
        if ((Input.GetButtonDown("Jump") && Controller.isGrounded && !isJumping))
        {
            isJumping = true;
            CharacterAnimator.SetTrigger("Jump");
            speedY += jumpSpeed;
        }

        // Гравитация и приземление
        if (!Controller.isGrounded)
        {
            speedY += gravity * Time.deltaTime;
        }
        else if (speedY < 0.0f)  // Когда персонаж на земле и падает вниз
        {
            speedY = 0.0f;
        } 
        CharacterAnimator.SetFloat("SpeedY", speedY/jumpSpeed);
        if (isJumping && speedY < 0.0f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f, LayerMask.GetMask("Default")))
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

            Invoke("Respawn", 2.0f);
        }

        //лкм

        if (Input.GetMouseButtonDown(0))
        {
            currentHitValue = Random.Range(0, 3) * 0.5f;
            CharacterAnimator.SetFloat("currentHitValue", currentHitValue);
            CharacterAnimator.SetTrigger("RandomHit"); 
        }

        // Спринт
        isSprint = Input.GetKey(KeyCode.LeftShift);
        // Движение персонажа
        Vector3 movement = new Vector3(horizontal, 0.0f, vertical);
        Vector3 rotatedMovement = Quaternion.Euler(0.0f, CharacterCamera.transform.rotation.eulerAngles.y, 0.0f) * movement.normalized;
        Vector3 verticalMovement = Vector3.up * speedY;
        float currentSpeed = isSprint ? sprintSpeed : movementSpeed;

        // Добавляем вертикальное движение
        //Vector3 finalMovement = rotatedMovement * currentSpeed + Vector3.up * speedY;
        Controller.Move((verticalMovement + rotatedMovement * currentSpeed) * Time.deltaTime);

        // Анимация и вращение
        if (rotatedMovement.sqrMagnitude > 0.0f)
        {
            rotationAngle = Mathf.Atan2(rotatedMovement.x, rotatedMovement.z) * Mathf.Rad2Deg;
            targetAnimationSpeed = isSprint ? 1.0f : 0.5f;
        }
        else
        {
            targetAnimationSpeed = 0.0f;
        }

        // Анимация скорости движения
        CharacterAnimator.SetFloat("Speed", Mathf.Lerp(CharacterAnimator.GetFloat("Speed"), targetAnimationSpeed, animationBlendSpeed));

        // Вращение персонажа
        Quaternion currentRotation = Controller.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0.0f, rotationAngle, 0.0f);
        Controller.transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, rotationSpeed);
    }
}
