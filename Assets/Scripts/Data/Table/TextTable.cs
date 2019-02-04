using UnityEngine;

public class TextTable : DataController.DicBase
{
    private static int languageIdx = 0;

    public static void SetLanguage(SystemLanguage language)
    {
        //현재 영문번역 안되어있음. //
        language = SystemLanguage.Korean;

        switch (language)
        {
            case SystemLanguage.Korean:
                languageIdx = 0;
                break;
            case SystemLanguage.English:
            default:
                languageIdx = 1;
                break;
        }
    }

    public string text { get; private set; }
    
    public override bool TryLoadTable(string[] str, out int key)
    {
        int arrayIdx = 0;
        try
        {
            key = int.Parse(str[arrayIdx++]);
            text = str[arrayIdx + languageIdx];
        }
        catch (System.Exception e)
        {
            key = -1;
            Debug.Log(arrayIdx);
            Debug.LogError(e.ToString());
            return false;
        }
        return true;
    }

    public static readonly int adoptTitleKey = 80000;           //데려오기. //
    public static readonly int invenTitleKey = 80001;           //정보보기. //
    public static readonly int selectCharacterMsg = 80002;      //캐릭터를 선택하세요. //
    public static readonly int playmateKey = 80003;             //단짝친구. //
    public static readonly int addPetPlaymateKey = 80004;       //단짝친구를 추가해주세요. //
    public static readonly int unselectKey = 80005;             //선택해제. //
    public static readonly int playTitleKey = 80006;            //놀러가기. //
    public static readonly int missionKey = 80007;              //목표. //
    public static readonly int setupKey = 80008;                //설정. //
    public static readonly int resetButtonKey = 80009;          //데이터 초기화. //
    public static readonly int quitMsgKey = 80010;              //게임을 그만하시려구요? //
    public static readonly int adoptPetPopupMsgKey = 80011;     //{0}를 데려올까요? //
    public static readonly int goShopPopupMsgKey = 80012;       //상점에 들르시겠어요? //
    public static readonly int playBtnKey = 80013;              //놀러가기. //
    public static readonly int cantPlayGameKey = 80014;         //함께 놀러갈 단짝친구를 선택해주세요. //
    public static readonly int pauseTitleKey = 80015;           //일시정지. //
    public static readonly int completeAllMissionKey = 80016;   //모든 목표 완수! //
    public static readonly int readySkillKey = 80017;           //준비. //
    public static readonly int upgradeBtnKey = 80018;           //업그레이드. //
    public static readonly int allClearKey = 80019;             //모두 완료. //
    public static readonly int earnCoinMsgSingularKey = 80020;  //축하해요!\n {0}행운을 얻으셨어요! //
    public static readonly int earnCoinMsgPluralKey = 80021;    //축하해요!\n {0}행운을 얻으셨어요! //
    public static readonly int notEnoughSkillLvUpgrade = 80022; //레벨이 부족합니다. //
    public static readonly int comboTextKey = 80023;            //{0} 콤보! //
    public static readonly int buyTextKey = 80024;              //구매. //
    public static readonly int shopTextKey = 80025;             //상점. //
    public static readonly int cantUpgradeKey = 80026;          //상점. //
    public static readonly int maxLevelKey = 80027;             //레벨이 최대에 도달했습니다. //
    public static readonly int adoptLevelKey = 80028;           //데려온 이후에 레벨업이 가능합니다. //
    public static readonly int rewardKey = 80029;               //보상. //
    public static readonly int errorBuyProductKey = 80030;      //결제 과정에서 오류가 발생했습니다. //
    public static readonly int senseCrackKey = 80031;           //해킹이 의심되어 게임을 재시작합니다. //
    public static readonly int luckyCountKey = 80032;           //행운\n{0}개. //
    public static readonly int loadingKey = 80033;              //로딩중... //
    public static readonly int freeCoinKey = 80034;             //무료 행운 {0}. //
    public static readonly int errorVideKey = 80035;            //문제가 발생하여 광고를 볼 수 없습니다. //
    public static readonly int failedVideoKey = 80036;          //광고 시청 중 문제가 발생하여 보상을 받을 수 없습니다. //
    public static readonly int skipVideoKey = 80037;            //광고를 끝까지 시청하지 않아 보상을 받을 수 없습니다. //
    public static readonly int warningBuyKey = 80038;           //경고 : 앱을 지우면 결제한 행운도 사라집니다. //
    public static readonly int timeupKey = 80039;               //Time Up. //
    public static readonly int gameoverKey = 80040;             //Game over. //
    public static readonly int readyKey = 80041;                //ready. //
    public static readonly int goKey = 80042;                   //Go. //
    public static readonly int continueOption1Key = 80043;      //모든 타일 제거. //
    public static readonly int continueOption2Key = 80044;      //{0}초 추가. //
    public static readonly int warningContinueKey = 80045;      //최소 {0}분마다 계속하기가 가능합니다. //
    public static readonly int notEnoughCoinKey = 80046;        //행운이 {0}만큼 부족해요. //
    public static readonly int wellcomeShopKey = 80047;         //상점에 오신걸 환영해요. //
    public static readonly int hasFreeCoinKey = 80048;          //무료로 행운 받아가세요. //
}
