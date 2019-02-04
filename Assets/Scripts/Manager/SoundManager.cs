using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : Singleton<SoundManager>
{
    [System.Serializable]
    private class SoundResource
    {
        public AudioClip sound = null;
        public Define.SoundType type = 0;
    }

    private struct SoundPack
    {
        public Define.SoundType type;
        public AudioSource source;
        public float fadingVolume;

        public bool isPause { get; private set; }

        public SoundPack(Define.SoundType type, AudioSource source)
        {
            this.type = type;
            this.source = source;

            fadingVolume = 0f;
            isPause = false;
        }

        public void SetPause(bool bPause) { isPause = bPause; }
    }

    [SerializeField] private List<SoundResource> soundResourceList;


    private Dictionary<Define.SoundType, List<SoundPack>> playingAudio = new Dictionary<Define.SoundType, List<SoundPack>>();
    private Dictionary<Define.SoundType, SoundPack> playingBGM = new Dictionary<Define.SoundType, SoundPack>();

    private Stack<AudioSource> audioSourceStack = new Stack<AudioSource>();
    private Dictionary<Define.SoundType, AudioClip> soundStore = new Dictionary<Define.SoundType, AudioClip>();

    public float soundVolume { get; private set; }
    public float bgmVolume { get; private set; }

    private readonly int soundTypeCount = System.Enum.GetValues(typeof(Define.SoundType)).Length;

    protected override void Awake()
    {
        if (instance == null)
        {
            instance = this;
            instance.Init();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected override void Init()
    {
        base.Init();

        if (soundResourceList != null)
        {
            for (int i = 0, max = soundResourceList.Count; i < max; i++)
            {
                if (!soundStore.ContainsKey(soundResourceList[i].type))
                    soundStore.Add(soundResourceList[i].type, soundResourceList[i].sound);
            }
        }
    }

    protected override void OnEnable()
    {
        SetSoundVolume(soundVolume);
        SetBGMVolume(bgmVolume);
    }

    public void SetSoundVolume(float volume)
    {
        if (soundVolume == volume)
            return;

        soundVolume = volume;

        for (int type = 0; type < soundTypeCount; type++)
        {
            Define.SoundType soundType = (Define.SoundType)type;

            if (!playingAudio.ContainsKey(soundType))
                continue;

            for (int i = 0, max = playingAudio[soundType].Count; i < max; i++)
            {
                playingAudio[soundType][i].source.volume = volume;
            }
        }
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmVolume == volume)
            return;

        bgmVolume = volume;

        for (int type = 0; type < soundTypeCount; type++)
        {
            Define.SoundType soundType = (Define.SoundType)type;

            if (!playingBGM.ContainsKey(soundType))
                continue;

            if(playingBGM[soundType].fadingVolume == 0f)
                playingBGM[soundType].source.volume = volume;
            else
                playingBGM[soundType].source.volume = playingBGM[soundType].fadingVolume;
        }
    }

    public void PlaySound(Define.SoundType soundType)
    {
        if (soundStore.ContainsKey(soundType))
        {
            if (!playingAudio.ContainsKey(soundType))
                playingAudio.Add(soundType, new List<SoundPack>());

            AudioSource source = GetAudioSource();
            playingAudio[soundType].Add(new SoundPack(soundType, source));

            source.clip = soundStore[soundType];
            source.volume = soundVolume;
            source.loop = false;
            source.Play();
        }
    }

    public void PauseSound(Define.SoundType soundType)
    {
        if (!playingAudio.ContainsKey(soundType))
            return;

        for (int i = 0, max = playingAudio[soundType].Count; i < max; i++)
        {
            if (playingAudio[soundType][i].source.isPlaying)
                playingAudio[soundType][i].source.Pause();

            playingAudio[soundType][i].SetPause(true);
        }
    }

    public void AllPasueSound()
    {
        for (int type = 0; type < soundTypeCount; type++)
        {
            Define.SoundType soundType = (Define.SoundType)type;
            PauseSound(soundType);
        }
    }

    public void ContinueSound(Define.SoundType soundType)
    {
        if (playingAudio.ContainsKey(soundType))
        {
            for (int i = 0, max = playingAudio[soundType].Count; i < max; i++)
            {
                if(!playingAudio[soundType][i].source.isPlaying)
                    playingAudio[soundType][i].source.UnPause();

                playingAudio[soundType][i].SetPause(false);
            }
        }
    }

    public void AllContinueSound()
    {
        for (int type = 0; type < soundTypeCount; type++)
        {
            Define.SoundType soundType = (Define.SoundType)type;
            if (!playingAudio.ContainsKey(soundType))
                continue;

            ContinueSound(soundType);
        }
    }

    public void StopSound(Define.SoundType soundType)
    {
        if (playingAudio.ContainsKey(soundType))
        {
            for (int i = playingAudio[soundType].Count - 1; i >= 0; i--)
            {
                AudioSource source = playingAudio[soundType][i].source;
                playingAudio[soundType][i].source.Stop();
                playingAudio[soundType].RemoveAt(i);

                ReturnAudioSource(source);
            }
        }
    }

    public void AllStopSound()
    {
        for (int type = 0; type < soundTypeCount; type++)
        {
            Define.SoundType soundType = (Define.SoundType)type;
            if (!playingAudio.ContainsKey(soundType))
                continue;

            StopSound(soundType);

            playingAudio[soundType].Clear();
        }
    }

    public void PlayBGM(Define.SoundType soundType, bool bLoop = true)
    {
        if (soundStore.ContainsKey(soundType))
        {
            if (!playingBGM.ContainsKey(soundType))
                playingBGM.Add(soundType, new SoundPack(soundType, GetAudioSource()));
            
            if (playingBGM[soundType].source.isPlaying)
                return;

            AudioClip source = soundStore[soundType];

            playingBGM[soundType].source.clip = source;
            playingBGM[soundType].source.volume = bgmVolume;
            playingBGM[soundType].source.loop = bLoop;
            playingBGM[soundType].source.Play();
        }
    }

    public void FadeOutAndStopBGM(Define.SoundType soundType, float time = 1f)
    {
        if (playingBGM.ContainsKey(soundType))
        {
            StartCoroutine(Fade(playingBGM[soundType], time, false, ()=>
            {
                StopBGM(soundType);
            }));
        }
    }

    public void FadeInAndPlayBGM(Define.SoundType soundType, float time = 1f)
    {
        PlayBGM(soundType);
        if (playingBGM.ContainsKey(soundType))
        {
            StartCoroutine(Fade(playingBGM[soundType], time, true));
        }
    }

    private IEnumerator Fade(SoundPack soundPack, float time, bool fadeIn, System.Action callback = null)
    {
        if (bgmVolume == 0)
            yield break;

        float start = fadeIn ? 1f : 0f;
        float direction = fadeIn ? -1f : 1f;
        float invTime = 1f / time;
        while (time > 0f)
        {
            time -= Time.deltaTime;
            if (time < 0f)
                time = 0f;

            float value = start + (time * invTime * direction);
            soundPack.fadingVolume = value;
            soundPack.source.volume = value;
            yield return null;
        }

        soundPack.fadingVolume = 0f;

        if (callback != null)
            callback();
    }

	public void StopBGM(Define.SoundType soundType)
	{
        if (playingBGM.ContainsKey(soundType))
        {
            playingBGM[soundType].source.Stop();
        }
	}

    public void StopAllBGM()
    {
        for (int type = 0; type < soundTypeCount; type++)
        {
            Define.SoundType soundType = (Define.SoundType)type;
            if (!playingBGM.ContainsKey(soundType))
                continue;

            StopBGM(soundType);
        }
    }

    public void PauseBGM(Define.SoundType soundType)
    {
        if (playingBGM.ContainsKey(soundType))
        {
            if (playingBGM[soundType].source.isPlaying)
                playingBGM[soundType].source.Pause();
        }
    }

    public void PauseAllBGM()
    {
        for (int type = 0; type < soundTypeCount; type++)
        {
            Define.SoundType soundType = (Define.SoundType)type;
            if (!playingBGM.ContainsKey(soundType))
                continue;

            PauseBGM(soundType);
        }
    }

    public void ContinueBGM(Define.SoundType soundType)
    {
        if (playingBGM.ContainsKey(soundType))
        {
            if (!playingBGM[soundType].source.isPlaying)
                playingBGM[soundType].source.UnPause();
        }
    }

    public void ContinueAllBGM()
    {
        for (int type = 0; type < soundTypeCount; type++)
        {
            Define.SoundType soundType = (Define.SoundType)type;
            if (!playingBGM.ContainsKey(soundType))
                continue;

            ContinueBGM(soundType);
        }
    }

    private void Update()
	{
        for (int type = 0; type < soundTypeCount; type++)
        {
            Define.SoundType soundType = (Define.SoundType)type;
            if (playingAudio.ContainsKey(soundType))
            {
                for (int i = playingAudio[soundType].Count - 1; i >= 0; i--)
                {
                    if (!playingAudio[soundType][i].source.isPlaying && !playingAudio[soundType][i].isPause)
                    {
                        ReturnAudioSource(playingAudio[soundType][i].source);
                        playingAudio[soundType].RemoveAt(i);
                    }
                }
            }
        }
	}

	private AudioSource GetAudioSource()
	{
		AudioSource source;
		if(audioSourceStack.Count <= 0)
		{
            GameObject obj = new GameObject("AudioSource");
            obj.transform.parent = transform;
            source = obj.AddComponent<AudioSource>();
		}
		else
		{
			source = audioSourceStack.Pop();
		}
		return source;
	}

	private void ReturnAudioSource(AudioSource source)
	{
        if (source.isPlaying)
            source.Stop();

		audioSourceStack.Push(source);
	}
}
