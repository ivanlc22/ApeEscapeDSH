using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CaptureSystem : MonoBehaviour
{
    public int capturedMonkeys = 0;
    public int totalMonkeys = 4;
    public ParticleSystem captureMonkeyParticleSystem; 
    public ParticleSystem captureMonkeyFailedParticleSystem;
    public Text capturedMonkeysText;
    public GameObject noCompletedLevel;
    public GameObject CompletedLevel;
    public AudioSource capturedMonkeySound;
    public AudioSource completedLevelSound;
    public AudioSource dodgeMonkeySound; 
    public bool levelCompleted = false;

    void Start()
    {
        capturedMonkeysText.text = capturedMonkeys + "/" + totalMonkeys;
    }

    // Función que gestiona la capturas al mono.
    public void captureMonkey(GameObject monkey)
    {
        MonkeyStatus monkeyStatus = monkey.GetComponent<MonkeyStatus>();
        float captureRatio = monkeyStatus.captureRatio;

        // Generar un número aleatorio entre 0 y 1
        float tryRatio = Random.Range(0f, 1f);

        // Comparar el número aleatorio con el captureRatio
        if (tryRatio <= captureRatio)
        {
            // "Capturamos" al mono
            // monkey.SetActive(false);
            Destroy(monkey);

            // Aumentamos el número de monos capturados
            capturedMonkeys++;
            capturedMonkeysText.text = capturedMonkeys + "/" + totalMonkeys;
            capturedMonkeySound.Play();

            // Ejecutamos el efecto de captura
            // Instanciamos y reproducimos el efecto de salto
            ParticleSystem newParticleSystem = Instantiate(captureMonkeyParticleSystem, monkey.transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity);
        }
        else
        {
            // Si no acertamos la captura, mostramos el efecto y ya. 
            dodgeMonkeySound.Play();
            ParticleSystem newParticleSystem = Instantiate(captureMonkeyFailedParticleSystem, monkey.transform.position, Quaternion.identity);
        }
    }

    // Se encarga de acabar el nivel si hemos capturado a todos los monos.
    public void endLevel()
    {
        if (capturedMonkeys < totalMonkeys)
        {
            noCompletedLevel.GetComponent<Text>().text = "¡TE FALTAN " + (totalMonkeys - capturedMonkeys) + " MONO(s)!";
            StartCoroutine(ShowNoCompletedLevel());
        }
        else
        {
            if (!levelCompleted)
            {
                levelCompleted = true;
                StartCoroutine(ShowCompletedLevel());   
            }
        }
    }

    // Muestra el texto de nivel no completado.
    private IEnumerator ShowNoCompletedLevel()
    {
        noCompletedLevel.SetActive(true);

        yield return new WaitForSeconds(3f);

        noCompletedLevel.SetActive(false);
    }

    // Muestra el texto de nivel completado.
    private IEnumerator ShowCompletedLevel()
    {
        completedLevelSound.Play();
        CompletedLevel.SetActive(true);

        yield return new WaitForSeconds(3f);

        CompletedLevel.SetActive(false);

        ChangeSceneEndGame();
    }

    // Cambia a la escena de gracias por jugar.
    public void ChangeSceneEndGame()
    {
        SceneManager.LoadScene("ThanksForPlaying");
    }

}
