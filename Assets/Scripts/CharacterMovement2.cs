using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement2 : MonoBehaviour
{
    // Animator

    public Animator animator;
    int isRunningHash; 

    // Input System

    // Clase PlayerInput generada con el input system
    PlayerInput input;
    Vector2 currentMovement;
    Vector3 movement;
    bool movementPressed;
    bool jumpPressed;
    bool isJumping = false; 
    public float jumpForce = 15f; 

    public float movementSpeed = 5f;
    public float gravityMultiplier = 3.0f;
    private float gravity = -9.81f; 
    float fallVelocity;
    public CharacterController controller; 

    [SerializeField] Transform groundCheck;
    [SerializeField] float groundRadius;
    [SerializeField] LayerMask whatIsGrounded;


    void Awake()
    {
        // Creamos instancia de la clase PlayerInput
        input = new PlayerInput();

        // ctx = context. Contiene el estado actual del input.
        // Obtenemos el input del joystick a través de una función lambda.
        input.CharacterControls.Movement.performed += ctx => {
            currentMovement = ctx.ReadValue<Vector2>();
            movementPressed = currentMovement.x != 0 || currentMovement.y != 0;
        }; 

        input.CharacterControls.Jump.performed += ctx => jumpPressed = ctx.ReadValueAsButton();
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        isRunningHash = Animator.StringToHash("isRunning");
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    void HandleRotation()
    {
        Vector3 currentPosition = transform.position;

        Vector3 newPosition = new Vector3(currentMovement.x, 0, currentMovement.y);

        Vector3 positionToLookAt = currentPosition + newPosition;

        transform.LookAt(positionToLookAt);
    }

    void HandleMovement()
    {
        // Obtener parámetros de control de animaciones
        bool isRunning = animator.GetBool(isRunningHash);

        // Empezar animación de correr si estamos moviendo al personaje.
        if (movementPressed)
        {
            animator.SetBool(isRunningHash, true);
        }

        if (!movementPressed)
        {
            animator.SetBool(isRunningHash, false);
        }

        // Actualizar la posición del objeto en la dirección del movimiento
        movement = new Vector3(currentMovement.x, 0, currentMovement.y);

        // Aplicar gravedad si el personaje no está tocando el suelo
        //if (!controller.isGrounded)
        if (!IsGrounded())
        {
            fallVelocity += gravity * gravityMultiplier * Time.deltaTime;
            movement.y = fallVelocity;
        }
        else
        {
            fallVelocity = 0f;
        }

        // Funcion en el que tendremos las diferentes acciones del jugador: saltar, atacar... etc
        PlayerActions();

        controller.Move(movement * movementSpeed * Time.deltaTime);

    }

    public bool IsGrounded()
    {
        if (Physics.CheckSphere(groundCheck.position, groundRadius, (int)whatIsGrounded))
        {
            fallVelocity = 0f;
            return true; 
        }
        else
        {
            return false; 
        }
    }
    
    void PlayerActions()
    {
        // Salto
        if (IsGrounded() && jumpPressed)
        {
            movement.y += jumpForce;
            print("hola");
            jumpPressed = false; 
        }
    }

    // Cuando el personaje está en enable, se activa el control
    void OnEnable()
    {
        input.CharacterControls.Enable();
    }

    void OnDisable()
    {
        input.CharacterControls.Disable();
    }
}
