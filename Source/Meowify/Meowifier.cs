using System;
using System.Collections.Generic;
using System.Text;

namespace Meowify
{
    /// <summary>
    /// 喵化选项。与游戏设置解耦，单元测试可直接构造。
    /// 默认行为参考「QQ喵喵助手」：句尾加喵 + 人称替换 + 句末随机猫咪颜文字。
    /// </summary>
    public sealed class MeowOptions
    {
        /// <summary>句尾加语气词。</summary>
        public bool SentenceEnd = true;

        /// <summary>人称替换：我→本喵、你→主人（我们/你们 一并处理）。</summary>
        public bool PronounSwap = true;

        /// <summary>第一人称自称（我）；留空表示不替换「我」。</summary>
        public string SelfPronoun = "本喵";

        /// <summary>第一人称复数（我们）；留空 = 自动用「自称 + 们」。</summary>
        public string SelfPluralPronoun = "";

        /// <summary>第二人称称呼（你）；留空表示不替换「你」。</summary>
        public string OtherPronoun = "主人";

        /// <summary>第二人称复数（你们）；留空 = 自动用「称呼 + 们」。</summary>
        public string OtherPluralPronoun = "";

        /// <summary>第三人称「他 / 他们」；留空表示不替换。</summary>
        public string ThirdPronounHe = "";
        public string ThirdPronounHePlural = "";

        /// <summary>第三人称「她 / 她们」；留空表示不替换。</summary>
        public string ThirdPronounShe = "";
        public string ThirdPronounShePlural = "";

        /// <summary>第三人称「它 / 它们」；留空表示不替换。</summary>
        public string ThirdPronounIt = "";
        public string ThirdPronounItPlural = "";

        /// <summary>自定义人称替换：每项形如「原词=替换词」，会与上面的内置项一起生效。</summary>
        public string[] CustomPronouns = new string[0];

        /// <summary>少于这个可见字数的文本不加句尾词（避免把按钮、图标说明撑破）。</summary>
        public int MinChars = 2;

        /// <summary>句尾词候选；每个句子按原文哈希确定性地挑一个。空数组 = 不加句尾词。</summary>
        public string[] EndWords = { "喵" };

        /// <summary>英文文本使用的句尾词（英文界面生效，插入时会自动在前面补空格）。</summary>
        public string[] EnglishEndWords = { "meow", "nya", "mrrp" };

        /// <summary>英文界面模式：句尾词改用 EnglishEndWords，并带前导空格。</summary>
        public bool EnglishMode;

        /// <summary>句尾词出现概率（0-100）。100 = 每句都加；调低后按原文哈希确定性跳过一部分句子。</summary>
        public int SentenceEndChance = 100;

        /// <summary>长句末尾追加随机猫咪颜文字。</summary>
        public bool Kaomoji = true;

        /// <summary>追加颜文字需要的最少可见字符数。</summary>
        public int KaomojiMinChars = 10;

        /// <summary>追加颜文字的概率（0-100）。</summary>
        public int KaomojiChance = 100;

        /// <summary>额外颜文字列表（自定义，始终参与抽取；默认空，内置社区颜文字池已足够）。</summary>
        public string[] KaomojiList = new string[0];

        /// <summary>用户额外添加的保护词（含「我/你」但不是人称代词，替换时整体跳过）。</summary>
        public string[] ExtraProtectedWords = new string[0];

        /// <summary>排除片段：文本里出现任意一个片段就完全不喵化（给个别模组的特殊文本留后门）。</summary>
        public string[] ExcludedPhrases = new string[0];

        /// <summary>适用范围：界面/按钮/提示文字（LoadedLanguage）。</summary>
        public bool ChannelKeyed = true;

        /// <summary>适用范围：DefInjected 注入的可见文本。</summary>
        public bool ChannelDefInjected = true;

        /// <summary>适用范围：语法规则生成的句子（事件、社交日志、故事）。</summary>
        public bool ChannelGrammar = true;

        /// <summary>适用范围：def 扫尾（标签、描述、背景故事）。</summary>
        public bool ChannelDefSweep = true;

        /// <summary>名字（小人、动物、物品名）也喵化。默认关，保持名字原样。</summary>
        public bool IncludeNames = false;

        /// <summary>关键词白名单：只有包含其中任意一个词的文本才喵化（空 = 不限制）。</summary>
        public string[] IncludeKeywords = new string[0];

        /// <summary>关键词黑名单：包含其中任意一个词的文本不喵化。</summary>
        public string[] ExcludeKeywords = new string[0];

        /// <summary>把句末连续重复的句尾词折叠（喵喵 → 喵~）。</summary>
        public bool CollapseRepeated = true;

        /// <summary>折叠后接的尾巴，填 "~" 就是喵~；留空则只折叠成单个句尾词。</summary>
        public string CollapseSuffix = "~";

        /// <summary>按语境挑颜文字（战斗/社交/工作/食物/动物各一套），关闭则只用自定义候选。</summary>
        public bool KaomojiContextual = true;

        public static MeowOptions Default
        {
            get { return new MeowOptions(); }
        }
    }

    /// <summary>
    /// 喵化核心：句尾加语气词、人称替换、句末随机颜文字。
    /// 规则：
    ///   - 按行处理，句末标点（。！？!?…．）前插入句尾词；
    ///   - 行尾没有标点时在行尾（尾部闭合标签/右引号括号之前）补句尾词；
    ///   - 该句末已经是任意句尾词则跳过（幂等，可反复调用）；
    ///   - 纯标签、纯数字、纯空白、纯标点、过短文本不加；
    ///   - 不碰 &lt;tag&gt; 内部、不碰 {0}/{PROJECT_label} 占位符；
    ///   - 句尾词与颜文字都按原文哈希确定性选择（同一句永远同一个，不会每帧闪烁）。
    /// </summary>
    public static class Meowifier
    {
        private const int MaxCacheEntries = 50000;

        private static readonly HashSet<char> SentenceEndChars = new HashSet<char>
        {
            '。', '！', '？', '!', '?', '…', '．'
        };

        private static readonly HashSet<char> ClosingChars = new HashSet<char>
        {
            // 注意：不要包含 '}' 与 ']'——翻译串里的 {0}、{PROJECT_label} 占位符不能被拆开
            '」', '』', '”', '’', '）', ')', '】', '》', '〉', '］', '｝'
        };

        /// <summary>
        /// 含有「我 / 你」但并非人称代词的固定词，替换前先遮起来，避免「迷你」→「迷主人」这类误伤。
        /// </summary>
        private static readonly string[] BuiltinProtectedWords =
        {
            "迷你", "自我", "忘我", "无我", "敌我", "你我", "你死我活", "你争我夺",
            "其他", "其它", "吉他", "他乡", "他人"
        };

        private const char MaskStart = '\uE000';
        private const char MaskEnd = '\uE001';

        private static readonly Dictionary<string, string> Cache = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>最近一次构建的规则快照；Options 没换就直接复用，省掉每次调用的数组分配。</summary>
        private static Rules lastRules;
        private static readonly object CacheLock = new object();

        /// <summary>当前生效的选项；模组加载和设置变更时替换。</summary>
        public static MeowOptions Options = MeowOptions.Default;

        public static void ClearCache()
        {
            lock (CacheLock)
            {
                Cache.Clear();
            }
        }

        public static string Meowify(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            lock (CacheLock)
            {
                string cached;
                if (Cache.TryGetValue(text, out cached))
                {
                    return cached;
                }
            }

            string result;
            try
            {
                result = MeowifyCore(text);
            }
            catch (Exception)
            {
                return text;
            }

            if (!string.Equals(result, text, StringComparison.Ordinal))
            {
                lock (CacheLock)
                {
                    if (Cache.Count >= MaxCacheEntries)
                    {
                        Cache.Clear();
                    }
                    Cache[text] = result;
                }
            }

            return result;
        }

        private static string MeowifyCore(string text)
        {
            MeowOptions options = Options;

            // 已经是喵化产物（结尾带颜文字）就直接返回，避免二次处理
            if (options.Kaomoji && EndsWithKaomoji(text))
            {
                return text;
            }

            // 纯数字/纯符号/纯空白没有可喵化的内容，直接返回（性能优化）
            if (!HasAnyLetter(text))
            {
                return text;
            }

            Rules rules = GetRules(options);

            // 命中排除片段 / 关键词过滤不通过就原样返回
            if (IsExcluded(text, rules.ExcludedPhrases))
            {
                return text;
            }

            if (!PassesKeywordFilter(text, rules))
            {
                return text;
            }

            string result;
            if (text.IndexOf('\n') < 0)
            {
                result = MeowifyLine(text, rules);
            }
            else
            {
                string[] lines = text.Split('\n');
                bool changed = false;
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line.Length == 0)
                    {
                        continue;
                    }
                    string meowed = MeowifyLine(line, rules);
                    if (!string.Equals(meowed, line, StringComparison.Ordinal))
                    {
                        lines[i] = meowed;
                        changed = true;
                    }
                }
                result = changed ? string.Join("\n", lines) : text;
            }

            // 折叠句末连续重复的同一个句尾词（喵喵 → 喵~，修多层叠加）
            result = CollapseRepeatedEndWords(result, rules);
            return AppendKaomoji(text, result, rules);
        }

        /// <summary>取规则快照：选项对象没变就复用上次构建的结果。</summary>
        private static Rules GetRules(MeowOptions options)
        {
            Rules cached = lastRules;
            if (cached != null && ReferenceEquals(cached.Options, options))
            {
                return cached;
            }
            Rules built = Rules.From(options);
            lastRules = built;
            return built;
        }

        /// <summary>解析后的规则快照，一次 Meowify 调用内复用，避免反复分配。</summary>
        private sealed class Rules
        {
            public MeowOptions Options;
            public string[] EndWords;
            public bool EnglishMode;
            public string[][] PronounPairs;
            public string[] ProtectedWords;
            public string[] ExcludedPhrases;
            public string[] IncludeKeywords;
            public string[] ExcludeKeywords;
            public int SentenceEndChance;
            public bool KaomojiContextual;
            public bool CollapseRepeated;
            public string CollapseSuffix;

            public static Rules From(MeowOptions options)
            {
                Rules rules = new Rules();
                rules.Options = options;
                rules.EnglishMode = options.EnglishMode;
                rules.EndWords = CleanList(rules.EnglishMode ? options.EnglishEndWords : options.EndWords);
                if (rules.EnglishMode)
                {
                    for (int i = 0; i < rules.EndWords.Length; i++)
                    {
                        if (rules.EndWords[i].Length > 0 && rules.EndWords[i][0] != ' ')
                        {
                            rules.EndWords[i] = " " + rules.EndWords[i];
                        }
                    }
                }
                rules.ProtectedWords = MergeProtected(options.ExtraProtectedWords);
                rules.ExcludedPhrases = CleanList(options.ExcludedPhrases);
                rules.IncludeKeywords = CleanList(options.IncludeKeywords);
                rules.ExcludeKeywords = CleanList(options.ExcludeKeywords);
                rules.SentenceEndChance = options.SentenceEndChance;
                rules.KaomojiContextual = options.KaomojiContextual;
                rules.CollapseRepeated = options.CollapseRepeated;
                rules.CollapseSuffix = options.CollapseSuffix == null ? string.Empty : options.CollapseSuffix;

                List<string[]> pairs = new List<string[]>(20);

                string self = Trim(options.SelfPronoun);
                string other = Trim(options.OtherPronoun);
                string he = Trim(options.ThirdPronounHe);
                string she = Trim(options.ThirdPronounShe);
                string it = Trim(options.ThirdPronounIt);

                AddPronoun(pairs, "我们", Plural(options.SelfPluralPronoun, self));
                AddPronoun(pairs, "我", self);
                AddPronoun(pairs, "你们", Plural(options.OtherPluralPronoun, other));
                AddPronoun(pairs, "你", other);
                AddPronoun(pairs, "他们", Plural(options.ThirdPronounHePlural, he));
                AddPronoun(pairs, "他", he);
                AddPronoun(pairs, "她们", Plural(options.ThirdPronounShePlural, she));
                AddPronoun(pairs, "她", she);
                AddPronoun(pairs, "它们", Plural(options.ThirdPronounItPlural, it));
                AddPronoun(pairs, "它", it);

                string[] custom = options.CustomPronouns;
                if (custom != null)
                {
                    for (int i = 0; i < custom.Length; i++)
                    {
                        string line = Trim(custom[i]);
                        int split = line.IndexOf('=');
                        if (split <= 0 || split >= line.Length - 1)
                        {
                            continue;
                        }
                        AddPronoun(pairs, Trim(line.Substring(0, split)), Trim(line.Substring(split + 1)));
                    }
                }

                // 长词优先，避免「我们」里的「我」先被换掉
                pairs.Sort(ComparePronounPairs);
                rules.PronounPairs = pairs.ToArray();
                return rules;
            }

            private static int ComparePronounPairs(string[] a, string[] b)
            {
                int byLength = b[0].Length - a[0].Length;
                return byLength != 0 ? byLength : string.CompareOrdinal(a[0], b[0]);
            }
        }

        private static string Trim(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        /// <summary>复数形式：给了就用手填的，否则自动「单数 + 们」。</summary>
        private static string Plural(string configuredPlural, string singular)
        {
            string plural = Trim(configuredPlural);
            if (plural.Length > 0)
            {
                return plural;
            }
            return singular.Length > 0 ? singular + "们" : string.Empty;
        }

        private static void AddPronoun(List<string[]> pairs, string from, string to)
        {
            if (from.Length == 0 || to.Length == 0 || string.Equals(from, to, StringComparison.Ordinal))
            {
                return;
            }
            for (int i = 0; i < pairs.Count; i++)
            {
                if (string.Equals(pairs[i][0], from, StringComparison.Ordinal))
                {
                    return;
                }
            }
            pairs.Add(new[] { from, to });
        }

        private static string MeowifyLine(string line, Rules rules)
        {
            bool sentenceEnd = rules.Options.SentenceEnd && rules.EndWords.Length > 0;
            if (!rules.Options.PronounSwap && !sentenceEnd)
            {
                return line;
            }
            string s = rules.Options.PronounSwap ? SwapPronouns(line, rules) : line;
            if (!rules.Options.SentenceEnd || rules.EndWords.Length == 0)
            {
                return s;
            }
            return MeowLineSentenceEnd(s, rules);
        }

        /// <summary>在可见文本段里做人称替换，标签与占位符原样保留。</summary>
        private static string SwapPronouns(string line, Rules rules)
        {
            StringBuilder sb = new StringBuilder(line.Length + 8);
            int len = line.Length;
            int i = 0;
            int runStart = 0;

            while (i < len)
            {
                char c = line[i];
                if (c == '<')
                {
                    int tagEnd = line.IndexOf('>', i + 1);
                    if (tagEnd >= 0)
                    {
                        AppendRun(sb, line, runStart, i, rules);
                        sb.Append(line, i, tagEnd - i + 1);
                        i = tagEnd + 1;
                        runStart = i;
                        continue;
                    }
                }
                else if (c == '{')
                {
                    int phEnd = line.IndexOf('}', i + 1);
                    if (phEnd >= 0)
                    {
                        AppendRun(sb, line, runStart, i, rules);
                        sb.Append(line, i, phEnd - i + 1);
                        i = phEnd + 1;
                        runStart = i;
                        continue;
                    }
                }
                i++;
            }
            AppendRun(sb, line, runStart, len, rules);
            return sb.ToString();
        }

        private static void AppendRun(StringBuilder sb, string line, int start, int end, Rules rules)
        {
            if (end <= start)
            {
                return;
            }
            string run = line.Substring(start, end - start);
            run = MaskProtected(run, rules.ProtectedWords);
            run = ReplacePronounsOnce(run, rules.PronounPairs);
            run = UnmaskProtected(run, rules.ProtectedWords);
            sb.Append(run);
        }

        /// <summary>
        /// 单次从左到右扫描替换人称：替换出来的词不会被再次匹配（保证幂等），
        /// 同一位置按「长词优先」选最长的原词，遮罩段原样透传。
        /// </summary>
        private static string ReplacePronounsOnce(string run, string[][] pairs)
        {
            if (pairs.Length == 0)
            {
                return run;
            }
            StringBuilder sb = new StringBuilder(run.Length + 8);
            int len = run.Length;
            int i = 0;
            while (i < len)
            {
                char c = run[i];

                // 保护词遮罩段整体透传
                if (c == MaskStart)
                {
                    int maskEnd = run.IndexOf(MaskEnd, i + 1);
                    if (maskEnd >= 0)
                    {
                        sb.Append(run, i, maskEnd - i + 1);
                        i = maskEnd + 1;
                        continue;
                    }
                }

                // 先看是否已经是替换结果（幂等：避免「他」→「他喵」再变「他喵喵」）
                bool emitted = false;
                for (int p = 0; p < pairs.Length; p++)
                {
                    string to = pairs[p][1];
                    if (to.Length > 0 && MatchesAt(run, i, to))
                    {
                        sb.Append(to);
                        i += to.Length;
                        emitted = true;
                        break;
                    }
                }
                if (emitted)
                {
                    continue;
                }

                // 再匹配原词（pairs 已按原词长度降序，长词优先）
                for (int p = 0; p < pairs.Length; p++)
                {
                    string from = pairs[p][0];
                    if (from.Length > 0 && MatchesAt(run, i, from))
                    {
                        sb.Append(pairs[p][1]);
                        i += from.Length;
                        emitted = true;
                        break;
                    }
                }
                if (emitted)
                {
                    continue;
                }

                sb.Append(c);
                i++;
            }
            return sb.ToString();
        }

        private static bool MatchesAt(string text, int index, string token)
        {
            if (token.Length == 0 || index + token.Length > text.Length)
            {
                return false;
            }
            return string.CompareOrdinal(text, index, token, 0, token.Length) == 0;
        }

        private static string MaskProtected(string run, string[] protectedWords)
        {
            for (int i = 0; i < protectedWords.Length; i++)
            {
                string word = protectedWords[i];
                if (run.IndexOf(word, StringComparison.Ordinal) >= 0)
                {
                    run = run.Replace(word, MaskStart.ToString() + i + MaskEnd.ToString());
                }
            }
            return run;
        }

        private static string UnmaskProtected(string run, string[] protectedWords)
        {
            for (int i = 0; i < protectedWords.Length; i++)
            {
                string token = MaskStart.ToString() + i + MaskEnd.ToString();
                if (run.IndexOf(token, StringComparison.Ordinal) >= 0)
                {
                    run = run.Replace(token, protectedWords[i]);
                }
            }
            return run;
        }

        private static string MeowLineSentenceEnd(string line, Rules rules)
        {
            string[] endWords = rules.EndWords;
            string endWord = PickWord(line, endWords);

            int len = line.Length;
            StringBuilder sb = new StringBuilder(len + 8);
            bool letterSeen = false;
            int visibleChars = 0;

            int i = 0;
            while (i < len)
            {
                char c = line[i];

                // 标签整体透传，不参与句读判断
                if (c == '<')
                {
                    int tagEnd = line.IndexOf('>', i + 1);
                    if (tagEnd >= 0)
                    {
                        sb.Append(line, i, tagEnd - i + 1);
                        i = tagEnd + 1;
                        continue;
                    }
                }

                if (IsSentenceEndChar(c, rules, line, i))
                {
                    int j = i;
                    while (j < len && IsSentenceEndChar(line[j], rules, line, j))
                    {
                        j++;
                    }

                    if (letterSeen && ShouldAddEndWord(line, i, rules))
                    {
                        int insertAt = TrailingDecorationStart(sb);
                        if (!EndsWithEndWordOrSuffix(sb, insertAt, rules))
                        {
                            sb.Insert(insertAt, endWord);
                        }
                    }

                    sb.Append(line, i, j - i);
                    i = j;
                    continue;
                }

                if (char.IsLetter(c))
                {
                    letterSeen = true;
                }
                if (!char.IsWhiteSpace(c))
                {
                    visibleChars++;
                }
                sb.Append(c);
                i++;
            }

            if (!letterSeen || visibleChars < Math.Max(1, rules.Options.MinChars))
            {
                return line;
            }

            int endInsertAt = TrailingDecorationStart(sb);
            char last = LastVisibleChar(sb, endInsertAt);
            if (ShouldAddEndWord(line, -1, rules) && last != '\0' && !IsSentenceEnd(last, rules) &&
                !EndsWithEndWordOrSuffix(sb, endInsertAt, rules) && !EndsWithNamePlaceholder(sb, endInsertAt))
            {
                sb.Insert(endInsertAt, endWord);
            }

            string result = sb.ToString();
            return string.Equals(result, line, StringComparison.Ordinal) ? line : result;
        }

        private static string AppendKaomoji(string original, string result, Rules rules)
        {
            MeowOptions options = rules.Options;
            if (!options.Kaomoji)
            {
                return result;
            }
            if (EndsWithKaomoji(result))
            {
                return result;
            }
            if (CountVisible(result) < options.KaomojiMinChars)
            {
                return result;
            }
            if (!HasSentenceEndPunctuation(result, rules))
            {
                return result;
            }

            string[] candidates = BuildKaomojiCandidates(original, rules);
            if (candidates.Length == 0)
            {
                return result;
            }

            uint h = Hash(original);
            int chance = options.KaomojiChance;
            if (chance <= 0 || (int)(h % 100u) >= chance)
            {
                return result;
            }

            int end = result.Length;
            while (end > 0 && char.IsWhiteSpace(result[end - 1]))
            {
                end--;
            }
            if (end == 0)
            {
                return result;
            }

            string kaomoji = candidates[(int)(h % (uint)candidates.Length)];
            return result.Substring(0, end) + kaomoji + result.Substring(end);
        }
        /// <summary>句尾词：按原文哈希确定性挑选，同一句永远同一个词。</summary>
        private static string PickWord(string line, string[] words)
        {
            return words[(int)(Hash(line) % (uint)words.Length)];
        }

        private static bool EndsWithAnyWord(StringBuilder sb, int limit, string[] words)
        {
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                int wordLen = word.Length;
                if (wordLen == 0 || wordLen > limit)
                {
                    continue;
                }
                bool match = true;
                for (int k = 0; k < wordLen; k++)
                {
                    if (sb[limit - wordLen + k] != word[k])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    return true;
                }
            }
            return false;
        }

        private static string[] CleanList(string[] list)
        {
            if (list == null || list.Length == 0)
            {
                return new string[0];
            }
            List<string> result = new List<string>(list.Length);
            for (int i = 0; i < list.Length; i++)
            {
                string item = list[i] == null ? null : list[i].Trim();
                if (!string.IsNullOrEmpty(item) && !result.Contains(item))
                {
                    result.Add(item);
                }
            }
            return result.ToArray();
        }

        private static string[] MergeProtected(string[] extra)
        {
            List<string> result = new List<string>(BuiltinProtectedWords.Length + (extra == null ? 0 : extra.Length));
            for (int i = 0; i < BuiltinProtectedWords.Length; i++)
            {
                result.Add(BuiltinProtectedWords[i]);
            }
            if (extra != null)
            {
                for (int i = 0; i < extra.Length; i++)
                {
                    string item = extra[i] == null ? null : extra[i].Trim();
                    if (!string.IsNullOrEmpty(item) && item.IndexOf(MaskStart) < 0 && item.IndexOf(MaskEnd) < 0 && !result.Contains(item))
                    {
                        result.Add(item);
                    }
                }
            }
            return result.ToArray();
        }

        private static bool EndsWithKaomoji(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            int end = text.Length;
            while (end > 0 && char.IsWhiteSpace(text[end - 1]))
            {
                end--;
            }
            if (end <= 0)
            {
                return false;
            }
            if (TailMatchesAny(text, end, KaomojiPools.All))
            {
                return true;
            }
            string[] custom = Options == null ? null : Options.KaomojiList;
            return custom != null && TailMatchesAny(text, end, custom);
        }

        /// <summary>以候选串结尾判定（区分大小写，用于幂等检测）。</summary>
        private static bool TailMatchesAny(string text, int end, string[] list)
        {
            if (list == null)
            {
                return false;
            }
            for (int i = 0; i < list.Length; i++)
            {
                string k = list[i];
                if (!string.IsNullOrEmpty(k) && end >= k.Length &&
                    string.CompareOrdinal(text, end - k.Length, k, 0, k.Length) == 0)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>文本里是否存在字母（中英文都算）；纯数字/纯符号直接跳过喵化。</summary>
        private static bool HasAnyLetter(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsLetter(text[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>统计可见字符（跳过标签、占位符、空白）。</summary>
        private static int CountVisible(string text)
        {
            int count = 0;
            int i = 0;
            int len = text.Length;
            while (i < len)
            {
                char c = text[i];
                if (c == '<')
                {
                    int tagEnd = text.IndexOf('>', i + 1);
                    if (tagEnd >= 0)
                    {
                        i = tagEnd + 1;
                        continue;
                    }
                }
                else if (c == '{')
                {
                    int phEnd = text.IndexOf('}', i + 1);
                    if (phEnd >= 0)
                    {
                        i = phEnd + 1;
                        continue;
                    }
                }
                if (!char.IsWhiteSpace(c))
                {
                    count++;
                }
                i++;
            }
            return count;
        }

        /// <summary>可见文本里是否出现了句末标点。</summary>
        private static bool HasSentenceEndPunctuation(string text, Rules rules)
        {
            int i = 0;
            int len = text.Length;
            while (i < len)
            {
                char c = text[i];
                if (c == '<')
                {
                    int tagEnd = text.IndexOf('>', i + 1);
                    if (tagEnd >= 0)
                    {
                        i = tagEnd + 1;
                        continue;
                    }
                }
                else if (c == '{')
                {
                    int phEnd = text.IndexOf('}', i + 1);
                    if (phEnd >= 0)
                    {
                        i = phEnd + 1;
                        continue;
                    }
                }
                if (IsSentenceEnd(c, rules))
                {
                    return true;
                }
                i++;
            }
            return false;
        }

        /// <summary>FNV-1a 32 位，跨进程稳定（不用 string.GetHashCode）。</summary>
        private static uint Hash(string text)
        {
            uint h = 2166136261u;
            for (int i = 0; i < text.Length; i++)
            {
                h ^= text[i];
                h *= 16777619u;
            }
            return h;
        }

        /// <summary>是否要在这个位置加句尾词：按概率 + 原文哈希确定性决定，同一句永远同一结果。</summary>
        private static bool ShouldAddEndWord(string line, int position, Rules rules)
        {
            int chance = rules.SentenceEndChance;
            if (chance >= 100)
            {
                return true;
            }
            if (chance <= 0)
            {
                return false;
            }
            uint h = Hash(line) ^ ((uint)(position + 1) * 2654435761u);
            return (int)(h % 100u) < chance;
        }

        /// <summary>文本里是否出现了任一排除片段。</summary>
        private static bool IsExcluded(string text, string[] phrases)
        {
            for (int i = 0; i < phrases.Length; i++)
            {
                string phrase = phrases[i];
                if (phrase.Length > 0 && text.IndexOf(phrase, StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>把句末/行尾连续重复的同一个句尾词折叠，并按设置接上折叠尾巴（喵喵 → 喵~）。</summary>
        private static string CollapseRepeatedEndWords(string text, Rules rules)
        {
            string[] endWords = rules.EndWords;
            if (!rules.CollapseRepeated || text == null || text.Length < 2 || endWords == null || endWords.Length == 0)
            {
                return text;
            }

            StringBuilder sb = new StringBuilder(text.Length);
            int len = text.Length;
            int i = 0;
            while (i < len)
            {
                char c = text[i];
                if (c == '\n')
                {
                    CollapseRepeatedTail(sb, rules);
                    sb.Append(c);
                    i++;
                    continue;
                }
                if (IsSentenceEnd(c, rules))
                {
                    CollapseRepeatedTail(sb, rules);
                    int j = i;
                    while (j < len && IsSentenceEnd(text[j], rules))
                    {
                        j++;
                    }
                    sb.Append(text, i, j - i);
                    i = j;
                    continue;
                }
                sb.Append(c);
                i++;
            }
            CollapseRepeatedTail(sb, rules);

            string collapsed = sb.ToString();
            return string.Equals(collapsed, text, StringComparison.Ordinal) ? text : collapsed;
        }

        private static void CollapseRepeatedTail(StringBuilder sb, Rules rules)
        {
            string[] endWords = rules.EndWords;
            bool removed = false;
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int w = 0; w < endWords.Length; w++)
                {
                    string word = endWords[w];
                    int wordLen = word.Length;
                    if (wordLen == 0 || sb.Length < wordLen * 2)
                    {
                        continue;
                    }
                    int n = sb.Length;
                    if (!TailEquals(sb, n, word) || !TailEquals(sb, n - wordLen, word))
                    {
                        continue;
                    }
                    sb.Length = n - wordLen;
                    changed = true;
                    removed = true;
                    break;
                }
            }
            if (removed && !string.IsNullOrEmpty(rules.CollapseSuffix))
            {
                sb.Append(rules.CollapseSuffix);
            }
        }
        private static bool TailEquals(StringBuilder sb, int end, string word)
        {
            int wordLen = word.Length;
            if (end < wordLen)
            {
                return false;
            }
            for (int k = 0; k < wordLen; k++)
            {
                if (sb[end - wordLen + k] != word[k])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 行尾是否是「冒号 + 占位符」这种模板（如 "Possible unwanted relation: {0}"、"版本：{0}"）。
        /// 这种尾巴补喵会把代入的参数（常见是名字）粘成「孙冽喵」，看着像名字被喵化了；
        /// 而「粗糙的{0}」「{0}或{1}」这类尾巴是属性/物品，补喵没有歧义，照旧处理。
        /// </summary>
        private static bool EndsWithNamePlaceholder(StringBuilder sb, int limit)
        {
            int k = Math.Min(limit, sb.Length) - 1;
            if (k < 0 || sb[k] != '}')
            {
                return false;
            }
            int open = k - 1;
            while (open >= 0 && sb[open] != '{' && sb[open] != '}')
            {
                char c = sb[open];
                if (!(char.IsLetterOrDigit(c) || c == '_' || c == '.' || c == ':' || c == '-' || c == ',' || c == ' '))
                {
                    return false;
                }
                open--;
            }
            if (open < 0 || sb[open] != '{')
            {
                return false;
            }
            int before = open - 1;
            while (before >= 0 && char.IsWhiteSpace(sb[before]))
            {
                before--;
            }
            return before >= 0 && (sb[before] == ':' || sb[before] == '：');
        }

        /// <summary>句末标点判定：中文标点恒定；英文模式额外把「.」当句末（1.7 这类数字里的点除外）。</summary>
        private static bool IsSentenceEndChar(char c, Rules rules, string text, int index)
        {
            if (SentenceEndChars.Contains(c))
            {
                return true;
            }
            if (!rules.EnglishMode || c != '.')
            {
                return false;
            }
            // 只有夹在两个数字之间的点才当成小数点（1.7 / 1.2.3）；句末的 3.14. 仍算句号。
            bool prevDigit = index > 0 && char.IsDigit(text[index - 1]);
            bool nextDigit = index + 1 < text.Length && char.IsDigit(text[index + 1]);
            return !(prevDigit && nextDigit);
        }

        /// <summary>单个字符是否句末标点（用于行尾判断与折叠，不做数字上下文检查）。</summary>
        private static bool IsSentenceEnd(char c, Rules rules)
        {
            return SentenceEndChars.Contains(c) || (rules.EnglishMode && c == '.');
        }

        /// <summary>跳过行尾空白、闭合标签、右引号/括号，返回可插入句尾词的位置。</summary>
        private static int TrailingDecorationStart(StringBuilder sb)
        {
            int k = sb.Length - 1;
            while (k >= 0)
            {
                char c = sb[k];
                if (char.IsWhiteSpace(c))
                {
                    k--;
                    continue;
                }
                if (c == '>')
                {
                    int open = k - 1;
                    while (open >= 0 && sb[open] != '<')
                    {
                        open--;
                    }
                    if (open >= 0 && open + 1 < k && sb[open + 1] == '/')
                    {
                        k = open - 1;
                        continue;
                    }
                    break;
                }
                if (ClosingChars.Contains(c))
                {
                    k--;
                    continue;
                }
                break;
            }
            return k + 1;
        }

        /// <summary>返回 limit 之前最后一个可见（非空白、跳过标签）字符。</summary>
        private static char LastVisibleChar(StringBuilder sb, int limit)
        {
            int k = Math.Min(limit, sb.Length) - 1;
            while (k >= 0)
            {
                char c = sb[k];
                if (char.IsWhiteSpace(c))
                {
                    k--;
                    continue;
                }
                if (c == '>')
                {
                    int open = k - 1;
                    while (open >= 0 && sb[open] != '<')
                    {
                        open--;
                    }
                    if (open >= 0)
                    {
                        k = open - 1;
                        continue;
                    }
                }
                return c;
            }
            return '\0';
        }

        /// <summary>白名单/黑名单关键词过滤。</summary>
        private static bool PassesKeywordFilter(string text, Rules rules)
        {
            if (rules.ExcludeKeywords.Length > 0 && ContainsAny(text, rules.ExcludeKeywords))
            {
                return false;
            }
            if (rules.IncludeKeywords.Length > 0 && !ContainsAny(text, rules.IncludeKeywords))
            {
                return false;
            }
            return true;
        }

        private static bool ContainsAny(string text, string[] words)
        {
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0 && text.IndexOf(words[i], StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>行尾是否已经是句尾词，或已经是「句尾词 + 折叠尾巴」（喵 / 喵~）。</summary>
        private static bool EndsWithEndWordOrSuffix(StringBuilder sb, int limit, Rules rules)
        {
            if (EndsWithAnyWord(sb, limit, rules.EndWords))
            {
                return true;
            }
            string suffix = rules.CollapseSuffix;
            if (!rules.CollapseRepeated || string.IsNullOrEmpty(suffix) || limit < suffix.Length)
            {
                return false;
            }
            for (int k = 0; k < suffix.Length; k++)
            {
                if (sb[limit - suffix.Length + k] != suffix[k])
                {
                    return false;
                }
            }
            return EndsWithAnyWord(sb, limit - suffix.Length, rules.EndWords);
        }

        /// <summary>组装这次可用的颜文字候选：语境池 + 自定义列表。</summary>
        private static string[] BuildKaomojiCandidates(string text, Rules rules)
        {
            List<string> list = new List<string>(24);
            if (rules.KaomojiContextual)
            {
                string[] pool = KaomojiPools.ForText(text);
                for (int i = 0; i < pool.Length; i++)
                {
                    if (!list.Contains(pool[i]))
                    {
                        list.Add(pool[i]);
                    }
                }
            }
            string[] custom = rules.Options.KaomojiList;
            if (custom != null)
            {
                for (int i = 0; i < custom.Length; i++)
                {
                    string k = custom[i];
                    if (!string.IsNullOrEmpty(k) && !list.Contains(k))
                    {
                        list.Add(k);
                    }
                }
            }
            // 关掉语境池又没填自定义时，退回内置可爱池，别让颜文字功能变成摆设
            if (list.Count == 0)
            {
                string[] pool = KaomojiPools.Cute;
                for (int i = 0; i < pool.Length; i++)
                {
                    list.Add(pool[i]);
                }
            }
            return list.ToArray();
        }

        /// <summary>内置颜文字池：来源为 kaomoji-collection（MIT）等社区合集，按语境挑一套「应景」的。</summary>
        internal static class KaomojiPools
        {
            public static readonly string[] Cute =
            {
                "（=^ω^=）", "（≧▽≦）", "（｡･ω･｡）", "（＾ω＾）",
                "（´▽`）", "（＝ω＝）", "（*´ω`*）", "（◜◡◝）",
                "（╹◡╹）", "（*^ω^*）", "✨（❁´◡`❁）✨", "ヾ（*・ω・）ﾉ",
                "◝（*'▿'*）◜", "o＜´・ω・`＞o", "＼（＾▽＾）／", "（ ˘ω˘ ）"
            };

            public static readonly string[] Combat =
            {
                "（｀Д´）/", "ヽ（#ﾟДﾟ）ﾉ┌┛", "（╬◣д◢）", "щ（ﾟДﾟщ）",
                "（#ﾟДﾟ）", "（｀・ω・´）", "o（｀ω´*）o", "ヽ（｀Д´#）ﾉ",
                "（ﾉ｀Д）ﾉ", "Σ（°Д°）", "（｀-ω-´）✧", "（∩^o^）⊃━♡"
            };

            public static readonly string[] Social =
            {
                "（・ω・）ノ", "（＾▽＾）ノ", "ヽ（´▽｀）ノ", "（◕‿◕）ﾉ",
                "（*´ω｀*）ﾉ", "⊂（・▽・⊂）", "ヾ（・ω・｀）ﾉ~~", "（´▽`）ノ",
                "♪（´ε｀ ）", "（´ε｀ ）♡", "（*˘◡˘*）♡", "（´▽`）ノ~~"
            };

            public static readonly string[] Work =
            {
                "（・ω・）b", "（ ^ω^ ）b", "（｀・ω・´）", "□_ヾ（◕‿◕）",
                "□_ヾ（´ω`）", "ヽ（＾Д＾）ﾉ", "Σp[◎]ω・´）", "（/^▽^）/",
                "（｀-ω-´）✧", "d（-_^）"
            };

            public static readonly string[] Food =
            {
                "（◕‿◕）♡", "（⌒‿⌒）♡", "（*˘▾˘*）♡", "ヽ（♡‿♡）ﾉ",
                "o（^▽^）o♡", "（ ˘ω˘ ）", "（・ω・）ノ", "（ ・`ω・´）",
                "（￣～￣）", "（*´ω`*）♡"
            };

            public static readonly string[] Animal =
            {
                "（'ω'）", "（^>ω<^）", "∩（^ΦωΦ^）∩", "（∪^ω^）",
                "（υ◉ω◉υ）", "U[^ω^]U", "（ΘωΘ）", "（Θ◡Θ）",
                "（Θ▿Θ）", "ε（ c ´ω`）", "（V）（´・ω・`）（V）", "（V）（♡ω♡）（V）",
                "（⌒ω⌒）", "（ ˘ω˘ ）"
            };

            public static readonly string[] Sad =
            {
                "（´-ω-`）", "（´・ω・｀）", "（´._.`）♡", "（ ˘ω˘ ; ）",
                "ヾ（・ω・`; ）ノ", "╮（´-ω-`）╭", "ﾟ（ﾟ`ω´ ﾟ）ﾟ", "（´◦ω◦｀）"
            };

            public static readonly string[] Love =
            {
                "（♥ε♥）", "（♥ω♥*）", "（*´ω｀*）♡", "♡（.◜ω◝.）♡",
                "（*/ω＼*）", "（//▽//）", "（♡_♡）", "（♥‿♥）",
                "（✿♥‿♥✿）", "（❁´`❁）♡", "（♥◡♥）", "（♥o♥）"
            };

            private static readonly string[] CombatKeys =
            {
                "战斗", "攻击", "受伤", "流血", "死亡", "杀死", "击杀", "射击", "爆炸", "袭击",
                "敌人", "开火", "伤口", "疼痛", "昏迷", "失血", "战争", "突袭", "格挡", "中弹"
            ,
                "attack", "enemy", "wound", "bleed", "death", "kill", "shot", "explosion", "raid", "fight"
            };

            private static readonly string[] SadKeys =
            {
                "哭", "悲伤", "难过", "崩溃", "绝望", "沮丧", "哀悼", "痛苦", "不幸", "生病",
                "死亡", "逝去", "发疯", "精神崩溃"
            ,
                "sad", "cry", "grief", "despair", "died", "death", "breakdown", "ill"
            };

            private static readonly string[] SocialKeys =
            {
                "社交", "交谈", "聊天", "朋友", "关系", "恋人", "结婚", "婚姻", "争吵", "侮辱",
                "调情", "派对", "结识", "拜访", "求婚", "好感", "招募", "访客"
            ,
                "chat", "talk", "social", "friend", "visit"
            };

            private static readonly string[] LoveKeys =
            {
                "爱", "恋爱", "恋人", "亲密", "亲吻", "拥抱", "结婚", "伴侣", "心动", "甜"
            ,
                "love", "lover", "romance", "marry", "wedding", "date"
            };

            private static readonly string[] WorkKeys =
            {
                "建造", "采矿", "砍伐", "种植", "收获", "烹饪", "制作", "研究", "工作", "搬运",
                "修理", "缝制", "挖掘", "打扫", "建造中", "蓝图"
            ,
                "work", "job", "mine", "research", "build", "craft", "harvest"
            };

            private static readonly string[] FoodKeys =
            {
                "食物", "进食", "吃饭", "吃", "饭", "喝", "饿", "饱", "美味", "饥饿", "营养", "腐烂", "餐", "料理", "烹饪"
            ,
                "food", "meal", "eat", "hungry", "delicious", "cook", "raw"
            };

            private static readonly string[] AnimalKeys =
            {
                "动物", "猫", "狗", "驯服", "训练", "宠物", "鸟", "牛", "羊", "兔", "兽", "绮罗", "喵"
            ,
                "cat", "dog", "animal", "pet", "tame", "puppy", "kitten"
            };

            /// <summary>所有内置池合并（幂等检测与调试用）。</summary>
            public static readonly string[] All = BuildAll();

            private static string[] BuildAll()
            {
                List<string> list = new List<string>(160);
                string[][] pools = { Cute, Combat, Social, Work, Food, Animal, Sad, Love };
                for (int p = 0; p < pools.Length; p++)
                {
                    for (int i = 0; i < pools[p].Length; i++)
                    {
                        if (!list.Contains(pools[p][i]))
                        {
                            list.Add(pools[p][i]);
                        }
                    }
                }
                return list.ToArray();
            }

            public static string[] ForText(string text)
            {
                if (text == null || text.Length == 0)
                {
                    return Cute;
                }
                if (ContainsAny(text, CombatKeys))
                {
                    return Combat;
                }
                if (ContainsAny(text, SadKeys))
                {
                    return Sad;
                }
                if (ContainsAny(text, FoodKeys))
                {
                    return Food;
                }
                if (ContainsAny(text, AnimalKeys))
                {
                    return Animal;
                }
                if (ContainsAny(text, LoveKeys))
                {
                    return Love;
                }
                if (ContainsAny(text, SocialKeys))
                {
                    return Social;
                }
                if (ContainsAny(text, WorkKeys))
                {
                    return Work;
                }
                return Cute;
            }
        }

    }
}
