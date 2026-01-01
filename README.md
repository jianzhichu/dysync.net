# dysync.net - 抖音同步工具（抖小云）

`dysync.net` 是一款基于 **.NET Core ** 和 **Vue** 开发的工具，用于同步抖音收藏夹、「我喜欢」的视频及指定博主作品，支持多账号同步，内置视频信息刮削功能，同步后的视频可直接在 Emby 或 Jellyfin 中播放。

> 🔧 **问题反馈**：如使用中遇到任何问题，欢迎进Q群反馈：759876963，不一定及时回复，见谅。

---

## 📺 Emby 效果预览
![Emby播放效果](docs/emby.png)

---

## 📋 目录
- [dysync.net - 抖音同步工具（抖小云）](#dysyncnet---抖音同步工具抖小云)
  - [📺 Emby 播放效果预览](#-emby-播放效果预览)
  - [📋 目录](#-目录)
  - [1. 获取抖音关键信息（必做！同步核心凭证）](#1-获取抖音关键信息必做同步核心凭证)
    - [1.1 提取抖音 Cookie](#11-提取抖音-cookie)
    - [1.2 提取 `sec_user_id`（个人/指定博主）](#12-提取-sec_user_id个人指定博主)
  - [2. 路径映射规则（核心！错配会导致无法访问/数据丢失）](#2-路径映射规则核心错配会导致无法访问数据丢失)
  - [3. 默认账号密码（首次登录用）](#3-默认账号密码首次登录用)
  - [4. 运行方式（推荐 Docker Compose）](#4-运行方式推荐-docker-compose)
    - [镜像版本](#镜像版本)
    - [构建命令示例](#构建命令示例)
      - [方式一：Docker 命令行](#方式一docker-命令行)
    - [方式二：Docker Compose 运行（推荐）](#方式二docker-compose-运行推荐)
  - [🚀 5. 软件截图](#-5-软件截图)
  - [🚀 5. 已有功能与计划](#-5-已有功能与计划)

---

## 1. 获取抖音关键信息（必做！同步核心凭证）

Cookie 及 `sec_user_id` 是同步功能的核心，需严格按步骤获取，避免遗漏或错误。



### 1.1 提取抖音 Cookie 
1. 打开 **抖音网页版** (https://www.douyin.com/) 并登录目标账号；
2. 进入「我的收藏」页面，确保页面加载完成；
3. 按 `F12` 打开浏览器「开发者工具」，切换到「Network (网络)」标签；
4. 刷新页面，在搜索框中输入 `v1/web/aweme/listcollection` 筛选请求；
5. 点击任意一条筛选结果，在右侧「Headers (标头)」中找到 `Cookie` 字段，**完整复制整段内容**（不可删减字符）。

![获取Cookie步骤](docs/getcookies.png)

### 1.2 提取你的`sec_user_id`
- **个人 sec_user_id**（同步自己喜欢用）：  
  1.进入自己的抖音主页：
- 方式：按 `F12` 点击`Network` 或`网络` 筛选器里面填`web/user/following/list` 然后 查看请求的 `payload` 或`负载` 即可找到`sec_user_id`;
  2.进入抖音主页后随便点一个自己的作品，然后你的名字，进入目标博主主页；  
  方式：直接复制地址栏中 `user/` 到 `?from_tab_name` 中间部分内容即是博主的 `sec_user_id`；

  ![获取Cookie步骤](docs/getmysecuid.png)

### 1.3 提取 博主的`sec_user_id`以及博主的uid
- **对于想下载博主视频，但是又不想关注博主，需要用到**
- 1.进入博主主页，按`F12` 点击`Network` 或`网络` 筛选器里面填`/web/aweme/post` 然后切到`预览`或`preview` 展开 json数据结果 找到`aweme_list` 然后随便点开其中一个子项 即可找到`author_user_id` 这便是博主的uid 。后续在关注列表中，需要手动添加非关注博主同步视频时将会要用到。
 ![获取Cookie步骤](docs/getuperuid.png)
---

## 2. 路径映射规则（核心！错配会导致无法访问/数据丢失）
为实现视频在 Emby/Jellyfin 中正常播放及数据持久化，需正确配置本地路径与容器路径的映射：

| 存储类型     | 容器内路径      | 本地路径配置要求                                                      | 用途说明                   |
| ------------ | --------------- | --------------------------------------------------------------------- | -------------------------- |
| 个人收藏视频 | `/app/collect`  | 映射到本地目录（如 NAS：`/volume1/抖音/收藏`、电脑：`D:/抖音/收藏`）  | 存储同步后的收藏视频       |
| 个人喜欢视频 | `/app/favorite` | 映射到本地目录（建议与收藏视频路径区分，如 `/volume1/抖音/喜欢`）     | 存储同步后的「我喜欢」视频 |
| 图文视频     | `/app/images`   | 映射到本地目录（建议与收藏视频路径区分，如 `/volume1/抖音/图文视频`） | 存储同步后的「图文视频」   |
| 指定博主视频 | `/app/uper`     | 映射到本地目录（如 `/volume1/抖音/博主作品`）                         | 存储同步后的博主视频       |
| 数据库文件   | `/app/db`       | 映射到本地稳定目录（如 `/volume1/抖音/工具数据库`）                   | 持久化配置、同步记录       |

> ✅ **关键注意**：  
> 1. 本地路径需与后续后台「抖音授权」页面配置的「文件存储路径」**完全一致**；  
> 2. 未配置路径映射时，文件仅存于容器内部，Emby/Jellyfin 无法访问，且容器删除后数据丢失。

![路径配置示例](docs/auth2.png)

![路径配置示例](docs/followset.png)

---

## 3. 默认账号密码（首次登录用）
首次访问后台管理页面时，使用以下默认账号密码：
- **用户名**：`douyin`
- **密码**：`douyin2026`

> ⚠️ **安全建议**：登录后修改密码。

---

## 4. 运行方式（推荐 Docker Compose）

### 镜像版本

| 镜像标签          | 架构           |
| ----------------- | -------------- |
| `beta_1.9.5`      | x86_64 (amd64) |
| `arm_1.9.5`       | ARM64          |


### 构建命令示例

将下方命令中的「本地路径」替换为你的实际路径，终端执行即可：

#### 方式一：Docker 命令行
```bash
docker run -d --restart=always \
  -v /opt/dysync/coll:/app/collect \
  -v /opt/dysync/favorite:/app/favorite \
  -v /opt/dysync/db:/app/db \
  -v /opt/dysync/imgs:/app/images \
  -v /opt/dysync/uper:/app/uper \
  -p 10101:10101 \
  --name dysync2026 \
  ccr.ccs.tencentyun.com/jianzhichu/dysync:beta_1.9.5
```


### 方式二：Docker Compose 运行（推荐）
创建 docker-compose.yml 文件，复制以下内容，替换「本地路径」后执行 docker-compose up -d：

```bash

version: '3.8'

services:
  dysync:
    image: ccr.ccs.tencentyun.com/jianzhichu/dysync:beta_1.9.5
    container_name: dysync2026  # 容器名称
    restart: unless-stopped # 始终重启容器，除非容器被手动停止或Docker服务停止
    ports:
      - "10101:10101" 
    volumes:
      # 基础路径映射
      - /vol2/1000/media/dysync/db:/app/db          # 数据库目录（持久化配置和同步记录）
      - /vol2/1000/media/dysync/dy1/coll:/app/collect   # 个人收藏视频目录
      - /vol2/1000/media/dysync/dy1/fav:/app/favorite  # 个人喜欢视频目录
      - /vol2/1000/media/dysync/dy1/imgv:/app/images    # 图文视频目录
      - /vol2/1000/media/dysync/dy1/up:/app/uper      # 指定博主视频目录
      
      # 多账号路径映射示例（可选，需在后台对应账号配置中指定存储路径）
      - /vol2/1000/media/dysync/dy2/collect:/app/collect2 
      - /vol2/1000/media/dysync/dy2/fav:/app/favorite2  
      - /vol2/1000/media/dysync/dy2/imgv:/app/images2
	  - /vol2/1000/media/dysync/dy2/up:/app/uper

    network_mode: bridge
    dns:
      - 8.8.8.8  # Google DNS（提升海外访问稳定性）
      - 114.114.114.114  # 国内DNS（提升国内访问稳定性）
      - 223.5.5.5  # 阿里云DNS（备用）
    deploy:
      resources: 
          limits:  
           memory: 120m #限制内存占用不超过120mb，如果运行过程中报错，可以再增加点。不做限制会涨到300多

```

## 🚀 5. 软件截图

![输入图片说明](docs/homepage.png)

![输入图片说明](docs/homepage_night.png)

![输入图片说明](docs/datalist.png)

![输入图片说明](docs/auth.png)

![输入图片说明](docs/logs.png)

![输入图片说明](docs/set.png)

![输入图片说明](docs/appconf.png)

![输入图片说明](docs/player.png)

![输入图片说明](docs/follows.png)

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

6. ✅ 增加清除日志（防止容器被日志占用太多空间，可配置保留天数）

7. ✅ 将网站名称改成 "抖小云" 灵感来源于哪吒电影 驮门的那个小云云

8. ✅ Cookie 过期提醒，在D音授权页可查看

9. ✅ 自动根据去重规则进行去重（可设置去重优先级，同一个视频出现再多个分类时适用）

10. ✅ 在同步记录页面中直接播放视频

11. ✅ 关注列表同步。

11. ✅ 选中记录，批量重新同步。以及分享（比较简陋）

12. ✅ 关注列表支持新增，非关注的博主

13. ✅ 增加永久删除功能，删除后，以后不会再同步该视频

14. ✅ 增加配置项及关注列表（手动添加部分）导出导入功能

15. ✅ 增加移动端（主要统计数据和日志以及最近十条同步记录-可手机端播放）

16. ✅ 增加开关配置是否仅同步最近视频，默认开启（针对之前收藏或者点赞了很多乱七八糟的视频，太多，又不想一个个去抖音取消的情况）
 
17. ✅ 完成飞牛fpk打包:https://github.com/jianzhichu/FnDepot


