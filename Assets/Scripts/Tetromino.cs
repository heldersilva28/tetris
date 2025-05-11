using UnityEngine;
using UnityEngine.Tilemaps;

// Enum que define os diferentes tipos de Tetromino (peças do Tetris)
public enum Tetromino
{
    I, J, L, O, S, T, Z
}

// Estrutura que armazena os dados de um Tetromino
[System.Serializable]
public struct TetrominoData
{
    public Tile tile; // Representa o tile gráfico associado ao Tetromino
    public Tetromino tetromino; // Tipo do Tetromino

    // Propriedades privadas para as células e os "wall kicks"
    public Vector2Int[] cells { get; private set; } // Posições das células que compõem o Tetromino
    public Vector2Int[,] wallKicks { get; private set; } // Dados para os "wall kicks" (ajustes ao rodar)

    // Método para inicializar os dados do Tetromino com base nos dados estáticos
    public void Initialize()
    {
        cells = Data.Cells[tetromino]; // Obtém as células do Tetromino
        wallKicks = Data.WallKicks[tetromino]; // Obtém os "wall kicks" do Tetromino
    }
}
