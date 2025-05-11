using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; } // Tilemap do tabuleiro
    public Piece activePiece { get; private set; } // Peça ativa no tabuleiro

    public TetrominoData[] tetrominoes; // Dados de todas as peças Tetromino
    public Vector2Int boardSize = new Vector2Int(10, 20); // Tamanho do tabuleiro
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0); // Posição inicial para spawn das peças
    public int score { get; private set; } // Pontuação do jogador
    public int level { get; private set; } // Nível atual do jogador
    public int linesCleared { get; private set; } // Total de linhas limpas

    public Text numberScoreText; // Texto para exibir a pontuação
    public Text numberLevelText; // Texto para exibir o nível

    public RectInt Bounds
    {
        get
        {
            // Calcula os limites do tabuleiro
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < tetrominoes.Length; i++) {
            tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        // Inicia o jogo
        Time.timeScale = 1; // Despausa o jogo
        score = 0; // Inicializa a pontuação
        level = 0; // Inicializa o nível
        linesCleared = 0; // Inicializa o contador de linhas limpas
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random];

        activePiece.Initialize(this, spawnPosition, data);

        if (IsValidPosition(activePiece, spawnPosition)) {
            Set(activePiece);
        } else {
            GameOver();
        }
    }

    public void GameOver()
    {
        tilemap.ClearAllTiles();
        Debug.Log($"Game Over! Final Score: {score}");
        Time.timeScale = 0; // Pausa o jogo
        GameData.finalScore = score;
        GameData.finalLevel = level;
        // Exibe a tela de Game Over
        SceneManager.LoadScene("Game Over"); // Carrega a cena de Game Over
        //RestartGame();
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        // A posição é válida apenas se todas as células forem válidas
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            // Uma célula fora dos limites é inválida
            if (!bounds.Contains((Vector2Int)tilePosition)) {
                return false;
            }

            // Uma célula já ocupada por outra peça é inválida
            if (tilemap.HasTile(tilePosition)) {
                return false;
            }
        }

        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;
        int linesClearedThisTurn = 0; // Contador de linhas limpas nesta jogada

        // Limpa linhas de baixo para cima
        while (row < bounds.yMax)
        {
            if (IsLineFull(row)) {
                LineClear(row);
                linesClearedThisTurn++; // Incrementa o contador de linhas limpas
            } else {
                row++;
            }
        }

        if (linesClearedThisTurn > 0) {
            AddScore(linesClearedThisTurn); // Calcula a pontuação com base nas linhas limpas
            linesCleared += linesClearedThisTurn; // Atualiza o total de linhas limpas
            UpdateLevel(); // Atualiza o nível com base nas linhas limpas
        }
    }

    public bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            // A linha não está cheia se faltar uma célula
            if (!tilemap.HasTile(position)) {
                return false;
            }
        }

        return true;
    }

    public void LineClear(int row)
    {
        RectInt bounds = Bounds;

        // Limpa todas as células na linha
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        // Move todas as linhas acima para baixo
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }

            row++;
        }
    }

    private void AddScore(int lines)
    {
        int points = 0;

        // Calcula a pontuação com base no número de linhas limpas
        switch (lines)
        {
            case 1: points = 40 * (level + 1); break;
            case 2: points = 100 * (level + 1); break;
            case 3: points = 300 * (level + 1); break;
            case 4: points = 1200 * (level + 1); break;
        }

        score += points;
        Debug.Log($"Score: {score}");

        if (numberScoreText != null) {
            numberScoreText.text = score.ToString();
        }
    }

    private void UpdateLevel()
    {
        // Atualiza o nível a cada 10 linhas limpas
        level = linesCleared / 5;
        Debug.Log($"Level: {level}");
        if (numberLevelText != null) {
            numberLevelText.text = level.ToString();
        }
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reinicia a cena atual
    }
}
