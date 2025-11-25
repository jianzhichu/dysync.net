# dysync.net - 抖音同步工具（抖小云）

`dysync.net` 是一款基于 **.NET Core 6.0** 和 **Vue** 开发的工具，用于同步抖音收藏夹、「我喜欢」的视频及指定博主作品，解决个人收藏视频易失效的问题。支持多账号同步，内置视频信息刮削功能，同步后的视频可直接在 Emby 或 Jellyfin 中播放。

> 🔧 问题反馈：使用中遇到任何问题，欢迎添加 QQ：279225040 咨询

---

### 📺 Emby 播放效果预览
![Emby播放效果](docs/emby.png)

---

## 📌 1. 获取抖音关键信息（必做！同步核心凭证）
Cookie 及 `sec_user_id` 是同步功能的核心，需严格按步骤获取，避免遗漏或错误。

### 1.1 提取抖音 Cookie
1. 打开 **抖音网页版** (https://www.douyin.com/) 并登录目标账号；
2. 进入「我的收藏」页面，确保页面加载完成；
3. 按 `F12` 打开浏览器「开发者工具」，切换到「Network (网络)」标签；
4. 刷新页面，在搜索框中输入 `v1/web/aweme/listcollection` 筛选请求；
5. 点击任意一条筛选结果，在右侧「Headers (标头)」中找到 `Cookie` 字段，**完整复制整段内容**（不可删减字符）。

![获取Cookie步骤](docs/getcookies.png)
![提取个人sec_user_id](docs/secUserId.png)

### 1.2 提取 `sec_user_id`（个人/指定博主）
- **个人 sec_user_id**（同步自己的收藏/喜欢用）：  
  进入自己的抖音主页，浏览器地址栏中 `sec_user_id=` 后的字符串即为个人 ID（如 `https://www.douyin.com/user/sec_user_id=xxx`）。
- **博主 sec_user_id**（同步指定博主作品用）：  
  1. 进入目标博主主页；  
  2. 方式1：直接复制地址栏中 `user/` 到  `?from_tab_name`中间部分内容即是博主的 `sec_user_id`；  
  3. 方式2：按 `F12` → 「Network」→ 任意请求 → 「Headers」→ 提取 `sec_user_id` 字段值。

> ⚠️ 风控提示：同步博主作品时，**慎用开启全量同步**（一次性下载过多易被抖音限制访问）。

![提取博主sec_user_id](docs/upers-uid.png)

---

## 📁 2. 路径映射规则（核心！错配会导致无法访问/数据丢失）
为实现视频在 Emby/Jellyfin 中正常播放及数据持久化，需正确配置本地路径与容器路径的映射：

| 存储类型         | 容器内路径       | 本地路径配置要求                                                                 | 用途说明                     |
|------------------|------------------|----------------------------------------------------------------------------------|------------------------------|
| 个人收藏视频     | `/app/collect`   | 映射到本地目录（如 NAS：`/volume1/抖音/收藏`、电脑：`D:/抖音/收藏`）             | 存储同步后的收藏视频         |
| 个人喜欢视频     | `/app/favorite`  | 映射到本地目录（建议与收藏视频路径区分，如 `/volume1/抖音/喜欢`）                 | 存储同步后的「我喜欢」视频   |
| 指定博主视频     | `/app/uper`      | 映射到本地目录（如 `/volume1/抖音/博主作品`）                                     | 存储同步后的博主视频         |
| 数据库文件       | `/app/db`        | 映射到本地稳定目录（如 `/volume1/抖音/工具数据库`）                               | 持久化配置、同步记录（防止容器删除后数据丢失） |
| 多账号视频（可选）| `/app/user1`/`/app/user2` 等 | 为每个账号分配独立本地路径（如 `/volume1/抖音/账号A`、`/volume1/抖音/账号B`） | 实现多账号隔离同步           |

> ✅ 关键注意：  
> 1. 本地路径需与后续后台「抖音授权」页面配置的「文件存储路径」**完全一致**；  
> 2. 未配置路径映射时，文件仅存于容器内部，Emby/Jellyfin 无法访问，且容器删除后数据丢失。

![路径配置示例](docs/auth2.png)

---

## 🔑 3. 默认账号密码（首次登录用）
首次访问后台管理页面时，使用以下默认账号密码：
- **用户名**：`douyin`
- **密码**：`douyin2025`

> ⚠️ 安全建议：登录后修改密码。

---

## 🚀 4. 运行方式（推荐 Docker Compose）


# Dysync.net Docker 镜像版本说明

## 镜像版本

| 镜像标签                          | 架构           | 功能描述                                                                 | 镜像大小  | 适用场景                     |
|-----------------------------------|----------------|--------------------------------------------------------------------------|-----------|------------------------------|
| `jianzhichu/dysync.net:latest`    | x86_64 (amd64) | **标准版**<br>- 包含核心功能<br>- **不含 FFmpeg**<br>- 不能下载图文视频   | ~200M     | 仅需基础功能，追求轻量部署   |
| `jianzhichu/dysync.net:full_latest` | x86_64 (amd64) | **完整版**<br>- 包含全部核心功能<br>- **内置 FFmpeg**<br>- 支持图文视频下载与合成 | ~700M     | 需要完整媒体处理能力的场景   |
| `jianzhichu/dysync.net:arm_latest` | ARM64          | **ARM 标准版**<br>- 核心功能（与 `latest` 一致）<br>- **适配 ARM 架构**   | ~200M     | ARM 设备（如树莓派）的轻量部署 |
| `jianzhichu/dysync.net:full_arm_latest` | ARM64      | **ARM 完整版**<br>- 完整功能（与 `full_latest` 一致）<br>- **适配 ARM 架构** | ~500M     | ARM 设备的完整功能部署       |
| `jianzhichu/dysync.net:beta_1.0`  | x86_64 (amd64) | **测试版 v1.0**<br>- 包含最新开发特性<br>- 功能可能不稳定<br>- 不含 FFmpeg | ~200M     | 开发测试、尝鲜新功能         |

## 构建命令示例

以下是构建上述各版本镜像的 Docker 命令参考：

```bash


将下方命令中的「本地路径」替换为你的实际路径，终端执行即可：
```bash

### 方式一：Docker 命令行
docker run -d --restart=always \
  -v /opt/dysync/coll:/app/collect \
  -v /opt/dysync/favorite \
  -v /opt/dysync/db:/app/db \
  -v /opt/dysync/imgs:/app/images \
  -v /opt/dysync/uper:/app/uper \
  -p 10103:10101 \
  --name dysync_arm_full \
  registry.cn-hangzhou.aliyuncs.com/jianzhichu/dysync.net:latest
# 注意：-p 后面的容器端口必须为 10101（源码固定）


### 方式二：Docker Compose 运行（推荐）
创建 docker-compose.yml 文件，复制以下内容，替换「本地路径」后执行 docker-compose up -d：

version: '3.8'

services:
  dysync:
    image: registry.cn-hangzhou.aliyuncs.com/jianzhichu/dysync.net:latest
    container_name: dysync_arm_full  # 容器名称
    restart: always  # 总是重启
    ports:
      - "10101:10101"  # 端口映射
    volumes:
      - /opt/dysync/db:/app/db  # 数据库目录
      - /opt/dysync/coll:/app/collect  # 收集目录
      - /opt/dysync/favorite:/app/favorite  # 收藏目录（补充了容器内路径）
      - /opt/dysync/imgs:/app/images  # 图片目录
      - /opt/dysync/uper:/app/uper  # 上传目录
      # 下面是多账号路径映射示例（可选）
      - /opt/dysync/coll2:/app/collect  # 收集目录
      - /opt/dysync/favorite2:/app/favorite  # 收藏目录（补充了容器内路径）
      - /opt/dysync/imgs2:/app/images  # 图片目录
      - /opt/dysync/uper2:/app/uper  # 上传目录
    # 配置DNS服务器（可选）
    network_mode: bridge
    dns:
      - 8.8.8.8  # Google DNS
      - 114.114.114.114  # 国内DNS
      - 223.5.5.5 


```
## 🚀 5. 软件截图

![输入图片说明](docs/homepage.png)

![输入图片说明](docs/homepage_night.png)

![输入图片说明](docs/datalist.png)

![输入图片说明](docs/auth.png)

![输入图片说明](docs/logs.png)

![输入图片说明](docs/set.png)


# 抖小云 功能和计划


1. ✅ 支持多账号同步（每个账号可单独配置存储路径）

2. ✅ 收藏的视频

3. ✅ 喜欢的视频（点赞的视频）

4. ✅ 图文视频（需要将图片 + mp3 合成视频）

5. ✅ 指定博主的视频,可配置是否单独存放一个总文件夹，是否直接用视频标题做文件名

6. ✅ 增加清除日志（防止容器被日志占用太多空间，可配置保留天数）

7. ✅ 将网页名称改成 "抖小云" 灵感来源于哪吒电影 驮门的那个小云云

8. ☐ Cookie 过期提醒（或者看看能不能实现扫码登录自动获取 Cookie）

9. ☐ 重复视频去重（同一个视频同时属于收藏视频、喜欢的视频或指定的博主作品）

10. ☐ 网页界面可直接播放视频

