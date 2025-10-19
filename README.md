# dysync.net - 抖音同步工具（收藏/喜欢/指定博主，无缝对接Emby/Jellyfin）

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

![路径配置示例](docs/upersuidset.png)

---

## 🔑 3. 默认账号密码（首次登录用）
首次访问后台管理页面时，使用以下默认账号密码：
- **用户名**：`douyin`
- **密码**：`douyin2025`

> ⚠️ 安全建议：登录后修改密码。

---

## 🚀 4. 运行方式（推荐 Docker Compose，更易维护）


将下方命令中的「本地路径」替换为你的实际路径，终端执行即可：
```bash
### 方式一：Docker 命令行
docker run -d --restart=always \
  -v /你的/本地/收藏视频路径:/app/collect \
  -v /你的/本地/喜欢视频路径:/app/favorite \
  -v /你的/本地/数据库路径:/app/db \
  -v /你的/本地/博主视频路径:/app/uper \
  -p 10101:10101 \
  --name dysync2025 \
  registry.cn-hangzhou.aliyuncs.com/jianzhichu/dysync.net
# 注意：-p 后面的容器端口必须为 10101（源码固定）


### 方式二：Docker Compose 运行（推荐）
创建 docker-compose.yml 文件，复制以下内容，替换「本地路径」后执行 docker-compose up -d：

```yaml
version: '3.8'

services:
  dysync:
    image: registry.cn-hangzhou.aliyuncs.com/jianzhichu/dysync.net
    container_name: dysync2025  # 容器名称可自定义
    restart: always  # 容器异常退出时自动重启
    ports:
      - "10101:10101"  # 端口映射：本地端口:容器端口（容器端口10101不可修改）
    volumes:
      # 第一个账号：收藏视频存储路径（本地路径:容器路径）
      - /本地/收藏视频路径:/app/collect
      # 第一个账号：喜欢视频存储路径（本地路径:容器路径）
      - /本地/喜欢视频路径:/app/favorite
      # 数据库存储路径（持久化配置和同步记录）
      - /本地/数据库路径:/app/db
      # 指定博主视频存储路径
      - /本地/博主视频路径:/app/uper
      # 多账号示例：第二个账号视频存储路径（需在后台对应配置）
      - /本地/第二个账号视频路径:/app/yeyeye
    # （可选）环境变量配置（取消注释并修改）
    # environment:
    #   - ENV_VAR_NAME=value
    # （可选）自定义网络配置（取消注释并修改）
    # networks:
    #   - custom_network

# （可选）自定义网络配置
# networks:
#   custom_network:
#     driver: bridge
```
## 🚀 5. 软件截图

![输入图片说明](docs/homepage.png)

![输入图片说明](docs/datalist.png)

![输入图片说明](docs/logs.png)

![输入图片说明](docs/set.png)