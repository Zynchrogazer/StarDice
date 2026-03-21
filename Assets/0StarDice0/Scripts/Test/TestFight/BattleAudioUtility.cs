using UnityEngine;

public static class BattleAudioUtility
{
    public static void PlaySfx(MonoBehaviour owner, AudioClip[] sfxList, int index)
    {
        if (owner == null || sfxList == null || index < 0 || index >= sfxList.Length) return;

        AudioClip clip = sfxList[index];
        if (clip == null) return;

        AudioSource source = owner.GetComponent<AudioSource>();
        if (source == null)
        {
            source = owner.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
        }

        source.PlayOneShot(clip);
    }
}
