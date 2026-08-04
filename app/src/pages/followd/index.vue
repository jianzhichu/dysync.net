<template>
  <div class="dept-user-card-container">
    <!-- 搜索框 + Tab导航 同一行布局 -->
    <div class="search-tab-container">
      <div class="tab-wrapper">
        <a-tabs v-model:value="activeTabKey" type="line" class="custom-tabs" @change="handleTabChange">
          <a-tab-pane v-for="tab in tabList" :key="tab.key" :tab="`${tab.name}(${tab.total || 0})`" />
        </a-tabs>
      </div>
      <div class="search-area">

        未开启<a-switch v-model:checked="quaryData.unOpen" @change="onSyncFilterChange" size="small" />
        同步<a-switch v-model:checked="quaryData.openSync" @change="onSyncFilterChange" size="small" />
        全同步<a-switch v-model:checked="quaryData.fullSync" @change="onSyncFilterChange" size="small" />
        <!-- 搜索按钮 -->
        <a-button type="default" class="search-btn" @click="toggleSearchInput">
          <CloseOutlined v-if="searchInputVisible" />
          <SearchOutlined v-else />
        </a-button>
        <transition name="search-input-fade">
          <div v-if="searchInputVisible" class="search-input-wrapper">
            <a-input v-model:value="quaryData.followUserName" placeholder="输入博主用户名或抖音号，按回车" allow-clear @pressEnter="handleSearch" class="search-input" ref="searchInputRef" />
          </div>
        </transition>
        <a-button type="primary" class="sync-btn" @click="handleAdd" :disabled="isAddDisabled">
          <PlusOutlined />
          <span class="sync-btn-text">新增</span>
        </a-button>
        <!-- 同步按钮（请求期间禁用） -->
        <a-button type="danger" class="sync-btn" @click="handleSyncAll" :disabled="isSyncDisabled">
          <SyncOutlined />
          <span class="sync-btn-text">立即同步</span>
        </a-button>
      </div>
    </div>
    <!-- 卡片列表区域 -->
    <div ref="cardListRef" class="card-list-container" @scroll.passive="handleScroll">
      <a-card v-for="(item, index) in currentTabData" :key="item.id" :data-key="item.id" class="custom-card" :bordered="true" :hoverable="true" :class="{ 'no-followed-card': item.isNoFollowed }">
        <div class="card-inner">
          <!-- 非关注标记 -->
          <!-- <div v-if="item.isNoFollowed" class="no-followed-tag">
            非关注
          </div> -->

          <div class="card-switch">
            <a-switch v-model:checked="item.openSync" @change="(checked) => handleSwitchChange(item, checked)" checked-children="开" un-checked-children="关" />
          </div>

          <div class="card-main-content">
            <div class="avatar-wrapper" @click="goDouyinUp(item)">
              <a-avatar shape="circle" size="large" :src="item.uperAvatar" v-if="item.uperAvatar" />
              <a-avatar shape="circle" size="large" v-else class="avatar-placeholder">
                {{ item.uperName.charAt(0) }}
              </a-avatar>
            </div>
            <div class="card-content">
              <div class="card-name">
                <!-- 博主姓名增加点击跳转 -->
                <span class="author-name-link" @click="goToRecordPage(item)" :title="'查看该博主的全部记录'">
                  {{ item.uperName }}
                </span>
                <span style="font-size:12px;">{{item.douyinNo?`(${item.douyinNo})`:''}}</span>
                <!-- 非关注小标记 -->
                <span v-if="item.isNoFollowed" class="no-followed-badge">非关注</span>
                <!-- 删除按钮（仅非关注项显示，放在名字+非关注后面）v-if="item.isNoFollowed"  -->
                <a-button type="text" class="delete-btn" @click="(e) => { e.stopPropagation(); handleDeleteItem(item); }" :disabled="item.isSaving" title="删除该非关注博主">
                  <close-outlined />
                </a-button>
              </div>
              <!-- 签名多行显示：高度控制 + 溢出截断 + Tooltip气泡 -->
              <div class="card-desc">
                <a-tooltip placement="top" :title="item.signature || '无签名'">
                  <span class="signature-text">
                    {{ truncateText(item.signature || '无签名', 30) }}
                  </span>
                </a-tooltip>
              </div>
              <div class="card-path-sync-container">
                <template v-if="!item.openSync">
                  <span class="path-placeholder"></span>
                </template>
                <template v-else>
                  <div class="path-area">
                    <template v-if="item.isEditing">
                      <div class="edit-input-group">
                        <!-- 输入框禁用：item.isSaving 为 true 时 -->
                        <a-input v-model:value="item.savePath" placeholder="请输入文件夹名称" @keypress.enter="() => handleSavePath(item)" maxlength="30" :disabled="item.isSaving" />
                        <!-- 保存按钮禁用：item.isSaving 为 true 时 -->
                        <a-button type="text" class="edit-btn" @click="() => handleSavePath(item)" :disabled="item.isSaving">
                          <SaveOutlined />
                        </a-button>
                      </div>
                    </template>
                    <template v-else>
                      <span class="path-text" :class="{ 'path-empty': !item.savePath }">
                        {{ item.savePath || '默认用博主名字' }}
                      </span>
                      <!-- 编辑按钮禁用：item.isSaving 为 true 时 -->
                      <a-button type="text" class="edit-btn" @click="() => handleEditPath(item)" title="编辑文件夹名称" :disabled="item.isSaving">
                        <EditOutlined />
                      </a-button>
                    </template>
                  </div>
                  <div class="sync-switch-wrapper">
                    <span class="sync-label">全量同步</span>
                    <a-switch v-model:checked="item.fullSync" size="small" @change="(checked) => handleSyncChange(item, checked)" />
                  </div>
                </template>
              </div>
            </div>
          </div>
        </div>
      </a-card>

      <!--
        无限滚动哨兵：
        IntersectionObserver 同时兼容“容器内部滚动”和“页面滚动”。
        scroll 事件仍保留，作为旧浏览器和特殊布局的兜底。
      -->
      <div v-show="hasMore && !noMoreData" ref="loadMoreSentinelRef" class="load-more-sentinel" aria-hidden="true"></div>

      <!-- 加载状态 -->
      <div v-if="loading" class="loading-container">
        <a-spin size="middle" />
        <span class="loading-text">加载中...</span>
      </div>
      <!-- 无更多数据 -->
      <div v-if="noMoreData && followData.length > 0" class="no-more-container">暂无更多数据</div>
      <!-- 空状态 -->
      <div v-if="followData.length === 0 && !loading" class="empty-container">
        <Empty description="暂无关注用户" />
      </div>
    </div>

    <a-modal v-model:visible="addModalVisible" title="新增非关注博主" :width="600" :confirm-loading="addFormLoading" @ok="handleAddSubmit" @cancel="handleAddCancel">
      <a-form :model="addForm" :rules="addFormRules" ref="addFormRef" layout="horizontal" class="add-form" :label-col="{ span: 6 }" :wrapper-col="{ span: 17 }">
        <a-form-item name="uperName" label="博主姓名" :validate-status="addFormErrors.uperName ? 'error' : ''" :help="addFormErrors.uperName || ''">
          <a-input v-model:value="addForm.uperName" placeholder="请输入博主姓名" maxlength="20" @input="clearFormError('uperName')" />
        </a-form-item>

        <a-form-item name="uperId" label="博主Uid" :validate-status="addFormErrors.uperId ? 'error' : ''" :help="addFormErrors.uperId || ''" class="uper-id-form-item">
          <a-input v-model:value="addForm.uperId" placeholder="请输入博主Uid" maxlength="50" @input="clearFormError('uperId')" />
        </a-form-item>

        <a-form-item name="secUid" label="博主SecUid" :validate-status="addFormErrors.secUid ? 'error' : ''" :help="addFormErrors.secUid || ''">
          <a-input v-model:value="addForm.secUid" placeholder="请输入博主secUid" />
        </a-form-item>

        <a-form-item name="savePath" label="保存文件夹" :validate-status="addFormErrors.savePath ? 'error' : ''" :help="addFormErrors.savePath || ''">
          <a-input v-model:value="addForm.savePath" placeholder="不填默认使用博主姓名" maxlength="20" @input="clearFormError('savePath')" />
        </a-form-item>

        <!-- 是否同步 - 独立表单项 -->
        <a-form-item label="是否同步">
          <a-switch v-model:checked="addForm.openSync" checked-children="是" un-checked-children="否" />
        </a-form-item>

        <!-- 是否全量同步 - 独立表单项 -->
        <a-form-item label="全量同步">
          <a-switch v-model:checked="addForm.fullSync" checked-children="是" un-checked-children="否" :disabled="!addForm.openSync" />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted, onUnmounted, UnwrapRef, reactive, nextTick, Ref } from 'vue';
import { message, Spin, Empty, Tooltip, Modal, Form, FormInstance, Popconfirm } from 'ant-design-vue';
import { useApiStore } from '@/store';
// 顶部新增导入
import { useRouter } from 'vue-router';
import { useRouteParamStore } from '@/store/routeParam';
import {
  CloseOutlined,
  SearchOutlined,
  PlusOutlined,
  SyncOutlined,
  SaveOutlined,
  EditOutlined,
  DeleteOutlined,
} from '@ant-design/icons-vue';

// 类型定义
interface TabItem {
  key: string;
  name: string;
  total?: number;
}

interface FollowItem {
  id: string;
  mySelfId: string;
  uperName: string;
  enterprise: string;
  signature: string;
  uperAvatar: string;
  fullSync: boolean;
  openSync: boolean;
  savePath?: string;
  isEditing: boolean;
  isSaving?: boolean;
  uperId?: string; // 原userId改为uperId
  douyinNo?: string;
  isNoFollowed: boolean; // 新增：是否为非关注博主
  secUid?: string;
}

interface QuaryParam {
  pageIndex: number;
  pageSize: number;
  followUserName: string | null;
  mySelfId?: string;
  openSync: boolean;
  fullSync: boolean;
  unOpen: boolean;
}

interface AddForm {
  uperName: string;
  secUid: string;
  savePath: string;
  openSync: boolean;
  fullSync: boolean;
  mySelfId: string;
  uperId: string;
  douyinNo: string;
}

// Tab列表数据
const tabList = ref<TabItem[]>([]);
const activeTabKey = ref('');

// 关注用户列表数据
const followData = ref<FollowItem[]>([]);

// 状态变量
const loading = ref(false);
const noMoreData = ref(false);
const hasMore = ref(true);
const searchInputVisible = ref(false);
const searchInputRef = ref<HTMLInputElement | null>(null);
const isSyncDisabled = ref(false);
const isAddDisabled = ref(false);

// 无限滚动相关 DOM 与观察器
const cardListRef = ref<HTMLDivElement | null>(null);
const loadMoreSentinelRef = ref<HTMLDivElement | null>(null);
let loadMoreObserver: IntersectionObserver | null = null;
let deferredLoadTimer: ReturnType<typeof setTimeout> | null = null;

// 新增表单相关
const addModalVisible = ref(false);
const addFormLoading = ref(false);
const addFormRef = ref<FormInstance | null>(null);

// 新增表单数据
const addForm = ref<AddForm>({
  uperName: '',
  secUid: '',
  savePath: '',
  openSync: false,
  fullSync: false,
  mySelfId: '',
  uperId: '',
  douyinNo: '',
});

// 表单校验规则
const addFormRules = ref({
  uperName: [
    { required: true, message: '请输入博主姓名', trigger: 'blur' },
    { max: 20, message: '姓名长度不能超过20个字符', trigger: 'blur' },
  ],
  uperId: [{ required: true, message: '请输入博主Uid', trigger: 'blur' }],
  secUid: [{ required: true, message: '请输入博主secUid', trigger: 'blur' }],
  savePath: [{ max: 20, message: '文件夹名称长度不能超过20个字符', trigger: 'blur' }],
});

const router = useRouter();
const paramStore = useRouteParamStore();
// 新增：跳转记录页并携带博主名称
const goToRecordPage = (item: FollowItem) => {
  // 1. 先把参数存入 Store
  paramStore.setWorkplaceAuthor(item.uperName);
  // 2. 纯路径跳转，不带 query，标签不会重复
  router.push('/workplace');
};
// 表单错误信息
const addFormErrors = ref({
  uperName: '',
  uperId: '',
  savePath: '',
  secUid: '',
});

// 搜索参数
const quaryData: UnwrapRef<QuaryParam> = reactive({
  pageIndex: 0,
  pageSize: 20,
  followUserName: null,
  mySelfId: '',
  openSync: false,
  fullSync: false,
  unOpen: false,
});

// 生命周期 - 挂载时初始化
onMounted(async () => {
  quaryData.pageIndex = 0;

  // 等待列表 DOM 挂载后启用底部哨兵。
  await nextTick();
  setupLoadMoreObserver();

  initData();
});

const onSyncFilterChange = () => {
  resetPagingState();
  initData();
};

// 生命周期 - 卸载时释放观察器和延迟任务
onUnmounted(() => {
  loadMoreObserver?.disconnect();
  loadMoreObserver = null;

  if (deferredLoadTimer) {
    clearTimeout(deferredLoadTimer);
    deferredLoadTimer = null;
  }
});

// 重置分页状态。所有筛选、搜索和 Tab 切换都必须从第一页重新开始。
const resetPagingState = () => {
  quaryData.pageIndex = 0;
  noMoreData.value = false;
  hasMore.value = true;
};

// 初始化数据（统一入口，避免循环）
const initData = () => {
  resetPagingState();

  GetCookies().then(() => {
    // Cookie 获取成功后，直接加载当前 Tab 数据。
    if (activeTabKey.value) {
      quaryData.mySelfId = activeTabKey.value;
      GetFollows(true);
    } else {
      followData.value = [];
      noMoreData.value = true;
      hasMore.value = false;
    }
  });
};

// 获取Cookie列表（Tab数据）- 移除循环调用
const GetCookies = (): Promise<void> => {
  return new Promise((resolve) => {
    useApiStore()
      .CookieList()
      .then((res) => {
        if (res.code === 0) {
          tabList.value = res.data;
          // 当前账号不存在时回退到第一个 Tab，避免列表被旧 key 过滤为空。
          const currentTabExists = tabList.value.some((tab) => tab.key === activeTabKey.value);
          if (tabList.value.length > 0 && !currentTabExists) {
            activeTabKey.value = tabList.value[0].key;
          }
        }
        resolve();
      })
      .catch((err) => {
        console.error('获取Tab数据失败：', err);
        message.error('获取Tab数据失败，请刷新重试');
        resolve();
      });
  });
};

// 当前哨兵接近可视区域时继续加载。
// 首屏数据不足以形成滚动条时，也会自动补取下一页。
const scheduleLoadMoreIfNeeded = () => {
  if (deferredLoadTimer) {
    clearTimeout(deferredLoadTimer);
  }

  deferredLoadTimer = setTimeout(() => {
    deferredLoadTimer = null;

    if (loading.value || !hasMore.value || noMoreData.value) {
      return;
    }

    const sentinel = loadMoreSentinelRef.value;
    if (!sentinel) {
      return;
    }

    const rect = sentinel.getBoundingClientRect();
    const preloadDistance = 180;

    if (rect.top <= window.innerHeight + preloadDistance) {
      GetFollows(false);
    }
  }, 60);
};

// 获取关注用户列表
const GetFollows = (isReset = false) => {
  if (loading.value) {
    return;
  }

  if (!isReset && (!hasMore.value || noMoreData.value)) {
    return;
  }

  if (!activeTabKey.value) {
    return;
  }

  if (isReset) {
    resetPagingState();
    quaryData.mySelfId = activeTabKey.value;
  }

  const requestedPage = isReset ? 0 : quaryData.pageIndex;
  const requestParams: QuaryParam = {
    ...quaryData,
    pageIndex: requestedPage,
    mySelfId: activeTabKey.value,
  };

  loading.value = true;

  useApiStore()
    .FollowList(requestParams)
    .then((res) => {
      if (res.code !== 0) {
        throw new Error(res.message || '获取关注用户列表失败');
      }

      const rawData = Array.isArray(res.data?.data) ? res.data.data : [];
      const total = Number(res.data?.total) || 0;

      const formattedData: FollowItem[] = rawData.map((item: FollowItem) => ({
        ...item,
        isSaving: false,
        isEditing: item.isEditing ?? false,
        uperId: item.uperId || item.id,
        isNoFollowed: item.isNoFollowed ?? false,
      }));

      if (isReset) {
        followData.value = formattedData;
      } else {
        const existingKeys = new Set(followData.value.map((item) => item.id));
        const uniqueNewData = formattedData.filter((item) => !existingKeys.has(item.id));
        followData.value = [...followData.value, ...uniqueNewData];
      }

      // 更新当前 Tab 总数。
      const tabIndex = tabList.value.findIndex((tab) => tab.key === activeTabKey.value);
      if (tabIndex !== -1) {
        tabList.value[tabIndex].total = total;
      }

      const loadedForCurrentTab = followData.value.filter((item) => item.mySelfId === activeTabKey.value).length;

      const reachedTotal = total > 0 && loadedForCurrentTab >= total;
      const emptyPage = formattedData.length === 0;
      const shortPage = formattedData.length < requestParams.pageSize;

      noMoreData.value = reachedTotal || emptyPage || shortPage;
      hasMore.value = !noMoreData.value;

      // 只有本页成功返回后才推进页码，避免请求失败造成跳页。
      quaryData.pageIndex = requestedPage + 1;
    })
    .catch((err) => {
      console.error('获取关注用户列表异常：', err);
      message.error(err instanceof Error ? err.message : '网络异常，请重试');

      // 请求失败不直接判定“没有更多”，允许用户继续滚动重试。
      hasMore.value = true;
      noMoreData.value = false;
    })
    .finally(() => {
      loading.value = false;

      nextTick(() => {
        scheduleLoadMoreIfNeeded();
      });
    });
};

// 切换搜索框显示/隐藏
const toggleSearchInput = () => {
  searchInputVisible.value = !searchInputVisible.value;

  if (searchInputVisible.value) {
    nextTick(() => {
      searchInputRef.value?.focus();
    });
  } else {
    // 隐藏时清空搜索条件并重新加载
    if (quaryData.followUserName) {
      quaryData.followUserName = null;
      quaryData.pageIndex = 0;
      GetFollows(true);
    }
  }
};

// 执行搜索
const handleSearch = () => {
  resetPagingState();
  GetFollows(true);
};

// Tab 切换。
// a-tabs 的 v-model 可能先更新 activeTabKey 再触发 change，不能用“值相同”提前 return。
const handleTabChange = (key: string) => {
  activeTabKey.value = key;
  quaryData.mySelfId = key;
  searchInputVisible.value = false;
  quaryData.followUserName = null;

  resetPagingState();
  GetFollows(true);
};

// 计算当前Tab的用户数据
const currentTabData = computed(() => {
  return followData.value.filter((item) => item.mySelfId === activeTabKey.value);
});

// 统一触发下一页加载。
const loadNextPage = () => {
  if (loading.value || !hasMore.value || noMoreData.value) {
    return;
  }

  GetFollows(false);
};

// IntersectionObserver 使用浏览器视口作为 root。
// 它既能感知 card-list-container 内部滚动，也能兼容窄屏下页面本身滚动。
const setupLoadMoreObserver = () => {
  loadMoreObserver?.disconnect();

  if (!('IntersectionObserver' in window)) {
    return;
  }

  const sentinel = loadMoreSentinelRef.value;
  if (!sentinel) {
    return;
  }

  loadMoreObserver = new IntersectionObserver(
    (entries) => {
      if (entries.some((entry) => entry.isIntersecting)) {
        loadNextPage();
      }
    },
    {
      root: null,
      rootMargin: '0px 0px 180px 0px',
      threshold: 0.01,
    }
  );

  loadMoreObserver.observe(sentinel);
};

// 滚动事件作为兜底。直接使用事件目标，不再全局 querySelector。
const handleScroll = (event: Event) => {
  if (loading.value || !hasMore.value || noMoreData.value) {
    return;
  }

  const cardContainer = event.currentTarget as HTMLDivElement | null;
  if (!cardContainer) {
    return;
  }

  const remaining = cardContainer.scrollHeight - cardContainer.scrollTop - cardContainer.clientHeight;

  if (remaining <= 180) {
    loadNextPage();
  }
};

// 开关状态变更（启用/禁用用户）
const handleSwitchChange = (item: FollowItem, checked: boolean) => {
  item.openSync = checked;
  uploadSyncStatus(item);
};

// 全量同步开关变更
const handleSyncChange = (item: FollowItem, checked: boolean) => {
  item.fullSync = checked;
  uploadSyncStatus(item);
};

// 编辑存储路径
const handleEditPath = (item: FollowItem) => {
  if (item.isSaving) return;
  item.isEditing = true;
  // 延迟聚焦输入框
  setTimeout(() => {
    const input = document.querySelector(
      `.dept-user-card-container .custom-card[data-key="${item.id}"] .edit-input-group .ant-input`
    ) as HTMLInputElement | null;
    input?.focus();
  }, 100);
};

// 保存存储路径
const handleSavePath = (item: FollowItem) => {
  if (item.isSaving) return;
  uploadSyncStatus(item);
};

// 更新同步状态
const uploadSyncStatus = (item: FollowItem) => {
  item.isSaving = true;
  useApiStore()
    .OpenOrCloseSync({
      Id: item.id,
      OpenSync: item.openSync,
      FullSync: item.fullSync,
      SavePath: item.savePath,
      uperId: item.uperId, // 原userId改为uperId
    })
    .then((res) => {
      if (res.code === 0) {
        message.success(`保存成功，将在下次任务执行时生效`);
        item.isEditing = false;
      } else {
        message.error('保存失败' + (res.message || '未知错误'));
      }
    })
    .catch((err) => {
      console.error('保存失败', err);
      message.error('保存失败，请重试');
    })
    .finally(() => {
      item.isSaving = false;
    });
};

// 批量同步所有用户
const handleSyncAll = () => {
  if (isSyncDisabled.value) return;

  isSyncDisabled.value = true;
  loading.value = true;

  useApiStore()
    .StartJobNow()
    .then((res) => {
      if (res.code === 0) {
        message.success('后台开始同步...根据您关注的数量，需要的时间不一定...请耐心等待');
      } else {
        message.error('同步失败：' + (res.message || '未知错误'));
      }
    })
    .catch((err) => {
      console.error('同步异常：', err);
      message.error('同步失败，请重试');
    })
    .finally(() => {
      isSyncDisabled.value = false;
      loading.value = false;
    });
};

// 工具函数：文本截断，超过指定长度显示省略号
const truncateText = (text: string, maxLength: number): string => {
  if (!text) return '';
  // 计算字符串长度（中文算1个字符）
  if (text.length <= maxLength) {
    return text;
  }
  return text.slice(0, maxLength) + '...';
};

// ===================== 新增功能相关 =====================
// 打开新增弹窗
const handleAdd = () => {
  // 重置表单
  addForm.value = {
    uperName: '',
    uperId: '',
    savePath: '',
    openSync: false,
    fullSync: false,
    secUid: '',
    mySelfId: activeTabKey.value,
    douyinNo: '',
  };
  // 清空错误信息
  Object.keys(addFormErrors.value).forEach((key) => {
    addFormErrors.value[key as keyof typeof addFormErrors.value] = '';
  });
  // 重置表单校验状态
  addFormRef.value?.resetFields();
  // 显示弹窗
  addModalVisible.value = true;
};

// 关闭新增弹窗
const handleAddCancel = () => {
  addModalVisible.value = false;
  // 重置表单
  addFormRef.value?.resetFields();
  Object.keys(addFormErrors.value).forEach((key) => {
    addFormErrors.value[key as keyof typeof addFormErrors.value] = '';
  });
};

// 清空表单错误
const clearFormError = (field: keyof typeof addFormErrors.value) => {
  addFormErrors.value[field] = '';
};

// 提交新增表单
const handleAddSubmit = () => {
  // 手动校验表单
  addFormRef.value
    ?.validate()
    .then(() => {
      addFormLoading.value = true;

      // 构造提交数据
      const submitData = {
        mySelfId: activeTabKey.value, // 当前选中的Tab的key
        uperId: addForm.value.uperId,
        uperName: addForm.value.uperName,
        savePath: addForm.value.savePath,
        openSync: addForm.value.openSync,
        fullSync: addForm.value.fullSync,
        secUid: addForm.value.secUid,
        signature: '', // 默认为空签名
        uperAvatar: '', // 默认为空头像
        enterprise: '', // 默认为空企业信息
        isNoFollowed: true, // 新增的非关注博主，标记为true
      };

      // 调用新增接口
      useApiStore()
        .AddFollow(submitData)
        .then((res) => {
          if (res.code === 0) {
            message.success('新增非关注博主成功！');
            addModalVisible.value = false;
            // 重新加载数据
            quaryData.pageIndex = 0;
            GetFollows(true);

            // 更新Tab总数
            const tabIndex = tabList.value.findIndex((tab) => tab.key === activeTabKey.value);
            if (tabIndex !== -1) {
              tabList.value[tabIndex].total = (tabList.value[tabIndex].total || 0) + 1;
            }
          } else {
            message.error('新增失败：' + (res.message || '未知错误'));
          }
        })
        .catch((err) => {
          console.error('新增关注博主异常：', err);
          message.error('网络异常，请重试');
        })
        .finally(() => {
          addFormLoading.value = false;
        });
    })
    .catch((errors) => {
      // 处理表单校验错误
      errors.forEach((err: any) => {
        if (err.field && addFormErrors.value.hasOwnProperty(err.field)) {
          addFormErrors.value[err.field as keyof typeof addFormErrors.value] = err.message;
        }
      });
    });
};

// ===================== 删除功能相关 =====================
// 删除非关注博主
const handleDeleteItem = (item: FollowItem) => {
  Modal.confirm({
    title: '确认删除',
    content: `确定要删除非关注博主「${item.uperName}」吗？删除后将无法恢复。`,
    okText: '确认删除',
    cancelText: '取消',
    okType: 'danger',
    maskClosable: false,
    onOk: () => {
      return new Promise((resolve, reject) => {
        useApiStore()
          .DelFollow({
            id: item.id,
            mySelfId: item.mySelfId,
            uperId: item.uperId,
          })
          .then((res) => {
            if (res.code === 0) {
              message.success('删除成功！');
              initData();
              resolve(true);
            } else {
              message.error('删除失败：' + (res.message || '未知错误'));
              reject(false);
            }
          })
          .catch((err) => {
            console.error('删除非关注博主异常：', err);
            message.error('网络异常，请重试');
            reject(false);
          });
      });
    },
  });
};

const goDouyinUp = (item: FollowItem) => {
  if (!item.secUid) {
    message.warning('该博主缺少 secUid，无法打开抖音主页');
    return;
  }

  window.open('https://www.douyin.com/user/' + item.secUid, '_blank', 'noopener,noreferrer');
};
</script>

<style scoped>
/* 原有样式保持不变 */
.dept-user-card-container {
  max-width: 1500px;
  margin: 0 auto;
  padding: 10px;
  min-height: 100vh;
}

.search-tab-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  margin-bottom: 24px;
  flex-wrap: wrap;
}

.tab-wrapper {
  flex: 1;
  min-width: 200px;
}

.search-area {
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.3s ease;
}

.sync-btn {
  height: 40px !important;
  padding: 0 16px !important;
  white-space: nowrap;
  display: flex;
  align-items: center;
  gap: 6px;
  transition: all 0.3s ease;
}

.sync-btn-text {
  font-size: 14px;
}

.sync-btn:disabled {
  background-color: #f5f5f5 !important;
  border-color: #d9d9d9 !important;
  color: #bfbfbf !important;
  cursor: not-allowed !important;
  opacity: 0.8;
}

.search-btn {
  height: 40px !important;
  padding: 0 16px !important;
  white-space: nowrap;
}

.search-input-wrapper {
  width: 280px;
  transition: all 0.3s ease;
}

.search-input {
  width: 100% !important;
  height: 40px !important;
}

.search-input-fade-enter-from,
.search-input-fade-leave-to {
  width: 0 !important;
  opacity: 0;
  overflow: hidden;
}

.search-input-fade-enter-active,
.search-input-fade-leave-active {
  transition: all 0.3s ease;
}

.custom-tabs {
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  overflow: hidden;
}

.ant-tabs-nav-list .ant-tabs-tab:first-child {
  padding-left: 24px !important;
}

.ant-tabs-nav-list .ant-tabs-tab {
  padding-left: 16px !important;
  padding-right: 16px !important;
}

.card-list-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(360px, 1fr));
  gap: 28px !important;
  max-height: calc(100vh - 140px);
  overflow-y: auto;
  padding-bottom: 40px;
}

.custom-card {
  border-radius: 12px !important;
  box-shadow: none !important;
  border: 1px solid #e5e7eb !important;
  transition: all 0.3s ease !important;
  overflow: hidden !important;
  display: flex !important;
  flex-direction: column !important;
  background: transparent !important;
  position: relative; /* 为非关注标记定位 */
  /* 移除之前为右下角按钮预留的底部padding */
}

.custom-card:hover {
  border-color: #c7d2fe !important;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08) !important;
}

/* 非关注卡片特殊样式 */
.no-followed-card {
  border: 1px solid #feb2b2 !important;
  background-color: #fef7fb !important;
}

.no-followed-card:hover {
  border-color: #fc8181 !important;
  box-shadow: 0 4px 12px rgba(239, 68, 68, 0.08) !important;
}

/* 非关注标记 */
.no-followed-tag {
  position: absolute;
  top: 12px;
  left: 12px;
  background-color: #ef4444;
  color: white;
  font-size: 11px;
  padding: 2px 8px;
  border-radius: 12px;
  font-weight: 500;
  z-index: 10;
}

/* 非关注小徽章 */
.no-followed-badge {
  display: inline-block;
  background-color: #fee2e2;
  color: #dc2626;
  font-size: 11px;
  padding: 1px 6px;
  border-radius: 4px;
  margin-left: 8px;
  vertical-align: middle;
}

/* 删除按钮（放在名字+非关注后面） */
.delete-btn {
  display: inline-flex !important;
  align-items: center !important;
  justify-content: center !important;
  margin-left: 8px !important;
  padding: 4px !important;
  height: 24px !important;
  width: 24px !important;
  color: #ef4444 !important;
  border-radius: 50% !important;
  transition: all 0.2s ease;
  vertical-align: middle;
}

.delete-btn:hover {
  color: #dc2626 !important;
  background-color: #fee2e2 !important;
}

.delete-btn:disabled {
  color: #fca5a5 !important;
  cursor: not-allowed;
  background-color: transparent !important;
}

.delete-btn .anticon {
  font-size: 14px !important;
}

.card-inner {
  position: relative;
  padding: 0px 16px !important;
  flex: 1 !important;
  display: flex !important;
  flex-direction: column !important;
}

.card-switch {
  position: absolute !important;
  top: 16px;
  right: 16px;
  z-index: 10;
}

.card-main-content {
  display: flex;
  align-items: flex-start;
  gap: 20px;
  margin-top: 8px !important;
  flex: 1 !important;
}

.avatar-wrapper {
  width: 64px;
  height: 64px;
  flex-shrink: 0;
  margin-top: 4px;
}

.ant-avatar-lg {
  width: 64px !important;
  height: 64px !important;
}

.avatar-placeholder {
  background: linear-gradient(135deg, #4096ff, #69b1ff);
  color: #fff;
  font-size: 24px;
  font-weight: 600;
}

.card-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
  margin: 4px 0;
}

.card-name {
  font-size: 16px !important;
  font-weight: 600;
  color: #1d2129;
  line-height: 1.4;
  display: flex;
  align-items: center;
  flex-wrap: wrap;
}

.card-desc {
  font-size: 12px !important;
  color: #86909c;
  line-height: 1.5;
  width: 100%;
}

.signature-text {
  display: -webkit-box;
  width: 250px;
  max-height: 54px;
  line-height: 1.5;
  overflow: hidden;
  text-overflow: ellipsis;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  cursor: pointer;
  word-break: break-all;
}

.card-path-sync-container {
  display: flex;
  align-items: center;
  width: 100%;
  height: 34px !important;
  margin-top: 8px !important;
  gap: 8px;
}

.path-area {
  display: flex;
  align-items: center;
  gap: 8px;
  flex: 1;
  max-width: calc(100% - 120px);
  overflow: hidden;
}

.path-placeholder {
  width: 100%;
  height: 100%;
}

.path-text {
  font-size: 13px !important;
  color: #4e5969;
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  height: 100%;
  line-height: 34px !important;
}

.path-empty {
  color: #c9cdd4;
  font-style: italic;
}

.sync-switch-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
  white-space: nowrap;
  width: 120px !important;
  justify-content: flex-end;
}

.sync-label {
  font-size: 12px !important;
  color: #6b7280;
  font-weight: 500;
}

.edit-btn {
  width: 32px !important;
  height: 32px !important;
  padding: 0 !important;
  margin: 0 !important;
  border-radius: 50% !important;
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  background: transparent !important;
  color: #4096ff !important;
  border: none !important;
}

.edit-btn .anticon {
  font-size: 18px !important;
}

.edit-btn:hover {
  background: transparent !important;
  color: #2563eb !important;
}

.edit-input-group {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100%;
  height: 100%;
}

.ant-input {
  flex: 1;
  height: 100% !important;
  font-size: 13px !important;
}

:deep(.ant-card-body) {
  padding: 0 !important;
  height: 100% !important;
  display: flex !important;
  flex-direction: column !important;
  padding: 1px !important;
}

.ant-card-bordered {
  border-width: 1px !important;
}

.loading-container {
  grid-column: 1 / -1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 20px 0;
  color: #6b7280;
}

.loading-text {
  font-size: 14px;
}

.no-more-container {
  grid-column: 1 / -1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px 0;
  color: #9ca3af;
  font-size: 14px;
}

.empty-container {
  grid-column: 1 / -1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px 0;
}

/* 新增表单样式 */
.add-form {
  margin-top: 16px;
}

.form-switch-group {
  display: flex;
  flex-wrap: wrap;
  gap: 24px;
  margin-top: 8px;
}

.form-switch-item {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
  min-width: 200px;
}

.switch-label {
  font-size: 14px;
  color: #4e5969;
  white-space: nowrap;
}

/* 移动端适配 */
@media (max-width: 768px) {
  .card-list-container {
    grid-template-columns: 1fr !important;
    gap: 20px !important;
    max-height: calc(100vh - 180px);
  }

  .search-tab-container {
    flex-direction: column;
    align-items: flex-start;
  }

  .search-input-wrapper {
    width: 100% !important;
  }

  .card-inner {
    padding: 14px !important;
  }

  .custom-card {
    height: auto !important;
  }

  .path-area {
    max-width: calc(100% - 110px);
  }

  .sync-switch-wrapper {
    width: 110px !important;
  }

  .sync-label {
    font-size: 11px !important;
  }

  .sync-btn-text {
    display: none;
  }

  .sync-btn {
    padding: 0 12px !important;
  }

  .sync-btn:disabled {
    padding: 0 12px !important;
  }

  .signature-text {
    width: 250px;
    max-height: 30px;
    line-height: 1.25;
    -webkit-line-clamp: 2;
  }

  /* 移动端删除按钮调整 */
  .delete-btn {
    margin-left: 6px !important;
    height: 22px !important;
    width: 22px !important;
  }

  .delete-btn .anticon {
    font-size: 13px !important;
  }

  .form-switch-group {
    flex-direction: column;
    gap: 16px;
  }

  .form-switch-item {
    width: 100%;
  }
}

/* 黑暗模式样式 */
html.dark-mode .dept-user-card-container .custom-card {
  border-color: #374151 !important;
}

html.dark-mode .dept-user-card-container .custom-card:hover {
  border-color: #6366f1 !important;
}

html.dark-mode .dept-user-card-container .card-name {
  color: #f3f4f6 !important;
}

html.dark-mode .dept-user-card-container .card-desc {
  color: #9ca3af !important;
}

html.dark-mode .dept-user-card-container .path-text {
  color: #d1d5db !important;
}

html.dark-mode .dept-user-card-container .path-empty {
  color: #6b7280 !important;
}

html.dark-mode .dept-user-card-container .sync-label {
  color: #9ca3af !important;
}

html.dark-mode .dept-user-card-container .sync-btn {
  color: #d1d5db !important;
  border-color: #4b5563 !important;
}

html.dark-mode .dept-user-card-container .sync-btn:hover {
  color: #f3f4f6 !important;
  border-color: #6b7280 !important;
  background-color: rgba(255, 255, 255, 0.04) !important;
}

html.dark-mode .switch-label {
  color: #d1d5db !important;
}

/* 黑暗模式下非关注卡片样式 */
html.dark-mode .no-followed-card {
  border-color: #7f1d1d !important;
  background-color: transparent !important;
  /* background-color: #2b0707 !important; */
}

html.dark-mode .no-followed-card:hover {
  border-color: #991b1b !important;
  box-shadow: 0 4px 12px rgba(220, 38, 38, 0.15) !important;
}

html.dark-mode .no-followed-tag {
  background-color: #dc2626 !important;
  color: #fef2f2 !important;
}

html.dark-mode .no-followed-badge {
  background-color: #7f1d1d !important;
  color: #fecaca !important;
}

html.dark-mode .delete-btn {
  color: #fecaca !important;
}

html.dark-mode .delete-btn:hover {
  color: #fee2e2 !important;
  background-color: #7f1d1d !important;
}

/* 精准控制uperId表单项的间距 */
.uper-id-form-item {
  --ant-form-item-extra-margin-top: 4px !important; /* 核心：减小extra与input的间距（默认12px） */
}

/* 表单提醒样式 - 进一步减小间距 */
.form-hint {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 0 !important; /* 覆盖可能的默认margin */
  padding-top: 2px; /* 微调上内边距，控制最终间距 */
  font-size: 12px;
  color: #6b7280;
  line-height: 1.4; /* 减小行高，让整体更紧凑 */
  height: auto;
}

.hint-link {
  color: #4096ff;
  text-decoration: underline;
  cursor: pointer;
  transition: color 0.3s ease;
}

.hint-link:hover {
  color: #2563eb;
  text-decoration: none;
}

.hint-desc {
  color: #9ca3af;
  font-size: 11px; /* 小一点的字体，更紧凑 */
}

/* 黑暗模式适配 */
html.dark-mode .form-hint {
  color: #9ca3af;
}

html.dark-mode .hint-link {
  color: #60a5fa;
}

html.dark-mode .hint-link:hover {
  color: #3b82f6;
}

html.dark-mode .hint-desc {
  color: #6b7280;
}

/* 可选：统一调整所有form-item的extra间距（如果需要） */
:deep(.ant-form-item) {
  --ant-form-item-extra-margin-top: 6px; /* 全局调整，优先级低于单独设置的类 */
}

/* 博主姓名可点击样式 */
.author-name-link {
  color: #1890ff;
  cursor: pointer;
  transition: color 0.2s ease;
}
.author-name-link:hover {
  color: #40a9ff;
  text-decoration: underline;
}

/* ===== 关注用户管理页面：视觉美化覆盖 ===== */

/* 页面背景与整体留白 */
.dept-user-card-container {
  box-sizing: border-box;
  max-width: 1560px;
  min-height: 100vh;
  margin: 0 auto;
  padding: 18px 20px 24px;
  background: radial-gradient(circle at 100% 0, rgba(64, 150, 255, 0.08), transparent 28%),
    linear-gradient(180deg, #f8fafc 0%, #f5f7fa 100%);
}

/* 顶部工具栏 */
.search-tab-container {
  position: sticky;
  top: 0;
  z-index: 30;
  gap: 14px;
  margin-bottom: 18px;
  padding: 12px 14px;
  border: 1px solid #e8edf3;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.94);
  box-shadow: 0 8px 28px rgba(31, 45, 61, 0.07);
  backdrop-filter: blur(12px);
}

.tab-wrapper {
  min-width: 260px;
  overflow: hidden;
}

/* 顶部账号 Tab 改为轻量胶囊 */
.custom-tabs {
  overflow: visible;
  border-radius: 12px;
  box-shadow: none;
  background: transparent;
}

:deep(.custom-tabs .ant-tabs-nav) {
  margin: 0 !important;
}

:deep(.custom-tabs .ant-tabs-nav::before) {
  display: none !important;
}

:deep(.custom-tabs .ant-tabs-nav-wrap) {
  overflow-x: auto;
}

:deep(.custom-tabs .ant-tabs-tab) {
  margin: 0 4px 0 0 !important;
  padding: 8px 13px !important;
  border-radius: 10px !important;
  color: #707b88 !important;
  font-size: 13px;
  transition: all 0.2s ease;
}

:deep(.custom-tabs .ant-tabs-tab:hover) {
  color: #1677ff !important;
  background: rgba(22, 119, 255, 0.06);
}

:deep(.custom-tabs .ant-tabs-tab-active) {
  background: rgba(22, 119, 255, 0.1) !important;
}

:deep(.custom-tabs .ant-tabs-tab-active .ant-tabs-tab-btn) {
  color: #1677ff !important;
  font-weight: 600 !important;
}

:deep(.custom-tabs .ant-tabs-ink-bar) {
  display: none !important;
}

/* 筛选与按钮区域 */
.search-area {
  min-width: 0;
  gap: 9px;
  padding: 5px 6px 5px 10px;
  border: 1px solid #edf1f5;
  border-radius: 13px;
  color: #687383;
  background: #f8fafc;
  font-size: 12px;
  line-height: 1;
}

:deep(.search-area .ant-switch) {
  margin-left: -4px;
}

:deep(.search-area .ant-switch-checked) {
  background: #3ba55d !important;
}

.search-btn,
.sync-btn {
  height: 36px !important;
  border-radius: 10px !important;
  box-shadow: none !important;
}

.search-btn {
  width: 36px;
  padding: 0 !important;
  color: #5f6b78;
  border-color: #e3e8ee;
  background: #fff;
}

.search-btn:hover {
  color: #1677ff !important;
  border-color: rgba(22, 119, 255, 0.35) !important;
  background: rgba(22, 119, 255, 0.05) !important;
}

.search-input-wrapper {
  width: 250px;
}

:deep(.search-input.ant-input-affix-wrapper),
.search-input {
  height: 36px !important;
  border-radius: 10px !important;
  border-color: #e2e8f0 !important;
  background: #fff !important;
  box-shadow: none !important;
}

:deep(.search-input.ant-input-affix-wrapper-focused),
:deep(.search-input.ant-input-affix-wrapper:hover) {
  border-color: rgba(22, 119, 255, 0.42) !important;
}

.sync-btn {
  padding: 0 13px !important;
  border: 0 !important;
  color: #fff !important;
  font-weight: 600;
}

.search-area .sync-btn.ant-btn-primary {
  background: linear-gradient(135deg, #4096ff, #1677ff) !important;
  box-shadow: 0 5px 13px rgba(22, 119, 255, 0.2) !important;
}

.search-area .sync-btn.ant-btn-dangerous {
  background: linear-gradient(135deg, #ff7875, #f04444) !important;
  box-shadow: 0 5px 13px rgba(240, 68, 68, 0.18) !important;
}

.search-area .sync-btn:not(:disabled):active {
  transform: translateY(1px);
}

.sync-btn:disabled {
  color: #a7b0ba !important;
  border: 1px solid #e5e9ee !important;
  background: #f1f3f5 !important;
  box-shadow: none !important;
}

/* 卡片列表 */
.card-list-container {
  grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
  gap: 16px !important;
  max-height: calc(100vh - 122px);
  padding: 2px 3px 34px;
  scrollbar-gutter: stable;
}

.card-list-container::-webkit-scrollbar {
  width: 7px;
}

.card-list-container::-webkit-scrollbar-track {
  background: transparent;
}

.card-list-container::-webkit-scrollbar-thumb {
  border-radius: 999px;
  background: rgba(127, 139, 153, 0.25);
}

.load-more-sentinel {
  grid-column: 1 / -1;
  width: 100%;
  height: 1px;
  pointer-events: none;
}

/* 博主卡片 */
.custom-card {
  min-height: 154px;
  overflow: hidden !important;
  border: 1px solid #e7ecf1 !important;
  border-radius: 16px !important;
  background: linear-gradient(180deg, #ffffff 0%, #fbfcfd 100%) !important;
  box-shadow: 0 5px 18px rgba(31, 45, 61, 0.055) !important;
  transition: transform 0.22s ease, box-shadow 0.22s ease, border-color 0.22s ease !important;
}

.custom-card::before {
  content: '';
  position: absolute;
  inset: 0 auto 0 0;
  width: 3px;
  background: linear-gradient(180deg, #69b1ff, #1677ff);
}

.custom-card:hover {
  transform: translateY(-2px);
  border-color: rgba(22, 119, 255, 0.24) !important;
  box-shadow: 0 10px 26px rgba(31, 45, 61, 0.09) !important;
}

/* 非关注卡片 */
.no-followed-card {
  border-color: #f2dada !important;
  background: linear-gradient(180deg, #fffafa 0%, #fffdfd 100%) !important;
}

.no-followed-card::before {
  background: linear-gradient(180deg, #ff8a8a, #ef4444);
}

.no-followed-card:hover {
  border-color: rgba(239, 68, 68, 0.3) !important;
  box-shadow: 0 10px 26px rgba(239, 68, 68, 0.08) !important;
}

:deep(.custom-card .ant-card-body) {
  height: 100% !important;
  padding: 0 !important;
}

.card-inner {
  min-height: 154px;
  padding: 17px 18px 14px 20px !important;
}

/* 主开关 */
.card-switch {
  top: 13px;
  right: 14px;
  padding: 4px 6px;
  border: 1px solid #edf0f3;
  border-radius: 999px;
  background: rgba(248, 250, 252, 0.94);
}

:deep(.card-switch .ant-switch-checked),
:deep(.sync-switch-wrapper .ant-switch-checked) {
  background: #3ba55d !important;
}

/* 头像与主内容 */
.card-main-content {
  gap: 15px;
  margin-top: 3px !important;
}

.avatar-wrapper {
  width: 58px;
  height: 58px;
  margin-top: 2px;
  padding: 3px;
  border: 1px solid rgba(22, 119, 255, 0.15);
  border-radius: 50%;
  background: #fff;
  box-shadow: 0 4px 13px rgba(31, 45, 61, 0.09);
  cursor: pointer;
  transition: transform 0.2s ease;
}

.avatar-wrapper:hover {
  transform: scale(1.04);
}

:deep(.avatar-wrapper .ant-avatar),
:deep(.avatar-wrapper .ant-avatar-lg) {
  width: 50px !important;
  height: 50px !important;
}

.avatar-placeholder {
  font-size: 19px;
  background: linear-gradient(135deg, #4096ff, #69b1ff);
}

.card-content {
  min-width: 0;
  gap: 6px;
  margin: 1px 0 0;
}

/* 姓名行 */
.card-name {
  min-height: 27px;
  padding-right: 70px;
  color: #25313b;
  font-size: 15px !important;
  line-height: 1.35;
}

.author-name-link {
  display: inline-block;
  max-width: 170px;
  overflow: hidden;
  color: #25313b;
  font-weight: 700;
  white-space: nowrap;
  text-overflow: ellipsis;
  vertical-align: middle;
}

.author-name-link:hover {
  color: #1677ff;
  text-decoration: none;
}

.card-name > span:nth-child(2) {
  margin-left: 6px;
  padding: 2px 7px;
  border-radius: 999px;
  color: #85909c;
  background: #f1f4f7;
  font-size: 10px !important;
  line-height: 1.45;
  vertical-align: middle;
}

.no-followed-badge {
  margin-left: 6px;
  padding: 2px 7px;
  border-radius: 999px;
  color: #d9363e;
  background: #fff0f0;
  font-size: 10px;
  line-height: 1.45;
}

.delete-btn {
  width: 25px !important;
  height: 25px !important;
  margin-left: 5px !important;
  border-radius: 8px !important;
  background: rgba(239, 68, 68, 0.06) !important;
}

/* 签名 */
.card-desc {
  min-height: 34px;
  padding: 7px 9px;
  border-radius: 9px;
  color: #7f8b96;
  background: #f7f9fa;
}

.signature-text {
  width: 100%;
  max-height: 34px;
  color: inherit;
  font-size: 11px;
  line-height: 1.45;
  -webkit-line-clamp: 2;
}

/* 底部路径和全量同步 */
.card-path-sync-container {
  height: 34px !important;
  margin-top: 2px !important;
  gap: 7px;
}

.path-area {
  max-width: calc(100% - 112px);
  min-width: 0;
  height: 32px;
  padding: 0 5px 0 9px;
  border: 1px solid #edf0f3;
  border-radius: 9px;
  background: #f7f9fa;
}

.path-text {
  min-width: 0;
  height: 30px;
  color: #56616d;
  font-size: 11px !important;
  line-height: 30px !important;
}

.path-empty {
  color: #a9b1b9;
}

.edit-btn {
  width: 27px !important;
  height: 27px !important;
  border-radius: 8px !important;
  color: #1677ff !important;
}

.edit-btn:hover {
  background: rgba(22, 119, 255, 0.07) !important;
}

.edit-btn .anticon {
  font-size: 14px !important;
}

.edit-input-group {
  gap: 5px;
}

:deep(.edit-input-group .ant-input) {
  height: 28px !important;
  border: 0 !important;
  background: transparent !important;
  box-shadow: none !important;
}

.sync-switch-wrapper {
  width: 105px !important;
  height: 32px;
  padding: 0 8px;
  gap: 6px;
  border-radius: 9px;
  background: rgba(59, 165, 93, 0.08);
}

.sync-label {
  color: #64806c;
  font-size: 10px !important;
}

/* 加载、空状态 */
.loading-container,
.no-more-container,
.empty-container {
  border-radius: 14px;
}

.loading-container {
  min-height: 90px;
  color: #73808d;
}

.no-more-container {
  padding: 14px 0;
  color: #9aa4ad;
}

.empty-container {
  min-height: 320px;
  border: 1px dashed #dce3e8;
  background: rgba(255, 255, 255, 0.65);
}

/* 新增弹窗 */
:deep(.ant-modal-content) {
  overflow: hidden;
  border-radius: 16px !important;
  box-shadow: 0 18px 55px rgba(31, 45, 61, 0.18) !important;
}

:deep(.ant-modal-header) {
  margin: 0 !important;
  padding: 18px 22px 14px !important;
  border-bottom: 1px solid #edf0f3;
}

:deep(.ant-modal-title) {
  color: #25313b;
  font-size: 17px;
  font-weight: 700;
}

:deep(.ant-modal-body) {
  padding: 16px 22px 10px !important;
}

:deep(.ant-modal-footer) {
  padding: 12px 22px 18px !important;
  border-top: 1px solid #edf0f3;
}

:deep(.add-form .ant-input) {
  height: 38px !important;
  border-radius: 9px !important;
}

:deep(.add-form .ant-form-item-label > label) {
  color: #596573;
  font-size: 13px;
}

/* 平板及移动端 */
@media (max-width: 900px) {
  .dept-user-card-container {
    padding: 10px 10px 18px;
  }

  .search-tab-container {
    position: static;
    align-items: stretch;
    padding: 10px;
    border-radius: 14px;
  }

  .tab-wrapper {
    width: 100%;
  }

  .search-area {
    width: 100%;
    box-sizing: border-box;
    flex-wrap: wrap;
  }

  .search-input-wrapper {
    flex: 1;
    min-width: 180px;
    width: auto;
  }

  .card-list-container {
    max-height: none;
    overflow-y: visible;
  }
}

@media (max-width: 768px) {
  .card-list-container {
    gap: 12px !important;
  }

  .custom-card {
    min-height: 148px;
  }

  .card-inner {
    min-height: 148px;
    padding: 15px 14px 13px 17px !important;
  }

  .card-main-content {
    gap: 12px;
  }

  .avatar-wrapper {
    width: 52px;
    height: 52px;
  }

  :deep(.avatar-wrapper .ant-avatar),
  :deep(.avatar-wrapper .ant-avatar-lg) {
    width: 44px !important;
    height: 44px !important;
  }

  .card-name {
    padding-right: 62px;
  }

  .author-name-link {
    max-width: 130px;
  }

  .signature-text {
    width: 100%;
  }

  .path-area {
    max-width: calc(100% - 101px);
  }

  .sync-switch-wrapper {
    width: 94px !important;
  }

  .search-area {
    font-size: 11px;
  }
}

/* ===== 关注用户管理页面：紧凑版覆盖 ===== */

/* 页面整体更紧凑 */
.dept-user-card-container {
  padding: 10px 12px 16px;
}

/* 顶部工具栏压缩高度和留白 */
.search-tab-container {
  gap: 10px;
  margin-bottom: 12px;
  padding: 8px 10px;
  border-radius: 13px;
}

:deep(.custom-tabs .ant-tabs-tab) {
  padding: 6px 10px !important;
  font-size: 12px;
}

.search-area {
  gap: 6px;
  padding: 4px 5px 4px 8px;
  border-radius: 11px;
  font-size: 11px;
}

.search-btn,
.sync-btn {
  height: 32px !important;
  border-radius: 8px !important;
}

.search-btn {
  width: 32px;
}

.search-input-wrapper {
  width: 220px;
}

:deep(.search-input.ant-input-affix-wrapper),
.search-input {
  height: 32px !important;
  border-radius: 8px !important;
}

.sync-btn {
  padding: 0 10px !important;
  gap: 4px;
}

.sync-btn-text {
  font-size: 12px;
}

/* 卡片列表间距缩小 */
.card-list-container {
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 10px !important;
  max-height: calc(100vh - 100px);
  padding: 1px 2px 20px;
}

/* 卡片更矮、更紧凑 */
.custom-card {
  min-height: 128px;
  border-radius: 13px !important;
}

.card-inner {
  min-height: 128px;
  padding: 12px 13px 10px 15px !important;
}

.card-switch {
  top: 10px;
  right: 11px;
  padding: 2px 4px;
}

/* 头像缩小 */
.card-main-content {
  gap: 11px;
  margin-top: 1px !important;
}

.avatar-wrapper {
  width: 48px;
  height: 48px;
  padding: 2px;
}

:deep(.avatar-wrapper .ant-avatar),
:deep(.avatar-wrapper .ant-avatar-lg) {
  width: 42px !important;
  height: 42px !important;
}

.avatar-placeholder {
  font-size: 16px;
}

/* 内容区域压缩 */
.card-content {
  gap: 4px;
  margin-top: 0;
}

.card-name {
  min-height: 23px;
  padding-right: 58px;
  font-size: 14px !important;
}

.author-name-link {
  max-width: 150px;
  font-size: 14px;
}

.card-name > span:nth-child(2) {
  margin-left: 4px;
  padding: 1px 6px;
  font-size: 9px !important;
}

.no-followed-badge {
  margin-left: 4px;
  padding: 1px 6px;
  font-size: 9px;
}

.delete-btn {
  width: 22px !important;
  height: 22px !important;
  margin-left: 4px !important;
}

/* 签名区域更低 */
.card-desc {
  min-height: 28px;
  padding: 5px 7px;
  border-radius: 8px;
}

.signature-text {
  max-height: 28px;
  font-size: 10px;
  line-height: 1.4;
}

/* 路径和全量同步区域压缩 */
.card-path-sync-container {
  height: 29px !important;
  margin-top: 0 !important;
  gap: 5px;
}

.path-area {
  max-width: calc(100% - 96px);
  height: 28px;
  padding: 0 4px 0 7px;
  border-radius: 8px;
}

.path-text {
  height: 26px;
  font-size: 10px !important;
  line-height: 26px !important;
}

.edit-btn {
  width: 24px !important;
  height: 24px !important;
  border-radius: 7px !important;
}

.edit-btn .anticon {
  font-size: 12px !important;
}

:deep(.edit-input-group .ant-input) {
  height: 24px !important;
  font-size: 11px !important;
}

.sync-switch-wrapper {
  width: 90px !important;
  height: 28px;
  padding: 0 6px;
  gap: 4px;
  border-radius: 8px;
}

.sync-label {
  font-size: 9px !important;
}

/* 状态区域压缩 */
.loading-container {
  min-height: 64px;
  padding: 12px 0;
}

.no-more-container {
  padding: 10px 0;
  font-size: 12px;
}

.empty-container {
  min-height: 220px;
  padding: 24px 0;
}

/* 弹窗更紧凑 */
:deep(.ant-modal-header) {
  padding: 14px 18px 10px !important;
}

:deep(.ant-modal-body) {
  padding: 12px 18px 6px !important;
}

:deep(.ant-modal-footer) {
  padding: 10px 18px 14px !important;
}

:deep(.add-form .ant-form-item) {
  margin-bottom: 14px !important;
}

:deep(.add-form .ant-input) {
  height: 34px !important;
  border-radius: 8px !important;
}

/* 平板 */
@media (max-width: 900px) {
  .dept-user-card-container {
    padding: 8px 8px 14px;
  }

  .search-tab-container {
    padding: 8px;
  }

  .search-area {
    gap: 5px;
  }

  .search-input-wrapper {
    min-width: 150px;
  }
}

/* 移动端 */
@media (max-width: 768px) {
  .card-list-container {
    gap: 8px !important;
  }

  .custom-card,
  .card-inner {
    min-height: 122px;
  }

  .card-inner {
    padding: 11px 11px 9px 13px !important;
  }

  .card-main-content {
    gap: 9px;
  }

  .avatar-wrapper {
    width: 44px;
    height: 44px;
  }

  :deep(.avatar-wrapper .ant-avatar),
  :deep(.avatar-wrapper .ant-avatar-lg) {
    width: 38px !important;
    height: 38px !important;
  }

  .card-name {
    padding-right: 52px;
  }

  .author-name-link {
    max-width: 120px;
  }

  .path-area {
    max-width: calc(100% - 88px);
  }

  .sync-switch-wrapper {
    width: 82px !important;
  }
}

/* ===== 底部间距压缩与暗色主题恢复 ===== */

/* 页面底部留白进一步压缩 */
.dept-user-card-container {
  padding-bottom: 6px;
}

.card-list-container {
  padding-bottom: 6px;
}

.no-more-container {
  padding-top: 6px;
  padding-bottom: 4px;
}

.loading-container {
  padding-bottom: 6px;
}

.empty-container {
  margin-bottom: 0;
}

/* 平板与移动端底部留白同步压缩 */
@media (max-width: 900px) {
  .dept-user-card-container {
    padding-bottom: 6px;
  }

  .card-list-container {
    padding-bottom: 4px;
  }
}

@media (max-width: 768px) {
  .dept-user-card-container {
    padding-bottom: 4px;
  }

  .card-list-container {
    padding-bottom: 2px;
  }
}

/*
 * 暗色主题不再使用美化版新增的背景色和卡片色，
 * 继续沿用原文件中已有的 dark-mode 配色。
 */

/* ===== 卡片底部压缩 + 暗色主题恢复 ===== */

/* 关闭同步时，不再为路径区域保留空白高度 */
.card-path-sync-container:has(.path-placeholder) {
  height: 0 !important;
  min-height: 0 !important;
  margin-top: 0 !important;
  padding: 0 !important;
  overflow: hidden !important;
}

.path-placeholder {
  display: none !important;
}

/* 卡片整体再压缩一点，主要减少底部空白 */
.custom-card {
  min-height: 108px !important;
}

.card-inner {
  min-height: 108px !important;
  padding-top: 10px !important;
  padding-bottom: 8px !important;
}

.card-main-content {
  margin-top: 0 !important;
}

.card-content {
  gap: 3px !important;
}

.card-desc {
  min-height: 24px !important;
  padding-top: 4px !important;
  padding-bottom: 4px !important;
}

.signature-text {
  max-height: 26px !important;
}

.card-path-sync-container {
  height: 27px !important;
}

.path-area,
.sync-switch-wrapper {
  height: 26px !important;
}

.path-text {
  height: 24px !important;
  line-height: 24px !important;
}

/* 暗色主题：恢复成原先深色页面效果 */
html.dark-mode .dept-user-card-container {
  background: transparent !important;
}

/* 顶部不再显示白色工具卡片 */
html.dark-mode .dept-user-card-container .search-tab-container {
  padding: 4px 0 10px !important;
  border: 0 !important;
  border-radius: 0 !important;
  background: transparent !important;
  box-shadow: none !important;
  backdrop-filter: none !important;
}

html.dark-mode .dept-user-card-container .custom-tabs {
  background: transparent !important;
}

html.dark-mode .dept-user-card-container :deep(.custom-tabs .ant-tabs-tab) {
  color: #c2c3d1 !important;
  background: transparent !important;
}

html.dark-mode .dept-user-card-container :deep(.custom-tabs .ant-tabs-tab:hover) {
  color: #b26cff !important;
  background: transparent !important;
}

html.dark-mode .dept-user-card-container :deep(.custom-tabs .ant-tabs-tab-active) {
  background: transparent !important;
}

html.dark-mode .dept-user-card-container :deep(.custom-tabs .ant-tabs-tab-active .ant-tabs-tab-btn) {
  color: #9b4dff !important;
}

html.dark-mode .dept-user-card-container :deep(.custom-tabs .ant-tabs-ink-bar) {
  display: block !important;
  height: 2px !important;
  background: #8b3dff !important;
}

/* 顶部筛选区恢复透明深色 */
html.dark-mode .dept-user-card-container .search-area {
  padding-left: 0 !important;
  padding-right: 0 !important;
  color: #d1d1dc !important;
  border: 0 !important;
  background: transparent !important;
}

html.dark-mode .dept-user-card-container .search-btn {
  color: #d1d1dc !important;
  border-color: #34354a !important;
  background: transparent !important;
}

html.dark-mode .dept-user-card-container .search-btn:hover {
  color: #ffffff !important;
  border-color: #52546d !important;
  background: rgba(255, 255, 255, 0.04) !important;
}

/* 新增按钮恢复紫色，立即同步恢复红色 */
html.dark-mode .dept-user-card-container .search-area .sync-btn.ant-btn-primary {
  color: #ffffff !important;
  background: #7c3aed !important;
  box-shadow: none !important;
}

html.dark-mode .dept-user-card-container .search-area .sync-btn.ant-btn-dangerous {
  color: #ffffff !important;
  background: #ef4444 !important;
  box-shadow: none !important;
}

html.dark-mode .dept-user-card-container .search-input-wrapper :deep(.ant-input),
html.dark-mode .dept-user-card-container :deep(.search-input.ant-input-affix-wrapper) {
  color: #e9e9f1 !important;
  border-color: #34354a !important;
  background: #1b1b31 !important;
}

/* 卡片恢复为原先的深色、透明感 */
html.dark-mode .dept-user-card-container .custom-card {
  border-color: #34364b !important;
  background: rgba(25, 25, 45, 0.82) !important;
  box-shadow: none !important;
}

html.dark-mode .dept-user-card-container .custom-card::before {
  display: none !important;
}

html.dark-mode .dept-user-card-container .custom-card:hover {
  transform: none !important;
  border-color: #4a4d67 !important;
  box-shadow: none !important;
}

html.dark-mode .dept-user-card-container .no-followed-card {
  border-color: #34364b !important;
  background: rgba(25, 25, 45, 0.82) !important;
}

html.dark-mode .dept-user-card-container .no-followed-card:hover {
  border-color: #4a4d67 !important;
}

/* 主开关区域恢复透明 */
html.dark-mode .dept-user-card-container .card-switch {
  padding: 0 !important;
  border: 0 !important;
  background: transparent !important;
}

/* 头像恢复原样，不显示白色外圈和阴影 */
html.dark-mode .dept-user-card-container .avatar-wrapper {
  padding: 0 !important;
  border: 0 !important;
  background: transparent !important;
  box-shadow: none !important;
}

/* 文字颜色恢复原先层级 */
html.dark-mode .dept-user-card-container .card-name {
  color: #f1f1f5 !important;
}

html.dark-mode .dept-user-card-container .author-name-link {
  color: #29a8ff !important;
}

html.dark-mode .dept-user-card-container .author-name-link:hover {
  color: #64c2ff !important;
}

html.dark-mode .dept-user-card-container .card-name > span:nth-child(2) {
  color: #f0f0f5 !important;
  background: transparent !important;
}

html.dark-mode .dept-user-card-container .card-desc {
  color: #9091a2 !important;
  background: transparent !important;
}

html.dark-mode .dept-user-card-container .signature-text {
  color: #9091a2 !important;
}

/* 路径和全量同步恢复深色控件感 */
html.dark-mode .dept-user-card-container .path-area {
  border-color: transparent !important;
  background: rgba(255, 255, 255, 0.035) !important;
}

html.dark-mode .dept-user-card-container .path-text {
  color: #c8c9d3 !important;
}

html.dark-mode .dept-user-card-container .path-empty {
  color: #747586 !important;
}

html.dark-mode .dept-user-card-container .sync-switch-wrapper {
  background: transparent !important;
}

html.dark-mode .dept-user-card-container .sync-label {
  color: #a4a5b2 !important;
}

html.dark-mode .dept-user-card-container .edit-btn {
  color: #30aaff !important;
}

html.dark-mode .dept-user-card-container .edit-btn:hover {
  color: #69c0ff !important;
  background: transparent !important;
}

/* 非关注标识和删除按钮恢复柔和红色 */
html.dark-mode .dept-user-card-container .no-followed-badge {
  color: #ffb8b8 !important;
  background: rgba(239, 68, 68, 0.15) !important;
}

html.dark-mode .dept-user-card-container .delete-btn {
  color: #ffc0c0 !important;
  background: transparent !important;
}

html.dark-mode .dept-user-card-container .delete-btn:hover {
  color: #ffffff !important;
  background: rgba(239, 68, 68, 0.18) !important;
}

/* 列表和状态区域保持深色页面背景 */
html.dark-mode .dept-user-card-container .card-list-container,
html.dark-mode .dept-user-card-container .loading-container,
html.dark-mode .dept-user-card-container .no-more-container {
  background: transparent !important;
}

html.dark-mode .dept-user-card-container .empty-container {
  color: #8f90a0 !important;
  border-color: #34364b !important;
  background: transparent !important;
}

/* 移动端也同步压缩卡片底部 */
@media (max-width: 768px) {
  .custom-card,
  .card-inner {
    min-height: 102px !important;
  }

  .card-inner {
    padding-bottom: 7px !important;
  }
}
</style>