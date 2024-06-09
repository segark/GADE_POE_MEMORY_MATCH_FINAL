using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scenes : MonoBehaviour
{
    // Method to load the menu scene
    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu"); // Replace with your actual menu scene name
    }

    // Method to reload the current scene
    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // Method to load the Human vs. AI scene
    public void LoadHumanVsAIScene()
    {
        SceneManager.LoadScene("MemoryMatch"); // Replace with your actual Human vs. AI scene name
    }
    public void LoadHumanVsHumanScene()
    {
        SceneManager.LoadScene("Human"); // Replace with your actual Human vs. AI scene name
    }
    public void EndGame()
    {
        Application.Quit();
    }
}
