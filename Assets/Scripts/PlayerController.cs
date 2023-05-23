using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Variables: Movement

    [Header("Movement Settings")]
    public float moveSpeed;
    public float attakingSpeed;
    public float jumpForce;
    public float fallSpeed;
    private Vector3 moveDirection;
    public CharacterController characterController;
    private bool movementReleased = false;

    #endregion

    #region Variables: Input

    [Header("Input Settings")]
    PlayerInput input;
    Vector2 currentMovement;
    Vector2 gadgetDirection;
    bool jumpPressed;
    bool movementPressed;
    bool gadgetUsePressed;
    bool LeftGadgetPressed;
    bool RightGadgetPressed;
    public bool inAttack = false;
    bool isFirstInput = true;
    private bool canAttack = true;
    private bool stopMovement = false;
    public float attackCooldown = 1.0f;

    #endregion

    #region Variables: Gravity

    [Header("Gravity Settings")]
    public float gravityScale = 5f; 

    #endregion

    #region Variables: Animation

    [Header("Animation Settings")]
    public Animator animator; 
    private int isRunningHash;
    private int isJumpingHash;
    private int isGroundedHash;
    private int isFallingHash;
    private int useGadgetHash;

    private float lastGroundedTime;
    private float lastJumpPressedTime;
    private float jumpGracePeriod = 0.5f;
    public GameObject hitbox;
    public ParticleSystem jumpParticleSystem;

    #endregion

    #region Variables: Gadgets

    [Header("Gadgets Settings")]
    public GameObject leftGadget;
    public GameObject rightGadget;
    public GameObject activeGadget;
    public PlayerStatus playerStatus;

    #endregion

    #region Variables: Ground check

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public LayerMask layerMask;
    private Rigidbody playerRigidbody;
    private bool isOnPlatform;

    #endregion 

    #region Variables: Audio Source

    [Header("Sounds Settings")]
    public AudioSource jumpSound; 
    public AudioSource fallDownSound;
    public AudioSource leftFoot;
    public AudioSource rightFoot; 

    #endregion 

    void Awake()
    {
        // Creamos instancia de la clase PlayerInput
        input = new PlayerInput();

        // ctx = context. Contiene el estado actual del input.
        
        // Obtenemos el input del joystick a través de una función lambda.
        // Movimiento del personaje
        input.CharacterControls.Movement.performed += ctx => {
            if (!inAttack)
            {
                currentMovement = ctx.ReadValue<Vector2>();
                //movementPressed = currentMovement.x != 0 || currentMovement.y != 0 ||
                //                  currentMovement.x <= 0.3 || currentMovement.y <= 0.3;
                float movementThreshold = 0.325f;
                movementPressed = Mathf.Abs(currentMovement.x) > movementThreshold || Mathf.Abs(currentMovement.y) > movementThreshold;
                movementReleased = !movementPressed; 
            }
        }; 

        // Usar gadgets - joystick derecho
        input.CharacterControls.UseGadgets.performed += ctx => {
            if (isFirstInput && !inAttack && IsGrounded())
            {
                gadgetDirection = ctx.ReadValue<Vector2>();

                if (gadgetDirection.x != 0 || gadgetDirection.y != 0)
                {
                    gadgetUsePressed = true;
                    isFirstInput = false;
                }
            }
        }; 

        // Botón salto
        input.CharacterControls.Jump.performed += ctx => {
            if (IsGrounded() && !inAttack)   // Solo lee el input de salto si el personaje está en el suelo y no atacando
            {
                jumpPressed = ctx.ReadValueAsButton();
                lastJumpPressedTime = Time.time;
            }
        };

        // Botones de dirección - Izquierdo
        input.CharacterControls.LeftGadget.performed += ctx => {
            if (!inAttack && activeGadget != leftGadget)
            {
                LeftGadgetPressed = true; 
            }
        };

        input.CharacterControls.RightGadget.performed += ctx => {
            if (!inAttack && activeGadget != rightGadget)
            {
                RightGadgetPressed = true; 
            }
        };
    }

    void Start()
    {
        // Establecemos los hash de las animaciones
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");
        isGroundedHash = Animator.StringToHash("isGrounded");
        isFallingHash = Animator.StringToHash("isFalling");
        useGadgetHash = Animator.StringToHash("useGadget");

        playerRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Detecta si el personaje está en el suelo y se lo comunica al animator
        if (IsGrounded())
        {
            animator.SetBool(isGroundedHash, true); 

            // Decimos que puede atacar si lleva un tiempo en el suelo (evita bugs con animaciones)
            if (Time.time - lastGroundedTime > attackCooldown)
            {
                canAttack = true;
            }
        }
        else
        {
            animator.SetBool(isGroundedHash, false); 
        }


        if (!inAttack && !playerStatus.isRespawning && !movementReleased)
        {
            HandleUseGadgets();
            HandleRotation(currentMovement.x, currentMovement.y);
            HandleMovement();
        }

        if (movementReleased)
        {
            // Detener el movimiento del personaje
            movementPressed = false;
            movementReleased = false;
            currentMovement = Vector2.zero;
        } 
    }

    void HandleUseGadgets()
    {
        // Usar gadget - ataque (red, espada)
        if (gadgetUsePressed && IsGrounded() && canAttack)
        { 
            moveDirection = Vector3.zero;
            animator.SetBool(useGadgetHash, true);

            // Flags
            inAttack = true;
            canAttack = false;
            stopMovement = true;

            // Obtener la dirección de rotación deseada
            Vector3 lookDirection = new Vector3(gadgetDirection.x, 0f, gadgetDirection.y);
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

            // Rotar el personaje hacia la dirección deseada
            transform.rotation = targetRotation;

            // Esperar a que la animación termine
            StartCoroutine(WaitForAnimation());
        }

        // Cambiar de gadgets

        if (LeftGadgetPressed)
        {
            rightGadget.SetActive(false);
            leftGadget.SetActive(true);

            activeGadget = leftGadget;

            LeftGadgetPressed = false;
        }

        if (RightGadgetPressed)
        {
            // Cambiamos el gadget activo
            leftGadget.SetActive(false);
            rightGadget.SetActive(true);

            // Indicamos cual es el que tenemos activo para futuras comprobaciones
            // y otros scripts
            activeGadget = rightGadget;

            // Volvemos a poner pressed en false para que lea el siguiente input
            RightGadgetPressed = false;
        }
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
      
        // Calculamos y normalizamos la dirección y velocidad de movimiento.
        if (!inAttack)
        {
            float yStore = moveDirection.y;
            moveDirection = new Vector3(currentMovement.x, 0f, currentMovement.y);
            moveDirection = moveDirection.normalized * moveSpeed;
            moveDirection.y = yStore;
        }

        // Aplicar la fuerza del salto si el jugador ha pulsado el botón
        if (jumpPressed && IsGrounded() && Time.time - lastJumpPressedTime < jumpGracePeriod)
        {
            moveDirection.y = jumpForce;
            jumpPressed = false;

            // Instanciamos y reproducimos el efecto de salto
            ParticleSystem newParticleSystem = Instantiate(jumpParticleSystem, transform.position, Quaternion.identity);

            // Reproducimos el efecto de sonido de salto
            jumpSound.Play();
        }

        // Aplicar gravedad de Unity
        moveDirection.y += Physics.gravity.y * gravityScale * Time.deltaTime;

        // Aplicar gravedad en caída, sin necesidad de ejecutar un salto
        if (!jumpPressed && moveDirection.y < -fallSpeed)
        {
            moveDirection.y = -fallSpeed;
            animator.SetBool(isFallingHash, true);
        } 

        // Función de character controller para aplicar el movimiento
        characterController.Move(moveDirection * Time.deltaTime);   
    }

    // Modifica la dirección en la que mira el personaje según la dirección en la que se mueve
    void HandleRotation(float x, float y)
    {
        // Esta rotacion solo se aplica si no estamos en ataque
        // porque la rotación del ataque debe sobreponerse a la del movimiento
        if (!inAttack)
        {
            Vector3 currentPosition = transform.position;

            Vector3 newPosition = new Vector3(x, 0, y);

            Vector3 positionToLookAt = currentPosition + newPosition;

            transform.LookAt(positionToLookAt);
        }
    }

    bool IsGrounded()
    {
        Collider[] colliders = Physics.OverlapBox(groundCheck.position, groundCheck.localScale, Quaternion.identity, layerMask);

        return colliders.Length > 0;
    }

    // Esta función es llamada por la animación de ataque cuando acabe,
    // así sabemos desde el código cuando poner inAttack en false.
    public void stopAttack()
    {
        print("Ataque termina");
        animator.SetBool(useGadgetHash, false);
        inAttack = false;
        gadgetUsePressed = false;
        isFirstInput = true; 

        // Repara el error de seguir corriendo cuando atacamos mientras nos movemos
        currentMovement = Vector2.zero;
        movementPressed = false; 
    }

    // Asegurar que la animación de usar gadget termina
    private System.Collections.IEnumerator WaitForAnimation()
    {
        // Obtener el índice del parámetro del trigger
        int triggerIndex = Animator.StringToHash("useGadget");

        // Esperar hasta que la animación haya terminado
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = true;
            playerRigidbody.transform.SetParent(collision.transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = false;
            playerRigidbody.transform.SetParent(null);
        }
    }

    // La animación de ataque llama a esta función para activar la hitbox
    public void activateHitbox()
    {
        hitbox.SetActive(true);
    }

    // La animación de ataque llama a esta función para desactivar la hitbox
    public void desactivateHitbox()
    {
        hitbox.SetActive(false);
    } 

    // La animación de correr llama a reproducir sonidos de pie izquierdo y derecho
    public void playLeftFootSound()
    {
        leftFoot.Play();
    }

    public void playRightFootSound()
    {
        rightFoot.Play();
    }
    
     // Reproduce sonido de caer. La llamada la hace la animación.
    public void playFallDownSound()
    {
        fallDownSound.Play();
    }

    void OnEnable()
    {
        input.CharacterControls.Enable();
    }

    void OnDisable()
    {
        input.CharacterControls.Disable();
    }
}