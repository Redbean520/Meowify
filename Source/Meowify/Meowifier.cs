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
        /// <summary>句末加「喵」。</summary>
        public bool SentenceEnd = true;

        /// <summary>人称替换：我→本喵、你→主人（我们/你们 一并处理）。</summary>
        public bool PronounSwap = true;

        /// <summary>长句末尾追加随机猫咪颜文字。</summary>
        public bool Kaomoji = true;

        /// <summary>追加颜文字需要的最少可见字符数（短标签不动，避免撑破按钮）。</summary>
        public int KaomojiMinChars = 10;

        /// <summary>颜文字候选列表。</summary>
        public string[] KaomojiList =
        {
            "（=^ω^=）",
            "（≧▽≦）",
            "（｡･ω･｡）",
            "（＾ω＾）",
            "（´▽`）"
        };

        public static MeowOptions Default
        {
            get { return new MeowOptions(); }
        }
    }

    /// <summary>
    /// 喵化核心：句尾加「喵」、人称替换（我→本喵 / 你→主人）、句末随机颜文字。
    /// 规则：
    ///   - 按行处理，句末标点（。！？!?…．）前插入「喵」；
    ///   - 行尾没有标点时在行尾（尾部闭合标签/右引号括号之前）补「喵」；
    ///   - 如果该句末已经是「喵」则跳过（幂等，可反复调用）；
    ///   - 纯标签、纯数字、纯空白、纯标点不加；
    ///   - 不碰 &lt;tag&gt; 内部、不碰 {0}/{PROJECT_label} 占位符；
    ///   - 颜文字按原文哈希确定性选择（同一句永远同一颜文字，不会每帧闪烁），
    ///     只加给足够长的、含句读的文本，避免撑破信息卡/按钮。
    /// </summary>
    public static class Meowifier
    {
        public const char MeowChar = '喵';

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

        private static readonly string[][] PronounPairs =
        {
            new[] { "我们", "本喵们" },
            new[] { "你们", "主人们" },
            new[] { "我", "本喵" },
            new[] { "你", "主人" }
        };

        /// <summary>
        /// 含有「我 / 你」但并非人称代词的固定词，替换前先遮起来，避免「迷你」→「迷主人」这类误伤。
        /// </summary>
        private static readonly string[] ProtectedWords =
        {
            "迷你", "自我", "忘我", "无我", "敌我", "你我", "你死我活", "你争我夺"
        };

        private const char MaskStart = '\uE000';
        private const char MaskEnd = '\uE001';

        private static readonly Dictionary<string, string> Cache = new Dictionary<string, string>(StringComparer.Ordinal);
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

            string result;
            if (text.IndexOf('\n') < 0)
            {
                result = MeowifyLine(text);
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
                    string meowed = MeowifyLine(line);
                    if (!string.Equals(meowed, line, StringComparison.Ordinal))
                    {
                        lines[i] = meowed;
                        changed = true;
                    }
                }
                result = changed ? string.Join("\n", lines) : text;
            }

            return AppendKaomoji(text, result, options);
        }

        private static string MeowifyLine(string line)
        {
            string s = Options.PronounSwap ? SwapPronouns(line) : line;
            if (!Options.SentenceEnd)
            {
                return s;
            }
            return MeowLineSentenceEnd(s);
        }

        /// <summary>在可见文本段里做人称替换，标签与占位符原样保留。</summary>
        private static string SwapPronouns(string line)
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
                        AppendRun(sb, line, runStart, i);
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
                        AppendRun(sb, line, runStart, i);
                        sb.Append(line, i, phEnd - i + 1);
                        i = phEnd + 1;
                        runStart = i;
                        continue;
                    }
                }
                i++;
            }
            AppendRun(sb, line, runStart, len);
            return sb.ToString();
        }

        private static void AppendRun(StringBuilder sb, string line, int start, int end)
        {
            if (end <= start)
            {
                return;
            }
            string run = line.Substring(start, end - start);
            run = MaskProtected(run);
            for (int p = 0; p < PronounPairs.Length; p++)
            {
                run = run.Replace(PronounPairs[p][0], PronounPairs[p][1]);
            }
            run = UnmaskProtected(run);
            sb.Append(run);
        }

        private static string MaskProtected(string run)
        {
            for (int i = 0; i < ProtectedWords.Length; i++)
            {
                string word = ProtectedWords[i];
                if (run.IndexOf(word, StringComparison.Ordinal) >= 0)
                {
                    run = run.Replace(word, MaskStart.ToString() + i + MaskEnd.ToString());
                }
            }
            return run;
        }

        private static string UnmaskProtected(string run)
        {
            for (int i = 0; i < ProtectedWords.Length; i++)
            {
                string token = MaskStart.ToString() + i + MaskEnd.ToString();
                if (run.IndexOf(token, StringComparison.Ordinal) >= 0)
                {
                    run = run.Replace(token, ProtectedWords[i]);
                }
            }
            return run;
        }

        private static string MeowLineSentenceEnd(string line)
        {
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

                    if (letterSeen)
                    {
                        int insertAt = TrailingDecorationStart(sb);
                        if (LastVisibleChar(sb, insertAt) != MeowChar)
                        {
                            sb.Insert(insertAt, MeowChar);
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

            if (!letterSeen || visibleChars < 2)
            {
                return line;
            }

            int endInsertAt = TrailingDecorationStart(sb);
            char last = LastVisibleChar(sb, endInsertAt);
            if (last != MeowChar && last != '\0' && !SentenceEndChars.Contains(last))
            {
                sb.Insert(endInsertAt, MeowChar);
            }

            string result = sb.ToString();
            return string.Equals(result, line, StringComparison.Ordinal) ? line : result;
        }

        private static string AppendKaomoji(string original, string result, MeowOptions options)
        {
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

            int end = result.Length;
            while (end > 0 && char.IsWhiteSpace(result[end - 1]))
            {
                end--;
            }
            if (end == 0)
            {
                return result;
            }

            uint h = Hash(original);
            string kaomoji = options.KaomojiList[(int)(h % (uint)options.KaomojiList.Length)];
            return result.Substring(0, end) + kaomoji + result.Substring(end);
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

        /// <summary>跳过行尾空白、闭合标签、右引号/括号，返回可插入「喵」的位置。</summary>
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
