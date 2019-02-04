using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PetAssistance : Assistance
{
    private struct SkillPack
    {
        public bool isNull;
        public int petIdx;
        public int nowCooltime;
        public int maxSkillCooltime;
        public int useSkillCount;
    }

    public static bool bActingSkill { get; private set; }

    private static int[] gotExpInGame;
    
    private SelectedPetController[] petControllers = new SelectedPetController[Define.selectedPetsCount];
    private SkillPack[] skillData = new SkillPack[Define.selectedPetsCount];
    private Action<int, int, int> skillCooltimeGageCallback;
    private int bonusExp;
    private Define.SkillType nowUseSkill;
    private MapAssistance mapAssistance;
    private ScoreAssistance scoreAssistance;
    private GameTimeAssistance gameTimeAssistance;

    public override void Init(TileSystem tileSystem)
    {
        base.Init(tileSystem);

        if (gotExpInGame == null)
            gotExpInGame = new int[Define.selectedPetsCount];

        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            skillData[i] = new SkillPack();
        }

        mapAssistance = GetAssistance(typeof(MapAssistance).Name) as MapAssistance;
        scoreAssistance = GetAssistance(typeof(ScoreAssistance).Name) as ScoreAssistance;
        gameTimeAssistance = GetAssistance(typeof(GameTimeAssistance).Name) as GameTimeAssistance;
    }

    public override void Enable()
    {
        bActingSkill = false;
        bonusExp = 1;
        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            gotExpInGame[i] = 0;

            PetData pet = DataManager.GetInstance().GetSelectedPetData(i);
            if (pet != null)
            {
                skillData[i].isNull = false;
                skillData[i].petIdx = pet.unqIdx;
                skillData[i].maxSkillCooltime = pet.GetMaxSkillCoolTime();
                skillData[i].nowCooltime = 0;
                skillData[i].useSkillCount = 0;
            }
            else
                skillData[i].isNull = true;
        }
        UpdateSkillCooltime();

        DataManager.GetInstance().ClearUseSkillData();
    }

    public override void ClearMemory()
    {
        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            if (petControllers[i] != null)
            {
                petControllers[i].ClearData();
                PoolManager.ReturnObject(petControllers[i]);
                petControllers[i] = null;
            }
        }
    }

    //선택한 동물을 위치시킨다. //
    public IEnumerator SetPositionSelectedPets()
    {
        yield return new WaitForSeconds(0.5f);
        //부족한 공간만큼 동물들을 생성해서 재배치 이동시킨다. //
        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            if (DataManager.GetInstance().GetSelectedPetData(i) != null)
            {
                SelectedPetController pet = GetSelectedPet(i);
                pet.Show();
                petControllers[i] = pet;
                yield return new WaitForSeconds(Define.readyTileMoveTime);
            }
            else
            {
                petControllers[i] = null;
            }
        }
    }

    //선택된 동물을 생성하여 반환한다. //
    private SelectedPetController GetSelectedPet(int idx)
    {
        SelectedPetController animal = PoolManager.GetObject<SelectedPetController>();
        animal.Init(this);
        animal.SetData(idx);
        return animal;
    }

    //제거된 블록 데이터를 바탕으로 스킬 사용이 가능한 동물이 있는지 확인한다. //
    public int CheckAllPetsCanUseSkill(ClearTileData data)
    {
        int skillFullCount = 0;
        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            if (skillData[i].isNull)
                continue;
            
            //해당 동물 타입의 지워진 블록이 있는지 확인하고, 있다면 반환. //
            int count;
            if (data.TryGetClearedTilesCount(i, out count))
            {
                //해당 동물의 쿨타임 차감. //
                AddPetSkillCooltime(i, count);
            }

            if (IsFullSkillGage(i))
                skillFullCount++;
        }
        return skillFullCount;
    }

    //해당 동물의 스킬 쿨타임을 추가한다. //
    private void AddPetSkillCooltime(int idx, int count)
    {
        if (skillData[idx].isNull)
        {
            Debug.Log("something is wrong");
            return;
        }

        skillData[idx].nowCooltime += count;
        if (skillData[idx].nowCooltime >= skillData[idx].maxSkillCooltime)
        {
            skillData[idx].nowCooltime = GetMaxSkillCoolTime(idx);

            if (petControllers[idx] != null)
                petControllers[idx].FullSkillGage();
        }
        else
        {
            if (petControllers[idx] != null)
                petControllers[idx].EmphasisSkillGage();
        }
        
        if (skillCooltimeGageCallback != null)
            skillCooltimeGageCallback(idx, skillData[idx].nowCooltime, GetMaxSkillCoolTime(idx));
    }

    public void UpdateSkillCooltime()
    {
        if (skillCooltimeGageCallback == null)
            return;

        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            if (!skillData[i].isNull)
                skillCooltimeGageCallback(i, skillData[i].nowCooltime, GetMaxSkillCoolTime(i));
        }
    }

    public bool IsFullSkillGage(int idx)
    {
        if (skillData[idx].isNull)
            return false;

        return skillData[idx].nowCooltime >= GetMaxSkillCoolTime(idx);
    }

    public bool CanUseSkill(int idx)
    {
        if (!GameManager.IsPlaying() || bActingSkill)
            return false;
        if (!IsFullSkillGage(idx))
            return false;
        PetData data = DataManager.GetInstance().GetSelectedPetData(idx);
        if (data == null)
            return false;
        SkillTable skill = DataManager.GetInstance().GetSkillTable(data.activeSkillType);
        if (skill == null)
            return false;
        return true;
    }

    public void UseSkill(int idx)
    {
        PetData data = DataManager.GetInstance().GetSelectedPetData(idx);
        SkillTable skill = DataManager.GetInstance().GetSkillTable(data.activeSkillType);

        bActingSkill = true;

        UseSkillPopup popup = PopupSystem.GetPopup<UseSkillPopup>(Define.PopupType.UseSkill);
        popup.SetData(data, () =>
        {
            skillData[idx].nowCooltime = 0;
            skillData[idx].useSkillCount++;
            scoreAssistance.SetSkillPointMode();

            nowUseSkill = skill.skillType;

            switch (skill.skillType)
            {
                case Define.SkillType.StopTime:
                    gameTimeAssistance.FreezeTime(skill.GetSkillValue(data.aSkillLv));
                    AfterUseSkill();
                    break;
                case Define.SkillType.BrokenTiles:
                    mapAssistance.AllBlocksDown(AfterUseSkill);
                    break;
                case Define.SkillType.ClearAllTiles:
                    mapAssistance.ClearAllBlocks(AfterUseSkill);
                    break;
                case Define.SkillType.DoubleScore:
                    scoreAssistance.SetMultiBonus(
                        DataManager.GetDesignValue(Define.GameDesign.DoubleSkillScoreBonus),
                        skill.GetSkillValue(data.aSkillLv)
                        );
                    AfterUseSkill();
                    break;
                case Define.SkillType.FillLines:
                    mapAssistance.ChangeTileTypeToFillLines(idx, AfterUseSkill);
                    break;
                case Define.SkillType.ChangePuzzles:
                    mapAssistance.ChangeAllPuzzles(AfterUseSkill);
                    break;
                default:
                    Debug.Log("something is wrong");
                    AfterUseSkill();
                    break;
            }
        });
        PopupSystem.OpenPopup(Define.PopupType.UseSkill);
    }

    private void AfterUseSkill()
    {
        bActingSkill = false;
        mapAssistance.CalculateAllClearTiles();

        if (mapAssistance.HasClearTilesOnMap())
        {
            StartCoroutine(mapAssistance.ClearTiles((ClearTileData clearTileData) =>
            {
                CheckAllPetsCanUseSkill(clearTileData);
                mapAssistance.UpdateMap();
                tileSystem.AddCombo();
            }));
        }
        //미션 갱신된 것이 있는지 체크. //
        DataManager.GetInstance().AddUseSkillCount();
        MissionManager.GetInstance().AddUseSkillCount();
        MissionManager.GetInstance().CheckClearedMission();
        UpdateSkillCooltime();
        
        DataManager.GetInstance().SetUseSkillData(nowUseSkill);
    }

    public static int GetExp(int idx)
    {
        if (idx >= Define.selectedPetsCount || idx < 0)
            return 0;

        return gotExpInGame[idx];
    }

    public static void SetExp(int idx, int exp)
    {
        if (idx >= Define.selectedPetsCount || idx < 0)
            return;

        gotExpInGame[idx] = exp;
    }

    public void AddExp(int idx, int score)
    {
        if (bonusExp != 1)
            score *= bonusExp;

        gotExpInGame[idx] += score;
    }

    public int GetNowSkillCoolTime(int idx)
    {
        if (skillData[idx].isNull)
        {
            Debug.Log("something is wrong");
            return 0;
        }

        return skillData[idx].nowCooltime;
    }

    public int GetMaxSkillCoolTime(int idx)
    {
        if (skillData[idx].isNull)
        {
            Debug.Log("something is wrong");
            return 9999;
        }
        //횟수에 따라 최대치가 증가, 최대 10회일때 최대쿨타임의 세배가 되도록. //
        return Mathf.RoundToInt(skillData[idx].maxSkillCooltime * (Mathf.Min(skillData[idx].useSkillCount, 10) * 0.2f + 1));
    }

    public void SetSkillCountCallback(Action<int, int, int> callback)
    {
        skillCooltimeGageCallback = callback;
    }
}
