using System;
using UnityEngine;
using Verse;

namespace Meowify
{
    /// <summary>
    /// 模组入口：Mod 构造时（早于 def 加载与语言注入）装上 Harmony 补丁，并载入设置。
    /// 设置界面用 Listing_Standard + 滚动视图，分预设 / 句尾 / 人称 / 颜文字 / 高级 / 预览几块。
    /// </summary>
    public class MeowifyMod : Mod
    {
        /// <summary>与 About\About.xml 的 modVersion、csproj 的 Version 保持一致。</summary>
        public const string Version = "1.2.0";

        public static MeowSettings Settings;

        private static readonly string[] PreviewSamples =
        {
            "你的殖民者发现了一片富含钢铁的矿脉。我们发财了！",
            "我刚刚做了一把迷你炮塔。",
            "敌人正在接近，准备战斗！"
        };

        private Vector2 scrollPosition;

        private enum Preset
        {
            Light,
            Standard,
            Fever
        }

        public MeowifyMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<MeowSettings>();
            ApplySettings();
            HarmonyPatches.Initialize();
            Log.Message("[Meowify] v" + Version + " 已加载，全场开始喵化。喵。");

            if (Prefs.DevMode)
            {
                Log.Message("[Meowify] 样例: " + Meowifier.Meowify("你的殖民者发现了一片富含钢铁的矿脉。"));
            }
        }

        public static void ApplySettings()
        {
            Meowifier.Options = Settings.ToOptions();
            Meowifier.ClearCache();
        }

        public override string SettingsCategory()
        {
            return "喵化世界 Meowify";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            bool changed = false;

            Rect viewRect = new Rect(0f, 0f, inRect.width - 24f, 1620f);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            Text.WordWrap = true;
            GUI.color = Color.white;

            // ---------------------------------------------------------------- 预设
            Section(listing, "一键预设");
            Rect presetRow = listing.GetRect(32f);
            float buttonWidth = (presetRow.width - 12f) / 3f;
            Rect lightRect = new Rect(presetRow.x, presetRow.y, buttonWidth, 30f);
            Rect standardRect = new Rect(presetRow.x + buttonWidth + 6f, presetRow.y, buttonWidth, 30f);
            Rect feverRect = new Rect(presetRow.x + (buttonWidth + 6f) * 2f, presetRow.y, buttonWidth, 30f);
            TooltipHandler.TipRegion(lightRect, "小喵一声：只加句尾喵，改动最小，适合怕吵的玩家。");
            TooltipHandler.TipRegion(standardRect, "猫娘日常：句尾喵 + 我→本喵 / 你→主人 + 长句颜文字，QQ喵喵助手同款。");
            TooltipHandler.TipRegion(feverRect, "喵力全开：句尾词 喵/喵呜/喵～、最短字数 1、长句必加颜文字。");
            if (Widgets.ButtonText(lightRect, "小喵一声：只加句尾喵"))
            {
                ApplyPreset(Preset.Light);
                ApplySettings();
                changed = true;
            }
            if (Widgets.ButtonText(standardRect, "猫娘日常：QQ喵喵助手"))
            {
                ApplyPreset(Preset.Standard);
                ApplySettings();
                changed = true;
            }
            if (Widgets.ButtonText(feverRect, "喵力全开：颜文字拉满"))
            {
                ApplyPreset(Preset.Fever);
                ApplySettings();
                changed = true;
            }
            listing.Gap(4f);

            // ---------------------------------------------------------------- 句尾
            Section(listing, "① 句尾语气词");
            changed |= Checkbox(listing, "句尾加语气词", ref Settings.SentenceEnd,
                "按中英文标点断句，在句末标点前插入句尾词；没有标点时补在行尾。");
            changed |= Slider(listing, "最短喵化字数", ref Settings.MinChars, 1f, 8f, 0.7f,
                "少于这个可见字数的文本不加句尾词，避免把按钮、图标说明撑破。");
            changed |= Slider(listing, "加喵概率 %", ref Settings.SentenceEndChance, 0f, 100f, 0.7f,
                "100 = 每句都加；调低后只给一部分句子加喵。按原文哈希决定，同一句结果固定。");
            changed |= Entry(listing, "句尾词候选（每行一个，随机挑一个；留空 = 不加）：",
                ref Settings.EndWordsRaw, EntryLines(Settings.EndWordsRaw, 2, 6));
            listing.Gap(6f);

            // ---------------------------------------------------------------- 人称
            Section(listing, "② 人称替换");
            changed |= Checkbox(listing, "替换人称代词", ref Settings.PronounSwap,
                "默认「我 → 本喵」「你 → 主人」，我们/你们 会一起变成复数形式。");
            if (Settings.PronounSwap)
            {
                changed |= TextRow(listing, "第一人称（我 →）：", ref Settings.SelfPronoun,
                    "留空表示不替换「我」。默认：本喵");
                changed |= TextRow(listing, "第二人称（你 →）：", ref Settings.OtherPronoun,
                    "留空表示不替换「你」。默认：主人");
            }
            listing.Gap(6f);

            // ---------------------------------------------------------------- 颜文字
            Section(listing, "③ 句末颜文字");
            changed |= Checkbox(listing, "长句追加颜文字", ref Settings.Kaomoji,
                "可见字数达到阈值、且含句读的文本，末尾按概率追加一个猫咪颜文字。");
            if (Settings.Kaomoji)
            {
                changed |= Slider(listing, "触发概率 %", ref Settings.KaomojiChance, 0f, 100f, 0.7f,
                    "100 = 每句都加；0 = 不加。按原文哈希决定，同一句结果固定。");
                changed |= Slider(listing, "颜文字最短字数", ref Settings.KaomojiMinChars, 4f, 40f, 0.7f,
                    "全文可见字符数达到这个长度才会追加颜文字。");
                changed |= Checkbox(listing, "按语境挑颜文字", ref Settings.KaomojiContextual,
                    "开：战斗/社交/食物/动物/工作各有专属颜文字池，内置可爱池兜底。关：只从下面的候选里挑。");
                changed |= Entry(listing, "额外颜文字（每行一个，留空就用内置池）：",
                    ref Settings.KaomojiListRaw, EntryLines(Settings.KaomojiListRaw, 3, 8));
            }
            listing.Gap(6f);

            // ---------------------------------------------------------------- 预览
            listing.Gap(10f);
            Section(listing, "预览");
            for (int i = 0; i < PreviewSamples.Length; i++)
            {
                string preview = Meowifier.Meowify(PreviewSamples[i]);
                listing.Label("· " + preview, 60f);
            }

            // ---------------------------------------------------------------- 适用范围
            Section(listing, "④ 适用范围（关掉哪一层，哪一层就保持原样）");
            changed |= Checkbox(listing, "界面文本（按钮 / 提示 / 事件弹窗）", ref Settings.ChannelKeyed,
                "语言包 Keyed 文本。关掉后按钮、提示、菜单保持原语言。");
            changed |= Checkbox(listing, "定义注入文本（物品 / 技能 / 特性描述）", ref Settings.ChannelDefInjected,
                "DefInjected 文本。关掉后物品名、描述、特性说明不会被喵化。");
            changed |= Checkbox(listing, "语法生成句子（事件 / 社交 / 战斗日志）", ref Settings.ChannelGrammar,
                "GrammarResolver 生成的句子。关掉后事件、社交、战斗日志保持原样。");
            changed |= Checkbox(listing, "定义字段兜底（未翻译的 def 文本）", ref Settings.ChannelDefSweep,
                "直接改 def 的 label/description 与背景故事。关掉后未翻译或代码生成的文本不会喵化。");
            changed |= Checkbox(listing, "给名字也加喵（小人 / 动物 / 武器）", ref Settings.IncludeNames,
                "默认关：名字由语法生成，加了喵会很怪。开：连名字一起喵。");
            listing.Gap(6f);

            // ---------------------------------------------------------------- 名字与重复
            Section(listing, "⑤ 重复与关键词");
            changed |= Checkbox(listing, "折叠连续句尾词", ref Settings.CollapseRepeated,
                "开：句末出现「喵喵」「喵呜喵呜」时折叠成一个，再接后缀。");
            changed |= TextRow(listing, "折叠后的后缀：", ref Settings.CollapseSuffix,
                "折叠后接的符号，默认 ~，于是「喵喵。」变成「喵~。」。留空就不加后缀。");
            changed |= Entry(listing, "只喵这些词（每行一个，留空 = 全部）：", ref Settings.IncludeKeywordsRaw, 3);
            listing.Label("白名单：填了就只处理包含这些词的文本（例如只填「殖民者」）。");
            changed |= Entry(listing, "不要喵这些词（每行一个）：", ref Settings.ExcludeKeywordsRaw, 3);
            listing.Label("黑名单：包含这些词的文本整段原样返回，优先级高于白名单。");
            listing.Gap(6f);

            // ---------------------------------------------------------------- 高级
            Section(listing, "⑥ 高级");
            changed |= Entry(listing, "额外保护词（每行一个）：", ref Settings.ExtraProtectedWordsRaw, 3);
            listing.Label("含有「我 / 你」但不是人称代词的词，替换前会整体跳过。" +
                          "内置：迷你、自我、忘我、无我、敌我、你我…");
            listing.Gap(4f);
            changed |= Entry(listing, "排除片段（每行一个，命中就整段不喵化）：", ref Settings.ExcludedPhrasesRaw, 3);
            listing.Label("给个别模组的特殊文本留后门：文本里只要出现这些片段，就原样返回，不加喵也不替换人称。");

            // ---------------------------------------------------------------- 底部
            listing.Gap(12f);
            if (listing.ButtonText("恢复默认"))
            {
                Settings.ResetToDefaults();
                ApplySettings();
                changed = true;
            }
            listing.Label("改动即时生效，关闭窗口时自动保存。喵化世界 Meowify v" + Version);

            listing.End();
            Widgets.EndScrollView();

            if (changed)
            {
                ApplySettings();
                Settings.Write();
            }
        }


        private static void Section(Listing_Standard listing, string title)
        {
            listing.Gap(10f);
            GameFont previous = Text.Font;
            Text.Font = GameFont.Medium;
            listing.Label(title);
            Text.Font = previous;
            listing.GapLine(4f);
        }

        private static bool Checkbox(Listing_Standard listing, string label, ref bool value, string tooltip)
        {
            bool before = value;
            listing.CheckboxLabeled(label, ref value, tooltip);
            return before != value;
        }

        private static bool Slider(Listing_Standard listing, string label, ref int value, float min, float max,
            float labelPct, string tooltip)
        {
            float result = listing.SliderLabeled(label + "：" + value, value, min, max, labelPct, tooltip);
            int rounded = Mathf.RoundToInt(result);
            if (rounded != value)
            {
                value = rounded;
                return true;
            }
            return false;
        }

        private static bool TextRow(Listing_Standard listing, string label, ref string value, string tooltip)
        {
            Rect row = listing.GetRect(30f);
            Widgets.Label(new Rect(row.x, row.y, row.width * 0.4f, row.height), label);
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(row, tooltip);
            }
            string edited = Widgets.TextField(new Rect(row.x + row.width * 0.4f, row.y, row.width * 0.6f, 28f), value ?? string.Empty);
            if (!string.Equals(edited, value, StringComparison.Ordinal))
            {
                value = edited;
                return true;
            }
            return false;
        }

        private static int EntryLines(string value, int min, int max)
        {
            if (string.IsNullOrEmpty(value))
            {
                return min;
            }
            int lines = 0;
            foreach (string line in value.Split('\n'))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    lines++;
                }
            }
            return Mathf.Clamp(lines + 1, min, max);
        }

        private static bool Entry(Listing_Standard listing, string label, ref string value, int lineCount)
        {
            listing.Label(label);
            string edited = listing.TextEntry(value ?? string.Empty, lineCount);
            if (!string.Equals(edited, value, StringComparison.Ordinal))
            {
                value = edited;
                return true;
            }
            return false;
        }

        private void ApplyPreset(Preset preset)
        {
            switch (preset)
            {
                case Preset.Light:
                    Settings.SentenceEnd = true;
                    Settings.EndWordsRaw = MeowSettings.DefaultEndWords;
                    Settings.MinChars = 2;
                    Settings.SentenceEndChance = 100;
                    Settings.PronounSwap = false;
                    Settings.Kaomoji = false;
                    break;

                case Preset.Standard:
                    Settings.SentenceEnd = true;
                    Settings.EndWordsRaw = MeowSettings.DefaultEndWords;
                    Settings.MinChars = 2;
                    Settings.SentenceEndChance = 100;
                    Settings.PronounSwap = true;
                    Settings.SelfPronoun = MeowSettings.DefaultSelfPronoun;
                    Settings.OtherPronoun = MeowSettings.DefaultOtherPronoun;
                    Settings.Kaomoji = true;
                    Settings.KaomojiMinChars = 10;
                    Settings.KaomojiChance = 100;
                    Settings.KaomojiListRaw = MeowSettings.DefaultKaomojiList;
                    break;

                case Preset.Fever:
                    Settings.SentenceEnd = true;
                    Settings.EndWordsRaw = "喵\n喵呜\n喵～";
                    Settings.MinChars = 1;
                    Settings.SentenceEndChance = 100;
                    Settings.PronounSwap = true;
                    Settings.SelfPronoun = MeowSettings.DefaultSelfPronoun;
                    Settings.OtherPronoun = MeowSettings.DefaultOtherPronoun;
                    Settings.Kaomoji = true;
                    Settings.KaomojiMinChars = 5;
                    Settings.KaomojiChance = 100;
                    Settings.KaomojiListRaw = MeowSettings.DefaultKaomojiList;
                    break;
            }

            // 1.3 新增项：所有预设都用推荐默认（通道全开、名字不喵、折叠重复、语境颜文字）
            Settings.ChannelKeyed = true;
            Settings.ChannelDefInjected = true;
            Settings.ChannelGrammar = true;
            Settings.ChannelDefSweep = true;
            Settings.IncludeNames = false;
            Settings.CollapseRepeated = true;
            Settings.CollapseSuffix = MeowSettings.DefaultCollapseSuffix;
            Settings.KaomojiContextual = true;
        }
    }
}
