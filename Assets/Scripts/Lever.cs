using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public GameObject flameThrower;
    public GameObject leverActivated;
    public bool isActive = false;
    public AudioSource leverSound; 

    // Se encarga de activar el lanzallamas de la última zona cuando el jugador activa la palanca.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActive)
        {
            flameThrower.GetComponent<Coin>().enabled = true; 
            isActive = true; 

            leverSound.Play();

            gameObject.SetActive(false);
            leverActivated.SetActive(true);
        }
    }
}
