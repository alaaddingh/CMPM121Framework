using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Rpn;
using RpnComponents;

public class EnemySpawner : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;

    private WaveData currentWave;
    private EnemyData firstEnemy;

    void Start()
    {
        string enemiesPath = "enemies";
        TextAsset enemiesFile = Resources.Load<TextAsset>(enemiesPath);

        if (enemiesFile != null)
        {
            List<EnemyData> enemyData = JsonConvert.DeserializeObject<List<EnemyData>>(enemiesFile.text);
            firstEnemy = enemyData[0]; 
        }
        else
        {
            Debug.LogError("Failed to load enemies.json");
            return;
        }

        GameObject selector = Instantiate(button, level_selector.transform);
        selector.transform.localPosition = new Vector3(0, 130);
        selector.GetComponent<MenuSelectorController>().spawner = this;
        selector.GetComponent<MenuSelectorController>().SetLevel("Start");
    }

    public void StartLevel(string levelName)
    {
        if (level_selector != null)
        {
            level_selector.gameObject.SetActive(false);
        }

        if (GameManager.Instance.player != null)
        {
            GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        }

        string levelsPath = "levels";
        TextAsset levelsFile = Resources.Load<TextAsset>(levelsPath);
        if (levelsFile != null)
        {
            List<WaveData> waveData = JsonConvert.DeserializeObject<List<WaveData>>(levelsFile.text);
            currentWave = waveData.Find(w => w.name == levelName); // Find the wave data for the specified level

            if (currentWave != null)
            {
                GameManager.Instance.state = GameManager.GameState.INWAVE; // Update game state
                StartCoroutine(SpawnWave());
            }
            else
            {
                Debug.LogError($"Level '{levelName}' not found in levels.json.");
            }
        }
        else
        {
            Debug.LogError("Failed to load levels.json.");
        }
    }

    public void NextWave()
    {
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        Debug.Log("Starting wave...");

        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }

        GameManager.Instance.state = GameManager.GameState.INWAVE;

        foreach (var spawn in currentWave.spawns)
        {
            int waveNumber = 1; 
            float baseHp = firstEnemy.hp; 
            int count = EnemyCountCalculator.CalculateCount(spawn.count, waveNumber);
            float hp = EnemyHpCalculator.CalculateHp(spawn.hp, waveNumber, baseHp);

            Debug.Log($"Spawning {count} enemies of type {spawn.enemy} with HP = {hp}");

            for (int i = 0; i < count; i++)
            {
                yield return SpawnEnemy(spawn.enemy, hp);
            }
        }

        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);

        GameManager.Instance.state = GameManager.GameState.WAVEEND;
        Debug.Log("Wave ended.");
    }

    IEnumerator SpawnEnemy(string enemyType, float hp)
    {
        SpawnPoint spawnPoint = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;
        Vector3 position = spawnPoint.transform.position + new Vector3(offset.x, offset.y, 0);

        Debug.Log($"Spawning enemy: {enemyType} at position {position}");

        // Instantiate the enemy
        GameObject newEnemy = Instantiate(enemy, position, Quaternion.identity);

        // Assign enemy properties (e.g., HP)
        newEnemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(0); // Example sprite
        EnemyController en = newEnemy.GetComponent<EnemyController>();
        en.hp = new Hittable(Mathf.RoundToInt(hp), Hittable.Team.MONSTERS, newEnemy);
        en.speed = Mathf.RoundToInt(firstEnemy.speed); // Convert float to int
        GameManager.Instance.AddEnemy(newEnemy);

        yield return new WaitForSeconds(0.5f); // Delay between spawns
    }
}

public class WaveData
{
    public string name { get; set; }
    public int waves { get; set; }
    public List<SpawnData> spawns { get; set; }
}

public class SpawnData
{
    public string enemy { get; set; }
    public string count { get; set; }
    public string hp { get; set; }
}

public class EnemyData
{
    public string name { get; set; }
    public int sprite { get; set; }
    public float hp { get; set; }
    public float speed { get; set; }
    public float damage { get; set; }
}