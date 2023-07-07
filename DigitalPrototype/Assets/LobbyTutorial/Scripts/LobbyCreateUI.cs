using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour {


    public static LobbyCreateUI Instance { get; private set; }


    [SerializeField] private Button createButton;
    [SerializeField] private Button lobbyNameButton;
    [SerializeField] private Button publicPrivateButton;
    [SerializeField] private TMP_Dropdown mapDropdown;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI publicPrivateText;
    [SerializeField] private TMP_Dropdown unitNumDropdown;
    [SerializeField] private Slider springsSlider;
    [SerializeField] private TextMeshProUGUI springsEcho;
    
    private string lobbyName = "newLobby";
    private bool isPrivate;
    private LobbyManager.Map map = LobbyManager.Map.map1;
    private int unitNumber = 2;
    private int startingSprings = 20;

    private void Awake()
    {
        Instance = this;

        createButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.CreateLobby(
                lobbyName,
                isPrivate,
                map,
                unitNumber,
                startingSprings
            );
            Hide();
        });

        lobbyNameButton.onClick.AddListener(() =>
        {
            UI_InputWindow.Show_Static("Lobby Name", lobbyName,
                "abcdefghijklmnopqrstuvxywzABCDEFGHIJKLMNOPQRSTUVXYWZ .,-", 20,
                () =>
                {
                    // Cancel
                },
                (string lobbyName) =>
                {
                    this.lobbyName = lobbyName;
                    UpdateText();
                });
        });

        publicPrivateButton.onClick.AddListener(() =>
        {
            isPrivate = !isPrivate;
            UpdateText();
        });


        mapDropdown.onValueChanged.AddListener(delegate
        {
            mapDropdownValueChanged(mapDropdown);
        });
        
        unitNumDropdown.onValueChanged.AddListener(delegate
        {
            unitDropdownValueChanged(unitNumDropdown);
        });
        
        springsSlider.onValueChanged.AddListener(delegate
        {
            sliderValueChanged(springsSlider);
        });

        unitNumDropdown.value = (unitNumber - 1);
        springsSlider.value = startingSprings;
        UpdateText();
        Hide();
    }

    private void sliderValueChanged(Slider change)
    {
        startingSprings = (int)change.value;
        UpdateText();
    }
    
    private void unitDropdownValueChanged(TMP_Dropdown change)
    {
        if (change.value == 0)
        {
            unitNumber = 1;
        }
        else if (change.value == 1)
        {
            unitNumber = 2;
        }
        else if (change.value == 2)
        {
            unitNumber = 3;
        }
        else if (change.value == 3)
        {
            unitNumber = 4;
        }
        else if (change.value == 4)
        {
            unitNumber = 5;
        }
        else if (change.value == 5)
        {
            unitNumber = 6;
        }
        else if (change.value == 6)
        {
            unitNumber = 7;
        }
        else if (change.value == 7)
        {
            unitNumber = 8;
        }
        else if (change.value == 8)
        {
            unitNumber = 9;
        }
        else if (change.value == 9)
        {
            unitNumber = 10;
        }
        
        UpdateText();
    }
    
    private void mapDropdownValueChanged(TMP_Dropdown change)
    {
        if (change.value == 0)
        {
            map = LobbyManager.Map.map1;
        }
        else if (change.value == 1)
        {
            map = LobbyManager.Map.map2;
        }
        else if (change.value == 2)
        {
            map = LobbyManager.Map.map3;
        }
        else if (change.value == 3)
        {
            map = LobbyManager.Map.map4;
        }
        else if (change.value == 4)
        {
            map = LobbyManager.Map.map5;
        }
        
        UpdateText();
    }

    private void UpdateText() {
        lobbyNameText.text = lobbyName;
        publicPrivateText.text = isPrivate ? "Private" : "Public";
        springsEcho.text = "" + startingSprings; 
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    public void Show() {
        gameObject.SetActive(true);

        lobbyName = "MyLobby";
        isPrivate = false;
        map = LobbyManager.Map.map1;

        UpdateText();
    }

}