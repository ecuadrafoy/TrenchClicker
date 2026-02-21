
using UnityEngine;

public class SoldierVisual : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private enum SoldierState { Running, Attacking, Dying }
    private SoldierState state;
    private Sprite[] activeRunFrames;
    private Sprite[] attackFrames;
    private Sprite[][] dieFrames;
    private Sprite[] activeDieFrames;
    // Animation state
    private int currentFrame;
    private float frameTimer;
    private float attackTimer;
    [SerializeField] private float frameInterval = 0.1f;
    [SerializeField] private float deathFrameInterval = 0.15f;
    [SerializeField] private float attackDuration = 3f;
    // Movement
    private float speed;
    private Vector3 targetPosition;
    private float casualtyChance;
    private Vector3 deathVelocity;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void Initialize(Sprite[] runSprites, Sprite[] attackSprites, Sprite[][] dieSprites, float moveSpeed, Vector3 target, float deathChance)
    {
        activeRunFrames = runSprites;
        attackFrames = attackSprites;
        dieFrames = dieSprites;
        speed = moveSpeed;
        targetPosition = target;
        casualtyChance = deathChance;

        state = SoldierState.Running;

        currentFrame = 0;
        frameTimer = 0f;
        deathVelocity = Vector3.zero;
        spriteRenderer.flipX = false;

        spriteRenderer.sprite = activeRunFrames[0];
        gameObject.SetActive(true);
    }
    void Update()
    {
        float interval = (state == SoldierState.Dying) ? deathFrameInterval : frameInterval;
        frameTimer += Time.deltaTime;
        if (frameTimer >= interval)
        {
            frameTimer -= interval;
            currentFrame++;
            switch (state)
            {
                case SoldierState.Running:
                    if (currentFrame >= activeRunFrames.Length) currentFrame = 0;
                    spriteRenderer.sprite = activeRunFrames[currentFrame];
                    break;
                case SoldierState.Attacking:
                    if (currentFrame >= attackFrames.Length) currentFrame = 0;
                    spriteRenderer.sprite = attackFrames[currentFrame];
                    break;
                case SoldierState.Dying:
                    if (currentFrame >= activeDieFrames.Length)
                    {
                        Deactivate();
                        return;
                    }
                    spriteRenderer.sprite = activeDieFrames[currentFrame];
                    break;
            }
        }
        else if (state == SoldierState.Attacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackDuration)
                Deactivate();
        }
        if (state == SoldierState.Dying)
        {
            // Carry forward momentum and decelerate — prevents the sudden "wall stop" on death
            transform.Translate(deathVelocity * Time.deltaTime, Space.World);
            deathVelocity = Vector3.Lerp(deathVelocity, Vector3.zero, Time.deltaTime * 4f);
        }
        else
            if (state == SoldierState.Running)
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
                    StartAttacking();
            }
    }

    private void StartDying()
    {
        state = SoldierState.Dying;
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
    private void StartAttacking()
    {
        state = SoldierState.Attacking;
        currentFrame = 0;
        frameTimer = 0f;
        attackTimer = 0f;
        spriteRenderer.sprite = attackFrames[0];
    }
    public void ForceDeactivate()
    {
        Deactivate();
    }
    private void Deactivate()
    {
        gameObject.SetActive(false);
        SoldierVisualManager.Instance.ReturnToPool(this);
    }
}

