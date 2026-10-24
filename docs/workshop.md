# 工坊发布文案草稿（Steam Workshop）

上传时按下面顺序填；方括号里是操作提示，不要复制进工坊。

## 标题

    喵化世界 Meowify

## 摘要（工坊右侧短描述，≤ 200 字）

    给 RimWorld 1.7 的全部文本加一声「喵」：句尾喵、我→本喵 / 你→主人、长句随机猫咪颜文字。
    不改任何翻译文件，运行时生效，兼容任意语言与其它模组；一键预设 + 细调 + 实时预览。需要 Harmony。

## 标签

    Mod / 1.7 / Harmony / 汉化 / 界面 / 文本

## 简介正文（BBCode，直接贴进工坊描述框）

    [h1]喵化世界 Meowify[/h1]
    [b]—— 让边缘世界里的每一句话，都变成猫娘在说话。[/b]

    [h2]这是什么[/h2]
    这是一个纯文本层的模组：不改动任何翻译文件、不改动 Def，只在游戏运行时把显示出来的文字换成
    「QQ喵喵助手」那种腔调 ——
    [list]
    [*] 句尾加「喵」：新建殖民地 → 新建殖民地喵
    [*] 人称替换：我 → 本喵，你 → 主人（可关、可改）
    [*] 长句随机猫咪颜文字：（=^ω^=）（≧▽≦）等 5 种，可自定义
    [*] 覆盖主菜单、按钮、提示、物品/地块信息、事件日志、小人背景故事、语法规则生成的文本
    [/list]

    [h2]三种预设，也可以自己调[/h2]
    [list]
    [*] [b]小喵一声[/b]：只加句尾喵，改动最小，适合怕吵的玩家
    [*] [b]猫娘日常[/b]：句尾喵 + 人称替换 + 长句颜文字，QQ喵喵助手同款
    [*] [b]喵力全开[/b]：句尾词 喵/喵呜/喵～，最短字数 1，长句必加颜文字
    [/list]
    选项 → Mod 设置 → 喵化世界 Meowify 里还能细调：句尾词候选、最短喵化字数、自称/对称、颜文字概率与候选、
    额外保护词（避免「迷你」「自我」「敌我」被误替换），改完立即生效，并有实时预览。

    [h2]安装[/h2]
    [list]
    [*] 需要 [b]Harmony[/b]（brrainz.harmony 的 1.7 版本）
    [*] 订阅本模组，在模组列表里勾选，重启游戏
    [*] 加载顺序：Harmony 在前，本模组在后（About.xml 已声明 loadAfter）
    [/list]

    [h2]兼容性 / 说明[/h2]
    [list]
    [*] 纯运行时文本变换：不写回游戏数据、不改存档结构，随时可以关掉
    [*] 与其它翻译、文本类模组同时使用通常没问题；若和机器翻译类模组叠加，可能叠成双份喵化
    [*] 已为中文语言包设计（按中文句末标点断句），英文与其他语言同样会加「喵」
    [*] 开发模式下的翻译报告/调试按钮不会被改动
    [/list]

    [h2]致谢[/h2]
    [list]
    [*] 风格来源：QQ喵喵助手（qq790795623-sudo/qq-meow-assistant）
    [*] 时序参考：csh1668/AutoTranslation —— 同样在 LoadedLanguage 注入阶段做文章
    [*] 感谢 Ludeon 与 RimWorld 中文翻译组
    [/list]

    [h2]更新日志[/h2]
    [b]v1.0.0[/b]
    [list]
    [*] 首次发布：四层 Harmony 补丁覆盖 Keyed / DefInjected / 语法规则 / Def 扫尾
    [*] 三种一键预设 + 完整设置界面 + 实时预览
    [*] 句尾词候选、人称自定义、颜文字概率与候选、额外保护词
    [/list]

## 截图上传顺序

1. `docs/screenshots/00-preview.jpg`（四格拼图，也可直接用作 About/Preview.png）
2. `docs/screenshots/01-main-menu.jpg`
3. `docs/screenshots/02-ingame.jpg`
4. `docs/screenshots/03-social-log.jpg`
5. `docs/screenshots/04-settings-presets.jpg`
6. `docs/screenshots/05-settings-preview.jpg`

## 上传前清单

- [ ] 版本号三处一致：About/About.xml `<modVersion>`、MeowifyMod.cs `const string Version`、Meowify.csproj `<Version>`
- [ ] `powershell -NoProfile -ExecutionPolicy Bypass -File tools\make-release.ps1` 跑通，产物在 `release\Meowify\`
- [ ] 用发布包（不含 Source）在干净环境启动一次：日志里 `Harmony patches installed: 4/4`，0 ERROR
- [ ] 关闭 devMode 再拍图（避免出现 Dev: 快速测试喵 之类的调试项）
- [ ] 工坊上传 zip 后，把生成的 PublishedFileId 写入 `About/PublishedFileId.txt`（git 忽略）
- [ ] 工坊页填标题/摘要/标签，按上面顺序上传截图
