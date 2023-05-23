using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonkeyAI : MonoBehaviour
{
    #region Variables: Agent Settings
    
    [Header("Agent Settings")]
    private NavMeshAgent agent;
    public GameObject player;
    public Transform[] waypoints; // Todo lo relevante a waypoints esta desactivado, sin embargo, lo dejo por si en el futuro lo mejoro
    public int currentWaypoint = 0;

    public float EnemyDistanceRun = 4.0f;
    bool waypointReached = false; 

    private Transform startTransform;
    public float multiplyBy;

    #endregion

    #region Variables: Animator Settings

    [Header("Animator Settings")]
    public Animator animator;

    #endregion

    #region Variables: Audio Settings

    [Header("Audio Settings")]

    public AudioSource monkeySound;

    private bool monkeySoundPlayed = false; 

    #endregion

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // agent.SetDestination(waypoints[0].position);
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
    
        if (agent.velocity.magnitude > 0.1f)
        {
            // Iniciar animación de correr
            animator.SetBool("isRunning", true);
        }
        else
        {
            // Parar animación de correr
            animator.SetBool("isRunning", false);

            // Mirar al jugador cuando esta quieto
            Vector3 directionToPlayer = player.transform.position - transform.position;
            directionToPlayer.y = 0; // Solo mantener la rotación horizontal
            
            // Establecer la rotación del agente hacia el jugador
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = targetRotation;
            }
        }

        // El mono solo huye si el jugador se acerca
        if (distance < EnemyDistanceRun)
        {
            if (!monkeySoundPlayed)
            {
                monkeySound.Play();
                monkeySoundPlayed = true;   
            }

            RunFrom();    
        }
    }
    

    // El mono huirá en dirección contraria al jugador con un factor de aleatoriedad
    public void RunFrom()
    {
        // guardar transform inicial
        startTransform = transform;
         
        Vector3 directionToPlayer = player.transform.position - transform.position;
        directionToPlayer.y = 0; // Quita la rotacion vertical
        Quaternion targetRotation = Quaternion.LookRotation(-directionToPlayer, Vector3.up);
        transform.rotation = targetRotation;
 
        // Posicion de la rotacion, multiplyBy hace que sea aleatoria
        float multiplyBy = Random.Range(1f, 10f);
        Vector3 runTo = transform.position + transform.forward * multiplyBy;
        //Debug.Log("runTo = " + runTo);

        NavMeshHit hit;

        NavMesh.SamplePosition(runTo, out hit, 5, 1 << NavMesh.GetAreaFromName("Walkable")); 
 
        transform.position = startTransform.position;
        transform.rotation = startTransform.rotation;
        agent.SetDestination(hit.position);
    }
    
    // Obtener waypoint aleatorio
    Transform GetRandomEscapePoint()
    {
        if(waypoints.Length > 0)
        {
            int randomIndex = Random.Range(0, waypoints.Length);
            return waypoints[randomIndex];

        }

        return null; // Array de posiciones vacío.
    }

    // Obtener waypoint más cercano
    Transform GetClosestEscapePoint()
    {
        Transform closestPoint = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform waypoint in waypoints)
        {
            float distance = Vector3.Distance(transform.position, waypoint.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = waypoint;
            }
        }

        return closestPoint;
    }

}
