using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Este script se utiliza para gestionar el collider de los gadgets con los monos.
// Actualmente, de la red y la espada. 
public class HitboxNet : MonoBehaviour
{
    public CaptureSystem captureSystem;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monkey"))
        {
            // Se produjo una colisión con la hitbox con tag "Monkey"
            HandleCollision(other.gameObject);
        }
    }

    // Llama a la función de CaptureSystem : captureMonkey
    private void HandleCollision(GameObject monkey)
    {
        captureSystem.captureMonkey(monkey);
    }
}
