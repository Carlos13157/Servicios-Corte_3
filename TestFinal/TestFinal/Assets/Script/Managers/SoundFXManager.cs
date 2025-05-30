using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour{
    public static SoundFXManager instance;
    [SerializeField] private AudioSource soundFXSource;

    private void Awake(){
        if(instance == null){
            instance = this;
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume){
        AudioSource audioSource = Instantiate(soundFXSource, spawnTransform.position, Quaternion.identity);

        audioSource.transform.SetParent(spawnTransform);

        audioSource.clip = audioClip;

        audioSource.volume = volume;

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }

    public float PlayRandomSoundFXClip(AudioClip[] audioClips, Transform spawnTransform, float volume, bool canInterrupt, AudioSource audioSource = null){
        int randIndex = Random.Range(0, audioClips.Length);
        AudioClip selectedClip = audioClips[randIndex];

        bool isNewAudioSource = false;
        if (audioSource == null){
            audioSource = Instantiate(soundFXSource, spawnTransform.position, Quaternion.identity);
            audioSource.transform.SetParent(spawnTransform);
            isNewAudioSource = true; 
        }

        if (audioSource.isPlaying){
            if(canInterrupt){
                audioSource.Stop();
            }else{
                StartCoroutine(WaitForAudioToFinish(audioSource, selectedClip, volume, isNewAudioSource));
                return selectedClip.length;
            }
            
        }
        
        audioSource.clip = selectedClip;
        audioSource.volume = volume;
        audioSource.Play();

        if (isNewAudioSource){
            Destroy(audioSource.gameObject, selectedClip.length);
        }

        return selectedClip.length;
    }

    private IEnumerator WaitForAudioToFinish(AudioSource audioSource, AudioClip selectedClip, float volume, bool isNewAudioSource){
        // if (audioSource == null || selectedClip == null){
        //     yield break;
        // }
        yield return new WaitUntil(() => !audioSource.isPlaying);
        audioSource.clip = selectedClip;
        audioSource.volume = volume;
        audioSource.Play();

        if (isNewAudioSource)
        {
            Destroy(audioSource.gameObject, selectedClip.length);
        }
    }
}
