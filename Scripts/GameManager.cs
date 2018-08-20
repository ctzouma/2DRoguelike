using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public float levelStartDelay = 2f;
    public float turnDelay = 0.1f;
    public static GameManager instance = null;
    public BoardManager boardScript;
    public int playerFoodPoints = 100;
    [HideInInspector] public bool playersTurn = true;

    private Text levelText;
    private Text restartText;
    private GameObject levelImage;
    private int level = 0;
    private List<Enemy> enemies;
    private bool enemiesMoving;
    private bool doingSetup;
    private bool isGameOver = false;

    void Awake()
    {
        doingSetup = true;
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        enemies = new List<Enemy>();
        boardScript = gameObject.GetComponent<BoardManager>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        level++;
        InitGame();
    }

    public bool IsDoingSetup ()
    {
        return doingSetup;
    }

    public bool IsGameOver ()
    {
        return isGameOver;
    }

    void InitGame()
    {
        doingSetup = true;

        SoundManager.instance.PlayBGM();

        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();
        levelText.text = "Day " + level;

        restartText = GameObject.Find("RestartText").GetComponent<Text>();
        restartText.text = "";

        levelImage.SetActive(true);
        Invoke("HideLevelImage", levelStartDelay);

        enemies.Clear();
        boardScript.SetupScene(level);
    }

    private void Reset()
    {
        level = 0;
        playerFoodPoints = 100;
        GameObject player = GameObject.FindWithTag("Player");
        player.GetComponent<Player>().ResetFood();
        playersTurn = true;
        isGameOver = false;
    }

    private void HideLevelImage()
    {
        levelImage.SetActive(false);
        doingSetup = false;
    }

    public void GameOver()
    {
        
        restartText.text = "Touch screen to restart";
        levelText.text = "After " + level + " days, you starved.";
        levelImage.SetActive(true);
        isGameOver = true;
        StopCoroutine(MoveEnemies());
    }
	
	void Update () 
    {
        if (isGameOver)
        {
            if (SwipeManager.instance.IsTap) 
            {
                Reset();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        if (playersTurn || enemiesMoving || doingSetup || isGameOver) return;

        StartCoroutine(MoveEnemies());
	}

    public void AddEnemyToList(Enemy script)
    {
        enemies.Add(script);
    }

    IEnumerator MoveEnemies()
    {
        enemiesMoving = true;
        yield return new WaitForSeconds(turnDelay);
        if (enemies.Count == 0)
        {
            yield return new WaitForSeconds(turnDelay);
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].MoveEnemy();
            yield return new WaitForSeconds(enemies[i].moveTime);
        }

        playersTurn = true;
        enemiesMoving = false;
    }
}
