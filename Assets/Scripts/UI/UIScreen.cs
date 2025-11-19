using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// UI画面の管理を行うクラス
/// 画面の表示/非表示、フォーカス管理、履歴の管理を担当
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UIScreen : MonoBehaviour
{
	[Tooltip("モーダル画面かどうか（他の画面の操作を無効にするか）")]
	public bool isModal = false;
	[Tooltip("画面がフォーカスされたときに初期選択されるUI要素")]
	public Selectable initialSelected;
	[Tooltip("画面がフォーカスされたときに実行されるイベント")]
	public UnityEvent onFocused;
	[Tooltip("画面がフォーカスを失ったときに実行されるイベント")]
	public UnityEvent onDefocused;
	[Tooltip("前に表示されていた画面への参照")]
	[ReadOnly] public UIScreen previousScreen = null;

	// CanvasGroupのキャッシュ
	CanvasGroup _group = null;
	/// <summary>
	/// CanvasGroupへの参照を取得（キャッシュあり）
	/// </summary>
	public CanvasGroup Group
	{
		get
		{
			if (_group) return _group;
			return _group = GetComponent<CanvasGroup>();
		}
	}

	// 現在アクティブな画面への静的参照
	public static UIScreen activeScreen;

	// クラスメソッド（静的メソッド）

	/// <summary>
	/// 現在アクティブな画面を非表示にする
	/// </summary>
	public static void HideActive()
	{
		activeScreen.gameObject.SetActive(false);
	}

	/// <summary>
	/// 現在アクティブな画面を表示する
	/// </summary>
	public static void ShowActive()
	{
		activeScreen.gameObject.SetActive(true);
	}

	/// <summary>
	/// 指定した画面にフォーカスを移す
	/// </summary>
	/// <param name="screen">フォーカスする画面</param>
    public static void Focus(UIScreen screen) {
        if ( activeScreen )
            activeScreen.FocusScreen(screen);
        else
            screen.Focus();
    }

	/// <summary>
	/// 最初の画面まで戻る
	/// </summary>
    public static void BackToInitial()
	{
		while (activeScreen?.previousScreen)
		{
			activeScreen.Defocus();
			UIScreen temp = activeScreen;
			activeScreen = activeScreen.previousScreen;
			temp.previousScreen = null;
		}
		if (activeScreen) activeScreen.Focus();
	}

	/// <summary>
	/// 全ての画面を閉じる
	/// </summary>
	public static void CloseAll()
	{
		while (activeScreen)
		{
			activeScreen.BackOrClose();
		}
	}


	// インスタンスメソッド

	/// <summary>
	/// Unity Awakeメソッド
	/// アクティブな画面がない場合、この画面をフォーカスする
	/// </summary>
	private void Awake()
	{
		if (activeScreen == null) Focus();
	}

	/// <summary>
	/// Unity OnDestroyメソッド
	/// アクティブな画面がこの画面の場合、適切に後処理を行う
	/// </summary>
	private void OnDestroy()
	{
		
		if (activeScreen == this)
		{
#if UNITY_EDITOR
			if (EditorApplication.isPlayingOrWillChangePlaymode == false) return;
#endif
			if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isLoaded == false)
			{
				activeScreen = null;
				return;
			}
			Debug.LogWarning($"Active UIScreen {this} is being destroyed");
			Back();
		}
	}

	/// <summary>
	/// 他の画面にフォーカスを移す
	/// </summary>
	/// <param name="screen">フォーカスする画面</param>
	public void FocusScreen(UIScreen screen)
	{
		// 自分自身の場合はフォーカスのみ
		if (screen == this)
		{
			Focus();
			return;
		}
        
		// 画面履歴を設定してフォーカス
		screen.previousScreen = this;
		screen.Focus();
		if (!screen.isModal)
		{
			// モーダルでない場合は現在の画面を非フォーカス
			Defocus();
		}
		else
		{
			// モーダルの場合は操作を無効にするのみ
			Group.interactable = false;
		}
	}

	/// <summary>
	/// この画面をフォーカスする
	/// </summary>
	public void Focus()
	{
		Group.interactable = true;
		if(TryGetComponent<Animator>(out Animator animator))
		{
			animator.Play(ResourcesManager.PANEL_IN);
		}
		else
		{
			gameObject.SetActive(true);
		}
		activeScreen = this;
		
		// 初期選択要素があれば選択
		if (initialSelected)
		{
			initialSelected.Select();
		}
		
		// フォーカスイベントを実行
		if (onFocused != null) onFocused.Invoke();
	}

	/// <summary>
	/// この画面のフォーカスを外す
	/// </summary>
	public void Defocus()
	{
		if(TryGetComponent<Animator>(out Animator animator))
		{
			animator.Play(ResourcesManager.PANEL_OUT);
		}
		else
		{
			gameObject.SetActive(false);
		}
		if (onDefocused != null) onDefocused.Invoke();
	}

    /// <summary>
    /// 前の画面を有効にせずに戻る処理（Backと同等）
    /// </summary>
    public void Close()
    {
        Defocus();
        if (previousScreen)
        {
            Defocus();
            activeScreen = previousScreen;
            previousScreen = null;
        }
		else
		{
			activeScreen = null;
		}
    }

	/// <summary>
	/// 前の画面に戻る
	/// </summary>
    public void Back()
	{
		if (previousScreen)
		{
			//Do some checks for if in-game?
				Defocus();
				previousScreen.Focus();
				previousScreen = null;
		}
	}

	/// <summary>
	/// 前の画面に戻るか、なければ閉じる
	/// </summary>
	public void BackOrClose()
	{
		Defocus();
		if (previousScreen)
		{
			previousScreen.Focus();
			previousScreen = null;
		}
		else
		{
			activeScreen = null;
		}
	}

	/// <summary>
	/// 指定した画面まで戻る
	/// </summary>
	/// <param name="screen">戻り先の画面</param>
	public void BackTo(UIScreen screen)
	{
		while (activeScreen != screen && activeScreen?.previousScreen)
		{
			activeScreen.Back();
		}

		if (activeScreen != screen)
		{
			Focus(screen);
		}
	}
}

#if UNITY_EDITOR
/// <summary>
/// UIScreenのカスタムエディター
/// Unity Editorでの表示をカスタマイズ
/// </summary>
[CustomEditor(typeof(UIScreen))]
public class UIScreenEditor : Editor
{
    private Button[] buttons;
	SerializedProperty _initialSelected;
	SerializedProperty _onFocused;
	SerializedProperty _onDefocused;

	/// <summary>
	/// エディターが有効になったときの処理
	/// </summary>
	private void OnEnable()
	{
		buttons = ((MonoBehaviour)target).GetComponentsInChildren<Button>();
		_initialSelected = serializedObject.FindProperty("initialSelected");
		_onFocused = serializedObject.FindProperty("onFocused");
		_onDefocused = serializedObject.FindProperty("onDefocused");
	}

	/// <summary>
	/// エディターが無効になったときの処理
	/// </summary>
	private void OnDisable()
	{
		buttons = null;
	}

	/// <summary>
	/// インスペクターのGUI描画
	/// </summary>
	public override void OnInspectorGUI()
	{
		//DrawDefaultInspector();
		UIScreen scr = (UIScreen)target;
		EditorGUILayout.BeginHorizontal();
		scr.isModal = EditorGUILayout.ToggleLeft("Is Modal", scr.isModal, GUILayout.ExpandWidth(false));
		GUILayout.FlexibleSpace();
		GUILayout.Label(scr.gameObject.name, EditorStyles.boldLabel);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.ObjectField(_initialSelected);
		EditorGUILayout.PropertyField(_onFocused);
		EditorGUILayout.PropertyField(_onDefocused);
		serializedObject.ApplyModifiedProperties();
		
		// 子オブジェクトのボタンを表示し、選択可能にする
		if (buttons.Length > 0)
		{
			foreach (Button btn in buttons)
			{
				if (GUILayout.Button(btn.gameObject.name))
				{
					Selection.activeObject = btn;
				}
			}
		}
	}
}
#endif