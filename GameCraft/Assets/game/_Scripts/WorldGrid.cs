using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGrid : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap blockTilemap;
    public Tilemap fireTilemap;

    public GameObject butterflyPrefab; 


    public TileBase groundTile;     
    public TileBase fireTile;     
    public TileBase treeTile;       
    public TileBase plantTile;      
    public TileBase jarTile;       
    
    private int gridWidth;
    private int gridHeight;

    private Main main;

    public void Start()
    {
        main = FindObjectOfType<Main>();
    }
    
    public void GenerateWorld(string[] worldMap)
    {
        ClearWorld(); 

        gridHeight = worldMap.Length;
        gridWidth = worldMap[0].Length;

        int centerX = gridWidth / 2;
        int centerY = gridHeight / 2;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                char tileChar = worldMap[y][x];

                Vector3Int position = new Vector3Int(x - centerX, centerY - y - 1, 0);

                groundTilemap.SetTile(position, groundTile);

                switch (tileChar)
                {
                    case 'f':
                        fireTilemap.SetTile(position, fireTile);
                        break;
                    case 't':
                        blockTilemap.SetTile(position, treeTile);
                        break;
                    case 'p':
                        blockTilemap.SetTile(position, plantTile);
                        break;
                    case 'b':
                        if (main.currentLevelIndex == 10 || main.currentLevelIndex == 11 || main.currentLevelIndex == 12 || main.currentLevelIndex == 13)
                        {
                            Butterfly.CreateButterfly(butterflyPrefab, position, true, blockTilemap, fireTilemap, groundTilemap);
                        }
                        else
                        {
                            Butterfly.CreateButterfly(butterflyPrefab, position, false, blockTilemap, fireTilemap, groundTilemap);
                        }
                        break;
                    case 'j':
                        blockTilemap.SetTile(position, jarTile);
                        break;
                    case '+':
                        break;
                }
            }
        }
        
        main.AddNewFiresToList();
    
        main.butterfly = FindObjectOfType<Butterfly>();
    }

    public void ClearWorld()
    {
        groundTilemap.ClearAllTiles();
        blockTilemap.ClearAllTiles();
        fireTilemap.ClearAllTiles();

        Butterfly butterfly = FindObjectOfType<Butterfly>();

        if (butterfly != null)
        {
            Destroy(butterfly.gameObject);
        }
    }

}
