using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartCSManager : MonoBehaviour
{
    [SerializeField] private Button af_button; // cs technique "af"
    [SerializeField] private Button rf_button; // cs technique "rf"
    [SerializeField] private Button pc_button; // cs technique "pc"

    public string selected_technique = "af";
    private Color selectedColor = new Color32(0x5B, 0xB4, 0x03, 255); // #5BB403
    private Color baseColor = new Color32(0x20, 0x96, 0xF3, 255);     // #2096F3

    void Start()
    {
        // Assign button clicks
        af_button.onClick.AddListener(() => OnClick("af"));
        rf_button.onClick.AddListener(() => OnClick("rf"));
        pc_button.onClick.AddListener(() => OnClick("pc"));

        SelectButton(af_button, "af");

    }

    public void OnClick(string technique)
    {
        if (technique == "af")
            SelectButton(af_button, "af");
        else if (technique == "rf")
            SelectButton(rf_button, "rf");
        else if (technique == "pc")
            SelectButton(pc_button, "pc");
    }

    private void SelectButton(Button selectedButton, string technique)
    {
        // reset all
        SetButtonColor(af_button, baseColor);
        SetButtonColor(rf_button, baseColor);
        SetButtonColor(pc_button, baseColor);

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