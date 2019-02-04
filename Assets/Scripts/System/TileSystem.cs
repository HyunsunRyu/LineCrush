using UnityEngine;

public class TileSystem : ISystem
{
    public static TileSystem Instance { get; private set; }

    [SerializeField] private GameTimeAssistance gameTimeAssistance;
    [SerializeField] private PetAssistance petAssistance;
    [SerializeField] private MapAssistance mapAssistance;
    [SerializeField] private ScoreAssistance scoreAssistance;

    private MapData mapData;

    private bool activeGameGuideTutorial = false;
    private bool activeSkillGuideTutorial = false;
    private int comboCount = 0;
    private int keepComboCount = 0;
    private int nowKeepComboCount = 0;
    private System.Action<int> comboCountCallback = null;
    private System.Action<float> addTimeCallback = null;

    private const int maxCombo = 6;
    private const int minCombo = 2;

    public override void InitSystem()
	{
        Instance = this;

        Assistance.ClearAssistances();
        Assistance.SetAssistance(gameTimeAssistance);
        Assistance.SetAssistance(petAssistance);
        Assistance.SetAssistance(mapAssistance);
        Assistance.SetAssistance(scoreAssistance);

        Assistance.InitAllAssistances(this);

        mapData = MapData.GetInstance();

        PuzzleController.SetCallback(PutPuzzleOnMap, EmphasisTilesOnMap);
    }
    
    public override void InitData()
	{
        ClearAssistance();
        ClearBasicData();

        mapData.InitData();

        Assistance.EnableAllAssistances();

        activeGameGuideTutorial = !TutorialManager.GetInstance().IsCleared(Define.TutorialType.GameGuide);
        activeSkillGuideTutorial = !TutorialManager.GetInstance().IsCleared(Define.TutorialType.SkillGuide);

        mapAssistance.SetGameGuideTutorial(activeGameGuideTutorial);

        SetCombo(0);
        nowKeepComboCount = 0;
        keepComboCount = 0;
        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            PetData pet = DataManager.GetInstance().GetSelectedPetData(i);
            if (pet != null && pet.passiveSkillType == Define.SkillType.KeepCombo)
            {
                int count = pet.GetPassiveSkill().GetSkillValue(pet.pSkillLv);
                if (keepComboCount < count)
                    keepComboCount = count;
            }
        }
    }

    private void ClearAssistance()
    {
        Assistance.ClearMemoryAllAssistances();
    }

    private void ClearBasicData()
    {
    }
    
    public override void ClearSystem()
    {
        Instance = null;
        ClearAssistance();
    }

    public void ClearMap()
    {
        mapAssistance.ClearMap();
        mapAssistance.UpdateMap();
    }

    public void SetComboCountCallback(System.Action<int> callback)
    {
        comboCountCallback = callback;
    }

    public void SetAddTimeCallback(System.Action<float> callback)
    {
        addTimeCallback = callback;
    }

    public void GameStartOpening()
    {
        //상단에 구해줘야 할 동물들 리스트 등장. //
        StartCoroutine(petAssistance.SetPositionSelectedPets());
    }

    //게임 시작. //
    public void GameStart()
    {
        comboCount = 0;
        //게임 시간 체크 시작. //
        gameTimeAssistance.StartCheckGameTime();
        
        //하단에 퍼즐조각들 리스트 등장. //
        StartCoroutine(mapAssistance.CallInPuzzleTiles(null, ()=>
        {
            if (activeGameGuideTutorial)
            {
                StartGameGuideTutorial();
            }
        }));
    }

    private void StartGameGuideTutorial()
    {
        gameTimeAssistance.PauseTimeChecking(true);
        mapAssistance.ReturnPuzzles();
        TutorialManager.GetInstance().StartTutorial(Define.TutorialType.GameGuide, () =>
        {
            activeGameGuideTutorial = false;
            gameTimeAssistance.PauseTimeChecking(false);
            mapAssistance.SetGameGuideTutorial(false);
        });
    }

    private void StartSkillGuideTutorial()
    {
        int idx = 0;
        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            if (petAssistance.IsFullSkillGage(i))
            {
                idx = i;
                break;
            }
        }

        TutorialManager.GetInstance().SetTutorialValue(idx);
        gameTimeAssistance.PauseTimeChecking(true);
        TutorialManager.GetInstance().StartTutorial(Define.TutorialType.SkillGuide, () =>
        {
            activeSkillGuideTutorial = false;
            gameTimeAssistance.PauseTimeChecking(false);
        });
    }

    //맵 위에 타일 강조. //
    public void EmphasisTilesOnMap(Pos pos, PuzzleController tile)
    {
        mapAssistance.EmphasisTilesOnMap(pos, tile);
    }
    
    //해당 퍼즐을 맵 위에 놓았을 때. //
    public void PutPuzzleOnMap(PuzzleController tile)
    {
        //현재 위치에 퍼즐을 놓을 수 있는가? //
        if (mapAssistance.IsPossibleToPutOn())
        {
            if (tile.tileType.IsSameTile(TileData.BlockType.FillBlocks))
            {
                int side = Random.Range(0, 2);
                mapAssistance.FillLines(tile, side);
            }
            SoundManager.GetInstance().PlaySound(Define.SoundType.PutTileOnBoard);
            
            //퍼즐을 배치한 횟수 추가. //
            MissionManager.GetInstance().AddArrangeCount();

            //미션 갱신된 것이 있는지 체크. //
            MissionManager.GetInstance().CheckClearedMission();

            //더블 보너스가 있다면, 턴 차감. //
            scoreAssistance.SubDoubleBonusCount();

            //현재 위치에 퍼즐을 놓는다. //
            mapAssistance.PutPuzzleOnMap(tile);
            //지울 수 있는 타일이 있는지 모든 타일을 계산. //
            mapAssistance.CalculateAllClearTiles();
            //지울수 있는 타일이 맵 상에 있는가? //
            if (mapAssistance.HasClearTilesOnMap())
            {
                //지울 수 있는 모든 타일들을 지운다. //
                StartCoroutine(mapAssistance.ClearTiles((ClearTileData clearTileData)=>
                {
                    //모든 타일을 재갱신하고 강조 타일도 초기화한다. //
                    mapAssistance.UpdateMap();
                    //클리어한 타일들을 바탕으로 스킬사용이 가능한 동물이 있는지 체크한다. //
                    int count = petAssistance.CheckAllPetsCanUseSkill(clearTileData);

                    if (activeSkillGuideTutorial && count > 0)
                    {
                        StartSkillGuideTutorial();
                    }

                    //모든 지워져야 할 타일을 지운 후. //
                    //지워진 데이터를 맵에 반영시키고, //
                    mapAssistance.UpdateMap();
                    
                    //새로운 퍼즐 조각들을 배치시킨다. //
                    StartCoroutine(mapAssistance.CallInPuzzleTiles(null));

                    AddCombo();
                }));
            }
            else
            {
                //모든 타일을 재갱신하고 강조 타일도 초기화한다. //
                mapAssistance.UpdateMap();
                //새로운 퍼즐 조각들을 배치시킨다. //
                StartCoroutine(mapAssistance.CallInPuzzleTiles(null));

                nowKeepComboCount++;
                if (nowKeepComboCount > keepComboCount)
                    SetCombo(0);
            }
        }
        else
        {
            //없다면, 해당 타일을 원래 대기라인으로 돌려놓는다. //
            mapAssistance.PuzzleReturntoReadyLine(tile);
        }
    }

    public void AddCombo()
    {
        SetCombo(comboCount + 1);
    }

    public void SetCombo(int count)
    {
        comboCount = count;

        MissionManager.GetInstance().SetComboCount(comboCount);

        if (comboCountCallback != null)
            comboCountCallback(comboCount);

        float addTime = 0f;
        if (count > maxCombo)
            count = maxCombo;
        else if (count < minCombo)
            count = 0;

        if (count == 0)
        {
            nowKeepComboCount = 0;
            return;
        }

        switch (count)
        {
            case 2:
                addTime = 0.5f;
                break;
            case 3:
                addTime = 1f;
                break;
            case 4:
                addTime = 2f;
                break;
            case 5:
                addTime = 4f;
                break;
            case 6:
                addTime = 8f;
                break;
        }
        gameTimeAssistance.AddTime(addTime);
        if (addTimeCallback != null)
            addTimeCallback(addTime);
    }
    
    public void GameOver()
	{
        scoreAssistance.ClearWarningBonus();
        gameTimeAssistance.GameOver();
        mapAssistance.GameOver();
	}

    public void GameContinue()
    {
        gameTimeAssistance.GameContinue();
    }

    public static bool CanClickTile()
    {
        return !MapAssistance.bClearingTiles && !PetAssistance.bActingSkill;
    }
}