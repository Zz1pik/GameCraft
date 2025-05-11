using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Level
{
    public string[] map;
    public int steps;
    public Tile[] tiles; 

    public Level(string[] map, int steps, Tile[] tiles)
    {
        this.map = map;
        this.steps = steps;
        this.tiles = tiles;
    }
}

public class Main : MonoBehaviour
{
    public WorldGrid worldGrid;
    public TileCursor tileCursor; 
    public GameObject winScreen;
    public GameObject loseScreen;
    public Butterfly butterfly;
    
    public Tilemap fireTilemap;

    public TextMeshProUGUI stepsLeftText;
    public TextMeshProUGUI guideText;

    public GameObject fireBackground;
    
    public int stepsLeft;
    public bool butterFlyStep;
    public bool wictory = false;
    public bool canPlace;
    public bool hasGuide = false;
    
    private Level[] levels;

    public int currentLevelIndex = 11;
    public int maxLevels;
    
    public AudioSource audioSource;
    public AudioSource audioSourceMusic;
    public AudioSource audioSourceBurn;

    public List<FireInstance> activeFires = new List<FireInstance>();

    private bool burnSound = false;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }

    void Start()
    {
        currentLevelIndex = 0;
        
        tileCursor.OnTilePlaced += HandleTilePlaced;
        
        guideText.gameObject.SetActive(false);
        
        audioSourceMusic.clip = Resources.Load<AudioClip>("Audio/backgroundMusic"); 
        audioSourceMusic.loop = true;
        audioSourceMusic.Play(); 

        levels = new Level[]
        {
            new Level(new string[]
            {
                "+bj"
            }, 1, new Tile[] {worldGrid.treeTile as Tile}),

            new Level(new string[]
            {
                "+bpjt"
            }, 2, new Tile[] {worldGrid.treeTile as Tile}),
            
            new Level(new string[]
            {
                "ttttt",
                "tb+jt",
                "ttttt"
            }, 2, new Tile[] {worldGrid.treeTile as Tile, worldGrid.plantTile as Tile}),
            
            new Level(new string[]
            {
                "++++p",
                "b++pj",
                "++++p"
            }, 10, new Tile[] {worldGrid.treeTile as Tile}),
            
            new Level(new string[]
            {
                "++j",
                "p++",
                "bp+"
            }, 4, new Tile[] {worldGrid.treeTile as Tile, worldGrid.plantTile as Tile}),
            
            new Level(new string[]
            {
                "b++++",
                "tttt+",
                "+++++",
                "+tttt",
                "++++j"
            }, 16, new Tile[] {worldGrid.treeTile as Tile, worldGrid.plantTile as Tile}),

            new Level(new string[]
            {
                "ttttt",
                "b+tjt",
                "ttttt"
            }, 3, new Tile[] {worldGrid.fireTile as Tile}),

            new Level(new string[]
            {
                "ppppp",
                "bpppj",
                "ppppp"
            }, 8, new Tile[] {worldGrid.fireTile as Tile}),
            
            new Level(new string[]
            {
                "+bt+t",
                "++t+t",
                "++ttj"
            }, 7, new Tile[] {worldGrid.fireTile as Tile}),
            
            new Level(new string[]
            {
                "+ttt+",
                "tpppt",
                "tpbpt",
                "tpppt",
                "+tjt+"
            }, 8, new Tile[] {worldGrid.fireTile as Tile}),
            
            new Level(new string[]
            {
                "ttttt",
                "+bppj",
                "ttttt"
            }, 3, new Tile[] {worldGrid.treeTile as Tile}),
            
            new Level(new string[]
            {
                "++t++",
                "ppbpp",
                "p+p+p",
                "ptjtp"
            }, 10, new Tile[] {worldGrid.treeTile as Tile}),
            
            new Level(new string[]
            {
                "bpt++",
                "pt++t",
                "t++tj",
            }, 10, new Tile[] { worldGrid.fireTile as Tile, worldGrid.treeTile as Tile }),
            
            new Level(new string[]
            {
                "bpt+tt",
                "tptppp",
                "tptptt",
                "+ptppp",
                "pptpt+",
                "tppptj",
            }, 22, new Tile[] { worldGrid.treeTile as Tile, worldGrid.plantTile as Tile }),
        };

        maxLevels = levels.Length;

        worldGrid.GenerateWorld(levels[currentLevelIndex].map);
        stepsLeft = levels[currentLevelIndex].steps;

        fireBackground.SetActive(false);

        StartTurn();
        
        tileCursor.UpdateCurrentTileImage(); 
        UiStart();
    }

    public void StartTurn()
    {
        butterFlyStep = false;
        canPlace = true;

        tileCursor.tiles = null;
        tileCursor.tiles = levels[currentLevelIndex].tiles;
        tileCursor.UpdateCurrentTileImage();
        
        CheckBurningBlock();
        CheckFireStates();
        
        if (currentLevelIndex == 0 && !hasGuide)
        {
            hasGuide = true;
            guideText.gameObject.SetActive(true);
            StartCoroutine(LevelGuide0()); 
        } 
        else if (currentLevelIndex == 1 && !hasGuide)
        {
            hasGuide = true;
            guideText.gameObject.SetActive(true);
            StartCoroutine(LevelGuide1());
        }
        else if (currentLevelIndex == 2 && !hasGuide)
        {
            hasGuide = true;
            guideText.gameObject.SetActive(true);
            StartCoroutine(LevelGuide2()); 
        }
        else if (currentLevelIndex == 3 && !hasGuide)
        {
            canPlace = true;
            guideText.gameObject.SetActive(false);
        }
        else if (currentLevelIndex == 4 && !hasGuide)
        {
            canPlace = true;
            guideText.gameObject.SetActive(false);
        }
        else if (currentLevelIndex == 5 && !hasGuide)
        {
            canPlace = true;
            guideText.gameObject.SetActive(false);
        }
        else if (currentLevelIndex == 6 && !hasGuide)
        {
            hasGuide = true;
            guideText.gameObject.SetActive(true);
            StartCoroutine(LevelGuide6()); 
        }
        else if (currentLevelIndex == 7 && !hasGuide)
        {
            hasGuide = true;
            guideText.gameObject.SetActive(true);
            StartCoroutine(LevelGuide7()); 
        }
        else if (currentLevelIndex == 8 && !hasGuide)
        {
            canPlace = true;
            guideText.gameObject.SetActive(false);
        }
        else if (currentLevelIndex == 9 && !hasGuide)
        {
            canPlace = true;
            guideText.gameObject.SetActive(false);
        }
        else if (currentLevelIndex == 10 && !hasGuide)
        {
            hasGuide = true;
            guideText.gameObject.SetActive(true);
            StartCoroutine(LevelGuide10()); 
        }
        else if (currentLevelIndex == 11 && !hasGuide)
        {
            canPlace = true;
            guideText.gameObject.SetActive(false);
        }
        else if (currentLevelIndex == 12 && !hasGuide)
        {
            canPlace = true;
            guideText.gameObject.SetActive(false);
        }
        else if (currentLevelIndex == 13 && !hasGuide)
        {
            canPlace = true;
            guideText.gameObject.SetActive(false);
        }
    }

    private IEnumerator LevelGuide0()
    {
        ShowGuideText("<wave amp=1>Эта бабочка прекрасна, не правда ли?");
        yield return new WaitForSeconds(4.2f);
        ShowGuideText("<wave amp=1>Нажми <jump>ЛКМ</jump> слева от бабочки и размести дерево.");
        yield return new WaitForSeconds(2.5f);
        canPlace = true;
    }
    
    private IEnumerator LevelGuide1()
    {
        canPlace = false;
        ShowGuideText("<wave amp=1>Так что, похоже, ее нужно снова поймать...");
        yield return new WaitForSeconds(4.5f); 
        canPlace = true;
        yield return new WaitUntil(() => butterFlyStep);
        canPlace = false;
        ShowGuideText("<wave amp=1>Как видишь, она умеет летать над цветами. Запомните, я думаю, эта информация пригодится вам в дальнейшем.");
        yield return new WaitForSeconds(6f);
        canPlace = true;
    }
    
    private IEnumerator LevelGuide2()
    {
        canPlace = false;
        ShowGuideText("<wave amp=1>Похоже, мы зашли в тупик!");
        yield return new WaitForSeconds(3.2f); 
        ShowGuideText("<wave amp=1>Хорошо, что я припас дополнительную плитку специально для таких случаев! Нажми на <jump>ПКМ</jump> чтобы изменить тайл и размести его!</wave>");
        yield return new WaitForSeconds(7f); 
        canPlace = true;
        yield return new WaitUntil(() => butterFlyStep);
        canPlace = false;
        ShowGuideText("<wave amp=1>Теперь шансы увеличились! Просто загони её в банку!");
        yield return new WaitForSeconds(3.5f);
        canPlace = true;
    }
    
    private IEnumerator LevelGuide6()
    {
        canPlace = false;
        ShowGuideText("<wave amp=1>Похоже, дерево преградило нам путь...");
        yield return new WaitForSeconds(4f); 
        ShowGuideText("<wave amp=1>Но хорошо, что у нас есть огонь, который сжигает все цветы и деревья :)</wave>");
        yield return new WaitForSeconds(4f);
        canPlace = true;
        yield return new WaitUntil(() => butterFlyStep);
        ShowGuideText("<wave amp=1>Это так хорошо, что все живые существа боятся огня и стараются убежать от него в противоположную сторону.");
        canPlace = true;
    }
    
    private IEnumerator LevelGuide7()
    {
        fireBackground.SetActive(true);
        burnSound = true;

        audioSourceMusic.pitch = -0.1f;
        
        audioSourceBurn.clip = Resources.Load<AudioClip>("Audio/backgroundBurn");
        audioSourceBurn.loop = true;
        audioSourceBurn.Play();
        ShowGuideText("<shake><color=red>СОЖГИТЕ ВСЕ ЗДЕСЬ!");
        canPlace = true;
        yield return new Null();
    }

    private IEnumerator LevelGuide10()
    {
        canPlace = true;
        ShowGuideText("<wave amp=1>Что случилось? Почему она стала жёлтой???");
        yield return new WaitForSeconds(3.2f);
        ShowGuideText("<wave amp=1>Наверное, это какой-то другой вид. Ладно, просто загони её в банку, и всё.");
        yield return new WaitForSeconds(3f);
    }
    
    
    public void ShowGuideText(string messageText)
    {
        guideText.text = messageText;
    }

    public void CheckFireStates()
    {
        for (int i = activeFires.Count - 1; i >= 0; i--)
        {
            activeFires[i].UpdateFire();
            
            if (activeFires[i].turnsLeft <= 0)
            {
                activeFires.RemoveAt(i);
            }
        }
    }
    
    public void AddNewFiresToList()
    {
        BoundsInt bounds = fireTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                
                if (fireTilemap.HasTile(tilePosition))
                {
                    if (!IsFireInstanceAtPosition(tilePosition))
                    {
                        FireInstance newFire = new FireInstance(tilePosition, fireTilemap, 3);
                        activeFires.Add(newFire);
                    }
                }
            }
        }
    }

    private bool IsFireInstanceAtPosition(Vector3Int position)
    {
        foreach (FireInstance fire in activeFires)
        {
            if (fire.position == position)
            {
                return true;
            }
        }
        return false; 
    }
    
    public void UiStart()
    {
        stepsLeftText.gameObject.SetActive(true);
        
        winScreen.gameObject.SetActive(false);
        loseScreen.gameObject.SetActive(false);
        
        UpdateText(); 
    }

    private void HandleTilePlaced()
    {
        if (butterfly != null)
        {
            canPlace = false;
            butterfly.MoveButterfly();
            stepsLeft--;

            UpdateText();
        }
    }

    public void NextLevel()
    {
        if (currentLevelIndex < levels.Length)
        {
            if (burnSound)
            {
                audioSourceMusic.pitch = 1;
                audioSourceBurn.Stop();
            }

            hasGuide = false;

            guideText.gameObject.SetActive(false);
            winScreen.gameObject.SetActive(false);
            stepsLeftText.gameObject.SetActive(true);

            wictory = false;
            
            fireBackground.SetActive(false);

            worldGrid.GenerateWorld(levels[currentLevelIndex].map);
            stepsLeft = levels[currentLevelIndex].steps; 
            
            StartTurn();
            UpdateText();
        }
    }
    
    public void RestartLevel()
    {
        guideText.gameObject.SetActive(false);
        loseScreen.gameObject.SetActive(false);
        stepsLeftText.gameObject.SetActive(true);

        audioSourceMusic.Play();
        
        activeFires.Clear();

        worldGrid.GenerateWorld(levels[currentLevelIndex].map);
        stepsLeft = levels[currentLevelIndex].steps; 
        
        StartTurn();
        UiStart();
    }

    public void Victory()
    {
        hasGuide = false;
        wictory = true;
        
        audioSourceMusic.clip = Resources.Load<AudioClip>("Audio/win");
        audioSourceMusic.Play();
        
        guideText.gameObject.SetActive(false);
        stepsLeftText.gameObject.SetActive(false);
        winScreen.gameObject.SetActive(true);
    }
    
    public void Lose()
    {
        hasGuide = false;
        
        audioSourceMusic.Pause();
        
        guideText.gameObject.SetActive(false);
        stepsLeftText.gameObject.SetActive(false);
        loseScreen.gameObject.SetActive(true);
    }

    private void UpdateText()
    {
        stepsLeftText.text = "Осталось ходов: " + stepsLeft;
    }
    
    public void CheckBurningBlock()
    {
        if (stepsLeft > 0)
        {
            BoundsInt bounds = worldGrid.blockTilemap.cellBounds;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int tilePosition = new Vector3Int(x, y, 0);
                    
                    TileBase blockTile = worldGrid.blockTilemap.GetTile(tilePosition);
                    TileBase fireTile = worldGrid.fireTilemap.GetTile(tilePosition);
                    
                    if ((blockTile == worldGrid.treeTile || blockTile == worldGrid.plantTile) && 
                        fireTile == worldGrid.fireTile)
                    {
                        audioSource.PlayOneShot(Resources.Load<AudioClip>("Audio/acidBurn"));

                        ParticleSystem particle;
 
                        Vector3 worldPosition = worldGrid.blockTilemap.CellToWorld(tilePosition);
                        particle = Instantiate(Resources.Load<ParticleSystem>("Particle/acidBurnEffect"), worldPosition + new Vector3(0.5f, 0.5f, 0f), Quaternion.identity);

                        particle.Play();

                        worldGrid.blockTilemap.SetTile(tilePosition, null);
                        worldGrid.fireTilemap.SetTile(tilePosition, null);
                    }
                }
            }
        }
    }
}
