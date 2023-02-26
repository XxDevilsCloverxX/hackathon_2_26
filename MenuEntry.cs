using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// class to set a menu entry with action to report its id when pressed
/// </summary>
public class MenuEntry : MonoBehaviour
{

    public TMP_Text text;
    private int menuItem; //index of item in menu


    public void setMenuID(int id) { menuItem = id; }
    public int getMenuID(int id) { return menuItem; }
    public void setName(string name) { text.text = name; }

    public delegate void UIAction(int id);
    UIAction thisaction;
    public void setButtonAction(UIAction function)
    {
        thisaction = function;
    }

    public void onPress() { print("pressed");  thisaction(menuItem);  }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
