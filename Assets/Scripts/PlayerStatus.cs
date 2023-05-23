using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 100;
    public int cointCounter;
    public Transform respawnPoint;
    public bool isRespawning;
    public GameObject player;
    public PlayerController playerController;
    public ParticleSystem respawnParticleSystem;
    public ParticleSystem dieParticleSystem;
    public ParticleSystem pickCoinParticleSystem; 
    public AudioSource coinSound;
    public AudioSource dieSound;

    public CaptureSystem captureSystem;
    public bool levelCompleted = false;

    void Start()
    {
        maxHealth = currentHealth; 
        cointCounter = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si te caes, reapareces
        if (other.CompareTag("FallingArea"))
        {
            Respawn();
        }

        // Si te toca el fuego, te hace daño
        if (other.CompareTag("Flames"))
        {
            takeDamage(100);
        }

        // Si colisionas con una moneda, la coges y se reproduce el sonido y el efecto
        if (other.CompareTag("Coin"))
        {
            coinSound.Play();
            ParticleSystem newParticleSystem = Instantiate(pickCoinParticleSystem, other.gameObject.transform.position, Quaternion.identity);
            other.gameObject.SetActive(false);
            cointCounter++;
        }

        if (other.CompareTag("End"))
        {
           captureSystem.endLevel();
        }
    }



    // Hace reaparecer al jugador en el punto de respawn.
    // Se debe desactivar characterController porque al parecer usar
    // character controller impide que se hagan cambios de posición.
    void Respawn()
    {
        //GetComponent<CharacterController>().enabled = false;
        //transform.position = respawnPoint.position; 
        //GetComponent<CharacterController>().enabled = true;

        if (!isRespawning)
        {
            maxHealth = currentHealth; 
            StartCoroutine(RespawnCo());
        }
    }

    // Resta el daño a la vida del objetivo.
    // Si el daño deja sin vida al jugador, reaparece.
    void takeDamage(int amount)
    {
        currentHealth -= amount;
        
        if (currentHealth <= 0)
        {
            Respawn();
        }
    }

    // Hace el respawn y las animaciones con pequeños retardos para que se vea natural.
    public IEnumerator RespawnCo()
    {
        GameObject renderer = transform.GetChild(0).gameObject;

        GetComponent<CharacterController>().enabled = false;
        isRespawning = true;
        playerController.activeGadget.SetActive(false);
        renderer.SetActive(false);

        dieSound.Play();

        ParticleSystem newParticleSystem1 = Instantiate(dieParticleSystem, player.transform.position, Quaternion.Euler(-90f, 0f, 0f));

        yield return new WaitForSeconds(1f);

        transform.position = respawnPoint.position;
        transform.rotation = Quaternion.Euler(Vector3.zero);; 

        ParticleSystem newParticleSystem2 = Instantiate(respawnParticleSystem, player.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(1f);

        renderer.SetActive(true);
        playerController.activeGadget.SetActive(true);

        GetComponent<CharacterController>().enabled = true;
        isRespawning = false;

        yield return new WaitForSeconds(1f);

        newParticleSystem2.Stop();
    }
}
