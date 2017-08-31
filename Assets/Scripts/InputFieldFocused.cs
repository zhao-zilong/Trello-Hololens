using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class InputFieldFocused : MonoBehaviour {

    bool executed = false;

    public DictationRecognizer _dictationRecognizer;


    public void enableDictationRecognizer() {

        _dictationRecognizer = new DictationRecognizer();

        _dictationRecognizer.InitialSilenceTimeoutSeconds = 20f;

        _dictationRecognizer.DictationHypothesis += _dictationRecognizer_DictationHypothesis;

        _dictationRecognizer.DictationResult += _dictationRecognizer_DictationResult;

        _dictationRecognizer.DictationComplete += _dictationRecognizer_DictationComplete;
    }

    private void _dictationRecognizer_DictationComplete(DictationCompletionCause cause)
    {
        //_dictationRecognizer.Stop();
        Debug.Log("Dictation finished");
    }

    private void _dictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        Debug.Log("result: "+text);
        Debug.Log(text == "clear");
        if (text == "clear") {

            this.GetComponent<InputField>().text = string.Empty;
            return;
        
        }
        if (text == "delete") {

            if (this.GetComponent<InputField>().text != string.Empty && this.GetComponent<InputField>().text.LastIndexOf(' ') > 0) {
                //Debug.Log(this.GetComponent<InputField>().text.LastIndexOf(' '));
                this.GetComponent<InputField>().text = this.GetComponent<InputField>().text.Substring(0 , this.GetComponent<InputField>().text.LastIndexOf(' '));
                return;
            }
            if (this.GetComponent<InputField>().text != string.Empty && this.GetComponent<InputField>().text.LastIndexOf(' ') == -1) {
                this.GetComponent<InputField>().text = string.Empty;
                return;
            }
        }
        if (this.GetComponent<InputField>().text == string.Empty)
        {
            this.GetComponent<InputField>().text = text;
        }
        else
        {
            this.GetComponent<InputField>().text = this.GetComponent<InputField>().text + " " + text;
        }
    }

    private void _dictationRecognizer_DictationHypothesis(string text)
    {
        Debug.Log("i'm thinking");
    }
	
	// Update is called once per frame
	void Update () {

        if (this.GetComponent<InputField>().isFocused == true && !executed) {

            this.GetComponent<Image>().color = Color.grey;
            executed = !executed;
            enableDictationRecognizer();
            Debug.Log("PhraseRecognitionSystem" + PhraseRecognitionSystem.Status);
            Debug.Log("_dictationRecognizer" + _dictationRecognizer.Status);
            if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running)
            {
                Debug.Log("shutdown");
                PhraseRecognitionSystem.Shutdown();
            }
            if (_dictationRecognizer.Status == SpeechSystemStatus.Stopped)
            {
                _dictationRecognizer.Start();
            }
            Debug.Log("isFocused is true");
 
        }
        if (this.GetComponent<InputField>().isFocused == false && executed)
        {
            Debug.Log("isFocused is false");
            this.GetComponent<Image>().color = Color.white;
            executed = !executed;
            Debug.Log("PhraseRecognitionSystem" + PhraseRecognitionSystem.Status);
            Debug.Log("_dictationRecognizer" + _dictationRecognizer.Status);
            if (_dictationRecognizer.Status == SpeechSystemStatus.Running) {

                _dictationRecognizer.Dispose();
            }

            if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Stopped)
            {
                Debug.Log("restart");
                PhraseRecognitionSystem.Restart();
            }
            Debug.Log("isFocused is false");
        }
    }
}
