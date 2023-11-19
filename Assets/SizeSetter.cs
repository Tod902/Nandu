using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SizeSetter : MonoBehaviour
{
    [SerializeField] private GameObject _SizeSetter;
    [SerializeField] private TMP_InputField _widthText;
    [SerializeField] private TMP_InputField _heightText;
    [SerializeField] private TMP_Text _errorText;

    private string _previousWidthText;
    private string _previousHeightText;
    //private bool _isFading;

    private void Start()
    {
        _errorText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (_previousWidthText != _widthText.text)
        {
            if (_widthText.text != "") if(!int.TryParse(_widthText.text, out int ignore)) _widthText.text = _previousWidthText;
            _previousWidthText = _widthText.text;
        }
        if (_previousHeightText != _heightText.text)
        {
            if (_widthText.text != "") if (!int.TryParse(_heightText.text, out int ignore)) _heightText.text = _previousHeightText;
            _previousHeightText = _heightText.text;
        }
    }

    public void Apply()
    {
        if (!int.TryParse(_widthText.text, out int width) || !int.TryParse(_heightText.text, out int height)) 
        {
            //DisplayErrorMessage();
            Fade.Instance.InOutTextFade(_errorText);
            return;
        }

        Debug.LogWarning(width + " " + height);
        if(!Grid.SetGridSize(new Vector2Int(width, height))) Fade.Instance.InOutTextFade(_errorText); 
        else Cancel();
    }

    public void Cancel() 
    {
        _errorText.gameObject.SetActive(false);
        _SizeSetter.SetActive(false);
        MenuStatus.ChangeStatus();
    }

    public void OpenSizeSetter()
    {
        _SizeSetter.SetActive(true);
        _widthText.text = "";
        _heightText.text = "";
        MenuStatus.ChangeStatus();
    }

    /*
    private void DisplayErrorMessage()
    {

        if (_isFading) return;
        StopAllCoroutines();
        StartCoroutine(FadeInOutText(4f));
    }

    private IEnumerator FadeInOutText(float duration)
    {
        _isFading = true;
        _errorText.color = new Color(_errorText.color.r, _errorText.color.g, _errorText.color.b, 0);
        _errorText.gameObject.SetActive(true);
        float t = 0f;
        while (t < duration / 2f)
        {
            yield return null;
            _errorText.color = new Color(_errorText.color.r, _errorText.color.g, _errorText.color.b, 2f * t / duration);
            t += Time.unscaledDeltaTime;
        }
        while (t < duration)
        {
            yield return null;
            _errorText.color = new Color(_errorText.color.r, _errorText.color.g, _errorText.color.b, 1f - (t - duration / 2f) / (duration / 2f));
            t += Time.unscaledDeltaTime;
        }
        _errorText.gameObject.SetActive(false);
        _isFading = false;
    }
    */
}
