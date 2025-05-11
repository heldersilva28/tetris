using UnityEngine;
using UnityEngine.Tilemaps;

// Classe que representa a peça fantasma (Ghost Piece)
public class Ghost : MonoBehaviour
{
    // Propriedades principais
    public Tile tile; // Tile gráfico da peça fantasma
    public Board mainBoard; // Referência ao tabuleiro principal
    public Piece trackingPiece; // Peça que a fantasma segue

    // Propriedades internas
    public Tilemap tilemap { get; private set; } // Tilemap da peça fantasma
    public Vector3Int[] cells { get; private set; } // Células da peça fantasma
    public Vector3Int position { get; private set; } // Posição da peça fantasma

    // Inicialização
    private void Awake()
    {
        // Inicializa o tilemap e as células da peça fantasma
        tilemap = GetComponentInChildren<Tilemap>();
        cells = new Vector3Int[4];
    }

    // Atualização tardia para sincronizar com a peça principal
    private void LateUpdate()
    {
        // Limpa os tiles antigos, copia as células da peça principal, faz a peça fantasma cair e define os novos tiles
        Clear();
        Copy();
        Drop();
        Set();
    }

    // Limpa os tiles da peça fantasma
    private void Clear()
    {
        // Remove os tiles da peça fantasma do tilemap
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    // Copia as células da peça principal
    private void Copy()
    {
        // Copia as células da peça principal para a peça fantasma
        for (int i = 0; i < cells.Length; i++) {
            cells[i] = trackingPiece.cells[i];
        }
    }

    // Faz a peça fantasma "cair" até a posição válida mais baixa
    private void Drop()
    {
        // Calcula a posição mais baixa válida para a peça fantasma
        Vector3Int position = trackingPiece.position;

        int current = position.y;
        int bottom = -mainBoard.boardSize.y / 2 - 1;

        mainBoard.Clear(trackingPiece);

        for (int row = current; row >= bottom; row--)
        {
            position.y = row;

            if (mainBoard.IsValidPosition(trackingPiece, position)) {
                this.position = position;
            } else {
                break;
            }
        }

        mainBoard.Set(trackingPiece);
    }

    // Define os tiles da peça fantasma no tilemap
    private void Set()
    {
        // Define os tiles da peça fantasma no tilemap com base na posição calculada
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, tile);
        }
    }

}
