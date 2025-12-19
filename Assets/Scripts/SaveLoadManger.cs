using UnityEngine;

public class SaveLoadManger : MonoBehaviour
{
    public static SaveLoadManger Instance;
    static string _gameFrameKey = "game_frame";

    void Awake()
    {
        Instance = this;
    }
    public void Save(GamePanel gameFrame)
    {
        string jsonDataToSave = JsonUtility.ToJson(gameFrame);
        PlayerPrefs.SetString(_gameFrameKey, jsonDataToSave);
        PlayerPrefs.Save();
    }

    public bool CanLoad()
    {
        return PlayerPrefs.HasKey(_gameFrameKey);
    }

    public GamePanel Load()
    {
        GamePanel loadedGameFrame = null;
        if (CanLoad())
        {
            string jsonGameFrame = PlayerPrefs.GetString(_gameFrameKey);
            loadedGameFrame = JsonUtility.FromJson<GamePanel>(jsonGameFrame);
        }
        return loadedGameFrame;
    }

    public void ClearData()
    {
        PlayerPrefs.DeleteKey(_gameFrameKey);
        PlayerPrefs.Save();
    }
}
