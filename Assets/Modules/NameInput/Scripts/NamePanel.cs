using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Threading;
using static UnityEngine.UI.CanvasScaler;
using UnityEditor;
using UnityEngine.SceneManagement;
using Fusion;

public class NamePanel : MonoBehaviour
{
    private List<string[]> ALPHABET_LOWER = new List<string[]>() {
        new string[] { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p" },
        new string[] { "a", "s", "d", "f", "g", "h", "j", "k", "l" },
        new string[] { "z", "x", "c", "v", "b", "n", "m", "-" }
    };

    private List<string[]> ALPHABET_UPPER = new List<string[]>() {
        new string[] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
        new string[] { "A", "S", "D", "F", "G", "H", "J", "K", "L" },
        new string[] { "Z", "X", "C", "V", "B", "N", "M", "-" }
    };

    private List<string[]> HIRAGANA = new List<string[]>() {
        new string[] { "あ", "い", "う", "え", "お" },
        new string[] { "か", "き", "く", "け", "こ" },
        new string[] { "さ", "し", "す", "せ", "そ" },
        new string[] { "た", "ち", "つ", "て", "と" },
        new string[] { "な", "に", "ぬ", "ね", "の" },
        new string[] { "は", "ひ", "ふ", "へ", "ほ" },
        new string[] { "ま", "み", "む", "め", "も" },
        new string[] { "や", "", "ゆ", "", "よ" },
        new string[] { "ら", "り", "る", "れ", "ろ" },
        new string[] { "わ", "", "を", "", "ん" },
        new string[] { "ゃ", "", "ゅ", "", "ょ" },
        new string[] { "ー", "", "っ", "", "～" },
        new string[] { "が", "ぎ", "ぐ", "げ", "ご" },
        new string[] { "ざ", "じ", "ず", "ぜ", "ぞ" },
        new string[] { "だ", "ぢ", "づ", "で", "ど" },
        new string[] { "ば", "び", "ぶ", "べ", "ぼ" },
        new string[] { "ぱ", "ぴ", "ぷ", "ぺ", "ぽ" }
    };

    private List<string[]> KATAKANA = new List<string[]>() {
        new string[] { "ア", "イ", "ウ", "エ", "オ" },
        new string[] { "カ", "キ", "ク", "ケ", "コ" },
        new string[] { "サ", "シ", "ス", "セ", "ソ" },
        new string[] { "タ", "チ", "ツ", "テ", "ト" },
        new string[] { "ナ", "ニ", "ヌ", "ネ", "ノ" },
        new string[] { "ハ", "ヒ", "フ", "ヘ", "ホ" },
        new string[] { "マ", "ミ", "ム", "メ", "モ" },
        new string[] { "ヤ", "", "ユ", "", "ヨ" },
        new string[] { "ラ", "リ", "ル", "レ", "ロ" },
        new string[] { "ワ", "", "ヲ", "", "ン" },
        new string[] { "ャ", "", "ュ", "", "ョ" },
        new string[] { "ー", "", "ッ", "", "～"},
        new string[] { "ガ", "ギ", "グ", "ゲ", "ゴ" },
        new string[] { "ザ", "ジ", "ズ", "ゼ", "ゾ" },
        new string[] { "ダ", "ヂ", "ヅ", "デ", "ド" },
        new string[] { "バ", "ビ", "ブ", "ベ", "ボ" },
        new string[] { "パ", "ピ", "プ", "ペ", "ポ" },
    };

    private Vector2 WORD_BUTTONS_SCALE_RATIOS = new Vector2(0.98f, 0.98f);
    private float WORD_BUTTON_TEXT_SCALE_RATIO = 0.7f;
    private int MAX_USER_NAME_COUNT = 10;

    [SerializeField] private TMP_InputField nameInputField = default;
    [SerializeField] private GameObject alphabetPanel = default;
    [SerializeField] private GameObject enOperationPanel = default;
    [SerializeField] private GameObject japanesePanel = default;
    [SerializeField] private GameObject jpOperationPanel = default;
    [SerializeField] private GameObject wordButtonPrefab;
    [SerializeField] public TextMeshProUGUI nameText = default;
    [SerializeField] private Button enTypeChangeButton = default;
    [SerializeField] private Button jpTypeChangeButton = default;
    [SerializeField] private NameInput nameInput = default;
    

    private List<string[]>[,] fullWords;
    private List<List<GameObject>> wordButtons = new List<List<GameObject>>();

    private int[] wordTypeIndex = { 0, 0 };

    private string oldName = default;
    private string currentName = default;

    // Start is called before the first frame update
    void Start()
    {
        this.fullWords = new List<string[]>[,] { { ALPHABET_LOWER, ALPHABET_UPPER }, { HIRAGANA, KATAKANA } };

        this.fullWords[1, 0] = this.PrepareWords(this.fullWords[1, 0]);
        this.fullWords[1, 1] = this.PrepareWords(this.fullWords[1, 1]);

        this.MakeWordButtons();
    }

    private List<string[]> PrepareWords(List<string[]> words)
    {
        List<List<string>> wordTempList = new List<List<string>>();

        foreach (string[] word in words)
        {
            wordTempList.Add(word.ToList());
        }

        List<List<string>> resultInList = this.TransposeColumnsAndRows(wordTempList, string.Empty);
        List<string[]> resultInArray = new List<string[]>();

        foreach (List<string> result in resultInList)
        {
            resultInArray.Add(result.ToArray());
        }
        return resultInArray;
    }

    public void Init(string currentName)
    {
        this.oldName = currentName;
        this.currentName = currentName;
        this.nameInputField.text = this.currentName;
    }

    private void MakeWordButtons()
    {
        if (this.wordButtons.Count != 0)
        {
            foreach (List<GameObject> wordButton in this.wordButtons)
            {
                foreach (GameObject wb in wordButton)
                {
                    Destroy(wb);
                }
                wordButton.Clear();
            }
            this.wordButtons.Clear();
        }

        GameObject target = default;
        RectTransform baseRect = default;

        List<string[]> fullWordTemps = this.fullWords[this.wordTypeIndex[0], this.wordTypeIndex[1]];

        if (this.wordTypeIndex[0] == 0 && this.wordTypeIndex[1] == 0 ||
            this.wordTypeIndex[0] == 0 && this.wordTypeIndex[1] == 1)
        {
            target = this.alphabetPanel;
            baseRect = target.GetComponent<RectTransform>();

            this.alphabetPanel.SetActive(true);
            this.enOperationPanel.SetActive(true);
            this.japanesePanel.SetActive(false);
            this.jpOperationPanel.SetActive(false);
        }

        if (this.wordTypeIndex[0] == 1 && this.wordTypeIndex[1] == 0 ||
            this.wordTypeIndex[0] == 1 && this.wordTypeIndex[1] == 1)
        {
            target = this.japanesePanel;
            baseRect = target.GetComponent<RectTransform>();

            this.alphabetPanel.SetActive(false);
            this.enOperationPanel.SetActive(false);
            this.japanesePanel.SetActive(true);
            this.jpOperationPanel.SetActive(true);
        }

        int maxLength = this.GetArrayMaxLength(this.fullWords[this.wordTypeIndex[0], this.wordTypeIndex[1]]);

        Vector2 initSize = new Vector2(
            baseRect.sizeDelta.x / maxLength * WORD_BUTTONS_SCALE_RATIOS.x,
            baseRect.sizeDelta.y / fullWordTemps.Count * WORD_BUTTONS_SCALE_RATIOS.y
        );

        Vector2 initPosition = new Vector2(
            -1.0f * (baseRect.sizeDelta.x - initSize.x) * 0.5f + ((1.0f - WORD_BUTTONS_SCALE_RATIOS.x) * initSize.x * 0.5f),
            (baseRect.sizeDelta.y - initSize.y) * 0.5f - ((1.0f - WORD_BUTTONS_SCALE_RATIOS.y) * initSize.y * 0.5f));

        float wordButtonDeltaPosX = (2.0f - WORD_BUTTONS_SCALE_RATIOS.x) * initSize.x;
        float wordButtonDeltaPosY = (2.0f - WORD_BUTTONS_SCALE_RATIOS.y) * initSize.y;

        for (int i = 0; i < fullWordTemps.Count; i++)
        {
            float column = initPosition.y - (wordButtonDeltaPosY * i);
            List<GameObject> wordButtonRows = new List<GameObject>();

            for (int j = 0; j < fullWordTemps[i].Length; j++)
            {
                if (fullWordTemps[i][j] != "")
                {
                    float row = initPosition.x + (wordButtonDeltaPosX * j);

                    GameObject wordButton = Instantiate(this.wordButtonPrefab);

                    wordButton.GetComponent<RectTransform>().sizeDelta = new Vector2(initSize.x, initSize.y);
                    wordButton.transform.position = new Vector3(row, column, 0.0f);

                    wordButton.transform.SetParent(target.transform, false);

                    TextMeshProUGUI wordButtonText = wordButton.GetComponentInChildren<TextMeshProUGUI>();
                    wordButtonText.text = fullWordTemps[i][j];
                    wordButtonText.fontSize = initSize.y * WORD_BUTTON_TEXT_SCALE_RATIO;

                    Button button = wordButton.GetComponent<Button>();
                    button.onClick.AddListener(() => this.OnButtonClicked(wordButton));

                    wordButton.name = "WordButton";
                    wordButtonRows.Add(wordButton);
                }
                else
                {
                    wordButtonRows.Add(null);
                }
            }
            this.wordButtons.Add(wordButtonRows);
        }
    }

    private int GetArrayMaxLength(List<string[]> targets)
    {
        int maxLength = 0;

        foreach (string[] target in targets)
        {
            if (maxLength < target.Length)
            {
                maxLength = target.Length;
            }
        }
        return maxLength;
    }

    public void OnTextFieldValueChanged()
    {
        this.currentName = this.nameInputField.text;
        this.nameText.text = this.currentName;
    }

    public void OnButtonClicked(GameObject button)
    {
        switch (button.name)
        {
            case "WordButton":

                if (this.nameInputField.text.Length < MAX_USER_NAME_COUNT)
                {
                    TextMeshProUGUI wordButtonText = button.GetComponentInChildren<TextMeshProUGUI>();

                    this.currentName += wordButtonText.text;
                    this.nameInputField.text = this.currentName;
                    this.nameText.text = this.nameInputField.text;
                }
                break;

            case "EnTypeChangeButton":

                if (this.wordTypeIndex[1] == 0)
                {
                    this.enTypeChangeButton.GetComponentInChildren<TextMeshProUGUI>().text = "↓";
                    this.wordTypeIndex[1] = 1;
                    this.MakeWordButtons();
                }
                else if (this.wordTypeIndex[1] == 1)
                {
                    this.enTypeChangeButton.GetComponentInChildren<TextMeshProUGUI>().text = "↑";
                    this.wordTypeIndex[1] = 0;
                    this.MakeWordButtons();
                }
                break;

            case "JpTypeChangeButton":

                if (this.wordTypeIndex[1] == 0)
                {
                    this.jpTypeChangeButton.GetComponentInChildren<TextMeshProUGUI>().text = "ひら\nがな";
                    this.wordTypeIndex[1] = 1;
                    this.MakeWordButtons();
                }
                else if (this.wordTypeIndex[1] == 1)
                {
                    this.jpTypeChangeButton.GetComponentInChildren<TextMeshProUGUI>().text = "カタ\nカナ";
                    this.wordTypeIndex[1] = 0;
                    this.MakeWordButtons();
                }
                break;

            case "EnLanguageButton":

                this.wordTypeIndex[0] = 1;
                this.wordTypeIndex[1] = 0;
                this.jpTypeChangeButton.GetComponentInChildren<TextMeshProUGUI>().text = "カタ\nカナ";
                this.MakeWordButtons();

                break;

            case "JpLanguageButton":

                this.wordTypeIndex[0] = 0;
                this.wordTypeIndex[1] = 0;
                this.enTypeChangeButton.GetComponentInChildren<TextMeshProUGUI>().text = "↑";
                this.MakeWordButtons();

                break;

            case "ResetButton":

                this.currentName = "";
                this.nameInputField.text = this.currentName;
                this.nameText.text = this.nameInputField.text;

                break;

            case "OKButton":

                this.nameInput.OnNameChanged(this.nameInputField.text);
                GetComponent<NameInput>().SetActive_PanelController(false);
                ES3.Save<string>("UserName", this.nameInputField.text);
                if (SceneManager.GetActiveScene().buildIndex == 1)
                {
                    if (PlayerObject.Local != null)
                        PlayerObject.Local.Rpc_SetNickname(this.nameInputField.text);
                }
                break;

            case "CancelButton":

                this.currentName = this.oldName;
                this.nameInputField.text = this.oldName;
                this.nameText.text = this.nameInputField.text;

                GetComponent<NameInput>().SetActive_PanelController(false);

                break;

            default:
                break;
        }

        if (button.name == "EnBackSpaceButton" || button.name == "JpBackSpaceButton")
        {
            this.currentName = this.currentName.Remove(this.currentName.Length - 1);
            this.nameInputField.text = this.currentName;
            this.nameText.text = this.nameInputField.text;
        }
    }

    private List<List<string>> TransposeColumnsAndRows(List<List<string>> targetList, string pad)
    {
        var resultList = new List<List<string>>();

        foreach (var row in targetList.Select((v, i) => new { v, i }))
        {
            while (resultList.Count() < row.v.Count())
                resultList.Add(new List<string>());

            foreach (var col in row.v.Select((v, i) => new { v, i }))
            {
                while (row.i > resultList[col.i].Count())
                    resultList[col.i].Add(string.Empty);

                resultList[col.i].Add(col.v);
            }
        }

        foreach (var row in resultList)
        {
            while (row.Count() < targetList.Count())
            {
                row.Add(pad);
            }
        }

        return resultList;
    }
}
