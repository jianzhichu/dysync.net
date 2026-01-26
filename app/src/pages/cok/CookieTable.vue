<script lang="ts" setup>
import { getBase64 } from '@/utils/file';
import { FormInstance } from 'ant-design-vue';
import { reactive, ref, onMounted, UnwrapRef, watch, nextTick } from 'vue';
import dayjs from 'dayjs';
import { Dayjs } from 'dayjs';
import {
  EditFilled,
  DeleteFilled,
  SearchOutlined,
  PlusOutlined,
  ExclamationCircleOutlined,
  StopOutlined,
  ClockCircleOutlined,
  DeleteOutlined,
} from '@ant-design/icons-vue';
import { useApiStore } from '@/store';
import { message } from 'ant-design-vue';

import { StarOutlined, StarFilled, StarTwoTone } from '@ant-design/icons-vue';
const columns = ref([]);
columns.value = [
  {
    title: 'Cookie名称',
    dataIndex: 'userName',
    width: 180,
  },
  { title: 'Cookie状态', dataIndex: 'statusMsg' },
  { title: '收藏路径', dataIndex: 'savePath' },
  { title: '喜欢路径', dataIndex: 'favSavePath' },
  { title: '博主路径', dataIndex: 'upSavePath' },
  // { title: '图文视频', dataIndex: 'imgSavePath' },
  // { title: 'Cookie', dataIndex: 'cookies' },
  { title: '状态', dataIndex: 'status', width: 180 },
  { title: '操作', dataIndex: 'edit', width: 200 }, // 加宽操作列宽度
];

// 定义数组中单个对象的类型：包含uper和uid字段（可选字符串类型，根据实际需求调整是否可选）
interface UpSecUserIdItem {
  uper?: string; // 若要求必填，可去掉问号 `uper: string;`
  uid?: string; // 同上，必填则去掉问号
  syncAll: boolean;
}

type DataItem = {
  id?: string;
  userName?: string;
  cookies?: string;
  savePath?: string;
  favSavePath?: string;
  secUserId?: string;
  status?: number;
  _isNew?: boolean;
  upSecUserIdsJson?: UpSecUserIdItem[];
  upSecUserIds?: string;
  upSavePath?: string;
  // imgSavePath?: string;
  // useSinglePath?: boolean; // 新增：是否全部用一个地址
  useCollectFolder?: boolean;
  downMix?: boolean;
  downSeries?: boolean;
  mixPath?: string;
  seriesPath?: string;
  downCollect?: boolean;
  downFavorite?: boolean;
  downFollowd?: boolean;
};

const loading = ref(false);
const datas: UnwrapRef<DataItem[]> = reactive([]);
const pagination = ref({
  current: 1,
  defaultPageSize: 10,
  total: 0,
  showTotal: () => `共 ${0} 条`,
});

interface QuaryParam {
  pageIndex: number;
  pageSize: number;
}
const quaryData: UnwrapRef<QuaryParam> = reactive({
  pageIndex: 0,
  pageSize: 20,
});

const GetRecords = () => {
  loading.value = true;
  quaryData.pageIndex = pagination.value.current;
  quaryData.pageSize = pagination.value.defaultPageSize;
  useApiStore()
    .CookiePageList(quaryData)
    .then((res) => {
      loading.value = false;
      if (res.code === 0) {
        dataSource.value = res.data.data;
        pagination.value.current = res.data.pageIndex;
        pagination.value.defaultPageSize = res.data.pageSize;
        pagination.value.total = res.data.total;
        pagination.value.showTotal = () => `共 ${res.data.total} 条`;
      }
    });
};

function addNew() {
  showModal.value = true;
  form._isNew = true;
}

const showModal = ref(false);

const newCookie = (cookie?: DataItem) => {
  if (!cookie) {
    cookie = { _isNew: true };
  }
  cookie.userName = undefined;
  cookie.cookies = undefined;
  cookie.savePath = undefined;
  cookie.favSavePath = undefined;
  cookie.secUserId = undefined;
  cookie.status = 0; // 0=关闭，1=开启
  cookie.id = '0';
  cookie.upSecUserIdsJson = undefined;
  cookie.upSavePath = undefined;
  // cookie.imgSavePath = undefined;
  // cookie.useSinglePath = false; // 新增：默认不使用单一路径
  cookie.useCollectFolder = false; //是否按收藏夹来下载。
  cookie.downMix = false; //是否下载收藏夹的合集
  cookie.downSeries = false; //是否下载短剧
  cookie.mixPath = undefined; //合集存储路径
  cookie.seriesPath = undefined; //短剧存储路径
  cookie.downCollect = false;
  cookie.downFavorite = false;
  cookie.downFollowd = false;
  return cookie;
};

const copyObject = (target: any, source?: any) => {
  if (!source) {
    return target;
  }
  Object.keys(target).forEach((key) => (target[key] = source[key]));
};

const form = reactive<DataItem>(newCookie());

// 新增：监听收藏路径变化，当启用单一路径时同步到其他路径
// watch(
//   [() => form.savePath, () => form.useSinglePath],
//   ([newSavePath, useSinglePath]) => {
//     // if (useSinglePath && newSavePath) {
//     //   form.favSavePath = newSavePath;
//     //   form.upSavePath = newSavePath;
//     //   // form.imgSavePath = newSavePath;
//     // }
//   },
//   { immediate: true }
// );

function reset() {
  return newCookie(form);
}

function cancel() {
  showModal.value = false;
  reset();
}

const formModel = ref<FormInstance>();

const formLoading = ref(false);

function submit() {
  formLoading.value = true;

  formModel.value
    ?.validateFields()
    .then((resData: DataItem) => {
      if (form._isNew) {
        // authors.push({ ...res });
      } else {
        copyObject(editRecord.value, resData);
      }
      useApiStore()
        .UpdateConfig(resData)
        .then((res) => {
          loading.value = false;
          if (res.code === 0) {
            showModal.value = false;
            message.success('保存成功，同步任务将在5-10秒后重新启动...');
            reset();
            GetRecords();
          } else {
            message.error('保存失败' + res.message);
          }
        });
    })
    .catch((e) => {
      console.error(e);
    })
    .finally(() => {
      formLoading.value = false;
    });
}

const editRecord = ref<DataItem>();

import { Modal } from 'ant-design-vue';

const deleted = (id: string) => {
  Modal.confirm({
    title: '确认删除',
    content: '确定要删除这条记录吗？此操作不可撤销。',
    okText: '确认',
    cancelText: '取消',
    onOk: () => {
      useApiStore()
        .deleteCookie(id)
        .then((res) => {
          loading.value = false;
          if (res.code === 0) {
            showModal.value = false;
            reset();
            GetRecords();
          }
        });
    },
    onCancel: () => {
      console.log('已取消删除');
    },
  });
};

function edit(record: DataItem) {
  cookieId.value = record.id;
  editRecord.value = record;
  console.log(record);
  copyObject(form, record);
  // 确保useSinglePath有默认值
  // if (form.useSinglePath === undefined) {
  //   form.useSinglePath = false;
  // }
  showModal.value = true;
}

// 新增：切换同步状态方法

const switchSyncStatus = (record: DataItem) => {
  // 保存原始状态，用于弹窗取消后还原
  const statusText = record.status === 1 ? '开启' : '停止';
  const title = `确认${statusText}同步`;
  const content = `确定要${statusText}【${record.userName || '该'}】Cookie的同步任务吗？`;

  Modal.confirm({
    title,
    content,
    okText: '确认',
    cancelText: '取消',
    onOk: () => {
      loading.value = true;
      useApiStore()
        .SwitchCookieStatus({
          id: record.id,
          status: record.status,
        })
        .then((res) => {
          loading.value = false;
          if (res.code === 0) {
            message.success(`${statusText}同步成功`);
            GetRecords(); // 刷新列表，更新 switch 状态
          } else {
            message.error(`${statusText}同步失败：${res.message || '未知错误'}`);
            record.status = record.status === 1 ? 0 : 1;
          }
        })
        .catch((err) => {
          loading.value = false;
          console.error('切换同步状态失败：', err);
          message.error('切换同步状态失败，请稍后重试');
          record.status = record.status === 1 ? 0 : 1;
        });
    },
    onCancel: () => {
      console.log(`已取消${statusText}同步`);
      record.status = record.status === 1 ? 0 : 1;
    },
  });
};
const StatusDict = {
  0: '同步已停止',
  1: '同步已开启',
};

const dataSource = ref(datas);
const showCookiesModal = ref(false);
let showCookiesData = '';
const showCookies = (recode: DataItem) => {
  showCookiesModal.value = true;
  showCookiesData = recode.cookies;
};

const showUpersModal = ref(false);
let showUpersData = '';
const showUpers = (recode: DataItem) => {
  showUpersModal.value = true;
  showUpersData = recode.upSecUserIds;
};

const addRow = () => {
  if (!form.upSecUserIdsJson) {
    form.upSecUserIdsJson = [];
  }
  form.upSecUserIdsJson.push({ uper: '', uid: '', syncAll: false });
};
const removeRow = (index: number) => {
  if (form.upSecUserIdsJson) {
    form.upSecUserIdsJson.splice(index, 1);
  }
};
const rowCount = 10;

// 组件挂载时获取配置
onMounted(() => {
  GetRecords();
});

// ========== 抽屉相关响应式数据 ==========
// 抽屉显示状态
const showDrawer = ref(false);
// 抽屉类型（区分收藏夹/合集/短剧）
type DrawerType = 'collect' | 'mix' | 'series';

const cookieId = ref('');
const cateType = ref(5);

const drawerType = ref<DrawerType>('collect');
// 抽屉滚动容器引用（用于监听触底分页）
const drawerScrollRef = ref<HTMLDivElement | null>(null);
// 抽屉数据列表
const drawerDataList = ref<DrawerItem[]>([]);
// 抽屉分页配置
const drawerPagination = reactive({
  current: 1,
  pageSize: 20,
  total: 0,
  loading: false,
  hasMore: true, // 是否还有更多数据
});

// ========== 定义抽屉数据项接口 ==========
interface DrawerItem {
  id: string; // 不显示
  name: string; // 名称
  saveFolder: string; // 保存文件夹
  sync: boolean; // 是否同步
  coverUrl: string; // 封面
  cookieId: string; // 不显示
  xId: string; // 不显示
  total: number;
}

// ========== 3个打开抽屉的方法 ==========
const openCollectFolderSetModal = () => {
  drawerType.value = 'collect';
  cateType.value = 5;
  openCommonDrawer();
};

const openMixDownSetModal = () => {
  drawerType.value = 'mix';
  cateType.value = 6;
  openCommonDrawer();
};

const openSeriesDownSetModal = () => {
  drawerType.value = 'series';
  cateType.value = 7;
  openCommonDrawer();
};

// ========== 公用抽屉打开逻辑 ==========
const openCommonDrawer = () => {
  // 重置抽屉状态
  drawerDataList.value = [];
  drawerPagination.current = 1;
  drawerPagination.total = 0;
  drawerPagination.hasMore = true;
  // 显示抽屉
  showDrawer.value = true;
  // 加载第一页数据
  loadDrawerData();
  // 监听滚动触底（延迟绑定，确保DOM已渲染）
  nextTick(() => {
    bindDrawerScrollEvent();
  });
};

// ========== 绑定抽屉滚动触底事件 ==========
const bindDrawerScrollEvent = () => {
  const scrollContainer = drawerScrollRef.value;
  if (!scrollContainer) return;

  // 先移除原有事件，避免重复绑定
  scrollContainer.removeEventListener('scroll', handleDrawerScroll);
  // 添加滚动事件
  scrollContainer.addEventListener('scroll', handleDrawerScroll);
};

// ========== 滚动触底处理逻辑 ==========
const handleDrawerScroll = () => {
  const scrollContainer = drawerScrollRef.value;
  if (!scrollContainer || drawerPagination.loading || !drawerPagination.hasMore) return;

  // 计算滚动位置（触底阈值20px，避免精准触底才触发）
  const { scrollTop, clientHeight, scrollHeight } = scrollContainer;
  const isBottom = scrollTop + clientHeight >= scrollHeight - 20;

  if (isBottom) {
    // 加载下一页
    drawerPagination.current += 1;
    loadDrawerData();
  }
};

// ========== 加载抽屉数据（
const loadDrawerData = () => {
  if (drawerPagination.loading || !drawerPagination.hasMore) return;

  drawerPagination.loading = true;
  useApiStore()
    .CatePageList({
      cookieId: cookieId.value,
      cateType: cateType.value,
    })
    .then((res) => {
      if (res.code === 0) {
        drawerDataList.value = [...drawerDataList.value, ...res.data.data];
        drawerPagination.loading = false;
        // 更新分页状态
        drawerPagination.total = res.data.total;
        drawerPagination.loading = false;
        // 判断是否还有更多数据
        drawerPagination.hasMore = drawerDataList.value.length < drawerPagination.total;
      } else {
        message.error(res.message);
      }
    })
    .finally(() => {
      drawerPagination.loading = false;
    });
};

// ========== 获取抽屉类型名称（用于页面展示） ==========
const getDrawerTypeName = () => {
  switch (drawerType.value) {
    case 'collect':
      return '收藏夹';
    case 'mix':
      return '合集';
    case 'series':
      return '短剧';
    default:
      return '';
  }
};

// ========== 切换同步状态（可根据需求对接真实接口） ==========
const toggleDrawerItemSync = (item: DrawerItem, index: number) => {
  if (drawerDataList.value[index]) {
    drawerDataList.value[index].sync = !item.sync;
    console.log(`切换${getDrawerTypeName()}【${item.name}】的同步状态为：${!item.sync}`);
  }
};
// ========== 关闭抽屉清理资源 ==========
const closeDrawer = () => {
  showDrawer.value = false;
  // 移除滚动事件监听
  const scrollContainer = drawerScrollRef.value;
  if (scrollContainer) {
    scrollContainer.removeEventListener('scroll', handleDrawerScroll);
  }
};

// 2. 新增：抽屉右上角保存按钮的点击方法（仅抽屉相关，不影响其他逻辑）
const saveDrawerData = () => {
  if (drawerDataList.value.length === 0) {
    message.info('暂无需要保存的配置数据');
    return;
  }
  drawerPagination.loading = true;
  useApiStore()
    .BatchSaveCate(drawerDataList.value)
    .then((res) => {
      if (res.code === 0) {
        showDrawer.value = false;
        message.success('保存成功');
      } else {
        message.error(res.message);
      }
    })
    .finally(() => {
      drawerPagination.loading = false;
    });
};

const switchdownCollect = (e: any) => {
  if (!e) form.useCollectFolder = e;
};
</script>

<template>
  <a-modal :title="form._isNew ? '新增' : '编辑'" v-model:visible="showModal" @ok="submit" @cancel="cancel" width="100%" wrap-class-name="full-modal">
    <a-form ref="formModel" :model="form" :labelCol="{ span: 3 }" :wrapperCol="{ span: 20 }">
      <a-form-item label="Cookie名称" required name="userName">
        <a-input v-model:value="form.userName" />
      </a-form-item>
      <a-form-item label="id" required name="id" v-show="false">
        <a-input v-model:value="form.id" />
      </a-form-item>
      <a-form-item label="Cookie值" name="cookies">
        <a-textarea v-model:value="form.cookies" :rows="rowCount" />
      </a-form-item>

      <a-form-item label="我的secUserId" name="secUserId">
        <div style="display: flex; align-items: center; gap: 6px;">
          <a-input v-model:value="form.secUserId" style="flex: 1;" placeholder="" />
          <a-tooltip title="如果要同步“我喜欢”的视频和关注列表时，必填！！！">
            <ExclamationCircleOutlined style="color: #faad14;font-size: 16px;" />
          </a-tooltip>
        </div>
      </a-form-item>

      <!-- 修复：下载收藏视频 - 拆分Form.Item，用a-form-item-rest包裹非收集字段 -->
      <a-form-item label="下载收藏视频" name="downCollect">
        <div style="display: flex; align-items: center; gap: 12px;">
          <div class="form-item-div">
            <a-switch v-model:checked="form.downCollect" @change="switchdownCollect" :checked-value="true" :un-checked-value="false" size="default" />
            <!-- 非收集字段用a-form-item-rest包裹 -->
            <a-form-item-rest v-if="form.downCollect">
              <a-form-item name="savePath" noStyle>
                <a-input v-model:value="form.savePath" placeholder='请输入容器路径' class="form-item-div-input" />
              </a-form-item>
            </a-form-item-rest>
          </div>
          <a-alert message="开启后自动下载默认收藏夹视频，记得填写映射路径（容器内部路径）" type="info" size="small" style="flex: 1; margin-bottom: 0;" />
        </div>
      </a-form-item>

      <!-- 修复：自定义收藏夹 - 拆分Form.Item -->
      <a-form-item v-if="form.savePath && form.savePath.length>0" label="自定义收藏夹" name="useCollectFolder">
        <div style="display: flex; align-items: center; gap: 12px;">
          <div class="form-item-div">
            <a-switch v-model:checked="form.useCollectFolder" :checked-value="true" :un-checked-value="false" size="default" />
            <a-form-item-rest v-if="form.useCollectFolder">
              <a-input v-model:value="form.savePath" :disabled="form.useCollectFolder&&form.downCollect" placeholder="" class="form-item-div-input" />
              <a-button @click="openCollectFolderSetModal" shape="circle" type="dashed" style="margin-left:5px;" v-if="form.useCollectFolder">
                <star-outlined />
              </a-button>
            </a-form-item-rest>
          </div>
          <a-alert message="开启后自动下载自定义分类后的收藏夹，开启后不在下载默认收藏夹视频，存储路径与默认收藏夹存储路径一致" :type="form.useCollectFolder?'error':'info'" size="small" style="flex: 1; margin-bottom: 0;" />
        </div>
      </a-form-item>

      <!-- 修复：下载喜欢视频 - 拆分Form.Item -->
      <a-form-item label="下载喜欢视频" name="downFavorite">
        <div style="display: flex; align-items: center; gap: 12px;">
          <div class="form-item-div">
            <a-switch v-model:checked="form.downFavorite" :checked-value="true" :un-checked-value="false" size="default" />
            <a-form-item-rest v-if="form.downFavorite">
              <a-form-item name="favSavePath" noStyle>
                <a-input v-model:value="form.favSavePath" placeholder='请输入容器路径' class="form-item-div-input" />
              </a-form-item>
              <a-button shape="circle" @click="()=>{message.success('别点了，这只是为了好看的😄')}" type="dashed" style="margin-left:5px;">
                <like-outlined />
              </a-button>
            </a-form-item-rest>
          </div>
          <a-alert message="开启后自动下载喜欢（点赞）的视频，记得填写映射路径（容器内部路径）" type="info" size="small" style="flex: 1; margin-bottom: 0;" />
        </div>
      </a-form-item>

      <!-- 修复：下载关注视频 - 拆分Form.Item -->
      <a-form-item label="下载关注视频" name="downFollowd">
        <div style="display: flex; align-items: center; gap: 12px;">
          <div class="form-item-div">
            <a-switch v-model:checked="form.downFollowd" :checked-value="true" :un-checked-value="false" size="default" />
            <a-form-item-rest v-if="form.downFollowd">
              <a-form-item name="upSavePath" noStyle>
                <a-input v-model:value="form.upSavePath" placeholder='请输入容器路径' class="form-item-div-input" />
              </a-form-item>
              <a-button shape="circle" @click="()=>{message.success('别点了，这只是为了好看的😄')}" type="dashed" style="margin-left:5px;">
                <heart-outlined />
              </a-button>
            </a-form-item-rest>
          </div>
          <a-alert message="开启后自动下载关注的博主视频，记得填写映射路径（容器内部路径）" type="info" size="small" style="flex: 1; margin-bottom: 0;" />
        </div>
      </a-form-item>

      <!-- 修复：下载合集视频 - 拆分Form.Item -->
      <a-form-item label="下载合集视频" name="downMix">
        <div style="display: flex; align-items: center; gap: 12px;">
          <div class="form-item-div">
            <a-switch v-model:checked="form.downMix" :checked-value="true" :un-checked-value="false" size="default" />
            <a-form-item-rest v-if="form.downMix">
              <a-form-item name="mixPath" noStyle>
                <a-input v-model:value="form.mixPath" class="form-item-div-input" placeholder='默认使用收藏夹路径' />
              </a-form-item>
              <a-button @click="openMixDownSetModal" shape="circle" type="dashed" style="margin-left:5px;">
                <gift-outlined />
              </a-button>
            </a-form-item-rest>
          </div>
          <a-alert message="开启后自动下载收藏的合集视频（还需要开启合集同步开关，不填目录径默认存储到收藏目录，设置后记得docker里面加映射，注意：付费视频下载后无法播放）" :type="form.downMix?'error':'info'" size="small" style="flex: 1; margin-bottom: 0;" />
        </div>
      </a-form-item>

      <!-- 修复：下载短剧视频 - 拆分Form.Item -->
      <a-form-item label="下载短剧视频" name="downSeries">
        <div style="display: flex; align-items: center; gap: 12px;">
          <div class="form-item-div">
            <a-switch v-model:checked="form.downSeries" :checked-value="true" :un-checked-value="false" size="default" />
            <a-form-item-rest v-if="form.downSeries">
              <a-form-item name="seriesPath" noStyle>
                <a-input v-model:value="form.seriesPath" placeholder='默认使用收藏夹路径' class="form-item-div-input" />
              </a-form-item>
              <a-button @click="openSeriesDownSetModal" shape="circle" type="dashed" style="margin-left:5px;">
                <fire-outlined />
              </a-button>
            </a-form-item-rest>
          </div>
          <a-alert message="开启后自动下载收藏的短剧视频（还需要开启短剧同步开关，不填目录径默认存储到收藏目录，设置后记得docker里面加映射，注意：付费视频下载后无法播放）
" :type="form.downSeries?'error':'info'" size="small" style="flex: 1; margin-bottom: 0;" />
        </div>
      </a-form-item>

      <!-- 任务同步状态 - 保持原有结构（仅一个Switch，无警告） -->
      <a-form-item label="任务同步状态" name="status">
        <div style="display: flex; align-items: center; gap: 8px;">
          <a-switch v-model:checked="form.status" :checked-value="1" :un-checked-value="0" size="default" />
          <span>{{ form.status === 1 ? '' : '' }}</span>
        </div>
      </a-form-item>
    </a-form>
  </a-modal>

  <!-- 公用抽屉组件（层级高于原有弹窗）- 优化后 -->
  <a-drawer :title="getDrawerTypeName()+'配置'" v-model:visible="showDrawer" placement="right" width="800px" :z-index="10010" :mask-z-index="10009" @close="closeDrawer" class="common-drawer">
    <!-- 抽屉右上角：添加保存按钮（利用 extra 插槽，不改动其他模板） -->
    <template #extra>
      <a-button type="primary" :loading="drawerPagination.loading" @click="saveDrawerData" class="drawer-save-btn">
        <template #icon>
          <SaveOutlined />
        </template>
        保存
      </a-button>
    </template>

    <!-- 抽屉滚动容器（绑定ref和滚动样式） -->
    <div ref="drawerScrollRef" class="drawer-scroll-container">
      <!-- 加载中状态 -->
      <div v-if="drawerPagination.loading && drawerDataList.length === 0" class="drawer-loading">
        <Spin size="large" />
      </div>

      <!-- 卡片网格展示 - 优化布局（横向一行展示名称、保存路径） -->
      <a-card v-else :bordered="false" class="drawer-card-container grid-container">
        <a-card-grid v-for="(item, index) in drawerDataList" :key="item.id" class="drawer-card-grid">
          <!-- 竖向封面（移除预览功能，仅保留基础展示） -->
          <div class="grid-cover vertical-cover" v-if="drawerType!='collect'">
            <a-image :preview="false" :src="item.coverUrl" fit="cover" />
          </div>

          <!-- 名称：横向一行展示（标签 + 内容 在同一行） -->
          <div class="grid-item horizontal-item name">
            <label class="drawer-label">名称：</label>
            <span>{{ item.name || '未命名' }}</span>
          </div>

          <!-- 保存文件夹：横向一行展示（标签 + 输入框 在同一行） -->
          <div class="grid-item horizontal-item save-folder">
            <label class="drawer-label">保存：</label>
            <a-input v-model:value="item.saveFolder" size="small" placeholder="默认用名称作文件夹" class="save-path-input" />
          </div>
          <div class="grid-item horizontal-item name">
            <label class="drawer-label">集数：</label>
            <span>{{ item.total || '0' }}</span>
          </div>

          <!-- 同步开关（保持原有逻辑，横向对齐） -->
          <div class="grid-item horizontal-item sync-switch">
            <label class="drawer-label">同步：</label>
            <a-switch :checked="item.sync" @change="() => toggleDrawerItemSync(item, index)" size="small" />
          </div>
        </a-card-grid>
      </a-card>

      <!-- 无更多数据提示 -->
      <div v-if="!drawerPagination.hasMore && drawerDataList.length > 0" class="no-more-data">
        <!-- 已加载全部{{ getDrawerTypeName() }}数据 -->
      </div>

      <!-- 加载下一页中 -->
      <div v-if="drawerPagination.loading && drawerDataList.length > 0" class="loading-more">
        <Spin size="small" />
        <span>加载中...</span>
      </div>
    </div>
  </a-drawer>
  <!-- 成员表格 -->
  <a-table v-bind="$attrs" :columns="columns" :dataSource="dataSource" :pagination="false">
    <template #title>
      <div class="flex justify-end pr-4">
        <a-button type="primary" @click="GetRecords()" :loading="formLoading" class="mr-2">
          <template #icon>
            <SearchOutlined />
          </template>
          查询
        </a-button>

        <a-button type="primary" @click="addNew" :loading="formLoading">
          <template #icon>
            <PlusOutlined />
          </template>
          新增
        </a-button>
      </div>
    </template>
    <template #bodyCell="{ column, text, record }">
      <template v-if="column.dataIndex === 'status'">
        <!-- 新：a-switch 显示状态，绑定切换逻辑 -->
        <div style="display: flex; align-items: center; gap: 8px;">
          <a-switch v-model:checked="record.status" :checked-value="1" :un-checked-value="0" size="small" :disabled="loading" @change="() => switchSyncStatus(record)" />
          <span :style="{
    fontSize: '12px',
    color: record.status === 1 ? '#52c41a' : '#ff4d4f'
  }">
            {{ StatusDict[record.status] }}
          </span>
        </div>
      </template>
      <template v-else-if="column.dataIndex === 'cookies'">
        <a-button @click="showCookies(record)">查看</a-button>
      </template>
      <template v-else-if="column.dataIndex === 'upSecUserIds'">
        <a-button @click="showUpers(record)">查看</a-button>
      </template>
      <template v-else-if="column.dataIndex === 'edit'">
        <!-- 已删除「开启/停止」按钮，仅保留编辑和删除 -->
        <a-button :disabled="showModal || loading" type="link" @click="edit(record)">
          <template #icon>
            <EditFilled />
          </template>
          编辑
        </a-button>

        <a-button :disabled="loading" type="link" @click="deleted(record.id)" danger>
          <template #icon>
            <DeleteOutlined />
          </template>
          删除
        </a-button>
      </template>
      <div v-else class="text-subtext">
        {{ text }}
      </div>
    </template>
  </a-table>

  <!-- Cookie详情弹窗 -->
  <a-modal title="Cookie 详情" :visible="showCookiesModal" style="width:1200px;" @cancel="showCookiesModal = false" @ok="showCookiesModal = false">
    <div class="cookie-content">
      {{ showCookiesData || '无Cookie数据' }}
    </div>
  </a-modal>

  <!-- 博主信息弹窗 -->
  <a-modal title="要同步的博主信息" :visible="showUpersModal" style="width:1200px;" @cancel="showUpersModal = false" @ok="showUpersModal = false">
    <div class="cookie-content">
      {{ showUpersData || '未设置' }}
    </div>
  </a-modal>
</template>
<style scoped lang="less">
// 原有非抽屉样式（保留，不改动）
.cookie-content {
  white-space: pre-wrap;
  word-break: break-all;
  max-height: 400px;
  overflow-y: auto;
  padding: 10px;
  box-sizing: border-box;
}
.ant-form-item {
  margin-bottom: 10px;
}
/* 透明滚动条样式 - WebKit内核浏览器 */
.cookie-content::-webkit-scrollbar {
  width: 6px; /* 更细的滚动条 */
}
.cookie-content::-webkit-scrollbar-track {
  background: transparent;
}
.cookie-content::-webkit-scrollbar-thumb {
  background: rgba(150, 150, 150, 0.2); /* 浅灰透明 */
  border-radius: 3px;
}
.cookie-content::-webkit-scrollbar-thumb:hover {
  background: rgba(150, 150, 150, 0.4); /* 略深一点的透明 */
}
.cookie-content::-webkit-scrollbar-corner {
  background: transparent;
}
.form-item-div {
  width: 300px;
}
.form-item-div-input {
  width: 180px;
  margin-left: 10px;
}
/* Firefox 透明滚动条适配 */
.cookie-content {
  scrollbar-width: thin;
  scrollbar-color: rgba(150, 150, 150, 0.2) transparent;
}

/* a-textarea 透明滚动条 */
:deep(.ant-input-textarea-input) {
  overflow-y: auto;
  scrollbar-width: thin;
  scrollbar-color: rgba(150, 150, 150, 0.2) transparent;
}
:deep(.ant-input-textarea-input)::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}
:deep(.ant-input-textarea-input)::-webkit-scrollbar-track {
  background: transparent;
}
:deep(.ant-input-textarea-input)::-webkit-scrollbar-thumb {
  background: rgba(150, 150, 150, 0.2);
  border-radius: 3px;
}
:deep(.ant-input-textarea-input)::-webkit-scrollbar-thumb:hover {
  background: rgba(150, 150, 150, 0.4);
}
:deep(.ant-input-textarea-input)::-webkit-scrollbar-corner {
  background: transparent;
}

/* 禁用状态的输入框样式优化 */
:deep(.ant-input-disabled) {
  background-color: #f5f5f5 !important;
  color: #666 !important;
}

/* 修复：移除对alert的自定义样式修改，还原官方默认样式 */
.alert-wrapper {
  flex: 1;
  margin-bottom: 0 !important;
}
// ========== 精准匹配模板：a-card-grid 样式优化 ==========
/* 父容器：解决卡片间隙，避免换行（不改动模板，仅样式控制） */
.drawer-card-container.grid-container {
  // 清除 a-card 自带内边距，避免与卡片间隙冲突
  :deep(.ant-card-body) {
    padding: 16px;
    margin: 0;
  }
  // 启用 Flex 布局，实现卡片均匀间隙（核心：gap 不与组件自带样式冲突）
  display: flex;
  flex-wrap: wrap;
  gap: 20px; /* 卡片之间的横向/纵向间隙（可自定义，不会换行） */
  box-sizing: border-box;
  /* 固定左对齐：单元素/多元素都保持从左往右排列 */
  justify-content: flex-start;
  /* 新增：兜底最小高度，避免空容器塌陷 */
  min-height: 200px;
  /* 新增：左内边距，让单元素和多元素视觉对齐一致 */
  padding-left: 5px;
}

/* 新增：单元素样式修复 - 关键修复（仅防变形，不居中） */
.drawer-card-container.grid-container:has(:only-child) :deep(.drawer-card-grid) {
  /* 单元素时保持3列布局的宽度，不拉伸，防止变形 */
  width: calc(33.333% - 10.333px) !important;
  /* 可选：如果觉得单元素太窄，可取消下面注释，设置固定宽度 */
  /* width: 300px !important; */
  min-width: 200px; /* 强制最小宽度，防止压缩变形 */
}

/* a-card-grid 核心样式：圆角 + 固定宽度 + 内边距 */
:deep(.drawer-card-grid) {
  // 精确宽度计算：3列均分，减去 gap 分摊值，避免换行
  width: calc(33.333% - 10.333px) !important; /* 20px gap 分摊到3列，每列减 ~13.333px */
  /* 新增：最小宽度限制，防止变形（核心） */
  min-width: 200px;
  margin: 5px !important; /* 清除组件自带 margin，避免间隙重复 */
  border-radius: 12px !important; /* 卡片圆角（可自定义大小） */
  padding: 16px !important; /* 卡片内部整体内边距 */
  box-sizing: border-box;
  border: 1px solid #f0f0f0; /* 卡片边框，增强圆角视觉感 */
  // 卡片内部竖向排列，确保每个内容块独占一行
  display: flex;
  flex-direction: column;
  gap: 12px; /* 卡片内部元素之间的间距（可自定义） */
}

/* 图片容器：横向铺满卡片 + 自带内边距 */
:deep(.drawer-card-grid .grid-cover.vertical-cover) {
  width: 100% !important; /* 横向铺满卡片 */
  padding: 4px; /* 图片内部内边距（可自定义，实现图片与卡片边框的间距） */
  box-sizing: border-box;
  border-radius: 8px; /* 图片容器轻微圆角，与卡片圆角呼应 */
  overflow: hidden;
}

/* 图片：强制铺满容器，不拉伸 */
:deep(.drawer-card-grid .vertical-cover .ant-image) {
  width: 100% !important;
  height: auto !important; /* 高度自适应，保持图片比例 */
  display: block;
}
:deep(.drawer-card-grid .vertical-cover .ant-image-img) {
  width: 100% !important;
  height: 100% !important;
  object-fit: cover !important; /* 保持图片比例，裁剪多余部分 */
}

/* 名称、保存路径、同步开关：独占一行样式 */
:deep(.drawer-card-grid .grid-item.horizontal-item) {
  width: 100% !important; /* 独占一行，横向铺满卡片 */
  display: flex;
  align-items: center; /* 标签与内容垂直居中 */
  margin: 0 !important; /* 清除多余间距，确保独占一行 */
  padding: 4px 0; /* 上下轻微内边距，增强可读性 */
}

// 亮色模式（保留）
:deep(.drawer-label) {
  flex-shrink: 0;
  font-size: 12px;
  color: rgba(0, 0, 0, 0.6);
}

/* 名称内容、输入框：占满剩余宽度 */
:deep(.drawer-card-grid .horizontal-item span) {
  flex: 1;
  font-size: 12px;
  color: rgba(0, 0, 0, 0.88);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
:deep(.drawer-card-grid .horizontal-item .save-path-input) {
  flex: 1;
  width: 100% !important;
  font-size: 12px;
}

/* 同步开关：两端对齐，保持独占一行 */
:deep(.drawer-card-grid .sync-switch) {
  justify-content: space-between; /* 标签左对齐，开关右对齐 */
}

/* 响应式适配：小屏幕自动调整列数，保持间隙不换行 */
@media (max-width: 768px) {
  :deep(.drawer-card-grid) {
    width: calc(50% - 10px) !important; /* 2列均分，20px gap 每列减10px */
    /* 新增：响应式最小宽度 */
    min-width: 180px;
  }

  /* 响应式下单元素适配（保持左对齐） */
  .drawer-card-container.grid-container:has(:only-child) :deep(.drawer-card-grid) {
    width: calc(50% - 10px) !important;
    min-width: 180px;
  }
}
@media (max-width: 480px) {
  :deep(.drawer-card-grid) {
    width: 100% !important; /* 1列铺满，无间距分摊 */
    /* 新增：移动端最小宽度铺满 */
    min-width: 100%;
  }

  /* 移动端单元素适配（1列铺满） */
  .drawer-card-container.grid-container:has(:only-child) :deep(.drawer-card-grid) {
    width: 100% !important;
    min-width: 100%;
  }
}

// ========== 黑暗模式样式（html.dark-mode + 链式类名，光效增强 + hover 效果） ==========
/* 卡片：深色背景 + 隐式边框 + 增强光效（默认状态） */
html.dark-mode .drawer-card-container.grid-container .drawer-card-grid {
  border-color: rgba(142, 140, 140, 0.1) !important; /* 边框透明化（隐掉） */
  background-color: #1a1a2e !important;
  /* 增强光效：双层阴影+浅蓝紫微光，更醒目且不刺眼（核心修改） */
  box-shadow: 0 6px 16px rgba(0, 20, 60, 0.4),
    /* 主阴影：加深蓝紫调，增大模糊半径，光效更明显 */ 0 2px 6px rgba(100, 120, 255, 0.2),
    /* 辅阴影：新增浅蓝紫微光，光效更突出 */ inset 0 1px 0 rgba(255, 255, 255, 0.05) !important; /* 内阴影：轻微高光，增强卡片质感 */
  /* 可选：完全隐藏边框 */
  /* border: none !important; */
  transition: all 0.3s ease-in-out !important; /* 过渡动画：hover 效果平滑切换，不生硬 */
}

/* 卡片 hover 效果：悬浮时光效更突出，层次感拉满 */
html.dark-mode .drawer-card-container.grid-container .drawer-card-grid:hover {
  box-shadow: 0 8px 24px rgba(0, 30, 80, 0.5),
    /* 主阴影：进一步加深、扩大，立体感更强 */ 0 4px 12px rgba(120, 140, 255, 0.35),
    /* 辅阴影：蓝紫微光更亮，光效更醒目 */ inset 0 1px 0 rgba(255, 255, 255, 0.08) !important; /* 内阴影：高光增强，质感更优 */
  transform: translateY(-2px) !important; /* 可选：轻微上浮，增强交互感（可删除） */
}

/* label：强制纯白色（核心需求，格式不变） */
html.dark-mode .drawer-card-container.grid-container .drawer-card-grid .horizontal-item label {
  color: #ffffff !important;
}

/* 名称内容：浅白色（格式不变） */
html.dark-mode .drawer-card-container.grid-container .drawer-card-grid .horizontal-item span {
  color: rgba(255, 255, 255, 0.88) !important;
}

/* 输入框：深色适配（格式不变） */
html.dark-mode .drawer-card-container.grid-container .drawer-card-grid .ant-input {
  background-color: #2f2f2f !important;
  border-color: #404040 !important;
  color: rgba(255, 255, 255, 0.88) !important;
}
</style>

<!-- 新增全局样式块，确保alert样式不受scoped影响（仅针对alert还原默认） -->
<style lang="less">
.full-modal {
  .ant-modal {
    max-width: 100%;
    top: 0;
    padding-bottom: 0;
    margin: 0;
  }
  .ant-modal-content {
    display: flex;
    flex-direction: column;
    height: calc(100vh);
  }
  .ant-modal-body {
    flex: 1;
  }
}

/* 还原Ant Design Vue alert官方默认样式，消除scoped样式的间接影响 */
.ant-alert {
  box-sizing: border-box;
  margin: 0;
  /* padding: 16px 16px; */
  color: rgba(0, 0, 0, 0.88);
  font-size: 14px;
  line-height: 1.5714285714;
  list-style: none;
  position: relative;
  display: flex;
  align-items: center;
  border-radius: 8px;
}
.ant-alert-small {
  padding: 8px 16px;
  font-size: 12px;
  border-radius: 4px;
}
/* 还原alert不同类型的官方默认颜色 */
.ant-alert-info {
  background-color: #e6f4ff;
  border: 1px solid #91caff;
}
.ant-alert-info .ant-alert-message {
  color: #1677ff;
}
/* 还原alert不同类型的官方默认颜色 */
.ant-alert-success {
  background-color: #daf1d3;
  border: 1px solid #5bbc51;
}
.ant-alert-success .ant-alert-message {
  color: #228b22;
}

/* 还原 Alert 各类型的官方默认颜色 */
/* 错误（error）类型 - 红色系 */
.ant-alert-error {
  background-color: #fff1f0; /* 错误类型背景色 */
  border: 1px solid #ff4d4f; /* 错误类型边框色 */
}
.ant-alert-error .ant-alert-message {
  color: #f5222d; /* 错误类型文字主色 */
}

.ant-alert-warning {
  background-color: #fffbe6;
  border: 1px solid #ffe58f;
}
.ant-alert-warning .ant-alert-message {
  color: #faad14;
}
/* 确保alert图标颜色同步官方样式 */
.ant-alert-info .ant-alert-icon {
  color: #1677ff;
}
.ant-alert-warning .ant-alert-icon {
  color: #faad14;
}
</style>