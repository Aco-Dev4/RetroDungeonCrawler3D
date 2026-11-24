using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class CardTypeBadge : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private CardTypeBadges _row;
    private string _label;

    internal void Setup(CardTypeBadges row, string label)
    {
        _row = row;
        _label = label;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _row?.ShowLabel(_label, ((RectTransform)transform).anchoredPosition.y);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _row?.HideLabel();
    }

    private void OnDisable()
    {
        _row?.HideLabel();
    }
}

internal sealed class CardTypeBadges
{
    private const float BadgeSize = 15f;
    private const float BadgeSpacing = 4f;
    private readonly RectTransform _row;
    private readonly TMP_Text _fontSource;
    private readonly List<CardTypeBadge> _badges = new();
    private Image _tooltip;
    private TextMeshProUGUI _tooltipLabel;

    private CardTypeBadges(RectTransform parent, TMP_Text fontSource)
    {
        _fontSource = fontSource;
        var rowObject = new GameObject("Card Type Badges", typeof(RectTransform), typeof(LayoutElement));
        rowObject.layer = parent.gameObject.layer;
        _row = (RectTransform)rowObject.transform;
        _row.SetParent(parent, false);
        _row.anchorMin = _row.anchorMax = new Vector2(1f, 0.5f);
        _row.pivot = new Vector2(1f, 0.5f);
        _row.anchoredPosition = Vector2.left * 9f;
        rowObject.GetComponent<LayoutElement>().ignoreLayout = true;
    }

    internal static void Apply(ref CardTypeBadges row, RectTransform parent, CardTypeCatalog catalog,
        CardType types, TMP_Text fontSource)
    {
        if (row == null && types != CardType.None && catalog != null)
            row = new CardTypeBadges(parent, fontSource);

        row?.SetTypes(types, catalog);
    }

    private void SetTypes(CardType types, CardTypeCatalog catalog)
    {
        HideLabel();
        int count = 0;
        if (catalog != null && catalog.entries != null)
        {
            foreach (CardTypeEntry entry in catalog.entries)
            {
                if (entry.type == CardType.None || (types & entry.type) == 0) continue;

                CardTypeBadge badge = count < _badges.Count ? _badges[count] : CreateBadge();
                badge.gameObject.name = entry.label + " Badge";
                badge.Setup(this, entry.label);
                var graphic = badge.GetComponent<Image>();
                graphic.sprite = entry.icon;
                graphic.preserveAspect = true;
                graphic.rectTransform.anchoredPosition = Vector2.down * count * (BadgeSize + BadgeSpacing);
                badge.gameObject.SetActive(true);
                count++;
            }
        }

        for (int i = count; i < _badges.Count; i++)
            _badges[i].gameObject.SetActive(false);

        _row.sizeDelta = new Vector2(BadgeSize, Mathf.Max(0f, count * (BadgeSize + BadgeSpacing) - BadgeSpacing));
        _row.gameObject.SetActive(count > 0);
    }

    private CardTypeBadge CreateBadge()
    {
        var badgeObject = new GameObject("Type Badge", typeof(RectTransform), typeof(Image), typeof(Outline));
        badgeObject.layer = _row.gameObject.layer;
        var rect = (RectTransform)badgeObject.transform;
        rect.SetParent(_row, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
        rect.sizeDelta = Vector2.one * BadgeSize;
        var outline = badgeObject.GetComponent<Outline>();
        outline.effectColor = new Color32(22, 21, 20, 255);
        outline.effectDistance = new Vector2(1.2f, -1.2f);
        CardTypeBadge badge = badgeObject.AddComponent<CardTypeBadge>();
        _badges.Add(badge);
        return badge;
    }

    internal void ShowLabel(string label, float badgeY)
    {
        if (_tooltip == null)
            CreateTooltip();

        _tooltipLabel.text = label;
        _tooltip.rectTransform.sizeDelta = new Vector2(_tooltipLabel.preferredWidth + 12f, 22f);
        _tooltip.rectTransform.anchoredPosition = new Vector2(-BadgeSize - 4f, badgeY - BadgeSize * 0.5f);
        _tooltip.transform.SetAsLastSibling();
        _tooltip.gameObject.SetActive(true);
    }

    internal void HideLabel()
    {
        if (_tooltip != null)
            _tooltip.gameObject.SetActive(false);
    }

    private void CreateTooltip()
    {
        var surface = new GameObject("Card Type Tooltip", typeof(RectTransform), typeof(Image));
        surface.layer = _row.gameObject.layer;
        _tooltip = surface.GetComponent<Image>();
        var rect = _tooltip.rectTransform;
        rect.SetParent(_row, false);
        rect.anchorMin = rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(1f, 0.5f);
        _tooltip.color = new Color32(26, 24, 23, 244);
        _tooltip.raycastTarget = false;

        var label = new GameObject("Type Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.layer = surface.layer;
        _tooltipLabel = label.GetComponent<TextMeshProUGUI>();
        _tooltipLabel.rectTransform.SetParent(rect, false);
        _tooltipLabel.rectTransform.anchorMin = Vector2.zero;
        _tooltipLabel.rectTransform.anchorMax = Vector2.one;
        _tooltipLabel.rectTransform.sizeDelta = Vector2.zero;
        _tooltipLabel.rectTransform.anchoredPosition = Vector2.zero;
        if (_fontSource != null)
            _tooltipLabel.font = _fontSource.font;

        _tooltipLabel.fontSize = 13f;
        _tooltipLabel.color = Color.white;
        _tooltipLabel.alignment = TextAlignmentOptions.Center;
        _tooltipLabel.textWrappingMode = TextWrappingModes.NoWrap;
        _tooltipLabel.raycastTarget = false;
    }
}

