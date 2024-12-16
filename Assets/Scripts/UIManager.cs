using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    private GameManager gameManager;
    private TextMeshProUGUI linesCounterText;
    private TextMeshProUGUI levelCounterText;
    private Button hintButton;
    private GameObject winPanel;
    private GameObject losePanel;
    private GameObject levelSelectPanel;
    private Button undoButton;

    public void Initialize(GameManager gm)
    {
        gameManager = gm;
        CreateUI();
    }

    public void HideWinScreen()
    {
        winPanel.SetActive(false);
    }

    /*public void OnNextLevelButton()
    {
        GameManager.NextLevel();
    }

    public void OnRestartButton()
    {
        GameManager.StartNewGame();
        HideWinScreen();
    }*/

    private void CreateUI()
    {
        // Setup canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        canvas.transform.SetParent(transform);

        // Get the canvas from GameManager
        //Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        //if (canvas == null)
        //{
        //    Debug.LogError("Canvas not found!");
        //    return;
        //}

        // Create panels
        winPanel = CreatePanel("Win Panel", canvas.transform);
        losePanel = CreatePanel("Lose Panel", canvas.transform);
        levelSelectPanel = CreatePanel("Level Select Panel", canvas.transform);

        // Setup lines counter
        GameObject linesCounter = new GameObject("Lines Counter");
        linesCounter.transform.SetParent(canvas.transform, false);
        linesCounterText = linesCounter.AddComponent<TextMeshProUGUI>();
        linesCounterText.fontSize = 36;
        linesCounterText.alignment = TextAlignmentOptions.Left;
        linesCounterText.color = Color.black;
        RectTransform counterRect = linesCounterText.GetComponent<RectTransform>();
        counterRect.anchorMin = new Vector2(0, 1);
        counterRect.anchorMax = new Vector2(0, 1);
        counterRect.pivot = new Vector2(0, 1);
        counterRect.sizeDelta = new Vector2(200, 50);
        counterRect.anchoredPosition = new Vector2(20, -20);

        // Setup hint button
        GameObject hintObj = new GameObject("Hint Button");
        hintObj.transform.SetParent(canvas.transform, false);
        hintButton = hintObj.AddComponent<Button>();
        
        // Добавляем изображение для кнопки
        Image buttonImage = hintObj.AddComponent<Image>();
        buttonImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        
        // Добавляем текст на кнопку
        GameObject buttonTextObj = new GameObject("Button Text");
        buttonTextObj.transform.SetParent(hintObj.transform);
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Show Hint";
        buttonText.fontSize = 24;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.black;
        
        // Настраиваем позицию кнопки
        RectTransform hintRect = hintObj.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 1);
        hintRect.anchorMax = new Vector2(0.5f, 1);
        hintRect.pivot = new Vector2(0.5f, 1);
        hintRect.sizeDelta = new Vector2(120, 40);
        hintRect.anchoredPosition = new Vector2(0, -20);
        
        // Настраиваем позицию текста кнопки
        RectTransform textRect = buttonTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        // Добавляем обработчик нажатия
        hintButton.onClick.AddListener(() => {
            if (gameManager != null)
            {
                gameManager.ToggleHint();
                buttonText.text = buttonText.text == "Show Hint" ? "Hide Hint" : "Show Hint";
            }
        });

        // Setup level counter
        GameObject levelCounterObj = new GameObject("Level Counter");
        levelCounterObj.transform.SetParent(canvas.transform, false);
        levelCounterText = levelCounterObj.AddComponent<TextMeshProUGUI>();
        levelCounterText.fontSize = 36;
        levelCounterText.alignment = TextAlignmentOptions.Right;
        levelCounterText.color = Color.black;
        RectTransform levelRect = levelCounterText.GetComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(1, 1);
        levelRect.anchorMax = new Vector2(1, 1);
        levelRect.pivot = new Vector2(1, 1);
        levelRect.sizeDelta = new Vector2(200, 50);
        levelRect.anchoredPosition = new Vector2(-20, -20);

        // Setup undo button
        GameObject undoObj = new GameObject("Undo Button");
        undoObj.transform.SetParent(canvas.transform, false);
        undoButton = undoObj.AddComponent<Button>();
        Image undoImage = undoObj.AddComponent<Image>();
        undoImage.color = new Color(0.7f, 0.7f, 0.7f, 1f); // Серый цвет

        // Create arrow symbol
        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(undoObj.transform, false);
        TextMeshProUGUI arrowText = arrowObj.AddComponent<TextMeshProUGUI>();
        arrowText.text = "←"; // Unicode стрелка влево
        arrowText.fontSize = 32;
        arrowText.alignment = TextAlignmentOptions.Center;
        arrowText.color = Color.black;

        // Position the undo button below level counter
        RectTransform undoRect = undoObj.GetComponent<RectTransform>();
        undoRect.anchorMin = new Vector2(1, 1);
        undoRect.anchorMax = new Vector2(1, 1);
        undoRect.pivot = new Vector2(1, 1);
        undoRect.sizeDelta = new Vector2(50, 50);
        undoRect.anchoredPosition = new Vector2(-20, -80);

        RectTransform arrowRect = arrowObj.GetComponent<RectTransform>();
        arrowRect.anchorMin = Vector2.zero;
        arrowRect.anchorMax = Vector2.one;
        arrowRect.sizeDelta = Vector2.zero;

        undoButton.onClick.AddListener(() => {
            if (gameManager != null)
            {
                gameManager.UndoMove();
            }
        });

        // Setup win panel
        CreatePanelContent(winPanel, "Level Complete!", "Next Level", "Level Select", () =>
        {
            gameManager.LoadNextLevel();
        }, () =>
        {
            gameManager.ShowLevelSelect();
        });

        // Setup lose panel
        CreatePanelContent(losePanel, "Game Over!", "Retry", "Level Select", () =>
        {
            gameManager.RestartGame();
        }, () =>
        {
            gameManager.ShowLevelSelect();
        });

        // Setup level select panel
        SetupLevelSelectPanel();
    }

    public void UpdateUI()
    {
        if (gameManager != null)
        {
            linesCounterText.text = $"Lines: {gameManager.GetRemainingLines()}";
            
            // Обновляем отображение уровня и кнопки отмены в бесконечном режиме
            if (gameManager.IsInInfiniteMode())
            {
                levelCounterText.text = $"Level: {gameManager.GetCurrentLevel()}";
                levelCounterText.gameObject.SetActive(true);
                undoButton.gameObject.SetActive(true);
            }
            else
            {
                levelCounterText.gameObject.SetActive(false);
                undoButton.gameObject.SetActive(false);
            }
        }
    }

    private GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.2f, 0.2f);
        rectTransform.anchorMax = new Vector2(0.8f, 0.8f);
        rectTransform.sizeDelta = Vector2.zero;
        
        panel.SetActive(false);
        return panel;
    }

    private void CreatePanelContent(GameObject panel, string title, string button1Text, string button2Text, UnityAction button1Action, UnityAction button2Action)
    {
        // Добавляем фон
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.9f, 0.9f, 0.9f, 0.95f);

        // Создаем заголовок
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = title;
        titleText.fontSize = 48;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.black;

        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.6f);
        titleRect.anchorMax = new Vector2(0.9f, 0.9f);
        titleRect.sizeDelta = Vector2.zero;

        // Создаем кнопку Next Level
        GameObject button1Obj = new GameObject(button1Text + " Button");
        button1Obj.transform.SetParent(panel.transform, false);
        Button button1 = button1Obj.AddComponent<Button>();
        Image button1Image = button1Obj.AddComponent<Image>();
        button1Image.color = new Color(0.2f, 0.6f, 1f, 1f);

        GameObject button1TextObj = new GameObject("Button Text");
        button1TextObj.transform.SetParent(button1Obj.transform, false);
        TextMeshProUGUI button1TMPro = button1TextObj.AddComponent<TextMeshProUGUI>();
        button1TMPro.text = button1Text;
        button1TMPro.fontSize = 32;
        button1TMPro.alignment = TextAlignmentOptions.Center;
        button1TMPro.color = Color.white;

        RectTransform button1Rect = button1.GetComponent<RectTransform>();
        button1Rect.anchorMin = new Vector2(0.2f, 0.35f);
        button1Rect.anchorMax = new Vector2(0.8f, 0.5f);
        button1Rect.sizeDelta = Vector2.zero;

        RectTransform button1TextRect = button1TMPro.GetComponent<RectTransform>();
        button1TextRect.anchorMin = Vector2.zero;
        button1TextRect.anchorMax = Vector2.one;
        button1TextRect.sizeDelta = Vector2.zero;

        // Создаем кнопку Level Select
        GameObject button2Obj = new GameObject(button2Text + " Button");
        button2Obj.transform.SetParent(panel.transform, false);
        Button button2 = button2Obj.AddComponent<Button>();
        Image button2Image = button2Obj.AddComponent<Image>();
        button2Image.color = new Color(0.7f, 0.7f, 0.7f, 1f);

        GameObject button2TextObj = new GameObject("Button Text");
        button2TextObj.transform.SetParent(button2Obj.transform, false);
        TextMeshProUGUI button2TMPro = button2TextObj.AddComponent<TextMeshProUGUI>();
        button2TMPro.text = button2Text;
        button2TMPro.fontSize = 32;
        button2TMPro.alignment = TextAlignmentOptions.Center;
        button2TMPro.color = Color.white;

        RectTransform button2Rect = button2.GetComponent<RectTransform>();
        button2Rect.anchorMin = new Vector2(0.2f, 0.15f);
        button2Rect.anchorMax = new Vector2(0.8f, 0.3f);
        button2Rect.sizeDelta = Vector2.zero;

        RectTransform button2TextRect = button2TMPro.GetComponent<RectTransform>();
        button2TextRect.anchorMin = Vector2.zero;
        button2TextRect.anchorMax = Vector2.one;
        button2TextRect.sizeDelta = Vector2.zero;

        // Добавляем обработчики нажатий
        button1.onClick.AddListener(button1Action);
        button2.onClick.AddListener(button2Action);
    }

    private void CreateButton(Transform parent, string text, Vector2 anchorPosition, UnityAction action)
    {
        GameObject buttonObj = new GameObject(text + " Button");
        buttonObj.transform.SetParent(parent, false);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(1, 1, 1, 0.9f);

        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(action);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 24;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.black;

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(anchorPosition.x - 0.15f, anchorPosition.y - 0.05f);
        buttonRect.anchorMax = new Vector2(anchorPosition.x + 0.15f, anchorPosition.y + 0.05f);
        buttonRect.sizeDelta = Vector2.zero;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }

    private void SetupLevelSelectPanel()
    {
        // Create title
        GameObject titleObj = new GameObject("Level Select Title");
        titleObj.transform.SetParent(levelSelectPanel.transform, false);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "Select Mode";
        title.fontSize = 48;
        title.alignment = TextAlignmentOptions.Center;

        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.85f);
        titleRect.anchorMax = new Vector2(0.5f, 0.95f);
        titleRect.sizeDelta = new Vector2(400, 100);
        titleRect.anchoredPosition = Vector2.zero;

        // Create infinite mode button in the center
        GameObject buttonObj = new GameObject("Infinite Mode Button");
        buttonObj.transform.SetParent(levelSelectPanel.transform, false);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = Color.black;

        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(() => gameManager.StartInfiniteMode());

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Infinite Mode";
        buttonText.fontSize = 32;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.3f, 0.45f);
        buttonRect.anchorMax = new Vector2(0.7f, 0.55f);
        buttonRect.sizeDelta = Vector2.zero;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }

    private void CreateLevelButton(Transform parent, int level, UnityAction action)
    {
        GameObject buttonObj = new GameObject("Level " + level + " Button");
        buttonObj.transform.SetParent(parent, false);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(1, 1, 1, 0.9f);

        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(action);

        // Create level number
        GameObject numberObj = new GameObject("Number");
        numberObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI numberText = numberObj.AddComponent<TextMeshProUGUI>();
        numberText.text = level.ToString();
        numberText.fontSize = 48;
        numberText.alignment = TextAlignmentOptions.Center;
        numberText.color = Color.black;

        RectTransform numberRect = numberObj.GetComponent<RectTransform>();
        numberRect.anchorMin = Vector2.zero;
        numberRect.anchorMax = Vector2.one;
        numberRect.sizeDelta = Vector2.zero;

        // Create "Level" text
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = "Level";
        labelText.fontSize = 24;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = Color.black;

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.7f);
        labelRect.anchorMax = new Vector2(1, 0.9f);
        labelRect.sizeDelta = Vector2.zero;

        // Adjust number position
        numberRect.anchorMin = new Vector2(0, 0.2f);
        numberRect.anchorMax = new Vector2(1, 0.7f);
    }

    public void ShowWinScreen()
    {
        HideAllScreens();
        winPanel.SetActive(true);
    }

    public void ShowLoseScreen()
    {
        HideAllScreens();
        losePanel.SetActive(true);
    }

    public void ShowLevelSelect()
    {
        HideAllScreens();
        levelSelectPanel.SetActive(true);
    }

    public void HideAllScreens()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        levelSelectPanel.SetActive(false);
    }

    public void UpdateLinesCounter(int remainingLines)
    {
        if (linesCounterText != null)
        {
            linesCounterText.text = "Lines: " + remainingLines;
        }
    }
}