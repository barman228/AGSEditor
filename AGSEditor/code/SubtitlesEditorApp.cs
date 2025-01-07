using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SFB;

public class SubtitlesEditorApp : MonoBehaviour
{
    public AudioSource audioSource;
    public Text currentSubtitleText;
    public Slider timeSlider;
    public Button playButton;
    public Button pauseButton;
    public Button stopButton;
    public Button addSubtitleButton;
    public Button saveButton;
    public Button selectAudioButton;
    public Button selectSubtitleFileButton;
    public InputField subtitleFilePathInput;
    public Transform subtitleListContent;
    public GameObject subtitleItemPrefab;
    public Text timing;
    public Text timingfull;

    private List<SubtitleLine> subtitleLines = new List<SubtitleLine>();
    private string subtitleFilePath;

    private void Start()
    {
        Screen.SetResolution(1280, 720, false);
        playButton.onClick.AddListener(PlayAudio);
        pauseButton.onClick.AddListener(PauseAudio);
        stopButton.onClick.AddListener(StopAudio);
        addSubtitleButton.onClick.AddListener(AddSubtitle);
        saveButton.onClick.AddListener(SaveSubtitles);
        selectAudioButton.onClick.AddListener(OpenAudioFile);
        selectSubtitleFileButton.onClick.AddListener(OpenSubtitleFile);
        timeSlider.onValueChanged.AddListener(OnTimeSliderChanged);
    }

    private void Update()
    {
        if (audioSource.isPlaying)
        {
            timeSlider.value = audioSource.time / audioSource.clip.length;
            UpdateCurrentSubtitle(audioSource.time);
            float tim = Mathf.Round(audioSource.time * 1000f) / 1000f;
            timing.text = tim.ToString();
        }
    }

    public void LoadSubtitles()
    {
        if (string.IsNullOrEmpty(subtitleFilePath) || !File.Exists(subtitleFilePath))
        {
            Debug.LogError("Subtitles file not found.");
            return;
        }

        subtitleLines.Clear();
        foreach (Transform child in subtitleListContent)
        {
            Destroy(child.gameObject);
        }

        string[] lines = File.ReadAllLines(subtitleFilePath);
        foreach (string line in lines)
        {
            if (line.StartsWith("[") && line.Contains("]"))
            {
                int closingBracketIndex = line.IndexOf(']');
                string timeString = line.Substring(1, closingBracketIndex - 1);
                string subtitleText = line.Substring(closingBracketIndex + 1);

                if (float.TryParse(timeString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float time))
                {
                    AddSubtitleItem(time, subtitleText);
                }
            }
        }
    }

    public void SaveSubtitles()
    {
        if (string.IsNullOrEmpty(subtitleFilePath))
        {
            Debug.LogError("Invalid subtitles file path.");
            return;
        }

        List<string> lines = new List<string>();
        foreach (var line in subtitleLines)
        {
            lines.Add($"[{line.time.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)}]{line.text}");
        }

        File.WriteAllLines(subtitleFilePath, lines);
    }

    private void PlayAudio()
    {
        audioSource.Play();
    }

    private void PauseAudio()
    {
        audioSource.Pause();
    }

    private void StopAudio()
    {
        audioSource.Stop(); audioSource.time = 0;
    }

    private void OnTimeSliderChanged(float value)
    {
        if (audioSource.clip != null)
        {
            audioSource.time = value * audioSource.clip.length;
        }
    }

    private void UpdateCurrentSubtitle(float currentTime)
    {
        currentSubtitleText.text = "";

        for (int i = 0; i < subtitleLines.Count; i++)
        {
            if (i < subtitleLines.Count - 1)
            {
                if (currentTime >= subtitleLines[i].time && currentTime < subtitleLines[i + 1].time)
                {
                    currentSubtitleText.text = subtitleLines[i].text;
                    break;
                }
            }
            else
            {
                if (currentTime >= subtitleLines[i].time)
                {
                    currentSubtitleText.text = subtitleLines[i].text;
                    break;
                }
            }
        }
    }

    private void AddSubtitle()
    {
        if (audioSource != null)
        {
            AddSubtitleItem(audioSource.time, "New Subtitle");
        }
    }

    private void AddSubtitleItem(float time, string text)
    {
        GameObject subtitleItem = Instantiate(subtitleItemPrefab, subtitleListContent);
        SubtitleItemUI itemUI = subtitleItem.GetComponent<SubtitleItemUI>();
        itemUI.Initialize(time, text, OnSubtitleItemUpdated, OnSubtitleItemRemoved);

        subtitleLines.Add(new SubtitleLine(time, text));
    }

    private void OnSubtitleItemUpdated(SubtitleItemUI itemUI, float updatedTime, string updatedText)
    {
        // Ищем строку, связанную с UI
        SubtitleLine line = subtitleLines.Find(l => Mathf.Approximately(l.time, itemUI.Time));
        if (line != null)
        {
            // Обновляем данные
            line.time = updatedTime;
            line.text = updatedText;
        }
        else
        {
            Debug.LogWarning("Subtitle line not found for update!");
        }
    }

    private void OnSubtitleItemRemoved(SubtitleItemUI itemUI)
    {
        SubtitleLine line = subtitleLines.Find(l => Mathf.Approximately(l.time, itemUI.Time));
        if (line != null)
        {
            subtitleLines.Remove(line);
        }

        Destroy(itemUI.gameObject);
    }

    private void OpenAudioFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Audio File", "", "mp3", false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            StartCoroutine(LoadAudioClip(paths[0]));
        }
    }

    private IEnumerator<WWW> LoadAudioClip(string path)
    {
        using (var www = new WWW("file://" + path))
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                audioSource.clip = www.GetAudioClip(); float timf = Mathf.Round(audioSource.clip.length * 1000f) / 1000f; timingfull.text = timf.ToString();
            }
            else
            {
                Debug.LogError("Error loading audio: " + www.error);
            }
        }
    }

    private void OpenSubtitleFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Subtitle File", "", "txt", false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            subtitleFilePath = paths[0];
            subtitleFilePathInput.text = subtitleFilePath;
            LoadSubtitles();
        }
    }

    private class SubtitleLine
    {
        public float time;
        public string text;

        public SubtitleLine(float time, string text)
        {
            this.time = time;
            this.text = text;
        }
    }
}
