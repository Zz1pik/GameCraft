using UnityEngine;
using System.Collections.Generic;

public class Notepad : MonoBehaviour
{
    public RectTransform drawingArea; // Область, в которой можно рисовать
    public Camera uiCamera; // Камера, которая рендерит UI
    public Color drawingColor = Color.black; // Цвет рисования
    public Color eraserColor = Color.white; // Цвет стирания (например, белый)
    public float lineWidth = 1f; // Толщина линии
    public Sprite drawingSprite; // Спрайт, которым рисуем
    public GameObject penObject; // Спрайт курсора, когда он в области рисования
    public Transform penPos; // Исходная позиция курсора
    public Vector3 offset; // Смещение для позиции курсора

    private bool isDrawing = false; // Флаг, чтобы отслеживать, рисуем ли мы в данный момент
    public bool isErasing = false; // Флаг, чтобы отслеживать, стираем ли мы в данный момент
    private LineRenderer currentLineRenderer; // Компонент LineRenderer для текущей линии
    private List<Vector3> points = new List<Vector3>(); // Список точек для текущей линии
    private List<LineRenderer> lineRenderers = new List<LineRenderer>(); // Список всех линий

    private Vector2 screenBoundsMin; // Минимальная точка экрана
    private Vector2 screenBoundsMax; // Максимальная точка экрана

    private void Start()
    {
        // Получаем границы области рисования на экране
        SetDrawingAreaBounds();
    }

    private void Update()
    {
        // Получаем позицию мыши в мировых координатах
        Vector3 mousePos = GetMouseWorldPosition();

        // Проверяем, находится ли курсор в зоне рисования
        if (IsPointerInsideDrawingArea())
        {
            // Скрыть стандартный курсор
            Cursor.visible = false;

            // Объект курсора следит за позицией мыши
            penObject.transform.position = mousePos + offset;
        }
        else
        {
            // Показать стандартный курсор, если он выходит за пределы зоны рисования
            penObject.transform.position = penPos.position;
            Cursor.visible = true;
            isDrawing = false;
        }

        // Включаем рисование при нажатии левой кнопки мыши
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerInsideDrawingArea())
            {
                if (isErasing)
                {
                    EraseLine(mousePos); // Если в режиме стирания, стираем
                }
                else
                {
                    StartDrawing(); // Иначе начинаем рисование
                }
            }
        }

        // Прекращаем рисовать или стирать при отпускании кнопки мыши
        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false; // Прекращаем рисование
        }

        // Добавляем точки в линию, если рисуем
        if (isDrawing)
        {
            DrawLine(mousePos);
        }
    }

    // Метод для начала рисования
    private void StartDrawing()
    {
        isDrawing = true; // Начинаем рисовать
        points.Clear(); // Очищаем старые точки

        // Создаём новый LineRenderer для новой линии
        currentLineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
        currentLineRenderer.startWidth = lineWidth;
        currentLineRenderer.endWidth = lineWidth;
        currentLineRenderer.startColor = drawingColor;
        currentLineRenderer.endColor = drawingColor;
        currentLineRenderer.positionCount = 0;
        currentLineRenderer.sortingOrder = 1;
        currentLineRenderer.gameObject.transform.SetParent(drawingArea.transform);

        // Если задан спрайт, применяем его к новому LineRenderer
        if (drawingSprite != null)
        {
            currentLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            currentLineRenderer.material.mainTexture = drawingSprite.texture;
        }
        else
        {
            currentLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        lineRenderers.Add(currentLineRenderer); // Добавляем в список линий
    }

    // Метод для рисования
    private void DrawLine(Vector3 mousePos)
    {
        if (!points.Contains(mousePos)) // Если эта точка ещё не добавлена
        {
            points.Add(mousePos); // Добавляем точку
            currentLineRenderer.positionCount = points.Count; // Обновляем количество точек в LineRenderer
            currentLineRenderer.SetPosition(points.Count - 1, mousePos); // Устанавливаем новую точку
        }
    }

    // Метод для стирания линии
    private void EraseLine(Vector3 mousePos)
    {
        // Проверяем каждую линию
        foreach (var lineRenderer in lineRenderers)
        {
            // Для каждой линии проверяем, попадает ли мышь в зону вокруг линии
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                float distance = Vector2.Distance(lineRenderer.GetPosition(i), mousePos);
                if (distance < lineWidth) // Если расстояние меньше толщины линии
                {
                    // Удаляем точку (или линию)
                    lineRenderer.positionCount--;
                    for (int j = i; j < lineRenderer.positionCount; j++)
                    {
                        lineRenderer.SetPosition(j, lineRenderer.GetPosition(j + 1)); // Сдвигаем точки
                    }
                    break; // Выход из цикла, так как точка найдена и удалена
                }
            }
        }
    }

    private void SetDrawingAreaBounds()
    {
        // Получаем мировые координаты углов области рисования с учетом размера и привязки
        Vector3[] worldCorners = new Vector3[4];
        drawingArea.GetWorldCorners(worldCorners); // Получаем все 4 угла RectTransform в мировых координатах

        // worldCorners[0] - это нижний левый угол, worldCorners[2] - верхний правый
        screenBoundsMin = new Vector2(worldCorners[0].x, worldCorners[0].y);
        screenBoundsMax = new Vector2(worldCorners[2].x, worldCorners[2].y);
    }

    private bool IsPointerInsideDrawingArea()
    {
        // Получаем положение указателя мыши в мировых координатах
        Vector3 mouseWorldPosition = GetMouseWorldPosition();

        // Проверяем, находится ли мышь внутри границ области рисования
        return mouseWorldPosition.x >= screenBoundsMin.x && mouseWorldPosition.x <= screenBoundsMax.x &&
               mouseWorldPosition.y >= screenBoundsMin.y && mouseWorldPosition.y <= screenBoundsMax.y;
    }

    private Vector3 GetMouseWorldPosition()
    {
        // Получаем положение мыши на экране в мировых координатах
        Vector3 mouseWorldPosition = uiCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, uiCamera.nearClipPlane));

        // Возвращаем только X и Y координаты для проверки внутри зоны рисования
        return new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, 0f); // Игнорируем Z-координату
    }

    // Метод для переключения на режим стирания
    public void ToggleEraseMode(bool erase)
    {
        isErasing = erase;
        if (erase)
        {
            drawingColor = eraserColor; // Если в режиме стирания, меняем цвет на белый
        }
        else
        {
            drawingColor = Color.black; // Возвращаем цвет рисования
        }
    }
}
