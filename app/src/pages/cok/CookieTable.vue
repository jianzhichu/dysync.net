<script lang="ts" setup>
import { getBase64 } from '@/utils/file';
import { FormInstance } from 'ant-design-vue';
import { reactive, ref, onMounted } from 'vue';
import dayjs from 'dayjs';
import { Dayjs } from 'dayjs';
import { EditFilled } from '@ant-design/icons-vue';
import { useApiStore } from '@/store';
import type { UnwrapRef } from 'vue';

import { StarOutlined, StarFilled, StarTwoTone } from '@ant-design/icons-vue';
const columns = [
  {
    title: 'Cookie名',
    dataIndex: 'userName',
    width: 100,
  },
  { title: '状态', dataIndex: 'status', width: 80 },
  { title: '收藏路径', dataIndex: 'savePath', width: 100 },
  { title: '喜欢路径', dataIndex: 'favSavePath', width: 100 },
  { title: 'Up主路径', dataIndex: 'upSavePath', width: 100 },
  // { title: 'Cookie', dataIndex: 'cookies' },
  { title: 'Up主secUserId', dataIndex: 'upSecUserIds' },
  { title: '用户secUserId', dataIndex: 'secUserId' },
  { title: '操作', dataIndex: 'edit', width: 200 },
  // { title: 'id', dataIndex: 'id', width: 200, hiden: false },
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
};

// const dataSource = reactive<DataItem[]>([
//   {
//     userName: 'Li Zhi',
//     cookies: '131231',
//     savePath: 'x',
//     status: 1,
//     id: '1',
//   },
// ]);

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
onMounted(() => {
  GetRecords();
});

function addNew() {
  showModal.value = true;
  form._isNew = true;
}

const showModal = ref(false);

const newAuthor = (author?: DataItem) => {
  if (!author) {
    author = { _isNew: true };
  }
  author.userName = undefined;
  author.cookies = undefined;
  author.savePath = undefined;
  author.favSavePath = undefined;
  author.secUserId = undefined;
  author.status = 0;
  author.id = '0';
  author.upSecUserIdsJson = undefined;
  author.upSavePath = undefined;
  return author;
};

const copyObject = (target: any, source?: any) => {
  if (!source) {
    return target;
  }
  Object.keys(target).forEach((key) => (target[key] = source[key]));
};

const form = reactive<DataItem>(newAuthor());

function reset() {
  return newAuthor(form);
}

function cancel() {
  showModal.value = false;
  reset();
}

const formModel = ref<FormInstance>();

const formLoading = ref(false);

function submit() {
  formLoading.value = true;
  let self = this;

  formModel.value
    ?.validateFields()
    .then((resData: DataItem) => {
      if (form._isNew) {
        // authors.push({ ...res });
      } else {
        copyObject(editRecord.value, resData);
      }
      // alert(JSON.stringify(form.upSecUserIdsJson));
      // console.log('1', authors);
      // console.log('2', res);
      useApiStore()
        .UpdateConfig(resData)
        .then((res) => {
          loading.value = false;
          if (res.code === 0) {
            showModal.value = false;
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

import { Modal } from 'ant-design-vue'; // 假设使用Ant Design Vue的Modal组件

const deleted = (id: string) => {
  // 显示确认对话框
  Modal.confirm({
    title: '确认删除',
    content: '确定要删除这条记录吗？此操作不可撤销。',
    okText: '确认',
    cancelText: '取消',
    onOk: () => {
      // 用户确认后执行删除操作
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
      // 用户取消删除，不执行任何操作
      console.log('已取消删除');
    },
  });
};
/**
 * 编辑
 * @param record
 */
function edit(record: DataItem) {
  editRecord.value = record;
  copyObject(form, record);
  showModal.value = true;
}

type Status = 0 | 1;

const StatusDict = {
  0: '关闭',
  1: '开启',
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
  form.upSecUserIdsJson.push({ uper: '', uid: '', syncAll: false });
};
const removeRow = (index) => {
  form.upSecUserIdsJson.splice(index, 1);
};
const rowCount = 4;
</script>
<template>
  <a-modal :title="form._isNew ? '新增Cookie' : '编辑Cookie'" v-model:visible="showModal" @ok="submit" @cancel="cancel" width="1000px">
    <a-form ref="formModel" :model="form" :labelCol="{ span: 3 }" :wrapperCol="{ span: 20 }">
      <a-form-item label="Cookie别名" required name="userName">
        <a-input v-model:value="form.userName" />
      </a-form-item>
      <a-form-item label="id" required name="id" v-show="false">
        <a-input v-model:value="form.id" />
      </a-form-item>
      <a-form-item label="收藏的存储路径" name="savePath">
        <a-input v-model:value="form.savePath" />
      </a-form-item>

      <a-form-item label="Cookie" name="cookies">
        <a-textarea v-model:value="form.cookies" :rows='rowCount' />
      </a-form-item>
      <a-form-item label="喜欢的存储路径" name="favSavePath">
        <div style="display: flex; align-items: center; gap: 6px;">
          <a-input v-model:value="form.favSavePath" style="flex: 1;" />
          <a-tooltip title="同步“我喜欢的”视频时，必填！！！">
            <ExclamationCircleOutlined style="color: #faad14;font-size: 16px;" />
          </a-tooltip>
        </div>
      </a-form-item>
      <a-form-item label="SecUserId" name="secUserId">
        <div style="display: flex; align-items: center; gap: 6px;">
          <a-input v-model:value="form.secUserId" style="flex: 1;" />
          <a-tooltip title="同步“我喜欢的”视频时，必填！！！">
            <ExclamationCircleOutlined style="color: #faad14;font-size: 16px;" />
          </a-tooltip>
        </div>
      </a-form-item>
      <a-form-item label="Up主视频存储路径" name="upSavePath">
        <div style="display: flex; align-items: center; gap: 6px;">
          <a-input v-model:value="form.upSavePath" style="flex: 1;" />
          <a-tooltip title="同步指定博主视频时必填！！！">
            <ExclamationCircleOutlined style="color: #faad14;font-size: 16px;" />
          </a-tooltip>
        </div>
      </a-form-item>

      <a-form-item label="Uper主secUserIds" name="upSecUserIdsJson">
        <a-form-item-rest> <!-- 用这个包裹所有辅助元素 -->
          <!-- 添加行按钮 -->
          <a-button type="primary" @click="addRow" style="margin-bottom: 12px">
            添加抖音Up主
            <template #icon>
              <PlusOutlined />
            </template>
          </a-button>

          <!-- 动态渲染所有行 -->
          <div v-for="(row, index) in form.upSecUserIdsJson" :key="index" style="display: flex; gap: 12px; margin-bottom: 8px; align-items: center">
            <!-- uper输入框 -->
            <a-input v-model:value="row.uper" placeholder="博主别名，可自定义" style="flex: 1" />

            <!-- uid输入框 -->
            <a-input v-model:value="row.uid" placeholder="博主secUserId" style="flex: 3" />

            <!-- syncAll 开关字段 -->
            <div style="flex: 1; display: flex; align-items: center;">
              <a-tooltip title="默认关闭，仅同步 UP 主最新一页数据；开启将同步全部作品（量大不建议开启）">
                <span style="margin-right: 8px; cursor: default;color:#faad14">同步全部作品</span>
              </a-tooltip>
              <a-switch v-model:checked="row.syncAll" />
            </div>

            <!-- 删除当前行按钮 -->
            <a-button type="text" danger @click="removeRow(index)">
              <template #icon>
                <DeleteOutlined />
              </template>
            </a-button>
          </div>
        </a-form-item-rest>
      </a-form-item>
      <a-form-item label="状态" name="status">
        <a-select style="width: 90px" v-model:value="form.status" :options="[
            { label: '关闭', value: 0 },
            { label: '开启', value: 1 },
          ]" />
      </a-form-item>
    </a-form>
  </a-modal>

  <!-- 成员表格 -->
  <a-table v-bind="$attrs" :columns="columns" :dataSource="dataSource" :pagination="false">
    <template #title>
      <!-- 关键修改：将 justify-between 改为 justify-end（使子元素靠右侧对齐） -->
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
        <a-badge class="text-subtext" :color="'green'">
          <template #text>
            <span class="text-subtext">{{ StatusDict[text as Status] }}</span>
          </template>
        </a-badge>
      </template>
      <template v-else-if="column.dataIndex === 'cookies'">
        <!-- 触发按钮 -->
        <a-button @click="showCookies(record)">查看</a-button>
      </template>
      <template v-else-if="column.dataIndex === 'upSecUserIds'">
        <!-- 触发按钮 -->
        <a-button @click="showUpers(record)">查看</a-button>
      </template>
      <template v-else-if="column.dataIndex === 'edit'">
        <a-button :disabled="showModal" type="link" @click="edit(record)">
          <template #icon>
            <EditFilled />
          </template>
          编辑
        </a-button>

        <a-button type="link" @click="deleted(record.id)" danger>
          <template #icon>
            <DeleteFilled />
          </template>
          删除
        </a-button>
      </template>
      <div v-else class="text-subtext">
        {{ text }}
      </div>
    </template>
  </a-table>

  <!-- Modal弹窗 -->
  <a-modal title="Cookie 详情" :visible="showCookiesModal" style="width:1200px;" @cancel="showCookiesModal = false" @ok="showCookiesModal = false">
    <!-- 弹窗内容 -->
    <div class="cookie-content">
      {{ showCookiesData || '无Cookie数据' }}
    </div>
  </a-modal>

  <!-- Modal弹窗 -->
  <a-modal title="要同步的Up主信息" :visible="showUpersModal" style="width:1200px;" @cancel="showUpersModal = false" @ok="showUpersModal = false">
    <!-- 弹窗内容 -->
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

/* 透明滚动条样式 - WebKit内核浏览器 */
.cookie-content::-webkit-scrollbar {
  width: 6px; /* 更细的滚动条 */
}

/* 轨道完全透明 */
.cookie-content::-webkit-scrollbar-track {
  background: transparent;
}

/* 滑块半透明（默认几乎看不见） */
.cookie-content::-webkit-scrollbar-thumb {
  background: rgba(150, 150, 150, 0.2); /* 浅灰透明 */
  border-radius: 3px;
}

/* hover时稍微显示一点 */
.cookie-content::-webkit-scrollbar-thumb:hover {
  background: rgba(150, 150, 150, 0.4); /* 略深一点的透明 */
}

/* 角落也透明 */
.cookie-content::-webkit-scrollbar-corner {
  background: transparent;
}

/* Firefox 透明滚动条适配 */
.cookie-content {
  scrollbar-width: thin;
  scrollbar-color: rgba(150, 150, 150, 0.2) transparent;
}

.cookie-content {
  white-space: pre-wrap;
  word-break: break-all;
  max-height: 400px;
  overflow-y: auto;
  padding: 10px;
  box-sizing: border-box;
}
.cookie-content::-webkit-scrollbar {
  width: 6px;
}
.cookie-content::-webkit-scrollbar-track {
  background: transparent;
}
.cookie-content::-webkit-scrollbar-thumb {
  background: rgba(150, 150, 150, 0.2);
  border-radius: 3px;
}
.cookie-content::-webkit-scrollbar-thumb:hover {
  background: rgba(150, 150, 150, 0.4);
}
.cookie-content::-webkit-scrollbar-corner {
  background: transparent;
}
.cookie-content {
  scrollbar-width: thin;
  scrollbar-color: rgba(150, 150, 150, 0.2) transparent;
}

/* ---------------------- 新增：a-textarea 透明滚动条 ---------------------- */
/* 1. 穿透 scoped，定位 a-textarea 内部的原生 textarea 元素 */
:deep(.ant-input-textarea-input) {
  /* 确保内容超出时显示滚动条（a-textarea 默认已配置，可省略） */
  overflow-y: auto;
  /* Firefox 透明滚动条：thin 细滚动条 + 滑块颜色/轨道颜色 */
  scrollbar-width: thin;
  scrollbar-color: rgba(150, 150, 150, 0.2) transparent;
}

/* 2. WebKit 浏览器（Chrome/Safari/Edge）透明滚动条 */
/* 滚动条宽度 */
:deep(.ant-input-textarea-input)::-webkit-scrollbar {
  width: 6px; /* 与 cookie-content 保持一致的细滚动条 */
  height: 6px; /* 横向滚动条（如需） */
}

/* 滚动条轨道（完全透明） */
:deep(.ant-input-textarea-input)::-webkit-scrollbar-track {
  background: transparent;
}

/* 滚动条滑块（半透明，hover 时加深） */
:deep(.ant-input-textarea-input)::-webkit-scrollbar-thumb {
  background: rgba(150, 150, 150, 0.2); /* 浅灰透明，默认几乎看不见 */
  border-radius: 3px; /* 圆角优化 */
}
:deep(.ant-input-textarea-input)::-webkit-scrollbar-thumb:hover {
  background: rgba(150, 150, 150, 0.4); /* hover 时略深，提升交互感知 */
}

/* 滚动条角落（完全透明，避免留白） */
:deep(.ant-input-textarea-input)::-webkit-scrollbar-corner {
  background: transparent;
}
</style>