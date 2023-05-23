using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameThrower : MonoBehaviour
{
    public GameObject flamesHitbox;
    private bool isActive = true;
    public GameObject flameParticleSystem;
    public AudioSource flameSound;
    public bool isTimed;

    private void Start()
    {
        // Iniciar la corutina para activar y desactivar el objeto
        if (isTimed)
        {
            StartCoroutine(ActivateAndDeactivateObject());
        }
        else
        {
            flamesHitbox.SetActive(true);
            flameParticleSystem.SetActive(true);
        }
    }

    private System.Collections.IEnumerator ActivateAndDeactivateObject()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);

            // Cambiar el estado del objeto
            isActive = !isActive;
            flamesHitbox.SetActive(isActive);
            flameParticleSystem.SetActive(isActive);
        }
    }
}
