using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    public int magazineSize = 8;
    public float reloadTime = 1.0f;

    private int currentAmmo;
    private float nextFireTime = 0f;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip shootClip;
    public AudioClip reloadClip;
    public AudioClip emptyClip;
    public AudioClip drawClip;
    public AudioClip holsterClip;
    public AudioClip[] stepClips;

    [Header("Footsteps")]
    public float stepInterval = 0.3f;
    private float stepTimer = 0f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;

    private Vector2 moveInput;
    private bool facingRight = true;
    private bool hasGun = false;
    private bool isReloading = false;

    // Interacción
    private Interactable currentInteractable;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        currentAmmo = magazineSize;
    }

    void Update()
    {
        // ============================================================
        // MOVIMIENTO
        // ============================================================

        if (!isReloading)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
        }
        else
        {
            moveInput = Vector2.zero;
        }

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        anim.SetBool("isMoving", isMoving);

        // ============================================================
        // MIRADA IZQ/DER
        // ============================================================

        if (!isReloading)
        {
            if (moveInput.x > 0) facingRight = true;
            else if (moveInput.x < 0) facingRight = false;
        }

        sprite.flipX = !facingRight;

        // ============================================================
        // SACAR / GUARDAR ARMA
        // ============================================================

        if (Input.GetKeyDown(KeyCode.E) && !isReloading)
        {
            hasGun = !hasGun;
            anim.SetBool("hasGun", hasGun);

            if (sfxSource)
            {
                if (hasGun && drawClip)
                    sfxSource.PlayOneShot(drawClip);
                else if (!hasGun && holsterClip)
                    sfxSource.PlayOneShot(holsterClip);
            }
        }

        // ============================================================
        // DISPARO
        // ============================================================

        if (hasGun && !isReloading)
        {
            if (Input.GetMouseButtonDown(0))
                TryShoot(); // Disparo por click

            if (Input.GetMouseButton(0) && currentAmmo > 0)
                TryShoot(); // Autofire 
        }

        // ============================================================
        // RECARGA MANUAL
        // ============================================================

        if (Input.GetKeyDown(KeyCode.R) && hasGun && !isReloading && currentAmmo < magazineSize)
        {
            StartReload();
        }

        anim.SetBool("isReloading", isReloading);

        // ============================================================
        // PASOS
        // ============================================================

        if (isMoving && !isReloading)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayStep();
                stepTimer = stepInterval;
            }
        }
        else stepTimer = 0f;

        // ============================================================
        // INTERACCIÓN (F)
        // ============================================================

        if (Input.GetKeyDown(KeyCode.F) && !isReloading && currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    // ============================================================
    // DISPARO
    // ============================================================

    void TryShoot()
    {
        if (Time.time < nextFireTime) return;

        if (currentAmmo <= 0)
        {
            if (sfxSource && emptyClip)
                sfxSource.PlayOneShot(emptyClip);

            return;
        }

        nextFireTime = Time.time + fireRate;
        currentAmmo--;

        float dirX = facingRight ? 1f : -1f;
        Vector2 shootDir = new Vector2(dirX, 0f);

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null) b.SetDirection(shootDir);

        if (sfxSource && shootClip)
            sfxSource.PlayOneShot(shootClip);
    }

    // ============================================================
    // RECARGA
    // ============================================================

    void StartReload()
    {
        isReloading = true;

        if (sfxSource && reloadClip)
            sfxSource.PlayOneShot(reloadClip);

        Invoke(nameof(FinishReload), reloadTime);
    }

    void FinishReload()
    {
        isReloading = false;
        currentAmmo = magazineSize;
    }

    // ============================================================
    // PASOS
    // ============================================================

    void PlayStep()
    {
        if (!sfxSource || stepClips == null || stepClips.Length == 0) return;
        int i = Random.Range(0, stepClips.Length);
        sfxSource.PlayOneShot(stepClips[i]);
    }

    // ============================================================
    // TRIGGERS PARA INTERACTUABLES
    // ============================================================

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Interactable it = collision.GetComponent<Interactable>();
        if (it != null)
        {
            currentInteractable = it;
            it.OnPlayerEnterRange(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Interactable it = collision.GetComponent<Interactable>();
        if (it != null && it == currentInteractable)
        {
            it.OnPlayerExitRange(this);
            currentInteractable = null;
        }
    }
}
