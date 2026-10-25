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

        /// <summary>第一人称自称；留空表示不替换「我」。</summary>
        public string SelfPronoun = "本喵";

        /// <summary>第二人称称呼；留空表示不替换「你」。</summary>
        public string OtherPronoun = "主人";

        /// <summary>少于这个可见字数的文本不加句尾词（避免把按钮、图标说明撑破）。</summary>
        public int MinChars = 2;

        /// <summary>句尾词候选；每个句子按原文哈希确定性地挑一个。空数组 = 不加句尾词。</summary>
        public string[] EndWords = { "喵" };

        /// <summary>句尾词出现概率（0-100）。100 = 每句都加；调低后按原文哈希确定性跳过一部分句子。</summary>
        public int SentenceEndChance = 100;

        /// <summary>长句末尾追加随机猫咪颜文字。</summary>
        public bool Kaomoji = true;

        /// <summary>追加颜文字需要的最少可见字符数。</summary>
        public int KaomojiMinChars = 10;

        /// <summary>追加颜文字的概率（0-100）。</summary>
        public int KaomojiChance = 100;

        /// <summary>颜文字候选列表。</summary>
        public string[] KaomojiList =
        {
            "（=^ω^=）",
            "（≧▽≦）",
            "（｡･ω･｡）",
            "（＾ω＾）",
            "（´▽`）"
        };

        /// <summary>用户额外添加的保护词（含「我/你」但不是人称代词，替换时整体跳过）。</summary>
        public string[] ExtraProtectedWords = new string[0];

        /// <summary>排除片段：文本里出现任意一个片段就完全不喵化（给个别模组的特殊文本留后门）。</summary>
        public string[] ExcludedPhrases = new string[0];

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
            "迷你", "自我", "忘我", "无我", "敌我", "你我", "你死我活", "你争我夺"
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
            if (options.Kaomoji && EndsWithKaomoji(text, options.KaomojiList))
            {
                return text;
            }

            // 纯数字/纯符号/纯空白没有可喵化的内容，直接返回（性能优化）
            if (!HasAnyLetter(text))
            {
                return text;
            }

            Rules rules = GetRules(options);

            // 命中排除片段就原样返回
            if (IsExcluded(text, rules.ExcludedPhrases))
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

            // 折叠句末连续重复的同一个句尾词（修「喵喵。」这种多层叠加）
            result = CollapseRepeatedEndWords(result, rules.EndWords);
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
            public string[][] PronounPairs;
            public string[] ProtectedWords;
            public string[] ExcludedPhrases;
            public int SentenceEndChance;

            public static Rules From(MeowOptions options)
            {
                Rules rules = new Rules();
                rules.Options = options;
                rules.EndWords = CleanList(options.EndWords);
                rules.ProtectedWords = MergeProtected(options.ExtraProtectedWords);
                rules.ExcludedPhrases = CleanList(options.ExcludedPhrases);
                rules.SentenceEndChance = options.SentenceEndChance;

                string self = options.SelfPronoun == null ? string.Empty : options.SelfPronoun.Trim();
                string other = options.OtherPronoun == null ? string.Empty : options.OtherPronoun.Trim();
                List<string[]> pairs = new List<string[]>(4);
                if (self.Length > 0)
                {
                    pairs.Add(new[] { "我们", self + "们" });
                    pairs.Add(new[] { "我", self });
                }
                if (other.Length > 0)
                {
                    pairs.Add(new[] { "你们", other + "们" });
                    pairs.Add(new[] { "你", other });
                }
                rules.PronounPairs = pairs.ToArray();
                return rules;
            }
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
            string[][] pairs = rules.PronounPairs;
            for (int p = 0; p < pairs.Length; p++)
            {
                run = run.Replace(pairs[p][0], pairs[p][1]);
            }
            run = UnmaskProtected(run, rules.ProtectedWords);
            sb.Append(run);
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

                if (SentenceEndChars.Contains(c))
                {
                    int j = i;
                    while (j < len && SentenceEndChars.Contains(line[j]))
                    {
                        j++;
                    }

                    if (letterSeen && ShouldAddEndWord(line, i, rules))
                    {
                        int insertAt = TrailingDecorationStart(sb);
                        if (!EndsWithAnyWord(sb, insertAt, endWords))
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
            if (ShouldAddEndWord(line, -1, rules) && last != '\0' && !SentenceEndChars.Contains(last) &&
                !EndsWithAnyWord(sb, endInsertAt, endWords))
            {
                sb.Insert(endInsertAt, endWord);
            }

            string result = sb.ToString();
            return string.Equals(result, line, StringComparison.Ordinal) ? line : result;
        }

        private static string AppendKaomoji(string original, string result, Rules rules)
        {
            MeowOptions options = rules.Options;
            if (!options.Kaomoji || options.KaomojiList == null || options.KaomojiList.Length == 0)
            {
                return result;
            }
            if (EndsWithKaomoji(result, options.KaomojiList))
            {
                return result;
            }
            if (CountVisible(result) < options.KaomojiMinChars)
            {
                return result;
            }
            if (!HasSentenceEndPunctuation(result))
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

            string kaomoji = options.KaomojiList[(int)(h % (uint)options.KaomojiList.Length)];
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

        private static bool EndsWithKaomoji(string text, string[] list)
        {
            if (string.IsNullOrEmpty(text) || list == null)
            {
                return false;
            }
            int end = text.Length;
            while (end > 0 && char.IsWhiteSpace(text[end - 1]))
            {
                end--;
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
        private static bool HasSentenceEndPunctuation(string text)
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
                if (SentenceEndChars.Contains(c))
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

        /// <summary>把句末/行尾连续重复的同一个句尾词折叠成一个。</summary>
        private static string CollapseRepeatedEndWords(string text, string[] endWords)
        {
            if (text == null || text.Length < 2 || endWords == null || endWords.Length == 0)
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
                    CollapseRepeatedTail(sb, endWords);
                    sb.Append(c);
                    i++;
                    continue;
                }
                if (SentenceEndChars.Contains(c))
                {
                    CollapseRepeatedTail(sb, endWords);
                    int j = i;
                    while (j < len && SentenceEndChars.Contains(text[j]))
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
            CollapseRepeatedTail(sb, endWords);

            string collapsed = sb.ToString();
            return string.Equals(collapsed, text, StringComparison.Ordinal) ? text : collapsed;
        }

        private static void CollapseRepeatedTail(StringBuilder sb, string[] endWords)
        {
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
                    break;
                }
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
    }
}
