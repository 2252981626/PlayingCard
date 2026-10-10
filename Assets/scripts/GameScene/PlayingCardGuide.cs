using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayingCardGuide : MonoBehaviour, IEventListener
{
    private const float IdleDelay = 10f;
    private const float MoveDuration = 1f;
    private const float FadeDuration = 0.2f;
    private const float ExtractHintDuration = 1f;
    private const float SlotOffsetY = 60f;
    private const int GuideSortingOrder = 250;

    private enum GuideType
    {
        MoveToRecycle,
        RevealHidden,
        MoveCard,
    }

    private class GuideMove
    {
        public PlayingCard source;
        public PlayingCardQueue targetQueue;
        public PlayingCardRecycle targetRecycle;
        public List<PlayingCard> cards;
        public GuideType type;
    }

    private GameScenePanel gameScenePanel;
    private Sequence visualSequence;
    private readonly List<GameObject> visualCards = new List<GameObject>();
    private Transform guideLayer;
    private float idleTimer;
    private bool isDragging;
    private bool isShowing;
    private bool isLooping;
    private bool isExtractGuideHint;
    private int extractGuideFrame = -1;
    public string guideState;

    private void Update()
    {
        if (gameScenePanel == null)
        {
            guideState = "noPanel";
            return;
        }

        bool pointerDown = Input.GetMouseButton(0) || Input.touchCount > 0;
        if (isDragging && !pointerDown)
        {
            isDragging = false;
        }

        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            if (Time.frameCount == extractGuideFrame)
            {
                idleTimer = 0f;
            }
            else
            {
                NotifyInteraction();
            }
        }

        if (isShowing)
        {
            guideState = "showing";
            return;
        }

        if (isDragging)
        {
            guideState = "dragging";
            return;
        }

        if (!gameScenePanel.isOpen)
        {
            guideState = "panelClosed";
            ResetIdle();
            return;
        }

        if (GameScenePanel.isPause)
        {
            guideState = "gamePaused";
            ResetIdle();
            return;
        }

        if (isLooping)
        {
            guideState = "looping";
            if (!ShowHint())
            {
                ResetIdle();
            }
            return;
        }

        idleTimer += Time.deltaTime;
        guideState = "idle:" + idleTimer.ToString("0.0");
        if (idleTimer >= IdleDelay)
        {
            idleTimer = 0f;
            isLooping = true;
            if (!ShowHint())
            {
                ResetIdle();
            }
        }
    }

    private void ResetIdle()
    {
        idleTimer = 0f;
        isLooping = false;
    }

    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RegisterListener(GameEvent.StopHintAnim, this);
        }
    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.UnregisterListener(GameEvent.StopHintAnim, this);
        }
    }

    public void Init(GameScenePanel panel)
    {
        gameScenePanel = panel;
        isDragging = false;
        ResetIdle();
        ClearHint();
    }

    public void StartHintTimer()
    {
        ResetIdle();
    }

    public void Pause()
    {
        ResetIdle();
        ClearHint();
    }

    public void Resume()
    {
        ResetIdle();
    }

    public void Reset()
    {
        isDragging = false;
        ResetIdle();
        ClearHint();
    }

    public void NotifyInteraction()
    {
        if (isDragging)
        {
            return;
        }

        ClearHint();
        ResetIdle();
    }

    public void NotifyDragStart()
    {
        isDragging = true;
        ResetIdle();
        ClearHint();
    }

    public void NotifyDragEnd()
    {
        isDragging = false;
        ResetIdle();
    }

    public void ShowExtractGuide()
    {
        if (gameScenePanel == null || gameScenePanel.gameSceneItem_Extract == null)
        {
            return;
        }

        gameScenePanel.gameSceneItem_Extract.ShowGuide();
        isExtractGuideHint = true;
        extractGuideFrame = Time.frameCount;
        idleTimer = 0f;
        isLooping = true;
    }

    private void HideExtractGuide()
    {
        isExtractGuideHint = false;
        if (gameScenePanel != null && gameScenePanel.gameSceneItem_Extract != null)
        {
            gameScenePanel.gameSceneItem_Extract.HideGuide();
        }
    }

    public void OnEventTriggered(GameEvent eventType, object data = null)
    {
        if (eventType == GameEvent.StopHintAnim)
        {
            ClearHint();
        }
    }

    private bool ShowHint()
    {
        if (isDragging || gameScenePanel == null || !gameScenePanel.isOpen || GameScenePanel.isPause)
        {
            guideState = "blocked";
            return false;
        }

        GuideMove move = FindBestMove();
        if (move != null)
        {
            guideState = "move:" + move.source.GetName() + "->" + (move.targetRecycle != null ? "recycle" : move.targetQueue.name);
            ShowMoveHint(move);
            return true;
        }

        ResiduePlayingCards residue = gameScenePanel.playingCardControl.residuePlayingCards;
        if (residue.rightCards != null && residue.rightCards.Count > 0)
        {
            guideState = "draw:" + residue.rightCards.Count;
            ShowDrawHint(residue);
            return true;
        }

        if (residue.leftCards != null && residue.leftCards.Count > 0)
        {
            guideState = "recycle:" + residue.leftCards.Count;
            return ShowRecycleHint(residue);
        }

        guideState = "extract";
        return ShowExtractItemHint();
    }

    private bool ShowExtractItemHint()
    {
        if (gameScenePanel.gameSceneItem_Extract == null)
        {
            return false;
        }

        isShowing = true;
        isExtractGuideHint = true;
        gameScenePanel.gameSceneItem_Extract.ShowGuide();

        visualSequence = DOTween.Sequence()
            .AppendInterval(ExtractHintDuration)
            .AppendCallback(FinishHint)
            .SetTarget(this);

        return true;
    }

    private GuideMove FindBestMove()
    {
        List<GuideMove> candidates = GetCandidates();
        GuideMove recycleMove = null;
        GuideMove revealMove = null;
        GuideMove normalMove = null;

        foreach (var candidate in candidates)
        {
            if (candidate.type == GuideType.MoveToRecycle && recycleMove == null)
            {
                recycleMove = candidate;
            }
            else if (candidate.type == GuideType.RevealHidden && revealMove == null)
            {
                revealMove = candidate;
            }
            else if (candidate.type == GuideType.MoveCard && normalMove == null)
            {
                normalMove = candidate;
            }
        }

        if (recycleMove != null)
        {
            return recycleMove;
        }

        if (revealMove != null)
        {
            return revealMove;
        }

        return normalMove;
    }

    private List<GuideMove> GetCandidates()
    {
        PlayingCardControl control = gameScenePanel.playingCardControl;
        List<GuideMove> candidates = new List<GuideMove>();

        foreach (var queue in control.playingCardQueues)
        {
            int count = queue.Cells.Count;
            if (count == 0)
            {
                continue;
            }

            if (queue.Cells[count - 1].frontOrBack != PlayingCard_FrontOrBack.Front)
            {
                continue;
            }

            int start = count - 1;
            while (start > 0 && IsSequenceLink(queue.Cells[start - 1], queue.Cells[start]))
            {
                start--;
            }

            List<PlayingCard> cards = new List<PlayingCard>();
            for (int i = start; i < count; i++)
            {
                cards.Add(queue.Cells[i]);
            }

            GuideMove candidate = BuildCandidate(cards, queue, start);
            if (candidate != null)
            {
                candidates.Add(candidate);
            }
        }

        PlayingCard residueTop = GetResidueTopCard(control.residuePlayingCards);
        if (residueTop != null)
        {
            List<PlayingCard> cards = new List<PlayingCard> { residueTop };
            GuideMove candidate = BuildCandidate(cards, null, -1);
            if (candidate != null)
            {
                candidates.Add(candidate);
            }
        }

        return candidates;
    }

    private GuideMove BuildCandidate(List<PlayingCard> cards, PlayingCardQueue sourceQueue, int start)
    {
        PlayingCard source = cards[0];

        if (cards.Count == 1)
        {
            PlayingCardRecycle recycle = FindRecycleTarget(source);
            if (recycle != null)
            {
                return new GuideMove
                {
                    source = source,
                    targetRecycle = recycle,
                    cards = cards,
                    type = GuideType.MoveToRecycle,
                };
            }
        }

        PlayingCardQueue targetQueue = FindQueueTarget(source, sourceQueue);
        if (targetQueue == null)
        {
            return null;
        }

        bool revealsHidden = start > 0 && sourceQueue.Cells[start - 1].frontOrBack == PlayingCard_FrontOrBack.Back;

        if (sourceQueue != null && start == 0 && targetQueue.Cells.Count == 0)
        {
            return null;
        }

        return new GuideMove
        {
            source = source,
            targetQueue = targetQueue,
            cards = cards,
            type = revealsHidden ? GuideType.RevealHidden : GuideType.MoveCard,
        };
    }

    private PlayingCard GetResidueTopCard(ResiduePlayingCards residue)
    {
        if (residue == null || residue.leftCards == null || residue.leftCards.Count == 0)
        {
            return null;
        }

        PlayingCard card = residue.leftCards[residue.leftCards.Count - 1];
        if (card == null || card.frontOrBack != PlayingCard_FrontOrBack.Front)
        {
            return null;
        }

        return card;
    }

    private PlayingCardRecycle FindRecycleTarget(PlayingCard source)
    {
        foreach (var recycle in gameScenePanel.playingCardControl.playingCardRecycles)
        {
            if (recycle == source.playingCardRecycle)
            {
                continue;
            }

            if (recycle.CheckRecycleRootsCondition(source))
            {
                return recycle;
            }
        }

        return null;
    }

    private PlayingCardQueue FindQueueTarget(PlayingCard source, PlayingCardQueue sourceQueue)
    {
        foreach (var queue in gameScenePanel.playingCardControl.playingCardQueues)
        {
            if (queue == sourceQueue || queue == source.playingCardQueue)
            {
                continue;
            }

            if (IsQueueMoveLegal(source, queue))
            {
                return queue;
            }
        }

        return null;
    }

    private bool IsQueueMoveLegal(PlayingCard source, PlayingCardQueue queue)
    {
        if (queue.Cells.Count == 0)
        {
            return PlayingCardControl.GetName(source.order) == "K";
        }

        PlayingCard target = queue.Cells[queue.Cells.Count - 1];
        if (IsRed(target.type) == IsRed(source.type))
        {
            return false;
        }

        return target.order == source.order + 1;
    }

    private bool IsSequenceLink(PlayingCard upper, PlayingCard lower)
    {
        if (upper.frontOrBack != PlayingCard_FrontOrBack.Front || lower.frontOrBack != PlayingCard_FrontOrBack.Front)
        {
            return false;
        }

        if (upper.order != lower.order + 1)
        {
            return false;
        }

        return IsRed(upper.type) != IsRed(lower.type);
    }

    private bool IsRed(PlayingCardType type)
    {
        return type == PlayingCardType.hearts || type == PlayingCardType.diamonds;
    }

    private void ShowMoveHint(GuideMove move)
    {
        ClearHint();
        isShowing = true;

        if (move.targetRecycle != null)
        {
            Vector3 recycleDestination = move.targetRecycle.recycleRoot.position;
            for (int i = 0; i < move.cards.Count; i++)
            {
                CreateMovingCard(move.cards[i].cardImg.sprite, move.cards[i].transform.position, recycleDestination);
            }
        }
        else
        {
            int baseSlot = move.targetQueue.Cells.Count;
            for (int i = 0; i < move.cards.Count; i++)
            {
                Vector3 destination = GetQueueSlotWorldPosition(move.targetQueue, baseSlot + i);
                CreateMovingCard(move.cards[i].cardImg.sprite, move.cards[i].transform.position, destination);
            }
        }

        visualSequence = DOTween.Sequence()
            .AppendInterval(MoveDuration + FadeDuration)
            .AppendCallback(FinishHint)
            .SetTarget(this);
    }

    private void ShowDrawHint(ResiduePlayingCards residue)
    {
        ClearHint();
        isShowing = true;

        PlayingCard source = residue.rightCards[residue.rightCards.Count - 1];
        CreateMovingCard(source.backImg.sprite, source.transform.position, residue.point1.position);

        visualSequence = DOTween.Sequence()
            .AppendInterval(MoveDuration + FadeDuration)
            .AppendCallback(FinishHint)
            .SetTarget(this);
    }

    private bool ShowRecycleHint(ResiduePlayingCards residue)
    {
        ClearHint();

        Transform border = residue.highlightBorder;
        if (border == null)
        {
            return false;
        }

        CanvasGroup canvasGroup = border.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            return false;
        }

        isShowing = true;
        EnsureTopCanvas(border.gameObject);
        border.gameObject.SetActive(true);
        canvasGroup.alpha = 0f;

        visualSequence = DOTween.Sequence()
            .Append(canvasGroup.DOFade(1f, 0.5f))
            .Append(canvasGroup.DOFade(0f, 0.5f))
            .Append(canvasGroup.DOFade(1f, 0.5f))
            .Append(canvasGroup.DOFade(0f, 0.5f))
            .AppendCallback(() =>
            {
                canvasGroup.alpha = 1f;
                border.gameObject.SetActive(false);
                FinishHint();
            })
            .SetTarget(this);

        return true;
    }

    private Vector3 GetQueueSlotWorldPosition(PlayingCardQueue queue, int slot)
    {
        Vector3 baseLocal = queue.cellTrans.InverseTransformPoint(queue.basePlayingCardBg.transform.position);
        Vector3 slotLocal = baseLocal + new Vector3(0f, -SlotOffsetY * slot, 0f);
        return queue.cellTrans.TransformPoint(slotLocal);
    }

    private void CreateMovingCard(Sprite sprite, Vector3 source, Vector3 destination)
    {
        GameObject prefab = gameScenePanel.playingCardControl.playingCardBunshinPrefab;
        if (prefab == null)
        {
            return;
        }

        GameObject clone = Instantiate(prefab, GetGuideLayer());
        clone.name = "PlayingCardBunshin";
        clone.transform.SetAsLastSibling();
        clone.transform.position = source;
        clone.transform.localScale = Vector3.one;

        Image cardImage = clone.GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.sprite = sprite;
            cardImage.raycastTarget = false;
        }

        CanvasGroup canvasGroup = clone.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = clone.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Transform border = clone.transform.Find("HighlightBorder");
        if (border != null)
        {
            border.gameObject.SetActive(true);
            Image borderImage = border.GetComponent<Image>();
            if (borderImage != null)
            {
                borderImage.raycastTarget = false;
            }
        }

        visualCards.Add(clone);
        clone.transform.DOMove(destination, MoveDuration).SetEase(Ease.InOutQuad).SetTarget(this);
        canvasGroup.DOFade(0f, FadeDuration).SetDelay(MoveDuration).SetTarget(this);
    }

    private Transform GetGuideLayer()
    {
        if (guideLayer != null)
        {
            return guideLayer;
        }

        GameObject layer = new GameObject("GuideLayer", typeof(RectTransform));
        RectTransform rect = layer.GetComponent<RectTransform>();
        rect.SetParent(gameScenePanel.root, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;

        Canvas canvas = layer.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = GuideSortingOrder;

        guideLayer = layer.transform;
        return guideLayer;
    }

    private void EnsureTopCanvas(GameObject target)
    {
        Canvas canvas = target.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = target.AddComponent<Canvas>();
        }

        canvas.overrideSorting = true;
        canvas.sortingOrder = GuideSortingOrder;
    }

    private void FinishHint()
    {
        ClearVisuals();
        idleTimer = 0f;

        if (isLooping && !ShowHint())
        {
            isLooping = false;
        }
    }

    private void ClearHint()
    {
        visualSequence?.Kill();
        visualSequence = null;
        ClearVisuals();

        if (isExtractGuideHint)
        {
            HideExtractGuide();
        }
    }

    private void ClearVisuals()
    {
        foreach (var card in visualCards)
        {
            if (card != null)
            {
                card.transform.DOKill();
                CanvasGroup cardCanvasGroup = card.GetComponent<CanvasGroup>();
                if (cardCanvasGroup != null)
                {
                    cardCanvasGroup.DOKill();
                }

                Destroy(card);
            }
        }
        visualCards.Clear();

        if (gameScenePanel != null && gameScenePanel.playingCardControl != null)
        {
            ResiduePlayingCards residue = gameScenePanel.playingCardControl.residuePlayingCards;
            if (residue != null && residue.highlightBorder != null)
            {
                Transform border = residue.highlightBorder;
                CanvasGroup canvasGroup = border.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.DOKill();
                    canvasGroup.alpha = 1f;
                }

                border.gameObject.SetActive(false);
            }
        }

        isShowing = false;
    }
}
