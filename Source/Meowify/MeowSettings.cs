using System;
using System.Collections.Generic;
using Verse;

namespace Meowify
{
    /// <summary>游戏内设置（Options → Mod 设置）。默认值对齐「QQ喵喵助手」的喵语风格。</summary>
    public class MeowSettings : ModSettings
    {
        public bool SentenceEnd = true;
        public bool PronounSwap = true;
        public bool Kaomoji = true;
        public int KaomojiMinChars = 10;
        public string KaomojiListRaw =
            "（=^ω^=）\n（≧▽≦）\n（｡･ω･｡）\n（＾ω＾）\n（´▽`）";

        public override void ExposeData()
        {
            Scribe_Values.Look(ref SentenceEnd, "sentenceEnd", true);
            Scribe_Values.Look(ref PronounSwap, "pronounSwap", true);
            Scribe_Values.Look(ref Kaomoji, "kaomoji", true);
            Scribe_Values.Look(ref KaomojiMinChars, "kaomojiMinChars", 10);
            Scribe_Values.Look(ref KaomojiListRaw, "kaomojiList", null);
            if (KaomojiListRaw == null)
            {
                KaomojiListRaw = "（=^ω^=）\n（≧▽≦）\n（｡･ω･｡）\n（＾ω＾）\n（´▽`）";
            }
        }

        public void ResetToDefaults()
        {
            SentenceEnd = true;
            PronounSwap = true;
            Kaomoji = true;
            KaomojiMinChars = 10;
            KaomojiListRaw = "（=^ω^=）\n（≧▽≦）\n（｡･ω･｡）\n（＾ω＾）\n（´▽`）";
        }

        public MeowOptions ToOptions()
        {
            return new MeowOptions
            {
                SentenceEnd = SentenceEnd,
                PronounSwap = PronounSwap,
                Kaomoji = Kaomoji,
                KaomojiMinChars = KaomojiMinChars,
                KaomojiList = ParseList(KaomojiListRaw)
            };
        }

        public static string[] ParseList(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return new string[0];
            }
            List<string> result = new List<string>();
            foreach (string line in raw.Split('\n'))
            {
                string item = line.Trim();
                if (item.Length > 0 && !result.Contains(item))
                {
                    result.Add(item);
                }
            }
            return result.ToArray();
        }
    }
}
