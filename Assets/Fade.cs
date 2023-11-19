using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Fade : MonoBehaviour
{
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        MenuStatus.AddOnStatusChangeListener(StopAllFades);
    }
    public static Fade Instance { get; private set; } = null;
    private static Dictionary<TMP_Text, bool> _textIsFadingDict = new Dictionary<TMP_Text, bool>();

    public void InOutTextFade(TMP_Text text, float duration)
    {
        if (!_textIsFadingDict.ContainsKey(text))
        {
            _textIsFadingDict.Add(text, true);
        }
        else if (_textIsFadingDict[text] == true) return;
        else _textIsFadingDict[text] = true;
        StartFade(text, duration);
    }

    /// <summary>
    /// using a standard duration
    /// </summary>
    public void InOutTextFade(TMP_Text text)
    {
        InOutTextFade(text, 4f);
    }

    private void StartFade(TMP_Text text, float duration)
    {
        //Stop the active Fades

        //Since I can't change the values of the Dict during the loop this is used as a roundabout way to fix that issue (Error: InvalidOperationException: Collection was modified; enumeration operation may not execute.)
        List<TMP_Text> valuesToChange = new List<TMP_Text>();

        foreach (KeyValuePair<TMP_Text, bool> keyValuePair in _textIsFadingDict)
        {
            if (keyValuePair.Key != text)
            {
                if (keyValuePair.Value)
                {
                    StopAllCoroutines();
                    keyValuePair.Key.gameObject.SetActive(false);
                    valuesToChange.Add(keyValuePair.Key);
                }
            }
        }
        for (int i = 0; i < valuesToChange.Count; i++)
        {
            _textIsFadingDict[valuesToChange[i]] = false;
        }

        StartCoroutine(InOutText(duration, text));
    }

    private void StopAllFades()
    {
        List<TMP_Text> valuesToChange = new List<TMP_Text>();
        foreach (KeyValuePair<TMP_Text, bool> keyValuePair in _textIsFadingDict)
        {
            if (keyValuePair.Value)
            {
                StopAllCoroutines();
                keyValuePair.Key.gameObject.SetActive(false);
                valuesToChange.Add(keyValuePair.Key);
            } 
        }
        for (int i = 0; i < valuesToChange.Count; i++)
        {
            _textIsFadingDict[valuesToChange[i]] = false;
        }
    }

    public static IEnumerator InOutText(float duration, TMP_Text text)
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        text.gameObject.SetActive(true);
        float t = 0f;
        while (t < duration / 3f)
        {
            yield return null;
            text.color = new Color(text.color.r, text.color.g, text.color.b, 3f * t / duration);
            t += Time.unscaledDeltaTime;
        }
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
        while (t < duration / 3f * 2)
        {
            yield return null;
            t += Time.unscaledDeltaTime;
        }
        while (t < duration)
        {
            yield return null;
            text.color = new Color(text.color.r, text.color.g, text.color.b, 1f - (t - 2f * duration / 3f));
            t += Time.unscaledDeltaTime;
        }
        text.gameObject.SetActive(false);
        _textIsFadingDict[text] = false;
    }
}
