using System;
using System.Collections;
using System.Collections.Generic;
using HurricaneVR.Framework.Core.Player;
using HurricaneVR.Framework.Core.Utils;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[RequireComponent(typeof(HVRPlayerController))]
public class HVRFootsteps : MonoBehaviour
{
    [Header("Footstep Sounds")]
    [SerializeField] private AudioClip[] _walkFootstepClips;
    [SerializeField] private AudioClip[] _runFootstepClips;
    
    [Header("Footstep Settings")] [Space]
    [SerializeField] private float _walkCooldown = 0.7f;
    [SerializeField] private float _runCooldown = 0.3f;
    [SerializeField] private float _walkThreshold = 1.4f;
    [SerializeField] private float _runThreshold = 3.4f;
    [SerializeField] private float _walkVolume = .5f;
    [SerializeField] private float _runVolume = 1f;
    [SerializeField] private float _walkPitch = 1f;
    [SerializeField] private float _runPitch = 1f;
    [SerializeField] private float _pitchRandomness = .1f;
    [SerializeField] private float _volumeRandomness = .2f;

    [Header("Debugging")] [Space]
    public bool isEnable = true;
    [SerializeField] private bool _debugGizmos;
    
    [Header("Events")] [Space]
    public UnityEvent OnWalkStep;
    public UnityEvent OnRunStep;
    
    private HVRPlayerController _hvrPlayerController;
    private Vector3 _groundPosition;
    private float _currentSpeed;
    private bool _isWalkReady = true;
    private bool _isRunReady = true;
    private int _lastWalkIndex;
    private int _lastRunIndex;
    private WaitForSeconds _walkWait;
    private WaitForSeconds _runWait;

    private void Awake()
    {
        CheckForEmptyAudio();
    }

    private void Start()
    {
        _hvrPlayerController = GetComponent<HVRPlayerController>();
        _walkWait = new WaitForSeconds(_walkCooldown);
        _runWait = new WaitForSeconds(_runCooldown);
    }

    private void FixedUpdate()
    {
        HandleFootsteps();
    }

    private void HandleFootsteps()
    {
        if (!isEnable) return;
        _currentSpeed = _hvrPlayerController.CharacterController.velocity.magnitude;
        if (_currentSpeed >= _runThreshold)
        {
            StartCoroutine(PlayRunFootsteps());
        }
        else if (_currentSpeed >= _walkThreshold)
        {
            StartCoroutine(PlayWalkFootsteps());
        }
    }

    private IEnumerator PlayWalkFootsteps()
    {
        if (_isWalkReady && _isRunReady)
        {
            PlayRandomSoundAtGround(ref _lastWalkIndex, _walkFootstepClips, _walkVolume, _walkPitch, OnWalkStep);
            _isWalkReady = false;
            yield return _walkWait;
            _isWalkReady = true;
        }
    }

    private IEnumerator PlayRunFootsteps()
    {
        if (_isRunReady && _isWalkReady)
        {
            PlayRandomSoundAtGround(ref _lastRunIndex, _runFootstepClips, _runVolume, _runPitch, OnRunStep);
            _isRunReady = false;
            yield return _runWait;
            _isRunReady = true;
        }
    }
    
    private void PlayRandomSoundAtGround(ref int lastClipIndex, IReadOnlyList<AudioClip> audioClips, float volume, float pitch, UnityEvent unityEvent)
    {
        _groundPosition = _hvrPlayerController.Camera.position - new Vector3(0, _hvrPlayerController.CameraHeight, 0);
        var clipIndex = Random.Range(0, audioClips.Count);
        while(clipIndex == lastClipIndex)
            clipIndex = Random.Range(0, audioClips.Count);
        var clip = audioClips[clipIndex];
        lastClipIndex = clipIndex;
        SFXPlayer.Instance?.PlaySFX(clip, _groundPosition, Random.Range(pitch-_pitchRandomness,pitch+_pitchRandomness), Random.Range(volume-_volumeRandomness,volume+_volumeRandomness));
        unityEvent.Invoke();
    }

    public void ChangeFootstepAudio(AudioClip[] walkClips, AudioClip[] runClips)
    {
        _walkFootstepClips = walkClips;
        _runFootstepClips = runClips;
    }

    private void CheckForEmptyAudio()
    {
        if (_walkFootstepClips.Length == 0 || _runFootstepClips.Length == 0)
        {
            Debug.Log("No footstep audio clips assigned, please assign some in the inspector, disabling footsteps sound");
            enabled = false;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (_debugGizmos && Application.isPlaying)
        {
           _groundPosition = _hvrPlayerController.Camera.position - new Vector3(0, _hvrPlayerController.CameraHeight, 0);
           Gizmos.color = Color.magenta;
           Gizmos.DrawWireSphere(_groundPosition, .2f); 
        }
    }
    
}