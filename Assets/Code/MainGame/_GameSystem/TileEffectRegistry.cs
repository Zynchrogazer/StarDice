using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// เป็นคลังเก็บและจัดการ Effect ที่เป็น Scriptable Objects ทั้งหมด
/// ใช้ Singleton Pattern เพื่อให้ GameManager เรียกใช้ได้สะดวก
/// </summary>
public class TileEffectRegistry : MonoBehaviour
{
    // Singleton Instance สำหรับให้คลาสอื่นเรียกใช้
    public static TileEffectRegistry Instance { get; private set; }

    [Tooltip("ลาก Effect Scriptable Objects ทั้งหมด (เช่น TrapEffectSO, ShopEffectSO) มาใส่ที่นี่")]
    public List<TileEffectSO> registeredEffects;

    // Dictionary ที่ใช้เก็บข้อมูลตอนเล่นเกมเพื่อการค้นหาที่รวดเร็ว (Performance)
    private Dictionary<TileType, ITileEffect> effectDictionary;

    private void Awake()
    {
        // ตั้งค่า Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // เรียกเมธอดเพื่อเตรียมฐานข้อมูล Effect ให้พร้อมใช้งาน
        Initialize();
    }

    /// <summary>
    /// แปลง List ที่เราตั้งค่าใน Inspector ให้กลายเป็น Dictionary
    /// เพื่อให้การค้นหา Effect ขณะเล่นเกมทำได้รวดเร็วมาก
    /// </summary>
    private void Initialize()
    {
        effectDictionary = new Dictionary<TileType, ITileEffect>();

        foreach (var effectAsset in registeredEffects)
        {
            // ตรวจสอบว่าลาก Asset มาใส่หรือไม่ และป้องกันการลงทะเบียน Type ซ้ำซ้อน
            if (effectAsset != null && !effectDictionary.ContainsKey(effectAsset.EffectType))
            {
                effectDictionary.Add(effectAsset.EffectType, effectAsset);
            }
            else
            {
                if (effectAsset == null)
                {
                    Debug.LogWarning("[TileEffectRegistry] Found an empty slot in the registered effects list.");
                }
                else
                {
                    Debug.LogWarning($"[TileEffectRegistry] Duplicate TileType found: {effectAsset.EffectType}. Only the first one was registered.");
                }
            }
        }

        Debug.Log($"[TileEffectRegistry] Initialized with {effectDictionary.Count} unique effects.");
    }

    /// <summary>
    /// เมธอดสาธารณะสำหรับให้ GameManager มาขอ Effect ไปใช้งาน
    /// </summary>
    /// <param name="type">ประเภทของช่องที่ต้องการหา Effect</param>
    /// <returns>Object ของ Effect ที่ตรงกัน หรือ null ถ้าไม่พบ</returns>
    public ITileEffect GetEffect(TileType type)
    {
        // ใช้ TryGetValue เพื่อความปลอดภัยและประสิทธิภาพ
        effectDictionary.TryGetValue(type, out ITileEffect effect);
        return effect;
    }
}