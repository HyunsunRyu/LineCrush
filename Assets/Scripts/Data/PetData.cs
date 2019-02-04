using CodeStage.AntiCheat.ObscuredTypes;

public class PetData
{
    private AnimalTable animalTable;
    private AnimationTable animTable;
    
    public int unqIdx { get { return animalTable.unqIdx; } }
    public Define.SkillType activeSkillType { get { return animalTable.activeSkillType; } }
    public Define.SkillType passiveSkillType { get { return animalTable.passiveSkillType; } }
    public ObscuredInt cost { get { return animalTable.cost; } }

    //changeable data. loaded from saved data. //
    public ObscuredInt level { get; private set; }
    public ObscuredInt exp { get; private set; }
    public int pSkillLv { get; private set; }
    public int aSkillLv { get; private set; }
    
    public PetData(AnimalTable animalTable, AnimationTable animTable)
    {
        this.animalTable = animalTable;
        this.animTable = animTable;
    }

    public void SetData(int level, int exp, int pSkillLv, int aSkillLv)
    {
        this.level = level;
        this.exp = exp;
        this.pSkillLv = pSkillLv;
        this.aSkillLv = aSkillLv;
    }

    public int GetSkillLevel(int idx)
    {
        if (idx < 0 || idx >= 2)
            return 0;
        return idx == 0 ? aSkillLv : pSkillLv;
    }

    public SkillTable GetActiveSkill() { return DataManager.GetInstance().GetSkillTable(activeSkillType); }
    public SkillTable GetPassiveSkill() { return DataManager.GetInstance().GetSkillTable(passiveSkillType); }

    public string GetName() { return DataManager.GetText(animalTable.nameIdx); }
    public string GetProfile() { return animTable.GetProfile(); }
    public string GetOnGameImage() { return animTable.GetOnGameImage(); }
    public bool TryGetImage(int idx, ref string image) { return animTable.TryGetImage(idx, ref image); }
    public string GetPopupOutline() { return animTable.GetPopupOutline(); }

    public bool AddExp(int addExp)
    {
        if (IsMaxLevel())
            return false;

        exp += addExp;
        return SetExp(exp);
    }

    public bool SetExp(int point)
    {
        if (IsMaxLevel())
            return false;

        bool bLevelUp = false;
        exp = point;
        while (true)
        {
            if (IsMaxLevel())
                return bLevelUp;

            int maxExp = GetMaxExp();
            if (maxExp <= 0)
            {
                Util.DebugLog("something is wrong");
                return bLevelUp;
            }
            if (exp >= maxExp)
            {
                exp -= maxExp;
                LevelUp();
                bLevelUp = true;
            }
            else
                return bLevelUp;
        }
    }

    public void SetLevel(int lv)
    {
        level = lv;
        if (level < 0)
            level = 0;
        if (IsMaxLevel())
            level = Define.maxCharacterLevel;
    }

    public void LevelUp()
    {
        if (IsMaxLevel())
            return;

        level++;
        if (IsMaxLevel())
            level = Define.maxCharacterLevel;
    }

    public bool IsMaxLevel()
    {
        return level >= Define.maxCharacterLevel;
    }

    public bool IsMaxLevel(int lv)
    {
        return lv >= Define.maxCharacterLevel;
    }

    public int GetMaxSkillCoolTime()
    {
        SkillTable skill = GetActiveSkill();
        
        if (skill != null)
        {
            return skill.GetSkillCoolTime(aSkillLv);
        }
        Util.DebugLog("something is wrong");
        return 9999;
    }

    public int GetMaxExp()
    {
        if (IsMaxLevel())
            return 0;

        LevelTable level = DataManager.GetInstance().GetLevelTable(animalTable.levelType, this.level);
        if (level != null)
        {
            return level.maxExp;
        }
        Util.DebugLog("something is wrong");
        return 9999;
    }

    public int GetMaxExp(int level)
    {
        LevelTable data = DataManager.GetInstance().GetLevelTable(animalTable.levelType, level);
        if (data != null)
        {
            return data.maxExp;
        }
        return -1;
    }

    public void ActiveSkillLevelUp()
    {
        if (GetActiveSkill().IsMaxLevel(aSkillLv))
            return;
        aSkillLv++;
    }

    public void PassiveSkillLevelUp()
    {
        if (GetPassiveSkill().IsMaxLevel(pSkillLv))
            return;
        pSkillLv++;
    }

    public bool CanActiveSkillUpgrade()
    {
        if (level <= aSkillLv)
            return false;
        if (GetActiveSkill().IsMaxLevel(aSkillLv))
            return false;
        int needCoin = 0;
        if (DataManager.GetInstance().IsEnoughCoin(GetActiveSkill().GetSkillCost(aSkillLv), ref needCoin))
            return true;
        return false;
    }

    public bool CanPassiveSkillUpgrade()
    {
        if (level <= pSkillLv)
            return false;
        if (GetPassiveSkill().IsMaxLevel(pSkillLv))
            return false;
        int needCoin = 0;
        if (DataManager.GetInstance().IsEnoughCoin(GetPassiveSkill().GetSkillCost(pSkillLv), ref needCoin))
            return true;
        return false;
    }
}
