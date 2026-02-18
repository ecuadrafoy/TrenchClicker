
using UnityEngine;

public class SoldierVisual : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private Sprite[] runFrames;
    private Sprite[][] dieFrames;
    private Sprite[] activeDieFrames;
    // Animation state
    private int currentFrame;
    private float frameTimer;
    [SerializeField] private float frameInterval = 0.1f;
    [SerializeField] private float deathFrameInterval = 0.15f;
    // Movement
    private float speed;
    private Vector3 targetPosition;
    private float casualtyChance;
    private Vector3 deathVelocity;
    // State tracking
    private bool isDying;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void Initialize(Sprite[] runSprites, Sprite[][] dieSprites, float moveSpeed, Vector3 target, float deathChance)
    {
        runFrames = runSprites;
        dieFrames = dieSprites;
        speed = moveSpeed;
        targetPosition = target;
        casualtyChance = deathChance;

        isDying = false;
        currentFrame = 0;
        frameTimer = 0f;
        deathVelocity = Vector3.zero;
        spriteRenderer.flipX = false;

        spriteRenderer.sprite = runFrames[0];
        gameObject.SetActive(true);
    }
    void Update()
    {
        float interval = isDying ? deathFrameInterval : frameInterval;
        frameTimer += Time.deltaTime;
        if (frameTimer >= interval)
        {
            frameTimer -= interval;
            currentFrame++;
            if (isDying)
            {
                if (currentFrame >= activeDieFrames.Length)
                {
                    Deactivate();
                    return;
                }
                spriteRenderer.sprite = activeDieFrames[currentFrame];
            }
            else
            {
                if (currentFrame >= runFrames.Length)
                    currentFrame = 0;
                spriteRenderer.sprite = runFrames[currentFrame];
            }

        }
        if (isDying)
        {
            // Carry forward momentum and decelerate — prevents the sudden "wall stop" on death
            transform.Translate(deathVelocity * Time.deltaTime, Space.World);
            deathVelocity = Vector3.Lerp(deathVelocity, Vector3.zero, Time.deltaTime * 4f);
        }
        else
        {
            // Random casualty chance — some soldiers die before reaching the target
            if (Random.value < casualtyChance * Time.deltaTime)
            {
                StartDying();
                return;
            }

            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
            if (Vector3.Distance(transform.position, targetPosition) < 0.3f)
                StartDying();
        }
    }

    private void StartDying()
    {
        isDying = true;
        currentFrame = 0;
        frameTimer = 0f;

        // Pick a random death animation row from the sheet
        activeDieFrames = dieFrames[Random.Range(0, dieFrames.Length)];

        // Carry forward momentum into death so the soldier doesn't stop dead
        Vector3 direction = (targetPosition - transform.position).normalized;
        deathVelocity = direction * speed * 0.4f;

        // Randomly flip to break up visual uniformity
        spriteRenderer.flipX = Random.value > 0.5f;

        spriteRenderer.sprite = activeDieFrames[0];
    }
    private void Deactivate()
    {
        gameObject.SetActive(false);
        SoldierVisualManager.Instance.ReturnToPool(this);
    }
}

