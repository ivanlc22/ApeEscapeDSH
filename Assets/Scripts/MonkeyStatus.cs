using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonkeyStatus : MonoBehaviour
{
    // Este script contiene los métodos para modificar el estado del mono,
    // Podremos cambiar la dificultad de cada uno desde aquí. Todos los monos tendrán este script en principio.

    // Probabilidad de capturar a un mono
    public float captureRatio = 1f;

    public void increaseCaptureRatio(int amount)
    {
        // amount deberá estar entre 0 y 1 porque es una probabilidad 
        if (amount > 0 && amount < 1)
        {
            // Si se pasa de 1 al sumar, la probabilidad será 100%, es decir, 1. 
            if (captureRatio + amount > 1)
            {
                captureRatio = 1f;
            }
            else // Si no se suma normal
            {
                captureRatio += amount;
            }
        }
    }

     public void decreaseCaptureRatio(int amount)
    {
        // amount deberá estar entre 0 y 1 porque es una probabilidad 
        if (amount > 0 && amount < 1)
        {
            // Si se pasa de 0.1 al restar, la probabilidad será 0.1 que es la mínima. Si es 0, el mono es imposible de atrapar.
            if (captureRatio - amount < 0)
            {
                captureRatio = 0.1f;
            }
            else // Si no se resta normal
            {
                captureRatio -= amount;
            }
        }
    }
}
