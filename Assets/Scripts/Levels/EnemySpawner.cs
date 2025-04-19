// EnemySpawner.cs — countdown inlined inside RunWaves (always 3 seconds)

using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Rpn;
using RpnComponents;

public class EnemySpawner : MonoBehaviour
{
    [Header("UI")] public Image level_selector;
    public GameObject button;

    [Header("Prefabs & Points")] public GameObject enemy;
    public SpawnPoint[] SpawnPoints;

    Dictionary<string, EnemyData> enemyLUT;
    WaveData currentWave;

    void Start()
    {
        // cache enemy base stats
        var list = JsonConvert.DeserializeObject<List<EnemyData>>(Resources.Load<TextAsset>("enemies").text);
        enemyLUT = new Dictionary<string, EnemyData>();
        foreach (var e in list) enemyLUT[e.name] = e;

        // "Easy" button (default right now)
        var sel = Instantiate(button, level_selector.transform);
        sel.transform.localPosition = new Vector3(0, 130);
        sel.GetComponent<MenuSelectorController>().spawner = this;
        sel.GetComponent<MenuSelectorController>().SetLevel("Easy");
    }

    public void StartLevel(string levelName)
    {
        level_selector.gameObject.SetActive(false);
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();

        var levels = JsonConvert.DeserializeObject<List<WaveData>>(Resources.Load<TextAsset>("levels").text);
        currentWave = levels.Find(l => l.name == levelName);
        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        for (int wave = 1; wave <= currentWave.waves; wave++)
        {
            // countdown
            GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
            GameManager.Instance.countdown = 3;
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown = 2;
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown = 1;
            yield return new WaitForSeconds(1);

            GameManager.Instance.state = GameManager.GameState.INWAVE;

            foreach (var spawn in currentWave.spawns)
            {
                var stats = enemyLUT[spawn.enemy];
                int   total = EnemyCountCalculator.CalculateCount(spawn.count, wave);
                float hp    = EnemyHpCalculator.CalculateHp(spawn.hp, wave, stats.hp);

                int spawned = 0;
                float delay = float.Parse(spawn.delay);
                foreach (int batch in spawn.sequence)
                {
                    for (int i = 0; i < batch && spawned < total; i++)
                    {
                        SpawnSingle(stats, hp);
                        spawned++;
                    }
                    if (spawned < total) yield return new WaitForSeconds(delay);
                }
                while (spawned < total) { SpawnSingle(stats, hp); spawned++; }
            }

            // wait until all are cleared
            yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
            GameManager.Instance.state = GameManager.GameState.WAVEEND;
        }
    }

    //  create enemy object one by one
    void SpawnSingle(EnemyData stats, float hp)
    {
        var sp  = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        var pos = sp.transform.position + (Vector3)(Random.insideUnitCircle * 1.8f);
        var obj = Instantiate(enemy, pos, Quaternion.identity);
        obj.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(stats.sprite);

        var ec = obj.GetComponent<EnemyController>();
        ec.hp    = new Hittable(Mathf.RoundToInt(hp), Hittable.Team.MONSTERS, obj);
        ec.speed = Mathf.RoundToInt(stats.speed);

        GameManager.Instance.AddEnemy(obj);
    }
}

// JSON containers
[System.Serializable] public class WaveData  { public string name;  public int waves;  public List<SpawnData> spawns; }
[System.Serializable] public class SpawnData { public string enemy; public string count; public string hp; public string delay; public int[] sequence; }
[System.Serializable] public class EnemyData  { public string name;  public int sprite; public float hp; public float speed; public float damage; }
