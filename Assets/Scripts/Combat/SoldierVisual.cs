
using UnityEngine;

public class SoldierVisual : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private Sprite[] runFrames;
    private Sprite[] dieFrames;
    // Animation state
    private int currentFrame;
    private float frameTimer;
    private float frameInterval = 0.1f;
    // Movement
    private float speed;
    private Vector3 targetPosition;
    //State tracking
    private bool isDying;
    private bool isActive;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void Initialize(Sprite[] runSprites, Sprite[] dieSprites, float moveSpeed, Vector3 target, float yOffset)
    {
        runFrames = runSprites;
        dieFrames = dieSprites;
        speed = moveSpeed;
        targetPosition = target;

        isDying = false;
        isActive = true;
        currentFrame = 0;
        frameTimer = 0f;
        //Position solider at its spawn point
        Vector3 spawnPos = transform.position;
        spawnPos.y += yOffset;
        transform.position = spawnPos;

        spriteRenderer.sprite = runFrames[0];
        gameObject.SetActive(true);
    }
    void Update()
    {
        if (!isActive) return;

        frameTimer += Time.deltaTime;
        if (frameTimer >= frameInterval)
        {
            frameTimer -= frameInterval;
            currentFrame++;
            if (isDying)
            {
                if (currentFrame >= dieFrames.Length)
                {
                    Deactivate();
                    return;
                }
                spriteRenderer.sprite = dieFrames[currentFrame];
            }
            else
            {
                if (currentFrame >= runFrames.Length)
                    currentFrame = 0;
                spriteRenderer.sprite = runFrames[currentFrame];
            }

        }
        // Only move while running, not while dying
        if (!isDying)
        {
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
        spriteRenderer.sprite = dieFrames[0];
    }
    private void Deactivate()
    {
        isActive = false;
        gameObject.SetActive(false);
        SoldierVisualManager.Instance.ReturnToPool(this);
    }
}

