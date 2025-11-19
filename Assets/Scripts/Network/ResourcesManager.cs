using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class ResourcesManager : MonoBehaviour
{
    public static ResourcesManager Instance { get; private set; }

    [Header("GameParams")]
    public int MaxBoxCount = 2;
    public int MaxBoxCountSingle = 2;
    public Dictionary<string, Texture2D> CharacterTextureByName { get; private set; } = new();

    [Header("PlayerParams")]
    public byte[] customImageBytes;
    public GameManager.EMode CurrentMode;
    public string userName;
    public string characterName;
    public bool IsSelected;
    public bool isWhite = true;
    public bool IsSpectator = false;

    [Header("StageObjects")]
    public Transform[] defencePoses;
    public GameObject Arrow;
    public GameObject Pitcher;
    public GameObject BarrierFreeGaze;
    public Transform DerbyTraceObj;
    public GameObject ExpandLand;
    public MeshRenderer stageRenderer;

    [Header("Prefabs")]
    public PlayerSessionItemUI playerSessionItemUI;
    public NetBaseballBat netBaseballBatPrefab;
    public NetBall netBallPrefab;
    public NetworkObject gravityFieldPrefab;

    [Header("StaticParams")]
    public static readonly string PANEL_IN = "Panel In";
    public static readonly string PANEL_OUT = "Panel Out";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        CurrentMode = ES3.Load<GameManager.EMode>(SaveKeys.GameMode, defaultValue: GameManager.EMode.BarrierFree);
        userName = ES3.Load<string>(SaveKeys.UserName, defaultValue: "Player");
        characterName = ES3.Load<string>(SaveKeys.CharacterName, defaultValue: "Architect_Female_V1_00");
        IsSelected = ES3.Load<bool>(SaveKeys.IsSelected, defaultValue: false);
        customImageBytes = ES3.Load<byte[]>(SaveKeys.ImageByte, defaultValue: null);
        var characterTextures = Resources.LoadAll("", typeof(Texture2D));
        foreach (var texture in characterTextures)
        {
            if (!CharacterTextureByName.ContainsKey(texture.name))
                CharacterTextureByName.Add(texture.name, texture as Texture2D);
        }

        switch (CurrentMode)
        {
            case GameManager.EMode.Online_BarrierFree:
                BarrierFreeGaze.SetActive(true);
                break;
            case GameManager.EMode.BarrierFree:
                BarrierFreeGaze.SetActive(true);
                break;
            case GameManager.EMode.Normal:
                Pitcher.SetActive(true);
                break;
            case GameManager.EMode.Evaluation:
                Arrow.SetActive(true);
                break;
            case GameManager.EMode.Derby:
                Pitcher.SetActive(true);
                ExpandLand.SetActive(true);
                break;
        }

        // foreach(var playerController in playerControllerPrefabs)
        // {
        // 	if (!PlayerControllerByName.ContainsKey(playerController.name))
        // 		PlayerControllerByName.Add(playerController.name, playerController);
        // }
    }
}
