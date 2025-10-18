<template>
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
        <a-radio-button value="1">我喜欢的</a-radio-button>
        <a-radio-button value="2">我收藏的</a-radio-button>
      </a-radio-group>
    </a-form-item>
    <a-form-item :wrapper-col="{ offset: 8, span: 16 }">
      <a-space>
        <a-button type="primary" @click="GetRecords">查询</a-button>
        <a-button type="danger" @click="StartNow">立即重新同步</a-button>
      </a-space>
    </a-form-item>
  </a-form>
  <a-table :columns="columns" :data-source="dataSource" bordered :pagination="pagination" @change="handleTableChange" :loading="loading">
  </a-table>
</template>
<script lang="ts" setup>
import { defineComponent, reactive, ref } from 'vue';
import { useApiStore } from '@/store';
import type { UnwrapRef } from 'vue';
import { onMounted } from 'vue';
import dayjs, { Dayjs } from 'dayjs';
import locale from 'ant-design-vue/es/date-picker/locale/zh_CN';
type RangeValue = [Dayjs, Dayjs];
import 'dayjs/locale/zh-cn';
dayjs.locale('zh-cn');
const columns = ref([
  {
    title: '同步时间',
    dataIndex: 'syncTimeStr',
    // sorter: true,
    align: 'center',
    width: 180,
  },
  {
    title: '同步类型',
    dataIndex: 'viedoTypeStr',
    // sorter: true,
    align: 'center',
    width: 100,
  },
  {
    title: '视频类型',
    dataIndex: 'viedoCate',
    // sorter: true,
    width: 300,
    align: 'center',
  },
  {
    title: '作者',
    dataIndex: 'author',
    // sorter: true,
    align: 'center',
    width: 150,
  },
  {
    title: '视频名称',
    dataIndex: 'videoTitle',
    // sorter: true,
    align: 'left',
  },

  {
    title: '用户',
    dataIndex: 'dyUser',
    // sorter: true,
    align: 'center',
    width: 200,
  },
]);
const loading = ref(false);
interface DataItem {}
const datas: UnwrapRef<DataItem[]> = reactive([]);

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
});
const pagination = ref({
  current: 1,
  defaultPageSize: 10,
  total: 0,
  showTotal: () => `共 ${0} 条`,
});

const handleTableChange = (e) => {
  console.log(e);
  pagination.value.current = e.current;
  pagination.value.defaultPageSize = e.defaultPageSize;
  pagination.value.total = e.total;
  pagination.value.showTotal = () => `共 ${e.total} 条`;
  GetRecords();
};

const StartNow = () => {
  useApiStore()
    .StartJobNow()
    .then((res) => {});
};
const datePicked = (ref, dateArry) => {
  quaryData.dates = dateArry;
  console.log(dateArry);
};
const dataSource = ref(datas);

const onViedoTypeChanged = (e) => {
  // console.log(e.target.value);
  quaryData.viedoType = e.target.value;
  pagination.value.current = 1;
  GetRecords();
};
</script>
<style scoped>
</style>
