using UnityEngine;
using UnityEngine.UI;

public class SubtitleItemUI : MonoBehaviour
{
    public InputField subtitleTextInput; // Поле для редактирования текста субтитра
    public InputField subtitleTimeInput; // Поле для редактирования времени субтитра
    public Button setTimeButton;        // Кнопка установки времени
    public Button removeButton;         // Кнопка удаления строки

    private float time;                 // Время субтитра
    private System.Action<SubtitleItemUI, float, string> onUpdate; // Callback для обновления
    private System.Action<SubtitleItemUI> onRemove;               // Callback для удаления

    public float Time => time;          // Время субтитра
    public string Text => subtitleTextInput.text; // Текст субтитра

    public void Initialize(float initialTime, string initialText,
        System.Action<SubtitleItemUI, float, string> onUpdateCallback,
        System.Action<SubtitleItemUI> onRemoveCallback)
    {
        time = initialTime;
        subtitleTextInput.text = initialText;
        subtitleTimeInput.text = initialTime.ToString("F3");
        onUpdate = onUpdateCallback;
        onRemove = onRemoveCallback;

        // Привязываем действия к кнопкам
        setTimeButton.onClick.AddListener(SetCurrentAudioTime);
        removeButton.onClick.AddListener(RemoveSubtitle);

        // Слушаем изменения текста
        subtitleTextInput.onEndEdit.AddListener(OnTextChanged);
        subtitleTimeInput.onEndEdit.AddListener(OnTimeChanged);
    }

    private void SetCurrentAudioTime()
    {
        time = FindObjectOfType<AudioSource>().time; // Устанавливаем текущее время аудио
        subtitleTimeInput.text = time.ToString("F3");
        onUpdate?.Invoke(this, time, subtitleTextInput.text); // Вызываем обновление
    }

    private void OnTextChanged(string newText)
    {
        onUpdate?.Invoke(this, time, newText); // Обновляем данные
    }

    private void OnTimeChanged(string newTimeText)
    {
        if (float.TryParse(newTimeText, out float newTime))
        {
            time = newTime;
            onUpdate?.Invoke(this, time, subtitleTextInput.text); // Обновляем данные
        }
        else
        {
            Debug.LogWarning("Invalid time format in subtitle.");
            subtitleTimeInput.text = time.ToString("F3"); // Сбрасываем на текущее время
        }
    }

    private void RemoveSubtitle()
    {
        onRemove?.Invoke(this); // Вызываем удаление
        Destroy(gameObject);    // Удаляем объект
    }
}

