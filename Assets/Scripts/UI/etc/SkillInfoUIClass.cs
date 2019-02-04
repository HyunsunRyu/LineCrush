using UnityEngine;

[System.Serializable]
public class SkillInfoUIClass
{
    [SerializeField] private GameObject skillBody;
    [SerializeField] private UISprite skillBack;
    [SerializeField] private UISprite skillImage;
    [SerializeField] private UILabel skillLabel;

    public void SetSkillInfo(int skillIdx, int skillLv)
    {
    }
}