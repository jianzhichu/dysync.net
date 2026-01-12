<template>
  <div style="margin: 5px 0;">
    <!-- 日志类型按钮组 -->
    <a-button-group>
      <a-button :type="typeValue === 'debug' ? 'primary' : 'default'" @click="typeValue = 'debug'; loadLogs()" class="type-btn">
        普通日志
      </a-button>
      <a-button :type="typeValue === 'error' ? 'primary' : 'default'" @click="typeValue = 'error'; loadLogs()" class="type-btn">
        错误日志
      </a-button>
    </a-button-group>

    <!-- 日期操作按钮组（核心调整颜色） -->
    <a-button-group style="margin-left: 10px;" class="date-btn-group">
      <a-button @click="changeDate('prev')">前一天</a-button>
      <a-button class="today-btn" @click="changeDate('current')">今天</a-button>
      <a-button @click="changeDate('next')">下一天</a-button>
    </a-button-group>

  </div>

  <!-- 日志展示区域 -->
  <div class="log-container">
    <a-card bordered>
      <pre class="log-content">{{ logs }}</pre>
    </a-card>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import { useApiStore } from '@/store';
import dayjs from 'dayjs';
import 'dayjs/locale/zh-cn';

// 初始化dayjs中文环境
dayjs.locale('zh-cn');

// 核心变量（极简：只维护两个核心变量）
const typeValue = ref<string>('debug'); // 日志类型
const currentDate = ref<dayjs.Dayjs>(dayjs()); // 当前选中的日期（唯一日期变量）
const logs = ref<string>('加载中...'); // 日志内容

// 计算可访问的日期范围（近10天：今天 ~ 9天前）
const maxDate = computed(() => dayjs()); // 最大日期：今天
const minDate = computed(() => dayjs().subtract(9, 'day')); // 最小日期：9天前

// 格式化日期文本（供显示和请求使用）
const currentDateText = computed(() => currentDate.value.format('YYYYMMDD'));
const minDateText = computed(() => minDate.value.format('YYYYMMDD'));
const maxDateText = computed(() => maxDate.value.format('YYYYMMDD'));

// 日期切换核心逻辑（极简：直接修改currentDate）
const changeDate = (action: 'prev' | 'current' | 'next') => {
  let newDate: dayjs.Dayjs;

  switch (action) {
    case 'prev':
      newDate = currentDate.value.subtract(1, 'day');
      break;
    case 'next':
      newDate = currentDate.value.add(1, 'day');
      break;
    case 'current':
    default:
      newDate = dayjs();
      break;
  }

  // 范围校验：限制在10天内
  if (newDate.isBefore(minDate.value)) {
    newDate = minDate.value;
    logs.value = `已到最早可查看日期(${minDateText.value})，无法继续往前`;
  } else if (newDate.isAfter(maxDate.value)) {
    newDate = maxDate.value;
    logs.value = `已到最新可查看日期(${maxDateText.value})，无法继续往后`;
  }

  // 更新当前日期并加载日志
  currentDate.value = newDate;
  loadLogs();
};

// 加载日志数据
const loadLogs = () => {
  // 拼接请求参数
  const params = `${typeValue.value}/${currentDateText.value}`;

  // 调用接口加载日志
  useApiStore()
    .apiGetLogs(params)
    .then((logContent) => {
      if (!logContent) {
        logs.value = `【${currentDateText.value}】暂无${typeValue.value === 'debug' ? '普通' : '错误'}日志数据`;
        return;
      }
      // 格式化日志时间（移除毫秒和时区）
      const formatLog = logContent.replace(/(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\.\d+ \+08:00/g, '$1');
      // 倒序显示
      logs.value = formatLog.split('\n').reverse().join('\n');
    })
    .catch((err) => {
      console.error('加载日志失败：', err);
      logs.value = `加载日志失败：${err.message || '网络异常'}`;
    });
};

// 页面挂载时加载初始日志
onMounted(() => {
  loadLogs();
});
</script>

<style lang='less' scoped>
// 日志展示容器样式
.log-container {
  width: 100%;
  height: calc(100vh - 80px);
  margin-top: 10px;

  .log-content {
    white-space: pre-wrap;
    word-wrap: break-word;
    height: 100%;
    max-height: 100%;
    overflow-y: auto;
    margin: 0;
    padding: 15px;
    background: #f9f9f9;
    font-size: 14px;
    line-height: 1.5;
  }
}

// 按钮组基础样式
:deep(.ant-btn-group) {
  .ant-btn {
    height: 36px;
    padding: 0 18px;
    font-size: 14px;
    border-radius: 0 !important; // 按钮组圆角处理
    transition: all 0.2s ease;
  }
  // 第一个按钮左圆角
  .ant-btn:first-child {
    border-radius: 6px 0 0 6px !important;
  }
  // 最后一个按钮右圆角
  .ant-btn:last-child {
    border-radius: 0 6px 6px 0 !important;
  }
}

// ========== 核心修改：日期按钮颜色 ==========
.date-btn-group {
  :deep(.ant-btn) {
    // 日期按钮默认样式
    background: #fff;
    border-color: #e6e6e6;
    color: #333;

    &:hover {
      background: #f0f9ff;
      border-color: #00b42a; // hover时边框变绿色
      color: #00b42a;
    }
  }

  // “今天”按钮高亮样式（核心颜色修改处）
  :deep(.today-btn) {
    background: #00b42a !important; // 深绿色背景（可替换成你想要的颜色）
    border-color: #00b42a !important;
    color: #fff !important;
    font-weight: 500;

    &:hover {
      background: #00c834 !important; // hover时稍浅的绿色
      border-color: #00c834 !important;
    }
  }
}

// ========== 可选：日志类型按钮颜色也同步调整（可选） ==========
.type-btn {
  :deep(&.ant-btn-primary) {
    background: #722ed1 !important; // 日志类型选中用紫色（可改）
    border-color: #722ed1 !important;
    color: #fff !important;

    &:hover {
      background: #8046e0 !important;
      border-color: #8046e0 !important;
    }
  }
}
</style>