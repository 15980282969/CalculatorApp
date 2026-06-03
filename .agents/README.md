# Qoder 技能配置

本项目使用的 Qoder 技能列表和配置信息。

## 已配置的技能

### amazon-storefront-design
- **版本**: 1.0.0
- **来源**: SkillHub (Community)
- **描述**: Amazon Store builder — page layouts, brand story, shoppable images, traffic driving, conversion optimization
- **安装时间**: 2026-06-03
- **安装级别**: 项目级 + 用户级
- **技能路径**: `.agents/skills/amazon-storefront-design/`

### github
- **版本**: 1.0.0
- **来源**: SkillHub (Community)
- **描述**: Interact with GitHub using the `gh` CLI
- **安装时间**: 2026-06-02
- **安装级别**: 项目级
- **技能路径**: `skills/github/`

## 技能存储位置说明

### 项目级技能
- **路径**: `.agents/skills/`
- **用途**: 跟随项目版本控制，团队成员共享
- **管理**: 通过 SkillHub CLI 安装后复制到此目录

### 用户级技能
- **路径**: `~/.agents/skills/`
- **用途**: Qoder IDE 技能库扫描目录
- **管理**: 所有项目共享的全局技能

### SkillHub 安装记录
- **路径**: `skills/`
- **用途**: SkillHub CLI 工具的安装目录
- **管理**: 自动记录在 `.skills_store_lock.json`

## 安装新技能的流程

1. 使用 SkillHub CLI 搜索技能：
   ```powershell
   python "$env:USERPROFILE\.skillhub\skills_store_cli.py" search <技能名>
   ```

2. 安装到项目：
   ```powershell
   python "$env:USERPROFILE\.skillhub\skills_store_cli.py" install <技能名>
   ```

3. 复制到项目级目录：
   ```powershell
   Copy-Item "$env:USERPROFILE\.agents\skills\<技能名>\*" -Destination ".agents/skills/<技能名>/" -Force -Recurse
   ```

4. 验证安装：
   ```powershell
   Get-ChildItem ".agents/skills/<技能名>/"
   ```

## 技能库同步

确保以下目录中的技能保持同步：
- `.agents/skills/` - 项目级技能（Qoder 扫描）
- `skills/` - SkillHub CLI 记录
- `~/.agents/skills/` - 用户级技能（可选）

## 注意事项

- `.agents/` 目录应添加到 `.gitignore` 中（如果需要版本控制技能，则不添加）
- 团队项目建议将 `.agents/skills/` 纳入版本控制
- 个人项目建议使用用户级技能目录
