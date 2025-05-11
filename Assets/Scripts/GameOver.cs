using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public Text scoreText;
    public Text levelText;

    void Start()
    {
        scoreText.text = "" + GameData.finalScore;
        levelText.text = "" + GameData.finalLevel;
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene("Tetris");
    }

    public void Menu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
