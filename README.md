# dysync.net - 抖音同步工具（我收藏的、我喜欢的）

`dysync.net` 是一款基于 **.NET Core 6.0** 和 **Vue** 开发的工具，用于同步抖音收藏夹以及我喜欢的视频，解决个人收藏和喜欢的视频容易失效的问题。支持多账号同步，并已预设刮削信息，同步后的视频可直接在 EMBY 或 Jellyfin (JF) 中播放。

> **注意**：目前项目代码暂不开源。如在使用中有任何问题，欢迎添加 QQ：279225040 进行咨询。

---

#### 先放一张Emby的图
![输入图片说明](docs/emby.png)

### 1. 获取抖音 Cookie 以及用户sec_user_d

Cookie 是同步功能的关键，请严格按照以下步骤获取：

1.  打开 **抖音网页版** (https://www.douyin.com/) 并登录。
2.  进入你的**收藏夹**页面。
3.  按 `F12` 打开浏览器「开发者工具」，切换到「Network (网络)」标签。
4.  在搜索框中输入 `v1/web/aweme/listcollection`。
5.  点击任意一条筛选出的请求，在右侧「Headers (标头)」中找到 `Cookie` 字段，**完整复制**其内容。

![输入图片说明](docs/getcookies.png)
![输入图片说明](docs/secUserId.png)

### 2. 路径映射规则

为了方便管理和播放，请理解并正确配置路径映射：
![输入图片说明](docs/config1.png)

*   **视频存储路径**：容器内路径为 `/app/downloads`。你需要将此路径映射到你本地的一个目录（如 NAS 或电脑硬盘）。**这个本地路径必须与后续在「抖音授权」页面配置的「文件存储路径」完全一致**。
*   **数据库存储路径**：容器内路径为 `/app/db`。映射此路径用于持久化工具的配置和同步记录，防止容器删除后数据丢失。
*   **多账号配置**：如需同步多个账号，需为每个账号指定一个独立的容器内路径（如 `/app/user1`, `/app/user2`），并分别进行映射。

> **重要**：如果不进行路径映射，所有文件将保存在容器内部，无法直接在 EMBY/JF 中访问。

---

## 🔑 默认账号密码

首次登录后台管理页面时使用：

*   **用户名**：`douyin`
*   **密码**：`douyin2025`

> **建议**：登录后请修改密码，保障账号安全。

---

## 🚀 运行方式

你可以选择以下任一方式启动服务。**推荐使用 Docker Compose**，因为它更易于管理和维护。

### 方式一：Docker 命令行

将下方命令中的本地路径替换为你的实际路径，然后在终端中执行。

```bash
docker run -d --restart=always \
  -v /你的/本地/视频/路径:/app/downloads \
  -v /你的/本地/数据库/路径:/app/db \
  -p 18101:10101 \
  --name dysync2025 \
  registry.cn-hangzhou.aliyuncs.com/jianzhichu/dysync.net:v1.0.1


```

### 方式 2：Docker-Compose 运行（推荐）

```yml
version: '3.8'

services:
  dysync:
    image: registry.cn-hangzhou.aliyuncs.com/jianzhichu/dysync.net:v1.0.1
    container_name: dysync2025  # 容器名称，可自定义
    restart: always  # 容器异常退出时自动重启
    ports:
      - "18101:10101"  # 端口映射：本地端口:容器端口（容器端口10101不可修改）
    volumes:
      # 第一个账号的视频存储路径（本地路径:容器路径）
      - /本地视频路径1:/app/downloads
      # 数据库存储路径
      - /本地数据库路径:/app/db
      # 第二个账号的视频存储路径（多账号示例，需在后台对应配置）
      - /本地视频路径2:/app/yeyeye
    # （可选）如需添加环境变量，取消下方注释并配置
    # environment:
    #   - ENV_VAR_NAME=value
    # （可选）如需自定义网络，取消下方注释并配置
    # networks:
    #   - custom_network

# （可选）自定义网络配置
# networks:
#   custom_network:
#     driver: bridge

```
## 软件截图
![输入图片说明](docs/homepage.png)

![输入图片说明](docs/datalist.png)

![输入图片说明](docs/logs.png)

![输入图片说明](docs/set.png)

