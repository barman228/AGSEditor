using UnityEngine;
using UnityEngine.UI;

public class SubtitleItemUI : MonoBehaviour
{
    public InputField subtitleTextInput; // ���� ��� �������������� ������ ��������
    public InputField subtitleTimeInput; // ���� ��� �������������� ������� ��������
    public Button setTimeButton;        // ������ ��������� �������
    public Button removeButton;         // ������ �������� ������

    private float time;                 // ����� ��������
    private System.Action<SubtitleItemUI, float, string> onUpdate; // Callback ��� ����������
    private System.Action<SubtitleItemUI> onRemove;               // Callback ��� ��������

    public float Time => time;          // ����� ��������
    public string Text => subtitleTextInput.text; // ����� ��������

    public void Initialize(float initialTime, string initialText,
        System.Action<SubtitleItemUI, float, string> onUpdateCallback,
        System.Action<SubtitleItemUI> onRemoveCallback)
    {
        time = initialTime;
        subtitleTextInput.text = initialText;
        subtitleTimeInput.text = initialTime.ToString("F3");
        onUpdate = onUpdateCallback;
        onRemove = onRemoveCallback;

        // ����������� �������� � �������
        setTimeButton.onClick.AddListener(SetCurrentAudioTime);
        removeButton.onClick.AddListener(RemoveSubtitle);

        // ������� ��������� ������
        subtitleTextInput.onEndEdit.AddListener(OnTextChanged);
        subtitleTimeInput.onEndEdit.AddListener(OnTimeChanged);
    }

    private void SetCurrentAudioTime()
    {
        time = FindObjectOfType<AudioSource>().time; // ������������� ������� ����� �����
        subtitleTimeInput.text = time.ToString("F3");
        onUpdate?.Invoke(this, time, subtitleTextInput.text); // �������� ����������
    }

    private void OnTextChanged(string newText)
    {
        onUpdate?.Invoke(this, time, newText); // ��������� ������
    }

    private void OnTimeChanged(string newTimeText)
    {
        if (float.TryParse(newTimeText, out float newTime))
        {
            time = newTime;
            onUpdate?.Invoke(this, time, subtitleTextInput.text); // ��������� ������
        }
        else
        {
            Debug.LogWarning("Invalid time format in subtitle.");
            subtitleTimeInput.text = time.ToString("F3"); // ���������� �� ������� �����
        }
    }

    private void RemoveSubtitle()
    {
        onRemove?.Invoke(this); // �������� ��������
        Destroy(gameObject);    // ������� ������
    }
}

