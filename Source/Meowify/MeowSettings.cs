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
        public const string DefaultCollapseSuffix = "~";
        public const string DefaultKaomojiList =
            "（=^ω^=）\n（≧▽≦）\n（｡･ω･｡）\n（＾ω＾）\n（´▽`）\n（＝ω＝）\n（ฅ´ω`ฅ）\n（´｡• ᵕ •｡`）\n（*´▽`*）\n（＾・ω・＾）";

        // 句尾
        public bool SentenceEnd = true;
        public string EndWordsRaw = DefaultEndWords;
        public int MinChars = 2;
        public int SentenceEndChance = 100;

        // 人称
        public bool PronounSwap = true;
        public string SelfPronoun = DefaultSelfPronoun;
        public string OtherPronoun = DefaultOtherPronoun;

        // 颜文字
        public bool Kaomoji = true;
        public int KaomojiMinChars = 10;
        public int KaomojiChance = 100;
        public string KaomojiListRaw = DefaultKaomojiList;

        /// <summary>按语境挑颜文字。</summary>
        public bool KaomojiContextual = true;

        // 适用范围
        public bool ChannelKeyed = true;
        public bool ChannelDefInjected = true;
        public bool ChannelGrammar = true;
        public bool ChannelDefSweep = true;
        public bool IncludeNames = false;

        // 关键词过滤
        public string IncludeKeywordsRaw = string.Empty;
        public string ExcludeKeywordsRaw = string.Empty;

        // 折叠重复句尾词
        public bool CollapseRepeated = true;
        public string CollapseSuffix = DefaultCollapseSuffix;

        // 高级
        public string ExtraProtectedWordsRaw = string.Empty;
        public string ExcludedPhrasesRaw = string.Empty;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref SentenceEnd, "sentenceEnd", true);
            Scribe_Values.Look(ref EndWordsRaw, "endWords", DefaultEndWords);
            Scribe_Values.Look(ref MinChars, "minChars", 2);
            Scribe_Values.Look(ref SentenceEndChance, "sentenceEndChance", 100);
            Scribe_Values.Look(ref PronounSwap, "pronounSwap", true);
            Scribe_Values.Look(ref SelfPronoun, "selfPronoun", DefaultSelfPronoun);
            Scribe_Values.Look(ref OtherPronoun, "otherPronoun", DefaultOtherPronoun);
            Scribe_Values.Look(ref Kaomoji, "kaomoji", true);
            Scribe_Values.Look(ref KaomojiMinChars, "kaomojiMinChars", 10);
            Scribe_Values.Look(ref KaomojiChance, "kaomojiChance", 100);
            Scribe_Values.Look(ref KaomojiListRaw, "kaomojiList", DefaultKaomojiList);
            Scribe_Values.Look(ref KaomojiContextual, "kaomojiContextual", true);
            Scribe_Values.Look(ref ChannelKeyed, "channelKeyed", true);
            Scribe_Values.Look(ref ChannelDefInjected, "channelDefInjected", true);
            Scribe_Values.Look(ref ChannelGrammar, "channelGrammar", true);
            Scribe_Values.Look(ref ChannelDefSweep, "channelDefSweep", true);
            Scribe_Values.Look(ref IncludeNames, "includeNames", false);
            Scribe_Values.Look(ref IncludeKeywordsRaw, "includeKeywords", string.Empty);
            Scribe_Values.Look(ref ExcludeKeywordsRaw, "excludeKeywords", string.Empty);
            Scribe_Values.Look(ref CollapseRepeated, "collapseRepeated", true);
            Scribe_Values.Look(ref CollapseSuffix, "collapseSuffix", DefaultCollapseSuffix);
            Scribe_Values.Look(ref ExtraProtectedWordsRaw, "extraProtectedWords", string.Empty);
            Scribe_Values.Look(ref ExcludedPhrasesRaw, "excludedPhrases", string.Empty);
        }

        public void ResetToDefaults()
        {
            SentenceEnd = true;
            EndWordsRaw = DefaultEndWords;
            MinChars = 2;
            SentenceEndChance = 100;
            PronounSwap = true;
            SelfPronoun = DefaultSelfPronoun;
            OtherPronoun = DefaultOtherPronoun;
            Kaomoji = true;
            KaomojiMinChars = 10;
            KaomojiChance = 100;
            KaomojiListRaw = DefaultKaomojiList;
            KaomojiContextual = true;
            ChannelKeyed = true;
            ChannelDefInjected = true;
            ChannelGrammar = true;
            ChannelDefSweep = true;
            IncludeNames = false;
            IncludeKeywordsRaw = string.Empty;
            ExcludeKeywordsRaw = string.Empty;
            CollapseRepeated = true;
            CollapseSuffix = DefaultCollapseSuffix;
            ExtraProtectedWordsRaw = string.Empty;
            ExcludedPhrasesRaw = string.Empty;
        }

        public MeowOptions ToOptions()
        {
            MeowOptions options = new MeowOptions();
            options.SentenceEnd = SentenceEnd;
            options.EndWords = ParseList(EndWordsRaw);
            options.MinChars = MinChars < 1 ? 1 : MinChars;
            options.SentenceEndChance = Clamp(SentenceEndChance, 0, 100);
            options.PronounSwap = PronounSwap;
            options.SelfPronoun = SelfPronoun == null ? string.Empty : SelfPronoun.Trim();
            options.OtherPronoun = OtherPronoun == null ? string.Empty : OtherPronoun.Trim();
            options.Kaomoji = Kaomoji;
            options.KaomojiMinChars = KaomojiMinChars < 1 ? 1 : KaomojiMinChars;
            options.KaomojiChance = Clamp(KaomojiChance, 0, 100);
            options.KaomojiList = ParseList(KaomojiListRaw);
            options.KaomojiContextual = KaomojiContextual;
            options.ChannelKeyed = ChannelKeyed;
            options.ChannelDefInjected = ChannelDefInjected;
            options.ChannelGrammar = ChannelGrammar;
            options.ChannelDefSweep = ChannelDefSweep;
            options.IncludeNames = IncludeNames;
            options.IncludeKeywords = ParseList(IncludeKeywordsRaw);
            options.ExcludeKeywords = ParseList(ExcludeKeywordsRaw);
            options.CollapseRepeated = CollapseRepeated;
            options.CollapseSuffix = CollapseSuffix == null ? DefaultCollapseSuffix : CollapseSuffix.Trim();
            options.ExtraProtectedWords = ParseList(ExtraProtectedWordsRaw);
            options.ExcludedPhrases = ParseList(ExcludedPhrasesRaw);
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
