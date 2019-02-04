using UnityEngine;

public class Define
{
    public enum PuzzleType
    {
        I4 = 0,
        L4 = 1,
        J4 = 2,
        O4 = 3,
        S4 = 4,
        Z4 = 5,
        T4 = 6,
        I3 = 7,
        L3 = 8,
        J3 = 9,
        I2 = 10,
        Point = 11,
    }

    public enum ItemType
    {
        None = 0,
        Destroy,
        AddTime,
        Coin
    }

    public enum ScoreType
    {
        Basic = 0,
        Double,
        Triple,
        Quad,
        Amazing
    }

    public enum DialogType
    {
        //Lobby
        LobbyOptionDialog = 0,
        InvenDialog = 1,
        GameReadyDialog = 2,
        RewardDialog = 5,
        ShopDialog = 6,
        //Game
        PauseDialog = 3,
        ResultDialog = 4,
        ContinueDialog = 7,
        //None
        None = -1,
    }

    public enum PopupType
    {
        Quit = 0,
        UseCoin = 1,
        NeedCoin = 2,
        Basic = 3,
        Ready = 4,
        Go = 5,
        TimeUp = 6,
        SkillInfo = 7,
        AddCoin = 9,
        UseSkill = 10,
        None
    }

    public enum MissionType
    {
        GetScore = 0,
        UseSkill = 1,
        ClearLines = 2,
        Combo = 3,
        ArrangePuzzleCount = 4,
    }

    public enum RewardType
    {
        PlayCount = 0,
        UseCoin = 1,
        UseSkill = 2,
        HasPets = 3,
        LevelUpPets = 4,
        BestScore = 5,
    }

    public enum SoundType
    {
        LobbyBGM = 0,
        GameBGM = 1,
        TickTock = 2,
        TickTockBack = 3,
        Click = 4,
        ShowDialog = 5,
        HideDialog = 6,
        PutTileOnBoard = 7,
        TileMoveBack = 8,
        CrushLine = 9,
        ShowPopup = 10,
        HidePopup = 11,
        UseSkill = 12,
        Ending = 13,
        ReadyVoice = 14,
        GoVoice = 15,
        TimeUpVoice = 16,
        GoFunny = 17,
        GameOver = 18,
        Tick = 19,
    }

    public enum SkillType
    {
        //active. //
        StopTime = 0,
        ChangePuzzles = 1,
        BrokenTiles = 2,
        DoubleScore = 3,
        FillLines = 4,
        ClearAllTiles = 5,
        //passive. //
        AddTime = 10,
        KeepCombo = 11,
    }

    public enum TutorialType
    {
        GameGuide = 0,
        SkillGuide = 1,
        AdoptGuide = 2,
        RewardGuide = 3,
        None = 4,
    }

    public enum TutorialEmphasis
    {
        NoEmphasis = 0,
        JustEmphasis,
        EmphasisTarget,
    }

    public enum Pivot
    {
        Center = 0,
        Left = 1,
        Bottom = 2,
        LeftBottom = 3,
    }

    public enum GameDesign
    {
        MaxCoinCount = 0,           //획득할 수 있는 코인의 최대 수. //
        StartCoin = 1,              //초기 시작 시 주어지는 코인 수. //
        BasicGameTime = 2,          //기본 게임 시간. //
        DoubleSkillScoreBonus = 3,  //스킬로 인한 점수 상승 폭. //
        DefaultScore = 4,           //기본 점수. //
        FreeCoin = 5,               //무료 코인. //
        FreeCoinTime = 6,           //무료 코인 광고 제한 시간. //
        AndroidAdsID = 7,           //안드로이드 Unity Ads ID. //
        IOSAdsID = 8,               //IOS Unity Ads ID. //
        ContinueTime = 9,           //이어하기 광고 제한 시간. //
        ContinueCheckTime = 10,     //이어하기 선택 시간. //
        ContinueAddTime = 11,       //이어하기 추가 시간. //
    }

    public static readonly int nullValue = -1;

    public const int width = 8;
	public const int height = 8;
    public const float blockDistance = 120f;
    public static readonly float invBlockDistance = 1f / blockDistance;
    public static readonly Vector3 blockCenterPos = new Vector3(0f, 282f, 0f);
    public const int selectedPetsCount = 3;
    public const int readyBlockCount = 3;
    public const int missionLevelCount = 3;
    public const float readyBlockSpeed = 3000f;
	public const float smallSizeRate = 0.5f;
	public static readonly string titleData = "stage,startWidth,startHeight,searchCount,mapData";
    public static readonly string addTimeIconName = "skill_total_time";

    public const float minColliderSize = 400f;
	public const float clearTileTime = 0.05f;
	public const float readyTileMoveTime = 0.15f;
	public const float readyHeight = -800f;
	public const float readyWidth = 300f;
    public const float petWidthDistance = 300f;
    public const float animalLineHeight = -664f;
	public static readonly Vector3 readyPosition = new Vector3(700f, readyHeight, 0f);
	public static readonly Vector3 fingerDistance = new Vector3(0f, 300f, 0f);

	public const float maxGamePlayTime = 180f;
    public const float warningTime = 10f;
    
    public const int warningBonus = 2;
    public const int defaultScore = 100;
    public const int maxCharacterLevel = 9; //캐릭터 최대 레벨. //
    
    public static readonly Color backColor = Util.GetColor(168, 181, 229);

    //Logo Manager. //
    public const float keepLogoTime = 0.5f; //로고 유지 시간. //
    
    // Sprite's Names. //
    public const string playmate_red = "playmate_red";
    public const string playmate_blue = "playmate_blue";
    public const string playmate_green = "playmate_green";

    public const string profile_outline = "profile_outline";
    public const string profile_outline_red = "profile_red_outline";
    public const string profile_outline_blue = "profile_blue_outline";
    public const string profile_outline_green = "profile_green_outline";
    public const string profile_outline_unselect = "profile_unselect_outline";

    public const string profile_back = "profile_back";
    public const string profile_back_red = "profile_red_back";
    public const string profile_back_blue = "profile_blue_back";
    public const string profile_back_green = "profile_green_back";
    public const string profile_back_unselect = "profile_unselect_back";

    public const string select_change_button = "select_change_button";
    public const string unselect_change_button = "unselect_change_button";

    public static readonly string[] skillLevelBack = new string[2] { "skill_lv_active", "skill_lv_passive" };

    public static readonly string freeCoinId = "rewardedVideo";
    public static readonly string continueId = "video";
}