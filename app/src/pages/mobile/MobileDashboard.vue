<template>
  <div class="stats-dashboard-mobile">
    <div class="dashboard-container">
      <!-- 看板 Tab 内容 -->
      <div v-if="activeTab === 'dashboard'" class="tab-content">
        <!-- 仅显示核心统计概览（无底部详细数据） -->
        <section class="stats-overview">
          <!-- 合并后的总视频数和空间总计卡片（保留原有移动端样式） -->
          <div class="stat-card primary-card main-card">
            <div class="stat-header">
              <div class="header-left">
                <span class="stat-meta">视频统计</span>
                <!-- 拆分视频总数和总空间到左右两侧 -->
                <div class="stat-value-container">
                  <div class="stat-value video-count">
                    {{ totalVideos }}
                    <span class="value-label">个</span>
                  </div>
                  <div class="stat-value size-count">
                    {{ fileSizeTotal }}G
                  </div>
                </div>
              </div>
              <!-- 移除SVG图标 -->
            </div>
            <div class="stat-subitems">
              <div class="subitem" :title="`我喜欢的视频数: ${favoriteCount} (占用: ${favoriteSize}G)`">
                <div class="subitem-icon like-icon">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
                  </svg>
                </div>
                <!-- 子项调整：数量放在label后，占用空间单独显示 -->
                <span class="subitem-meta">我喜欢的 <span class="subitem-count">({{ favoriteCount }})</span></span>
                <span class="subitem-size">{{ favoriteSize }}G</span>
              </div>
              <div class="subitem" :title="`我收藏的视频数: ${collectCount} (占用: ${collectSize}G)`">
                <div class="subitem-icon collect-icon">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M19 21l-7-5-7 5V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2z"></path>
                  </svg>
                </div>
                <span class="subitem-meta">我收藏的 <span class="subitem-count">({{ collectCount }})</span></span>
                <span class="subitem-size">{{ collectSize }}G</span>
              </div>
              <div class="subitem" :title="`我关注的视频数: ${followCount} (占用: ${followSize}G)`">
                <div class="subitem-icon follow-icon">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                    <circle cx="8.5" cy="7" r="4"></circle>
                    <line x1="20" y1="8" x2="20" y2="14"></line>
                    <line x1="23" y1="11" x2="17" y2="11"></line>
                  </svg>
                </div>
                <span class="subitem-meta">我关注的 <span class="subitem-count">({{ followCount }})</span></span>
                <span class="subitem-size">{{ followSize }}G</span>
              </div>
              <div class="subitem" :title="`图文视频数: ${graphicVideoCount} (占用: ${graphicVideoSize}G)`">
                <div class="subitem-icon graphic-icon">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
                    <circle cx="8.5" cy="8.5" r="1.5"></circle>
                    <polyline points="21 15 16 10 5 21"></polyline>
                  </svg>
                </div>
                <span class="subitem-meta">图文视频 <span class="subitem-count">({{ graphicVideoCount }})</span></span>
                <span class="subitem-size">{{ graphicVideoSize }}G</span>
              </div>
            </div>
          </div>

          <!-- 新增 Top Videos 列表（使用原有移动端卡片样式体系） -->
          <div class="stat-card secondary-card main-card">
            <div class="stat-header">
              <div class="header-left">
                <span class="stat-meta">最新同步</span>
                <!-- <div class="stat-value">TOP {{ topVideos.length }}</div> -->
              </div>
              <!-- 移除SVG图标 -->
            </div>
            <div class="stat-subitems video-list-container">
              <div v-for="(video, index) in topVideos" :key="index" class="subitem video-item" :title="video.title">
                <div class="video-info">
                  <!-- 点击标题播放视频 -->
                  <span class="subitem-meta video-title" @click="openVideoPlayer(video)">{{ video.title }}</span>
                  <span class="subitem-value video-time">{{ video.time }}</span>
                </div>
              </div>
              <div v-if="topVideos.length === 0" class="empty-video-list">
                暂无热门视频数据
              </div>
            </div>
          </div>
        </section>
      </div>

      <!-- 日志 Tab 内容（完全保留原有样式） -->
      <div v-if="activeTab === 'log'" class="tab-content log-tab">
        <div class="log-header">
          <h2 class="log-title">系统日志</h2>
          <div class="log-filter">
            <button class="filter-btn" :class="{ active: logFilter === 'all' }" @click="logFilter = 'all'">
              全部
            </button>
            <button class="filter-btn" :class="{ active: logFilter === 'debug' }" @click="logFilter = 'debug'">
              DEBUG
            </button>
            <button class="filter-btn" :class="{ active: logFilter === 'error' }" @click="logFilter = 'error'">
              ERROR
            </button>
          </div>
        </div>

        <div class="log-list">
          <div v-for="(log,index) in filteredLogs" :key="index" class="log-item" :class="log.type.toLowerCase()" @click="openLogDetail(log)">
            <div class="log-item-header">
              <span class="log-type" :class="log.type.toLowerCase()">
                {{ log.type }}
              </span>
              <span class="log-time">{{formatShowDate(log?.date)  }}</span>
            </div>
            <div class="log-file">{{ log.fileName }}</div>
          </div>

          <div v-if="filteredLogs.length === 0" class="empty-log">
            暂无{{ logFilter === 'all' ? '' : logFilter.toUpperCase() }}类型日志
          </div>
        </div>
      </div>
    </div>

    <!-- 底部导航菜单（完全保留原有样式） -->
    <div class="bottom-nav">
      <div class="nav-item" :class="{ active: activeTab === 'dashboard' }" @click="activeTab = 'dashboard'">
        <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polygon points="3 11 12 2 21 11 12 20 3 11"></polygon>
          <line x1="12" y1="2" x2="12" y2="20"></line>
        </svg>
        <span>看板</span>
      </div>
      <div class="nav-item" :class="{ active: activeTab === 'log' }" @click="activeTab = 'log'">
        <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
          <polyline points="14 2 14 8 20 8"></polyline>
          <line x1="16" y1="13" x2="8" y2="13"></line>
          <line x1="16" y1="17" x2="8" y2="17"></line>
          <polyline points="10 9 9 9 8 9"></polyline>
        </svg>
        <span>日志</span>
      </div>
    </div>

    <!-- 日志详情弹窗（完全保留原有样式） -->
    <div v-if="showLogModal" class="log-modal-mask" @click="closeLogModal">
      <div class="log-modal-content" @click.stop>
        <div class="log-modal-header">
          <div class="header-left">
            <span class="modal-type-tag" :class="currentLog?.type.toLowerCase()">
              {{ currentLog?.type }}
            </span>
            <span class="modal-date">{{ formatShowDate(currentLog?.date) }}</span>
          </div>
          <button class="close-btn" @click="closeLogModal">
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"></line>
              <line x1="6" y1="6" x2="18" y2="18"></line>
            </svg>
          </button>
        </div>

        <div class="log-modal-body">
          <div class="file-name">{{ currentLog?.fileName }}</div>
          <pre class="log-content">{{ logContent || '加载日志内容中...' }}</pre>
        </div>

        <div class="log-modal-footer">
          <button class="copy-btn" @click="copyLogContent" :disabled="!logContent">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="margin-right: 6px;">
              <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
              <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
            </svg>
            复制日志
          </button>
        </div>
      </div>
    </div>

    <!-- 新增：视频播放弹窗（全屏适配+暂停显示删除按钮） -->
    <div v-if="showVideoPlayer" class="video-modal-mask" @click="closeVideoPlayer">
      <div class="video-modal-content" @click.stop>
        <!-- 移除原有标题栏，将关闭按钮直接放在内容容器内，实现悬浮 -->
        <button class="video-close-btn floating-close-btn" @click="closeVideoPlayer">
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#ffffff" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18"></line>
            <line x1="6" y1="6" x2="18" y2="18"></line>
          </svg>
        </button>

        <div class="video-modal-body">
          <!-- 加载中状态 -->
          <div v-if="videoLoading" class="video-loading">
            <div class="loading-spinner"></div>
            <p>加载视频中...</p>
          </div>
          <!-- 错误状态 -->
          <div v-else-if="videoError" class="video-error">
            <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#f44336" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="10"></circle>
              <line x1="12" y1="8" x2="12" y2="12"></line>
              <line x1="12" y1="16" x2="12.01" y2="16"></line>
            </svg>
            <p>视频加载失败</p>
            <button class="retry-btn" @click="loadVideo(currentVideo)">重试</button>
          </div>
          <!-- 视频播放区域：添加ref用于获取视频实例，绑定暂停/播放事件 -->
          <div v-else class="video-player-wrapper">
            <video ref="videoPlayerRef" class="video-player" controls autoplay playsinline :src="videoPlayUrl" @error="handleVideoError" @pause="onVideoPause" @play="onVideoPlay">
              您的浏览器不支持HTML5视频播放
            </video>
            <!-- 悬浮透明删除X按钮：仅暂停时显示 -->
            <button class="video-delete-btn" v-show="isVideoPaused" @click.stop="deleteCurrentVideo" title="删除该视频">
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="#ffffff" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <line x1="18" y1="6" x2="6" y2="18"></line>
                <line x1="6" y1="6" x2="18" y2="18"></line>
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { ref, onMounted, computed } from 'vue';
import { useApiStore } from '@/store';
import { message } from 'ant-design-vue';

// 类型接口
interface Author {
  name: string;
  count: number;
  icon: string;
}
interface Category {
  name: string;
  count: number;
  color: string;
  icon: string;
}

interface LogItem {
  id: string;
  type: 'DEBUG' | 'ERROR';
  time: string;
  message: string;
  file: string;
  date: string;
  fileName: string;
}

// 新增 TopVideo 类型接口（增加videoId字段）
interface TopVideoItem {
  title: string;
  time: string;
  id: string; // 视频ID
}

// 状态管理（仅保留核心数据）
const totalVideos = ref<number>(0);
const fileSizeTotal = ref<string>('0.00');
const favoriteCount = ref<number>(0);
const collectCount = ref<number>(0);
const followCount = ref<number>(0);
const graphicVideoCount = ref<number>(0);
const favoriteSize = ref<string>('0.00');
const collectSize = ref<string>('0.00');
const followSize = ref<string>('0.00');
const graphicVideoSize = ref<string>('0.00');

// 导航相关状态
const activeTab = ref<string>('dashboard');
const logFilter = ref<string>('all');
const logs = ref<LogItem[]>([]);

// 弹窗相关状态
const showLogModal = ref<boolean>(false);
const currentLog = ref<LogItem | null>(null);
const logContent = ref<string>('');

// Top Videos 相关状态
const topVideos = ref<TopVideoItem[]>([]);

// 新增：视频播放相关状态
const showVideoPlayer = ref<boolean>(false);
const currentVideo = ref<TopVideoItem | null>(null);
const videoPlayUrl = ref<string>('');
const videoLoading = ref<boolean>(false);
const videoError = ref<boolean>(false);
// 新增：视频实例引用和暂停状态
const videoPlayerRef = ref<HTMLVideoElement | null>(null);
const isVideoPaused = ref<boolean>(false); // 控制删除按钮显示

// 过滤后的日志列表
const filteredLogs = computed(() => {
  if (logFilter.value === 'all') {
    return logs.value;
  }
  return logs.value.filter((log) => log.type.toLowerCase() === logFilter.value);
});

// 组件名称
defineOptions({ name: 'StatsDashboardMobile' });

// 加载核心数据
onMounted(() => {
  loadDashboardData();
  loadLogData();
  TopVideo(); // 加载热门视频数据
});

const loadDashboardData = async () => {
  try {
    const res = await useApiStore().VideoStatics();
    totalVideos.value = res.data.videoCount;
    fileSizeTotal.value = res.data.videoSizeTotal || '0.00';
    favoriteCount.value = res.data.favoriteCount;
    collectCount.value = res.data.collectCount;
    followCount.value = res.data.followCount || 0;
    graphicVideoCount.value = res.data.graphicVideoCount || 0;
    favoriteSize.value = res.data.videoFavoriteSize || '0.00';
    collectSize.value = res.data.videoCollectSize || '0.00';
    followSize.value = res.data.videoFollowSize || '0.00';
    graphicVideoSize.value = res.data.graphicVideoSize || '0.00';
  } catch (err) {
    console.error('加载移动端仪表盘数据失败：', err);
  }
};

// 加载日志数据（模拟接口，实际项目中替换为真实接口）
const loadLogData = async () => {
  try {
    useApiStore()
      .MobileLogs()
      .then((res) => {
        if (res.code == 0) {
          logs.value = res.data;
        }
      });
  } catch (err) {
    console.error('加载日志数据失败：', err);
  }
};

// 加载最新同步的视频10个
const TopVideo = () => {
  useApiStore()
    .TopVideo(10)
    .then((res) => {
      if (res.code == 0) {
        topVideos.value = res.data;
      }
    });
};

// 打开日志详情弹窗
const openLogDetail = (log: LogItem) => {
  currentLog.value = log;
  showLogModal.value = true;
  loadLogDetailContent(log);
};

// 关闭日志详情弹窗
const closeLogModal = () => {
  showLogModal.value = false;
  currentLog.value = null;
  logContent.value = '';
};

// 加载日志详情内容
const loadLogDetailContent = (log: LogItem) => {
  try {
    const type = log.type.toLowerCase();
    const date = log.date;
    const requestParams = `${type}/${date}`;

    useApiStore()
      .apiGetLogs(requestParams)
      .then((logContentStr: string) => {
        const formattedLog = formatLogTime(logContentStr);
        const lines = formattedLog.split('\n');
        const reversedLines = lines.reverse();
        const reversedText = reversedLines.join('\n');
        logContent.value = reversedText;
      })
      .catch((err) => {
        console.error('加载日志详情失败：', err);
        logContent.value = '日志内容加载失败，请重试';
      });
  } catch (err) {
    console.error('加载日志详情失败：', err);
    logContent.value = '日志内容加载失败，请重试';
  }
};

// 格式化日期显示（20251211 → 2025-12-11）
const formatShowDate = (dateStr?: string) => {
  if (!dateStr || dateStr.length !== 8) return dateStr || '';
  return `${dateStr.slice(0, 4)}-${dateStr.slice(4, 6)}-${dateStr.slice(6, 8)}`;
};

// 处理日志时间戳，仅保留时分秒
const formatLogTime = (logContent: string) => {
  const timeRegex = /\d{4}-\d{2}-\d{2} (\d{2}:\d{2}:\d{2})\.\d+ \+08:00/g;
  return logContent.replace(timeRegex, '$1');
};

// 复制日志内容（移动端兼容）
const copyLogContent = () => {
  if (!logContent.value) {
    message.warn('暂无日志内容可复制');
    return;
  }

  if (navigator.clipboard) {
    navigator.clipboard
      .writeText(logContent.value)
      .then(() => message.success('日志内容已复制到剪贴板'))
      .catch(() => {
        fallbackCopyTextToClipboard(logContent.value);
      });
  } else {
    fallbackCopyTextToClipboard(logContent.value);
  }
};

// 复制降级方案（适配移动端）
const fallbackCopyTextToClipboard = (text: string) => {
  const textArea = document.createElement('textarea');
  textArea.value = text;
  textArea.style.position = 'fixed';
  textArea.style.top = '0';
  textArea.style.left = '0';
  textArea.style.opacity = '0';
  document.body.appendChild(textArea);
  textArea.focus();
  textArea.select();

  try {
    const successful = document.execCommand('copy');
    const msg = successful ? '日志内容已复制到剪贴板' : '复制失败，请手动复制';
    message.success(msg);
  } catch (err) {
    console.error('Fallback: Oops, unable to copy', err);
    message.error('复制失败，请手动复制');
  }

  document.body.removeChild(textArea);
};

// 新增：打开视频播放弹窗
const openVideoPlayer = (video: TopVideoItem) => {
  if (!video.id) {
    message.warn('视频ID不存在，无法播放');
    return;
  }

  currentVideo.value = video;
  showVideoPlayer.value = true;
  loadVideo(video).then(() => {
    // 视频加载完成后触发播放，避免浏览器拦截
    if (videoPlayerRef.value) {
      videoPlayerRef.value.play().catch((err) => console.log('自动播放被拦截：', err));
    }
  });
};

// 新增：加载视频播放地址
const loadVideo = async (video: TopVideoItem) => {
  try {
    videoLoading.value = true;
    videoError.value = false;
    const timestamp = new Date().getTime();
    videoPlayUrl.value = `${import.meta.env.VITE_API_URL}api/Video/play/${video.id}?t=${timestamp}`;
  } catch (err) {
    console.error('加载视频失败：', err);
    videoError.value = true;
    message.error('视频加载失败，请稍后重试');
  } finally {
    videoLoading.value = false;
  }
};

// 新增：关闭视频播放弹窗
const closeVideoPlayer = () => {
  showVideoPlayer.value = false;
  currentVideo.value = null;
  videoPlayUrl.value = '';
  videoLoading.value = false;
  videoError.value = false;
  isVideoPaused.value = false; // 重置暂停状态
};

// 新增：处理视频播放错误
const handleVideoError = () => {
  videoError.value = true;
  console.error('视频播放出错');
};

// 新增：视频暂停回调
const onVideoPause = () => {
  isVideoPaused.value = true;
};

// 新增：视频播放回调
const onVideoPlay = () => {
  isVideoPaused.value = false;
};

const deleteCurrentVideo = async () => {
  // 关闭播放弹窗，刷新视频列表
  closeVideoPlayer();
  TopVideo(); // 重新加载最新视频列表
  loadDashboardData(); // 刷新仪表盘统计数据
};
</script>

<style scoped>
/* 完全保留原有移动端样式 */
.stats-dashboard-mobile {
  min-height: 100vh;
  background-color: #ffffff;
  color: #333333;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  padding: 15px 0;
  display: flex;
  flex-direction: column;
  padding-bottom: 80px; /* 为底部导航留出空间 */
  position: relative;
}
.dashboard-container {
  width: 100%;
  padding: 0 15px;
  box-sizing: border-box;
  flex: 1;
  overflow-y: auto;
  -webkit-overflow-scrolling: touch; /* 移动端弹性滚动 */
}
.tab-content {
  width: 100%;
  height: 100%;
}
.stats-overview {
  display: grid;
  grid-template-columns: 1fr;
  gap: 20px;
  margin-bottom: 20px;
}
.main-card {
  padding: 20px 16px;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.06);
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
  border: 1px solid #f0f0f0;
  background: #fff;
}
.main-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 25px rgba(0, 0, 0, 0.08);
}
.stat-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 16px;
}
.header-left {
  display: flex;
  flex-direction: column;
  gap: 4px;
  width: 100%;
}
.stat-meta {
  font-size: 14px;
  color: #666666;
  text-transform: uppercase;
  letter-spacing: 0.4px;
  font-weight: 500;
}
/* 新增：视频总数和总空间容器样式 */
.stat-value-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
  margin-top: 8px;
}
.stat-value {
  font-size: 26px;
  font-weight: 700;
  color: #1a1a1a;
  line-height: 1.1;
}
.value-label {
  font-size: 14px;
  color: #666;
  font-weight: 500;
  margin-left: 4px;
}
.video-count {
  font-size: 22px;
}
.size-count {
  font-size: 22px;
  color: #666;
}
.unit {
  font-size: 18px;
  color: #444444;
  font-weight: 500;
}
.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background-color: rgba(76, 175, 80, 0.15);
  color: #4caf50;
}
.secondary-card .stat-icon {
  background-color: rgba(33, 150, 243, 0.15);
  color: #2196f3;
}
.stat-subitems {
  display: grid;
  grid-template-columns: 1fr;
  gap: 12px;
  padding-top: 16px;
  border-top: 1px solid #f0f0f0;
}
.subitem {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 14px;
  background: #ffffff;
  border: 1px solid #f0f0f0;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  transition: all 0.2s ease;
  cursor: default;
  justify-content: space-between;
}
.subitem:hover {
  transform: translateY(-1px);
  box-shadow: 0 3px 10px rgba(0, 0, 0, 0.06);
  border-color: #e0e0e0;
}
.subitem-icon {
  width: 28px;
  height: 28px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background-color: rgba(233, 30, 99, 0.1);
  color: #e91e63;
}
.collect-icon {
  background-color: rgba(255, 152, 0, 0.1);
  color: #ff9800;
}
.follow-icon {
  background-color: rgba(156, 39, 176, 0.1);
  color: #9c27b0;
}
.graphic-icon {
  background-color: rgba(255, 159, 64, 0.1);
  color: #d9091a;
}
/* 视频列表图标样式（沿用原有样式体系） */
.video-list-icon {
  background-color: rgba(33, 150, 243, 0.15);
  color: #2196f3;
}
.video-index {
  font-size: 12px;
  font-weight: 600;
}
.subitem-meta {
  font-size: 13px;
  color: #666666;
  font-weight: 500;
  flex: 1;
}
/* 新增：子项数量样式 */
.subitem-count {
  color: #222;
  font-weight: 600;
  margin-left: 4px;
}
/* 新增：子项占用空间样式 */
.subitem-size {
  font-size: 12px;
  color: #999;
  font-weight: 400;
}
/* 视频列表文字样式（适配移动端） */
.video-title {
  display: -webkit-box;
  -webkit-line-clamp: 2; /* 显示2行 */
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
  line-height: 1.4;
  max-height: 2.8em; /* 2行 * line-height */
  width: 100%;
  padding-right: 4px;
  cursor: pointer;
  transition: color 0.2s;
}
.video-title:hover {
  color: #2196f3;
}
.video-time {
  font-size: 11px !important;
  color: #999;
  margin-top: 2px;
  font-weight: 300 !important;
  display: flex;
  align-items: center; /* 垂直居中 */
  justify-content: flex-end; /* 水平右对齐 */
}
.video-info {
  flex: 1;
  overflow: hidden;
}
.subitem-value {
  font-size: 15px;
  font-weight: 600;
  color: #222222;
  display: flex;
  align-items: baseline;
  gap: 3px;
}
.subitem-value .unit {
  font-size: 12px;
  color: #555555;
  font-weight: 500;
}
.primary-card {
  border-top: 3px solid #4caf50;
}
.secondary-card {
  border-top: 3px solid #2196f3;
}

/* 空视频列表样式（适配移动端） */
.empty-video-list {
  text-align: center;
  padding: 20px 0;
  color: #999;
  font-size: 14px;
}

/* 底部导航样式（完全保留） */
.bottom-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  height: 60px;
  background-color: #ffffff;
  border-top: 1px solid #f0f0f0;
  display: flex;
  justify-content: space-around;
  align-items: center;
  z-index: 100;
  box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.05);
}
.nav-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
  height: 100%;
  color: #666666;
  transition: all 0.2s ease;
  cursor: pointer;
}
.nav-item.active {
  color: #4caf50;
}
.nav-item span {
  font-size: 12px;
  margin-top: 4px;
  font-weight: 500;
}
.nav-item svg {
  transition: all 0.2s ease;
}
.nav-item.active svg {
  transform: scale(1.1);
}

/* 日志 Tab 样式（完全保留） */
.log-tab {
  padding: 10px 0;
}
.log-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding: 0 5px;
}
.log-title {
  font-size: 18px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0;
}
.log-filter {
  display: flex;
  gap: 8px;
}
.filter-btn {
  padding: 6px 12px;
  border-radius: 20px;
  border: 1px solid #e0e0e0;
  background-color: #ffffff;
  color: #666666;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}
.filter-btn.active {
  background-color: #4caf50;
  color: #ffffff;
  border-color: #4caf50;
}
.filter-btn:hover:not(.active) {
  border-color: #cccccc;
  color: #333333;
}

/* 日志列表样式（完全保留） */
.log-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.log-item {
  padding: 15px;
  border-radius: 8px;
  border: 1px solid #f0f0f0;
  background-color: #ffffff;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  cursor: pointer;
  transition: all 0.2s ease;
}
.log-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}
.log-item.debug {
  border-left: 4px solid #2196f3;
}
.log-item.error {
  border-left: 4px solid #f44336;
}
.log-item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}
.log-type {
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
}
.log-type.debug {
  background-color: rgba(33, 150, 243, 0.1);
  color: #2196f3;
}
.log-type.error {
  background-color: rgba(244, 67, 54, 0.1);
  color: #f44336;
}
.log-time {
  font-size: 11px;
  color: #999999;
}
.log-file {
  font-size: 12px;
  color: #666666;
}
.empty-log {
  text-align: center;
  padding: 40px 20px;
  color: #999999;
  font-size: 14px;
}

/* 日志详情弹窗样式（完全保留） */
.log-modal-mask {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
  box-sizing: border-box;
}
.log-modal-content {
  width: 100%;
  max-width: 500px;
  background-color: #ffffff;
  border-radius: 12px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
  max-height: 80vh;
  display: flex;
  flex-direction: column;
}
.log-modal-header {
  padding: 10px;
  border-bottom: 1px solid #f0f0f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.header-left {
  display: flex;
  align-items: center;
  gap: 8px;
}
.modal-type-tag {
  padding: 3px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
}
.modal-type-tag.debug {
  background-color: rgba(33, 150, 243, 0.1);
  color: #2196f3;
}
.modal-type-tag.error {
  background-color: rgba(244, 67, 54, 0.1);
  color: #f44336;
}
.modal-date {
  font-size: 14px;
  color: #666;
  font-weight: 500;
}
.close-btn {
  background: transparent;
  border: none;
  cursor: pointer;
  color: #666;
  padding: 4px;
  border-radius: 4px;
  transition: all 0.2s ease;
}
.close-btn:hover {
  background-color: #f5f5f5;
  color: #333;
}
.log-modal-body {
  padding: 10px;
  flex: 1;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}
.file-name {
  font-size: 14px;
  color: #333;
  font-weight: 600;
  margin-bottom: 5px;
  border-bottom: 1px solid #f0f0f0;
}
.log-content {
  flex: 1;
  margin: 0;
  padding: 5px;
  background-color: #f9f9f9;
  border-radius: 8px;
  overflow-y: auto;
  overflow-x: auto;
  font-family: Consolas, Monaco, monospace;
  font-size: 13px;
  line-height: 1.6;
  white-space: pre-wrap;
  word-break: break-all;
  -webkit-overflow-scrolling: touch;
  touch-action: pan-y;
  color: #333;
}
.log-modal-footer {
  padding: 16px 20px;
  border-top: 1px solid #f0f0f0;
  display: flex;
  justify-content: flex-end;
}
.copy-btn {
  padding: 8px 16px;
  background-color: #4caf50;
  color: #ffffff;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: all 0.2s ease;
}
.copy-btn:disabled {
  background-color: #cccccc;
  cursor: not-allowed;
}
.copy-btn:hover:not(:disabled) {
  background-color: #43a047;
}
/* 视频播放弹窗样式（纯全屏+悬浮右上角关闭按钮） */
.video-modal-mask {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #000; /* 纯黑背景更贴合全屏播放 */
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
  padding: 0;
  box-sizing: border-box;
  -webkit-backdrop-filter: blur(4px);
  backdrop-filter: blur(4px);
}
.video-modal-content {
  width: 100vw; /* 视口宽度100% */
  height: 100vh; /* 视口高度100% */
  max-width: none; /* 移除原有最大宽度限制 */
  max-height: none; /* 移除原有最大高度限制 */
  background-color: #111;
  border-radius: 0; /* 全屏移除圆角 */
  box-shadow: none; /* 全屏无需阴影 */
  display: flex;
  flex-direction: column;
  overflow: hidden;
  position: relative; /* 为悬浮关闭按钮提供定位上下文 */
}
/* 悬浮右上角关闭按钮样式 */
.floating-close-btn {
  position: absolute;
  top: 20px; /* 右上角间距，可调整 */
  right: 20px; /* 右上角间距，可调整 */
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background-color: rgba(0, 0, 0, 0.5);
  border: none;
  cursor: pointer;
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  z-index: 2010; /* 确保在视频上方 */
  backdrop-filter: blur(2px);
}
.floating-close-btn:hover {
  background-color: rgba(255, 255, 255, 0.2);
  transform: scale(1.1);
}
.video-modal-body {
  padding: 0;
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #000;
  position: relative;
  width: 100%;
  height: 100%; /* 完全填充父容器，无高度损耗 */
}
/* 新增：视频播放器容器（用于定位删除按钮） */
.video-player-wrapper {
  position: relative;
  width: 100vw;
  height: 100vh;
}
.video-player {
  width: 100%;
  height: 100%;
  object-fit: contain; /* 保持视频比例，全屏填充无黑边（也可改为 cover 强制填充，可能裁剪视频） */
}
/* 悬浮透明删除X按钮样式 */
.video-delete-btn {
  position: fixed;
  top: 50%;
  right: 20px;
  transform: translateY(-50%);
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background-color: rgba(0, 0, 0, 0.3);
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #ffffff;
  transition: all 0.2s ease;
  z-index: 2010;
  backdrop-filter: blur(2px);
}
.video-delete-btn:hover,
.video-delete-btn:active {
  background-color: rgba(244, 67, 54, 0.5);
  transform: translateY(-50%) scale(1.1);
}
/* 视频加载状态 */
.video-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #fff;
  padding: 40px 20px;
  width: 100%;
  height: 100%;
}
.loading-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid rgba(255, 255, 255, 0.2);
  border-radius: 50%;
  border-top-color: #fff;
  animation: spin 1s ease-in-out infinite;
  margin-bottom: 16px;
}
@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
/* 视频错误状态 */
.video-error {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #fff;
  padding: 40px 20px;
  text-align: center;
  width: 100%;
  height: 100%;
}
.video-error p {
  margin: 16px 0 24px;
  font-size: 14px;
}
.retry-btn {
  padding: 8px 16px;
  background-color: #2196f3;
  color: #fff;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s;
}
.retry-btn:hover {
  background-color: #1976d2;
}

/* 夜间模式（完全保留原有样式） */
html.dark-mode .stats-dashboard-mobile {
  background-color: #1a1a2e;
  color: #eaeaea;
  padding-bottom: 80px;
}
html.dark-mode .main-card {
  border-color: rgba(255, 255, 255, 0.1);
  background-color: rgba(30, 30, 50, 0.9);
}
html.dark-mode .stat-subitems {
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}
html.dark-mode .subitem {
  background: rgba(40, 40, 65, 0.7);
  border: 1px solid rgba(255, 255, 255, 0.05);
}
html.dark-mode .subitem:hover {
  background: rgba(40, 40, 65, 0.9);
  border-color: rgba(255, 255, 255, 0.1);
}
html.dark-mode .stat-meta {
  color: #b0b0c3;
}
html.dark-mode .stat-value {
  color: #ffffff;
}
html.dark-mode .value-label {
  color: #d0d0d0;
}
html.dark-mode .size-count {
  color: #d0d0d0;
}
html.dark-mode .unit {
  color: #d0d0d0;
}
html.dark-mode .stat-icon {
  background-color: rgba(76, 175, 80, 0.25);
}
html.dark-mode .secondary-card .stat-icon {
  background-color: rgba(33, 150, 243, 0.25);
}
html.dark-mode .subitem-meta {
  color: #c0c0d3;
}
html.dark-mode .subitem-count {
  color: #ffffff;
}
html.dark-mode .subitem-size {
  color: #a0a0b3;
}
html.dark-mode .subitem-value {
  color: #ffffff;
}
html.dark-mode .subitem-value .unit {
  color: #b0b0c3;
}
html.dark-mode .subitem-icon {
  background-color: rgba(233, 30, 99, 0.2);
}
html.dark-mode .collect-icon {
  background-color: rgba(255, 152, 0, 0.2);
}
html.dark-mode .follow-icon {
  background-color: rgba(156, 39, 176, 0.2);
}
html.dark-mode .graphic-icon {
  background-color: rgba(255, 159, 64, 0.2);
}
html.dark-mode .video-list-icon {
  background-color: rgba(33, 150, 243, 0.25);
}
html.dark-mode .video-time {
  color: #a0a0b3;
}
html.dark-mode .empty-video-list {
  color: #808099;
}

/* 夜间模式 - 底部导航（完全保留） */
html.dark-mode .bottom-nav {
  background-color: #16213e;
  border-top-color: rgba(255, 255, 255, 0.1);
}
html.dark-mode .nav-item {
  color: #b0b0c3;
}
html.dark-mode .nav-item.active {
  color: #4caf50;
}

/* 夜间模式 - 日志样式（完全保留） */
html.dark-mode .log-title {
  color: #ffffff;
}
html.dark-mode .filter-btn {
  background-color: #1f4068;
  border-color: rgba(255, 255, 255, 0.1);
  color: #e0e0e0;
}
html.dark-mode .filter-btn.active {
  background-color: #4caf50;
  color: #ffffff;
  border-color: #4caf50;
}
html.dark-mode .log-item {
  background-color: rgba(30, 30, 50, 0.9);
  border-color: rgba(255, 255, 255, 0.05);
}
html.dark-mode .log-file {
  color: #c0c0d3;
}
html.dark-mode .empty-log {
  color: #808099;
}

/* 夜间模式 - 弹窗样式（完全保留） */
html.dark-mode .log-modal-content {
  background-color: #1e1e3f;
  border-color: rgba(255, 255, 255, 0.1);
}
html.dark-mode .log-modal-header,
html.dark-mode .log-modal-footer {
  border-color: rgba(255, 255, 255, 0.1);
}
html.dark-mode .modal-date {
  color: #c0c0d3;
}
html.dark-mode .file-name {
  color: #ffffff;
  border-color: rgba(255, 255, 255, 0.1);
}
html.dark-mode .log-content {
  background-color: #2a2a4a;
  color: #eaeaea;
}
html.dark-mode .close-btn {
  color: #c0c0d3;
}
html.dark-mode .close-btn:hover {
  background-color: #2a2a4a;
  color: #ffffff;
}

/* 夜间模式 - 视频弹窗样式适配 */
html.dark-mode .video-delete-btn {
  background-color: rgba(0, 0, 0, 0.4);
}
html.dark-mode .video-delete-btn:hover,
html.dark-mode .video-delete-btn:active {
  background-color: rgba(244, 67, 54, 0.6);
}

.ant-message {
  z-index: 10000 !important;
}
</style>