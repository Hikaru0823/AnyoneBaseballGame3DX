using System.Collections;
using KanKikuchi.AudioManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeBatterUI : MonoBehaviour
{
    public Animator animator;
    [SerializeField] private GameObject[] teams;
    [SerializeField] private TMP_Text userNameText;
    [SerializeField] private RawImage userImage;
    [SerializeField] private Image progressImage;
    [SerializeField] private Image targetImage;
    private Coroutine changeBatterCoroutine;

    public void ChangeBatter(PlayerObject player, float duration)
    {
        Stop();
        changeBatterCoroutine = StartCoroutine(ChangeBatter_Routine(player, duration));
    }

    public void Stop()
    {
        if (changeBatterCoroutine != null)
        {
            StopCoroutine(changeBatterCoroutine);
            changeBatterCoroutine = null;
        }
    }

    public IEnumerator ChangeBatter_Routine(PlayerObject player, float duration)
    {
        userNameText.text = player.Nickname;
        userImage.texture = player.IsImageSelected ? GameManager.Instance.PlayerCustomImages[player.Ref] : ResourcesManager.Instance.CharacterTextureByName[player.CharacterName];
        teams[0].SetActive(player.IsWhite);
        teams[1].SetActive(!player.IsWhite);
        yield return StartCoroutine(FlashingImage(player == PlayerObject.Local, duration));
        if (GameManager.Instance.Runner.IsServer) GameManager.State.Server_SetState(GameState.EGameState.BeforeHit);
    }
    IEnumerator FlashingImage(bool isNotFlash, float duration)
    {
        targetImage.enabled = false;
        for(int i = 0; i < 20; i++)
        {
            if(isNotFlash)
            {
                targetImage.enabled = !targetImage.enabled;
                if(targetImage.enabled)
                    SEManager.Instance.Play(SEPath.HIGH_LIGHT);
            }
            progressImage.fillAmount = (i + 1) / 20f;
            yield return new WaitForSeconds(duration / 20f);
            
        }
    }
}
