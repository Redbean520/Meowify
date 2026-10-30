using System;
using System.Collections.Generic;
using Verse;

namespace Meowify
{
    /// <summary>
    /// 设置界面与提示的双语支持：英文界面下把中文 UI 字符串换成英文，缺词条回退中文。
    /// 只在语言切换时变化，游戏本身要求重启语言，所以结果缓存。
    /// </summary>
    public static class MeowifyText
    {
        private static bool? cachedEnglish;

        /// <summary>当前界面语言是否为英文。</summary>
        public static bool IsEnglishUI()
        {
            if (cachedEnglish.HasValue)
            {
                return cachedEnglish.Value;
            }
            bool english = false;
            try
            {
                LoadedLanguage language = LanguageDatabase.activeLanguage;
                if (language == null || string.IsNullOrEmpty(language.folderName))
                {
                    // 模组构造阶段语言还没选好，先返回 false 但不缓存，等语言加载后再判定。
                    return false;
                }
                english = language.folderName.StartsWith("English", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception)
            {
                english = false;
            }
            cachedEnglish = english;
            return english;
        }

        /// <summary>英文界面下取英文翻译，否则原样返回中文。</summary>
        public static string L(string chinese)
        {
            if (chinese == null || !IsEnglishUI())
            {
                return chinese;
            }
            string english;
            if (English != null && English.TryGetValue(chinese, out english))
            {
                return english;
            }
            return chinese;
        }

        private static readonly Dictionary<string, string> English = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "你的殖民者发现了一片富含钢铁的矿脉。我们发财了！", "Your colonists found a vein rich in steel. We're rich!" },
            { "我刚刚做了一把迷你炮塔。", "I just built a mini-turret." },
            { "敌人正在接近，准备战斗！", "Enemies are closing in - ready for a fight!" },
            { "预设", "Presets" },
            { "句尾", "Sentence" },
            { "人称", "Pronouns" },
            { "颜文字", "Kaomoji" },
            { "范围", "Scope" },
            { "高级", "Advanced" },
            { " 已加载，全场开始喵化。喵。", " loaded - meowifying everything. Meow." },
            { "[Meowify] 样例: ", "[Meowify] Sample: " },
            { "你的殖民者发现了一片富含钢铁的矿脉。", "Your colonists found a vein rich in steel." },
            { "喵化世界 Meowify", "Meowify" },
            { "恢复默认", "Reset to defaults" },
            { "改动即时生效，关闭窗口时自动保存。喵化世界 Meowify v", "Changes apply instantly and save when this window closes. Meowify v" },
            { "一键预设", "Presets" },
            { "小喵一声：只加句尾喵，改动最小，适合怕吵的玩家。", "A Tiny Meow: sentence-ending word only - the smallest change." },
            { "猫娘日常：句尾喵 + 我→本喵 / 你→主人 + 长句颜文字，QQ喵喵助手同款。", "Catgirl Daily: sentence-ending meow + I→this cat / you→master + kaomoji on longer text." },
            { "喵力全开：句尾词 喵/喵呜/喵～、最短字数 1、长句必加颜文字。", "Meow Overload: meow/mrrp/nya~ endings, 1-char minimum, kaomoji on every long line." },
            { "小喵一声", "A Tiny Meow" },
            { "猫娘日常", "Catgirl Daily" },
            { "喵力全开", "Meow Overload" },
            { "效果预览", "Preview" },
            { "想要更细的控制，用上面的标签页：句尾、人称、颜文字、范围、高级。", "For finer control use the tabs above: Sentence, Pronouns, Kaomoji, Scope, Advanced." },
            { "句尾语气词", "Sentence-ending word" },
            { "句尾加语气词", "Add a sentence-ending word" },
            { "按中英文标点断句，在句末标点前插入句尾词；没有标点时补在行尾。", "Splits on CJK and English punctuation and inserts the word before the final mark; lines without punctuation get it appended." },
            { "最短喵化字数", "Minimum visible characters" },
            { "少于这个可见字数的文本不加句尾词，避免把按钮、图标说明撑破。", "Text shorter than this gets no ending word, so buttons and icon labels stay intact." },
            { "加喵概率 %", "Chance to add the word %" },
            { "100 = 每句都加；调低后只给一部分句子加喵。按原文哈希决定，同一句结果固定。", "100 = every sentence; lower values only add it sometimes. Decided by a hash of the original text, so the same sentence always behaves the same." },
            { "句尾词候选（每行一个，随机挑一个；留空 = 不加）：", "Ending words (one per line, one is picked; empty = none):" },
            { "重复句尾词", "Repeated ending words" },
            { "折叠连续句尾词", "Collapse repeated ending words" },
            { "开：句末出现「喵喵」「喵呜喵呜」时折叠成一个，再接后缀。", "On: trailing 喵喵 / 喵呜喵呜 collapse to one before the suffix is added." },
            { "折叠后的后缀：", "Suffix after collapsing:" },
            { "折叠后接的符号，默认 ~，于是「喵喵。」变成「喵~。」。留空就不加后缀。", "Appended after a collapse (default ~, so 喵喵。 becomes 喵~。). Empty = no suffix." },
            { "人称替换", "Pronoun replacement" },
            { "替换人称代词", "Replace pronouns" },
            { "每个代词都可以单独设定；留空 = 这个词不替换。", "Every pronoun can be set separately; empty = that word is left alone." },
            { "已关闭人称替换。", "Pronoun replacement is off." },
            { "第一人称", "First person" },
            { "我 →", "我 →" },
            { "默认：本喵。留空 = 不替换「我」。", "Default: 本喵. Empty = 我 is not replaced." },
            { "我们 →", "我们 →" },
            { "留空 = 自动用「我」的替换词 + 们。", "Empty = the 我 replacement plus a plural suffix." },
            { "第二人称", "Second person" },
            { "你 →", "你 →" },
            { "默认：主人。留空 = 不替换「你」。", "Default: 主人. Empty = 你 is not replaced." },
            { "你们 →", "你们 →" },
            { "留空 = 自动用「你」的替换词 + 们。", "Empty = the 你 replacement plus a plural suffix." },
            { "第三人称（默认留空 = 保持原样）", "Third person (empty by default = unchanged)" },
            { "他 →", "他 →" },
            { "留空 = 不替换「他」。", "Empty = 他 is not replaced." },
            { "他们 →", "他们 →" },
            { "留空 = 自动用「他」的替换词 + 们。", "Empty = the 他 replacement plus a plural suffix." },
            { "她 →", "她 →" },
            { "留空 = 不替换「她」。", "Empty = 她 is not replaced." },
            { "她们 →", "她们 →" },
            { "留空 = 自动用「她」的替换词 + 们。", "Empty = the 她 replacement plus a plural suffix." },
            { "它 →", "它 →" },
            { "留空 = 不替换「它」。", "Empty = 它 is not replaced." },
            { "它们 →", "它们 →" },
            { "留空 = 自动用「它」的替换词 + 们。", "Empty = the 它 replacement plus a plural suffix." },
            { "自定义替换表", "Custom replacement table" },
            { "每行一个，格式「原词=替换词」：", "One per line, format word=replacement:" },
            { "例如：俺=本喵、咱=本喵、阁下=主人。会和上面的内置项一起生效。", "For example 俺=本喵, 咱=本喵, 阁下=主人. These stack with the built-ins above." },
            { "保护词：含有「我 / 你 / 他」但不是代词的词（迷你、自我、其他、吉他…）会整体跳过，不会被拆开。", "Protected words: words containing 我 / 你 / 他 that are not pronouns (迷你, 自我, 其他, 吉他...) are skipped whole and never split." },
            { "句末颜文字", "Trailing kaomoji" },
            { "长句追加颜文字", "Add kaomoji to longer text" },
            { "可见字数达到阈值、且含句读的文本，末尾按概率追加一个猫咪颜文字。", "Text that is long enough and has sentence punctuation gets a cat kaomoji appended by chance." },
            { "触发概率 %", "Chance %" },
            { "100 = 每句都加；0 = 不加。按原文哈希决定，同一句结果固定。", "100 = always; 0 = never. Decided by a hash of the original text, so results are stable." },
            { "颜文字最短字数", "Minimum characters for kaomoji" },
            { "全文可见字符数达到这个长度才会追加颜文字。", "Only text at least this long gets a kaomoji." },
            { "按语境挑颜文字", "Pick kaomoji by context" },
            { "战斗、难过、食物、动物、恋爱、社交、工作各有专属池，其余用可爱池。", "Combat, sad, food, animal, love, social and work each have their own pool; everything else uses the cute pool." },
            { "内置语境池：战斗 / 难过 / 食物 / 动物 / 恋爱 / 社交 / 工作 / 可爱。", "Built-in pools: combat / sad / food / animal / love / social / work / cute." },
            { "额外颜文字（每行一个，会加进候选池）：", "Extra kaomoji (one per line, added to the pool):" },
            { "内置池来自社区颜文字合集（kaomoji-collection，MIT）；这里填的会额外加进去。", "The built-in pools come from the kaomoji-collection community set (MIT); anything here is added on top." },
            { "适用范围（关掉哪一层，哪一层就保持原样）", "Scope (turn a layer off and it stays untouched)" },
            { "界面文本（按钮 / 提示 / 事件弹窗）", "Interface text (buttons / tips / event popups)" },
            { "语言包 Keyed 文本。关掉后按钮、提示、菜单保持原语言。", "Language Keyed text. Off = buttons, tips and menus stay as-is." },
            { "定义注入文本（物品 / 技能 / 特性描述）", "Def-injected text (items / abilities / traits)" },
            { "DefInjected 文本。关掉后物品名、描述、特性说明不会被喵化。", "DefInjected text. Off = item names, descriptions and trait text stay as-is." },
            { "语法生成句子（事件 / 社交 / 战斗日志）", "Grammar-generated sentences (events / social / combat logs)" },
            { "GrammarResolver 生成的句子。关掉后事件、社交、战斗日志保持原样。", "Sentences produced by GrammarResolver. Off = event, social and combat logs stay as-is." },
            { "定义字段兜底（未翻译的 def 文本）", "Def field fallback (untranslated def text)" },
            { "直接改 def 的 label/description 与背景故事。关掉后未翻译或代码生成的文本不会喵化。", "Edits def label/description and backstories directly. Off = untranslated or code-generated text stays as-is." },
            { "给名字也加喵（小人 / 动物 / 武器）", "Meowify names too (pawns / animals / weapons)" },
            { "默认关：名字由语法生成，加了喵会很怪。开：连名字一起喵。", "Off by default: names come from grammar and a meow would look odd. On: names meow too." },
            { "关键词过滤", "Keyword filter" },
            { "只喵这些词（每行一个，留空 = 全部）：", "Only meowify text containing these words (one per line, empty = all):" },
            { "白名单：填了就只处理包含这些词的文本（例如只填「殖民者」）。", "Whitelist: when set, only text containing one of these words is processed." },
            { "不要喵这些词（每行一个）：", "Never meowify text containing these words (one per line):" },
            { "黑名单：包含这些词的文本整段原样返回，优先级高于白名单。", "Blacklist: text containing one of these is returned untouched, and it wins over the whitelist." },
            { "保护词与排除片段", "Protected words and excluded phrases" },
            { "额外保护词（每行一个）：", "Extra protected words (one per line):" },
            { "含有「我 / 你 / 他」但不是人称代词的词，替换前会整体跳过。", "Words containing 我 / 你 / 他 that are not pronouns are skipped before replacement." },
            { "内置：迷你、自我、忘我、无我、敌我、你我、其他、其它、吉他…", "Built-in: 迷你, 自我, 忘我, 无我, 敌我, 你我, 其他, 其它, 吉他..." },
            { "排除片段（每行一个，命中就整段不喵化）：", "Excluded phrases (one per line; a hit leaves the whole text untouched):" },
            { "给个别模组的特殊文本留后门：文本里只要出现这些片段，就原样返回，不加喵也不替换人称。", "An escape hatch for other mods' special text: if any phrase appears, the text is returned as-is - no meow and no pronoun swap." },
            { "喵化世界 Meowify v", "Meowify v" },
            { " · 运行时改文本，不修改任何翻译文件。", " · rewrites text at runtime, no translation files touched." },
            { "喵\n喵呜\n喵～", "meow\nmrrp\nnya~" },
            { "英文句尾词（每行一个，仅英文界面生效）：", "English ending words (one per line, only used in an English UI):" },
            { "英文界面下句尾词会自动带上前导空格，例如 Hello. → Hello meow.", "In an English UI the ending word gets a leading space, e.g. Hello. → Hello meow." },
        };
    }
}
