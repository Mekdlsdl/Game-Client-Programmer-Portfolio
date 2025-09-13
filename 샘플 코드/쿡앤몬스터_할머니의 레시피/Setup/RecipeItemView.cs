using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeItemView : MonoBehaviour
{
    [SerializeField] private Sprite[] _grade;

    [SerializeField] private ButtonInfo _buttonInfo;
    [SerializeField] private Image _background;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text nameText;


    public void Bind(RecipeData data, bool isUnlocked)
    {
        if (_background != null)
        {
            _background.sprite = _grade[(int)data.Grade];

            if (isUnlocked)
            {
                Color backColor = _background.color;
                backColor.a = 1f;
                _background.color = backColor;
            }
        }

        if (_icon != null)
        {
            _icon.sprite = Utils.LoadIconSprite(data.Icon);

            if (isUnlocked)
            {
                Color iconColor = _icon.color;
                iconColor.a = 1f;
                _icon.color = iconColor;
            }
        }

        if (nameText != null) nameText.text = data.DisplayName;

        if (_buttonInfo != null)
        {
            _buttonInfo.Id = data.key;
            _buttonInfo.Count = isUnlocked ? 1 : 0;
        }
    }
}
