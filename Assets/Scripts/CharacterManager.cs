using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

//[ExecuteInEditMode]
public class CharacterManager : MonoBehaviour
{
    [Header("Resouces")]
    public GameObject characterButtonPrefab;
    public Transform buttonContent;
    public Transform characterModelContent;

    [Header("Characters")]
    public List<CharacterItem> characters = new();

    [Header("Settings")]
    [SerializeField] private bool updateCharacter;
    [SerializeField] private UnityEvent<string> onCharacterChanged;
    public int currentCharacterIndex = 0;
    private int currentButtonIndex = 0;
    private int newCharacterIndex;

    private GameObject currentCharacter;
    private GameObject nextCharacter;
    private GameObject currentButton;
    private GameObject nextButton;

    private Animator currentButtonAnimator;
    private Animator nextButtonAnimator;

    string buttonFadeIn = "Normal to Pressed";
    string buttonFadeOut = "Pressed to Dissolve";

    [System.Serializable]
    public class CharacterItem
    {
        public string characterName;
        public GameObject characterModel;
        public Sprite characterImage;
        public GameObject buttonObject;
    }
    
    void OnEnable()
    {
        if(updateCharacter)
        {
            characters.Clear();
            var characterModels = Resources.LoadAll("Characters/Models", typeof(GameObject));
            var characterSprites = Resources.LoadAll("Characters/Sprites", typeof(Sprite));

            for(int i = 0; i < characterModels.Length; i++)
            {
                //キャラクターがすでにあるかどうか名前で判断
                var isExist = false;
                var characterName = characterModels[i].name;
                for(int j = 0; j < buttonContent.childCount; j++)
                    if(buttonContent.GetChild(j).name == characterName)
                        {isExist = true;    continue;}
                if(isExist) continue;

                //キャラクターのボタンを生成
                var characterButton = Instantiate(characterButtonPrefab, buttonContent);
                characterButton.name = characterName;
                characterButton.transform.Find("Content").Find("Background").Find("Image1").GetComponent<Image>().sprite = characterSprites[i] as Sprite; 

                //キャラクターのモデルを生成
                var characterModel = Instantiate(characterModels[i] as GameObject, characterModelContent);
                characterModel.name = characterModel.name.Replace("(Clone)", "");

                var characterItem = new CharacterItem();
                characterItem.characterName = characterName;
                characterItem.characterModel = characterModel;
                characterItem.characterImage = characterSprites[i] as Sprite;
                characterItem.buttonObject = characterButton;
                characters.Add(characterItem);
            }
            updateCharacter = false;
        }

        currentButton = characters[currentButtonIndex].buttonObject;
        currentButton.GetComponent<GazeController>().SetState(true);
        currentButtonAnimator = currentButton.GetComponent<Animator>(); 
        currentButtonAnimator.Play(buttonFadeIn);
    }

    void Awake()
    {
        var characterName = ES3.Load<string>("CharacterName", defaultValue:"Architect_Female_V1_00");
        for(int i = 0; i < characters.Count; i++)
        {
            var button = characters[i].buttonObject;
            button.GetComponent<Button>().onClick.AddListener(() => ChangeCharacter(button.name));
            characters[i].characterModel.gameObject.SetActive(false);
            if(characterName == characters[i].characterModel.name)
            {
                characters[i].characterModel.gameObject.SetActive(true);
                currentCharacterIndex = i;
                currentButtonIndex = i;
            }    
        }
    }

    public void ChangeCharacter(string newCharacter)
    {
        ES3.Save<string>("CharacterName", newCharacter);
        onCharacterChanged?.Invoke(newCharacter);
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].characterName == newCharacter)
            {
                newCharacterIndex = i;
                break;
            }
        }

        if (newCharacterIndex != currentCharacterIndex)
        {
            //変更前キャラクターと変更後キャラクターを取得
            currentCharacter = characters[currentCharacterIndex].characterModel;
            currentCharacterIndex = newCharacterIndex;
            nextCharacter = characters[currentCharacterIndex].characterModel;

            //キャラクターのアクティベーション管理
            currentCharacter.SetActive(false);
            nextCharacter.SetActive(true);

            //移動前ボタンと移動後ボタンを取得
            currentButton = characters[currentButtonIndex].buttonObject;
            currentButtonIndex = newCharacterIndex;
            nextButton = characters[currentButtonIndex].buttonObject;

            //ボタンのアニメーション管理
            currentButtonAnimator = currentButton.GetComponent<Animator>();
            nextButtonAnimator = nextButton.GetComponent<Animator>();    
            currentButton.GetComponent<GazeController>().SetState(false);
            currentButtonAnimator.Play(buttonFadeOut);
            nextButton.GetComponent<GazeController>().SetState(true);
            nextButtonAnimator.Play(buttonFadeIn);
        }
    }
}
