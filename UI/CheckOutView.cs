using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CheckOutView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI userText;
    [SerializeField] private Image aliveIcon;
    [SerializeField] private Image deadIcon;

    [SerializeField] private Transform noteParent;
    [SerializeField] private GameObject note;


    public void Init()
    {
        foreach (Transform child in noteParent) Destroy(child.gameObject);
        userText.SetText("");
        aliveIcon.enabled = false;
        deadIcon.enabled = false;
    }
    public void ViewData(string userName, bool isAlive)
    {
        userText.SetText(userName);
        aliveIcon.gameObject.SetActive(isAlive);
        deadIcon.gameObject.SetActive(!isAlive);
    }

    public void ViewData(string msg)
    {
        Instantiate(note, noteParent);
        TextMeshProUGUI noteText = note.GetComponent<TextMeshProUGUI>();
        noteText.SetText(msg);
    }
}

