using System.Collections.Generic;
using UnityEngine;
using System.IO;
[System.Serializable]
public class LifetimeStatsData
{
    public int totalHoleInOnes = 0;
    public int totalEagles = 0;
    public int totalBirdies = 0;
    public int totalPars = 0;

    // We use a List of a custom class instead of a Dictionary so Unity can serialize it to JSON
    public List<CourseRecord> courseRecords = new List<CourseRecord>();

    // NEW: The Tutorial Flag
    public bool hasSeenTutorial = false;
    public string playerName = "New Golfer";
}

[System.Serializable]
public class CourseRecord
{
    public string courseName;
    public int bestScore;

    public CourseRecord(string name, int score)
    {
        courseName = name;
        bestScore = score;
    }
}

public class PlayerStatsManager : MonoBehaviour
{
    // A singleton pattern so you can easily call this from anywhere (e.g., PlayerStatsManager.Instance.AddBirdie())
    public static PlayerStatsManager Instance { get; private set; }

    private LifetimeStatsData currentStats;
    private const string SAVE_KEY = "PlayerLifetimeStats";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // --- UPDATED: Ask PlayerPrefs what they played last time. 
            // The '0' is the default fallback if they have never played before.
            int profileToLoad = PlayerPrefs.GetInt("LastUsedProfile", 0);
            LoadProfile(profileToLoad);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- STAT TRACKING METHODS ---

    public void AddHoleInOne() { currentStats.totalHoleInOnes++; SaveStats(); }
    public void AddEagle() { currentStats.totalEagles++; SaveStats(); }
    public void AddBirdie() { currentStats.totalBirdies++; SaveStats(); }
    public void AddPar() { currentStats.totalPars++; SaveStats(); }

    // --- COURSE RECORD LOGIC ---

    public void UpdateCourseBestScore(string courseName, int finalScore)
    {
        // Check if we already have a record for this course
        CourseRecord existingRecord = currentStats.courseRecords.Find(record => record.courseName == courseName);

        if (existingRecord != null)
        {
            // Update it only if the new score is lower (better) than the old score
            if (finalScore < existingRecord.bestScore)
            {
                existingRecord.bestScore = finalScore;
                Debug.Log($"New best score for {courseName}: {finalScore}!");
            }
        }
        else
        {
            // No record exists yet, so this is automatically the best score
            currentStats.courseRecords.Add(new CourseRecord(courseName, finalScore));
            Debug.Log($"First time completing {courseName}. Score: {finalScore}");
        }

        SaveStats();
    }

    public int GetBestScoreForCourse(string courseName)
    {
        // Search the list for a matching name
        CourseRecord record = currentStats.courseRecords.Find(r => r.courseName == courseName);

        if (record != null)
        {
            return record.bestScore;
        }

        return 0; // Return 0 to indicate they haven't played it yet
    }

    // --- UPDATED: SAVE & LOAD LOGIC ---

    // The currently active save slot (0, 1, or 2)
    public int currentProfileIndex = 0;

    public void SetPlayerName(string newName)
    {
        currentStats.playerName = newName;
        SaveStats(); // Instantly save to the hard drive when they change it
    }

    public void RenameProfile(int profileIndex, string newName)
    {
        string path = GetSavePath(profileIndex);

        if (File.Exists(path))
        {
            // 1. Read the existing save file
            string json = File.ReadAllText(path);

            // 2. Load it into a temporary container (so it doesn't overwrite your active game!)
            LifetimeStatsData tempData = JsonUtility.FromJson<LifetimeStatsData>(json);

            // 3. Change the name
            tempData.playerName = newName;

            // 4. Save the temporary container back to the hard drive
            string newJson = JsonUtility.ToJson(tempData, true);
            File.WriteAllText(path, newJson);

            Debug.Log($"Renamed Profile {profileIndex + 1} to {newName} in storage.");

            // SAFETY NET: If they just renamed the profile they are CURRENTLY playing on,
            // we need to update the active game memory too so they don't go out of sync!
            if (currentProfileIndex == profileIndex)
            {
                currentStats.playerName = newName;
            }
        }
        else
        {
            // Optional: If they rename an "Empty Slot", you can choose to generate a 
            // brand new save file for them right here!
            LifetimeStatsData newData = new LifetimeStatsData();
            newData.playerName = newName;

            string newJson = JsonUtility.ToJson(newData, true);
            File.WriteAllText(path, newJson);

            Debug.Log($"Created new save for Profile {profileIndex + 1} with name {newName}.");
        }
    }

    // --- NEW: Peek at a save file without actually loading it! ---
    public string GetProfileName(int profileIndex)
    {
        string path = GetSavePath(profileIndex);

        // If the file exists, read it and grab the name
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            // We load it into a temporary variable so it doesn't overwrite your active 'currentStats'
            LifetimeStatsData tempData = JsonUtility.FromJson<LifetimeStatsData>(json);
            return tempData.playerName;
        }

        // If the file doesn't exist, let the menu know it's an empty slot!
        return "Golfer " + (profileIndex + 1);
    }



    // We now pass the index into the path!
    private string GetSavePath(int profileIndex)
    {
        return Application.persistentDataPath + $"/lifetimeStats_{profileIndex}.json";
    }

    private void SaveStats()
    {
        string json = JsonUtility.ToJson(currentStats, true);

        // Save to whichever profile is currently active
        string path = GetSavePath(currentProfileIndex);

        File.WriteAllText(path, json);
        Debug.Log($"Saved stats to Profile {currentProfileIndex + 1}: " + path);
    }

    // Call this from your Main Menu to switch active saves!
    public void LoadProfile(int profileIndex)
    {
        currentProfileIndex = profileIndex;
        string path = GetSavePath(profileIndex);

        // --- NEW: Save this profile index to the computer's registry ---
        PlayerPrefs.SetInt("LastUsedProfile", profileIndex);
        PlayerPrefs.Save();

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            currentStats = JsonUtility.FromJson<LifetimeStatsData>(json);
            Debug.Log($"Loaded Profile {profileIndex + 1} for {currentStats.playerName}!");
        }
        else
        {
            currentStats = new LifetimeStatsData();
            // Optional: Give it a default name like "Player 1", "Player 2", etc.
            currentStats.playerName = "Golfer " + (profileIndex + 1);
            Debug.Log($"Created new blank save for Profile {profileIndex + 1}.");
        }
    }

    // Optional but highly recommended: A way to wipe a save slot clean
    public void DeleteProfile(int profileIndex)
    {
        string path = GetSavePath(profileIndex);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Deleted Profile {profileIndex + 1}.");
        }

        // If we just deleted the profile we are currently using, reload it as a blank slate
        if (currentProfileIndex == profileIndex)
        {
            LoadProfile(profileIndex);
        }
    }

    // Optional: A way to retrieve the stats to display on a UI menu
    public LifetimeStatsData GetCurrentStats()
    {
        return currentStats;
    }

    public bool HasSeenTutorial()
    {
        return currentStats.hasSeenTutorial;
    }

    public void MarkTutorialAsSeen()
    {
        currentStats.hasSeenTutorial = true;
        SaveStats();
    }
}