<template>
  <div>
    <!-- 查询条件区域：仅优化布局与样式 -->
    <div class="query-container">
      <a-form :model="quaryData" class="query-form">
        <!-- 第一行：基础筛选条件 -->
        <div class="query-fields-grid">
          <a-form-item label="同步日期" class="query-field query-field-date">
            <a-range-picker v-model:value="value1" :ranges="ranges" :locale="locale" @change="datePicked" class="query-control range-picker" />
          </a-form-item>

          <a-form-item label="发布日期" class="query-field query-field-date">
            <a-range-picker v-model:value="value2" :ranges="ranges2" :locale="locale" @change="datePicked2" class="query-control range-picker" />
          </a-form-item>

          <a-form-item label="博主" ref="author" name="author" class="query-field">
            <a-input v-model:value="quaryData.author" class="query-control query-input" placeholder="请输入博主名称" />
          </a-form-item>

          <a-form-item label="标题" ref="title" name="title" class="query-field">
            <a-input v-model:value="quaryData.title" class="query-control query-input" placeholder="请输入视频标题" />
          </a-form-item>
        </div>

        <!-- 第二行：账号、视频类型和操作按钮 -->
        <div class="query-toolbar">
          <div class="query-toolbar-main">
            <a-form-item label="账号" class="query-account-field">
              <a-select ref="select" v-model:value="quaryData.cookieId" class="account-select" :options="cookies" />
            </a-form-item>

            <a-form-item label="视频类型" class="query-type-field">
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
          </div>

          <div class="query-toolbar-actions">
            <a-button type="primary" @click="GetRecords" class="query-button">
              <SearchOutlined />
              <span>查询</span>
            </a-button>

            <div class="batch-mode-control">
              <span class="batch-mode-label">批量操作</span>
              <a-switch v-model:checked="isBatchMode" checked-children="开" un-checked-children="关" class="batch-switch" />
            </div>

            <div v-if="isBatchMode" class="batch-action-group">
              <a-button @click="handleBatchSync" class="batch-sync-button" :disabled="selectedRowKeys.length === 0 || isSyncing">
                <SyncOutlined />
                重新下载
              </a-button>

              <a-button danger @click="handleBatchDelete" class="batch-delete-button" :disabled="selectedRowKeys.length === 0 || isSyncing">
                <DeleteOutlined />
                永久删除
              </a-button>
            </div>

            <a-button danger @click="handShowDeleteVideos" class="deleted-records-button">
              <DeleteOutlined />
              已删除
            </a-button>
          </div>
        </div>
      </a-form>
    </div>

    <!-- 已删除视频-抽屉 -->

    <a-drawer title="已删除视频" size="large" :visible="deleteVideoShow" @close="onDeleteVideoClose" class="deleted-video-drawer">
      <template #extra>
        <span class="deleted-video-total">共 {{ deleteVideos.length }} 条</span>
      </template>

      <div class="deleted-video-list-wrapper">
        <a-list size="small" bordered :data-source="pagedDeleteVideos" class="deleted-video-list">
          <template #renderItem="{ item, index }">
            <a-list-item>
              <div class="delete-video-title-container">
                <span class="delete-video-index">
                  {{ (deleteVideoPagination.current - 1) * deleteVideoPagination.pageSize + index + 1 }}.
                </span>
                <span class="delete-video-title" :title="item.videoTitle || '无标题'">
                  {{ item.videoTitle || '无标题' }}
                </span>
              </div>
            </a-list-item>
          </template>

          <template #empty>
            <div class="deleted-video-empty">暂无已删除视频</div>
          </template>
        </a-list>

        <div v-if="deleteVideos.length > 0" class="deleted-video-pagination">
          <a-pagination v-model:current="deleteVideoPagination.current" v-model:page-size="deleteVideoPagination.pageSize" :total="deleteVideos.length" :page-size-options="['10', '20', '50', '100']" show-size-changer show-less-items :show-total="(total) => `共 ${total} 条`" />
        </div>
      </div>
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
    <a-table class="record-table" size="small" :columns="columns" :data-source="dataSource" bordered :pagination="pagination" table-layout="fixed" @change="handleTableChange" :loading="loading" :row-selection="isBatchMode ? rowSelection : null" row-key="id">
      <template #bodyCell="{ column, record }">
        <template v-if="column.dataIndex === 'createTimeStr'">
          <span class="publish-date-text">{{ formatPublishDate(record.createTimeStr) }}</span>
        </template>

        <template v-if="column.dataIndex === 'videoTitle'">
          <a class="video-title-link" :title="record.videoTitle || '无标题'" @click="handleVideoClick(record)" @mouseenter="handleTitleMouseEnter" @mouseleave="handleTitleMouseLeave">
            {{ formatVideoTitle(record.videoTitle) }}
          </a>
        </template>
        <template v-if="column.key === 'operation'">
          <div class="operation-actions">
            <a-tooltip title="重新同步">
              <a-button type="text" shape="circle" class="operation-icon-btn operation-sync-btn" @click="handleReDownload(record)" :disabled="isSyncing" aria-label="重新同步">
                <SyncOutlined />
              </a-button>
            </a-tooltip>

            <a-tooltip title="分享">
              <a-button type="text" shape="circle" class="operation-icon-btn operation-share-btn" @click="handleShare(record)" :disabled="!record.id" aria-label="分享">
                <ShareAltOutlined />
              </a-button>
            </a-tooltip>

            <a-tooltip title="永久删除">
              <a-button type="text" shape="circle" danger class="operation-icon-btn operation-delete-btn" @click="handleDelete(record)" :disabled="!record.id" aria-label="永久删除">
                <DeleteOutlined />
              </a-button>
            </a-tooltip>
          </div>
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
import { useRoute } from 'vue-router';
import { useRouteParamStore } from '@/store/routeParam';
import {
  SearchOutlined,
  SyncOutlined,
  ShareAltOutlined,
  ClearOutlined,
  CopyOutlined,
  DeleteOutlined,
} from '@ant-design/icons-vue';

const route = useRoute();
const paramStore = useRouteParamStore();

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
  cookieId?: string;
}

// 引入dayjs中文包
import 'dayjs/locale/zh-cn';
import { forEach } from 'lodash';
dayjs.locale('zh-cn');

// 批量操作相关状态
const isBatchMode = ref(false); // 批量操作开关状态
const selectedRowKeys = ref<string[]>([]); // 选中的行ID集合
const isSyncing = ref(false); // 同步状态（必须在 rowSelection computed 之前初始化）
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
    title: 'CK名称',
    key: 'dyUser',
    dataIndex: 'dyUser',
    align: 'center',
    width: 110,
    ellipsis: true,
  },
  {
    title: '同步时间',
    key: 'syncTimeStr',
    dataIndex: 'syncTimeStr',
    align: 'center',
    width: 160,
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
    key: 'createTimeStr',
    dataIndex: 'createTimeStr',
    align: 'center',
    width: 110,
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
    key: 'viedoTypeStr',
    dataIndex: 'viedoTypeStr',
    align: 'center',
    width: 110,
    ellipsis: true,
  },
  {
    title: '博主',
    key: 'author',
    dataIndex: 'author',
    align: 'center',
    width: 150,
    ellipsis: true,
    sorter: true,
    sortOrder: sortParams.value.field === 'author' ? sortParams.value.order : null,
    onHeaderCell: () => ({
      onClick: () => {
        handleSortChange('author');
      },
    }),
  },
  // {
  //   title: '视频类型',
  //   dataIndex: 'viedoCate',
  //   width: 200,
  //   align: 'center',
  // },

  {
    title: '视频标题',
    key: 'videoTitle',
    dataIndex: 'videoTitle',
    align: 'left',
    // 不设置固定宽度：自动占用其余空间
    ellipsis: true,
  },
  {
    title: '操作',
    key: 'operation',
    align: 'center',
    width: 108,
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
  sortField: 'syncTime', // 默认与 sortParams 保持一致
  sortOrder: 'desc', // 📌 默认降序
  cookieId: '',
});

// 分页配置
const pagination = ref({
  current: 1,
  defaultPageSize: 10,
  total: 0,
  showSizeChanger: true, // 强制显示「每页显示数量」下拉框（关键修复）
  showTotal: (total: number) => `共 ${total} 条`,
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
/** 发布时间仅显示日期：YYYY-MM-DD */
const formatPublishDate = (value?: string) => {
  if (!value) return '-';

  const parsed = dayjs(value);
  if (parsed.isValid()) {
    return parsed.format('YYYY-MM-DD');
  }

  // 兼容后端返回非标准日期字符串
  const rawValue = String(value).trim();
  return rawValue.length >= 10 ? rawValue.slice(0, 10) : rawValue;
};

/** 格式化表格视频标题：超过20字符显示省略号 */
const formatVideoTitle = (title?: string) => {
  if (!title) return '无标题';
  return title.length > 60 ? `${title.slice(0, 60)}...` : title;
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
  } else {
    delete quaryData.dates;
  }

  if (value2.value) {
    quaryData.dates2 = value2.value.map((date) => date.format('YYYY-MM-DD'));
  } else {
    delete quaryData.dates2;
  }

  // 将 Ant Design Vue 的排序值转换为后端参数。
  quaryData.sortField = sortParams.value.field;
  quaryData.sortOrder = sortParams.value.order === 'ascend' ? 'asc' : 'desc';

  useApiStore()
    .VideoPageList({ ...quaryData })
    .then((res) => {
      if (res.code !== 0) {
        message.warning(res.message || '获取数据失败');
        return;
      }

      dataSource.value = Array.isArray(res.data?.data) ? res.data.data : [];
      pagination.value.current = Number(res.data?.pageIndex) || 1;
      pagination.value.defaultPageSize = Number(res.data?.pageSize) || 10;
      pagination.value.total = Number(res.data?.total) || 0;
      pagination.value.showTotal = (total: number) => `共 ${total} 条`;
    })
    .catch((error) => {
      console.error('获取表格数据失败:', error);
      message.error('获取数据失败，请稍后重试');
    })
    .finally(() => {
      loading.value = false;
    });
};

// 监听工作台传入的博主；所有依赖变量均已初始化，避免 TDZ 错误。
watch(
  () => paramStore.workplaceAuthor,
  (newVal, oldVal) => {
    const author = String(newVal ?? '').trim();
    const previousAuthor = String(oldVal ?? '').trim();

    if (author === previousAuthor) {
      return;
    }

    quaryData.author = author;
    pagination.value.current = 1;
    GetRecords();
  }
);

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

interface CookieOption {
  value: string;
  label: string;
}

const cookies = ref<CookieOption[]>([]);

const getCookies = () => {
  useApiStore()
    .CookiePageList({})
    .then((res) => {
      const source = Array.isArray(res.data?.data) ? res.data.data : [];

      cookies.value = [
        { value: '', label: '全部' },
        ...source.map((item: Record<string, unknown>) => ({
          value: String(item.id ?? ''),
          label: String(item.userName ?? ''),
        })),
      ];

      // 保留当前有效账号；无效时回退到“全部”。
      if (!cookies.value.some((item) => item.value === quaryData.cookieId)) {
        quaryData.cookieId = '';
      }

      GetRecords();
    })
    .catch((error) => {
      console.error('获取账号列表失败:', error);
      cookies.value = [{ value: '', label: '全部' }];
      quaryData.cookieId = '';
      message.error('获取账号列表失败，已按全部账号查询');
      GetRecords();
    });
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
const datePicked = (_: unknown, dateArry: RangeValue | null) => {
  quaryData.dates = dateArry?.map((date) => date.format('YYYY-MM-DD'));
  console.log('选择的同步日期范围:', quaryData.dates);
};

/** 发布日期选择器变化事件 */
const datePicked2 = (_: unknown, dateArry: RangeValue | null) => {
  quaryData.dates2 = dateArry?.map((date) => date.format('YYYY-MM-DD'));
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

const deleteVideos = ref<any[]>([]);

const deleteVideoPagination = reactive({
  current: 1,
  pageSize: 10,
});

const pagedDeleteVideos = computed(() => {
  const start = (deleteVideoPagination.current - 1) * deleteVideoPagination.pageSize;
  const end = start + deleteVideoPagination.pageSize;

  return deleteVideos.value.slice(start, end);
});

const handShowDeleteVideos = () => {
  deleteVideoPagination.current = 1;
  deleteVideoShow.value = true;
  getDeleteViedos();
};

const getDeleteViedos = () => {
  useApiStore()
    .GetDeleteViedos()
    .then((res) => {
      deleteVideos.value = Array.isArray(res.data) ? res.data : [];

      const maxPage = Math.max(1, Math.ceil(deleteVideos.value.length / deleteVideoPagination.pageSize));

      if (deleteVideoPagination.current > maxPage) {
        deleteVideoPagination.current = maxPage;
      }
    })
    .catch((error) => {
      deleteVideos.value = [];
      deleteVideoPagination.current = 1;
      console.error('获取已删除视频失败：', error);
      message.error('获取已删除视频失败，请稍后重试');
    });
};

const onDeleteVideoClose = () => {
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

// 视频永久删除，不再下载
const handleDelete = (record: DataItem) => {
  Modal.confirm({
    title: '确认永久删除',
    content: `您确定要永久删除这条视频数据吗？此操作不可撤销，以后也不会再下载。`,
    okText: '永久删除',
    cancelText: '取消',
    okType: 'danger',
    onOk: async () => {
      try {
        useApiStore()
          .DeleteVideo(record.id)
          .then((res) => {
            if (res.code == 0) {
              message.success('永久删除成功，以后不会再下载。');
            } else {
              message.error('永久删除失败');
            }
            GetRecords();
          });
      } catch (error) {
        console.error('永久删除失败', error);
        message.error('视频永久删除失败，请稍后再试');
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
  const author = String(paramStore.workplaceAuthor ?? '').trim();
  if (author) {
    quaryData.author = author;
  }

  // getCookies 成功或失败后统一执行一次 GetRecords。
  getCookies();
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

html.dark-mode .ant-table-column-sort {
  background: #161627;
}

/* ===== 表格发布时间与操作列优化 ===== */
.publish-date-text {
  color: #4e5969;
  font-variant-numeric: tabular-nums;
  white-space: nowrap;
}

.operation-actions {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  white-space: nowrap;
}

.operation-icon-btn {
  width: 30px !important;
  min-width: 30px !important;
  height: 30px !important;
  padding: 0 !important;
  display: inline-flex !important;
  align-items: center !important;
  justify-content: center !important;
  border-radius: 8px !important;
  transition: color 0.2s ease, background-color 0.2s ease, transform 0.2s ease;
}

.operation-icon-btn:not(:disabled):hover {
  transform: translateY(-1px);
}

.operation-sync-btn {
  color: #7c3aed !important;
}

.operation-sync-btn:not(:disabled):hover {
  color: #6d28d9 !important;
  background: rgba(124, 58, 237, 0.1) !important;
}

.operation-share-btn {
  color: #1677ff !important;
}

.operation-share-btn:not(:disabled):hover {
  color: #0958d9 !important;
  background: rgba(22, 119, 255, 0.1) !important;
}

.operation-delete-btn {
  color: #ff4d4f !important;
}

.operation-delete-btn:not(:disabled):hover {
  color: #cf1322 !important;
  background: rgba(255, 77, 79, 0.1) !important;
}

/* 暗色主题 */
html.dark-mode .publish-date-text {
  color: #c4c6d0;
}

html.dark-mode .operation-sync-btn {
  color: #a970ff !important;
}

html.dark-mode .operation-share-btn {
  color: #5ab0ff !important;
}

html.dark-mode .operation-delete-btn {
  color: #ff7875 !important;
}

html.dark-mode .operation-sync-btn:not(:disabled):hover {
  background: rgba(169, 112, 255, 0.14) !important;
}

html.dark-mode .operation-share-btn:not(:disabled):hover {
  background: rgba(90, 176, 255, 0.14) !important;
}

html.dark-mode .operation-delete-btn:not(:disabled):hover {
  background: rgba(255, 120, 117, 0.14) !important;
}

/* ===== 响应式表格列宽 =====
 * 小列使用 columns 中的固定宽度，视频标题列不指定宽度，自动占满剩余空间。
 * 不再设置固定总宽度，避免浏览器缩放、侧栏宽度或生产布局变化导致操作列溢出。
 */
.record-table,
.record-table .ant-spin-nested-loading,
.record-table .ant-spin-container,
.record-table .ant-table,
.record-table .ant-table-container,
.record-table .ant-table-content {
  width: 100%;
  max-width: 100%;
  min-width: 0;
  box-sizing: border-box;
}

.record-table .ant-table-container table {
  width: 100% !important;
  min-width: 0 !important;
  table-layout: fixed !important;
}

/* 桌面端不产生横向滚动，操作列始终处于容器右侧。 */
.record-table .ant-table-content {
  overflow-x: hidden !important;
}

.record-table .ant-table-cell {
  min-width: 0;
  box-sizing: border-box;
  overflow: hidden;
}

.record-table .video-title-link {
  display: block;
  width: 100%;
  min-width: 0;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

/* 操作按钮总宽约 92px，108px 列宽可完整容纳三个按钮。 */
.record-table .operation-actions {
  width: 100%;
  max-width: 100%;
}

/* 小屏幕允许滚动，避免固定信息列被压得无法阅读。 */
@media (max-width: 900px) {
  .record-table .ant-table-content {
    overflow-x: auto !important;
  }

  .record-table .ant-table-container table {
    min-width: 760px !important;
  }
}

/* ===== 表格紧凑行高（当前 style 不是 scoped，不使用 :deep） ===== */

/*
 * 当前文件使用的是 <style>，不是 <style scoped>。
 * 因此直接定位 Ant Design Table 生成的真实 DOM：
 * .ant-table-thead、.ant-table-tbody、.ant-table-cell。
 */
.record-table .ant-table-thead > tr > th,
.record-table .ant-table-tbody > tr > td {
  padding: 10px 15px !important;
  line-height: 1.35 !important;
  height: auto !important;
}

/* 第一列排序单元格也使用相同行高 */
.record-table .ant-table-tbody > tr > td.ant-table-column-sort {
  padding: 10px 15px !important;
}

/* 操作列左右间距固定为 5px */
.record-table .ant-table-thead > tr > th:last-child,
.record-table .ant-table-tbody > tr > td:last-child {
  padding-left: 5px !important;
  padding-right: 5px !important;
}

/* 操作按钮为 28px，正文行实际高度约为 38px */
.record-table .operation-icon-btn {
  width: 28px !important;
  min-width: 28px !important;
  height: 28px !important;
  padding: 0 !important;
}

/* 避免操作按钮内部行高把表格行撑高 */
.record-table .operation-icon-btn .anticon {
  line-height: 1 !important;
}

/* 日期和普通文本保持单行 */
.record-table .publish-date-text,
.record-table .operation-actions {
  line-height: 1.35 !important;
}

/* ===== 查询条件区域：整齐紧凑版 ===== */
.query-container {
  margin: 12px 0 14px;
  padding: 14px;
  border: 1px solid #e8edf3;
  border-radius: 12px;
  background: #ffffff;
  box-shadow: 0 4px 16px rgba(31, 45, 61, 0.045);
}

.query-form {
  width: 100%;
}

/* 第一行：四个筛选条件严格对齐 */
.query-fields-grid {
  display: grid;
  grid-template-columns:
    minmax(260px, 1.25fr)
    minmax(260px, 1.25fr)
    minmax(180px, 0.8fr)
    minmax(180px, 0.8fr);
  gap: 12px;
  padding-bottom: 12px;
  border-bottom: 1px solid #edf1f5;
}

.query-container .query-field,
.query-container .query-account-field,
.query-container .query-type-field {
  min-width: 0;
  margin: 0 !important;
}

.query-container .ant-form-item-label {
  padding: 0 0 5px !important;
  line-height: 1 !important;
}

.query-container .ant-form-item-label > label {
  height: auto !important;
  color: #5d6875;
  font-size: 12px;
  line-height: 1.2;
  font-weight: 600;
}

.query-container .ant-form-item-control,
.query-container .ant-form-item-control-input,
.query-container .ant-form-item-control-input-content {
  min-width: 0;
  width: 100%;
}

.query-control,
.account-select {
  width: 100% !important;
}

.query-container .ant-picker,
.query-container .ant-input,
.query-container .ant-input-affix-wrapper,
.query-container .ant-select-selector {
  height: 34px !important;
  border-color: #dfe5ec !important;
  border-radius: 8px !important;
  background: #fafbfc !important;
  box-shadow: none !important;
  transition: border-color 0.2s ease, background-color 0.2s ease, box-shadow 0.2s ease;
}

.query-container .ant-picker {
  padding: 4px 10px !important;
}

.query-container .ant-input,
.query-container .ant-input-affix-wrapper {
  font-size: 12px;
}

.query-container .ant-input-affix-wrapper .ant-input {
  height: auto !important;
  border: 0 !important;
  background: transparent !important;
}

.query-container .ant-select-selector {
  display: flex;
  align-items: center;
  padding: 0 10px !important;
}

.query-container .ant-select-selection-item,
.query-container .ant-select-selection-placeholder {
  line-height: 32px !important;
  font-size: 12px;
}

.query-container .ant-picker:hover,
.query-container .ant-input:hover,
.query-container .ant-input-affix-wrapper:hover,
.query-container .ant-select:hover .ant-select-selector {
  border-color: #aab7c6 !important;
  background: #ffffff !important;
}

.query-container .ant-picker-focused,
.query-container .ant-input:focus,
.query-container .ant-input-affix-wrapper-focused,
.query-container .ant-select-focused .ant-select-selector {
  border-color: #7c3aed !important;
  background: #ffffff !important;
  box-shadow: 0 0 0 2px rgba(124, 58, 237, 0.09) !important;
}

/* 第二行 */
.query-toolbar {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 14px;
  padding-top: 12px;
}

.query-toolbar-main {
  min-width: 0;
  flex: 1;
  display: flex;
  align-items: flex-end;
  gap: 12px;
}

.query-account-field {
  flex: 0 0 130px;
}

.query-type-field {
  min-width: 0;
  flex: 1;
}

.video-type-radio {
  display: inline-flex;
  max-width: 100%;
  flex-wrap: wrap;
  gap: 0;
}

.video-type-radio .ant-radio-button-wrapper {
  height: 34px;
  padding: 0 13px;
  color: #5d6875;
  border-color: #dfe5ec;
  background: #ffffff;
  font-size: 12px;
  line-height: 32px;
}

.video-type-radio .ant-radio-button-wrapper:first-child {
  border-radius: 8px 0 0 8px;
}

.video-type-radio .ant-radio-button-wrapper:last-child {
  border-radius: 0 8px 8px 0;
}

.video-type-radio .ant-radio-button-wrapper:hover {
  color: #7c3aed;
}

.video-type-radio .ant-radio-button-wrapper-checked:not(.ant-radio-button-wrapper-disabled) {
  color: #ffffff;
  border-color: #7c3aed;
  background: #7c3aed;
  box-shadow: -1px 0 0 0 #7c3aed;
}

.video-type-radio .ant-radio-button-wrapper-checked:not(.ant-radio-button-wrapper-disabled)::before {
  background-color: #7c3aed !important;
}

/* 右侧操作区固定整齐排列 */
.query-toolbar-actions {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 8px;
  white-space: nowrap;
}

.query-button,
.batch-sync-button,
.batch-delete-button,
.deleted-records-button {
  height: 34px !important;
  padding: 0 13px !important;
  border-radius: 8px !important;
  font-size: 12px;
  box-shadow: none !important;
}

.query-button {
  min-width: 82px;
  border-color: #7c3aed !important;
  background: #7c3aed !important;
}

.query-button:hover {
  border-color: #6d28d9 !important;
  background: #6d28d9 !important;
}

.batch-mode-control {
  height: 34px;
  padding: 0 9px;
  display: inline-flex;
  align-items: center;
  gap: 7px;
  border: 1px solid #e2e7ed;
  border-radius: 8px;
  color: #687382;
  background: #f8fafb;
}

.batch-mode-label {
  font-size: 11px;
}

.batch-action-group {
  display: inline-flex;
  align-items: center;
  gap: 7px;
}

.batch-sync-button {
  color: #6d28d9;
  border-color: rgba(124, 58, 237, 0.28);
  background: rgba(124, 58, 237, 0.06);
}

.batch-sync-button:not(:disabled):hover {
  color: #ffffff;
  border-color: #7c3aed;
  background: #7c3aed;
}

.batch-delete-button,
.deleted-records-button {
  color: #ef4444 !important;
  border-color: rgba(239, 68, 68, 0.3) !important;
  background: rgba(239, 68, 68, 0.045) !important;
}

.batch-delete-button:not(:disabled):hover,
.deleted-records-button:hover {
  color: #ffffff !important;
  border-color: #ef4444 !important;
  background: #ef4444 !important;
}

/* 暗色主题 */
html.dark-mode .query-container {
  border-color: #303247;
  background: #19192d;
  box-shadow: none;
}

html.dark-mode .query-fields-grid {
  border-bottom-color: #303247;
}

html.dark-mode .query-container .ant-form-item-label > label {
  color: #b9bbc8;
}

html.dark-mode .query-container .ant-picker,
html.dark-mode .query-container .ant-input,
html.dark-mode .query-container .ant-input-affix-wrapper,
html.dark-mode .query-container .ant-select-selector {
  color: #e7e8ef !important;
  border-color: #34364c !important;
  background: #202037 !important;
}

html.dark-mode .query-container .ant-input-affix-wrapper .ant-input {
  background: transparent !important;
}

html.dark-mode .query-container .ant-picker-input > input,
html.dark-mode .query-container .ant-input,
html.dark-mode .query-container .ant-select-selection-item {
  color: #e7e8ef !important;
}

html.dark-mode .query-container .ant-picker-input > input::placeholder,
html.dark-mode .query-container .ant-input::placeholder,
html.dark-mode .query-container .ant-select-selection-placeholder {
  color: #74768a !important;
}

html.dark-mode .video-type-radio .ant-radio-button-wrapper {
  color: #b9bbc8;
  border-color: #34364c;
  background: #202037;
}

html.dark-mode .video-type-radio .ant-radio-button-wrapper-checked:not(.ant-radio-button-wrapper-disabled) {
  color: #ffffff;
  border-color: #7c3aed;
  background: #7c3aed;
}

html.dark-mode .batch-mode-control {
  color: #b9bbc8;
  border-color: #34364c;
  background: #202037;
}

/* 中等宽度：基础条件变成两列，操作区自动换行 */
@media (max-width: 1380px) {
  .query-fields-grid {
    grid-template-columns: repeat(2, minmax(240px, 1fr));
  }

  .query-toolbar {
    align-items: stretch;
    flex-direction: column;
  }

  .query-toolbar-actions {
    justify-content: flex-start;
    flex-wrap: wrap;
  }
}

/* 平板 */
@media (max-width: 900px) {
  .query-container {
    padding: 12px;
  }

  .query-toolbar-main {
    align-items: stretch;
    flex-direction: column;
  }

  .query-account-field {
    flex-basis: auto;
    width: 180px;
  }
}

/* 移动端 */
@media (max-width: 680px) {
  .query-fields-grid {
    grid-template-columns: 1fr;
    gap: 9px;
  }

  .query-account-field {
    width: 100%;
  }

  .video-type-radio {
    width: 100%;
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: 5px;
  }

  .video-type-radio .ant-radio-button-wrapper {
    padding: 0 5px;
    text-align: center;
    border: 1px solid #dfe5ec !important;
    border-radius: 7px !important;
  }

  .query-toolbar-actions {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    width: 100%;
  }

  .query-button,
  .batch-sync-button,
  .batch-delete-button,
  .deleted-records-button,
  .batch-mode-control {
    width: 100%;
    justify-content: center;
  }

  .batch-action-group {
    display: contents;
  }
}

/* ===== 查询条件对齐与已删除前端分页 ===== */

/*
 * 当前文件使用普通 <style>，直接覆盖 Ant Design 表单结构。
 * 每个查询项统一为：固定宽度标签 + 自适应控件。
 */
.query-fields-grid {
  align-items: center;
}

.query-container .query-field {
  display: grid !important;
  grid-template-columns: 62px minmax(0, 1fr);
  align-items: center;
  column-gap: 8px;
}

.query-container .query-field .ant-form-item-label {
  width: 62px;
  padding: 0 !important;
  overflow: visible;
  text-align: right;
  white-space: nowrap;
}

.query-container .query-field .ant-form-item-label > label {
  width: 100%;
  height: 34px !important;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  line-height: 34px !important;
}

.query-container .query-field .ant-form-item-control {
  min-width: 0;
}

.query-container .query-field .ant-form-item-control-input {
  min-height: 34px !important;
}

.query-container .query-field .ant-form-item-control-input-content {
  height: 34px;
  display: flex;
  align-items: center;
}

/* 第二行账号、视频类型采用相同的对齐方式 */
.query-container .query-account-field,
.query-container .query-type-field {
  display: grid !important;
  grid-template-columns: 62px minmax(0, 1fr);
  align-items: center;
  column-gap: 8px;
}

.query-container .query-account-field .ant-form-item-label,
.query-container .query-type-field .ant-form-item-label {
  width: 62px;
  padding: 0 !important;
  text-align: right;
  white-space: nowrap;
}

.query-container .query-account-field .ant-form-item-label > label,
.query-container .query-type-field .ant-form-item-label > label {
  width: 100%;
  height: 34px !important;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  line-height: 34px !important;
}

.query-container .query-account-field .ant-form-item-control-input,
.query-container .query-type-field .ant-form-item-control-input {
  min-height: 34px !important;
}

.query-container .query-account-field .ant-form-item-control-input-content,
.query-container .query-type-field .ant-form-item-control-input-content {
  min-width: 0;
  min-height: 34px;
  display: flex;
  align-items: center;
}

/* 第一行控件自身保持同一高度和垂直位置 */
.query-container .query-field .ant-picker,
.query-container .query-field .ant-input,
.query-container .query-field .ant-input-affix-wrapper {
  margin: 0 !important;
  vertical-align: middle;
}

/* 第二行取消原先底部对齐造成的视觉错位 */
.query-toolbar,
.query-toolbar-main {
  align-items: center;
}

.query-account-field {
  flex-basis: 200px;
}

/* 已删除视频抽屉 */
.deleted-video-total {
  color: #7a8592;
  font-size: 12px;
  font-variant-numeric: tabular-nums;
}

.deleted-video-list-wrapper {
  min-height: 100%;
  display: flex;
  flex-direction: column;
}

.deleted-video-list {
  flex: 1;
}

.deleted-video-list .ant-list-item {
  min-height: 46px;
  padding: 9px 12px !important;
}

.deleted-video-list .delete-video-title-container {
  margin-right: 0;
}

.deleted-video-list .delete-video-index {
  min-width: 38px;
  color: #8a949f;
  text-align: right;
  font-variant-numeric: tabular-nums;
}

.deleted-video-list .delete-video-title {
  color: #3f4a56;
  font-size: 13px;
}

.deleted-video-pagination {
  position: sticky;
  bottom: 0;
  z-index: 2;
  margin-top: 12px;
  padding: 12px 0 2px;
  display: flex;
  justify-content: flex-end;
  border-top: 1px solid #edf0f3;
  background: #ffffff;
}

.deleted-video-empty {
  min-height: 220px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #9aa3ad;
  font-size: 13px;
}

/* 暗色主题 */
html.dark-mode .deleted-video-total {
  color: #a3a5b2;
}

html.dark-mode .deleted-video-list .delete-video-index {
  color: #818393;
}

html.dark-mode .deleted-video-list .delete-video-title {
  color: #d7d8df;
}

html.dark-mode .deleted-video-pagination {
  border-top-color: #303247;
  background: #19192d;
}

html.dark-mode .deleted-video-empty {
  color: #858797;
}

/* 中等宽度下依旧保持标签与控件同一行 */
@media (max-width: 1380px) {
  .query-container .query-field,
  .query-container .query-account-field,
  .query-container .query-type-field {
    grid-template-columns: 62px minmax(0, 1fr);
  }
}

/* 移动端允许标签宽度略缩小 */
@media (max-width: 680px) {
  .query-container .query-field,
  .query-container .query-account-field,
  .query-container .query-type-field {
    grid-template-columns: 56px minmax(0, 1fr);
    column-gap: 6px;
  }

  .query-container .query-field .ant-form-item-label,
  .query-container .query-account-field .ant-form-item-label,
  .query-container .query-type-field .ant-form-item-label {
    width: 56px;
  }

  .deleted-video-pagination {
    justify-content: center;
    overflow-x: auto;
  }
}
</style>