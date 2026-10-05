using UnityEngine;

public class InfiniteSpaceBackground : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private GameObject tilePrefab;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Grid")]
    [SerializeField] private int gridSize = 3;

    private GameObject[,] tiles;

    private float tileWidth;
    private float tileHeight;

    private Vector2 currentGridPosition;

    private void Start()
    {
        if (tilePrefab == null)
        {
            Debug.LogError(
                "InfiniteSpaceBackground: Tile Prefab is not assigned."
            );

            return;
        }

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player =
                    playerObject.transform;
            }
        }

        if (player == null)
        {
            Debug.LogError(
                "InfiniteSpaceBackground: Player not found."
            );

            return;
        }

        CreateGrid();
    }

    private void Update()
    {
        if (player == null)
            return;

        UpdateGrid();
    }

    private void CreateGrid()
    {
        GameObject referenceTile =
            Instantiate(
                tilePrefab,
                Vector3.zero,
                Quaternion.identity,
                transform
            );

        SpriteRenderer renderer =
            referenceTile.GetComponent<SpriteRenderer>();

        if (renderer == null)
        {
            Debug.LogError(
                "InfiniteSpaceBackground: Tile prefab needs a SpriteRenderer."
            );

            Destroy(referenceTile);

            return;
        }

        tileWidth =
            renderer.bounds.size.x;

        tileHeight =
            renderer.bounds.size.y;

        tiles =
            new GameObject[
                gridSize,
                gridSize
            ];

        Destroy(referenceTile);

        currentGridPosition =
            GetGridPosition(
                player.position
            );

        BuildGrid();
    }

    private void BuildGrid()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject tile =
                    Instantiate(
                        tilePrefab,
                        transform
                    );

                tiles[x, y] =
                    tile;
            }
        }

        PositionGrid();
    }

    private void UpdateGrid()
    {
        Vector2 newGridPosition =
            GetGridPosition(
                player.position
            );

        if (newGridPosition == currentGridPosition)
            return;

        currentGridPosition =
            newGridPosition;

        PositionGrid();
    }

    private Vector2 GetGridPosition(
        Vector3 worldPosition
    )
    {
        float gridX =
            Mathf.Floor(
                worldPosition.x /
                tileWidth
            );

        float gridY =
            Mathf.Floor(
                worldPosition.y /
                tileHeight
            );

        return new Vector2(
            gridX,
            gridY
        );
    }

    private void PositionGrid()
    {
        float centerX =
            currentGridPosition.x *
            tileWidth;

        float centerY =
            currentGridPosition.y *
            tileHeight;

        int halfGrid =
            gridSize / 2;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject tile =
                    tiles[x, y];

                float worldX =
                    centerX +
                    (x - halfGrid) *
                    tileWidth +
                    tileWidth * 0.5f;

                float worldY =
                    centerY +
                    (y - halfGrid) *
                    tileHeight +
                    tileHeight * 0.5f;

                tile.transform.position =
                    new Vector3(
                        worldX,
                        worldY,
                        10f
                    );
            }
        }
    }
}