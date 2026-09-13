using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public enum SFX
    {
        ValidClick,
        InvalidClick,
        LevelUp,
        LevelSelected,
        HpUp,
        Hit,
        Shoot1,
        Shoot2,
        Shoot3,
        ChainChord,
        HeavyShoot,
        Stun,
        GameOver
    }

    [Header("BGM")]
    [SerializeField] private AudioClip _bgmClip;
    [SerializeField] private AudioClip _mainmenuClip;
    [SerializeField] private float _bgmVolume;
    private AudioSource _bgmPlayer;

    [Header("SFX")]
    [SerializeField] private AudioClip[] _sfxClip;
    [SerializeField] private float _sfxVolume;
    [SerializeField] private int _channels;
    private AudioSource[] _sfxPlayer;

    private int _channelIndex;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        _bgmPlayer = bgmObject.AddComponent<AudioSource>();
        _bgmPlayer.playOnAwake = false;
        _bgmPlayer.loop = true;
        _bgmPlayer.volume = _bgmVolume;

        GameObject sfxObject = new GameObject("SfxObject");
        sfxObject.transform.parent = transform;
        _sfxPlayer = new AudioSource[_channels];

        for (int i = 0; i < _channels; i++)
        {
            _sfxPlayer[i] = sfxObject.AddComponent<AudioSource>();
            _sfxPlayer[i].playOnAwake = false;
            _sfxPlayer[i].volume = _sfxVolume;
        }
    }

    /// <summary>
    /// 메인 메뉴 BGM 재생
    /// </summary>
    public void PlayMainMenuBgm()
    {
        PlayBgmClip(_mainmenuClip);
    }

    /// <summary>
    /// 인게임 전투 BGM 재생
    /// </summary>
    public void PlayInGameBgm()
    {
        PlayBgmClip(_bgmClip);
    }

    private void PlayBgmClip(AudioClip targetClip)
    {
        if (targetClip == null || _bgmPlayer == null) return;
        if (_bgmPlayer.clip == targetClip && _bgmPlayer.isPlaying) return;

        _bgmPlayer.Stop();
        _bgmPlayer.clip = targetClip;
        _bgmPlayer.Play();
    }

    public void StopBgm()
    {
        if (_bgmPlayer != null)
        {
            _bgmPlayer.Stop();
        }
    }

    /// <summary>
    /// SFX 열거형 기반 효과음 재생
    /// </summary>
    public void PlaySfx(SFX sfxType)
    {
        int index = (int)sfxType;
        if (_sfxClip == null || index < 0 || index >= _sfxClip.Length)
        {
            Debug.LogWarning($"[AudioManager] SFX 클립이 설정되지 않았습니다: {sfxType} (Index: {index})");
            return;
        }

        PlaySfxClip(_sfxClip[index]);
    }

    /// <summary>
    /// 인덱스 기반 효과음 재생 (하위 호환)
    /// </summary>
    public void PlaySfx(int sfxIndex)
    {
        if (_sfxClip == null || sfxIndex < 0 || sfxIndex >= _sfxClip.Length) return;
        PlaySfxClip(_sfxClip[sfxIndex]);
    }

    /// <summary>
    /// 다채널 풀링 기반 단일 클립 재생
    /// </summary>
    public void PlaySfxClip(AudioClip clip)
    {
        if (clip == null || _sfxPlayer == null || _sfxPlayer.Length == 0) return;

        // 1. 쉬고 있는 채널 순환 탐색
        for (int i = 0; i < _sfxPlayer.Length; i++)
        {
            int loopIndex = (i + _channelIndex) % _sfxPlayer.Length;

            if (_sfxPlayer[loopIndex].isPlaying) continue;

            _channelIndex = (loopIndex + 1) % _sfxPlayer.Length;
            _sfxPlayer[loopIndex].clip = clip;
            _sfxPlayer[loopIndex].Play();
            return;
        }

        // 2. 대규모 군집/폭발로 모든 채널이 사용 중일 때 덮어쓰기
        _channelIndex = (_channelIndex + 1) % _sfxPlayer.Length;
        _sfxPlayer[_channelIndex].clip = clip;
        _sfxPlayer[_channelIndex].Play();
    }

}