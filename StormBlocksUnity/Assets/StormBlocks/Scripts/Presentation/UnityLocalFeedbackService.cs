using System;
using System.Collections.Generic;
using StormBlocks.Services;
using UnityEngine;

namespace StormBlocks.Presentation
{
    [DisallowMultipleComponent]
    public sealed class UnityLocalFeedbackService : MonoBehaviour, IAudioService, IHapticsService
    {
        private readonly Dictionary<AudioEventId, AudioClip> _clips = new Dictionary<AudioEventId, AudioClip>();
        private AudioSource _effectsSource;
        private AudioSource _musicSource;
        private float _nearDeathIntensity;

        public bool Enabled { get; set; } = true;
        public bool MusicEnabled { get; set; } = true;
        public bool EffectsEnabled { get; set; } = true;
        public float MasterVolume { get; set; } = 0.85f;

        private void Awake()
        {
            EnsureSources();
            BuildClips();
            UpdateMusic();
        }

        private void Update()
        {
            if (_musicSource == null)
            {
                return;
            }

            _musicSource.volume = MusicEnabled ? Mathf.Lerp(0.025f, 0.11f, _nearDeathIntensity) * MasterVolume : 0f;
            _musicSource.pitch = Mathf.Lerp(0.92f, 1.08f, _nearDeathIntensity);
        }

        public void Configure(PlayerSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            MusicEnabled = settings.MusicEnabled;
            EffectsEnabled = settings.EffectsEnabled;
            Enabled = settings.HapticsEnabled;
            MasterVolume = settings.MasterVolume;
            EnsureSources();
            UpdateMusic();
        }

        public void Play(AudioEventId eventId)
        {
            EnsureSources();
            if (!EffectsEnabled || MasterVolume <= 0.001f)
            {
                return;
            }

            if (!_clips.TryGetValue(eventId, out var clip) || clip == null)
            {
                return;
            }

            float volume = eventId == AudioEventId.InvalidPlacement ? 0.32f : 0.58f;
            _effectsSource.PlayOneShot(clip, volume * MasterVolume);
        }

        public void Play(HapticEventId eventId)
        {
            if (!Enabled)
            {
                return;
            }

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (eventId == HapticEventId.HeavyClear ||
                eventId == HapticEventId.SuccessBurst ||
                eventId == HapticEventId.LongNearDeathWarning ||
                eventId == HapticEventId.WarningPulse)
            {
                Handheld.Vibrate();
            }
#endif
        }

        public void SetNearDeathIntensity(float intensity)
        {
            _nearDeathIntensity = Mathf.Clamp01(intensity);
        }

        private void EnsureSources()
        {
            if (_effectsSource == null)
            {
                _effectsSource = gameObject.AddComponent<AudioSource>();
                _effectsSource.playOnAwake = false;
                _effectsSource.spatialBlend = 0f;
            }

            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
                _musicSource.playOnAwake = false;
                _musicSource.loop = true;
                _musicSource.spatialBlend = 0f;
                _musicSource.clip = CreateToneClip("StormBlocks Warm Low Loop", 146.8f, 1.2f, 0.18f, 0.18f);
            }
        }

        private void UpdateMusic()
        {
            if (_musicSource == null)
            {
                return;
            }

            if (MusicEnabled && !_musicSource.isPlaying)
            {
                _musicSource.Play();
            }
            else if (!MusicEnabled && _musicSource.isPlaying)
            {
                _musicSource.Stop();
            }
        }

        private void BuildClips()
        {
            if (_clips.Count > 0)
            {
                return;
            }

            _clips[AudioEventId.UiTap] = CreateToneClip("SB UI Tap", 520f, 0.055f, 0.05f, 0.65f);
            _clips[AudioEventId.PiecePickup] = CreateToneClip("SB Piece Pickup", 420f, 0.085f, 0.04f, 0.7f);
            _clips[AudioEventId.PieceHover] = CreateToneClip("SB Piece Hover", 560f, 0.050f, 0.05f, 0.55f);
            _clips[AudioEventId.ValidPlacement] = CreateToneClip("SB Placement", 620f, 0.075f, 0.05f, 0.75f);
            _clips[AudioEventId.InvalidPlacement] = CreateToneClip("SB Invalid", 150f, 0.11f, 0.08f, 0.55f);
            _clips[AudioEventId.LineClear] = CreateToneClip("SB Line Clear", 740f, 0.16f, 0.08f, 0.8f);
            _clips[AudioEventId.Combo] = CreateToneClip("SB Combo", 980f, 0.22f, 0.06f, 0.85f);
            _clips[AudioEventId.SurvivorRescued] = CreateToneClip("SB Survivor", 880f, 0.18f, 0.05f, 0.7f);
            _clips[AudioEventId.StormWarning] = CreateToneClip("SB Storm Warning", 196f, 0.30f, 0.22f, 0.62f);
            _clips[AudioEventId.StormSpread] = CreateToneClip("SB Storm Spread", 112f, 0.22f, 0.18f, 0.55f);
            _clips[AudioEventId.StormPushback] = CreateToneClip("SB Pushback", 1040f, 0.24f, 0.05f, 0.9f);
            _clips[AudioEventId.ClutchSave] = CreateToneClip("SB Clutch", 1220f, 0.32f, 0.04f, 0.95f);
            _clips[AudioEventId.NearDeathLoop] = CreateToneClip("SB Near Death Pulse", 174f, 0.28f, 0.18f, 0.62f);
            _clips[AudioEventId.GameOver] = CreateToneClip("SB Game Over", 98f, 0.42f, 0.25f, 0.62f);
            _clips[AudioEventId.ResultsCelebration] = CreateToneClip("SB Results", 660f, 0.26f, 0.08f, 0.8f);
            _clips[AudioEventId.CosmeticUnlock] = CreateToneClip("SB Unlock", 1320f, 0.30f, 0.06f, 0.95f);
            _clips[AudioEventId.DailyStormStart] = CreateToneClip("SB Daily Start", 520f, 0.24f, 0.08f, 0.82f);
            _clips[AudioEventId.DailyStormEnd] = CreateToneClip("SB Daily End", 760f, 0.26f, 0.08f, 0.82f);
        }

        private static AudioClip CreateToneClip(string clipName, float frequency, float durationSeconds, float decay, float harmonic)
        {
            const int sampleRate = 24000;
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * durationSeconds));
            var samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Exp(-decay * t * 10f);
                float attack = Mathf.Clamp01(t / 0.018f);
                float primary = Mathf.Sin(Mathf.PI * 2f * frequency * t);
                float overtone = Mathf.Sin(Mathf.PI * 2f * frequency * 1.5f * t) * harmonic;
                samples[i] = (primary + overtone) * 0.34f * envelope * attack;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
