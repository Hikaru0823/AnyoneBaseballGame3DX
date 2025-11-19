using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentBatterUI : MonoBehaviour
{
    public Animator animator;
    [SerializeField] private RawImage image;
    [SerializeField] private TMP_Text nameText;

    public void SetBatter(PlayerObject player)
    {
        if (player.IsImageSelected)
        {
            if (GameManager.Instance.PlayerCustomImages.ContainsKey(player.Ref))
            {
                image.texture = GameManager.Instance.PlayerCustomImages[player.Ref];
            }
        }
        else
        {
            if (ResourcesManager.Instance.CharacterTextureByName.ContainsKey(player.CharacterName))
            {
                image.texture = ResourcesManager.Instance.CharacterTextureByName[player.CharacterName];
            }
        }
        nameText.text = player.Nickname;
        if (player == PlayerObject.Local)
        {
            nameText.color = new Color(0.6f, 0.6f, 1.0f, 1f);
        }
    }
}
