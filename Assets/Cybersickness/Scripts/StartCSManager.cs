using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class StartCSManager : MonoBehaviour
{
    [SerializeField] private Button none_button; // cs technique "none"
    [SerializeField] private Button af_button; // cs technique "af"
    [SerializeField] private Button hel_button; // cs technique "hel"
    [SerializeField] private Button af_hel_button; // cs technique "af_hel"

    [NonSerialized] public string selected_technique = "none";
    private Color selectedColor = new Color32(0x5B, 0xB4, 0x03, 255); // #5BB403
    private Color baseColor = new Color32(0x20, 0x96, 0xF3, 255);     // #2096F3

    void Start()
    {
        // Assign button clicks
        none_button.onClick.AddListener(() => OnClick("none"));
        af_button.onClick.AddListener(() => OnClick("af"));
        hel_button.onClick.AddListener(() => OnClick("hel"));
        af_hel_button.onClick.AddListener(() => OnClick("af_hel"));

        SelectButton(none_button, "none");

    }

    public void OnClick(string technique)
    {
        if (technique == "none")
            SelectButton(none_button, "none");
        else if (technique == "af")
            SelectButton(af_button, "af");
        else if (technique == "hel")
            SelectButton(hel_button, "hel");
        else if (technique == "af_hel")
            SelectButton(af_hel_button, "af_hel");
    }

    private void SelectButton(Button selectedButton, string technique)
    {
        // reset all
        SetButtonColor(none_button, baseColor);
        SetButtonColor(af_button, baseColor);
        SetButtonColor(hel_button, baseColor);
        SetButtonColor(af_hel_button, baseColor);

        // set this to selected
        SetButtonColor(selectedButton, selectedColor);

        selected_technique = technique;
    }

    private void SetButtonColor(Button button, Color color)
    {
        Image childImage = button.transform.Find("Image").GetComponent<Image>();
        childImage.color = color;
    }
}