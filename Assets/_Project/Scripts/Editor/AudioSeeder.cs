#if UNITY_EDITOR
using ChemistryGame.Core;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ChemistryGame.EditorTools
{
    /// <summary>
    /// Tạo bộ SFX procedural (sine/noise/sweep) thay vì download.
    /// Save WAV vào Audio/SFX, import vào project, bind vào AudioManager.
    /// </summary>
    public static class AudioSeeder
    {
        private const string DIR = "Assets/_Project/Audio/SFX";

        [MenuItem("ChemistryGame/Seed/Audio (Procedural SFX)")]
        public static void SeedAll()
        {
            Directory.CreateDirectory(DIR);
            Directory.CreateDirectory("Assets/_Project/Audio/Music");

            // SFX
            SaveTone("sfx_button",    DIR + "/sfx_button.wav",    0.08f, 880f, 0.4f, "click");
            SaveTone("sfx_back",      DIR + "/sfx_back.wav",      0.12f, 380f, 0.4f, "click");
            SaveTone("sfx_pour",      DIR + "/sfx_pour.wav",      0.35f, 220f, 0.35f, "water");
            SaveTone("sfx_drop",      DIR + "/sfx_drop.wav",      0.10f, 700f, 0.3f, "click");
            SaveTone("sfx_bubble",    DIR + "/sfx_bubble.wav",    0.5f,  300f, 0.3f, "bubble");
            SaveTone("sfx_burner",    DIR + "/sfx_burner.wav",    0.6f,  120f, 0.3f, "flame");
            SaveTone("sfx_filter",    DIR + "/sfx_filter.wav",    0.4f,  280f, 0.3f, "soft");
            SaveTone("sfx_indicator", DIR + "/sfx_indicator.wav", 0.15f, 540f, 0.35f, "ping");
            SaveTone("sfx_hint",      DIR + "/sfx_hint.wav",      0.25f, 1100f, 0.4f, "chime");
            SaveTone("sfx_star",      DIR + "/sfx_star.wav",      0.3f,  1320f, 0.45f, "chime");
            SaveTone("sfx_win",       DIR + "/sfx_win.wav",       0.5f,  660f, 0.5f, "fanfare");
            SaveTone("sfx_fail",      DIR + "/sfx_fail.wav",      0.4f,  220f, 0.4f, "buzz");

            // Music: simple looping tone-arpeggio
            SaveTone("music_menu",    "Assets/_Project/Audio/Music/music_menu.wav",    8f, 220f, 0.18f, "arp_calm");
            SaveTone("music_gameplay","Assets/_Project/Audio/Music/music_gameplay.wav",10f, 196f, 0.16f, "arp_focus");

            AssetDatabase.Refresh();
            Debug.Log("[AudioSeeder] All SFX & music generated.");
        }

        private static void SaveTone(string id, string path, float duration, float freq, float volume, string variant)
        {
            const int sampleRate = 22050;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float env = ComputeEnvelope(t, duration, variant);
                float v = 0f;
                switch (variant)
                {
                    case "click":
                        v = Mathf.Sin(2 * Mathf.PI * freq * t);
                        break;
                    case "water":
                    case "bubble":
                        v = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.5f
                          + (Random.value - 0.5f) * 0.5f;
                        v *= Mathf.Sin(2 * Mathf.PI * 4f * t) * 0.5f + 0.5f;
                        break;
                    case "flame":
                        v = (Random.value - 0.5f) * 1.4f;
                        v *= Mathf.Sin(2 * Mathf.PI * freq * t * (1 + Mathf.Sin(t * 8f) * 0.1f));
                        break;
                    case "soft":
                        v = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.6f
                          + Mathf.Sin(2 * Mathf.PI * freq * 1.5f * t) * 0.3f;
                        break;
                    case "ping":
                        v = Mathf.Sin(2 * Mathf.PI * freq * t)
                          + Mathf.Sin(2 * Mathf.PI * freq * 2f * t) * 0.5f;
                        break;
                    case "chime":
                        v = Mathf.Sin(2 * Mathf.PI * freq * t)
                          + Mathf.Sin(2 * Mathf.PI * freq * 1.5f * t) * 0.6f
                          + Mathf.Sin(2 * Mathf.PI * freq * 2f * t) * 0.4f;
                        break;
                    case "fanfare":
                    {
                        float f = freq * (1f + Mathf.Floor(t * 3f) * 0.25f);
                        v = Mathf.Sin(2 * Mathf.PI * f * t)
                          + Mathf.Sin(2 * Mathf.PI * f * 1.5f * t) * 0.5f;
                        break;
                    }
                    case "buzz":
                        v = ((t * freq) % 1f) < 0.5f ? 1f : -1f;
                        v *= 0.6f;
                        break;
                    case "arp_calm":
                    {
                        // Slow arpeggio pattern
                        float[] notes = { 1f, 1.189f, 1.498f, 1.682f, 1.498f, 1.189f };
                        int idx = (int)((t * 1.6f) % notes.Length);
                        float ff = freq * notes[idx];
                        v = Mathf.Sin(2 * Mathf.PI * ff * t) * 0.7f
                          + Mathf.Sin(2 * Mathf.PI * ff * 0.5f * t) * 0.3f;
                        break;
                    }
                    case "arp_focus":
                    {
                        float[] notes = { 1f, 1.189f, 1.335f, 1.498f };
                        int idx = (int)((t * 3f) % notes.Length);
                        float ff = freq * notes[idx];
                        v = Mathf.Sin(2 * Mathf.PI * ff * t) * 0.6f
                          + Mathf.Sin(2 * Mathf.PI * ff * 2f * t) * 0.2f;
                        break;
                    }
                    default:
                        v = Mathf.Sin(2 * Mathf.PI * freq * t);
                        break;
                }
                data[i] = Mathf.Clamp(v * volume * env, -1f, 1f);
            }

            WriteWav(path, data, sampleRate);
        }

        private static float ComputeEnvelope(float t, float dur, string variant)
        {
            if (variant == "arp_calm" || variant == "arp_focus") return 1f; // music: no decay
            // ADSR-like: quick attack + decay
            float attack = 0.02f;
            float decay  = Mathf.Max(0.05f, dur * 0.4f);
            if (t < attack) return t / attack;
            float remain = dur - t;
            if (remain < decay) return Mathf.Max(0f, remain / decay);
            return 1f;
        }

        private static void WriteWav(string path, float[] data, int sampleRate)
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                int byteRate = sampleRate * 2;
                int dataLen = data.Length * 2;

                w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                w.Write(36 + dataLen);
                w.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                w.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                w.Write(16);
                w.Write((short)1);          // PCM
                w.Write((short)1);          // mono
                w.Write(sampleRate);
                w.Write(byteRate);
                w.Write((short)2);
                w.Write((short)16);         // bits
                w.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                w.Write(dataLen);
                foreach (var s in data)
                {
                    short sample = (short)(Mathf.Clamp(s, -1f, 1f) * short.MaxValue);
                    w.Write(sample);
                }
                File.WriteAllBytes(path, ms.ToArray());
            }
        }

        [MenuItem("ChemistryGame/Seed/Bind Audio to AudioManager")]
        public static void BindAudioManager()
        {
            var am = Object.FindObjectOfType<AudioManager>();
            if (am == null) { Debug.LogWarning("No AudioManager in scene"); return; }

            var clipsField = typeof(AudioManager).GetField("clips", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var list = new List<AudioManager.NamedClip>();
            string[] ids = {
                "sfx_button","sfx_back","sfx_pour","sfx_drop","sfx_bubble","sfx_burner",
                "sfx_filter","sfx_indicator","sfx_hint","sfx_star","sfx_win","sfx_fail",
                "music_menu","music_gameplay"
            };
            foreach (var id in ids)
            {
                string p = id.StartsWith("music_") ? $"Assets/_Project/Audio/Music/{id}.wav"
                                                   : $"Assets/_Project/Audio/SFX/{id}.wav";
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(p);
                if (clip == null) continue;
                list.Add(new AudioManager.NamedClip { Id = id, Clip = clip, Volume = 1f });
            }
            clipsField.SetValue(am, list);
            EditorUtility.SetDirty(am);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(am.gameObject.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(am.gameObject.scene);
            Debug.Log($"[AudioSeeder] Bound {list.Count} clips.");
        }
    }
}
#endif
