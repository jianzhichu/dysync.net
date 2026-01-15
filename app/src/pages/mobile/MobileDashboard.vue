<template>
  <div class="stats-dashboard-mobile">
    <div class="dashboard-container">
      <!-- 看板 Tab 内容 -->
      <div v-if="activeTab === 'dashboard'" class="tab-content">
        <section class="stats-overview">
          <!-- 合并后的总视频数和空间总计卡片 -->
          <div class="stat-card primary-card main-card">
            <div class="stat-header">
              <div class="header-left">
                <span class="stat-meta">视频统计</span>
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
            </div>
            <div class="stat-subitems">
              <div class="subitem" :title="`我喜欢的视频数: ${favoriteCount} (占用: ${favoriteSize}G)`">
                <div class="subitem-icon like-icon">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
                  </svg>
                </div>
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

          <!-- 最新同步视频列表 -->
          <div class="stat-card secondary-card main-card">
            <div class="stat-header">
              <div class="header-left">
                <span class="stat-meta">最新同步</span>
              </div>
            </div>
            <div class="stat-subitems video-list-container">
              <div v-for="(video, index) in topVideos" :key="index" class="subitem video-item" :title="video.title">
                <div class="video-info">
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

      <!-- 日志 Tab 内容 -->
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

    <!-- 底部导航菜单 -->
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

    <!-- 日志详情弹窗 -->
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
          <!-- <div class="file-name">{{ currentLog?.fileName }}</div> -->
          <pre class="log-content">{{ logContent || '加载日志内容中...' }}</pre>
        </div>

        <div class="log-modal-footer">
          <button class="copy-btn" @click="copyLogContent" :disabled="!logContent">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="margin-right: 6px;">
              <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
              <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
            </svg>
            复制
          </button>
        </div>
      </div>
    </div>

    <!-- 视频播放弹窗（竖屏铺满+横屏适配，无旋转） -->
    <div v-if="showVideoPlayer" class="video-modal-mask" @click="closeVideoPlayer">
      <div class="video-modal-content" @click.stop>
        <button class="video-close-btn floating-close-btn" @click="closeVideoPlayer">
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#ffffff" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18"></line>
            <line x1="6" y1="6" x2="18" y2="18"></line>
          </svg>
        </button>

        <div class="video-modal-body">
          <div v-if="videoLoading" class="video-loading">
            <div class="loading-spinner"></div>
            <p>加载视频中...</p>
          </div>
          <div v-else-if="videoError" class="video-error">
            <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#f44336" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="10"></circle>
              <line x1="12" y1="8" x2="12" y2="12"></line>
              <line x1="12" y1="16" x2="12.01" y2="16"></line>
            </svg>
            <p>视频加载失败</p>
            <button class="retry-btn" @click="loadVideo(currentVideo)">重试</button>
          </div>
          <div v-else class="video-player-wrapper">
            <video ref="videoPlayerRef" class="video-player" controls autoplay playsinline :src="videoPlayUrl" @error="handleVideoError" @pause="onVideoPause" @play="onVideoPlay" width="100%" height="100%" preload="auto" @loadedmetadata="handleVideoLoadedMetadata">
              您的浏览器不支持HTML5视频播放
            </video>

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

interface TopVideoItem {
  title: string;
  time: string;
  id: string;
}

// 状态管理
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

// 视频播放相关状态（无旋转相关）
const showVideoPlayer = ref<boolean>(false);
const currentVideo = ref<TopVideoItem | null>(null);
const videoPlayUrl = ref<string>('');
const videoLoading = ref<boolean>(false);
const videoError = ref<boolean>(false);
const videoPlayerRef = ref<HTMLVideoElement | null>(null);
const isVideoPaused = ref<boolean>(false);

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
  TopVideo();
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

const TopVideo = () => {
  useApiStore()
    .TopVideo(10)
    .then((res) => {
      if (res.code == 0) {
        topVideos.value = res.data;
      }
    });
};

const openLogDetail = (log: LogItem) => {
  currentLog.value = log;
  showLogModal.value = true;
  loadLogDetailContent(log);
};

const closeLogModal = () => {
  showLogModal.value = false;
  currentLog.value = null;
  logContent.value = '';
};

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

const formatShowDate = (dateStr?: string) => {
  if (!dateStr || dateStr.length !== 8) return dateStr || '';
  return `${dateStr.slice(0, 4)}-${dateStr.slice(4, 6)}-${dateStr.slice(6, 8)}`;
};

const formatLogTime = (logContent: string) => {
  const timeRegex = /\d{4}-\d{2}-\d{2} (\d{2}:\d{2}:\d{2})\.\d+ \+08:00/g;
  return logContent.replace(timeRegex, '$1');
};

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

const openVideoPlayer = (video: TopVideoItem) => {
  if (!video.id) {
    message.warn('视频ID不存在，无法播放');
    return;
  }

  currentVideo.value = video;
  showVideoPlayer.value = true;
  loadVideo(video).then(() => {
    if (videoPlayerRef.value) {
      videoPlayerRef.value.play().catch((err) => console.log('自动播放被拦截：', err));
    }
  });
};

const loadVideo = async (video: TopVideoItem) => {
  try {
    videoLoading.value = true;
    videoError.value = false;
    const timestamp = new Date().getTime();
    videoPlayUrl.value = `${import.meta.env.VITE_API_URL}api/Video/play/${video.id}?t=${timestamp}`;
    if (videoPlayerRef.value) {
      videoPlayerRef.value.style.width = '100%';
      videoPlayerRef.value.style.height = '100%';
    }
  } catch (err) {
    console.error('加载视频失败：', err);
    videoError.value = true;
    message.error('视频加载失败，请稍后重试');
  } finally {
    videoLoading.value = false;
  }
};

const closeVideoPlayer = () => {
  showVideoPlayer.value = false;
  currentVideo.value = null;
  videoPlayUrl.value = '';
  videoLoading.value = false;
  videoError.value = false;
  isVideoPaused.value = false;
  if (videoPlayerRef.value) {
    videoPlayerRef.value.pause();
    videoPlayerRef.value.style.width = '100%';
    videoPlayerRef.value.style.height = '100%';
  }
};

const handleVideoError = () => {
  videoError.value = true;
  console.error('视频播放出错');
};

const onVideoPause = () => {
  isVideoPaused.value = true;
};

const onVideoPlay = () => {
  isVideoPaused.value = false;
};

const deleteCurrentVideo = async () => {
  closeVideoPlayer();
  TopVideo();
  loadDashboardData();
};
// 视频加载完成后，动态获取视频尺寸并调整容器
const handleVideoLoadedMetadata = () => {
  if (!videoPlayerRef.value) return;

  // 获取视频原宽高比
  const videoWidth = videoPlayerRef.value.videoWidth;
  const videoHeight = videoPlayerRef.value.videoHeight;
  const videoRatio = videoWidth / videoHeight; // 宽/高：>1 横向，≤1 竖向

  // 获取播放器容器
  const wrapper = document.querySelector('.video-player-wrapper');
  if (!wrapper) return;
  const wrapperWidth = wrapper.clientWidth;
  const wrapperHeight = wrapper.clientHeight;

  const video = videoPlayerRef.value;

  // 1. 竖向视频（高≥宽）：全屏填满，无黑边
  if (videoRatio <= 1) {
    video.style.width = '100%';
    video.style.height = '100%';
    video.style.objectFit = 'cover'; // 填满容器，裁剪多余部分
    video.style.margin = '0';
    video.style.position = 'static';
  }
  // 2. 横向视频（宽>高）：居中显示，完整保留，上下黑边
  else {
    // 重置为 contain，确保视频完整显示
    video.style.objectFit = 'contain';
    video.style.width = '100%';
    video.style.height = '100%';
    // 强制视频垂直+水平居中，解决下方没填满的问题
    video.style.position = 'absolute';
    video.style.top = '50%';
    video.style.left = '50%';
    video.style.transform = 'translate(-50%, -50%)';
    // 可选：限制横向视频最大高度不超过容器，避免溢出
    video.style.maxHeight = '100%';
    video.style.maxWidth = '100%';
  }
};
</script>
<!-- 全局样式，放在 scoped 样式外 -->
<style>
html,
body {
  margin: 0;
  padding: 0;
  width: 100%;
  height: 100%;
  overflow-x: hidden;
}
#app {
  margin: 0;
  padding: 0;
  width: 100%;
  height: 100%;
}
</style>

<style scoped>
.stats-dashboard-mobile {
  min-height: 100vh;
  background-color: #ffffff;
  color: #333333;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  padding: 5px 0;
  display: flex;
  flex-direction: column;
  padding-bottom: 80px;
  position: relative;
}
.dashboard-container {
  width: 100%;
  padding: 0 5px;
  box-sizing: border-box;
  flex: 1;
  overflow-y: auto;
  -webkit-overflow-scrolling: touch;
}
.tab-content {
  width: 100%;
  height: 100%;
}
.stats-overview {
  display: grid;
  grid-template-columns: 1fr;
  gap: 20px;
  margin-bottom: 5px;
}
.main-card {
  padding: 0px 10px;
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
.subitem-count {
  color: #222;
  font-weight: 600;
  margin-left: 4px;
}
.subitem-size {
  font-size: 12px;
  color: #999;
  font-weight: 400;
}
.video-title {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
  line-height: 1.4;
  max-height: 2.8em;
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
  align-items: center;
  justify-content: flex-end;
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
.empty-video-list {
  text-align: center;
  padding: 10px 0;
  color: #999;
  font-size: 14px;
}

/* 底部导航样式 */
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

/* 日志 Tab 样式 */
.log-tab {
  padding: 5px 0;
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

/* 日志列表样式 */
.log-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.log-item {
  padding: 10px;
  border-radius: 5px;
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

/* 日志详情弹窗样式 */
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
  padding: 10px;
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
  padding: 5px;
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
  padding: 5px 20px;
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
/* 视频弹窗遮罩：确保全屏无偏移 */
.video-modal-mask {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #000;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
  padding: 0;
  margin: 0;
  box-sizing: border-box;
  overflow: hidden;
}

/* 视频弹窗内容：全屏铺满，无内边距 */
.video-modal-content {
  width: 100vw;
  height: 100vh;
  max-width: none;
  max-height: none;
  background-color: #000; /* 改为纯黑，即使有微小留白也不明显 */
  border-radius: 0;
  box-shadow: none;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  position: absolute;
  top: 0;
  left: 0;
  margin: 0;
  padding: 0; /* 确保无内边距 */
  box-sizing: border-box;
  /* iOS安全区域适配：不额外增加留白，直接填充状态栏 */
  padding-top: env(safe-area-inset-top);
  height: calc(100vh - env(safe-area-inset-top));
}

/* 视频播放器容器：全屏铺满，无额外空间 */
.video-player-wrapper {
  position: relative; /* 新增：为视频绝对定位提供参考 */
  width: 100%;
  height: 100%;
  margin: 0;
  padding: 0;
  overflow: hidden;
  /* 消除安全区域带来的高度偏差 */
  height: calc(100% - env(safe-area-inset-top));
  /* 新增：确保容器内内容垂直居中（兜底） */
  display: flex;
  align-items: center;
  justify-content: center;
}
.floating-close-btn {
  position: absolute;
  top: env(safe-area-inset-top);
  right: 0px;
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
  z-index: 2010;
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
  height: 100%;
  margin: 0;
  overflow: hidden;
}
.video-player-wrapper {
  position: relative;
  width: 100%;
  height: 100%;
  margin: 0;
  padding: 0;
  overflow: hidden;
  height: calc(100% - env(safe-area-inset-top));
}
.video-player {
  width: 100%;
  height: 100%;
  object-fit: cover;
  margin: 0;
  padding: 0;
  border: none;
  outline: none;
  display: block;
}
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

/* 夜间模式 */
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

/* 夜间模式 - 视频弹窗样式适配（移除旋转按钮相关） */
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