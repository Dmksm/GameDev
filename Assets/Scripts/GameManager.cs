using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private UIManager uiManager;
    private LevelGenerator levelGenerator;
    private LineManager lineManager;
    private BoardManager boardManager;
    
    private int currentLevel = 1;
    private int remainingLines = 3;
    private bool isInfiniteMode = false;

    private void Start()
    {
        Debug.Log("GameManager: Starting initialization");
        try {
            Camera.main.orthographicSize = 8.9f;
            Camera.main.transform.position = new Vector3(0, 0, -10);
            Debug.Log($"GameManager: Camera initialized. OrthographicSize: {Camera.main.orthographicSize}, Position: {Camera.main.transform.position}");
            SetupCamera();
            InitializeManagers();
            ShowLevelSelect();
            Debug.Log("GameManager: Initialization completed successfully");
        }
        catch (System.Exception e) {
            Debug.LogError($"GameManager: Error during initialization: {e.Message}\n{e.StackTrace}");
        }
    }

    private void SetupCamera()
    {
        Debug.Log("GameManager: Setting up camera");
        Camera.main.orthographic = true;
    }

    private void InitializeManagers()
    {
        Debug.Log("GameManager: Initializing managers");
        // Create UI elements first
        GameObject canvas = new GameObject("Canvas");
        canvas.transform.SetParent(transform);
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();

        // Create counter text
        GameObject counterObj = new GameObject("Lines Counter");
        counterObj.transform.SetParent(canvas.transform, false);
        TextMeshProUGUI counterText = counterObj.AddComponent<TextMeshProUGUI>();
        counterText.fontSize = 36;
        counterText.alignment = TextAlignmentOptions.TopRight;
        RectTransform counterRect = counterText.GetComponent<RectTransform>();
        counterRect.anchorMin = new Vector2(1, 1);
        counterRect.anchorMax = new Vector2(1, 1);
        counterRect.pivot = new Vector2(1, 1);
        counterRect.sizeDelta = new Vector2(200, 50);
        counterRect.anchoredPosition = new Vector2(-20, -20);

        // Create managers
        GameObject uiObj = new GameObject("UI Manager");
        uiObj.transform.SetParent(transform);
        uiManager = uiObj.AddComponent<UIManager>();

        GameObject levelObj = new GameObject("Level Generator");
        levelObj.transform.SetParent(transform);
        levelGenerator = levelObj.AddComponent<LevelGenerator>();

        GameObject lineObj = new GameObject("Line Manager");
        lineObj.transform.SetParent(transform);
        lineManager = lineObj.AddComponent<LineManager>();

        GameObject boardObj = new GameObject("Board Manager");
        boardObj.transform.SetParent(transform);
        boardManager = boardObj.AddComponent<BoardManager>();

        // Initialize managers
        uiManager.Initialize(this);
        levelGenerator.Initialize();
        lineManager.Initialize(this, boardManager);
        boardManager.Initialize();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPosition = lineManager.GetWorldPosition(Input.mousePosition);
            lineManager.StartDrawing(worldPosition);
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 worldPosition = lineManager.GetWorldPosition(Input.mousePosition);
            lineManager.UpdatePreview(worldPosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Vector3 worldPosition = lineManager.GetWorldPosition(Input.mousePosition);
            lineManager.FinishDrawing(worldPosition);
        }
    }

    public void LoadLevel(int level)
    {
        currentLevel = level;
        isInfiniteMode = false;
        // Calculate lines based on level complexity
        remainingLines = CalculateRemainingLines(level);
        
        ResetLevel();
        uiManager.HideAllScreens();
        uiManager.UpdateLinesCounter(remainingLines);
    }

    private int CalculateRemainingLines(int level)
    {
        // Progressive line calculation for 100 levels
        // Base lines start at 3
        // Level 1: 3 lines
        // Level 20: ~8 lines
        // Level 50: ~12 lines
        // Level 100: ~18 lines
        float baseLines = 3;
        float progression = level * 0.15f; // Slower progression than objects
        return Mathf.Max(3, Mathf.FloorToInt(baseLines + progression));
    }

    public void StartInfiniteMode()
    {
        isInfiniteMode = true;
        remainingLines = 3;
        currentLevel = 1; // Сбрасываем уровень
        
        ResetLevel();
        uiManager.HideAllScreens();
        uiManager.UpdateUI();
    }

    private void ResetLevel()
    {
        lineManager.ClearLines();
        levelGenerator.GenerateLevel(currentLevel);
    }

    public void RestartGame()
    {
        if (isInfiniteMode)
        {
            StartInfiniteMode();
        }
        else
        {
            LoadLevel(currentLevel);
        }
    }

    public void LoadNextLevel()
    {
        currentLevel++; // Увеличиваем текущий уровень
        remainingLines = CalculateRemainingLines(currentLevel);
        ResetLevel();
        uiManager.HideAllScreens();
        uiManager.UpdateUI();
    }

    public void ShowLevelSelect()
    {
        // Очищаем все объекты
        lineManager.ClearLines();
        levelGenerator.ClearLevel();
        
        uiManager.ShowLevelSelect();
    }

    public bool CanDrawLine()
    {
        return remainingLines > 0;
    }

    public void OnLineDrawn()
    {
        remainingLines--;
        uiManager.UpdateLinesCounter(remainingLines);
        
        if (remainingLines <= 0)
        {
            // Если линии закончились, проверяем условие победы
            if (CheckWinCondition())
            {
                WinLevel();
            }
            else
            {
                LoseLevel();
            }
        }
        else if (CheckWinCondition())
        {
            // Если еще есть линии и условие победы выполнено
            WinLevel();
        }
    }

    private bool CheckWinCondition()
    {
        var stars = levelGenerator.GetStars();
        var junks = levelGenerator.GetJunks();
        var drawnLines = lineManager.GetDrawnLines();

        if (drawnLines.Count == 0)
        {
            return false; // Нет линий - нет победы
        }

        // Проверяем, разделены ли звезды и мусор линиями
        foreach (var star in stars)
        {
            foreach (var junk in junks)
            {
                // Проверяем, есть ли линия между звездой и мусором
                if (!IsPathBlockedByLines(star.transform.position, junk.transform.position, drawnLines))
                {
                    return false; // Нашли путь между звездой и мусором, который не пересекается линиями
                }
            }
        }

        return true; // Все пути между звездами и мусором пересекаются линиями
    }

    private bool IsPathBlockedByLines(Vector2 start, Vector2 end, System.Collections.Generic.List<LineRenderer> lines)
    {
        foreach (var line in lines)
        {
            Vector2 lineStart = line.GetPosition(0);
            Vector2 lineEnd = line.GetPosition(1);

            if (DoLinesIntersect(start, end, lineStart, lineEnd))
            {
                return true;
            }
        }
        return false;
    }

    private bool DoLinesIntersect(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End)
    {
        float denominator = ((line2End.y - line2Start.y) * (line1End.x - line1Start.x)) -
                          ((line2End.x - line2Start.x) * (line1End.y - line1Start.y));

        if (denominator == 0)
            return false;

        float ua = (((line2End.x - line2Start.x) * (line1Start.y - line2Start.y)) -
                   ((line2End.y - line2Start.y) * (line1Start.x - line2Start.x))) / denominator;
        float ub = (((line1End.x - line1Start.x) * (line1Start.y - line2Start.y)) -
                   ((line1End.y - line1Start.y) * (line1Start.x - line2Start.x))) / denominator;

        return (ua >= 0 && ua <= 1) && (ub >= 0 && ub <= 1);
    }

    private void WinLevel()
    {
        if (isInfiniteMode)
        {
            uiManager.ShowWinScreen(); // Показываем экран победы
        }
        else
        {
            uiManager.ShowWinScreen();
        }
    }

    private void LoseLevel()
    {
        uiManager.ShowLoseScreen();
    }

    public void NextLevel()
    {
        if (isInfiniteMode)
        {
            currentLevel++; // Увеличиваем уровень здесь
            remainingLines = CalculateRemainingLines(currentLevel); // Вычисляем количество линий для нового уровня
            ResetLevel(); // Генерируем новый уровень
            uiManager.UpdateUI(); // Обновляем UI
            uiManager.HideWinScreen(); // Скрываем экран победы
        }
        else
        {
            LoadNextLevel();
        }
    }

    public int GetRemainingLines()
    {
        return remainingLines;
    }

    public bool IsInInfiniteMode()
    {
        return isInfiniteMode;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public void OnLevelComplete()
    {
        if (isInfiniteMode)
        {
            WinLevel(); // Просто показываем экран победы
        }
        else
        {
            // Логика для обычного режима
            WinLevel();
        }
    }

    public void ToggleHint()
    {
        levelGenerator.ToggleHints();
    }

    public void UndoMove()
    {
        if (lineManager.UndoLastLine())
        {
            remainingLines = Mathf.Min(remainingLines + 1, CalculateRemainingLines(currentLevel));
            uiManager.UpdateUI();
        }
    }
}