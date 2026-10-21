# dysync.net - 抖音同步工具（抖小云）

`dysync.net` 是一款基于 **.NET Core ** 和 **Vue** 开发的工具，用于同步抖音收藏夹、「我喜欢」的视频及指定博主作品，支持多账号同步，内置视频信息刮削功能，同步后的视频可直接在 Emby 或 Jellyfin 中播放。

> 🔧 **问题反馈**：如使用中遇到任何问题，欢迎进Q群反馈：759876963，不一定及时回复，见谅。

---

## 📺 Emby 效果预览
![Emby播放效果](docs/emby.png)

---

## 📋 目录
- [dysync.net - 抖音同步工具（抖小云）](#dysyncnet---抖音同步工具抖小云)
  - [📺 Emby 效果预览](#-emby-效果预览)
  - [📋 目录](#-目录)
  - [1. 获取抖音关键信息（必做！同步核心凭证）](#1-获取抖音关键信息必做同步核心凭证)
    - [1.1 提取`Cookie`以及 `sec_uer_id`](#11-提取cookie以及-sec_uer_id)
    - [1.2 提取 博主的`sec_user_id`以及博主的uid](#12-提取-博主的sec_user_id以及博主的uid)
    - [1.3 扫码获取 Cookie（可选）](#13-扫码获取-cookie可选)
  - [2. Docker 路径映射与扩展存储](#2-docker-路径映射与扩展存储)
    - [默认数据目录](#默认数据目录)
    - [扩展存储目录](#扩展存储目录)
  - [3. 默认账号密码（首次登录用）](#3-默认账号密码首次登录用)
  - [4. Docker 镜像与运行](#4-docker-镜像与运行)
    - [镜像地址与版本标签](#镜像地址与版本标签)
    - [Docker Compose 运行（推荐）](#docker-compose-运行推荐)
  - [🚀 5. 软件截图](#-5-软件截图)
  - [🚀 5. 已有功能与计划](#-5-已有功能与计划)
  - [💖 支持项目](#-支持项目)
    - [⭐ Star 项目](#-star-项目)
    - [💰 赞赏支持](#-赞赏支持)
    - [🎁 其他支持方式](#-其他支持方式)
- [免责声明](#免责声明)
  - [1. 授权与使用限制](#1-授权与使用限制)
  - [2. 知识产权说明](#2-知识产权说明)
  - [3. 风险提示与责任豁免](#3-风险提示与责任豁免)
  - [4. 声明的修改与生效](#4-声明的修改与生效)

---

## 更新日志

- **0.3.1**：FFmpeg AMD 鍔犻€?BUG淇

- **0.3.1**：FFmpeg AMD 鍔犻€?BUG淇

- **0.3.2**：鏀寔瑙嗛璐ㄩ噺鍘熺敾涓嬭浇

## 1. 获取抖音关键信息（必做！同步核心凭证）

Cookie 及 `sec_user_id` 是同步功能的核心，需严格按步骤获取，避免遗漏或错误。


### 1.1 提取`Cookie`以及 `sec_uer_id` 
1. 打开 **抖音网页版** (https://www.douyin.com/) 并登录目标账号；
2. 进入个人主页，按下F12进入开发者模式、并切换到 `网络`(也可能叫`network`).
3. 在筛选框中输入`/follow`.
4. 点击自己头像边上的 `关注` 按钮、会弹出你的关注列表,然后在右侧网络请求里面会出现多个请求,随便选一个.
4. 在请求的标签里面切换到`负载`(也可能叫`payload`)
5. 找到sec_user_id，复制值即可
6. 在请求的标签里面切换到`标头`(也可能叫`Headers`)
7. 往下拉，直到出现`Cookie` ，复制完整的值,注意前后不要带换行符，很多人会多复制个换行符出来.

![cookie](docs/findcookie_1_2_3.png)
![cookie](docs/findcookie_4.png)
![cookie](docs/findcookie_5.png)
![cookie](docs/findcookie_6_7.png)

### 1.2 提取 博主的`sec_user_id`以及博主的uid
- **对于想下载博主视频，但是又不想关注博主，需要用到**
- 1.进入博主主页，按`F12` 点击`Network` 或`网络` 筛选器里面填`/web/aweme/post` 然后切到`预览`或`preview` 展开 json数据结果 找到`aweme_list` 然后随便点开其中一个子项 即可找到`aweme_list[0].author.sec_uid` 这便是博主的uid 。后续在关注列表中，需要手动添加非关注博主同步视频时将会要用到。
![获取Cookie步骤](docs/finduser_id.png)

### 1.3 扫码获取 Cookie（可选）

在 `DY授权` 页面对应账号的操作列点 **扫码更新**，用抖音 App 扫码后先预览获取到的 `Cookie`；确认更新后，系统会校验并直接更新所选账号。手动粘贴 Cookie 的方式仍可使用。

抖音的二维码接口必须由真实浏览器发起，所以扫码功能需要一个 Chromium 内核的浏览器：

- **Windows / 桌面环境运行**：自动使用本机已安装的 Chrome 或 Edge，无需配置。装在非默认位置时用 `QrBrowserPath` 指定可执行文件路径。
- **Docker 部署**：项目根目录的 `docker-compose.yml` 已包含扫码所需的 `dysync-chrome` 浏览器服务和 `QrBrowserUrl` 配置，直接使用该文件启动即可，无需再手动追加服务。


扫码后需要身份验证时：

手机和服务器不在同一网络（异地、云主机、代理）时，抖音通常会在扫码后要求额外验证。此时弹窗会自动切换到 **浏览器画面**，这是服务器上那个浏览器的实时画面，可以直接操作：

- 在画面上点击、拖动、滚动，选择验证方式；
- 需要输入验证码时，先点画面里的输入框，再在画面下方的输入栏里输入并点「发送」；
- 验证通过后会自动继续，取到 `Cookie` 后弹窗关闭。

注意事项：

- 浏览器的调试端口等同于浏览器的完全控制权，不要暴露到局域网或公网。
- 画面有一点延迟，每次操作后稍等一下再点下一步。抖音的验证方式不保证都能在这里完成，实在不行请改用手动粘贴。
- 单独重启 `dysync` 容器后，需要把 `dysync-chrome` 也重启一次（它借用的是 `dysync` 的网络）。
- 没有配置浏览器时点扫码会提示不可用，不影响其他功能。

---

## 2. Docker 路径映射与扩展存储

Docker Compose 的卷映射格式是 `宿主机路径:容器路径`。左侧是 NAS 或电脑上的实际目录，右侧是容器内的目录。程序和抖音授权配置使用**容器路径**；Emby/Jellyfin 访问视频时使用宿主机路径。

### 默认数据目录

| 用途         | 容器路径        | 说明                                             |
| ------------ | --------------- | ------------------------------------------------ |
| 收藏视频     | `/app/collect`  | 默认收藏视频目录                                 |
| 喜欢视频     | `/app/favorite` | 默认「我喜欢」视频目录                           |
| 博主视频     | `/app/uper`     | 默认指定博主作品目录                             |
| 合集视频     | `/app/mix`      | 默认合集目录                                     |
| 短剧视频     | `/app/series`   | 默认短剧目录                                     |
| 配置和数据库 | `/app/db`       | 建议持久化映射，避免重建容器后丢失配置和同步记录 |
| 自定义音频   | `/app/mp3`      | 图文或动态视频合成时使用                         |

账号授权中填写的保存路径必须是容器内路径，例如 `/app/collect`。不要把宿主机路径（例如 `/volume1/video`）填进授权配置。

### 扩展存储目录

需要增加磁盘空间时，在 Compose 的 `volumes` 中添加扩展目录挂载。**容器路径的最后一级必须以 `dyapp_` 开头**，例如 `/app/dyapp_1`、`/app/dyapp_2`。系统配置页会自动发现这些已挂载目录，并在扩展存储下拉框中显示容器路径。

选定扩展根目录后，程序会在其中按视频类型和账号自动创建子目录。例如选择 `/app/dyapp_1` 后，收藏视频会存入该根目录下的 `collect/<账号ID>`，喜欢视频则存入 `favorite/<账号ID>`。各视频类型可以分别选择存储根目录；切换根目录只影响之后下载的文件，已有文件不会搬移。切换阈值用于指定剩余空间下限，当前目录低于阈值时程序会自动切换到其他可用目录；没有目录满足阈值时会跳过本次下载。新增或修改 Compose 挂载后，需要重新创建容器，随后再到系统配置页选择目录。

FN 桌面版不使用 Docker 挂载：扩展存储根目录直接填写设备上的实际路径；多个目录用英文分号 `;` 分隔，例如 `/volume2/video/dysync-extension-1;/volume2/video/dysync-extension-2`。

> 未映射到宿主机的文件只存在于容器内，删除或重建容器可能导致文件丢失。请持久化映射 `/app/db` 和需要保留的视频目录。

![路径配置示例](docs/auth2.png)

![路径配置示例](docs/auth.png)

---

## 3. 默认账号密码（首次登录用）
首次访问后台管理页面时，使用以下默认账号密码：
- **用户名**：`douyin`
- **密码**：`douyin2026`

> ⚠️ **安全建议**：登录后修改密码。

---

## 4. Docker 镜像与运行

### 镜像地址与版本标签

| 设备架构       | 镜像地址                                       |
| -------------- | ---------------------------------------------- |
| x86_64 / amd64 | `ccr.ccs.tencentyun.com/jianzhichu/dysync`     |
| ARM64          | `ccr.ccs.tencentyun.com/jianzhichu/dysync_arm` |

以下 Compose 示例使用 `latest` 标签。

### Docker Compose 运行（推荐）

项目根目录已提供完整的 `docker-compose.yml`，包含应用和扫码浏览器服务。部署前请按实际环境修改 NAS 卷路径、数据库连接信息；ARM64 设备请切换到 ARM 镜像。确认配置后，在项目根目录执行：

<span style="color: red; font-weight: bold;">需要注意：unraid玩家可能需要增加 user配置 --user 1000:100</span>

```bash
services:
  dysync:
    image: ccr.ccs.tencentyun.com/jianzhichu/dysync:latest
    # ARM64 设备：注释上面的 x86 镜像，改用下面这一行。
    # image: ccr.ccs.tencentyun.com/jianzhichu/dysync_arm:latest
    container_name: dysync2026
    restart: unless-stopped
    ports:
      - "10101:10101"
    environment:
      # 扫码登录用的 Chromium 调试地址；浏览器服务与本服务共享网络命名空间。
      - QrBrowserUrl=http://127.0.0.1:9222
      - COMPlus_GCServer=0
      - COMPlus_GCConserveMemory=1
      # 按实际数据库设置修改以下连接信息。
      - DB_TYPE=MySql
      - DB_HOST=192.168.2.11
      - DB_PORT=3306
      - DB_NAME=dysync
      - DB_USER=dysync
      - DB_PASSWORD=111
    volumes:
      # NAS 上的持久化数据库、音频和媒体目录；按实际路径修改。
      - /vol2/1000/media/dysync/db:/app/db
      - /vol2/1000/media/dysync/mp3:/app/mp3
      - /vol2/1000/media/dysync/dy1/coll:/app/collect
      - /vol2/1000/media/dysync/dy1/fav:/app/favorite
      - /vol2/1000/media/dysync/dy1/up:/app/uper
      - /vol2/1000/media/dysync/dy1/mix:/app/mix
      - /vol2/1000/media/dysync/dy1/series:/app/series
      # 更多账号可照此增加路径映射，例如：
      - /vol2/1000/media/dysync/dy2/collect:/app/collect2
      - /vol2/1000/media/dysync/dy2/fav:/app/favorite2
      - /vol2/1000/media/dysync/dy2/up:/app/uper2
    network_mode: bridge
    dns:
      - 223.5.5.5
      - 114.114.114.114
    # 可选：Linux NAS 支持 VAAPI 且存在 /dev/dri 时启用硬件转码。
    # devices:
    #   - /dev/dri:/dev/dri

  dysync-chrome:
    image: chromedp/headless-shell:latest
    container_name: dysync-chrome
    restart: unless-stopped
    depends_on:
      - dysync
    # 只在 dysync 的容器网络内开放调试端口，不要映射到 NAS 主机。
    network_mode: "service:dysync"
    # 如需验证码画面显示中文，可挂载包含 .ttf/.otf/.ttc 的字体目录。
    # volumes:
    #   - /vol2/1000/media/dysync/fonts:/usr/share/fonts/truetype/dysync:ro

```


## 🚀 5. 软件截图

![输入图片说明](docs/homepage.png)

![输入图片说明](docs/homepage_night.png)

![输入图片说明](docs/datalist.png)

![输入图片说明](docs/appconf.png)

![输入图片说明](docs/player.png)

![输入图片说明](docs/follows.png)

![路径配置示例](docs/followset.png)

![输入图片说明](docs/logs.png)


<div align="center">
  <img src="docs/mobile-home.png" width="30%" />
  <img src="docs/mobile-log.png" width="30%" />
  <img src="docs/mobile-logdetail.jpg" width="30%" />
</div>

## 🚀 5. 已有功能与计划


1. ✅ 支持多账号同步（每个账号可单独配置存储路径）

2. ✅ 收藏的视频

3. ✅ 喜欢的视频（点赞的视频）

4. ✅ 图文视频（需要将图片 + mp3 合成视频）

5. ✅ 指定博主的视频,可配置是否单独存放一个总文件夹，是否直接用视频标题做文件名

6. ✅ 清除日志（防止容器被日志占用太多空间，可配置保留天数）

7. ✅ 将项目名称改成 "抖小云" 灵感来源于哪吒电影那坨云

8. ✅ Cookie 过期提醒，在DY授权页可查看

9. ✅ 自动根据去重规则进行去重（可设置去重优先级，同一个视频出现再多个分类时适用）

10. ✅ 在同步记录页面中直接播放视频

11. ✅ 关注列表同步。

11. ✅ 选中记录，批量重新同步。以及分享（比较简陋）

12. ✅ 关注列表支持新增，非关注的博主

13. ✅ 增加永久删除功能，删除后，以后不会再同步该视频

14. ✅ 增加配置项及关注列表（手动添加部分）导出导入功能

15. ✅ 支持移动端

16. ✅ 增加开关配置是否仅同步最近视频
 
17. ✅ 完成飞牛fpk打包:https://github.com/jianzhichu/FnDepot

18. ✅ 合集、短剧、自定义收藏夹下载。

19. ✅ 增加一个工具dy.cookie.exe在源码根目录，可以快速重置失效cookie,操作说明（一步一步操作即可）。

20. ✅ DY授权页支持扫码获取 Cookie 和 secUserId（需要浏览器，见 1.3）。
## 💖 支持项目

如果这个项目对你有帮助，欢迎通过以下方式支持：

### ⭐ Star 项目
点击右上角的 ⭐ Star 按钮，让更多人发现这个项目

### 💰 赞赏支持

- 扫码请作者喝杯奶茶 ☕

|               微信支付                |                 支付宝                 |
| :-----------------------------------: | :------------------------------------: |
| <img src="docs/wx.jpg" width="300" /> | <img src="docs/zfb.jpg" width="300" /> |

### 🎁 其他支持方式
- 分享给更多需要的朋友
- 提交 Bug 报告和功能建议
- 贡献代码和文档

---

<p align="center">
  <strong>感谢你的支持！❤️</strong>
</p>

# 免责声明

本项目（dysync.net）仅作为技术研究与学习交流的开源项目，其所有代码、功能及相关内容均不代表任何第三方平台的官方服务或立场。使用本项目前，请仔细阅读以下条款：

## 1. 授权与使用限制
- 本项目未获得抖音（Douyin）等任何第三方平台的官方授权、合作或认可，与该等平台无任何关联。
- 项目功能仅允许用于个人技术研究和学习目的，严禁用于商业用途、非法活动或违反任何第三方平台规则的行为。

## 2. 知识产权说明
- 项目中涉及的第三方平台（如抖音）的名称、商标、接口、内容等均为其各自所有者的知识产权。
- 本项目仅对技术实现方式进行研究，不涉及对第三方知识产权的侵占或滥用。如因使用本项目引发知识产权争议，相关责任由使用者自行承担。

## 3. 风险提示与责任豁免
- 使用本项目可能导致违反第三方平台的用户协议或相关规定，进而导致账号限制、功能封禁等风险，此类风险由使用者自行承担。
- 项目开发者不对使用本项目产生的任何直接或间接损失（包括但不限于数据丢失、设备损坏、账号问题等）承担责任。
- 使用者应确保自身行为符合《网络安全法》《著作权法》等相关法律法规，不得利用本项目从事侵权、违法或违背公序良俗的活动。

## 4. 声明的修改与生效
- 本免责声明的最终解释权归项目开发者所有，开发者保留随时修改本声明的权利。
- 修改后的声明将在项目仓库中更新，建议使用者定期查阅。
- 一旦使用本项目的代码、功能或相关资源，即视为您已充分理解并同意本免责声明的全部条款。如不同意，请立即停止使用。
