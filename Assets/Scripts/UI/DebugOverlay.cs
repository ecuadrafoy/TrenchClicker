using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Debug overlay that programmatically builds a Canvas UI for runtime inspection and control.
/// Toggle with F1. No prefabs or scene setup required — just add to any GameObject.
/// </summary>
public class DebugOverlay : MonoBehaviour
{
    public static DebugOverlay Instance { get; private set; }

    private Canvas canvas;
    private GameObject panelRoot;
    private bool isVisible = false;

    // Performance tracking
    private float fpsTimer = 0f;
    private int fpsFrameCount = 0;
    private float currentFPS = 0f;

    // UI references for updating
    private Text fpsText;
    private Text frameTimeText;
    private Text memoryText;

    private Text soldiersText;
    private Text groundGainedText;
    private Text enemyHPText;
    private Text maxReinforcedHPText;
    private Text assaultActiveText;
    private Text isReinforcingText;
    private Text timeRemainingText;
    private Text soldiersPerClickText;
    private Text damageRangeText;
    private Text trenchesCapturedText;
    private Text reinforcementRateText;
    private Text weatherStateText;
    private Text weatherEffText;
    private Text weatherTableText;
    private Text weatherDamageText;

    private Slider damageMinSlider;
    private Slider damageMaxSlider;
    private Slider timeScaleSlider;
    private InputField soldiersPerClickInput;
    private InputField addGroundInput;
    private Text timeScaleLabel;

    // Layout constants
    private const float PANEL_WIDTH = 320f;
    private const float ROW_HEIGHT = 22f;
    private const float SECTION_SPACING = 8f;
    private const float PADDING = 8f;
    private const int FONT_SIZE = 13;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        BuildUI();
        panelRoot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            isVisible = !isVisible;
            panelRoot.SetActive(isVisible);
        }

        if (!isVisible) return;

        UpdatePerformanceMetrics();
        UpdateGameStateDisplay();
    }

    // --- UI CONSTRUCTION ---

    private void BuildUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("DebugCanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Panel root with semi-transparent background
        panelRoot = CreatePanel(canvasObj.transform);

        // ScrollView inside the panel
        var scrollContent = CreateScrollView(panelRoot.transform);

        float y = 0f;

        // === PERFORMANCE SECTION ===
        y = AddSectionHeader(scrollContent, "PERFORMANCE", y);
        fpsText = AddLabel(scrollContent, "FPS: --", ref y);
        frameTimeText = AddLabel(scrollContent, "Frame: -- ms", ref y);
        memoryText = AddLabel(scrollContent, "Memory: -- MB", ref y);
        y += SECTION_SPACING;

        // === GAME STATE SECTION ===
        y = AddSectionHeader(scrollContent, "GAME STATE", y);
        soldiersText = AddLabel(scrollContent, "Soldiers: 0", ref y);
        groundGainedText = AddLabel(scrollContent, "Ground: 0.0 in", ref y);
        enemyHPText = AddLabel(scrollContent, "Enemy HP: 0/0", ref y);
        maxReinforcedHPText = AddLabel(scrollContent, "Max Reinforced HP: 0", ref y);
        assaultActiveText = AddLabel(scrollContent, "Assault: false", ref y);
        isReinforcingText = AddLabel(scrollContent, "Reinforcing: false", ref y);
        timeRemainingText = AddLabel(scrollContent, "Time Left: 0s", ref y);
        soldiersPerClickText = AddLabel(scrollContent, "Soldiers/Click: 1", ref y);
        damageRangeText = AddLabel(scrollContent, "Damage: 0.0-1.0", ref y);
        trenchesCapturedText = AddLabel(scrollContent, "Trenches: 0", ref y);
        reinforcementRateText = AddLabel(scrollContent, "Reinf. Rate: 0/s", ref y);
        weatherStateText = AddLabel(scrollContent, "Weather: Clear", ref y);
        weatherEffText = AddLabel(scrollContent, "Weather Eff: 100%", ref y);
        weatherTableText = AddLabel(scrollContent, "Weather Table: 0/0", ref y);
        weatherDamageText = AddLabel(scrollContent, "Last Dmg: 0.0 raw → 0.0 mod", ref y);
        y += SECTION_SPACING;

        // === CONTROLS SECTION ===
        y = AddSectionHeader(scrollContent, "CONTROLS", y);
        damageMinSlider = AddSlider(scrollContent, "Dmg Min", 0f, 10f, 0f, ref y, OnDamageMinChanged);
        damageMaxSlider = AddSlider(scrollContent, "Dmg Max", 0f, 10f, 1f, ref y, OnDamageMaxChanged);

        AddInputFieldRow(scrollContent, "Soldiers/Click:", ref y, out soldiersPerClickInput, OnSoldiersPerClickSubmit);
        AddInputFieldRow(scrollContent, "Add Ground:", ref y, out addGroundInput, OnAddGroundSubmit);

        timeScaleSlider = AddSlider(scrollContent, "TimeScale", 0.1f, 5f, 1f, ref y, OnTimeScaleChanged);
        timeScaleLabel = timeScaleSlider.transform.parent.GetComponentInChildren<Text>();
        y += SECTION_SPACING;

        // === QUICK ACTIONS SECTION ===
        y = AddSectionHeader(scrollContent, "QUICK ACTIONS", y);
        AddButton(scrollContent, "Capture Trench", ref y, OnCaptureTrench);
        AddButton(scrollContent, "Add 1000 Ground", ref y, OnAdd1000Ground);
        AddButton(scrollContent, "Reset Timer", ref y, OnResetTimer);
        AddButton(scrollContent, "Toggle Reinforcements", ref y, OnToggleReinforcements);
        AddButton(scrollContent, "Set HP to 1", ref y, OnSetHPTo1);
        AddButton(scrollContent, "Cycle Weather", ref y, OnCycleWeather);

        // Set content size
        var contentRT = scrollContent.GetComponent<RectTransform>();
        contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, y + PADDING);
    }

    private GameObject CreatePanel(Transform parent)
    {
        var panel = new GameObject("DebugPanel");
        panel.transform.SetParent(parent, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(PANEL_WIDTH, 0);

        var img = panel.AddComponent<Image>();
        img.color = new Color(0.05f, 0.05f, 0.1f, 0.85f);
        return panel;
    }

    private Transform CreateScrollView(Transform parent)
    {
        // ScrollRect container
        var scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(parent, false);
        var scrollRT = scrollObj.AddComponent<RectTransform>();
        scrollRT.anchorMin = Vector2.zero;
        scrollRT.anchorMax = Vector2.one;
        scrollRT.offsetMin = new Vector2(PADDING, PADDING);
        scrollRT.offsetMax = new Vector2(-PADDING, -PADDING);

        var scrollRect = scrollObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 20f;

        // Add mask
        var maskImg = scrollObj.AddComponent<Image>();
        maskImg.color = new Color(0, 0, 0, 0.01f); // nearly invisible, needed for mask
        scrollObj.AddComponent<Mask>().showMaskGraphic = false;

        // Viewport is the scroll object itself
        scrollRect.viewport = scrollRT;

        // Content container
        var content = new GameObject("Content");
        content.transform.SetParent(scrollObj.transform, false);
        var contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0, 1);
        contentRT.anchoredPosition = Vector2.zero;
        contentRT.sizeDelta = new Vector2(0, 1000); // will be resized after building

        scrollRect.content = contentRT;

        return content.transform;
    }

    private float AddSectionHeader(Transform parent, string title, float y)
    {
        var obj = new GameObject("Header_" + title);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(0, -y);
        rt.sizeDelta = new Vector2(0, ROW_HEIGHT + 2);

        var text = obj.AddComponent<Text>();
        text.text = $"--- {title} ---";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = FONT_SIZE;
        text.fontStyle = FontStyle.Bold;
        text.color = new Color(0.4f, 0.8f, 1f);
        text.alignment = TextAnchor.MiddleCenter;

        return y + ROW_HEIGHT + 4;
    }

    private Text AddLabel(Transform parent, string defaultText, ref float y)
    {
        var obj = new GameObject("Label");
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(4, -y);
        rt.sizeDelta = new Vector2(-8, ROW_HEIGHT);

        var text = obj.AddComponent<Text>();
        text.text = defaultText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = FONT_SIZE;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;

        y += ROW_HEIGHT;
        return text;
    }

    private Slider AddSlider(Transform parent, string label, float min, float max, float defaultVal, ref float y, UnityEngine.Events.UnityAction<float> callback)
    {
        // Label
        var labelObj = new GameObject("SliderLabel_" + label);
        labelObj.transform.SetParent(parent, false);
        var labelRT = labelObj.AddComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0, 1);
        labelRT.anchorMax = new Vector2(0.35f, 1);
        labelRT.pivot = new Vector2(0, 1);
        labelRT.anchoredPosition = new Vector2(4, -y);
        labelRT.sizeDelta = new Vector2(0, ROW_HEIGHT);

        var labelText = labelObj.AddComponent<Text>();
        labelText.text = $"{label}: {defaultVal:F1}";
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = FONT_SIZE;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleLeft;

        // Slider
        var sliderObj = CreateSliderObject(parent, y, min, max, defaultVal);
        var slider = sliderObj.GetComponent<Slider>();
        slider.onValueChanged.AddListener((val) =>
        {
            labelText.text = $"{label}: {val:F1}";
            callback(val);
        });

        y += ROW_HEIGHT + 4;
        return slider;
    }

    private GameObject CreateSliderObject(Transform parent, float y, float min, float max, float defaultVal)
    {
        var sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(parent, false);
        var sliderRT = sliderObj.AddComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0.36f, 1);
        sliderRT.anchorMax = new Vector2(1, 1);
        sliderRT.pivot = new Vector2(0, 1);
        sliderRT.anchoredPosition = new Vector2(0, -y);
        sliderRT.sizeDelta = new Vector2(-8, ROW_HEIGHT);

        // Background
        var bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        var bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0, 0.4f);
        bgRT.anchorMax = new Vector2(1, 0.6f);
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f);

        // Fill Area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        var fillAreaRT = fillArea.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0, 0.4f);
        fillAreaRT.anchorMax = new Vector2(1, 0.6f);
        fillAreaRT.offsetMin = Vector2.zero;
        fillAreaRT.offsetMax = Vector2.zero;

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRT = fill.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.3f, 0.7f, 1f);

        // Handle
        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        var handleAreaRT = handleArea.AddComponent<RectTransform>();
        handleAreaRT.anchorMin = Vector2.zero;
        handleAreaRT.anchorMax = Vector2.one;
        handleAreaRT.offsetMin = Vector2.zero;
        handleAreaRT.offsetMax = Vector2.zero;

        var handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        var handleRT = handle.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(12, 0);
        var handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        var slider = sliderObj.AddComponent<Slider>();
        slider.targetGraphic = handleImg;
        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = defaultVal;

        return sliderObj;
    }

    private void AddInputFieldRow(Transform parent, string label, ref float y, out InputField inputField, UnityEngine.Events.UnityAction<string> onSubmit)
    {
        // Label
        var labelObj = new GameObject("InputLabel_" + label);
        labelObj.transform.SetParent(parent, false);
        var labelRT = labelObj.AddComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0, 1);
        labelRT.anchorMax = new Vector2(0.45f, 1);
        labelRT.pivot = new Vector2(0, 1);
        labelRT.anchoredPosition = new Vector2(4, -y);
        labelRT.sizeDelta = new Vector2(0, ROW_HEIGHT);

        var labelText = labelObj.AddComponent<Text>();
        labelText.text = label;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = FONT_SIZE;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleLeft;

        // Input field background
        var inputObj = new GameObject("InputField");
        inputObj.transform.SetParent(parent, false);
        var inputRT = inputObj.AddComponent<RectTransform>();
        inputRT.anchorMin = new Vector2(0.46f, 1);
        inputRT.anchorMax = new Vector2(0.75f, 1);
        inputRT.pivot = new Vector2(0, 1);
        inputRT.anchoredPosition = new Vector2(0, -y);
        inputRT.sizeDelta = new Vector2(0, ROW_HEIGHT);

        var inputBg = inputObj.AddComponent<Image>();
        inputBg.color = new Color(0.15f, 0.15f, 0.2f);

        // Input text
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(inputObj.transform, false);
        var textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(4, 0);
        textRT.offsetMax = new Vector2(-4, 0);

        var inputText = textObj.AddComponent<Text>();
        inputText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        inputText.fontSize = FONT_SIZE;
        inputText.color = Color.white;
        inputText.supportRichText = false;

        inputField = inputObj.AddComponent<InputField>();
        inputField.textComponent = inputText;
        inputField.contentType = InputField.ContentType.DecimalNumber;

        // Apply button
        var btnObj = new GameObject("ApplyBtn");
        btnObj.transform.SetParent(parent, false);
        var btnRT = btnObj.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.76f, 1);
        btnRT.anchorMax = new Vector2(1, 1);
        btnRT.pivot = new Vector2(0, 1);
        btnRT.anchoredPosition = new Vector2(0, -y);
        btnRT.sizeDelta = new Vector2(-4, ROW_HEIGHT);

        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.5f, 0.2f);

        var btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        var btnTextRT = btnTextObj.AddComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = Vector2.zero;
        btnTextRT.offsetMax = Vector2.zero;

        var btnText = btnTextObj.AddComponent<Text>();
        btnText.text = "Set";
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = FONT_SIZE;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        var inputFieldRef = inputField;
        btn.onClick.AddListener(() => onSubmit(inputFieldRef.text));

        y += ROW_HEIGHT + 4;
    }

    private void AddButton(Transform parent, string label, ref float y, UnityEngine.Events.UnityAction callback)
    {
        var btnObj = new GameObject("Btn_" + label);
        btnObj.transform.SetParent(parent, false);
        var btnRT = btnObj.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0, 1);
        btnRT.anchorMax = new Vector2(1, 1);
        btnRT.pivot = new Vector2(0, 1);
        btnRT.anchoredPosition = new Vector2(4, -y);
        btnRT.sizeDelta = new Vector2(-8, ROW_HEIGHT + 4);

        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.25f, 0.25f, 0.35f);

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        var text = textObj.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = FONT_SIZE;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.35f, 0.35f, 0.5f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.25f);
        btn.colors = colors;
        btn.onClick.AddListener(callback);

        y += ROW_HEIGHT + 6;
    }

    // --- UPDATE METHODS ---

    private void UpdatePerformanceMetrics()
    {
        fpsFrameCount++;
        fpsTimer += Time.unscaledDeltaTime;
        if (fpsTimer >= 0.5f)
        {
            currentFPS = fpsFrameCount / fpsTimer;
            fpsTimer = 0f;
            fpsFrameCount = 0;
        }

        fpsText.text = $"FPS: {currentFPS:F0}";
        frameTimeText.text = $"Frame: {Time.unscaledDeltaTime * 1000f:F1} ms";

        float memoryMB = System.GC.GetTotalMemory(false) / (1024f * 1024f);
        memoryText.text = $"Memory: {memoryMB:F1} MB (GC)";
    }

    private void UpdateGameStateDisplay()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        soldiersText.text = $"Soldiers: {gm.GetTotalSoldiers()}";
        groundGainedText.text = $"Ground: {gm.GetgroundGained():F2} in";
        enemyHPText.text = $"Enemy HP: {gm.GetCurrentEnemyHP():F1} / {gm.GetMaxEnemyHP()}";
        maxReinforcedHPText.text = $"Max Reinforced HP: {gm.GetMaxReinforcedHP():F1}";
        assaultActiveText.text = $"Assault: {gm.IsAssaultActive()}";
        isReinforcingText.text = $"Reinforcing: {gm.IsReinforcing()}";
        timeRemainingText.text = $"Time Left: {gm.GetAssaultTimeRemaining():F1}s";
        soldiersPerClickText.text = $"Soldiers/Click: {gm.GetSoldierPerClick()}";
        damageRangeText.text = $"Damage: {gm.SoldierDamageMin:F1} - {gm.SoldierDamageMax:F1}";
        trenchesCapturedText.text = $"Trenches: {gm.GetTrenchesCaptured()}";
        reinforcementRateText.text = $"Reinf. Rate: {gm.GetReinforcementRate():F1}/s";
        weatherStateText.text = $"Weather: {WeatherConfig.GetDisplayName(gm.GetCurrentWeather())}";
        weatherEffText.text = $"Weather Eff: {gm.GetWeatherEffectiveness() * 100f:F0}%";
        weatherTableText.text = $"Weather Table: {gm.GetWeatherTableIndex()}/{gm.GetWeatherTableCount()}";
        weatherDamageText.text = $"Last Dmg: {gm.GetLastRawDamage():F1} raw → {gm.GetLastWeatherDamage():F1} mod";
    }

    // --- CONTROL CALLBACKS ---

    private void OnDamageMinChanged(float val)
    {
        GameManager.Instance?.SetSoldierDamageMin(val);
    }

    private void OnDamageMaxChanged(float val)
    {
        GameManager.Instance?.SetSoldierDamageMax(val);
    }

    private void OnSoldiersPerClickSubmit(string val)
    {
        if (int.TryParse(val, out int result))
        {
            GameManager.Instance?.SetSoldiersPerClick(result);
        }
    }

    private void OnAddGroundSubmit(string val)
    {
        if (float.TryParse(val, out float result))
        {
            GameManager.Instance?.AddGroundGained(result);
        }
    }

    private void OnTimeScaleChanged(float val)
    {
        Time.timeScale = val;
    }

    // --- QUICK ACTION CALLBACKS ---

    private void OnCaptureTrench()
    {
        GameManager.Instance?.SetCurrentEnemyHP(0f);
    }

    private void OnAdd1000Ground()
    {
        GameManager.Instance?.AddGroundGained(1000f);
    }

    private void OnResetTimer()
    {
        GameManager.Instance?.ResetAssaultTimer();
    }

    private void OnToggleReinforcements()
    {
        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.SetIsReinforcing(!gm.IsReinforcing());
        }
    }

    private void OnSetHPTo1()
    {
        GameManager.Instance?.SetCurrentEnemyHP(1f);
    }

    private void OnCycleWeather()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        // Cycle through enum values: Clear -> PartlyCloudy -> ... -> HeavyRain -> Clear
        int next = ((int)gm.GetCurrentWeather() + 1) % 5;
        gm.SetWeather((WeatherState)next);
    }
}
