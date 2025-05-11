using UnityEngine;

// Classe que representa uma peça do Tetris
public class Piece : MonoBehaviour
{
    // Propriedades principais da peça
    public Board board { get; private set; } // Referência ao tabuleiro
    public TetrominoData data { get; private set; } // Dados do Tetromino associado
    public Vector3Int[] cells { get; private set; } // Posições das células da peça
    public Vector3Int position { get; private set; } // Posição atual da peça no tabuleiro
    public int rotationIndex { get; private set; } // Índice da rotação atual

    // Configurações de tempo para movimentos e bloqueios
    public float stepDelay = 1f; // Tempo entre descidas automáticas
    public float moveDelay = 0.1f; // Tempo entre movimentos laterais
    public float lockDelay = 0.5f; // Tempo antes de bloquear a peça

    // Temporizadores internos
    private float stepTime; // Temporizador para descida automática
    private float moveTime; // Temporizador para movimentos laterais
    private float lockTime; // Temporizador para bloqueio da peça

    // Inicializa a peça com os dados fornecidos
    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.data = data;
        this.board = board;
        this.position = position;

        rotationIndex = 0; // Define a rotação inicial
        stepTime = Time.time + stepDelay; // Define o tempo para a próxima descida
        moveTime = Time.time + moveDelay; // Define o tempo para o próximo movimento
        lockTime = 0f; // Reseta o temporizador de bloqueio

        if (cells == null) {
            cells = new Vector3Int[data.cells.Length]; // Inicializa as células
        }

        // Copia as posições das células do Tetromino
        for (int i = 0; i < cells.Length; i++) {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }

    // Atualiza a peça a cada frame
    private void Update()
    {
        board.Clear(this); // Limpa a peça do tabuleiro

        lockTime += Time.deltaTime; // Incrementa o temporizador de bloqueio

        // Lida com a rotação
        if (Input.GetKeyDown(KeyCode.Q)) {
            Rotate(-1); // Roda para a esquerda
        } else if (Input.GetKeyDown(KeyCode.E)) {
            Rotate(1); // Roda para a direita
        }

        // Lida com o "hard drop" (queda instantânea)
        if (Input.GetKeyDown(KeyCode.Space)) {
            HardDrop();
        }

        // Lida com movimentos laterais e descida suave
        if (Time.time > moveTime) {
            HandleMoveInputs();
        }

        // Move a peça para baixo automaticamente
        if (Time.time > stepTime) {
            Step();
        }

        board.Set(this); // Atualiza a peça no tabuleiro
    }

    // Lida com os inputs de movimento
    private void HandleMoveInputs()
    {
        if (Input.GetKey(KeyCode.S)) // Movimento para baixo
        {
            if (Move(Vector2Int.down)) {
                stepTime = Time.time + stepDelay; // Atualiza o temporizador de descida
            }
        }

        if (Input.GetKey(KeyCode.A)) { // Movimento para a esquerda
            Move(Vector2Int.left);
        } else if (Input.GetKey(KeyCode.D)) { // Movimento para a direita
            Move(Vector2Int.right);
        }
    }

    // Move a peça para baixo automaticamente
    private void Step()
    {
        stepTime = Time.time + stepDelay; // Atualiza o temporizador de descida
        Move(Vector2Int.down); // Move para baixo

        if (lockTime >= lockDelay) { // Bloqueia a peça se o tempo de bloqueio for excedido
            Lock();
        }
    }

    // Faz a peça cair instantaneamente
    private void HardDrop()
    {
        while (Move(Vector2Int.down)) {
            continue;
        }

        Lock(); // Bloqueia a peça
    }

    // Bloqueia a peça no tabuleiro
    private void Lock()
    {
        board.Set(this); // Define a peça no tabuleiro
        board.ClearLines(); // Limpa linhas completas
        board.SpawnPiece(); // Gera uma nova peça
    }

    // Move a peça na direção especificada
    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition); // Verifica se a posição é válida

        if (valid) {
            position = newPosition; // Atualiza a posição
            moveTime = Time.time + moveDelay; // Atualiza o temporizador de movimento
            lockTime = 0f; // Reseta o temporizador de bloqueio
        }

        return valid;
    }

    // Roda a peça na direção especificada
    private void Rotate(int direction)
    {
        int originalRotation = rotationIndex; // Guarda a rotação original
        rotationIndex = Wrap(rotationIndex + direction, 0, 4); // Atualiza a rotação
        ApplyRotationMatrix(direction); // Aplica a matriz de rotação

        if (!TestWallKicks(rotationIndex, direction)) { // Testa os "wall kicks"
            rotationIndex = originalRotation; // Reverte a rotação se falhar
            ApplyRotationMatrix(-direction); // Reverte a matriz de rotação
        }
    }

    // Aplica a matriz de rotação às células da peça
    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        for (int i = 0; i < cells.Length; i++) {
            Vector3 cell = cells[i];

            int x, y;

            switch (data.tetromino) {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            cells[i] = new Vector3Int(x, y, 0);
        }
    }

    // Testa os "wall kicks" para ajustar a posição da peça após a rotação
    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++) {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];

            if (Move(translation)) {
                return true;
            }
        }

        return false;
    }

    // Obtém o índice do "wall kick" com base na rotação
    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0) {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    // Garante que o índice está dentro dos limites especificados
    private int Wrap(int input, int min, int max)
    {
        if (input < min) {
            return max - (min - input) % (max - min);
        } else {
            return min + (input - min) % (max - min);
        }
    }
}
