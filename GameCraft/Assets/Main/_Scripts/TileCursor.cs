using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TileCursor : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap blockTilemap;
    public Tilemap fireTilemap;

    public Tile[] tiles;
    private int currentTileIndex = 0;

    public Image currentTileImage;
    public Image nextTileImage;

    private GameObject ghostTile;
    private SpriteRenderer ghostSpriteRenderer; 

    public event Action OnTilePlaced;

    public bool switchComplite = false;
    
    public Main main;

    public void Start()
    {
        ghostTile = new GameObject("GhostTile");

        ghostSpriteRenderer = ghostTile.AddComponent<SpriteRenderer>();
        ghostSpriteRenderer.color = new Color(1, 1, 1, 0.8f); 
        ghostSpriteRenderer.sortingOrder = 12;
    }

    void Update()
    {
        if (main.butterFlyStep || !main.canPlace)
        {
            ghostSpriteRenderer.enabled = false;
            transform.GetComponent<SpriteRenderer>().enabled = false;
            return;
        }
        
        ghostSpriteRenderer.enabled = true;
        transform.GetComponent<SpriteRenderer>().enabled = true;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPosition = groundTilemap.WorldToCell(mouseWorldPos);
        Vector3 tileCenterPosition = groundTilemap.GetCellCenterWorld(gridPosition);
        ghostTile.transform.position = tileCenterPosition; 
        transform.position = tileCenterPosition;

        bool canPlaceTile = CanPlaceTile(gridPosition);
        ghostSpriteRenderer.color = canPlaceTile ? new Color(1, 1, 1, 0.8f) : new Color(1, 0, 0, 0.8f);

        if (Input.GetMouseButtonDown(0) && canPlaceTile && main.stepsLeft > 0)
        {
            PlaceTile(gridPosition);
        }

        if (Input.GetMouseButtonDown(1))
        {
            SwitchTile();
        }
    }

    private bool CanPlaceTile(Vector3Int position)
    {
        TileBase existingBlockTile = blockTilemap.GetTile(position);
        TileBase existingFireTile = fireTilemap.GetTile(position);
        TileBase existingGroundTile = groundTilemap.GetTile(position);

        Vector3 worldPosition = blockTilemap.GetCellCenterWorld(position);
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition);

        if (hitCollider != null)
        {
            Butterfly butterfly = hitCollider.GetComponent<Butterfly>();
            if (butterfly != null)
            {
                return false;
            }
        }
        
        if (existingBlockTile != null)
        {
            if ((existingBlockTile.name == "TreeTile" && tiles[currentTileIndex].name == "FireTile" || existingBlockTile.name == "PlantTile" && tiles[currentTileIndex].name == "FireTile") && existingFireTile == null)
            {
                return true; 
            }
            return false;
        }
        
        if (existingFireTile != null)
            return false;
        
        if (existingGroundTile == null)
            return false;

        return true; 
    }

    private void PlaceTile(Vector3Int position)
    {
        main.butterFlyStep = true;

        string currentTileName = tiles[currentTileIndex].name;


        GameObject tileObject = new GameObject("PlacedTile");
        tileObject.transform.position = currentTileName == "FireTile"
            ? blockTilemap.GetCellCenterWorld(position) 
            : new Vector3(blockTilemap.GetCellCenterWorld(position).x, 10f, blockTilemap.GetCellCenterWorld(position).z);

        SpriteRenderer tileSpriteRenderer = tileObject.AddComponent<SpriteRenderer>();
        tileSpriteRenderer.sprite = tiles[currentTileIndex].sprite;
        tileSpriteRenderer.sortingOrder = 10; 

        if (currentTileName == "FireTile")
        {
            tileObject.transform.localScale = Vector3.zero; 
            tileObject.transform.DOScale(Vector3.one, 0.5f).OnComplete(() =>
            {
                fireTilemap.SetTile(position, tiles[currentTileIndex]);
                main.audioSource.PlayOneShot(Resources.Load<AudioClip>("Audio/firePlace"));

                if (!blockTilemap.GetTile(position))
                {
                    FireInstance newFire = new FireInstance(position, fireTilemap, 3);
                    main.activeFires.Add(newFire);
                }

                OnTilePlaced?.Invoke();
                Destroy(tileObject); 
            });
        }
        else
        {
            Sequence tileFallSequence = DOTween.Sequence();

            tileFallSequence.Append(tileObject.transform.DOMove(blockTilemap.GetCellCenterWorld(position), 0.5f).SetEase(Ease.InBounce));

            tileFallSequence.Append(tileObject.transform.DOScale(new Vector3(1.2f, 0.8f, 1f), 0.1f)); 
            tileFallSequence.Append(tileObject.transform.DOScale(Vector3.one, 0.1f)); 

            tileFallSequence.OnComplete(() =>
            {
                blockTilemap.SetTile(position, tiles[currentTileIndex]);

                if (currentTileName == "TreeTile" || currentTileName == "PlantTile")
                {
                    string audioName = currentTileName == "TreeTile" ? "Audio/treePlace" : "Audio/plantPlace";
                    main.audioSource.PlayOneShot(Resources.Load<AudioClip>(audioName));
                }

                OnTilePlaced?.Invoke();
                Destroy(tileObject); 
            });
        }
    }

    public void SwitchTile()
    {
        if (tiles.Length == 0)
            return; 

        currentTileIndex = (currentTileIndex + 1) % tiles.Length; 
        UpdateCurrentTileImage();
        ghostSpriteRenderer.sprite = tiles[currentTileIndex].sprite; 

        if (main.hasGuide)
            switchComplite = true;
    }

    public void UpdateCurrentTileImage()
    {
        currentTileImage.sprite = tiles[currentTileIndex].sprite;

        if (tiles.Length > 1) 
        {
            int nextTileIndex = (currentTileIndex + 1) % tiles.Length; 
            nextTileImage.sprite = tiles[nextTileIndex].sprite; 
            nextTileImage.gameObject.SetActive(true); 
        }
        else
        {
            nextTileImage.gameObject.SetActive(false); 
        }

        ghostSpriteRenderer.sprite = tiles[currentTileIndex].sprite; 
    }

}
