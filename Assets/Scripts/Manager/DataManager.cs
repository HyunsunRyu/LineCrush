using UnityEngine;
using System.Text;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

public class DataManager : Singleton<DataManager>
{
    [SerializeField] private TextAsset animalTabelTextAsset;
    [SerializeField] private TextAsset skillTextAsset;
    [SerializeField] private TextAsset puzzleTextAsset;
    [SerializeField] private TextAsset tipTextAsset;
    [SerializeField] private TextAsset textTableTextAsset;
    [SerializeField] private TextAsset gameDesignTextAsset;
    [SerializeField] private TextAsset levelTextAsset;
    [SerializeField] private TextAsset missionTextAsset;
    [SerializeField] private TextAsset rewardTextAsset;
    [SerializeField] private TextAsset animationTextAsset;
    [SerializeField] private TextAsset tutorialTextAsset;
    [SerializeField] private TextAsset productTextAsset;

    //read from text assets. //
    private List<AnimalTable> animalsList = new List<AnimalTable>();
    private List<AnimationTable> animList = new List<AnimationTable>();
    private List<SkillTable> skillList = new List<SkillTable>();
    private List<PuzzleTable> puzzleTableList = new List<PuzzleTable>();
    private List<TipTable> tipList = new List<TipTable>();
    private Dictionary<int, TextTable> textDic = new Dictionary<int, TextTable>();
    private Dictionary<int, GameDesignTable> gameDesignDic = new Dictionary<int, GameDesignTable>();
    private List<ProductTable> productList = new List<ProductTable>();
    private Dictionary<int, List<TutorialTable>> tutorialDic = new Dictionary<int, List<TutorialTable>>();
    private Dictionary<int, List<LevelTable>> levelDic = new Dictionary<int, List<LevelTable>>();
    private Dictionary<int, List<MissionTable>> missionDic = new Dictionary<int, List<MissionTable>>();
    private Dictionary<int, List<RewardTable>> rewardDic = new Dictionary<int, List<RewardTable>>();

    //saved data key. //
    private readonly ObscuredString hasPetListKey = "HPLK";
    private readonly ObscuredString selectedAnimalsKey = "SADK";
    private readonly ObscuredString optionDataKey = "ODK";
    private readonly ObscuredString gameDataKey = "CDK";
    private readonly ObscuredString missionKey = "MK";
    private readonly ObscuredString rewardKey = "RK";
    private readonly ObscuredString tutorialKey = "TK";

    private StringBuilder strBuilder = new StringBuilder();
    private List<PetData> petList = new List<PetData>();
    private List<PetData> invenPetsList = new List<PetData>();
    private List<PetData> adoptPetsList = new List<PetData>();
    private List<PetData> playmatePetList = new List<PetData>();

    //계산을 위한 임시 데이터. 매번 생성하지 않기 위해 선언해둠. //
    private List<int> listForCalculator = new List<int>();

    //save and load data. //
    public ObscuredInt coin { get; private set; }
    public ObscuredDouble bestScore { get; private set; }
    public ObscuredInt useSkillCount { get; private set; }
    public ObscuredInt useCoin { get; private set; }
    public ObscuredInt playCount { get; private set; }
    public ObscuredInt videoCount { get; private set; }
    private System.DateTime lastShowFreeCoinVideo;
    private System.DateTime lastShowContinueVideo;

    private List<PetData> hasPetsList = new List<PetData>();
    private PetData[] selectedPetArray = new PetData[Define.selectedPetsCount];
    private Dictionary<int, Mission> nowMissions = new Dictionary<int, Mission>();
    private Dictionary<Define.SkillType, int> useSkillData = new Dictionary<Define.SkillType, int>();

    //UI표시를 위한 현재 선택한 동물 데이터. //
    public PetData nowSelectedPetData { get; private set; }

    private System.Action coinUiCallback;
    private System.Action adoptPetCallback;
    private DataController dataController;

    private SystemLanguage language;

    public ObscuredInt needCoinCount { get; private set; }

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
        DontDestroyOnLoad(gameObject);

        language = Application.systemLanguage;

        TextTable.SetLanguage(language);
        dataController = new DataController();
        ParsingTable();
    }

    protected override void OnEnable()
    {
        LoadAllSavedData();
    }

    private void ParsingTable()
    {
        dataController.ParsingTableList(animalTabelTextAsset, ref animalsList);
        dataController.ParsingTableList(animationTextAsset, ref animList);
        dataController.ParsingTableList(skillTextAsset, ref skillList);
        dataController.ParsingTableList(puzzleTextAsset, ref puzzleTableList);
        dataController.ParsingTableList(tipTextAsset, ref tipList);
        dataController.ParsingTableList(productTextAsset, ref productList);
        dataController.ParsingTableDic(textTableTextAsset, ref textDic);
        dataController.ParsingTableDic(gameDesignTextAsset, ref gameDesignDic);
        dataController.ParsingTableMixed(tutorialTextAsset, ref tutorialDic);
        dataController.ParsingTableMixed(levelTextAsset, ref levelDic);
        dataController.ParsingTableMixed(missionTextAsset, ref missionDic);
        dataController.ParsingTableMixed(rewardTextAsset, ref rewardDic);
        
        //set pet list. //
        SetPetData();
    }

    private void SetPetData()
    {
        petList.Clear();
        for (int i = 0, max = animalsList.Count; i < max; i++)
        {
            AnimalTable animal = animalsList[i];
            AnimationTable anim = GetAnimation(animal.unqIdx);

            if (animal != null && anim != null)
            {
                PetData pet = new PetData(animal, anim);
                petList.Add(pet);
            }
        }
    }
    
    private void LoadAllSavedData()
    {
        SetHasAnimalData(dataController.LoadData(hasPetListKey));
        SetSelectedAnimalData(dataController.LoadData(selectedAnimalsKey));
        SetOptionData(dataController.LoadData(optionDataKey));
        SetGameData(dataController.LoadData(gameDataKey));
        SetMissionData(dataController.LoadData(missionKey));
        SetRewardData(dataController.LoadData(rewardKey));
        SetTutorialData(dataController.LoadData(tutorialKey));

        if (GetHasPetListCount() > 0)
        {
            UnityEngine.Analytics.Analytics.CustomEvent("game start", new Dictionary<string, object>
            {
                { "coin", coin },
                { "best score", bestScore },
                { "use skill count", useSkillCount },
                { "use coin", useCoin },
                { "play count", playCount },
                { "video count", videoCount },
            });
        }
    }

    public void SaveAllData()
    {
        dataController.SaveData(hasPetListKey, GetConvertedHasAnimalData());
        dataController.SaveData(selectedAnimalsKey, GetConvertedSelectedAnimalData());
        dataController.SaveData(optionDataKey, GetConvertedOptionData());
        dataController.SaveData(gameDataKey, GetConvertedGameData());
        dataController.SaveData(missionKey, GetConvertedMissionData());
        dataController.SaveData(rewardKey, GetConvertedRewardData());
        dataController.SaveData(tutorialKey, GetConvertedTutorialData());
    }

    public void ResetData()
    {
        dataController.ResetData();
    }

    public void ClearUseSkillData()
    {
        useSkillData.Clear();
    }

    public void SetUseSkillData(Define.SkillType skillType)
    {
        if (!useSkillData.ContainsKey(skillType))
            useSkillData.Add(skillType, 0);
        useSkillData[skillType]++;
    }

    public string GetSkillData()
    {
        StringBuilder str = new StringBuilder();
        foreach (KeyValuePair<Define.SkillType, int> data in useSkillData)
        {
            str.Append(data.Key.ToString());
            str.Append(" : ");
            str.Append(data.Value.ToString());
            str.Append(", ");
        }
        return str.ToString();
    }

    private void SetHasAnimalData(string savedData)
    {
        hasPetsList.Clear();
        if (string.IsNullOrEmpty(savedData))
        {
            Debug.Log("No Saved Data : HasAnimalData");
            return;
        }

        string[] strElement = dataController.GetStringElements(savedData);

        int count = 0;
        int maxCount = strElement.Length;
        PetData data;

        while (maxCount > count)
        {
            int unqIdx = TryToParseInt(strElement, count++);
            int level = TryToParseInt(strElement, count++);
            int exp = TryToParseInt(strElement, count++);
            int pSkillLv = TryToParseInt(strElement, count++);
            int aSkillLv = TryToParseInt(strElement, count++);

            data = GetPetDataWithUnqIdx(unqIdx);
            data.SetData(level, exp, pSkillLv, aSkillLv);
            TryAdoptAnimal(data);
        }
    }

    private void SetSelectedAnimalData(string savedData)
    {
        for (int i = 0; i < Define.selectedPetsCount; i++)
            selectedPetArray[i] = null;

        if (string.IsNullOrEmpty(savedData))
        {
            Debug.Log("No Saved Data : SelectedAnimalData");
            return;
        }

        string[] strElement = dataController.GetStringElements(savedData);

        int count = 0;
        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            int unqIdx = TryToParseInt(strElement, count++);
            selectedPetArray[i] = unqIdx != Define.nullValue ? GetHasPetDataWithUnqIdx(unqIdx) : null;
        }
    }

    private void SetOptionData(string savedData)
    {
        if (string.IsNullOrEmpty(savedData))
        {
            Debug.Log("No Saved Data : OptionData");

            SoundManager.GetInstance().SetSoundVolume(1f);
            SoundManager.GetInstance().SetBGMVolume(1f);
        }
        else
        {
            int count = 0;
            string[] strElement = dataController.GetStringElements(savedData);

            SoundManager.GetInstance().SetSoundVolume(TryToParseFloat(strElement, count++, 1f));
            SoundManager.GetInstance().SetBGMVolume(TryToParseFloat(strElement, count++, 1f));
        }
    }

    private void SetGameData(string savedData)
    {
        if (string.IsNullOrEmpty(savedData))
        {
            Debug.Log("No Saved Data : CoinData");

            coin = GetDesignValue(Define.GameDesign.StartCoin);
        }
        else
        {
            int count = 0;
            string[] strElement = dataController.GetStringElements(savedData);

            coin = TryToParseInt(strElement, count++, 0);
            bestScore = TryToParseDouble(strElement, count++, 0);
            useSkillCount = TryToParseInt(strElement, count++, 0);
            useCoin = TryToParseInt(strElement, count++, 0);
            playCount = TryToParseInt(strElement, count++, 0);
            videoCount = TryToParseInt(strElement, count++, 0);
            string videoTime = TryToParseStr(strElement, count++, "");
            if (!System.DateTime.TryParse(videoTime, out lastShowFreeCoinVideo))
                lastShowFreeCoinVideo = System.DateTime.Now.AddSeconds(-GetDesignValue(Define.GameDesign.FreeCoinTime));

            videoTime = TryToParseStr(strElement, count++, "");
            if (!System.DateTime.TryParse(videoTime, out lastShowContinueVideo))
                lastShowContinueVideo = System.DateTime.Now.AddSeconds(-GetDesignValue(Define.GameDesign.FreeCoinTime));
        }
    }

    private void SetMissionData(string savedData)
    {
        if (string.IsNullOrEmpty(savedData))
        {
            Debug.Log("No Saved Data : MissionData");
        }
        else
        {
            int count = 0;
            string[] strElement = dataController.GetStringElements(savedData);

            int size = strElement.Length;
            while (size > count)
            {
                int level = TryToParseInt(strElement, count++, 0);
                Define.MissionType type = (Define.MissionType)TryToParseInt(strElement, count++, 0);

                MissionManager.GetInstance().SetMission(level, type);
            }
        }
    }

    private void SetRewardData(string savedData)
    {
        if (string.IsNullOrEmpty(savedData))
        {
            Debug.Log("No Saved Data : RewardData");
        }
        else
        {
            int count = 0;
            string[] strElement = dataController.GetStringElements(savedData);

            int size = strElement.Length;
            while (size > count)
            {
                Define.RewardType type = (Define.RewardType)TryToParseInt(strElement, count++, 0);
                int level = TryToParseInt(strElement, count++, 0);
                int value = TryToParseInt(strElement, count++, 0);
                bool isComplete = TryToParseBool(strElement, count++, false);

                RewardManager.GetInstance().SetSavedRewardData(type, level, value, isComplete);
            }
        }
    }

    private void SetTutorialData(string savedData)
    {
        if (string.IsNullOrEmpty(savedData))
        {
            Debug.Log("No Saved Data : TutorialData");
        }
        else
        {
            int count = 0;
            string[] strElement = dataController.GetStringElements(savedData);

            int size = strElement.Length;
            while (size > count)
            {
                Define.TutorialType type = (Define.TutorialType)TryToParseInt(strElement, count++, 0);
                bool bClear = TryToParseBool(strElement, count++, false);

                TutorialManager.GetInstance().SetTutorialClearData(type, bClear);
            }
        }
    }

    private string GetConvertedHasAnimalData()
    {
        strBuilder.Length = 0;
        for (int i = 0, max = hasPetsList.Count; i < max; i++)
        {
            strBuilder.Append(hasPetsList[i].unqIdx);
            strBuilder.Append(dataController.delimiter);
            strBuilder.Append(hasPetsList[i].level);
            strBuilder.Append(dataController.delimiter);
            strBuilder.Append(hasPetsList[i].exp);
            strBuilder.Append(dataController.delimiter);
            strBuilder.Append(hasPetsList[i].pSkillLv);
            strBuilder.Append(dataController.delimiter);
            strBuilder.Append(hasPetsList[i].aSkillLv);

            if (i != max - 1)
                strBuilder.Append(dataController.delimiter);
        }
        return strBuilder.ToString();
    }

    private string GetConvertedSelectedAnimalData()
    {
        strBuilder.Length = 0;

        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            if (selectedPetArray[i] != null)
                strBuilder.Append(selectedPetArray[i].unqIdx);
            else
                strBuilder.Append(Define.nullValue);

            if (i < Define.selectedPetsCount - 1)
                strBuilder.Append(dataController.delimiter);
        }
        return strBuilder.ToString();
    }

    private string GetConvertedOptionData()
    {
        strBuilder.Length = 0;

        strBuilder.Append(SoundManager.GetInstance().soundVolume);
        strBuilder.Append(dataController.delimiter);
        strBuilder.Append(SoundManager.GetInstance().bgmVolume);

        return strBuilder.ToString();
    }

    private string GetConvertedGameData()
    {
        strBuilder.Length = 0;

        strBuilder.Append(coin);
        strBuilder.Append(dataController.delimiter);
        strBuilder.Append(bestScore);
        strBuilder.Append(dataController.delimiter);
        strBuilder.Append(useSkillCount);
        strBuilder.Append(dataController.delimiter);
        strBuilder.Append(useCoin);
        strBuilder.Append(dataController.delimiter);
        strBuilder.Append(playCount);
        strBuilder.Append(dataController.delimiter);
        strBuilder.Append(videoCount);
        strBuilder.Append(dataController.delimiter);
        strBuilder.Append(lastShowFreeCoinVideo.ToString());
        strBuilder.Append(dataController.delimiter);
        strBuilder.Append(lastShowContinueVideo.ToString());

        return strBuilder.ToString();
    }

    private string GetConvertedMissionData()
    {
        strBuilder.Length = 0;

        for (int i = 0; i < 3; i++)
        {
            Mission data = MissionManager.GetInstance().GetNowMission(i);
            strBuilder.Append((int)data.missionLevel);
            strBuilder.Append(dataController.delimiter);
            strBuilder.Append((int)data.missionType);
            if (i < 2)
                strBuilder.Append(dataController.delimiter);
        }
        return strBuilder.ToString();
    }

    private string GetConvertedRewardData()
    {
        strBuilder.Length = 0;

        for (int i = 0; i < RewardManager.GetInstance().rewardCount; i++)
        {
            RewardBase data = RewardManager.GetInstance().GetNowReward((Define.RewardType)i);
            if (data != null)
            {
                strBuilder.Append((int)data.GetRewardType());
                strBuilder.Append(dataController.delimiter);
                strBuilder.Append(data.nowLevel);
                strBuilder.Append(dataController.delimiter);
                strBuilder.Append(data.nowValue);
                strBuilder.Append(dataController.delimiter);
                strBuilder.Append(ParseBoolToInt(data.isCompleted));
                if (i < RewardManager.GetInstance().rewardCount - 1)
                    strBuilder.Append(dataController.delimiter);
            }
        }
        return strBuilder.ToString();
    }

    private string GetConvertedTutorialData()
    {
        strBuilder.Length = 0;

        for (int i = 0; i < TutorialManager.GetInstance().tutorialCount; i++)
        {
            Define.TutorialType type = (Define.TutorialType)i;
            bool bClear = TutorialManager.GetInstance().IsCleared(type);
            strBuilder.Append(i);
            strBuilder.Append(dataController.delimiter);
            strBuilder.Append(ParseBoolToInt(bClear));
            if (i < TutorialManager.GetInstance().tutorialCount - 1)
                strBuilder.Append(dataController.delimiter);
        }
        return strBuilder.ToString();
    }

    public bool TryToParseBool(string[] str, int count, bool defaultValue = true)
    {
        int value;
        if (str.Length > count)
        {
            if (int.TryParse(str[count], out value))
                return value != 0;
        }

        Debug.Log("something is wrong");
        return defaultValue;
    }

    public int TryToParseInt(string[] str, int count, int defaultValue = 0)
    {
        int value;
        if (str.Length > count)
        {
            if (int.TryParse(str[count], out value))
                return value;
        }

        Debug.Log("something is wrong : " + count.ToString());
        return defaultValue;
    }

    public float TryToParseFloat(string[] str, int count, float defaultValue = 0f)
    {
        float value;
        if (str.Length > count)
        {
            if (float.TryParse(str[count], out value))
                return value;
        }

        Debug.Log("something is wrong");
        return defaultValue;
    }

    public double TryToParseDouble(string[] str, int count, double defaultValue = 0d)
    {
        double value;
        if (str.Length > count)
        {
            if (double.TryParse(str[count], out value))
                return value;
        }

        Debug.Log("something is wrong");
        return defaultValue;
    }

    public string TryToParseStr(string[] str, int count, string defaultValue = "")
    {
        if (str.Length > count)
            return str[count];

        Debug.Log("something is wrong");
        return defaultValue;
    }

    public System.DateTime TryToParseDateTime(string[] str, int count, System.DateTime defaultValue)
    {
        System.DateTime value;
        if (str.Length > count)
        {
            if (System.DateTime.TryParse(str[count], out value))
                return value;
        }

        Debug.Log("something is wrong");
        return defaultValue;
    }

    public bool TryGetProductKey(int idx, ref string productKey)
    {
        for (int i = 0, max = productList.Count; i < max; i++)
        {
            if (productList[i].idx == idx)
            {
                productKey = productList[i].productKey;
                return true;
            }
        }
        return false;
    }

    public bool TryGetProductValue(string productKey, ref int value)
    {
        for (int i = 0, max = productList.Count; i < max; i++)
        {
            if (string.Equals(productList[i].productKey, productKey))
            {
                value = productList[i].value;
                return true;
            }
        }
        return false;
    }

    public bool TryGetProductValue(int idx, ref int value)
    {
        for (int i = 0, max = productList.Count; i < max; i++)
        {
            if (productList[i].idx == idx)
            {
                value = productList[i].value;
                return true;
            }
        }
        return false;
    }

    public int ParseBoolToInt(bool value)
    {
        return value ? 1 : 0;
    }

    public static string GetText(int key)
    {
        if (GetInstance().textDic.ContainsKey(key) == true)
        {
            return GetInstance().textDic[key].text;
        }
        Debug.Log("WRONG KEY : " + key.ToString());
        return null;
    }

    public static int GetDesignValue(Define.GameDesign type)
    {
        int key = (int)type;
        if (GetInstance().gameDesignDic.ContainsKey(key))
            return GetInstance().gameDesignDic[key].value;

        Debug.Log("No Key : " + type.ToString() + " - " + key.ToString());
        return Define.nullValue;
    }

    public TutorialTable GetTutorial(Define.TutorialType type, int idx)
    {
        if (tutorialDic.ContainsKey((int)type))
        {
            if (tutorialDic[(int)type].Count > idx)
                return tutorialDic[(int)type][idx];
        }
        return null;
    }

    public int GetTileTableListCount() { return puzzleTableList.Count; }
    public int GetHasPetListCount() { return hasPetsList.Count; }
    public int GetInvenPetListCount() { return invenPetsList.Count; }
    public int GetAdoptPetListCount() { return adoptPetsList.Count; }
    public int GetPlaymatePetListCount() { return playmatePetList.Count; }
    public int GetProductListCount() { return productList.Count; }
    public int GetTotalSelectedPetsCount()
    {
        int count = 0;
        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            if (selectedPetArray[i] != null)
                count++;
        }
        return count;
    }

    public PuzzleTable GetTileTable(int idx)
    {
        if (puzzleTableList.Count <= idx)
            return null;
        return puzzleTableList[idx];
    }

    public TipTable GetRandomTipTable()
    {
        int randomIdx = Random.Range(0, tipList.Count);
        return tipList[randomIdx];
    }

    public TipTable GetTipTable(int idx)
    {
        if (tipList.Count <= idx)
            return null;
        return tipList[idx];
    }

    public PetData GetHasPetDataWithNumber(int idx)
    {
        if (hasPetsList.Count <= idx)
            return null;
        return hasPetsList[idx];
    }

    public PetData GetHasPetDataWithUnqIdx(int unqIdx)
    {
        for (int i = 0, max = hasPetsList.Count; i < max; i++)
        {
            if (hasPetsList[i].unqIdx == unqIdx)
                return hasPetsList[i];
        }
        return null;
    }

    public bool IsInHasPetDataList(int unqIdx)
    {
        for (int i = 0, max = hasPetsList.Count; i < max; i++)
        {
            if (hasPetsList[i].unqIdx == unqIdx)
                return true;
        }
        return false;
    }

    public LevelTable GetLevelTable(int lvType, int level)
    {
        if (levelDic.ContainsKey(lvType))
        {
            for (int i = 0, max = levelDic[lvType].Count; i < max; i++)
            {
                if (levelDic[lvType][i].level == level)
                    return levelDic[lvType][i];
            }
        }
        return null;
    }

    public MissionTable GetMissionTable(Define.MissionType type, int level)
    {
        if (missionDic.ContainsKey(level))
        {
            for (int i = 0, max = missionDic[level].Count; i < max; i++)
            {
                if (missionDic[level][i].missionType == type)
                    return missionDic[level][i];
            }
        }
        return null;
    }

    public RewardTable GetRewardTable(Define.RewardType type, int level)
    {
        if (rewardDic.ContainsKey((int)type))
        {
            for (int i = 0, max = rewardDic[(int)type].Count; i < max; i++)
            {
                if (rewardDic[(int)type][i].level == level)
                    return rewardDic[(int)type][i];
            }
        }
        return null;
    }

    public int GetRewardMaxLevel(Define.RewardType type)
    {
        if (rewardDic.ContainsKey((int)type))
            return rewardDic[(int)type].Count - 1;
        return 0;
    }

    public PetData GetPetDataWithUnqIdx(int unqIdx)
    {
        for (int i = 0, max = petList.Count; i < max; i++)
        {
            if (petList[i].unqIdx == unqIdx)
                return petList[i];
        }
        return null;
    }

    public SkillTable GetSkillTable(Define.SkillType skillType)
    {
        for (int i = 0, max = skillList.Count; i < max; i++)
        {
            if (skillList[i].skillType == skillType)
                return skillList[i];
        }
        return null;
    }

    public AnimationTable GetAnimation(int unqIdx)
    {
        for (int i = 0, max = animList.Count; i < max; i++)
        {
            if (animList[i].unqIdx == unqIdx)
                return animList[i];
        }
        return null;
    }

    public void ResetInvenAnimals()
    {
        for (int i = 0, max = hasPetsList.Count; i < max; i++)
        {
            if (!HasSameIndex(hasPetsList[i].unqIdx))
            {
                listForCalculator.Add(hasPetsList[i].unqIdx);
                invenPetsList.Add(hasPetsList[i]);
            }
        }
    }

    public void ResetAdoptAnimals()
    {
        adoptPetsList.Clear();

        //모든 동물 중 현재 가지고 있지 않은 동물을 순서대로 우선 나열. //
        for (int i = 0, max = petList.Count; i < max; i++)
        {
            if (GetHasPetDataWithUnqIdx(petList[i].unqIdx) == null)
            {
                adoptPetsList.Add(petList[i]);
            }
        }
    }

    public void ResetPlaymateAnimals(int idx)
    {
        playmatePetList.Clear();

        //해당 동물을 해제시키기 위한 용도의 UI. 선택한 칸에 동물이 없으면 패스. //
        if (GetSelectedPetData(idx) != null)
            playmatePetList.Add(null);

        //가지고 있는 동물들 중 현재 선택되지 않은 나머지 동물들을 포함시킨다. //
        for (int i = 0, count = hasPetsList.Count; i < count; i++)
        {
            int unqIdx = hasPetsList[i].unqIdx;

            for (int j = 0; j < Define.selectedPetsCount; j++)
            {
                if (selectedPetArray[j] == null)
                    continue;

                if (selectedPetArray[j].unqIdx == hasPetsList[i].unqIdx)
                {
                    unqIdx = Define.nullValue;
                    break;
                }
            }

            if (unqIdx != Define.nullValue)
                playmatePetList.Add(GetHasPetDataWithUnqIdx(unqIdx));
        }
    }

    private bool HasSameIndex(int idx)
    {
        for (int i = 0, max = listForCalculator.Count; i < max; i++)
        {
            if (listForCalculator[i] == idx)
                return true;
        }
        return false;
    }

    public bool TryGetInvenPetData(ref PetData data, int idx)
    {
        if (invenPetsList.Count <= idx)
            return false;

        data = invenPetsList[idx];
        return true;
    }

    public bool TryGetAdoptPetData(ref PetData data, int idx)
    {
        if (adoptPetsList.Count <= idx)
            return false;

        data = adoptPetsList[idx];
        return true;
    }

    public bool TryGetPlaymatePetData(ref PetData data, int idx)
    {
        if (playmatePetList.Count <= idx)
            return false;

        data = playmatePetList[idx];
        return true;
    }

    public bool TryGetInvenPetIndex(int unqIdx, ref int idx)
    {
        for (int i = 0, max = invenPetsList.Count; i < max; i++)
        {
            if (invenPetsList[i].unqIdx == unqIdx)
            {
                idx = i;
                return true;
            }
        }
        return false;
    }

    public bool IsSelectedAnimal(ref int selectedIdx, int unqIdx)
    {
        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            if (selectedPetArray[i] != null)
            {
                if (selectedPetArray[i].unqIdx == unqIdx)
                {
                    selectedIdx = i;
                    return true;
                }   
            }
        }
        return false;
    }

    public PetData GetSelectedPetData(int idx)
    {
        if (Define.selectedPetsCount > idx && idx >= 0)
        {
            return selectedPetArray[idx];
        }
        return null;
    }

    public void SetSelectedPetData(PetData data, int idx)
    {
        if (Define.selectedPetsCount > idx)
        {
            selectedPetArray[idx] = data;
        }
    }
    
    public string GetSkillIcon(Define.SkillType skillType)
    {
        SkillTable skill = GetSkillTable(skillType);
        if (skill != null)
        {
            return skill.skillIcon;
        }
        Debug.Log("NOSKILL : " + skillType.ToString());
        return "";
    }

    public bool TryAdoptAnimal(int unqIdx)
    {
        PetData table = GetPetDataWithUnqIdx(unqIdx);
        return TryAdoptAnimal(table);
    }

    public bool TryAdoptAnimal(PetData data)
    {
        for (int i = 0, max = GetHasPetListCount(); i < max; i++)
        {
            if (hasPetsList[i].unqIdx == data.unqIdx)
            {
                Debug.Log("already have this animal");
                return false;
            }
        }
        hasPetsList.Add(data);

        if (adoptPetCallback != null)
            adoptPetCallback();

        return true;
    }

    public void SetNowSelectedPetData(PetData data)
    {
        nowSelectedPetData = data;
    }

    public void SetCoinUICallback(System.Action callback)
    {
        coinUiCallback = callback;
    }

    public void SetAdoptPetCallback(System.Action callback)
    {
        adoptPetCallback = callback;
    }

    public void AddCoin(int count)
    {
        SetCoin(coin + count);
    }

    public void UseCoin(int count)
    {
        useCoin += count;
        SetCoin(coin - count);
    }

    private void SetCoin(int count)
    {
        coin = count;
        if (coin >= GetDesignValue(Define.GameDesign.MaxCoinCount))
            coin = GetDesignValue(Define.GameDesign.MaxCoinCount);
        if (coin <= 0)
            coin = 0;

        if (coinUiCallback != null)
            coinUiCallback();
    }

    public bool IsEnoughCoin(int coin, ref int needCoin)
    {
        if (this.coin >= coin)
        {
            return true;
        }
        else
        {
            needCoin = coin - this.coin;
            return false;
        }
    }

    public void SetBestScore(double score)
    {
        if (IsBestScore(score))
            bestScore = score;
    }

    public bool IsBestScore(double score)
    {
        if (bestScore >= score)
            return false;
        return true;
    }

    public Mission GetNowMission(int level)
    {
        if (nowMissions.ContainsKey(level))
            return nowMissions[level];

        return null;
    }

    public bool TryGetRandomMissionType(int level, ref Define.MissionType type)
    {
        if (missionDic.ContainsKey(level))
        {
            int randCount = Random.Range(0, missionDic[level].Count);
            type = missionDic[level][randCount].missionType;
            return true;
        }
        return false;
    }

    public void AddUseSkillCount()
    {
        useSkillCount++;
    }

    public void AddPlayCount()
    {
        playCount++;
    }

    public void AfterShowFreeCoinVideo()
    {
        lastShowFreeCoinVideo = System.DateTime.Now;
        videoCount++;
    }

    public void AfterShowContinueVideo()
    {
        lastShowContinueVideo = System.DateTime.Now;
        videoCount++;
    }

    public bool CanShowFreeCoinVideo()
    {
        System.TimeSpan sub = System.DateTime.Now - lastShowFreeCoinVideo;
        if (sub.TotalSeconds > GetDesignValue(Define.GameDesign.FreeCoinTime))
            return true;
        return false;
    }

    public bool CanShowContinueVideo()
    {
        System.TimeSpan sub = System.DateTime.Now - lastShowContinueVideo;
        if (sub.TotalSeconds > GetDesignValue(Define.GameDesign.ContinueTime))
            return true;
        return false;
    }

    public void SetNeedCoin(int coin)
    {
        needCoinCount = coin;
    }
}