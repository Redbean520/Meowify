# Steam 工坊发布文案（中英合一，可直接复制）

> 当前发布：**1.3.1**（已打包：`release\Meowify\Meowify-1.3.1\` 与 `Meowify-1.3.1.zip`，910,384 B，含 `About/PublishedFileId.txt`）｜ 目标：RimWorld 1.7 ｜ 依赖：Harmony（brrainz.harmony）
> 更新日期：2026-10-31（1.3.1：人称十项默认全部填好、第三人称改为猫娘称呼「那位 / 那几位 / 那只喵」；中英文案合并到本文档）
>
> 格式说明：工坊描述只认 **Steam BBCode**，不认 Markdown / HTML。正文按技能要求写：不用 `[list]` / `[*]` / `[img]` / `[quote]` / `[code]`，标题从 `[h2]` 起，行首 `- ` 当子弹点，标签全部成对（`[hr]` 单独用），不写更新日志。
> 中文文案在 ①–⑧，英文文案在 ⑨–⑬；工坊页面按语言贴对应那份，也可以中文正文末尾附英文短段。

---

# 一、中文

## ① 标题

```text
喵化世界 Meowify
```

备用（带英文）：
```text
喵化世界 Meowify — 让 RimWorld 里的每句话都会喵
```

## ② 摘要 / 列表短描述

```text
给 RimWorld 1.7 的全部文本加一声「喵」：句尾喵、十个人称全填好（我→本喵、你→主人、他→那位……）、长句随机猫咪颜文字。运行时生效，不改翻译文件；三种一键预设 + 加喵概率、排除片段等细调 + 实时预览，英文界面也能喵。需要 Harmony。
```

## ③ 描述正文（BBCode，整段粘贴进工坊描述框）

```bbcode
[b]让边缘世界里的每一句话，都变成猫娘在说话。[/b]

纯文本层的模组：不改翻译文件、不改 Def、不改存档，只在游戏运行时把显示出来的文字，换成「QQ喵喵助手」那种句尾带「喵」的腔调。三种一键预设，细节全部可调。

[h2]功能[/h2]
- 句尾加喵：新建殖民地 → 新建殖民地喵
- 人称替换（十项默认全部填好）：我 / 我们 → 本喵 / 本喵们，你 / 你们 → 主人 / 主人们，他 / 她 → 那位、他们 / 她们 → 那几位，它 / 它们 → 那只喵 / 那几只喵；每一项都能改、清空，还能加自定义替换表
- 长句末尾随机追加猫咪颜文字：（=^ω^=）（≧▽≦）（｡･ω･｡）……
- 覆盖范围：主菜单、按钮、提示、物品 / 地块 / 植物信息卡、事件与社交日志、小人背景故事，以及语法规则生成的句子
- 小人 / 动物 / 武器的名字保持原样（名字带喵会很怪）；想要连名字一起喵，可以在设置里打开
- 内置约 90 条社区猫咪颜文字，按可爱 / 战斗 / 社交 / 工作 / 食物 / 动物 / 难过 / 恋爱八类语境挑应景的
- 英文界面：句尾词自动换成 meow / nya / mrrp，设置界面也是英文（中文界面行为不变）

[h2]三种一键预设[/h2]
- [b]小喵一声[/b]：只加句尾喵，改动最小，适合怕吵的玩家
- [b]猫娘日常[/b]：句尾喵 + 十个人称替换 + 长句颜文字（默认）
- [b]喵力全开[/b]：句尾词 喵 / 喵呜 / 喵～，最短字数 1，长句必加颜文字

[h2]还能细调[/h2]
路径：选项 → Mod 设置 → 喵化世界 Meowify
- 句尾语气词：开关、最短喵化字数、加喵概率（可只给一部分句子加喵）、句尾词候选（每行一个，随机挑一个）
- 人称替换：我 / 我们 / 你 / 你们 / 他 / 他们 / 她 / 她们 / 它 / 它们 十个槽位都能改，还能加自定义替换表
- 句末颜文字：开关、触发概率、最短字数、候选列表，还可以按语境挑
- 额外保护词：含「我 / 你 / 他」但不是人称的词整段跳过（内置：迷你、自我、忘我、无我、敌我、你我、其他、吉他……）
- 排除片段：文本里出现指定片段就整段原样返回，给个别模组留后门
- 适用范围四开关：界面文本 / 定义注入文本 / 语法生成句子 / 定义字段兜底，关掉哪层哪层完全原样
- 折叠连续句尾词 + 自定义后缀：默认把「几句喵喵。」收成「几句喵~。」
- 只喵这些词 / 不要喵这些词：关键词白名单与黑名单，黑名单优先
- 设置界面六个标签页：预设 / 句尾 / 人称 / 颜文字 / 范围 / 高级；预设页有实时预览，关闭窗口自动保存

[h2]兼容性[/h2]
- 运行时文本变换：不写回游戏数据、不改存档结构，随时可以关掉或退订
- 与其它翻译模组、文本类模组共存没问题
- 兼容 HAR / 绮罗族等异种族模组：只喵可见文本，颜色通道、贴图路径等机器字段原样保留
- 与机翻类模组（如 Auto Translation）叠加时可能出现「双份喵化」，属于预期现象，二选一即可
- 仅支持 RimWorld 1.7（1.6 不适用），需要 Harmony

[hr]
[h2]安装[/h2]
订阅 → 在 Mod 列表启用 Meowify（放在 Harmony 之后）→ 重启游戏 → 选项 → Mod 设置里调。

[hr]
[h2]反馈[/h2]
有问题或想法，欢迎在工坊留言区留言；兼容性冲突请附上 Player.log 里的报错行。
```

## ④ 标签（Tags）

```text
Mod ｜ RimWorld 1.7 ｜ Harmony ｜ 中文 ｜ 文本/界面 ｜ 趣味
```

## ⑤ 工坊图片上传顺序

| 顺序 | 文件 | 说明 |
|---|---|---|
| 1 | `docs/screenshots/00-poster.jpg` | 二次元主视觉插画（即 About/Preview.png 的 1920×1080 版） |
| 2 | `docs/screenshots/06-feature-tail.jpg` | ① 句尾喵 |
| 3 | `docs/screenshots/07-feature-pronoun.jpg` | ② 人称替换 |
| 4 | `docs/screenshots/08-feature-kaomoji.jpg` | ③ 随机颜文字 |
| 5 | `docs/screenshots/09-onscreen.jpg` | 实机效果 |
| 6 | `docs/screenshots/01-main-menu.jpg` | 真机：主菜单 |
| 7 | `docs/screenshots/02-ingame.jpg` | 真机：殖民地界面 |
| 8 | `docs/screenshots/03-social-log.jpg` | 真机：社交日志 |
| 9 | `docs/screenshots/04-settings-presets.jpg` | 设置界面顶部（⚠️ 1.3 六标签页版待重拍） |
| 10 | `docs/screenshots/05-settings-preview.jpg` | 设置界面底部（⚠️ 同上，待重拍） |

## ⑥ 描述自检（发布前逐条过）

- [x] 标题 / 粗体没有 Markdown 语法（`#`、`**` 会原样显示）
- [x] 没有 HTML 残留（`<b>`、`<br>`、`<p>`）
- [x] 不用 `[list]` / `[*]`，改用 `- ` 短横线
- [x] BBCode 标签全部成对，`[hr]` 单独使用
- [x] 按维护者要求**不写更新日志**（没放 `[spoiler]`）
- [x] 与 `About/About.xml` 的 `<description>` 功能描述一致（六标签页、十个人称默认全填、社区颜文字、名字默认不改、英文界面）

## ⑦ 发布步骤清单

- [x] 三处版本号同步为 1.3.1（About.xml / MeowifyMod.cs / csproj）
- [x] 编译 0 警告 0 错误；核心逻辑单测 ALL PASS
- [ ] 设置界面六标签页真机截图（替换 04 / 05；截图待补）
- [x] `tools\make-release.ps1` 打包：`release\Meowify\Meowify-1.3.1\` + `Meowify-1.3.1.zip`（910,384 B，6 文件）
- [x] 发布包内容已装进 `Mods\Meowify\`（真实文件夹，dll 56,320 B，程序集 1.3.1.0）
- [x] `About/PublishedFileId.txt`（3826020497）保留在 mod 根目录并打进发布包——上传时走「更新 / Update」，覆盖原工坊条目，不要新建
- [ ] 更新已有工坊条目：填标题、摘要、描述（上面 ③ 整段粘贴），预览图用 About/Preview.png
- [ ] 按 ⑤ 顺序上传图片，首图为 00-poster.jpg
- [ ] 发布后在工坊页确认是原条目的新版本（作者页不应出现同名新条目）
- [x] 打 tag：`v1.3.1`，推送到 https://github.com/Redbean520/Meowify

## ⑧ 与 About.xml 的一致性说明

工坊描述是给浏览器看的 BBCode 长版；`About/About.xml` 的 `<description>` 是游戏内 Mod 列表显示的纯文本短版（也是纯文本、可用换行，不能用 BBCode）。两边功能点保持一致，改版本或加功能时两处一起过。

---

# 二、English

## ⑨ Title

```text
Meowify — every line in RimWorld now says meow
```

## ⑩ Short description

```text
Adds "meow" to every line of RimWorld 1.7: sentence-ending meow, ten pronoun slots pre-filled with cat-speak (I -> this cat, you -> master, he / she -> that one, it -> that meow...), random cat kaomoji on longer text. Runtime only, no translation files touched. Three presets plus fine-tuning and live preview. English UI supported. Requires Harmony.
```

## ⑪ BBCode body

```bbcode
[b]Every line in the rim now talks like a cat.[/b]

A pure text-layer mod: it does not edit translation files, defs or saves. It only rewrites text at display time, giving everything that "sentence ends with meow" voice. Three presets, everything adjustable.

[h2]Features[/h2]
- Sentence-ending word: "New colony" -> "New colony, meow"
- Pronouns: ten slots pre-filled with cat-speak (I / we -> this cat, you / you-all -> master, he / she -> that one, it -> that meow, with plurals); every slot editable, clearable or extendable with a custom table
- Random cat kaomoji at the end of longer text: (=^w^=) (^v^) ...
- Covers the main menu, buttons, tooltips, item / terrain / plant info cards, event logs, pawn backstories and grammar-generated sentences
- Pawn, animal and weapon names stay untouched by default (a meow in a name looks odd); a setting can meowify names too
- About 90 community cat kaomoji, picked by context: cute / combat / social / work / food / animal / sad / love
- English UI: sentence-ending words become meow / nya / mrrp, and the mod settings window is in English

[h2]Three presets[/h2]
- [b]A Tiny Meow[/b]: sentence-ending word only, smallest change
- [b]Catgirl Daily[/b]: sentence-ending meow + all ten pronoun swaps + kaomoji on longer text (default)
- [b]Meow Overload[/b]: meow / mrrp / nya~ endings, 1-character minimum, kaomoji on every long line

[h2]Fine-tuning[/h2]
Path: Options -> Mod settings -> Meowify
- Sentence-ending word: toggle, minimum visible characters, chance to apply (some sentences only), word list
- Pronouns: ten slots, a custom "word=replacement" table, plus protected words
- Kaomoji: toggle, chance, minimum length, custom list, context-aware pools
- Protected words: words containing 我 / 你 / 他 that are not pronouns are skipped whole
- Excluded phrases: any text containing one of these is returned untouched, an escape hatch for other mods
- Four scope toggles: interface text / def-injected text / grammar sentences / def field fallback
- Collapse repeated endings with a custom suffix: "meow meow." becomes "meow~."
- Keyword whitelist / blacklist (blacklist wins)
- Six tabs (Presets / Sentence / Pronouns / Kaomoji / Scope / Advanced) with live preview; saves on close

[h2]Compatibility[/h2]
- Text is transformed at runtime; nothing is written back to defs or saves, so it can be removed at any time
- Compatible with alien race mods such as HAR / Kiiro: only visible text is meowified - colour channel names, texture paths and other machine fields are kept intact
- Names are not meowified by default
- Chinese UI keeps its original behaviour; only the English UI switches to English endings
- Requires Harmony

[h2]Install[/h2]
- Subscribe, enable Harmony and Meowify in the mod list, restart
- Load order: Harmony before Meowify (handled automatically)

[h2]Feedback[/h2]
Bugs, weird sentences or ideas for new kaomoji are welcome in the comments.
```

## ⑫ Tags

```text
Mod, 1.7, Harmony, English, Text/UI, Fun
```

## ⑬ Notes

- 工坊页面按语言显示对应文案；同一份文档里中英双份，改功能时两边一起过。
- 截图顺序与中文一致；英文界面截图可以等英文设置界面实机拍好后再补。
- 英文标签与中文标签按页面语言分别填，不要混在一栏。
