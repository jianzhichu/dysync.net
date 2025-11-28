<template>
  <div>
    <a-form layout="inline" style="margin-top:10px;" :model="quaryData">
      <a-form-item label="同步日期">
        <a-range-picker v-model:value="value1" :ranges="ranges" :locale="locale" @change="datePicked" />
      </a-form-item>
      <a-form-item label="抖音作者" ref="author" name="author">
        <a-input v-model:value="quaryData.author"></a-input>
      </a-form-item>
      <a-form-item>
        <a-radio-group v-model:value="quaryData.viedoType" button-style="solid" @change="onViedoTypeChanged">
          <a-radio-button value="*">全部</a-radio-button>
          <a-radio-button value="1">喜欢的</a-radio-button>
          <a-radio-button value="2">收藏的</a-radio-button>
          <a-radio-button value="3">关注的</a-radio-button>
          <a-radio-button value="4" v-if="showImageViedo">图文视频</a-radio-button>
        </a-radio-group>
      </a-form-item>
      <a-form-item :wrapper-col="{ offset: 8, span: 16 }">
        <a-space>
          <a-button type="primary" @click="GetRecords">查询</a-button>
          <a-button type="danger" @click="StartNow" :disabled="isSyncing">
            <template #icon>
              <a-spin v-if="isSyncing" size="small" />
            </template>
            立即同步
          </a-button>
        </a-space>
      </a-form-item>
    </a-form>

    <!-- 视频播放弹窗 - 简化视频信息 + 优化样式 -->
    <a-modal v-model:visible="isModalOpen" :title="playingTitle" :width="900" :mask-closable="false" :footer="null" @cancel="handleCancel" :body-style="{ padding: '0', overflow: 'hidden', backgroundColor: '#fff' }" :style="{ 
    borderRadius: '8px',
    maxWidth: '85vw', // 最大宽度不超过视口85%（防止超屏）
    maxHeight: '80vh', // 最大高度不超过视口80%
    minWidth: '500px', // 最小宽度兜底（避免过小）
    minHeight: '380px'  // 最小高度兜底
  }" :mask-style="{ backgroundColor: 'rgba(0, 0, 0, 0.5)' }">
      <!-- 视频容器：增加加载遮罩层 -->
      <div class="video-container">
        <!-- 加载遮罩层 - 视频加载中显示 -->
        <div v-if="isVideoLoading" class="loading-overlay">
          <a-spin size="large" tip="视频加载中..." />
          <p class="loading-tip">请稍候，正在为您准备视频...</p>
        </div>

        <!-- 错误提示 - 视频加载失败显示 -->
        <div v-else-if="hasError" class="error-container">
          <a-alert type="error" showIcon :message="errorMessage" description="建议尝试：1. 检查网络连接 2. 刷新页面重试 3. 联系管理员" />
        </div>

        <!-- 视频播放器：添加尺寸限制和比例保持 -->
        <video ref="videoRef" class="video-element" controls preload="metadata" :autoplay="autoPlay" :muted="autoMuted" @error="handleVideoError" @loadeddata="() => isVideoLoading = false" @waiting="() => isVideoLoading = true" @canplay="() => isVideoLoading = false" :style="{ opacity: isVideoLoading || hasError ? 0 : 1, transition: 'opacity 0.3s ease' }">
          <source :src="videoUrl" type="video/mp4" />
          您的浏览器不支持 HTML5 视频播放，请升级浏览器。
        </video>
      </div>

      <!-- 视频信息栏：仅保留同步时间和视频类型 + 优化样式 -->
      <div v-if="currentVideoInfo" class="video-info-bar">
        <div class="info-container">
          <div class="info-item">
            <span class="info-label">同步时间：</span>
            <span class="info-value">{{ currentVideoInfo.syncTimeStr || '未知' }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">视频类型：</span>
            <span class="info-value">{{ currentVideoInfo.viedoCate || '未知' }}</span>
          </div>
        </div>
      </div>
    </a-modal>

    <!-- 表格：视频标题超长省略 + 悬停显示完整标题（修复编译报错） -->
    <a-table :columns="columns" :data-source="dataSource" bordered :pagination="pagination" @change="handleTableChange" :loading="loading">
      <template #bodyCell="{ column, record }">
        <template v-if="column.dataIndex === 'videoTitle'">
          <!-- 核心修复：将内联箭头函数改为方法调用，避免build报错 -->
          <a class="video-title-link" :title="record.videoTitle || '无标题'" @click="handleVideoClick(record)" @mouseenter="handleTitleMouseEnter" @mouseleave="handleTitleMouseLeave">
            {{ formatVideoTitle(record.videoTitle) }}
          </a>
        </template>
      </template>
    </a-table>
  </div>
</template>

<script lang="ts" setup>
import { reactive, ref, onMounted, nextTick, watch } from 'vue';
import { useApiStore } from '@/store';
import type { UnwrapRef } from 'vue';
import dayjs, { Dayjs } from 'dayjs';
import locale from 'ant-design-vue/es/date-picker/locale/zh_CN';
import { message, Spin, Alert } from 'ant-design-vue';

// 类型定义
type RangeValue = [Dayjs, Dayjs];
interface DataItem {
  id?: string; // 视频ID（后端返回的字段，用于拼接播放地址）
  videoTitle?: string; // 视频标题
  syncTimeStr?: string; // 同步时间
  viedoTypeStr?: string; // 同步类型
  author?: string; // 博主
  viedoCate?: string; // 视频类型
  dyUser?: string; // CK名称
}

interface QuaryParam {
  dates?: string[];
  pageIndex: number;
  pageSize: number;
  author: string;
  tag: string;
  viedoType: string;
}

// 引入dayjs中文包
import 'dayjs/locale/zh-cn';
dayjs.locale('zh-cn');

// 表格列配置
const columns = ref([
  {
    title: '同步时间',
    dataIndex: 'syncTimeStr',
    align: 'center',
    width: 180,
  },
  {
    title: '同步类型',
    dataIndex: 'viedoTypeStr',
    align: 'center',
    width: 100,
  },
  {
    title: '博主',
    dataIndex: 'author',
    align: 'center',
    width: 150,
  },
  {
    title: '视频类型',
    dataIndex: 'viedoCate',
    width: 300,
    align: 'center',
  },
  {
    title: '视频标题',
    dataIndex: 'videoTitle',
    align: 'left',
    width: 350, // 确保标题单元格有足够宽度
  },
  {
    title: 'CK名称',
    dataIndex: 'dyUser',
    align: 'center',
    width: 200,
  },
]);

// 基础状态
const loading = ref(false);
const datas: UnwrapRef<DataItem[]> = reactive([]);
const showImageViedo = ref(false);
const dataSource = ref(datas);

// 查询参数
const value1 = ref<RangeValue>();
const ranges = {
  今天: [dayjs(), dayjs()] as RangeValue,
  本月: [dayjs(), dayjs().endOf('month')] as RangeValue,
};
const quaryData: UnwrapRef<QuaryParam> = reactive({
  pageIndex: 0,
  pageSize: 20,
  author: '',
  tag: '',
  viedoType: '*',
});

// 分页配置
const pagination = ref({
  current: 1,
  defaultPageSize: 10,
  total: 0,
  showTotal: () => `共 ${0} 条`,
});

// 视频播放相关配置
const DEFAULT_LOW_VOLUME = 0.3; // 默认低音量（与示例一致）
// 视频播放相关状态
const isVideoLoading = ref(false); // 视频加载状态（控制加载动画显示）
const videoErrorMsg = ref(''); // 视频错误提示
const isSyncing = ref(false);

// 当前播放视频信息
const currentVideoInfo = ref<DataItem | null>(null);

// 状态管理（视频弹窗相关）
const isModalOpen = ref(false); // 弹窗显示状态
const videoRef = ref(null); // 视频元素引用
const videoUrl = ref(''); // 当前播放视频地址
const hasError = ref(false); // 错误状态
const errorMessage = ref(''); // 错误信息
const autoPlay = ref(true); // 弹窗打开自动播放
const autoMuted = ref(true); // 自动播放时静音（浏览器政策要求）
const videoId = ref('');
const playingTitle = ref('');

// -------------------------- 核心方法：标题格式化 --------------------------
/** 格式化表格视频标题：超过20字符显示省略号 */
const formatVideoTitle = (title?: string) => {
  if (!title) return '无标题';
  return title.length > 20 ? `${title.slice(0, 20)}...` : title;
};

/** 格式化弹窗标题：超过40字符显示省略号 */
const formatModalTitle = (title?: string) => {
  if (!title) return '视频播放';
  return title.length > 40 ? `${title.slice(0, 40)}...` : title;
};

/** 标题鼠标进入事件：添加下划线 */
const handleTitleMouseEnter = (e: Event) => {
  const target = e.target as HTMLElement;
  target.style.textDecoration = 'underline';
};

/** 标题鼠标离开事件：移除下划线 */
const handleTitleMouseLeave = (e: Event) => {
  const target = e.target as HTMLElement;
  target.style.textDecoration = 'none';
};
// ----------------------------------------------------------------------------------

// 查询表格数据
const GetRecords = () => {
  loading.value = true;
  quaryData.pageIndex = pagination.value.current;
  quaryData.pageSize = pagination.value.defaultPageSize;

  if (value1.value) {
    quaryData.dates = value1.value.map((date) => date.format('YYYY-MM-DD'));
  }

  useApiStore()
    .VideoPageList(quaryData)
    .then((res) => {
      loading.value = false;
      if (res.code === 0) {
        dataSource.value = res.data.data;
        pagination.value.current = res.data.pageIndex;
        pagination.value.defaultPageSize = res.data.pageSize;
        pagination.value.total = res.data.total;
        pagination.value.showTotal = () => `共 ${res.data.total} 条`;
      } else {
        message.warning(res.message || '获取数据失败');
      }
    })
    .catch((error) => {
      loading.value = false;
      console.error('获取表格数据失败:', error);
      message.error('获取数据失败，请稍后重试');
    });
};

// 立即同步
const StartNow = () => {
  if (isSyncing.value) return;

  message.success('请耐心等待，同步任务正在启动...');
  isSyncing.value = true;

  useApiStore()
    .StartJobNow()
    .then((res) => {
      if (res.code === 0) {
        message.success('同步任务启动成功！');
        GetRecords();
      } else {
        message.error(`同步任务启动失败: ${res.message || '未知错误'}`);
      }
    })
    .catch((error) => {
      console.error('同步任务API调用失败:', error);
      message.error('同步任务启动失败，请检查网络或联系管理员');
    })
    .finally(() => {
      isSyncing.value = false;
    });
};

// 日期选择器变化事件
const datePicked = (_, dateArry: RangeValue) => {
  quaryData.dates = dateArry.map((date) => date.format('YYYY-MM-DD'));
  console.log('选择的日期范围:', quaryData.dates);
};

// 表格分页变化事件
const handleTableChange = (paginationObj: any) => {
  pagination.value.current = paginationObj.current;
  pagination.value.defaultPageSize = paginationObj.pageSize;
  GetRecords();
};

// 获取配置
const getConfig = () => {
  useApiStore()
    .apiGetConfig()
    .then((res) => {
      if (res.code === 0) {
        showImageViedo.value = res.data.downImageVideoFromEnv;
      } else {
        message.warning(`获取配置失败: ${res.message}`);
      }
    })
    .catch((error) => {
      console.error('获取配置失败:', error);
      message.error('获取配置失败，请稍后重试');
    });
};

// 视频点击事件处理 - 核心修改：使用formatModalTitle处理弹窗标题长度
const handleVideoClick = (record: DataItem) => {
  if (!record.id) {
    message.warning('该视频暂无播放地址');
    return;
  }
  // 保存当前视频信息
  currentVideoInfo.value = record;
  videoId.value = record.id;
  // 弹窗标题显示格式化后的标题（超过40字符显示省略号）
  playingTitle.value = formatModalTitle(record.videoTitle);
  isModalOpen.value = true;
  // 显示加载状态
  isVideoLoading.value = true;
  // 重置错误状态
  hasError.value = false;
};

// 监听弹窗状态，加载默认视频
watch(
  isModalOpen,
  (isOpen) => {
    if (isOpen) {
      loadVideo();
    } else {
      pauseVideo();
      // 关闭弹窗时重置状态
      currentVideoInfo.value = null;
      videoUrl.value = '';
      isVideoLoading.value = false; // 重置加载状态
    }
  },
  { immediate: false }
);

// 关闭弹窗
const handleCancel = () => {
  isModalOpen.value = false;
};

// 加载视频
const loadVideo = () => {
  hasError.value = false;
  isVideoLoading.value = true; // 开始加载，显示加载动画

  // 拼接后端视频接口地址
  videoUrl.value = `${import.meta.env.VITE_API_URL}api/Video/play/${videoId.value}`;

  // 重新加载视频（解决切换视频不刷新的问题）
  nextTick(() => {
    if (videoRef.value) {
      // 监听视频加载进度
      videoRef.value.addEventListener('progress', handleVideoProgress);
      videoRef.value.load();
    }
  });
};

// 视频加载进度处理（可选：显示加载百分比）
const handleVideoProgress = (e: Event) => {
  const video = e.target as HTMLVideoElement;
  if (video.buffered.length > 0) {
    const bufferedEnd = video.buffered.end(video.buffered.length - 1);
    const duration = video.duration;
    // 当缓冲达到总时长的90%以上时，可以提前隐藏加载动画
    if (duration > 0 && bufferedEnd / duration > 0.9) {
      isVideoLoading.value = false;
      video.removeEventListener('progress', handleVideoProgress);
    }
  }
};

// 暂停视频
const pauseVideo = () => {
  if (videoRef.value) {
    (videoRef.value as HTMLVideoElement).pause();
    (videoRef.value as HTMLVideoElement).removeEventListener('progress', handleVideoProgress); // 移除进度监听
  }
};

// 视频错误处理
const handleVideoError = (e: Event) => {
  hasError.value = true;
  const video = e.target as HTMLVideoElement;
  const errorCode = video.error?.code;

  // 错误码映射（参考 HTML5 视频错误标准）
  const errorMap: Record<number, string> = {
    1: '视频加载中断',
    2: '网络错误（跨域未配置/后端服务未启动/接口不可用）',
    3: '视频解码失败（格式不支持或文件损坏）',
    4: '视频格式不支持',
    5: '视频文件不存在或后端权限不足',
  };

  errorMessage.value = `加载失败：${errorMap[errorCode as number] || '未知错误'}（视频ID：${videoId.value}）`;
  isVideoLoading.value = false; // 错误时隐藏加载态
  console.error('视频播放错误详情：', video.error);
};

// 修复缺失的方法
const onViedoTypeChanged = () => {
  // 可根据需要添加类型切换后的逻辑
};

// 页面挂载时初始化
onMounted(() => {
  GetRecords();
  getConfig();
});
</script>

<style>
/* 视频容器样式 - 与表格风格统一 */
.video-container {
  position: relative;
  border-bottom: 1px solid #e8e8e8; /* 与表格边框一致 */
  overflow: hidden;
  max-height: 420px;
}

/* 视频播放器样式 - 优化响应式和过渡效果 */
.video-element {
  width: 100%;
  height: auto;
  max-height: 420px;
  min-height: 250px;
  background-color: #000; /* 加载时显示黑色背景，提升体验 */
  object-fit: contain; /* 保持视频比例，不拉伸 */
  opacity: 1;
  transition: opacity 0.3s ease;
}

/* 加载遮罩层 - 优化居中效果和样式 */
.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.7); /* 半透明黑色背景，突出加载动画 */
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  z-index: 10;
  transition: all 0.3s ease;
}

/* 加载提示文字样式 */
.loading-tip {
  color: #ffffff;
  font-size: 16px;
  margin-top: 20px;
  text-align: center;
  padding: 0 20px;
}

/* 错误容器样式（优化错误显示） */
.error-container {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  background-color: #fff;
}

/* 视频信息栏：简化样式 + 优化布局 */
.video-info-bar {
  padding: 16px 24px;
  background: #f8f9fa; /* 淡灰色背景，更清爽 */
  border-bottom: 1px solid #e8e8e8;
}

.info-container {
  display: flex;
  gap: 40px; /* 两个信息项之间的间距 */
  align-items: center;
  flex-wrap: wrap; /* 响应式适配，小屏幕自动换行 */
}

.info-item {
  display: flex;
  align-items: center;
  font-size: 14px;
  line-height: 1.6;
}

.info-label {
  color: #666666; /* 标签深灰色，更醒目 */
  margin-right: 8px;
  white-space: nowrap;
  font-weight: 500;
}

.info-value {
  color: #333333; /* 数值深色，保证可读性 */
  white-space: nowrap;
}

/* 视频标题链接样式（统一提取到CSS，避免内联样式冲突） */
.video-title-link {
  color: #1890ff;
  cursor: pointer;
  text-decoration: none;
  display: inline-block;
  max-width: 100%;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* 弹窗标题样式优化：确保超长时不会撑破弹窗 */
:deep(.ant-modal-title) {
  font-size: 16px !important;
  font-weight: 500 !important;
  color: #1f2937 !important;
  line-height: 1.5 !important;
  white-space: nowrap !important;
  overflow: hidden !important;
  text-overflow: ellipsis !important;
  max-width: calc(100% - 40px) !important; /* 预留关闭按钮空间 */
}

/* 弹窗样式深度优化 - 与主风格完全统一 */
:deep(.ant-modal) {
  border-radius: 8px !important;
  box-shadow: 0 6px 30px rgba(0, 0, 0, 0.1) !important; /* Ant Design标准阴影 */
  overflow: hidden !important;
  max-width: 85vw !important; /* 最大宽度不超过视口85% */
  max-height: 80vh !important; /* 最大高度不超过视口80% */
  min-width: 500px !important; /* 最小宽度兜底 */
  min-height: 380px !important; /* 最小高度兜底 */
  width: 900px !important;
}

:deep(.ant-modal-header) {
  border-bottom: 1px solid #e8e8e8 !important;
  padding: 16px 24px !important;
  border-radius: 8px 8px 0 0 !important;
  background-color: #fff !important;
  display: flex !important;
  align-items: center !important;
  justify-content: space-between !important;
}

:deep(.ant-modal-close) {
  color: #8c8c8c !important;
  transition: all 0.2s ease !important;
  width: 40px !important;
  height: 40px !important;
  border-radius: 50% !important;
  flex-shrink: 0 !important;
}

:deep(.ant-modal-close:hover) {
  color: #1890ff !important;
  background-color: #f0f9ff !important; /* 与Ant Design按钮hover背景一致 */
}

:deep(.ant-modal-content) {
  border-radius: 8px !important;
  overflow: hidden !important;
}

:deep(.ant-modal-mask) {
  background-color: rgba(0, 0, 0, 0.5) !important;
  backdrop-filter: blur(2px) !important; /* 增加毛玻璃效果，提升质感 */
}

/* 加载组件样式优化 - 与Ant Design统一 */
:deep(.ant-spin-dot) {
  color: #1890ff !important;
  font-size: 36px !important; /* 放大加载动画，更醒目 */
}

:deep(.ant-spin-tip) {
  color: #ffffff !important; /* 加载提示文字白色，与深色背景对比 */
  font-size: 16px !important;
  margin-top: 20px !important;
}

/* 错误提示样式优化 - 与Ant Design警告组件统一 */
:deep(.ant-alert-error) {
  border: none !important;
  background-color: #fff2f0 !important;
  color: #ff4d4f !important;
  padding: 12px 16px !important;
  width: 100%;
  max-width: 600px;
}

:deep(.ant-alert-icon) {
  color: #ff4d4f !important;
  font-size: 16px !important;
  margin-right: 8px !important;
}

/* 表格单元格 hover 效果保持一致 */
:deep(.ant-table-tbody tr:hover td) {
  background-color: #fafafa !important;
}

/* 响应式优化：保持各屏幕尺寸下的一致性 */
@media (max-width: 1200px) {
  .video-element {
    max-height: 380px;
  }
}

@media (max-width: 768px) {
  .video-element {
    max-height: 300px;
  }
  .info-container {
    gap: 20px;
  }
  :deep(.ant-modal) {
    width: 95% !important;
    min-width: 320px !important;
    min-height: 320px !important;
  }
  /* 移动端弹窗标题适配 */
  :deep(.ant-modal-title) {
    max-width: calc(100% - 30px) !important;
    font-size: 15px !important;
  }
  /* 移动端加载动画适配 */
  :deep(.ant-spin-dot) {
    font-size: 28px !important;
  }
  .loading-tip {
    font-size: 14px;
  }
}

@media (max-width: 480px) {
  .video-element {
    min-height: 220px;
  }
  .video-info-bar {
    padding: 12px 16px;
  }
  .info-container {
    gap: 12px;
    flex-direction: column;
    align-items: flex-start;
  }
  /* 移动端弹窗标题适配 */
  :deep(.ant-modal-title) {
    max-width: calc(100% - 25px) !important;
    font-size: 14px !important;
  }
}
</style>