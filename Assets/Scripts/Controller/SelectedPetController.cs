using UnityEngine;
using System.Collections;

public class SelectedPetController : PoolObject
{
    public readonly Vector3 startPosition = new Vector3(700f, Define.animalLineHeight, 0f);

    public int index { get; private set; }
    
    private const float effectTime = 0.1f;

    [SerializeField] private UISprite imageSprite;
    [SerializeField] private Animator anim;
    
    private PetData petData = null;
    private PetAssistance assistance = null;

    public void Init(PetAssistance assistance)
    {
        this.assistance = assistance;
    }

    public void SetData(int idx)
    {
        if (index >= Define.selectedPetsCount || index < 0)
        {
            Debug.Log("something is wrong");
            return;
        }

        index = idx;
        petData = DataManager.GetInstance().GetSelectedPetData(index);

        imageSprite.spriteName = petData.GetOnGameImage();

        mTrans.parent = GameManager.animalRoot;
        mTrans.localScale = Vector3.one;
        mTrans.localPosition = startPosition;

        ChangeImageDepth(-10);
    }

    public void ClearData()
    {
        ChangeImageDepth(-10);
    }

    public void Show()
    {
        mTrans.localPosition = GetPos(index);
        anim.Play("Owl_ShowUp");
    }

    public void PlayIdle()
    {
        anim.Play("Idle");
    }

    public void EmphasisSkillGage()
    {
        anim.Play("SkillEmphasis");
    }

    public void FullSkillGage()
    {
        anim.Play("SkillEmphasisLoop");
    }

    private Vector3 GetPos(int idx)
    {
        return new Vector3(((Define.selectedPetsCount - 1) * -0.5f + idx) * Define.readyWidth, Define.animalLineHeight, 0f);
    }

    public void UseSkill()
    {
        if (!assistance.IsFullSkillGage(index))
            return;

        if (assistance.CanUseSkill(index))
        {
            PlayIdle();
            assistance.UseSkill(index);
        }
    }

    public void ChangeImageDepth(int depth)
    {
        imageSprite.depth = depth;
    }
}
