<script lang="ts" setup>
// 原有代码不变，此处省略（仅保留模板相关修改，脚本逻辑无变化）
import { getBase64 } from '@/utils/file';
import { FormInstance } from 'ant-design-vue';
import { reactive, ref, onMounted, UnwrapRef, watch } from 'vue';
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
  { title: '收藏路径', dataIndex: 'savePath' },
  { title: '喜欢路径', dataIndex: 'favSavePath' },
  { title: '博主路径', dataIndex: 'upSavePath' },
  { title: '图文视频', dataIndex: 'imgSavePath' },
  { title: 'Cookie', dataIndex: 'cookies' },
  { title: '有效状态', dataIndex: 'statusMsg' },
  { title: '状态', dataIndex: 'status', width: 180 },
  { title: '操作', dataIndex: 'edit', width: 350 }, // 加宽操作列宽度
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
  imgSavePath?: string;
  useSinglePath?: boolean; // 新增：是否全部用一个地址
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
  cookie.imgSavePath = undefined;
  cookie.useSinglePath = false; // 新增：默认不使用单一路径
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
watch(
  [() => form.savePath, () => form.useSinglePath],
  ([newSavePath, useSinglePath]) => {
    if (useSinglePath && newSavePath) {
      form.favSavePath = newSavePath;
      form.upSavePath = newSavePath;
      form.imgSavePath = newSavePath;
    }
  },
  { immediate: true }
);

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
            message.success('修改成功，同步任务将在5-10秒按新配置运行...');
            reset();
            GetRecords();
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
  editRecord.value = record;
  copyObject(form, record);
  // 确保useSinglePath有默认值
  if (form.useSinglePath === undefined) {
    form.useSinglePath = false;
  }
  showModal.value = true;
}

// 新增：切换同步状态方法
const switchSyncStatus = (record: DataItem) => {
  const targetStatus = record.status === 1 ? 0 : 1;
  const statusText = targetStatus === 1 ? '开启' : '停止';
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
          status: targetStatus,
        })
        .then((res) => {
          loading.value = false;
          if (res.code === 0) {
            message.success(`${statusText}同步成功`);
            GetRecords(); // 刷新列表
          } else {
            message.error(`${statusText}同步失败：${res.message || '未知错误'}`);
          }
        })
        .catch((err) => {
          loading.value = false;
          console.error('切换同步状态失败：', err);
          message.error('切换同步状态失败，请稍后重试');
        });
    },
    onCancel: () => {
      console.log(`已取消${statusText}同步`);
    },
  });
};

type Status = 0 | 1;

const StatusDict = {
  0: '同步已关闭',
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
const rowCount = 8;

// 组件挂载时获取配置
onMounted(() => {
  GetRecords();
});
const downImgVideo = ref(true);
</script>

<template>
  <a-modal :title="form._isNew ? '新增' : '编辑'" v-model:visible="showModal" @ok="submit" @cancel="cancel" width="1000px">
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
          <a-input v-model:value="form.secUserId" style="flex: 1;" placeholder="如果要同步“我喜欢”的视频和关注列表时，必填！！！" />
          <a-tooltip title="如果要同步“我喜欢”的视频和关注列表时，必填！！！">
            <ExclamationCircleOutlined style="color: #faad14;font-size: 16px;" />
          </a-tooltip>
        </div>
      </a-form-item>

      <!-- 收藏的存储路径 -->
      <a-form-item label="收藏的存储路径" name="savePath">
        <div style="display: flex; align-items: center; gap: 12px; width: 100%;">
          <a-input v-model:value="form.savePath" style="width: 200px;" />
          <a-alert message="不想同步收藏的视频就空着" type="info" size="small" style="flex: 1; margin-bottom: 0;" />
        </div>
      </a-form-item>

      <!-- 新增：是否全部用一个地址开关 -->
      <a-form-item label="统一存储路径" name="useSinglePath">
        <div style="display: flex; align-items: center; gap: 8px;">
          <a-switch v-model:checked="form.useSinglePath" :checked-value="true" :un-checked-value="false" size="default" />
          <span>{{ form.useSinglePath ? '共用收藏视频路径，如果是容器部署-docker-compose配置，只需要映射一个路径就行了' : '各类视频路径分开独立配置' }}</span>
        </div>
      </a-form-item>

      <!-- 喜欢的存储路径 -->
      <a-form-item label="喜欢的存储路径" name="favSavePath">
        <div style="display: flex; align-items: center; gap: 12px; width: 100%;">
          <a-input v-model:value="form.favSavePath" :disabled="form.useSinglePath" placeholder="" style="width: 200px;" />
          <a-alert message="不想同步喜欢的视频就空着" type="info" size="small" style="flex: 1; margin-bottom: 0;" />
        </div>
      </a-form-item>

      <!-- 关注的存储路径 -->
      <a-form-item label="关注的存储路径" name="upSavePath">
        <div style="display: flex; align-items: center; gap: 12px; width: 100%;">
          <a-input v-model:value="form.upSavePath" :disabled="form.useSinglePath" placeholder="开启统一存储路径模式后将自动同步收藏路径的值" style="width: 200px;" />
          <a-alert message="不想同步关注列表博主的视频就空着" type="info" size="small" style="flex: 1; margin-bottom: 0;" />
        </div>
      </a-form-item>

      <!-- 图文的存储路径 -->
      <a-form-item label="图文的存储路径" name="imgSavePath">
        <div style="display: flex; align-items: center; gap: 12px; width: 100%;">
          <a-input v-model:value="form.imgSavePath" :disabled="form.useSinglePath" placeholder="开启统一存储路径模式后将自动同步收藏路径的值" style="width: 200px;" />
          <a-alert message="如果系统配置页面开启了同步图文视频，且开启了单独存储，则必填！！！" type="info" size="small" style="flex: 1; margin-bottom: 0;" />
        </div>
      </a-form-item>

      <!-- 同步状态开关 -->
      <a-form-item label="同步状态" name="status">
        <div style="display: flex; align-items: center; gap: 8px;">
          <a-switch v-model:checked="form.status" :checked-value="1" :un-checked-value="0" size="default" />
          <span>{{ form.status === 1 ? '开启' : '停止' }}</span>
        </div>
      </a-form-item>
    </a-form>
  </a-modal>

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
        <a-badge class="text-subtext" :color="text === 1 ? 'green' : 'red'">
          <template #text>
            <span class="text-subtext">{{ StatusDict[text as Status] }}</span>
          </template>
        </a-badge>
      </template>
      <template v-else-if="column.dataIndex === 'cookies'">
        <a-button @click="showCookies(record)">查看</a-button>
      </template>
      <template v-else-if="column.dataIndex === 'upSecUserIds'">
        <a-button @click="showUpers(record)">查看</a-button>
      </template>
      <template v-else-if="column.dataIndex === 'edit'">
        <!-- 同步状态切换按钮 -->
        <a-button :disabled="loading" type="link" @click="switchSyncStatus(record)" :style="{ color: record.status === 1 ? '#ff4d4f' : 'green' }">
          <template #icon>
            <span v-if="record.status === 1" style="margin-right:5px;">
              <StopOutlined />
            </span>
            <span v-else style="margin-right:5px;">
              <ClockCircleOutlined />
            </span>
          </template>
          {{ record.status === 1 ? '停止同步'  : '开启同步' }}
        </a-button>

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

<style scoped>
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
/* 删除了原有的ant-alert-small和ant-alert-message自定义样式，避免覆盖官方主题 */
/* 如需调整alert间距，仅修改外层容器，不修改alert内部样式 */
.alert-wrapper {
  flex: 1;
  margin-bottom: 0 !important;
}
</style>

<!-- 新增全局样式块，确保alert样式不受scoped影响（仅针对alert还原默认） -->
<style>
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