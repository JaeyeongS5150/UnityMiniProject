using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    [SerializeField] private AudioManager.SFX _clickSound = AudioManager.SFX.ValidClick;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }

    private void PlaySound()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(_clickSound);
        }
    }
}