<template>
  <div>
    <!-- 优化查询区域：调整布局，时间、博主名称、标题放一行，宽度自适应 -->
    <div class="query-container">
      <a-form layout="inline" :model="quaryData" class="query-form">
        <!-- 第一行：时间选择器组 + 博主名称 + 标题（合并为一行，自适应宽度） -->
        <div class="form-row form-main-row">
          <a-form-item label="同步日期" class="form-item form-item-date">
            <a-range-picker v-model:value="value1" :ranges="ranges" :locale="locale" @change="datePicked" class="range-picker" />
          </a-form-item>

          <a-form-item label="发布日期" class="form-item form-item-date">
            <a-range-picker v-model:value="value2" :ranges="ranges2" :locale="locale" @change="datePicked2" class="range-picker" />
          </a-form-item>

          <a-form-item label="博主名称" ref="author" name="author" class="form-item form-item-input">
            <a-input v-model:value="quaryData.author" class="query-input" placeholder="请输入博主名称" />
          </a-form-item>
          <a-form-item label="标题" ref="title" name="title" class="form-item form-item-input">
            <a-input v-model:value="quaryData.title" class="query-input" placeholder="请输入标题" />
          </a-form-item>
        </div>

        <!-- 第二行：单选组 + 按钮组 -->
        <div class="form-row form-actions-row">
          <a-form-item label="视频类型" class="form-item radio-group-item">
            <a-radio-group v-model:value="quaryData.viedoType" button-style="solid" @change="onViedoTypeChanged" class="video-type-radio">
              <a-radio-button value="*">全部</a-radio-button>
              <a-radio-button value="1">喜欢的</a-radio-button>
              <a-radio-button value="2">收藏的</a-radio-button>
              <a-radio-button value="3">关注的</a-radio-button>
              <a-radio-button value="4" v-if="showImageViedo">图文视频</a-radio-button>
              <a-radio-button value="5">收藏夹</a-radio-button>
              <a-radio-button value="6">合集</a-radio-button>
              <a-radio-button value="7">短剧</a-radio-button>
            </a-radio-group>
          </a-form-item>

          <a-button type="primary" @click="GetRecords" class="query-button">
            <SearchOutlined />查询
          </a-button>
          <a-form-item class="form-item batch-operation-item" style="margin-left:20px;">
            <a-switch v-model:checked="isBatchMode" checked-children="批量操作" un-checked-children="批量操作" class="batch-switch" />
          </a-form-item>

          <a-form-item class="form-item button-group-item">
            <a-space size="middle" class="button-group">
              <a-button success @click="handleBatchShare" class="delete-button" v-if="isBatchMode" :disabled="selectedRowKeys.length === 0 || isSyncing">
                <ShareAltOutlined />
                批量分享
              </a-button>
              <a-button danger @click="handleBatchSync" class="delete-button" v-if="isBatchMode" :disabled="selectedRowKeys.length === 0 || isSyncing">
                <SyncOutlined />
                重新下载
              </a-button>
              <a-button danger @click="handleBatchDelete" class="delete-button" v-if="isBatchMode" :disabled="selectedRowKeys.length === 0 || isSyncing">
                <close-outlined />
                批量删除
              </a-button>
            </a-space>
          </a-form-item>
          <!-- 按钮代码 -->
          <a-form-item class="form-item delete-btn-2-wrapper">
            <a-button type="primary" danger @click="handShowDeleteVideos" class="delete-button-2">
              <!-- <ClearOutlined />  -->
              <!-- 注意首字母大写，Antd图标命名规范 -->
              <delete-outlined />
              已删除
            </a-button>
          </a-form-item>
        </div>
      </a-form>
    </div>

    <!-- 已删除视频-抽屉 -->

    <a-drawer title="已删除视频" size="large" :visible="deleteVideoShow" @close="onDeleteVideoClose">
      <template #extra>
      </template>
      <a-list size="small" bordered :data-source="deleteVideos">
        <template #renderItem="{item, index}">
          <a-list-item>
            <!-- 新增文本容器，用于控制省略号 -->
            <div class="delete-video-title-container">
              <span class="delete-video-index">{{ index + 1 }}.</span>
              <span class="delete-video-title" :title="item.videoTitle || '无标题'">
                {{ item.videoTitle }}
              </span>
            </div>

            <!-- <a-button type="text" size="small" class="copy-delete-video-btn" @click="(e) => copyVideoPath(item.videoSavePath)">
              <CopyOutlined /> 复制
            </a-button> -->
          </a-list-item>
        </template>
      </a-list>
    </a-drawer>
    <!-- 视频播放弹窗 - 保持原有 -->
    <a-modal v-model:visible="isModalOpen" :width="900" :mask-closable="false" :footer="null" @cancel="handleCancel" :body-style="{ padding: '0', overflow: 'hidden', backgroundColor: '#fff' }" :style="{ 
    borderRadius: '8px',
    maxWidth: '85vw',
    maxHeight: '80vh',
    minWidth: '500px',
    minHeight: '400px'
  }" :mask-style="{ backgroundColor: 'rgba(0, 0, 0, 0.5)' }">
      <!-- 自定义弹窗标题（替代原来的:title属性） -->
      <template #title>
        <span class="modal-title-with-tooltip" :title="formatFilePath(currentVideoInfo?.videoSavePath)">
          {{ playingTitle }}
        </span>
      </template>
      <div class="video-container">
        <div v-if="isVideoLoading" class="loading-overlay">
          <a-spin size="large" tip="视频加载中..." />
          <p class="loading-tip">请稍候，正在为您准备视频...</p>
        </div>
        <div v-else-if="hasError" class="error-container">
          <a-alert type="error" showIcon :message="errorMessage" description="建议尝试：1. 检查网络连接 2. 刷新页面重试 3. 联系管理员" />
        </div>
        <video ref="videoRef" class="video-element" controls preload="metadata" :autoplay="autoPlay" :muted="autoMuted" @error="handleVideoError" @loadeddata="() => isVideoLoading = false" @waiting="() => isVideoLoading = true" @canplay="() => isVideoLoading = false" :style="{ opacity: isVideoLoading || hasError ? 0 : 1, transition: 'opacity 0.3s ease' }">
          <source :src="videoUrl" type="video/mp4" />
          您的浏览器不支持 HTML5 视频播放，请升级浏览器。
        </video>
      </div>
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
          <div class="info-item">
            <a-popover placement="bottom">
              <template #content>
                <p>{{formatPathSeparator(currentVideoInfo?.videoSavePath)}}</p>
              </template>
              <a-button type="link" size="small" @click="copyVideoPath(formatPathSeparator(currentVideoInfo?.videoSavePath))" class="copy-path-btn">
                复制路径
              </a-button>
            </a-popover>
          </div>
        </div>
      </div>
    </a-modal>

    <!-- 表格 - 增加复选框和操作列 -->
    <a-table :columns="columns" :data-source="dataSource" bordered :pagination="pagination" @change="handleTableChange" :loading="loading" :row-selection="isBatchMode ? rowSelection : null" row-key="id" :sorter="true">
      <template #bodyCell="{ column, record }">
        <template v-if="column.dataIndex === 'videoTitle'">
          <a class="video-title-link" :title="record.videoTitle || '无标题'" @click="handleVideoClick(record)" @mouseenter="handleTitleMouseEnter" @mouseleave="handleTitleMouseLeave">
            {{ formatVideoTitle(record.videoTitle) }}
          </a>
        </template>
        <template v-if="column.key === 'operation'">
          <a-space size="small">
            <a-button type="link" @click="handleReDownload(record)" :disabled="isSyncing">
              <SyncOutlined />
              重新同步
            </a-button>
            <a-button type="link" @click="handleShare(record)" :disabled="!record.id">
              <ShareAltOutlined />
              分享
            </a-button>
            <a-button type="link" danger @click="handleDelete(record)" :disabled="!record.id">
              <DeleteOutlined />
              删除
            </a-button>
          </a-space>
        </template>
      </template>
    </a-table>
  </div>
</template>

<script lang="ts" setup>
import { reactive, ref, onMounted, nextTick, watch, computed } from 'vue';
import { useApiStore } from '@/store';
import type { UnwrapRef } from 'vue';
import dayjs, { Dayjs } from 'dayjs';
import locale from 'ant-design-vue/es/date-picker/locale/zh_CN';
import { message, Modal } from 'ant-design-vue';
import CryptoJS from 'crypto-js';
import {
  SearchOutlined,
  SyncOutlined,
  ShareAltOutlined,
  ClearOutlined,
  CopyOutlined,
  DeleteOutlined,
} from '@ant-design/icons-vue';

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
  fileHash?: string;
  authorId?: string;
  videoSavePath: string;
  createTimeStr?: string; // 发布时间
  isMergeVideo?: boolean;
}

// 📌 新增：排序参数类型定义
interface SortParam {
  field: string; // 排序字段
  order: 'ascend' | 'descend' | ''; // 排序方向：升序/降序/无
}
interface QuaryParam {
  dates?: string[];
  dates2?: string[];
  pageIndex: number;
  pageSize: number;
  author: string;
  title: string;
  viedoType: string;
  fileHash: string;
  authorId: string;
  sortField?: string; // 📌 新增：排序字段
  sortOrder?: string; // 📌 新增：排序方向（asc/desc）
}

// 引入dayjs中文包
import 'dayjs/locale/zh-cn';
import { forEach } from 'lodash';
dayjs.locale('zh-cn');

// 批量操作相关状态
const isBatchMode = ref(false); // 批量操作开关状态
const selectedRowKeys = ref<string[]>([]); // 选中的行ID集合
// 📌 新增：排序状态管理
const sortParams = ref<SortParam>({
  field: 'syncTime', // 默认排序字段（发布时间）
  order: 'descend', // 默认降序（最新的在前）
});

// 表格行选择器类型定义（对齐 Ant Design Vue 3.x 规范）
interface CustomTableRowSelection<T> {
  type: 'checkbox' | 'radio';
  selectedRowKeys: string[] | number[];
  onChange?: (
    selectedRowKeys: string[] | number[],
    selectedRows: T[],
    info: { type: 'select' | 'unselect' | 'selectAll' | 'unselectAll' | 'clear' }
  ) => void;
  preserveSelectedRowKeys?: boolean;
  getCheckboxProps?: (record: T) => { disabled?: boolean };
}

// ✅ 修复：用计算属性实现响应式绑定（解决 checkbox 选中卡顿）
const rowSelection = computed<CustomTableRowSelection<DataItem>>(() => ({
  type: 'checkbox',
  selectedRowKeys: selectedRowKeys.value, // 计算属性自动同步选中状态
  onChange: (selectedKeys, selectedRows) => {
    selectedRowKeys.value = selectedKeys as string[];
    console.log('选中的行ID：', selectedRowKeys.value);
    console.log('选中的行数据：', selectedRows);
  },
  preserveSelectedRowKeys: false,
  getCheckboxProps: (record) => ({
    disabled: isSyncing.value, // 同步时禁用复选框，避免冲突
  }),
}));

const columns = ref([
  {
    title: '同步时间',
    dataIndex: 'syncTimeStr',
    align: 'center',
    width: 180,
    sorter: true, // 开启排序
    // 绑定排序状态：当前排序字段是syncTime时显示对应排序方向
    sortOrder: sortParams.value.field === 'syncTime' ? sortParams.value.order : null,
    // 点击表头触发排序，指定排序字段为syncTime（对应后端字段）
    onHeaderCell: () => ({
      onClick: () => {
        handleSortChange('syncTime');
      },
    }),
  },
  {
    title: '发布时间',
    dataIndex: 'createTimeStr',
    align: 'center',
    width: 180,
    sorter: true,
    sortOrder: sortParams.value.field === 'createTime' ? sortParams.value.order : null,
    onHeaderCell: () => ({
      onClick: () => {
        handleSortChange('createTime');
      },
    }),
  },
  {
    title: '同步类型',
    dataIndex: 'viedoTypeStr',
    align: 'center',
    width: 120,
  },
  {
    title: '博主',
    dataIndex: 'author',
    align: 'center',
    width: 150,
    sorter: true,
    sortOrder: sortParams.value.field === 'author' ? sortParams.value.order : null,
    onHeaderCell: () => ({
      onClick: () => {
        handleSortChange('author');
      },
    }),
  },
  {
    title: '视频类型',
    dataIndex: 'viedoCate',
    width: 200,
    align: 'center',
  },
  {
    title: '视频标题',
    dataIndex: 'videoTitle',
    align: 'left',
    width: 350,
  },
  {
    title: 'CK名称',
    dataIndex: 'dyUser',
    align: 'center',
    width: 120,
  },
  {
    title: '操作',
    key: 'operation',
    align: 'center',
    width: 180,
  },
]);

// 📌支持同步时间/发布时间/博主列的排序图标正确更新
const handleSortChange = (field: string) => {
  // 如果点击的是当前排序字段，切换排序方向
  if (sortParams.value.field === field) {
    sortParams.value.order = sortParams.value.order === 'ascend' ? 'descend' : 'ascend';
  } else {
    // 新排序字段，默认降序
    sortParams.value.field = field;
    sortParams.value.order = 'descend';
  }

  // 遍历所有列，根据排序字段映射更新对应列的sortOrder（核心修复）
  columns.value.forEach((col) => {
    // 字段映射：列的dataIndex -> 后端排序字段sortParams.field
    const fieldMap = {
      syncTimeStr: 'syncTime',
      createTimeStr: 'createTime',
      author: 'author',
    };
    // 只有当前排序字段对应的列，显示排序方向，其他列置空
    col.sortOrder =
      fieldMap[col.dataIndex as keyof typeof fieldMap] === sortParams.value.field ? sortParams.value.order : null;
  });

  // 重新查询数据（传递排序参数）
  GetRecords();
};
// 监听批量操作开关状态变化，清空选中状态+强制表格重绘
watch(isBatchMode, (isOpen) => {
  if (!isOpen) {
    selectedRowKeys.value = [];
    // 强制表格重新渲染，解决状态残留问题
    nextTick(() => {
      const tableEl = document.querySelector('.ant-table') as HTMLElement;
      if (tableEl) {
        tableEl.setAttribute('key', Date.now().toString());
      }
    });
  }
});

// 基础状态（优化：删除冗余的 datas 响应式数组）
const loading = ref(false);
const showImageViedo = ref(true);
const dataSource = ref<DataItem[]>([]); // 直接用 ref 数组存储表格数据，减少响应式嵌套

// 查询参数
const value1 = ref<RangeValue>();
const ranges = {
  今天: [dayjs(), dayjs()] as RangeValue,
  本月: [dayjs(), dayjs().endOf('month')] as RangeValue,
};

const value2 = ref<RangeValue>();
const ranges2 = {
  今天: [dayjs(), dayjs()] as RangeValue,
  本月: [dayjs(), dayjs().endOf('month')] as RangeValue,
};
const quaryData: UnwrapRef<QuaryParam> = reactive({
  pageIndex: 0,
  pageSize: 20,
  author: '',
  title: '',
  viedoType: '*',
  authorId: '',
  fileHash: '',
  sortField: 'createTime', // 📌 默认排序字段
  sortOrder: 'desc', // 📌 默认降序
});

// 分页配置
const pagination = ref({
  current: 1,
  defaultPageSize: 10,
  total: 0,
  showSizeChanger: true, // 强制显示「每页显示数量」下拉框（关键修复）
  showTotal: () => `共 ${0} 条`,
  // showQuickJumper: true, // 显示快速跳转输入框（可选，增强体验）
  pageSizeOptions: ['10', '20', '50', '100'], // 自定义每页条数选项（可选）
  showSizeChange: (current, pageSize) => {
    // 可选：监听每页条数变化，重置当前页为第1页（避免最后一页数据不足的问题）
    pagination.value.current = 1;
    pagination.value.defaultPageSize = pageSize;
    GetRecords();
  },
});

// 视频播放相关配置
const DEFAULT_LOW_VOLUME = 0.3;
const isVideoLoading = ref(false); // 视频加载状态
const isSyncing = ref(false); // 同步状态
const currentVideoInfo = ref<DataItem | null>(null); // 当前播放视频信息

// 视频弹窗相关状态
const isModalOpen = ref(false);
const videoRef = ref<HTMLVideoElement | null>(null);
const videoUrl = ref('');
const hasError = ref(false);
const errorMessage = ref('');
const autoPlay = ref(true);
const autoMuted = ref(true);
const videoId = ref('');
const playingTitle = ref('');
let videoProgressListener: ((e: Event) => void) | null = null; // 进度监听器

/** 格式化存储路径（过长时中间省略） */
const formatFilePath = (filePath?: string) => {
  if (!filePath) return '暂无存储路径信息';
  // 路径超过80字符时，保留前40和后30字符，中间用...省略
  if (filePath.length > 80) {
    return `${filePath.slice(0, 40)}...${filePath.slice(-30)}`;
  }
  return filePath;
};

// -------------------------- 核心工具方法 --------------------------

const formatPathSeparator = (path: string | undefined) => {
  if (!path) return path; // 处理空路径情况
  // 正则表达式 /\\/g 表示全局匹配所有反斜杠
  return path.replace(/\\/g, '/');
};
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

// -------------------------- 核心业务方法 --------------------------
/** 查询表格数据 */
const GetRecords = () => {
  loading.value = true;
  quaryData.pageIndex = pagination.value.current;
  quaryData.pageSize = pagination.value.defaultPageSize;

  if (value1.value) {
    quaryData.dates = value1.value.map((date) => date.format('YYYY-MM-DD'));
  }
  if (value2.value) {
    quaryData.dates2 = value2.value.map((date) => date.format('YYYY-MM-DD')); // 修复：之前误写为value1
  }
  // 📌 关键：将前端排序状态转换为后端需要的参数
  quaryData.sortField = sortParams.value.field;
  // 转换排序方向（antd的ascend/descend 转 后端常用的asc/desc）
  quaryData.sortOrder = sortParams.value.order === 'ascend' ? 'asc' : 'desc';
  useApiStore()
    .VideoPageList(quaryData)
    .then((res) => {
      loading.value = false;
      if (res.code === 0) {
        dataSource.value = res.data.data; // 直接更新 ref 数组，优化响应式
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

// 📌 修复：分页时无排序操作，强制保留默认syncTime排序
const handleTableChange = (paginationObj: any, filters: any, sorter: any) => {
  pagination.value.current = paginationObj.current;
  pagination.value.defaultPageSize = paginationObj.pageSize;

  // 1. 如果是排序变化（用户点击表头），更新排序参数
  if (sorter.field) {
    // 列dataIndex -> 后端排序字段的映射
    const fieldMap: Record<string, string> = {
      syncTimeStr: 'syncTime',
      createTimeStr: 'createTime',
      author: 'author',
    };
    // 转换排序字段
    sortParams.value.field = fieldMap[sorter.field] || sorter.field;
    sortParams.value.order = sorter.order;

    // 更新所有列的排序图标
    columns.value.forEach((col) => {
      col.sortOrder = fieldMap[col.dataIndex as string] === sortParams.value.field ? sorter.order : null;
    });
  }
  // 2. 分页跳转（无排序操作），强制恢复默认排序syncTime的图标状态
  else if (!sorter.field && sortParams.value.field !== 'syncTime') {
    // 重置排序参数为默认：syncTime 降序
    sortParams.value.field = 'syncTime';
    sortParams.value.order = 'descend';
    // 刷新列的排序图标，只显示同步时间列的降序
    columns.value.forEach((col) => {
      col.sortOrder = col.dataIndex === 'syncTimeStr' ? 'descend' : null;
    });
  }

  // 分页变化时清空选中状态
  if (isBatchMode.value) {
    selectedRowKeys.value = [];
  }

  // 重新查询数据（携带正确的排序参数）
  GetRecords();
};

/** 立即同步 */
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

/** 同步日期选择器变化事件 */
const datePicked = (_, dateArry: RangeValue) => {
  quaryData.dates = dateArry.map((date) => date.format('YYYY-MM-DD'));
  console.log('选择的同步日期范围:', quaryData.dates);
};

/** 发布日期选择器变化事件 */
const datePicked2 = (_, dateArry: RangeValue) => {
  quaryData.dates2 = dateArry.map((date) => date.format('YYYY-MM-DD'));
  console.log('选择的发布日期范围:', quaryData.dates2);
};

/** 表格分页/排序变化事件 */
// const handleTableChange = (paginationObj: any) => {
//   pagination.value.current = paginationObj.current;
//   pagination.value.defaultPageSize = paginationObj.pageSize;
//   // 分页变化时清空选中状态（跨页不保留）
//   if (isBatchMode.value) {
//     selectedRowKeys.value = [];
//   }
//   GetRecords();
// };

/** 视频类型切换事件 */
const onViedoTypeChanged = () => {
  GetRecords();
};

// -------------------------- 视频播放相关方法 --------------------------
/** 点击视频标题播放 */
const handleVideoClick = (record: DataItem) => {
  if (record.isMergeVideo && record.videoSavePath.length == 0) {
    message.warning('图文视频配置：不下载视频，所有没有可播放的视频');
    return;
  }
  // 保存当前视频信息
  currentVideoInfo.value = record;
  console.log(currentVideoInfo);
  videoId.value = record.id;
  playingTitle.value = formatModalTitle(record.videoTitle);
  // 重置错误状态
  hasError.value = false;
  // 显示弹窗（触发watch加载视频）
  isModalOpen.value = true;
};

/** 加载视频（优化：简化逻辑，避免内存泄漏） */
const loadVideo = () => {
  if (!videoRef.value || !videoId.value) return;

  isVideoLoading.value = true;

  // 移除之前的监听器
  if (videoProgressListener) {
    videoRef.value.removeEventListener('progress', videoProgressListener);
    videoProgressListener = null;
  }

  // 拼接视频地址（添加时间戳避免缓存）
  const timestamp = new Date().getTime();
  videoUrl.value = `${import.meta.env.VITE_API_URL}api/Video/play/${videoId.value}?t=${timestamp}`;

  // 直接赋值src并加载
  videoRef.value.src = videoUrl.value;

  // 重新绑定进度监听器
  videoProgressListener = handleVideoProgress;
  videoRef.value.addEventListener('progress', videoProgressListener);

  // 触发加载
  videoRef.value.load();
};

/** 视频加载进度处理 */
const handleVideoProgress = (e: Event) => {
  const video = e.target as HTMLVideoElement;
  if (video.buffered.length > 0) {
    const bufferedEnd = video.buffered.end(video.buffered.length - 1);
    const duration = video.duration;
    // 缓冲达到90%以上隐藏加载动画
    if (duration > 0 && bufferedEnd / duration > 0.9) {
      isVideoLoading.value = false;
    }
  }
};

/** 暂停视频并释放资源 */
const pauseVideo = () => {
  if (!videoRef.value) return;

  const video = videoRef.value;
  // 暂停播放
  video.pause();
  // 移除监听器
  if (videoProgressListener) {
    video.removeEventListener('progress', videoProgressListener);
    videoProgressListener = null;
  }
  // 清空src
  video.src = '';
  // 重置状态
  isVideoLoading.value = false;
};

/** 视频错误处理 */
const handleVideoError = (e: Event) => {
  const video = e.target as HTMLVideoElement;
  const errorCode = video.error?.code;

  const errorMap: Record<number, string> = {
    1: '视频加载中断',
    2: '网络错误（跨域未配置/后端服务未启动/接口不可用）',
    3: '视频解码失败（格式不支持或文件损坏）',
    4: '视频格式不支持',
    5: '视频文件不存在或后端权限不足',
  };

  if (!video.src) {
    errorMessage.value = '视频地址为空，请重试';
  } else {
    errorMessage.value = `加载失败：${errorMap[errorCode as number] || '未知错误'}（视频ID：${videoId.value}）`;
  }

  hasError.value = true;
  isVideoLoading.value = false;
  console.error('视频播放错误详情：', video.error);
};

/** 关闭视频弹窗 */
const handleCancel = () => {
  // 暂停视频并释放资源
  pauseVideo();
  // 立即关闭弹窗
  isModalOpen.value = false;
  // 延迟重置状态
  setTimeout(() => {
    currentVideoInfo.value = null;
    videoUrl.value = '';
    videoId.value = '';
    playingTitle.value = '';
  }, 100);
};

// 监听弹窗状态，加载/释放视频
watch(
  isModalOpen,
  (isOpen) => {
    if (isOpen) {
      // 弹窗打开时，延迟加载视频（给DOM渲染时间）
      nextTick(() => {
        loadVideo();
      });
    } else {
      // 弹窗关闭时，立即暂停视频
      pauseVideo();
    }
  },
  { immediate: false }
);

// -------------------------- 批量操作和操作列事件 --------------------------
/** 批量删除事件 */
const handleBatchSync = () => {
  if (selectedRowKeys.value.length === 0) {
    message.warning('请先选择要重新下载的视频');
    return;
  }

  Modal.confirm({
    title: '确认重新下载吗',
    content: `您确定要重新下载选中的 ${selectedRowKeys.value.length} 条视频数据吗？`,
    okText: '确认重新下载',
    cancelText: '取消',
    okType: 'danger',
    onOk: async () => {
      reDownload({ ids: selectedRowKeys.value });
    },
  });
};

const handleBatchDelete = () => {
  if (selectedRowKeys.value.length === 0) {
    message.warning('请先选择要彻底删除的视频');
    return;
  }

  Modal.confirm({
    title: '确认删除这些下载的视频吗',
    content: `您确定要彻底下删除选中的 ${selectedRowKeys.value.length} 条视频数据吗？`,
    okText: '确认彻底删除',
    cancelText: '取消',
    okType: 'danger',
    onOk: async () => {
      deleteBatch({ ids: selectedRowKeys.value });
    },
  });
};

const deleteVideoShow = ref(false);
const handShowDeleteVideos = () => {
  deleteVideoShow.value = true;
  getDeleteViedos();
};

const deleteVideos = ref([]);
const getDeleteViedos = () => {
  useApiStore()
    .GetDeleteViedos()
    .then((res) => {
      deleteVideos.value = res.data;
    });
};
const onDeleteVideoClose = (e) => {
  deleteVideoShow.value = false;
};

const reDownload = (param: object) => {
  try {
    loading.value = true;
    console.log('执行批量删除，选中ID：', selectedRowKeys.value);

    useApiStore()
      .ReDownViedos(param)
      .then((res) => {
        loading.value = false;
        if (res.code === 0) {
          message.success('删除成功，下次任务执行时会重新下载');
          // 刷新数据并清空选中状态
          GetRecords();
          selectedRowKeys.value = [];
        } else {
          message.warning(res.message || '获取数据失败');
        }
      })
      .catch((error) => {
        loading.value = false;
      });
  } catch (error) {
    console.error('批量删除失败：', error);
    message.error('删除失败，请稍后重试');
  } finally {
    loading.value = false;
  }
};

const deleteBatch = (param: object) => {
  try {
    loading.value = true;
    console.log('执行批量删除，选中ID：', selectedRowKeys.value);

    useApiStore()
      .BathRealDelete(param)
      .then((res) => {
        loading.value = false;
        if (res.code === 0) {
          message.success('删除成功，以后都不会下载了哦，你自己选的');
          // 刷新数据并清空选中状态
          GetRecords();
          selectedRowKeys.value = [];
        } else {
          message.warning(res.message || '获取数据失败');
        }
      })
      .catch((error) => {
        loading.value = false;
      });
  } catch (error) {
    console.error('批量删除失败：', error);
    message.error('删除失败，请稍后重试');
  } finally {
    loading.value = false;
  }
};

/** 重新下载事件 */
const handleReDownload = (record: DataItem) => {
  if (!record.id) {
    message.warning('视频ID不存在，无法重新下载');
    return;
  }

  try {
    loading.value = true;
    const _ids = [record.id];
    reDownload({ ids: _ids });
  } catch (error) {
    console.error('重新下载失败：', error);
    message.error('重新下载失败，请稍后重试');
    loading.value = false;
  }
};

const handleBatchShare = () => {
  const matchedItems = dataSource.value.filter((item) => selectedRowKeys.value.includes(item.id));
  try {
    // console.log('执行分享操作，视频ID：', record.id, '视频标题：', record.videoTitle);
    // 生成分享链接
    const currentDomain = window.location.origin;
    let shareUrl = '';
    matchedItems.forEach((record) => {
      let k = CryptoJS.MD5(record.fileHash + record.authorId).toString();
      shareUrl += `${currentDomain}/share/${record.id}/${k}
      `;
    });
    copyToClipboard(shareUrl, '分享链接已复制到剪贴板！');
  } catch (error) {
    console.error('分享失败：', error);
    message.error('分享功能异常，请稍后重试');
  }
};

// 复制链接到剪贴板（兼容生产环境）
const copyToClipboard = async (shareUrl: string, msg: string) => {
  try {
    // 方案1：优先使用 navigator.clipboard（现代浏览器+HTTPS环境）
    if (navigator.clipboard && typeof navigator.clipboard.writeText === 'function') {
      await navigator.clipboard.writeText(shareUrl);
      message.success(msg);
    } else {
      // 方案2：降级使用 document.execCommand（兼容HTTP/旧浏览器）
      const textarea = document.createElement('textarea');
      // 隐藏文本域（避免影响页面布局）
      textarea.style.position = 'absolute';
      textarea.style.top = '-9999px';
      textarea.style.left = '-9999px';
      // 设置要复制的内容
      textarea.value = shareUrl;
      document.body.appendChild(textarea);
      // 选中并复制
      textarea.select();
      const success = document.execCommand('copy');
      document.body.removeChild(textarea); // 清理DOM

      if (success) {
        message.success(msg);
      } else {
        // 方案3：最终降级 - 显示链接让用户手动复制
        throw new Error('自动复制失败');
      }
    }
  } catch (error) {
    console.warn('复制失败，触发手动复制方案：', error);
    // 最终降级：显示链接弹窗
    Modal.info({
      title: '视频分享',
      content: `
        <p>分享链接：<a href="${shareUrl}" target="_blank" rel="noopener noreferrer">${shareUrl}</a></p>
        <p style="margin-top: 8px; color: #666;">请手动复制链接后分享给他人</p>
      `,
      okText: '已复制',
      onOk: () => {},
    });
  }
};
/** 分享事件 */
const handleShare = (record: DataItem) => {
  if (!record.id) {
    message.warning('视频ID不存在，无法分享');
    return;
  }

  try {
    const currentDomain = window.location.origin;
    // console.log('执行分享操作，视频ID：', record.id, '视频标题：', record.videoTitle);
    // 生成分享链接
    let k = CryptoJS.MD5(record.fileHash + record.authorId).toString();
    const shareUrl = `${currentDomain}/share/${record.id}/${k}`;
    copyToClipboard(shareUrl, '分享链接已复制到剪贴板！');
  } catch (error) {
    console.error('分享失败：', error);
    message.error('分享功能异常，请稍后重试');
  }
};

//视频删除不再下载
const handleDelete = (record: DataItem) => {
  Modal.confirm({
    title: '确认删除',
    content: `您确定要删除这条视频数据吗？此操作不可撤销，以后也不会再下载！！！`,
    okText: '确认删除',
    cancelText: '取消',
    okType: 'danger',
    onOk: async () => {
      try {
        useApiStore()
          .DeleteVideo(record.id)
          .then((res) => {
            if (res.code == 0) {
              message.success('删除成功,再也不会下载！！！');
            } else {
              message.error('删除失败');
            }
            GetRecords();
          });
      } catch (error) {
        console.error('删除失败', error);
        message.error('视频删除失败，请稍后再试');
      }
    },
  });
};

// 新增：复制视频路径方法
const copyVideoPath = (path?: string) => {
  if (!path) {
    message.warning('暂无视频存储路径');
    return;
  }
  copyToClipboard(path, '视频保存路径已复制到剪贴板！');
};

// -------------------------- 页面初始化 --------------------------
onMounted(() => {
  // getConfig();
  GetRecords();
});
</script>

<style>
/* 新增：优化视频元素的过渡效果，避免关闭时的视觉卡顿 */
.video-element {
  width: 100%;
  height: auto;
  max-height: 420px;
  min-height: 250px;
  background-color: #000;
  object-fit: contain;
  opacity: 1;
  transition: opacity 0.2s ease-in-out; /* 缩短过渡时间 */
  will-change: opacity; /* 告诉浏览器提前优化渲染 */
}
/* 新增：查询区域样式优化 */
.query-container {
  margin: 16px 0;
  padding: 16px;
  border-radius: 8px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
}

.query-form {
  width: 100%;
}

.form-row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  margin-bottom: 12px;
}

.form-row:last-child {
  margin-bottom: 0;
}

.form-item {
  margin-bottom: 0 !important;
  margin-right: 24px !important;
  display: flex;
  align-items: center;
}

/* 核心修改：主查询行自适应布局 */
.form-main-row {
  display: flex;
  flex-wrap: nowrap; /* 禁止换行 */
  align-items: center;
  width: 100%;
  overflow: hidden; /* 防止溢出 */
}

/* 日期选择器项：固定基础宽度，自适应收缩 */
.form-item-date {
  flex: 0 1 280px; /* 不放大，可缩小，基础宽度280px */
  min-width: 220px; /* 最小宽度，防止过度收缩 */
}

/* 输入框项：自适应拉伸填充剩余空间 */
.form-item-input {
  flex: 1 1 auto; /* 可放大，可缩小，自动宽度 */
  min-width: 180px; /* 最小宽度，保证可用性 */
}

/* 日期选择器自适应宽度 */
.range-picker {
  width: 100% !important; /* 占满父容器宽度 */
  min-width: 200px !important;
}

/* 输入框自适应宽度 */
.query-input {
  width: 100% !important; /* 占满父容器宽度 */
  min-width: 160px !important;
}

/* 新增：批量操作开关样式 */
.batch-operation-item {
  margin-left: 20px !important;
}

.batch-switch {
  --ant-switch-height: 24px;
  --ant-switch-width: 80px;
}

/* 新增：删除按钮样式 */
.delete-button {
  min-width: 100px;
}

/* 单选组样式 */
.video-type-radio {
  display: flex;
  flex-wrap: wrap;
}

.radio-group-item {
  flex: 1;
  min-width: 300px;
}

/* 按钮组样式 - 关键修改：保持原有布局 */
.button-group-item {
  margin-left: 8px !important; /* 仅保留少量间距，不使用auto */
  margin-right: 0 !important;
  display: flex !important;
  align-items: center !important;
}

.button-group {
  display: flex;
  gap: 12px;
}

.query-button,
.sync-button {
  min-width: 100px;
}

/* 核心修复：操作行布局 - 关键修改 */
.form-actions-row {
  display: flex;
  align-items: center;
  justify-content: flex-start;
  width: 100%;
  min-height: 40px;
  box-sizing: border-box;
  /* 移除之前的padding-right，避免影响其他按钮 */
  padding-right: 0 !important;
}

/* 已删除按钮容器 - 独立定位，不影响其他按钮 */
.delete-btn-2-wrapper {
  margin-left: auto !important; /* 自动靠右，不影响左侧按钮 */
  margin-right: 0 !important;
  padding: 0 !important;
  width: 100px !important;
  height: 32px !important;
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
}

/* 响应式调整：屏幕较小时允许主查询行换行 */
@media (max-width: 1440px) {
  .form-main-row {
    flex-wrap: wrap; /* 允许换行 */
  }
  .form-item-date,
  .form-item-input {
    margin-bottom: 12px !important; /* 换行后添加底部间距 */
  }
}

@media (max-width: 1200px) {
  .form-actions-row {
    flex-wrap: wrap; /* 允许其他元素换行 */
    min-height: 60px; /* 增大行高 */
  }
  .batch-operation-item {
    margin-left: 20px !important;
    margin-top: 8px !important;
  }
  /* 响应式下按钮组调整 */
  .button-group-item {
    margin-left: 20px !important;
    margin-top: 8px !important;
  }
  /* 已删除按钮在小屏幕下换行显示 */
  .delete-btn-2-wrapper {
    margin-left: 20px !important;
    margin-top: 8px !important;
    margin-right: 0 !important;
    width: auto !important;
  }
}

@media (max-width: 992px) {
  .form-item {
    margin-right: 16px !important;
  }
}

@media (max-width: 768px) {
  .form-item-date,
  .form-item-input {
    flex: 1 1 100%; /* 占满整行 */
    min-width: unset;
  }
  .button-group {
    width: 100%;
    justify-content: space-between;
  }
  .query-button,
  .sync-button,
  .delete-button {
    flex: 1;
    margin: 0 4px;
  }
}

/* 原有样式保持不变 */
.video-container {
  position: relative;
  border-bottom: 1px solid #e8e8e8;
  overflow: hidden;
  max-height: 420px;
}

.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.7);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  z-index: 10;
  transition: all 0.3s ease;
}

.loading-tip {
  color: #ffffff;
  font-size: 16px;
  margin-top: 20px;
  text-align: center;
  padding: 0 20px;
}

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

.video-info-bar {
  padding: 16px 24px;
  background: #f8f9fa;
  border-bottom: 1px solid #e8e8e8;
}

.info-container {
  display: flex;
  gap: 40px;
  align-items: center;
  flex-wrap: wrap;
}

.info-item {
  display: flex;
  flex: 1;
  align-items: center;
  font-size: 14px;
  line-height: 1.6;
  flex-wrap: nowrap;
}

.info-label {
  color: #666666;
  margin-right: 8px;
  white-space: nowrap;
  font-weight: 500;
}

.info-value {
  color: #333333;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  margin-right: 8px;
}

/* 新增：复制路径按钮样式 */
.copy-path-btn {
  padding: 0 6px !important;
  height: 24px !important;
  font-size: 12px !important;
  white-space: nowrap;
}

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

:deep(.ant-modal-title) {
  font-size: 16px !important;
  font-weight: 500 !important;
  color: #1f2937 !important;
  line-height: 1.5 !important;
  white-space: nowrap !important;
  overflow: hidden !important;
  text-overflow: ellipsis !important;
  max-width: calc(100% - 40px) !important;
}

:deep(.ant-modal) {
  border-radius: 8px !important;
  box-shadow: 0 6px 30px rgba(0, 0, 0, 0.1) !important;
  overflow: hidden !important;
  max-width: 85vw !important;
  max-height: 80vh !important;
  min-width: 500px !important;
  min-height: 380px !important;
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
  background-color: #f0f9ff !important;
}

:deep(.ant-modal-content) {
  border-radius: 8px !important;
  overflow: hidden !important;
}

:deep(.ant-modal-mask) {
  background-color: rgba(0, 0, 0, 0.5) !important;
  backdrop-filter: blur(2px) !important;
}

:deep(.ant-spin-dot) {
  color: #1890ff !important;
  font-size: 36px !important;
}

:deep(.ant-spin-tip) {
  color: #ffffff !important;
  font-size: 16px !important;
  margin-top: 20px !important;
}

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

/* 新增：表格复选框列样式调整 */
:deep(.ant-table-selection-column) {
  width: 50px !important;
  text-align: center !important;
}

/* 新增：操作列按钮样式 */
:deep(.ant-space-item button) {
  padding: 0 8px !important;
  height: 28px !important;
  font-size: 13px !important;
}

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
  :deep(.ant-modal-title) {
    max-width: calc(100% - 30px) !important;
    font-size: 15px !important;
  }
  :deep(.ant-spin-dot) {
    font-size: 28px !important;
  }
  .loading-tip {
    font-size: 14px;
  }
  /* 响应式下操作列调整 */
  :deep(.ant-table-column-has-fix-right) {
    right: 0 !important;
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
  :deep(.ant-modal-title) {
    max-width: calc(100% - 25px) !important;
    font-size: 14px !important;
  }
  /* 移动端操作列换行显示 */
  :deep(.ant-space) {
    flex-direction: column !important;
    align-items: flex-start !important;
    gap: 4px !important;
  }
}
/* 弹窗标题悬停样式 */
.modal-title-with-tooltip {
  position: relative;
  cursor: help; /* 鼠标变为帮助图标，提示可悬停 */
  padding: 2px 0;
}

/* 可选：添加下划线动画增强交互提示 */
.modal-title-with-tooltip:hover {
  text-decoration: underline;
  text-underline-offset: 4px;
  text-decoration-color: #1890ff;
  text-decoration-thickness: 1px;
}
/* 已删除视频抽屉 - 列表容器基础样式 */
:deep(.ant-drawer-body) {
  padding: 16px !important;
  overflow-y: auto;
}

:deep(.ant-list) {
  margin: 0 !important;
}

/* 已删除视频 - 列表项布局优化 */
:deep(.ant-list-item) {
  display: flex !important;
  align-items: center !important;
  justify-content: space-between !important;
  padding: 12px 16px !important;
  border-bottom: 1px solid #f0f0f0 !important;
  transition: background-color 0.2s ease;
}

/* 列表项悬停效果，增强交互感 */
:deep(.ant-list-item:hover) {
  background-color: #f8f9fa !important;
}

/* 已删除视频 - 标题容器（核心：实现单行省略） */
.delete-video-title-container {
  display: flex;
  align-items: center;
  flex: 1; /* 占满左侧剩余空间，限制文本宽度 */
  margin-right: 16px; /* 与复制按钮保持间距 */
  overflow: hidden; /* 隐藏溢出内容 */
}

/* 序号样式 */
.delete-video-index {
  color: #666;
  margin-right: 8px;
  flex: 0 0 auto; /* 序号不收缩、不放大，固定宽度 */
  white-space: nowrap;
}

/* 视频标题（核心：单行文本溢出省略） */
.delete-video-title {
  flex: 1; /* 占满容器剩余空间，触发宽度限制 */
  white-space: nowrap; /* 强制文本单行显示 */
  overflow: hidden; /* 隐藏溢出的文本 */
  text-overflow: ellipsis; /* 溢出部分显示省略号... */
  color: #333;
  font-size: 14px;
  line-height: 1.5;
}

/* 复制按钮样式优化 */
.copy-delete-video-btn {
  padding: 0 8px !important;
  height: 28px !important;
  font-size: 12px !important;
  color: #1890ff !important;
  flex: 0 0 auto; /* 按钮不收缩、不放大，固定宽度 */
}

.copy-delete-video-btn:hover {
  color: #40a9ff !important;
  background-color: #f0f9ff !important;
  border-radius: 4px !important;
}

/* 可选：适配移动端，优化小屏幕显示 */
@media (max-width: 768px) {
  .delete-video-title-container {
    margin-right: 12px;
  }

  .delete-video-title {
    font-size: 13px;
  }

  .copy-delete-video-btn {
    padding: 0 6px !important;
    height: 24px !important;
  }
}

/* 📌 新增：博主列排序图标样式优化（和发布时间列保持一致） */
:deep(.ant-table-column-title[data-column-key='author']) {
  cursor: pointer;
}

:deep(.ant-table-column-title[data-column-key='author']:hover) {
  color: #1890ff !important;
}
</style>