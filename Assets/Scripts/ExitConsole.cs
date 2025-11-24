using UnityEngine;
using TMPro;
using System.Collections;

public class ExitConsole : Interactable
{
    [Header("Texto")]
    public TextMeshPro exitText;
    public float typeSpeed = 0.03f;
    public string fullMessage = "PRESIONA ESC PARA SALIR DEL JUEGO";

    [Header("Audio Typewriter")]
    public AudioSource sfxSource;
    public AudioClip typeKeyClip;
    public int charsPerSound =1000; 

    private bool playerInside = false;
    private bool showingMessage = false;
    private bool typingInProgress = false;

    private Coroutine typingRoutine;

    void Start()
    {
        if (exitText != null)
        {
            exitText.gameObject.SetActive(false);
            exitText.text = "";
        }
    }

    // ================= INTERACTABLE =================

    public override void OnPlayerEnterRange(PlayerController p)
    {
        playerInside = true;
    }

    public override void OnPlayerExitRange(PlayerController p)
    {
        playerInside = false;
        HideMessage();
    }

    public override void Interact(PlayerController p)
    {
        // si ya se está mostrando o escribiendo, no hagas nada
        if (showingMessage || typingInProgress)
            return;

        ShowMessage();
    }

    // ================= UPDATE =================

    void Update()
    {
        if (!playerInside) return;

        // ESC para salir del juego solo si el mensaje está activo
        if (showingMessage && Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    // ================= LÓGICA DEL MENSAJE =================

    void ShowMessage()
    {
        showingMessage = true;
        typingInProgress = true;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        exitText.text = "";
        exitText.gameObject.SetActive(true);

        typingRoutine = StartCoroutine(TypeText());
    }

    void HideMessage()
    {
        showingMessage = false;
        typingInProgress = false;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        if (exitText != null)
        {
            exitText.gameObject.SetActive(false);
            exitText.text = "";
        }
    }

    IEnumerator TypeText()
    {
        int charIndex = 0;

        foreach (char c in fullMessage)
        {
            exitText.text += c;
            charIndex++;

           
            if (sfxSource != null && typeKeyClip != null && charIndex % charsPerSound == 0)
            {
                sfxSource.PlayOneShot(typeKeyClip);
            }

            yield return new WaitForSeconds(typeSpeed);
        }

        typingInProgress = false;
    }
}
