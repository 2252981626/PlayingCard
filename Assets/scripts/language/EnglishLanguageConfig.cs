using System.Collections;
using System.Collections.Generic;

public static class EnglishLanguageConfig
{
    public static Dictionary<string, string> currentTexts = new Dictionary<string, string>()
    {
        {"Loading", "Loading"},
        {"Level", "Level"},
        {"LEVEL", "LEVEL"},
        {"NoThanks", "No,Thanks"},
        {"CLAIMX10", "CLAIMX10"},
        {"CLAIMX2", "CLAIMX2"},
        {"CLAIM", "CLAIM"},
        {"CONTINUE", "CONTINUE"},
        {"QUIT", "QUIT"},
        {"BUY", "BUY"},
        {"FREE", "FREE"},
        {"OK", "OK"},
        {"OnlyClaim", "Only Claim"},
        {"PrivacyPolicy", "Privacy Policy"},
        {"TermsofService", "Terms of Service"},
        //网络
        {"RETRY", "RETRY"},
        {"NetworkStr", "Network connection lost. Please check your internet and try again."},       
        //Lobby
        {"HOME", "HOME"},
        {"PLAY", "PLAY"},
        {"DAILYCHALLENGE", "DAILY CHALLENGE"},
        //建筑
        {"UpgradeArchitecture", "Upgrade the {0}"},
        {"UnlockArchitecture", "Unlock the {0}"},
        {"Architecture1", "Government Building" },//1
        {"Architecture2", "Flower Shop" }, //9
        {"Architecture3", "Police Station" },//8
        {"Architecture4", "Restaurant" },//5
        {"Architecture5", "Bank" },//3
        {"Architecture6", "Shopping Mall" },//2
        {"Architecture7", "Hospital" },//4
        {"Architecture8", "School" },//7
        {"Architecture9", "Iron Gym" },//6
        {"Architecture10", "Amusement Park" },//10
        {"Unlocklevel", "Unlock level" },
        {"Output", "Output:"},     
        //gameTips
        {"GameTipsPanel_1", "Summary: In Klondike Solitaire, your goal is to build ascending card stacks in each foundation pile at the top-left corner. Each foundation pile can only hold one suit."},
        {"GameTipsPanel_2", "Foundation Piles: In Klondike Solitaire, Aces are the lowest cards and Kings are the highest. All four foundation piles must start with an Ace and finish with a King."},
        {"GameTipsPanel_3", "Tableau Piles: There are seven tableau columns. Cards must be stacked in descending order, alternating red and black suits. For example, a black 8 can be placed on top of a red 9."},
        {"GameTipsPanel_4", "Stacks: You can also move sequential stacks of cards between columns. Simply tap the bottom card of the stack and drag all the cards to another column."},
        {"GameTipsPanel_5", "Empty Columns: If a column is empty, you may place a King or any sequential stack topped with a King."},
        {"GameTipsPanel_6", "Move Cards: Tap and drag a single card to move it."},   
        //home
        {"DAILYREWARDS", "DAILY REWARDS"},
        {"LUCKYWHEEL", "LUCKY WHEEL"},
        
        {"Day", "Day"},
        //月份
        {"Jan", "Jan"},
        {"Feb", "Feb"},
        {"Mar", "Mar"},
        {"Apr", "Apr"},
        {"May", "May"},
        {"Jun", "Jun"},
        {"Jul", "Jul"},
        {"Aug", "Aug"},
        {"Sep", "Sep"},
        {"Oct", "Oct"},
        {"Nov", "Nov"},
        {"Dec", "Dec"},
        //星期
        {"Sun", "Sun"},
        {"Mon", "Mon"},
        {"Tue", "Tue"},
        {"Wed", "Wed"},
        {"Thu", "Thu"},
        {"Fri", "Fri"},
        {"Sat", "Sat"},
        //gameScene
        {"gameScene_move", "move:"},
        {"gameScene_time", "time:"},
        {"gameScene_score", "score:"},

        //tipsPanel
        {"NoItemHintTips", "No movable cards available!"},
        {"InsufficientDiamond", "Insufficient diamond!"},
        {"AdsNotReady", "The video is not ready,please try again later."},
        //评分
        {"EvaluationGamePanel_title1", "Are you enjoying the game?"},
        {"EvaluationGamePanel_btn1", "Not Really"},
        {"EvaluationGamePanel_btn2", "Love it!"},
        {"EvaluationGamePanel_title2", "Your 5 stars are very important to us.please give us 5 stars if you like it."},
        //引导
        {"Guide_TxBtn", "Tap here to view your newly earned {0}!"},
        {"Guide_TxPanel2", "Advance through levels to earn {0} progress!"},
        {"GuidePanel_Architecture2", "Once buildings are unlocked, they generate {0}. Clear more stages to unlock new buildings and upgrade them!"},
        {"GuidePanel_dailychangle", "Daily Challenge Unlocked! Clear stages to earn massive rewards!"},
        //Tx
        {"TxPanel_revise", "REVISE"},
        {"TxPanel_BoBao", "Congrats {0} {1} {2} successful!"},
        {"TxProgress_Tips", "Pass Level + Progress {0} All When Full!"},
        {"TxPanel_onlyleft", "only {0} left"},
        {"TxPanel_ex", "Once the progress reaches 100%, you can start the {0}!"},
        {"TxPanel_1Str", "Passed Today:"},
        {"TxPanel_2Str", "Avg {0}:"},
        {"TxPanel_3Str", "Avg Attempts:"},
        {"selectType_explain", "Please enter your account information"},

        {"FinalStep_explain", "Complete the collection tasks below to verify your account activity."},
        {"FinalStep_explain2", "Get letter props by clearing levels; extra letters auto-convert to {0}."},

        {"TxElementTypeSelectPanel_explain", "Please enter your account"},
        {"TxElementTypeSelectPanel_input1", "Please enter your {0} account"},
        {"TxElementTypeSelectPanel_input2", "Verify your {0} account"},
        {"TxElementTypeSelectPanel_Error", "Accounts are inconsistent!"},
        {"TxElementTypeSelectPanel_Error2", "Incorrect accounts input!"},
        {"CANCLE", "CANCLE"},
        {"SUBMIT", "SUBMIT"},

        {"TxElementGameStartPanel_t1", "When progress reaches 100%"},
        {"TxElementGameStartPanel_t2", "{0} All {1}"},
        {"TxElementGameStartPanel_t3", "Pass The Level Progress + {0}"},

        {"Special_Diamond__unit", "JA=="},//特殊钻石符号$
        {"CHT", "Y2FzaCBvdXQ="},//Cash out
        {"CH", "Q2FzaA=="},//Cash 
        {"WD", "V0lUSERSQVc="},//Withdraw?
        {"wd", "d2l0aGRyYXc="},
        {"Wh", "V2l0aGRyYXdhbA=="},//Withdrawal 
        {"wh", "d2l0aGRyYXdhbA=="},
        {"pp", "cGF5cGFs"},//paypal
        
    };
}
