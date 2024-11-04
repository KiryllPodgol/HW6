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
    private bool isSpawning = false;
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

    private void Start()
    {
        CharacterAnimator.SetTrigger("Spawn");
        isSpawning = true;
       


    }
    void Update()
    {
        if (!StartMove) return;
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        // Прыжок
        if (Input.GetButtonDown("Jump") && !isJumping && Controller.isGrounded)
        {
            isJumping = true;
            CharacterAnimator.SetTrigger("Jump");
            speedY = jumpSpeed;
        }

        // Гравитация и приземление
        if (!Controller.isGrounded)
        {
            speedY += gravity * Time.deltaTime;
        }
        else if (speedY < 0.0f)  // Когда персонаж на земле и падает вниз
        {
            speedY = 0.0f;
            if (isJumping)
            {
                isJumping = false;
                CharacterAnimator.SetTrigger("Land");
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && isDead)
        {
            CharacterAnimator.SetTrigger("Death");
          
            isDead = true;
        }
        else
        {
            isDead = false;
      
        }

        // Обновление вертикальной скорости анимации
        CharacterAnimator.SetFloat("SpeedY", speedY / jumpSpeed);

        // Спринт
        isSprint = Input.GetKey(KeyCode.LeftShift);

        // Движение персонажа
        Vector3 movement = new Vector3(horizontal, 0.0f, vertical);
        Vector3 rotatedMovement = Quaternion.Euler(0.0f, CharacterCamera.transform.rotation.eulerAngles.y, 0.0f) * movement.normalized;
        float currentSpeed = isSprint ? sprintSpeed : movementSpeed;

        // Добавляем вертикальное движение
        Vector3 finalMovement = rotatedMovement * currentSpeed + Vector3.up * speedY;
        Controller.Move(finalMovement * Time.deltaTime);

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
