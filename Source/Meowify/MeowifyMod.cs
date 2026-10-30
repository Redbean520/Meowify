using System;
using UnityEngine;
using Verse;

namespace Meowify
{
    /// <summary>
    /// 模组入口：Mod 构造时（早于 def 加载与语言注入）装上 Harmony 补丁，并载入设置。
    /// 设置界面改成标签页布局：预设 / 句尾 / 人称 / 颜文字 / 范围 / 高级，每页独立滚动。
    /// </summary>
    public class MeowifyMod : Mod
    {
        /// <summary>与 About\About.xml 的 modVersion、csproj 的 Version 保持一致。</summary>
        public const string Version = "1.3.0";

        public static MeowSettings Settings;

        private static string[] PreviewSamples
        {
            get
            {
                return new[]
                {
                    MeowifyText.L("你的殖民者发现了一片富含钢铁的矿脉。我们发财了！"),
                    MeowifyText.L("我刚刚做了一把迷你炮塔。"),
                    MeowifyText.L("敌人正在接近，准备战斗！")
                };
            }
        }

        private static string[] TabLabels
        {
            get
            {
                return new[] { MeowifyText.L("预设"), MeowifyText.L("句尾"), MeowifyText.L("人称"), MeowifyText.L("颜文字"), MeowifyText.L("范围"), MeowifyText.L("高级") };
            }
        }

        private enum SettingsTab
        {
            Presets = 0,
            Sentence = 1,
            Pronouns = 2,
            Kaomoji = 3,
            Scope = 4,
            Advanced = 5
        }

        private int currentTab;
        private readonly Vector2[] scrollPositions = new Vector2[TabLabels.Length];

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
            Log.Message("[Meowify] v" + Version + MeowifyText.L(" 已加载，全场开始喵化。喵。"));

            if (Prefs.DevMode)
            {
                Log.Message(MeowifyText.L("[Meowify] 样例: ") + Meowifier.Meowify(MeowifyText.L("你的殖民者发现了一片富含钢铁的矿脉。")));
            }
        }

        public static void ApplySettings()
        {
            Meowifier.Options = Settings.ToOptions();
            Meowifier.Options.EnglishMode = MeowifyText.IsEnglishUI();
            Meowifier.ClearCache();
        }

        public override string SettingsCategory()
        {
            return MeowifyText.L("喵化世界 Meowify");
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            bool changed = false;

            // ---------------------------------------------------------------- 头部
            GameFont previousFont = Text.Font;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width - 160f, 30f), MeowifyText.L("喵化世界 Meowify"));
            Text.Font = GameFont.Small;
            GUI.color = new Color(1f, 0.72f, 0.82f);
            Widgets.Label(new Rect(inRect.xMax - 150f, inRect.y + 6f, 150f, 24f), "v" + Version);
            GUI.color = Color.white;

            // ---------------------------------------------------------------- 标签页
            float tabY = inRect.y + 34f;
            float gap = 4f;
            float tabWidth = (inRect.width - gap * (TabLabels.Length - 1)) / TabLabels.Length;
            for (int i = 0; i < TabLabels.Length; i++)
            {
                Rect tabRect = new Rect(inRect.x + (tabWidth + gap) * i, tabY, tabWidth, 32f);
                if (Widgets.ButtonText(tabRect, TabLabels[i]))
                {
                    currentTab = i;
                }
                if (currentTab == i)
                {
                    Color old = GUI.color;
                    GUI.color = new Color(1f, 0.55f, 0.76f);
                    GUI.DrawTexture(new Rect(tabRect.x + 4f, tabRect.yMax + 1f, tabRect.width - 8f, 3f), BaseContent.WhiteTex);
                    GUI.color = old;
                }
            }

            // ---------------------------------------------------------------- 内容
            Rect contentRect = new Rect(inRect.x, tabY + 40f, inRect.width, inRect.height - 40f - 56f);
            Rect viewRect = new Rect(0f, 0f, contentRect.width - 24f, TabContentHeight());
            Widgets.BeginScrollView(contentRect, ref scrollPositions[currentTab], viewRect);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);
            Text.WordWrap = true;
            GUI.color = Color.white;
            Text.Font = GameFont.Small;

            switch ((SettingsTab)currentTab)
            {
                case SettingsTab.Presets:
                    DrawPresetsTab(listing, ref changed);
                    break;
                case SettingsTab.Sentence:
                    DrawSentenceTab(listing, ref changed);
                    break;
                case SettingsTab.Pronouns:
                    DrawPronounsTab(listing, ref changed);
                    break;
                case SettingsTab.Kaomoji:
                    DrawKaomojiTab(listing, ref changed);
                    break;
                case SettingsTab.Scope:
                    DrawScopeTab(listing, ref changed);
                    break;
                default:
                    DrawAdvancedTab(listing, ref changed);
                    break;
            }

            listing.End();
            Widgets.EndScrollView();
            Text.Font = previousFont;

            // ---------------------------------------------------------------- 底部固定栏
            Rect footer = new Rect(inRect.x, inRect.yMax - 44f, inRect.width, 40f);
            if (Widgets.ButtonText(new Rect(footer.x, footer.y + 4f, 180f, 32f), MeowifyText.L("恢复默认")))
            {
                Settings.ResetToDefaults();
                ApplySettings();
                changed = true;
            }
            GUI.color = new Color(0.85f, 0.85f, 0.85f);
            Widgets.Label(new Rect(footer.x + 196f, footer.y + 10f, footer.width - 196f, 24f),
                MeowifyText.L("改动即时生效，关闭窗口时自动保存。喵化世界 Meowify v") + Version);
            GUI.color = Color.white;

            if (changed)
            {
                ApplySettings();
                Settings.Write();
            }
        }

        private float TabContentHeight()
        {
            switch ((SettingsTab)currentTab)
            {
                case SettingsTab.Presets:
                    return 360f;
                case SettingsTab.Sentence:
                    return 610f;
                case SettingsTab.Pronouns:
                    return 760f;
                case SettingsTab.Kaomoji:
                    return 520f;
                case SettingsTab.Scope:
                    return 470f;
                default:
                    return 340f;
            }
        }

        // ================================================================= 预设
        private void DrawPresetsTab(Listing_Standard listing, ref bool changedRef)
        {
            bool changed = false;
            Section(listing, MeowifyText.L("一键预设"));
            Rect presetRow = listing.GetRect(34f);
            float buttonWidth = (presetRow.width - 12f) / 3f;
            Rect lightRect = new Rect(presetRow.x, presetRow.y, buttonWidth, 32f);
            Rect standardRect = new Rect(presetRow.x + buttonWidth + 6f, presetRow.y, buttonWidth, 32f);
            Rect feverRect = new Rect(presetRow.x + (buttonWidth + 6f) * 2f, presetRow.y, buttonWidth, 32f);
            TooltipHandler.TipRegion(lightRect, MeowifyText.L("小喵一声：只加句尾喵，改动最小，适合怕吵的玩家。"));
            TooltipHandler.TipRegion(standardRect, MeowifyText.L("猫娘日常：句尾喵 + 我→本喵 / 你→主人 + 长句颜文字，QQ喵喵助手同款。"));
            TooltipHandler.TipRegion(feverRect, MeowifyText.L("喵力全开：句尾词 喵/喵呜/喵～、最短字数 1、长句必加颜文字。"));
            if (Widgets.ButtonText(lightRect, MeowifyText.L("小喵一声")))
            {
                ApplyPreset(Preset.Light);
                changed = true;
            }
            if (Widgets.ButtonText(standardRect, MeowifyText.L("猫娘日常")))
            {
                ApplyPreset(Preset.Standard);
                changed = true;
            }
            if (Widgets.ButtonText(feverRect, MeowifyText.L("喵力全开")))
            {
                ApplyPreset(Preset.Fever);
                changed = true;
            }
            listing.Gap(6f);

            Section(listing, MeowifyText.L("效果预览"));
            for (int i = 0; i < PreviewSamples.Length; i++)
            {
                string preview = Meowifier.Meowify(PreviewSamples[i]);
                listing.Label("· " + preview, 60f);
            }
            listing.Gap(6f);
            listing.Label(MeowifyText.L("想要更细的控制，用上面的标签页：句尾、人称、颜文字、范围、高级。"));

            changedRef |= changed;
        }

        // ================================================================= 句尾
        private void DrawSentenceTab(Listing_Standard listing, ref bool changedRef)
        {
            bool changed = false;
            Section(listing, MeowifyText.L("句尾语气词"));
            changed |= Checkbox(listing, MeowifyText.L("句尾加语气词"), ref Settings.SentenceEnd,
                MeowifyText.L("按中英文标点断句，在句末标点前插入句尾词；没有标点时补在行尾。"));
            changed |= Slider(listing, MeowifyText.L("最短喵化字数"), ref Settings.MinChars, 1f, 8f, 0.7f,
                MeowifyText.L("少于这个可见字数的文本不加句尾词，避免把按钮、图标说明撑破。"));
            changed |= Slider(listing, MeowifyText.L("加喵概率 %"), ref Settings.SentenceEndChance, 0f, 100f, 0.7f,
                MeowifyText.L("100 = 每句都加；调低后只给一部分句子加喵。按原文哈希决定，同一句结果固定。"));
            changed |= Entry(listing, MeowifyText.L("句尾词候选（每行一个，随机挑一个；留空 = 不加）："),
                ref Settings.EndWordsRaw, EntryLines(Settings.EndWordsRaw, 2, 6));
            changed |= Entry(listing, MeowifyText.L("英文句尾词（每行一个，仅英文界面生效）："),
                ref Settings.EnglishEndWordsRaw, EntryLines(Settings.EnglishEndWordsRaw, 2, 6));
            listing.Label(MeowifyText.L("英文界面下句尾词会自动带上前导空格，例如 Hello. → Hello meow."));
            listing.Gap(6f);

            Section(listing, MeowifyText.L("重复句尾词"));
            changed |= Checkbox(listing, MeowifyText.L("折叠连续句尾词"), ref Settings.CollapseRepeated,
                MeowifyText.L("开：句末出现「喵喵」「喵呜喵呜」时折叠成一个，再接后缀。"));
            changed |= TextRow(listing, MeowifyText.L("折叠后的后缀："), ref Settings.CollapseSuffix,
                MeowifyText.L("折叠后接的符号，默认 ~，于是「喵喵。」变成「喵~。」。留空就不加后缀。"));

            changedRef |= changed;
        }

        // ================================================================= 人称
        private void DrawPronounsTab(Listing_Standard listing, ref bool changedRef)
        {
            bool changed = false;
            Section(listing, MeowifyText.L("人称替换"));
            changed |= Checkbox(listing, MeowifyText.L("替换人称代词"), ref Settings.PronounSwap,
                MeowifyText.L("每个代词都可以单独设定；留空 = 这个词不替换。"));
            if (!Settings.PronounSwap)
            {
                listing.Label(MeowifyText.L("已关闭人称替换。"));
                changedRef |= changed;
                return;
            }

            SubSection(listing, MeowifyText.L("第一人称"));
            changed |= TextRow(listing, MeowifyText.L("我 →"), ref Settings.SelfPronoun,
                MeowifyText.L("默认：本喵。留空 = 不替换「我」。"));
            changed |= TextRow(listing, MeowifyText.L("我们 →"), ref Settings.SelfPluralPronoun,
                MeowifyText.L("留空 = 自动用「我」的替换词 + 们。"));

            SubSection(listing, MeowifyText.L("第二人称"));
            changed |= TextRow(listing, MeowifyText.L("你 →"), ref Settings.OtherPronoun,
                MeowifyText.L("默认：主人。留空 = 不替换「你」。"));
            changed |= TextRow(listing, MeowifyText.L("你们 →"), ref Settings.OtherPluralPronoun,
                MeowifyText.L("留空 = 自动用「你」的替换词 + 们。"));

            SubSection(listing, MeowifyText.L("第三人称（默认留空 = 保持原样）"));
            changed |= TextRow(listing, MeowifyText.L("他 →"), ref Settings.ThirdPronounHe, MeowifyText.L("留空 = 不替换「他」。"));
            changed |= TextRow(listing, MeowifyText.L("他们 →"), ref Settings.ThirdPronounHePlural, MeowifyText.L("留空 = 自动用「他」的替换词 + 们。"));
            changed |= TextRow(listing, MeowifyText.L("她 →"), ref Settings.ThirdPronounShe, MeowifyText.L("留空 = 不替换「她」。"));
            changed |= TextRow(listing, MeowifyText.L("她们 →"), ref Settings.ThirdPronounShePlural, MeowifyText.L("留空 = 自动用「她」的替换词 + 们。"));
            changed |= TextRow(listing, MeowifyText.L("它 →"), ref Settings.ThirdPronounIt, MeowifyText.L("留空 = 不替换「它」。"));
            changed |= TextRow(listing, MeowifyText.L("它们 →"), ref Settings.ThirdPronounItPlural, MeowifyText.L("留空 = 自动用「它」的替换词 + 们。"));

            Section(listing, MeowifyText.L("自定义替换表"));
            changed |= Entry(listing, MeowifyText.L("每行一个，格式「原词=替换词」："), ref Settings.CustomPronounsRaw, 3);
            listing.Label(MeowifyText.L("例如：俺=本喵、咱=本喵、阁下=主人。会和上面的内置项一起生效。"));
            listing.Gap(4f);
            listing.Label(MeowifyText.L("保护词：含有「我 / 你 / 他」但不是代词的词（迷你、自我、其他、吉他…）会整体跳过，不会被拆开。"));

            changedRef |= changed;
        }

        // ================================================================= 颜文字
        private void DrawKaomojiTab(Listing_Standard listing, ref bool changedRef)
        {
            bool changed = false;
            Section(listing, MeowifyText.L("句末颜文字"));
            changed |= Checkbox(listing, MeowifyText.L("长句追加颜文字"), ref Settings.Kaomoji,
                MeowifyText.L("可见字数达到阈值、且含句读的文本，末尾按概率追加一个猫咪颜文字。"));
            if (Settings.Kaomoji)
            {
                changed |= Slider(listing, MeowifyText.L("触发概率 %"), ref Settings.KaomojiChance, 0f, 100f, 0.7f,
                    MeowifyText.L("100 = 每句都加；0 = 不加。按原文哈希决定，同一句结果固定。"));
                changed |= Slider(listing, MeowifyText.L("颜文字最短字数"), ref Settings.KaomojiMinChars, 4f, 40f, 0.7f,
                    MeowifyText.L("全文可见字符数达到这个长度才会追加颜文字。"));
                changed |= Checkbox(listing, MeowifyText.L("按语境挑颜文字"), ref Settings.KaomojiContextual,
                    MeowifyText.L("战斗、难过、食物、动物、恋爱、社交、工作各有专属池，其余用可爱池。"));
                if (Settings.KaomojiContextual)
                {
                    listing.Label(MeowifyText.L("内置语境池：战斗 / 难过 / 食物 / 动物 / 恋爱 / 社交 / 工作 / 可爱。"));
                }
                changed |= Entry(listing, MeowifyText.L("额外颜文字（每行一个，会加进候选池）："),
                    ref Settings.KaomojiListRaw, EntryLines(Settings.KaomojiListRaw, 3, 8));
                listing.Label(MeowifyText.L("内置池来自社区颜文字合集（kaomoji-collection，MIT）；这里填的会额外加进去。"));
            }

            changedRef |= changed;
        }

        // ================================================================= 范围
        private void DrawScopeTab(Listing_Standard listing, ref bool changedRef)
        {
            bool changed = false;
            Section(listing, MeowifyText.L("适用范围（关掉哪一层，哪一层就保持原样）"));
            changed |= Checkbox(listing, MeowifyText.L("界面文本（按钮 / 提示 / 事件弹窗）"), ref Settings.ChannelKeyed,
                MeowifyText.L("语言包 Keyed 文本。关掉后按钮、提示、菜单保持原语言。"));
            changed |= Checkbox(listing, MeowifyText.L("定义注入文本（物品 / 技能 / 特性描述）"), ref Settings.ChannelDefInjected,
                MeowifyText.L("DefInjected 文本。关掉后物品名、描述、特性说明不会被喵化。"));
            changed |= Checkbox(listing, MeowifyText.L("语法生成句子（事件 / 社交 / 战斗日志）"), ref Settings.ChannelGrammar,
                MeowifyText.L("GrammarResolver 生成的句子。关掉后事件、社交、战斗日志保持原样。"));
            changed |= Checkbox(listing, MeowifyText.L("定义字段兜底（未翻译的 def 文本）"), ref Settings.ChannelDefSweep,
                MeowifyText.L("直接改 def 的 label/description 与背景故事。关掉后未翻译或代码生成的文本不会喵化。"));
            changed |= Checkbox(listing, MeowifyText.L("给名字也加喵（小人 / 动物 / 武器）"), ref Settings.IncludeNames,
                MeowifyText.L("默认关：名字由语法生成，加了喵会很怪。开：连名字一起喵。"));
            listing.Gap(6f);

            Section(listing, MeowifyText.L("关键词过滤"));
            changed |= Entry(listing, MeowifyText.L("只喵这些词（每行一个，留空 = 全部）："), ref Settings.IncludeKeywordsRaw, 3);
            listing.Label(MeowifyText.L("白名单：填了就只处理包含这些词的文本（例如只填「殖民者」）。"));
            changed |= Entry(listing, MeowifyText.L("不要喵这些词（每行一个）："), ref Settings.ExcludeKeywordsRaw, 3);
            listing.Label(MeowifyText.L("黑名单：包含这些词的文本整段原样返回，优先级高于白名单。"));

            changedRef |= changed;
        }

        // ================================================================= 高级
        private void DrawAdvancedTab(Listing_Standard listing, ref bool changedRef)
        {
            bool changed = false;
            Section(listing, MeowifyText.L("保护词与排除片段"));
            changed |= Entry(listing, MeowifyText.L("额外保护词（每行一个）："), ref Settings.ExtraProtectedWordsRaw, 3);
            listing.Label(MeowifyText.L("含有「我 / 你 / 他」但不是人称代词的词，替换前会整体跳过。") +
                          MeowifyText.L("内置：迷你、自我、忘我、无我、敌我、你我、其他、其它、吉他…"));
            listing.Gap(4f);
            changed |= Entry(listing, MeowifyText.L("排除片段（每行一个，命中就整段不喵化）："), ref Settings.ExcludedPhrasesRaw, 3);
            listing.Label(MeowifyText.L("给个别模组的特殊文本留后门：文本里只要出现这些片段，就原样返回，不加喵也不替换人称。"));
            listing.Gap(8f);
            listing.Label(MeowifyText.L("喵化世界 Meowify v") + Version + MeowifyText.L(" · 运行时改文本，不修改任何翻译文件。"));

            changedRef |= changed;
        }

        // ================================================================= 小工具
        private static void Section(Listing_Standard listing, string title)
        {
            listing.Gap(8f);
            GameFont previous = Text.Font;
            Text.Font = GameFont.Medium;
            listing.Label(title);
            Text.Font = previous;
            listing.GapLine(4f);
        }

        private static void SubSection(Listing_Standard listing, string title)
        {
            listing.Gap(6f);
            GameFont previous = Text.Font;
            Text.Font = GameFont.Small;
            GUI.color = new Color(1f, 0.72f, 0.82f);
            listing.Label(title);
            GUI.color = Color.white;
            Text.Font = previous;
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
            Widgets.Label(new Rect(row.x, row.y, row.width * 0.3f, row.height), label);
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(row, tooltip);
            }
            string edited = Widgets.TextField(new Rect(row.x + row.width * 0.3f, row.y, row.width * 0.7f, 28f), value ?? string.Empty);
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
                    Settings.EnglishEndWordsRaw = MeowSettings.DefaultEnglishEndWords;
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
                    Settings.SelfPluralPronoun = string.Empty;
                    Settings.OtherPronoun = MeowSettings.DefaultOtherPronoun;
                    Settings.OtherPluralPronoun = string.Empty;
                    Settings.Kaomoji = true;
                    Settings.KaomojiMinChars = 10;
                    Settings.KaomojiChance = 100;
                    break;

                case Preset.Fever:
                    Settings.SentenceEnd = true;
                    Settings.EndWordsRaw = "喵\n喵呜\n喵～";
                    Settings.EnglishEndWordsRaw = "meow\nmrrp\nnya~";
                    Settings.MinChars = 1;
                    Settings.SentenceEndChance = 100;
                    Settings.PronounSwap = true;
                    Settings.SelfPronoun = MeowSettings.DefaultSelfPronoun;
                    Settings.SelfPluralPronoun = string.Empty;
                    Settings.OtherPronoun = MeowSettings.DefaultOtherPronoun;
                    Settings.OtherPluralPronoun = string.Empty;
                    Settings.Kaomoji = true;
                    Settings.KaomojiMinChars = 5;
                    Settings.KaomojiChance = 100;
                    break;
            }

            // 所有预设都用推荐默认（通道全开、名字不喵、折叠重复、语境颜文字）
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
