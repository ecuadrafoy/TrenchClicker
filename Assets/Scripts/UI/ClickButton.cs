using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ClickButton : MonoBehaviour
{
    private Button button;
    private Vector3 originalScale;

    [SerializeField] private float punchScale = 0.9f;
    [SerializeField] private float punchDuration = 0.1f;
    void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
    }
    void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }
    // Update is called once per frame
    void Update()
    {

    }
    private void OnButtonClick()
    {
        StopAllCoroutines();
        StartCoroutine(PunchAnimation());
    }
    private System.Collections.IEnumerator PunchAnimation()
    {
        float elapsed = 0f;
        // Scale Down
        while (elapsed < punchDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (punchDuration / 2f);
            transform.localScale = Vector3.Lerp(originalScale, originalScale * punchScale, t);
            yield return null;
        }
        // Scale back up
        elapsed = 0f;
        while (elapsed < punchDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (punchDuration / 2f);
            transform.localScale = Vector3.Lerp(originalScale * punchScale, originalScale, t);
            yield return null;
        }
        transform.localScale = originalScale;
    }
}
