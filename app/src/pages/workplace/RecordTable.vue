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
          <!-- 核心修改点：绑定 disabled 属性 -->
          <a-button type="danger" @click="StartNow" :disabled="isSyncing">
            <!-- 可选：添加加载状态提示 -->
            <template #icon>
              <a-spin v-if="isSyncing" size="small" />
            </template>
            立即同步
          </a-button>
        </a-space>
      </a-form-item>
    </a-form>
    <a-table :columns="columns" :data-source="dataSource" bordered :pagination="pagination" @change="handleTableChange" :loading="loading">
    </a-table>
  </div>
</template>

<script lang="ts" setup>
import { defineComponent, reactive, ref } from 'vue';
import { useApiStore } from '@/store';
import type { UnwrapRef } from 'vue';
import { onMounted } from 'vue';
import dayjs, { Dayjs } from 'dayjs';
import locale from 'ant-design-vue/es/date-picker/locale/zh_CN';
import { message, Spin } from 'ant-design-vue'; // 引入 Spin 用于显示加载图标
type RangeValue = [Dayjs, Dayjs];
import 'dayjs/locale/zh-cn';
dayjs.locale('zh-cn');

// ... (columns, loading, DataItem, datas, showImageViedo, QuaryParam, value1, ranges, quaryData 的定义保持不变)
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
  },
  {
    title: 'CK名称',
    dataIndex: 'dyUser',
    align: 'center',
    width: 200,
  },
]);
const loading = ref(false);
interface DataItem {}
const datas: UnwrapRef<DataItem[]> = reactive([]);
const showImageViedo = ref(false);
interface QuaryParam {
  dates?: string[];
  pageIndex: number;
  pageSize: number;
  author: string;
  tag: string;
  viedoType: string;
}

const value1 = ref<RangeValue>();
const ranges = {
  今天: [dayjs(), dayjs()] as RangeValue,
  本月: [dayjs(), dayjs().endOf('month')] as RangeValue,
};
const quaryData: UnwrapRef<QuaryParam> = reactive({
  datas: [],
  pageIndex: 0,
  pageSize: 20,
  author: '',
  tag: '',
  viedoType: '*',
});

const GetRecords = () => {
  loading.value = true;
  quaryData.pageIndex = pagination.value.current;
  quaryData.pageSize = pagination.value.defaultPageSize;
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
      }
    });
};

onMounted(() => {
  GetRecords();
  getConfig(); // 在 onMounted 中调用 getConfig
});

const pagination = ref({
  current: 1,
  defaultPageSize: 10,
  total: 0,
  showTotal: () => `共 ${0} 条`,
});

const getConfig = () => {
  useApiStore()
    .apiGetConfig()
    .then((res) => {
      if (res.code === 0) {
        showImageViedo.value = res.data.downImageVideoFromEnv;
      } else {
        // 可以在这里添加配置获取失败的处理
      }
    })
    .catch((error) => {
      console.error('获取配置失败:', error);
    });
};

const handleTableChange = (e) => {
  pagination.value.current = e.current;
  pagination.value.defaultPageSize = e.defaultPageSize;
  // 通常 total 是由后端返回的，这里不需要手动设置
  GetRecords();
};

// 核心修改点：添加一个 ref 来控制按钮状态
const isSyncing = ref(false);

const StartNow = () => {
  // 如果正在同步中，则直接返回，防止重复点击
  if (isSyncing.value) {
    return;
  }

  message.success('请耐心等待，同步任务正在启动...');
  isSyncing.value = true; // 开始同步，禁用按钮

  useApiStore()
    .StartJobNow()
    .then((res) => {
      // 根据后端返回的状态码判断是否真正成功
      if (res.code === 0) {
        message.success('同步任务启动成功！');
      } else {
        message.error(`同步任务启动失败: ${res.message || '未知错误'}`);
      }
    })
    .catch((error) => {
      console.error('同步任务API调用失败:', error);
      message.error('同步任务启动失败，请检查网络或联系管理员。');
    })
    .finally(() => {
      isSyncing.value = false; // 无论成功失败，都恢复按钮状态
    });
};

const datePicked = (ref, dateArry) => {
  // 注意：dateArry 是 Dayjs 对象数组，如果后端需要字符串，需要格式化
  // quaryData.dates = dateArry.map(date => date.format('YYYY-MM-DD'));
  quaryData.dates = dateArry;
  console.log(dateArry);
};

const dataSource = ref(datas);

const onViedoTypeChanged = (e) => {
  quaryData.viedoType = e.target.value;
  pagination.value.current = 1; // 切换类型后，重置页码到第一页
  GetRecords();
};
</script>

<style scoped>
</style>