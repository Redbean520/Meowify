# Steam 工坊发布文案（可直接复制）

> 当前发布：1.3.0（已打包，`release\Meowify\Meowify-1.3.0.zip`）｜ 目标：RimWorld 1.7 ｜ 依赖：Harmony（brrainz.harmony）
> 更新日期：2026-10-30

---

## ① 标题（Title）

```text
喵化世界 Meowify
```

备用（带英文）：
```text
喵化世界 Meowify — 让 RimWorld 里的每句话都会喵
```

## ② 摘要 / 列表短描述（Short description）

```text
给 RimWorld 1.7 的全部文本加一声「喵」：句尾喵、我→本喵 / 你→主人、长句随机猫咪颜文字。不改翻译文件，运行时生效，兼容其它模组；三种一键预设 + 加喵概率、排除片段等细调 + 实时预览。需要 Harmony。
```

英文摘要（贴在描述第一段末尾，给非中文玩家）：
```text
Every string in RimWorld 1.7 learns to meow: sentence-final "喵", 我→本喵 / 你→主人 pronoun swap, random cat kaomoji on long sentences. Runtime-only, no translation files touched, works alongside other mods. Requires Harmony.
```

## ③ 描述正文（BBCode，直接粘贴进工坊描述框）

```bbcode
[h1]喵化世界 Meowify[/h1]
[b]让边缘世界里的每一句话，都变成猫娘在说话。[/b]

[h2]这是什么[/h2]
纯文本层的模组：不改任何翻译文件、不改 Def、不改存档，只在游戏运行时把显示出来的文字换成「QQ喵喵助手」那种腔调——
[list]
[*] 句尾加「喵」：新建殖民地 → 新建殖民地喵
[*] 人称替换：我 → 本喵，你 → 主人（可关、也能改成别的词）
[*] 长句末尾随机追加猫咪颜文字：（=^ω^=）（≧▽≦）（｡･ω･｡）……
[*] 覆盖范围：主菜单、按钮、提示、物品/地块/植物信息卡、事件日志、小人背景故事，以及语法规则生成的句子
[*] 小人 / 动物 / 武器的[b]名字保持原样[/b]（名字里有喵会很怪）；想要连名字一起喵可以手动打开
[/list]

[h2]三种一键预设[/h2]
[list]
[*] [b]小喵一声[/b]：只加句尾喵，改动最小，适合怕吵的玩家
[*] [b]猫娘日常[/b]：句尾喵 + 我→本喵 / 你→主人 + 长句颜文字（默认）
[*] [b]喵力全开[/b]：句尾词 喵 / 喵呜 / 喵～，最短字数 1，长句必加颜文字
[/list]

[h2]还能细调[/h2]
路径：[b]选项 → Mod 设置 → 喵化世界 Meowify[/b]
[list]
[*] 句尾语气词：开关、最短喵化字数、加喵概率（可只给一部分句子加喵）、句尾词候选（每行一个，随机挑一个）
[*] 人称替换：我 / 我们 / 你 / 你们 / 他 / 他们 / 她 / 她们 / 它 / 它们 十个槽位都能改，还能加自定义替换表
[*] 句末颜文字：开关、触发概率、最短字数、候选列表；内置 8 类语境池、约 90 个社区颜文字，按语境挑应景的
[*] 额外保护词：含「我 / 你」但不是人称的词整段跳过（内置：迷你、自我、忘我、无我、敌我、你我……）
[*] 排除片段：文本里出现指定片段就整段原样返回，给个别模组留后门
[*] 适用范围四开关：界面文本 / 定义注入文本 / 语法生成句子 / 定义字段兜底，可以分层关掉，关掉哪层哪层完全原样
[*] 折叠连续句尾词 + 自定义后缀：默认 `几句喵喵。` → `几句喵~。`
[*] 只喵这些词 / 不要喵这些词：关键词白名单与黑名单，黑名单优先级更高
[*] 按语境挑颜文字：内置可爱 / 战斗 / 社交 / 工作 / 食物 / 动物 / 难过 / 恋爱八类池，按原文关键词挑应景的；也可自己加候选
[*] 设置界面改成六个标签页（预设 / 句尾 / 人称 / 颜文字 / 范围 / 高级），不再是一页长滚动
[*] 预设页有实时预览：改完立刻看到效果，关闭窗口自动保存
[/list]

[h2]兼容性[/h2]
[list]
[*] 运行时文本变换：不写回游戏数据、不改存档结构，随时可以关掉或退订
[*] 与其它翻译模组、文本类模组共存没问题
[*] 兼容 HAR / 绮罗族等异种族模组：只喵可见文本，颜色通道、贴图路径等机器字段原样保留
[*] 与机器翻译类模组（如 Auto Translation）叠加时，可能出现「双份喵化」——同一句被两层处理，属于预期现象，二选一即可
[*] 仅支持 RimWorld 1.7（1.6 不适用），需要 Harmony
[/list]

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
| 9 | `docs/screenshots/04-settings-presets.jpg` | 设置界面顶部（⚠️ 旧版单页布局，v1.3 六标签页版待重拍） |
| 10 | `docs/screenshots/05-settings-preview.jpg` | 设置界面底部（⚠️ 同上，待重拍） |

## ⑥ 发布步骤清单

- [x] 三处版本号同步为 1.3.0（About.xml / MeowifyMod.cs / csproj）
- [x] 编译 0 警告 0 错误；核心逻辑单测 ALL PASS（含新功能：句尾词折叠、加喵概率、排除片段、十个人称、自定义替换表、单次扫描幂等）
- [ ] 设置界面六标签页真机截图（替换 04 / 05；用户在 1.3 自测通过，截图待补）
- [x] `tools\make-release.ps1` 打包：`release\Meowify\Meowify-1.3.0\` + `Meowify-1.3.0.zip`（904,584 B）
- [ ] 把发布包内容装进 `Mods\Meowify\`（真实文件夹，不是 junction）
- [ ] 工坊新建条目：填标题、摘要、描述（上面 ③ 整段粘贴）
- [ ] 上传 zip：`release\Meowify\Meowify-1.3.0.zip`
- [x] 封面已换成「日系手绘卡通」插画版（`About/Preview.png` + `docs/screenshots/00-poster.jpg`）；旧图备份在 `art\Meowify\cover\About-Preview-backup.png`
- [ ] 按 ⑤ 顺序上传图片，首图为 00-poster.jpg
- [ ] 工坊生成 PublishedFileId 后写入 `About/PublishedFileId.txt`（该文件已被 .gitignore 忽略），再提交一次
- [x] 打 tag：`git tag v1.3.0`（推送待做，仓库目前没有 remote）
