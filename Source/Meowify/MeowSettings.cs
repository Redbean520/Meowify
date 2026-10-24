using System;
using System.Collections.Generic;
using Verse;

namespace Meowify
{
    /// <summary>
    /// 模组设置（Scribe 持久化）。列表类设置用「每行一个」的原始文本存，读回时解析成数组。
    /// </summary>
    public sealed class MeowSettings : ModSettings
    {
        public const string DefaultSelfPronoun = "本喵";
        public const string DefaultOtherPronoun = "主人";
        public const string DefaultEndWords = "喵";
        public const string DefaultKaomojiList =
            "（=^ω^=）\n（≧▽≦）\n（｡･ω･｡）\n（＾ω＾）\n（´▽`）";

        // 句尾
        public bool SentenceEnd = true;
        public string EndWordsRaw = DefaultEndWords;
        public int MinChars = 2;

        // 人称
        public bool PronounSwap = true;
        public string SelfPronoun = DefaultSelfPronoun;
        public string OtherPronoun = DefaultOtherPronoun;

        // 颜文字
        public bool Kaomoji = true;
        public int KaomojiMinChars = 10;
        public int KaomojiChance = 100;
        public string KaomojiListRaw = DefaultKaomojiList;

        // 高级
        public string ExtraProtectedWordsRaw = string.Empty;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref SentenceEnd, "sentenceEnd", true);
            Scribe_Values.Look(ref EndWordsRaw, "endWords", DefaultEndWords);
            Scribe_Values.Look(ref MinChars, "minChars", 2);
            Scribe_Values.Look(ref PronounSwap, "pronounSwap", true);
            Scribe_Values.Look(ref SelfPronoun, "selfPronoun", DefaultSelfPronoun);
            Scribe_Values.Look(ref OtherPronoun, "otherPronoun", DefaultOtherPronoun);
            Scribe_Values.Look(ref Kaomoji, "kaomoji", true);
            Scribe_Values.Look(ref KaomojiMinChars, "kaomojiMinChars", 10);
            Scribe_Values.Look(ref KaomojiChance, "kaomojiChance", 100);
            Scribe_Values.Look(ref KaomojiListRaw, "kaomojiList", DefaultKaomojiList);
            Scribe_Values.Look(ref ExtraProtectedWordsRaw, "extraProtectedWords", string.Empty);
        }

        public void ResetToDefaults()
        {
            SentenceEnd = true;
            EndWordsRaw = DefaultEndWords;
            MinChars = 2;
            PronounSwap = true;
            SelfPronoun = DefaultSelfPronoun;
            OtherPronoun = DefaultOtherPronoun;
            Kaomoji = true;
            KaomojiMinChars = 10;
            KaomojiChance = 100;
            KaomojiListRaw = DefaultKaomojiList;
            ExtraProtectedWordsRaw = string.Empty;
        }

        public MeowOptions ToOptions()
        {
            MeowOptions options = new MeowOptions();
            options.SentenceEnd = SentenceEnd;
            options.EndWords = ParseList(EndWordsRaw);
            options.MinChars = MinChars < 1 ? 1 : MinChars;
            options.PronounSwap = PronounSwap;
            options.SelfPronoun = SelfPronoun == null ? string.Empty : SelfPronoun.Trim();
            options.OtherPronoun = OtherPronoun == null ? string.Empty : OtherPronoun.Trim();
            options.Kaomoji = Kaomoji;
            options.KaomojiMinChars = KaomojiMinChars < 1 ? 1 : KaomojiMinChars;
            options.KaomojiChance = Clamp(KaomojiChance, 0, 100);
            options.KaomojiList = ParseList(KaomojiListRaw);
            options.ExtraProtectedWords = ParseList(ExtraProtectedWordsRaw);
            return options;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }

        /// <summary>把「每行一个」的文本解析成去空白、去重、保持顺序的数组。</summary>
        public static string[] ParseList(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return new string[0];
            }
            string[] parts = raw.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> result = new List<string>(parts.Length);
            for (int i = 0; i < parts.Length; i++)
            {
                string item = parts[i].Trim();
                if (item.Length > 0 && !result.Contains(item))
                {
                    result.Add(item);
                }
            }
            return result.ToArray();
        }
    }
}
