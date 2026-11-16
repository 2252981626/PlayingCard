using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 葡萄牙语配置
/// </summary>
public static class PortugueseLanguageConfig
{
    public static Dictionary<string, string> currentTexts = new Dictionary<string, string>()
    {
        {"Loading", "Carregando"},
        {"Level", "Nível"},
        {"LEVEL", "Nível"},
        {"NoThanks", "Não, obrigado"},
        {"CLAIMX10", "ResgatarX10"},
        {"CLAIMX2", "ResgatarX2"},
        {"CLAIM", "Resgatar"},
        {"CONTINUE", "Continuar"},
        {"QUIT", "Sair"},
        {"BUY", "Comprar"},
        {"FREE", "Grátis"},
        {"OK", "OK"},
        {"OnlyClaim", "Só Resgatar"},
        {"PrivacyPolicy", "Política de Privacidade"},
        {"TermsofService", "Termos de Serviço"},
        //网络
        {"RETRY", "Tentar"},
        {"NetworkStr", "Conexão perdida. Verifique sua internet e tente novamente."},       
        //Lobby
        {"HOME", "Início"},
        {"PLAY", "Jogar"},
        {"DAILYCHALLENGE", "Desafio Diário"},
        //建筑
        {"UpgradeArchitecture", "Atualizar o {0}"},
        {"UnlockArchitecture", "Desbloquear o {0}"},
        {"Architecture1", "Predio Gov" },//1
        {"Architecture2", "Loja Flores" }, //9
        {"Architecture3", "Delegacia Polícia" },//8
        {"Architecture4", "Restaurante" },//5
        {"Architecture5", "Banco" },//3
        {"Architecture6", "Shopping Center" },//2
        {"Architecture7", "Hospital" },//4
        {"Architecture8", "Escola" },//7
        {"Architecture9", "Academia Iron" },//6
        {"Architecture10", "Parque Diversões" },//10
        {"Unlocklevel", "Desbloquear Nível" },
        {"Output", "Produção:"},     
        //gameTips
        {"GameTipsPanel_1", "Resumo: No Paciência Klondike, seu objetivo é montar pilhas de cartas em ordem crescente em cada monte de base no canto superior esquerdo. Cada monte de base só aceita um único naipe."},
        {"GameTipsPanel_2", "Montes de Base: No Paciência Klondike, os Áses são as cartas menores e os Reis as maiores. Os quatro montes de base devem começar com um Ás e terminar com um Rei."},
        {"GameTipsPanel_3", "Montes do Quadro: Existem sete colunas de cartas. As cartas devem ser empilhadas em ordem decrescente, alternando naipes vermelho e preto. Por exemplo, um 8 preto pode ser colocado sobre um 9 vermelho."},
        {"GameTipsPanel_4", "Pilhas: Você também pode mover sequências de cartas entre as colunas. Basta tocar a carta inferior da pilha e arrastar todas as cartas para outra coluna."},
        {"GameTipsPanel_5", "Colunas Vazias: Se uma coluna estiver vazia, você pode colocar um Rei ou qualquer sequência de cartas que tenha um Rei no topo."},
        {"GameTipsPanel_6", "Mover Cartas: Toque e arraste uma única carta para movê-la."},   
        //home
        {"DAILYREWARDS", "Recomp. Diárias"},
        {"LUCKYWHEEL", "Roda Sorte"},

        {"Day", "Dia"},
        //月份
        {"Jan", "Jan"},
        {"Feb", "Fev"},
        {"Mar", "Mar"},
        {"Apr", "Abr"},
        {"May", "Mai"},
        {"Jun", "Jun"},
        {"Jul", "Jul"},
        {"Aug", "Ago"},
        {"Sep", "Set"},
        {"Oct", "Out"},
        {"Nov", "Nov"},
        {"Dec", "Dez"},
        //星期
        {"Sun", "Dom"},
        {"Mon", "Seg"},
        {"Tue", "Ter"},
        {"Wed", "Qua"},
        {"Thu", "Qui"},
        {"Fri", "Sex"},
        {"Sat", "Sáb"},
        //gameScene
        {"gameScene_move", "mover:"},
        {"gameScene_time", "tempo:"},
        {"gameScene_score", "pontuação:"},

        //tipsPanel
        {"NoItemHintTips", "Sem cartas disponíveis para mover!"},
        {"InsufficientDiamond", "Poucos diamantes!"},
        {"AdsNotReady", "Vídeo não pronto, tente depois."},
        //评分
        {"EvaluationGamePanel_title1", "Você está gostando do jogo?"},
        {"EvaluationGamePanel_btn1", "Não muito"},
        {"EvaluationGamePanel_btn2", "Adoro!"},
        {"EvaluationGamePanel_title2", "Sua avaliação de 5 estrelas é muito importante para nós. Por favor, nos dê 5 estrelas se gostar do jogo."},
        //引导
        {"Guide_TxBtn", "Toque para ver seu novo {0}!"},
        {"Guide_TxPanel2", "Avance pelos níveis para ganhar progresso de {0}!"},
        {"GuidePanel_Architecture2", "Edifícios desbloqueados geram {0}. Conclua fases para liberar e melhorar edifícios!"},
        {"GuidePanel_dailychangle", "Desafio Diário Desbloqueado! Conclua fases para ganhar grandes recompensas!"},
        //Tx
        {"TxPanel_revise", "REVISAR"},
        {"TxPanel_BoBao", "Parabéns! {0} {1} {2} com sucesso!"},
        {"TxProgress_Tips", "Passe de Nível + Progresso {0} Todos Quando Completos!"},
        {"TxPanel_onlyleft", "restam apenas {0}"},
        {"TxPanel_ex", "Quando o progresso chegar a 100%, você pode iniciar a {0}!"},
        {"TxPanel_1Str", "Concluídos Hoje:"},
        {"TxPanel_2Str", "Média {0}:"},
        {"TxPanel_3Str", "Média de Tentativas:"},
        {"selectType_explain", "Insira os dados da conta"},

        {"FinalStep_explain", "Conclua as tarefas de coleta abaixo para verificar a atividade da conta."},
        {"FinalStep_explain2", "Conclua fases para ganhar itens de letra; letras extras convertidas automaticamente em {0}."},

        {"TxElementTypeSelectPanel_explain", "Por favor, insira sua conta"},
        {"TxElementTypeSelectPanel_input1", "Por favor, insira sua conta de {0}"},
        {"TxElementTypeSelectPanel_input2", "Verifique sua conta de {0}"},
        {"TxElementTypeSelectPanel_Error", "Contas inconsistentes!"},
        {"TxElementTypeSelectPanel_Error2", "Conta inserida incorreta!"},
        {"CANCLE", "CANCELAR"},
        {"SUBMIT", "ENVIAR"},

        {"TxElementGameStartPanel_t1", "Quando o progresso chegar a 100%"},
        {"TxElementGameStartPanel_t2", "{0} Todos {1}"},
        {"TxElementGameStartPanel_t3", "Progresso ao Passar de Nível + {0}"},

        {"Special_Diamond__unit", "JA=="},
        {"CHT", "U2FjYXI="},
        {"CH", "c2FjYXI="},
        {"WD", "UmV0aXJhcg=="},
        {"wd", "cmV0aXJhcg=="},
        {"Wh", "UmV0aXJhZGE="},
        {"wh", "cmV0aXJhZGE="},
        {"pp", "cGF5cGFs"},
        
    };
}