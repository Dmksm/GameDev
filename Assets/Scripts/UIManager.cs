using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    private GameManager gameManager;
    private TextMeshProUGUI linesCounterText;
    private GameObject winPanel;
    private GameObject losePanel;
    private GameObject levelSelectPanel;

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
        // Get the canvas from GameManager
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        // Create panels
        winPanel = CreatePanel("Win Panel", canvas.transform);
        losePanel = CreatePanel("Lose Panel", canvas.transform);
        levelSelectPanel = CreatePanel("Level Select Panel", canvas.transform);

        // Setup lines counter
        GameObject counterObj = new GameObject("Lines Counter");
        counterObj.transform.SetParent(canvas.transform, false);
        linesCounterText = counterObj.AddComponent<TextMeshProUGUI>();
        linesCounterText.fontSize = 36;
        linesCounterText.alignment = TextAlignmentOptions.Left;
        linesCounterText.color = Color.black;
        RectTransform counterRect = linesCounterText.GetComponent<RectTransform>();
        counterRect.anchorMin = new Vector2(0, 1);
        counterRect.anchorMax = new Vector2(0, 1);
        counterRect.pivot = new Vector2(0, 1);
        counterRect.sizeDelta = new Vector2(200, 50);
        counterRect.anchoredPosition = new Vector2(20, -20);

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

    private GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.9f);

        panel.SetActive(false);
        return panel;
    }

    private void CreatePanelContent(GameObject panel, string titleText, string button1Text, string button2Text, UnityEngine.Events.UnityAction button1Action, UnityEngine.Events.UnityAction button2Action)
    {
        // Create title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = titleText;
        title.fontSize = 48;
        title.alignment = TextAlignmentOptions.Center;

        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.sizeDelta = new Vector2(400, 100);
        titleRect.anchoredPosition = Vector2.zero;

        // Create buttons
        CreateButton(panel.transform, button1Text, new Vector2(0.5f, 0.4f), button1Action);
        CreateButton(panel.transform, button2Text, new Vector2(0.5f, 0.2f), button2Action);
    }

    private void CreateButton(Transform parent, string text, Vector2 anchorPosition, UnityEngine.Events.UnityAction action)
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
        title.text = "Select Level";
        title.fontSize = 48;
        title.alignment = TextAlignmentOptions.Center;

        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.85f);
        titleRect.anchorMax = new Vector2(0.5f, 0.95f);
        titleRect.sizeDelta = new Vector2(400, 100);
        titleRect.anchoredPosition = Vector2.zero;

        // Create a grid container for level buttons
        GameObject gridContainer = new GameObject("Level Grid");
        gridContainer.transform.SetParent(levelSelectPanel.transform, false);
        RectTransform gridRect = gridContainer.AddComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0.1f, 0.2f);
        gridRect.anchorMax = new Vector2(0.9f, 0.8f);
        gridRect.sizeDelta = Vector2.zero;

        GridLayoutGroup grid = gridContainer.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(160, 160);
        grid.spacing = new Vector2(20, 20);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.Flexible;

        // Create level buttons
        for (int i = 0; i < 5; i++)
        {
            int level = i + 1;
            CreateLevelButton(gridContainer.transform, level, () => gameManager.LoadLevel(level));
        }

        // Create infinite mode button at the bottom
        CreateButton(levelSelectPanel.transform, 
            "Infinite Mode", 
            new Vector2(0.5f, 0.1f), 
            () => gameManager.StartInfiniteMode());
    }

    private void CreateLevelButton(Transform parent, int level, UnityEngine.Events.UnityAction action)
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
