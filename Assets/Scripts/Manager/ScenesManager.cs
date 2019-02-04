using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : Singleton<ScenesManager>
{
	//현재 불러와진 Scene으로 해당 씬의 Manager는 자신을 SceneManager에 등록해야 한다. //
	private static SceneObject nowScene = null;
	private static System.Action escapeMethod = null;

    protected override void Awake()
    {
        if (instance == null)
        {
            instance = this;
            instance.Init();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    protected override void Init()
    {
        base.Init();
    }

    protected override void OnEnable()
    {
    }

    public static void SetEscapeMethod(System.Action callback)
	{
		escapeMethod = callback;
	}
	
	public static void AddScene(SceneObject scene)
	{
		nowScene = scene;
	}
	
	public static void ChangeScene(string sceneName)
	{
		if(nowScene == null)
		{
			Debug.Log("Somthing is wrong! there is no added scene.");
			return;
		}
		nowScene.ClearManager();
		nowScene = null;
        SceneManager.LoadScene(sceneName);
	}
	
	private void Update()
	{
//#if !UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (escapeMethod != null)
            {
                escapeMethod();
            }
        }
//#endif
    }

    public static void ExitGame()
    {
        PopupSystem.GetPopup<QuitPopup>(Define.PopupType.Quit).SetCallback(() =>
        {
            Debug.Log("Exit Game");
            Application.Quit();
        }, null);
        PopupSystem.OpenPopup(Define.PopupType.Quit);
        SoundManager.GetInstance().PlaySound(Define.SoundType.Click);
    }
}
