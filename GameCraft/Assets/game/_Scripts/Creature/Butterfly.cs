using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Butterfly : MonoBehaviour
{
    public Tilemap blockTilemap;    
    public Tilemap fireTilemap;    
    public Tilemap groundTilemap;   

    private Vector3Int currentGridPosition;
    private Vector3Int lastDirection; 

    public SpriteRenderer shadow;

    private Main main;
    private WorldGrid worldGrid;
    
    public bool isButterfluFire = false; 

    public Sprite normalButterflySprite; 
    public Sprite fireButterflySprite;  

    void Start()
    {
        shadow.transform.gameObject.SetActive(true);
        currentGridPosition = groundTilemap.WorldToCell(transform.position);
        lastDirection = Vector3Int.zero; 
        main = FindObjectOfType<Main>();
        worldGrid = FindObjectOfType<WorldGrid>();

        GetComponent<SpriteRenderer>().sprite = isButterfluFire ? fireButterflySprite : normalButterflySprite;
    }
    
    private List<Vector3Int> visitedPlantTiles = new List<Vector3Int>(); 
    public void MoveButterfly()
    {
        Vector3Int[] directions = {
            new Vector3Int(0, 1, 0), 
            new Vector3Int(0, -1, 0),  
            new Vector3Int(-1, 0, 0), 
            new Vector3Int(1, 0, 0)   
        };

        List<Vector3Int> plantMoves = new List<Vector3Int>();
        List<Vector3Int> normalMoves = new List<Vector3Int>(); 
        List<Vector3Int> jarMoves = new List<Vector3Int>();   
        Vector3Int oppositeDirection = Vector3Int.zero;  
        bool isFireNearby = false;

        foreach (var direction in directions)
        {
            Vector3Int newPosition = currentGridPosition + direction;

            if (fireTilemap.HasTile(newPosition))
            {
                if (isButterfluFire)
                {
                    normalMoves.Add(newPosition); 
                }
                else
                {
                    isFireNearby = true;
                    oppositeDirection = -direction; 
                }
            }
            else if (IsTileAvailable(newPosition, direction)) 
            {
                if (IsPlantTile(newPosition) && !visitedPlantTiles.Contains(newPosition))
                {
                    plantMoves.Add(newPosition);
                }
                else if (IsJar(newPosition))
                {
                    jarMoves.Add(newPosition);
                }
                else
                {
                    normalMoves.Add(newPosition);
                }
            }
        }

        if (isFireNearby)
        {
            Vector3Int oppositePosition = currentGridPosition + oppositeDirection;
            
            if (IsTileAvailable(oppositePosition, oppositeDirection) && !fireTilemap.HasTile(oppositePosition) && !IsJar(oppositePosition))
            {
                MoveToNewPosition(oppositePosition);
                return;
            }
        }

        if (plantMoves.Count > 0)
        {
            Vector3Int moveDirection = GetNewestDirection(plantMoves);
            MoveToNewPosition(moveDirection);
            return;
        }

        if (normalMoves.Count > 0)
        {
            Vector3Int moveDirection = GetRandomDirection(normalMoves);
            MoveToNewPosition(moveDirection);
            return;
        }

        if (jarMoves.Count > 0)
        {
            Vector3Int moveDirection = GetRandomDirection(jarMoves);
            MoveToNewPosition(moveDirection);
        }
        else
        {
            ButterflyAway();
        }
    }

    
    private Vector3Int GetNewestDirection(List<Vector3Int> plantMoves)
    {
        return plantMoves[0]; 
    }

    public static Butterfly CreateButterfly(GameObject butterflyPrefab, Vector3Int position, bool isFire, Tilemap blockTilemap, Tilemap fireTilemap, Tilemap groundTilemap)
    {
        Vector3 pos = blockTilemap.GetCellCenterWorld(position);
        GameObject butterflyObject = Instantiate(butterflyPrefab, pos, Quaternion.identity);
        Butterfly butterfly = butterflyObject.GetComponent<Butterfly>();
        butterfly.isButterfluFire = isFire;
        butterfly.blockTilemap = blockTilemap; 
        butterfly.fireTilemap = fireTilemap;   
        butterfly.groundTilemap = groundTilemap; 
    
        return butterfly;
    }

    private void MoveToNewPosition(Vector3Int newPosition)
    {
        if (IsPlantTile(currentGridPosition) && isButterfluFire)
        {
            fireTilemap.SetTile(currentGridPosition, worldGrid.fireTile);
        }

        currentGridPosition = newPosition;

        if (IsPlantTile(newPosition))
        {
            visitedPlantTiles.Add(newPosition);
        }

        lastDirection = newPosition - currentGridPosition; 

        Vector3 directionToTarget = groundTilemap.GetCellCenterWorld(newPosition) - transform.position;
        float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f;

        transform.DORotateQuaternion(Quaternion.Euler(0, 0, angle), 0.5f);
        transform.DOMove(groundTilemap.GetCellCenterWorld(currentGridPosition), 0.5f).OnComplete(ButterflyTurnEnd);
    }

    private bool IsPlantTile(Vector3Int position)
    {
        TileBase tile = blockTilemap.GetTile(position);

        if (tile != null)
        {
            return tile.name == "PlantTile"; 
        }

        return false;
    }

    private bool IsTileAvailable(Vector3Int position, Vector3Int direction)
    {
        if (groundTilemap.HasTile(position))
        {
            if (blockTilemap.HasTile(position))
            {
                if (blockTilemap.GetTile(position).name == "PlantTile")
                    return true;

                if (IsJar(position))
                    return true;

                return false;
            }

            if (fireTilemap.HasTile(position))
            {
                if (!isButterfluFire)
                {
                    if (direction == new Vector3Int(0, 1, 0)) return IsTileAvailable(position + new Vector3Int(0, -1, 0), new Vector3Int(0, -1, 0));
                    if (direction == new Vector3Int(0, -1, 0)) return IsTileAvailable(position + new Vector3Int(0, 1, 0), new Vector3Int(0, 1, 0));
                    if (direction == new Vector3Int(-1, 0, 0)) return IsTileAvailable(position + new Vector3Int(1, 0, 0), new Vector3Int(1, 0, 0));
                    if (direction == new Vector3Int(1, 0, 0)) return IsTileAvailable(position + new Vector3Int(-1, 0, 0), new Vector3Int(-1, 0, 0));
                }
                return true;
            }

            return true;
        }
        return false; 
    }


    private bool IsJar(Vector3Int position)
    {
        TileBase tile = blockTilemap.GetTile(position);

        if (tile != null)
        {
            return tile.name == "JarOpenTile";
        }

        return false; 
    }

    private Vector3Int GetRandomDirection(List<Vector3Int> possibleMoves)
    {
        Vector3Int selectedMove = possibleMoves[Random.Range(0, possibleMoves.Count)];

        if (lastDirection != Vector3Int.zero && selectedMove == currentGridPosition + -lastDirection)
        {
            possibleMoves.Remove(selectedMove);
            selectedMove = possibleMoves[Random.Range(0, possibleMoves.Count)];
        }

        return selectedMove;
    }

    private void ButterflyTurnEnd()
    {
        CheckVictory();

        if (main.wictory)
            return;

        if (main.stepsLeft <= 0)
        {
            ButterflyAway();
        }
        else
        {
            
            main.StartTurn();
        }
    }

    public void ButterflyAway()
    {
        shadow.transform.gameObject.SetActive(false);
        Camera mainCamera = Camera.main;
        Vector3 screenBottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 screenTopRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        float screenWidth = screenTopRight.x - screenBottomLeft.x;
        float screenHeight = screenTopRight.y - screenBottomLeft.y;
        
        Vector3 randomTarget = Vector3.zero;

        int side = Random.Range(0, 4); 

        switch (side)
        {
            case 0: 
                randomTarget = new Vector3(screenBottomLeft.x - Random.Range(2f, 5f), Random.Range(screenBottomLeft.y, screenTopRight.y), 0);
                break;
            case 1: 
                randomTarget = new Vector3(screenTopRight.x + Random.Range(2f, 5f), Random.Range(screenBottomLeft.y, screenTopRight.y), 0);
                break;
            case 2: 
                randomTarget = new Vector3(Random.Range(screenBottomLeft.x, screenTopRight.x), screenTopRight.y + Random.Range(2f, 5f), 0);
                break;
            case 3: 
                randomTarget = new Vector3(Random.Range(screenBottomLeft.x, screenTopRight.x), screenBottomLeft.y - Random.Range(2f, 5f), 0);
                break;
        }

        Vector3 directionToTarget = randomTarget - transform.position;
        float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

        angle -= 90f; 


        float distance = Vector3.Distance(transform.position, randomTarget);
        float speed = 5f;
        float animationDuration = distance / speed;

        float scaleMultiplier = 3f;
        
        transform.DORotateQuaternion(Quaternion.Euler(0, 0, angle), 0.5f).OnComplete(() =>
        {
            transform.DOScale(transform.localScale * scaleMultiplier, animationDuration);
            transform.DOMove(randomTarget, animationDuration).SetEase(Ease.Linear).OnComplete(() =>
            {
                DOTween.Kill(transform);
                main.Lose();
            });
        });
    }

    
    public void CheckVictory()
    {
        if (IsJar(currentGridPosition))
        {
            DOTween.Kill(transform);

            main.currentLevelIndex++;
            
            if (main.currentLevelIndex == main.maxLevels)
            {
                main.Victory();
            }
            else
            {
                main.NextLevel();
            }
        }
    }
}
