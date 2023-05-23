using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    // Start is called before the first frame update
    public float rotationSpeed = 10f;
    private void Update()
    {
        // Obtener la rotación actual del objeto
        Vector3 currentRotation = transform.rotation.eulerAngles;

        // Calcular la nueva rotación en el eje Y
        float newYRotation = currentRotation.y + rotationSpeed * Time.deltaTime;

        // Aplicar la nueva rotación al objeto
        transform.rotation = Quaternion.Euler(currentRotation.x, newYRotation, currentRotation.z);
    }
}
