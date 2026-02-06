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
    <div class="card-list-container" @scroll="handleScroll">
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
                {{ item.uperName }} <span style="font-size:12px;">{{item.douyinNo?`(${item.douyinNo})`:''}}</span>
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
}

interface QuaryParam {
  pageIndex: number;
  pageSize: number;
  followUserName: string | null;
  mySelfId?: string;
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
});

// 生命周期 - 挂载时初始化
onMounted(() => {
  quaryData.pageIndex = 0;
  initData();
});

// 生命周期 - 卸载时移除滚动监听
onUnmounted(() => {
  const cardContainer = document.querySelector('.dept-user-card-container .card-list-container');
  if (cardContainer) {
    cardContainer.removeEventListener('scroll', handleScroll);
  }
});

// 初始化数据（统一入口，避免循环）
const initData = () => {
  GetCookies().then(() => {
    // Cookie获取成功后，直接加载当前Tab数据
    if (activeTabKey.value) {
      quaryData.mySelfId = activeTabKey.value;
      GetFollows(true);
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
          // 设置默认选中第一个Tab
          if (tabList.value.length > 0 && !activeTabKey.value) {
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

        // 格式化数据 - 确保isNoFollowed有默认值
        const formattedData = newData.map((item) => ({
          ...item,
          isSaving: false,
          isEditing: item.isEditing ?? false,
          uperId: item.uperId || item.id, // 兼容旧数据，优先使用uperId字段（原userId）
          isNoFollowed: item.isNoFollowed ?? false, // 新增字段默认值为false
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

// Tab切换 - 只在用户主动切换时触发，不再调用GetCookies
const handleTabChange = (key: string) => {
  if (activeTabKey.value === key) return; // 避免重复切换同一Tab

  activeTabKey.value = key;
  quaryData.mySelfId = key;
  quaryData.pageIndex = 0;
  noMoreData.value = false;
  hasMore.value = true;
  searchInputVisible.value = false;
  quaryData.followUserName = null;

  // 直接加载当前Tab数据，不再调用GetCookies
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

const goDouyinUp = (item) => {
  window.open('https://www.douyin.com/user/' + item.secUid, '_blank', 'noopener noreferrer');
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
</style>