using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ImageLoader : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Toggle  toggleButton;
    //[SerializeField] private RawImage buttonImage;
    [SerializeField] private Transform imageContent;
    [SerializeField] private RawImage previewImage;
    [SerializeField] private TextMeshProUGUI previewText;
    [SerializeField] private int imageWidth = 200; // 画像の横幅を指定
    private bool isSelected = false;
    private Dictionary<string, Texture2D> CharacterTextureByName = new();
    private const string ImagePath = "YOUR_RESOURCES/IMAGES";
    private Texture2D savedTex = null;
    private string savedTexName = "";
    private string savedCharacterName = "";
    string panelFadeIn = "Panel In";
    string panelFadeOut = "Panel Out";

    private Vector2 defaultPreviewSize; // デフォルトのRawImageサイズ
    private Vector2 defaultButtonSize;

    void Awake()
    {
        defaultPreviewSize = previewImage.rectTransform.sizeDelta;
        //defaultButtonSize = buttonImage.rectTransform.sizeDelta;
        isSelected = ES3.Load<bool>("IsSelected", defaultValue: false);
        var imageName = ES3.Load<string>("ImageName", defaultValue: "default_image");
        savedCharacterName = ES3.Load<string>("CharacterName", defaultValue: "Architect_Female_V1_00");

        var characterTextures = Resources.LoadAll("Characters/Sprites", typeof(Texture2D));
        foreach (var texture in characterTextures)
            CharacterTextureByName.Add(texture.name, texture as Texture2D);

        // exeファイルと同じ階層のYOUR_RESOURCES/IMAGES内の画像ファイルをすべて取得
        string exeDir = Directory.GetParent(Application.dataPath).FullName;
        string imagesDir = Path.Combine(exeDir, ImagePath);

        if (Directory.Exists(imagesDir))
        {
            var files = Directory.GetFiles(imagesDir, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                if (file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg"))
                {
                    byte[] bytes = File.ReadAllBytes(file);
                    Texture2D tex = new Texture2D(2, 2);
                    if (tex.LoadImage(bytes))
                    {
                        var buttonObj = Instantiate(buttonPrefab, imageContent);
                        var button = buttonObj.GetComponent<Button>();
                        string texName = Path.GetFileNameWithoutExtension(file);
                        tex = ResizeTextureToWidth(tex, imageWidth);
                        button.GetComponent<RawImage>().texture = tex;
                        button.onClick.AddListener(() => OnImageChanged(texName, tex));
                        if (imageName == texName)
                        {
                            ES3.Save<byte[]>("ImageByte", tex.EncodeToPNG()); // テクスチャを保存
                            savedTex = tex;
                            savedTexName = texName;
                        }
                    }
                }
            }
        }
        else
        {
            // フォルダがなければ新しく作成
            Directory.CreateDirectory(imagesDir);
        }
        toggleButton.isOn = isSelected;
        OnToggleValueChanged(isSelected);
    }

    public void OnToggleValueChanged(bool isOn)
    {
        Debug.Log($"Toggle is now {(isOn ? "ON" : "OFF")}");
        isSelected = isOn;
        ES3.Save<bool>("IsSelected", isSelected);
        previewImage.rectTransform.sizeDelta = defaultPreviewSize; // サイズをリセット
        //buttonImage.rectTransform.sizeDelta = defaultButtonSize; // サイズをリセット
        if (isSelected && savedTex != null)
        {
            SetRawImageSize(previewImage, savedTex);
            //SetRawImageSize(buttonImage, savedTex);
            //buttonImage.texture = savedTex;
            previewImage.texture = savedTex;
            previewText.text = savedTexName;
        }
        else
        {
            //buttonImage.texture = CharacterTextureByName[savedCharacterName];
            previewImage.texture = CharacterTextureByName[savedCharacterName];
            previewText.text = savedCharacterName;
            isSelected = false;
        }
    }

    public void OnCharacterChanged(string characterName)
    {
        if (CharacterTextureByName.ContainsKey(characterName))
        {
            if (!isSelected)
            {
                //buttonImage.texture = CharacterTextureByName[characterName];
                previewImage.texture = CharacterTextureByName[characterName];
            }
            previewText.text = characterName;
            savedCharacterName = characterName;
            savedTexName = characterName;
        }
        else
        {
            Debug.LogWarning($"Character texture for {characterName} not found.");
        }
    }

    public void OnImageChanged(string imageName, Texture2D texture)
    {
        var bynary = texture.EncodeToPNG();
        Debug.Log($"Image changed to {imageName}, size: {bynary.Length} bytes");
        ES3.Save<byte[]>("ImageByte", bynary); // テクスチャを保存
        ES3.Save<string>("ImageName", imageName);
        savedTex = texture;
        savedTexName = imageName;
        toggleButton.isOn = true;
        previewImage.rectTransform.sizeDelta = defaultPreviewSize; // サイズをリセット
        SetRawImageSize(previewImage, texture);
        previewImage.texture = texture;
        previewText.text = imageName;
        ///buttonImage.rectTransform.sizeDelta = defaultButtonSize; // サイズをリセット
        //SetRawImageSize(buttonImage, texture);
        //buttonImage.texture = texture;
    }

    public void OnVisible(bool isVisible)
    {
        ExitManager exitManger = ExitManager.Instance;
        if (isVisible)
        {
            animator.Play(panelFadeIn);
            exitManger.SetReturnButtonVisible(true);
            exitManger.returnButton.onClick.RemoveAllListeners();
            exitManger.returnButton.onClick.AddListener(() => OnVisible(false));
        }
        else
        {
            animator.Play(panelFadeOut);
            exitManger.SetReturnButtonVisible(false);
            exitManger.returnButton.onClick.RemoveAllListeners();
        }
    }

    // テクスチャをリサイズする関数
    private Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        // 新しいテクスチャを作成
        Texture2D resizedTexture = new Texture2D(width, height, source.format, false);

        // 新しいテクスチャに元のテクスチャのピクセルを再サンプリングして設定
        Color[] pixels = source.GetPixels(0, 0, source.width, source.height); // 元のピクセルを取得
        Color[] resizedPixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 元のテクスチャのピクセルを新しいサイズに合わせて補間
                float u = x / (float)width;
                float v = y / (float)height;
                resizedPixels[y * width + x] = source.GetPixelBilinear(u, v); // バイリニア補間
            }
        }

        // ピクセルデータを新しいテクスチャに設定
        resizedTexture.SetPixels(resizedPixels);
        resizedTexture.Apply(); // 変更を適用

        return resizedTexture;
    }

     // 横幅に合わせてテクスチャをリサイズする関数
    private Texture2D ResizeTextureToWidth(Texture2D source, int width)
    {
        // 縦横比を維持するために高さを計算
        float aspectRatio = (float)source.width / source.height;
        int newHeight = Mathf.RoundToInt(width / aspectRatio);

        // リサイズ処理
        Texture2D resizedTexture = ResizeTexture(source, width, newHeight);

        return resizedTexture;
    }

    // 縦横比を維持してRawImageのサイズを設定
    private void SetRawImageSize(RawImage rawImage, Texture2D texture)
    {
        RectTransform rt = rawImage.GetComponent<RectTransform>();

        float parentWidth = rt.rect.width;
        float parentHeight = rt.rect.height;
        float textureAspectRatio = (float)texture.width / texture.height;

        if (parentWidth / parentHeight > textureAspectRatio)
        {
            rt.sizeDelta = new Vector2(parentHeight * textureAspectRatio, parentHeight);
        }
        else
        {
            rt.sizeDelta = new Vector2(parentWidth, parentWidth / textureAspectRatio);
        }
    }
}
