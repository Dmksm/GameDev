using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    private const float BOARD_WIDTH = 8f;
    private const float BOARD_HEIGHT = 12f;
    private const float MIN_OBJECT_SPACING = 1f;

    private List<GameObject> stars = new List<GameObject>();
    private List<GameObject> junks = new List<GameObject>();
    private GameObject board;

    public void Initialize()
    {
        CreateBoard();
    }

    private void CreateBoard()
    {
        board = GameObject.CreatePrimitive(PrimitiveType.Quad);
        board.transform.SetParent(transform);
        board.transform.localScale = new Vector3(BOARD_WIDTH, BOARD_HEIGHT, 1);
        board.transform.position = new Vector3(0, 0, 1);
        
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = Color.white;
        board.GetComponent<Renderer>().material = material;
    }

    public void GenerateLevel(int level)
    {
        ClearLevel();

        // Определяем количество объектов для уровня
        int numStars = Mathf.Min(1 + level / 2, 3); // От 1 до 3 звезд
        int numJunks = Mathf.Min(2 + level / 2, 4); // От 2 до 4 мусора

        // Создаем список всех позиций
        List<Vector2> positions = new List<Vector2>();
        
        // Генерируем случайные позиции для всех объектов
        for (int i = 0; i < numStars + numJunks; i++)
        {
            Vector2 newPos;
            bool validPosition;
            int attempts = 0;
            const int MAX_ATTEMPTS = 100;

            do
            {
                validPosition = true;
                newPos = new Vector2(
                    Random.Range(-BOARD_WIDTH/2 + 1, BOARD_WIDTH/2 - 1),
                    Random.Range(-BOARD_HEIGHT/2 + 1, BOARD_HEIGHT/2 - 1)
                );

                // Проверяем расстояние до всех существующих позиций
                foreach (var pos in positions)
                {
                    if (Vector2.Distance(pos, newPos) < MIN_OBJECT_SPACING)
                    {
                        validPosition = false;
                        break;
                    }
                }

                attempts++;
                if (attempts >= MAX_ATTEMPTS)
                {
                    Debug.LogWarning("Не удалось найти подходящую позицию после " + MAX_ATTEMPTS + " попыток");
                    break;
                }
            }
            while (!validPosition);

            positions.Add(newPos);
        }

        // Создаем звезды
        for (int i = 0; i < numStars; i++)
        {
            if (i < positions.Count)
            {
                GameObject star = CreateStar(positions[i]);
                stars.Add(star);
            }
        }

        // Создаем мусор
        for (int i = 0; i < numJunks; i++)
        {
            if (i + numStars < positions.Count)
            {
                GameObject junk = CreateJunk(positions[i + numStars]);
                junks.Add(junk);
            }
        }
    }

    private GameObject CreateStar(Vector2 position)
    {
        GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        star.transform.SetParent(transform);
        star.transform.position = new Vector3(position.x, position.y, 0);
        star.transform.localScale = Vector3.one * 0.5f;

        star.AddComponent<CircleCollider2D>();
        
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = Color.yellow;
        star.GetComponent<Renderer>().material = material;

        return star;
    }

    private GameObject CreateJunk(Vector2 position)
    {
        GameObject junk = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        junk.transform.SetParent(transform);
        junk.transform.position = new Vector3(position.x, position.y, 0);
        junk.transform.localScale = Vector3.one * 0.3f;

        junk.AddComponent<CircleCollider2D>();
        
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = Color.gray;
        junk.GetComponent<Renderer>().material = material;

        return junk;
    }

    private void ClearLevel()
    {
        foreach (var star in stars)
        {
            if (star != null)
            {
                Destroy(star);
            }
        }
        stars.Clear();

        foreach (var junk in junks)
        {
            if (junk != null)
            {
                Destroy(junk);
            }
        }
        junks.Clear();
    }

    public List<GameObject> GetStars()
    {
        return stars;
    }

    public List<GameObject> GetJunks()
    {
        return junks;
    }
}
