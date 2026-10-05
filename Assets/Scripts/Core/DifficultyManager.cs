using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    [Header("Difficulty")]
    [SerializeField] private Difficulty currentDifficulty =
        Difficulty.Medium;

    [Header("Enemy Density")]
    [SerializeField] private int easyMaxEnemies = 8;
    [SerializeField] private int mediumMaxEnemies = 14;
    [SerializeField] private int hardMaxEnemies = 22;

    [Header("Base Spawn Interval")]
    [SerializeField] private float easySpawnInterval = 1.8f;
    [SerializeField] private float mediumSpawnInterval = 1.2f;
    [SerializeField] private float hardSpawnInterval = 0.75f;

    public Difficulty CurrentDifficulty =>
        currentDifficulty;

    public int GetMaxEnemies()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                return easyMaxEnemies;

            case Difficulty.Medium:
                return mediumMaxEnemies;

            case Difficulty.Hard:
                return hardMaxEnemies;

            default:
                return mediumMaxEnemies;
        }
    }

    public float GetBaseSpawnInterval()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                return easySpawnInterval;

            case Difficulty.Medium:
                return mediumSpawnInterval;

            case Difficulty.Hard:
                return hardSpawnInterval;

            default:
                return mediumSpawnInterval;
        }
    }

    public void SetDifficulty(
        Difficulty difficulty
    )
    {
        currentDifficulty =
            difficulty;

        Debug.Log(
            "DIFFICULTY SET: " +
            currentDifficulty
        );
    }

    public void SetEasy()
    {
        SetDifficulty(
            Difficulty.Easy
        );
    }

    public void SetMedium()
    {
        SetDifficulty(
            Difficulty.Medium
        );
    }

    public void SetHard()
    {
        SetDifficulty(
            Difficulty.Hard
        );
    }
}