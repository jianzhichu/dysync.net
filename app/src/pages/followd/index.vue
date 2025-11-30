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
        <!-- 搜索按钮 -->
        <a-button type="default" class="search-btn" @click="toggleSearchInput">
          <CloseOutlined v-if="searchInputVisible" />
          <SearchOutlined v-else />
        </a-button>
        <transition name="search-input-fade">
          <div v-if="searchInputVisible" class="search-input-wrapper">
            <a-input v-model:value="quaryData.followUserName" placeholder="输入关注的用户名，按回车" allow-clear @pressEnter="handleSearch" class="search-input" ref="searchInputRef" />
          </div>
        </transition>
        <!-- 同步按钮（请求期间禁用） -->
        <a-button type="primary" class="sync-btn" @click="handleSyncAll" :disabled="isSyncDisabled">
          <SyncOutlined />
          <span class="sync-btn-text">立即同步</span>
        </a-button>
      </div>
    </div>
    <!-- 卡片列表区域 -->
    <div class="card-list-container" @scroll="handleScroll">
      <a-card v-for="(item, index) in currentTabData" :key="item.id" :data-key="item.id" class="custom-card" :bordered="true" :hoverable="true">
        <div class="card-inner">
          <div class="card-switch">
            <a-switch v-model:checked="item.openSync" @change="(checked) => handleSwitchChange(item, checked)" checked-children="开" un-checked-children="关" />
          </div>
          <div class="card-main-content">
            <div class="avatar-wrapper">
              <a-avatar shape="circle" size="large" :src="item.uperAvatar" v-if="item.uperAvatar" />
              <a-avatar shape="circle" size="large" v-else class="avatar-placeholder">
                {{ item.uperName.charAt(0) }}
              </a-avatar>
            </div>
            <div class="card-content">
              <div class="card-name">{{ item.uperName }}</div>
              <!-- 签名多行显示：高度控制 + 溢出截断 + Tooltip气泡 -->
              <div class="card-desc">
                <a-tooltip placement="top" :title="item.signature || '无签名'">
                  <span class="signature-text">
                    {{ item.signature || '无签名' }}
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
                        <a-input v-model:value="item.savePath" placeholder="请输入文件夹名称" @keypress.enter="() => handleSavePath(item)" maxlength="10" :disabled="item.isSaving" />
                        <!-- 保存按钮禁用：item.isSaving 为 true 时 -->
                        <a-button type="text" class="edit-btn" @click="() => handleSavePath(item)" :disabled="item.isSaving">
                          <SaveOutlined />
                        </a-button>
                      </div>
                    </template>
                    <template v-else>
                      <span class="path-text" :class="{ 'path-empty': !item.savePath }">
                        {{ item.savePath || '默认用作者名字' }}
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

      <!-- 加载状态 -->
      <div v-if="loading" class="loading-container">
        <a-spin size="middle" />
        <span class="loading-text">加载中...</span>
      </div>
      <!-- 无更多数据 -->
      <div v-if="noMoreData && followData.length > 0" class="no-more-container">暂无更多数据</div>
      <!-- 空状态 -->
      <div v-if="followData.length === 0 && !loading" class="empty-container">
        <Empty description="暂无关注用户，或Cookie设置中未设置SecUserId" />
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted, onUnmounted, UnwrapRef, reactive, nextTick } from 'vue';
import { message, Spin, Empty, Tooltip } from 'ant-design-vue';
import { useApiStore } from '@/store';
import { SyncOutlined, CloseOutlined, SearchOutlined, SaveOutlined, EditOutlined } from '@ant-design/icons-vue';

// Tab列表数据
const tabList = ref<Array<{ key: string; name: string; total?: number }>>([]);
const activeTabKey = ref('');

// 关注用户列表数据：新增 isSaving 状态（控制保存期间按钮禁用）
const followData = ref<
  Array<{
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
    isSaving?: boolean; // 新增：保存中状态（true=禁用按钮）
  }>
>([]);

// 状态变量
const loading = ref(false);
const noMoreData = ref(false);
const hasMore = ref(true);
const searchInputVisible = ref(false);
const searchInputRef = ref<HTMLInputElement | null>(null);
const isSyncDisabled = ref(false); // 同步按钮禁用状态

// 搜索参数
interface QuaryParam {
  pageIndex: number;
  pageSize: number;
  followUserName: string | null;
  mySelfId?: string;
}
const quaryData: UnwrapRef<QuaryParam> = reactive({
  pageIndex: 0,
  pageSize: 20,
  followUserName: null,
  mySelfId: '',
});

// 生命周期 - 挂载时初始化
onMounted(() => {
  quaryData.pageIndex = 0;
  GetCookies();
});

// 生命周期 - 卸载时移除滚动监听
onUnmounted(() => {
  const cardContainer = document.querySelector('.dept-user-card-container .card-list-container');
  if (cardContainer) {
    cardContainer.removeEventListener('scroll', handleScroll);
  }
});

// 获取Cookie列表（Tab数据）
const GetCookies = () => {
  useApiStore()
    .CookieList()
    .then((res) => {
      if (res.code === 0) {
        const tabs = (res.data || [{ key: 'tab1', name: '部门一' }]).map((tab) => ({
          ...tab,
          total: 0,
        }));
        tabList.value = tabs;
        activeTabKey.value = tabList.value[0]?.key || '';
        quaryData.mySelfId = activeTabKey.value;
        GetFollows(true);
        handleTabChange(activeTabKey.value);
      }
    });
};

// 获取关注用户列表
const GetFollows = (isReset = false) => {
  if (loading.value || (noMoreData.value && !isReset)) return;

  loading.value = true;

  useApiStore()
    .FollowList(quaryData)
    .then((res) => {
      if (res.code === 0) {
        const newData = res.data.data || [];
        const total = res.data.total || 0;

        // 给新数据初始化 isSaving: false（避免未定义导致的禁用异常）
        const formattedData = newData.map((item) => ({
          ...item,
          isSaving: false, // 初始化保存中状态为false
          isEditing: item.isEditing ?? false, // 兼容后端未返回isEditing的情况
        }));

        // 更新当前Tab的总数
        if (isReset) {
          const tabIndex = tabList.value.findIndex((tab) => tab.key === activeTabKey.value);
          if (tabIndex !== -1) {
            tabList.value[tabIndex].total = total;
          }
        }

        // 判断是否还有更多数据
        if (formattedData.length < quaryData.pageSize) {
          noMoreData.value = true;
          hasMore.value = false;
        } else {
          noMoreData.value = false;
          hasMore.value = true;
        }

        // 合并数据（避免重复）
        if (isReset) {
          followData.value = formattedData;
        } else {
          const existingKeys = followData.value.map((item) => item.id);
          const uniqueNewData = formattedData.filter((item) => !existingKeys.includes(item.id));
          followData.value = [...followData.value, ...uniqueNewData];
        }

        quaryData.pageIndex += 1;
      } else {
        message.error('获取关注用户列表失败');
        noMoreData.value = true;
        hasMore.value = false;
      }
    })
    .catch((err) => {
      console.error('获取关注用户列表异常：', err);
      message.error('网络异常，请重试');
      noMoreData.value = true;
      hasMore.value = false;
    })
    .finally(() => {
      loading.value = false;
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
  quaryData.pageIndex = 0;
  noMoreData.value = false;
  hasMore.value = true;
  GetFollows(true);
};

// Tab切换
const handleTabChange = (key: string) => {
  activeTabKey.value = key;
  quaryData.mySelfId = key;
  quaryData.pageIndex = 0;
  noMoreData.value = false;
  hasMore.value = true;
  searchInputVisible.value = false;
  quaryData.followUserName = null;
  GetFollows(true);
};

// 计算当前Tab的用户数据
const currentTabData = computed(() => {
  return followData.value.filter((item) => item.mySelfId === activeTabKey.value);
});

// 滚动加载更多
const handleScroll = () => {
  const cardContainer = document.querySelector('.dept-user-card-container .card-list-container');
  if (!cardContainer || loading.value || !hasMore.value || noMoreData.value) return;

  const { scrollTop, scrollHeight, clientHeight } = cardContainer as HTMLDivElement;

  // 滚动到底部100px内时加载更多
  if (scrollTop + clientHeight >= scrollHeight - 100) {
    GetFollows(false);
  }
};

// 开关状态变更（启用/禁用用户）
const handleSwitchChange = (item, checked) => {
  uploadSyncStatus(item);
};

// 全量同步开关变更
const handleSyncChange = (item, checked) => {
  item.fullSync = checked;
  uploadFullSyncStatus(item);
};

// 编辑存储路径：增加防重复编辑（保存中不可编辑）
const handleEditPath = (item) => {
  if (item.isSaving) return; // 正在保存时，禁止点击编辑
  item.isEditing = true;
  // 延迟聚焦输入框（确保DOM已更新）
  setTimeout(() => {
    const input = document.querySelector(
      `.dept-user-card-container .custom-card[data-key="${item.id}"] .edit-input-group .ant-input`
    ) as HTMLInputElement | null;
    input?.focus();
  }, 100);
};

// 保存存储路径：增加按钮禁用逻辑
const handleSavePath = (item) => {
  // 防止重复提交：正在保存时直接返回
  if (item.isSaving) return;
  uploadSyncStatus(item);
};
const uploadSyncStatus = (item) => {
  useApiStore()
    .OpenOrCloseSync({
      Id: item.id,
      OpenSync: item.openSync,
      FullSync: item.fullSync,
      SavePath: item.savePath, // 保持原有逻辑：提交 savePath
    })
    .then((res) => {
      if (res.code === 0) {
        message.success(`保存成功，将在下次任务执行时生效`);
        item.isEditing = false; // 保存成功后关闭编辑状态
      } else {
        message.error('保存失败' + (res.msg || '未知错误'));
      }
    })
    .catch((err) => {
      console.error('保存失败', err);
      message.error('保存失败，请重试');
    })
    .finally(() => {
      // 接口响应结束：解除禁用（无论成功失败）
      item.isSaving = false;
    });
};

//全量同步开关
const uploadFullSyncStatus = (item) => {
  item.isSaving = true;
  uploadSyncStatus(item);
};

// 批量同步所有用户
const handleSyncAll = () => {
  if (isSyncDisabled.value) return;

  // 请求开始：禁用按钮+显示加载
  isSyncDisabled.value = true;
  loading.value = true;

  useApiStore()
    .SyncFollow()
    .then((res) => {
      if (res.code === 0) {
        message.success('后台开始同步...根据您关注的数量，需要的时间不一定...请耐心等待');
      } else {
        message.error('同步失败：' + (res.msg || '未知错误'));
      }
    })
    .catch((err) => {
      console.error('同步异常：', err);
      message.error('同步失败，请重试');
    })
    .finally(() => {
      // 请求完成：解禁按钮+关闭加载
      isSyncDisabled.value = false;
      loading.value = false;
    });
};
</script>

<style scoped>
/* 原有样式保持不变，无需修改 */
.dept-user-card-container {
  max-width: 1500px;
  margin: 0 auto;
  padding: 10px;
  min-height: 100vh;
}

/* 搜索+Tab容器 */
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

/* 同步按钮样式 */
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

/* 同步按钮禁用样式（浅色模式） */
.sync-btn:disabled {
  background-color: #f5f5f5 !important;
  border-color: #d9d9d9 !important;
  color: #bfbfbf !important;
  cursor: not-allowed !important;
  opacity: 0.8;
}

/* 搜索按钮样式 */
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

/* 搜索框过渡动画 */
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

/* Tab样式 */
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

/* 卡片列表容器 */
.card-list-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(360px, 1fr));
  gap: 28px !important;
  max-height: calc(100vh - 140px);
  overflow-y: auto;
  padding-bottom: 40px;
}

/* 卡片样式 */
.custom-card {
  border-radius: 12px !important;
  box-shadow: none !important;
  border: 1px solid #e5e7eb !important;
  transition: all 0.3s ease !important;
  overflow: hidden !important;
  display: flex !important;
  flex-direction: column !important;
  background: transparent !important;
}

.custom-card:hover {
  border-color: #c7d2fe !important;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08) !important;
}

.card-inner {
  position: relative;
  padding: 5px !important;
  flex: 1 !important;
  display: flex !important;
  flex-direction: column !important;
}

/* 卡片开关位置 */
.card-switch {
  position: absolute !important;
  top: 12px;
  right: 16px;
  z-index: 10;
}

/* 卡片主要内容 */
.card-main-content {
  display: flex;
  align-items: flex-start; /* 改为顶部对齐，避免多行签名导致头像居中错位 */
  gap: 20px;
  margin-top: 8px !important;
  flex: 1 !important;
}

/* 头像样式 */
.avatar-wrapper {
  width: 64px;
  height: 64px;
  flex-shrink: 0;
  margin-top: 4px; /* 微调头像位置，与多行签名对齐 */
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

/* 卡片内容区域 */
.card-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px; /* 缩小行间距，适配多行签名 */
  margin: 4px 0;
}

.card-name {
  font-size: 16px !important;
  font-weight: 600;
  color: #1d2129;
  line-height: 1.4;
}

/* 核心修改：签名多行显示（高度控制） */
.card-desc {
  font-size: 12px !important;
  color: #86909c;
  line-height: 1.5; /* 行高适配多行 */
  width: 100%;
  /* 保持原有宽度，不影响布局 */
}

.signature-text {
  display: -webkit-box; /* 关键：启用弹性盒模型 */
  width: 250px; /* 固定宽度（原300px不变） */
  max-height: 54px; /* 控制显示行数：12px字体+1.5行高 → 2行=36px，3行=54px */
  line-height: 1.5; /* 行高，需与font-size配合计算高度 */
  overflow: hidden; /* 隐藏溢出内容 */
  text-overflow: ellipsis; /* 溢出显示省略号 */
  -webkit-line-clamp: 3; /* 关键：限制显示2行（可改为3行） */
  -webkit-box-orient: vertical; /* 垂直排列 */
  cursor: pointer; /* 提示可悬停查看完整内容 */
  word-break: break-all; /* 长单词/URL自动换行，避免横向溢出 */
}

/* 路径+同步开关容器 */
.card-path-sync-container {
  display: flex;
  align-items: center;
  width: 100%;
  height: 34px !important;
  margin-top: 8px !important; /* 微调与签名的间距，适配多行 */
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

/* 全量同步开关区域 */
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

/* 编辑按钮样式 */
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

/* 加载状态 */
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

/* 无更多数据 */
.no-more-container {
  grid-column: 1 / -1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px 0;
  color: #9ca3af;
  font-size: 14px;
}

/* 空状态 */
.empty-container {
  grid-column: 1 / -1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px 0;
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
    height: auto !important; /* 移动端卡片高度自适应，避免多行签名溢出 */
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

  /* 移动端签名样式调整 */
  .signature-text {
    width: 250px; /* 移动端宽度缩减 */
    max-height: 30px; /* 移动端显示2行（12px字体+1.25行高） */
    line-height: 1.25;
    -webkit-line-clamp: 2; /* 保持2行显示 */
  }
}

/* ====================================== */
/* 黑暗模式样式（html.dark-mode 强制生效） */
/* ====================================== */
html.dark-mode .dept-user-card-container .custom-card {
  border-color: #374151 !important;
}

html.dark-mode .dept-user-card-container .custom-card:hover {
  border-color: #6366f1 !important;
}

html.dark-mode .dept-user-card-container .ant-card-bordered {
  border-color: #374151 !important;
}

html.dark-mode .dept-user-card-container .ant-card-bordered:hover {
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

html.dark-mode .dept-user-card-container .loading-text,
html.dark-mode .dept-user-card-container .no-more-container span {
  color: #9ca3af !important;
}

/* 黑暗模式下同步按钮样式 */
html.dark-mode .dept-user-card-container .sync-btn {
  color: #d1d5db !important;
  border-color: #4b5563 !important;
}

html.dark-mode .dept-user-card-container .sync-btn:hover {
  color: #f3f4f6 !important;
  border-color: #6b7280 !important;
  background-color: rgba(255, 255, 255, 0.04) !important;
}
</style>