<template>
  <a-form layout="inline" style="margin-top:5px;margin-bottom:5px;align-items: center;">

    <!-- 日期控制 -->
    <a-form-item>
      <div class="date-control-group">
        <a-button type="text" @click="handleDateMinus" class="date-btn"><left-outlined /></a-button>
        <span class="current-date-text">{{ currentDateText }}</span>
        <a-button type="text" @click="handleDatePlus" class="date-btn" :disabled="isToday"><right-outlined /></a-button>
      </div>
    </a-form-item>
    <!-- debug/error单选按钮 -->
    <a-form-item>
      <a-radio-group class="log-type-radio-group" v-model:value="typeValue" @change="typeChange" button-style="solid">
        <a-radio-button value="debug">debug</a-radio-button>
        <a-radio-button value="error">error</a-radio-button>
      </a-radio-group>
    </a-form-item>
    <!-- 复制按钮 - 同一行最右侧 -->
    <a-form-item style="margin-left: auto;">
      <a-button type="text" size="small" @click="copyLogs" class="copy-btn">
        <copy-outlined />
      </a-button>
    </a-form-item>
  </a-form>
  <div class="container">
    <a-card title="" :bordered="true" :class="{ 'log-error': typeValue === 'error' }">
      <pre class="card-width-pre">{{ logs }}</pre>
    </a-card>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, computed } from 'vue';
import { useApiStore } from '@/store';
import dayjs, { Dayjs } from 'dayjs';
import { LeftOutlined, RightOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';

dayjs.locale('zh-cn');

const currentDate = ref<Dayjs>(dayjs());
const typeValue = ref<string>('debug');
const iframeUrl = ref<string>('');
const logs = ref<string>('');

const currentDateText = computed(() => {
  return currentDate.value.format('YYYYMMDD');
});

const isToday = computed(() => {
  return currentDate.value.isSame(dayjs(), 'day');
});

const handleDateMinus = () => {
  currentDate.value = currentDate.value.subtract(1, 'day');
  updateIframeUrlAndLoadLogs();
};

const handleDatePlus = () => {
  if (isToday.value) return;
  currentDate.value = currentDate.value.add(1, 'day');
  updateIframeUrlAndLoadLogs();
};

const updateIframeUrlAndLoadLogs = () => {
  iframeUrl.value = `${typeValue.value}/${currentDateText.value}`;
  loadLogs();
};

const typeChange = (e: any) => {
  typeValue.value = e.target.value;
  updateIframeUrlAndLoadLogs();
};

const formatLogTime = (logContent: string) => {
  const timeRegex = /(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\.\d+ \+08:00/g;
  return logContent.replace(timeRegex, '$1');
};

const loadLogs = () => {
  useApiStore()
    .apiGetLogs(iframeUrl.value)
    .then((log) => {
      log = formatLogTime(log);
      const lines = log.split('\n');
      const reversedLines = lines.reverse();
      const reversedText = reversedLines.join('\n');
      logs.value = reversedText;
    })
    .catch((err) => {
      console.error('加载日志失败：', err);
      logs.value = '日志加载失败，请稍后重试';
    });
};

// 复制日志内容的核心函数
const copyLogs = async () => {
  try {
    // 空内容判断
    if (!logs.value || logs.value === '日志加载失败，请稍后重试') {
      message.warning('暂无可复制的日志内容');
      return;
    }
    // 使用浏览器剪贴板API复制内容
    await navigator.clipboard.writeText(logs.value);
    message.success('日志内容复制成功！');
  } catch (err) {
    console.error('复制失败：', err);
    // 降级方案：兼容不支持Clipboard API的浏览器
    const textArea = document.createElement('textarea');
    textArea.value = logs.value;
    document.body.appendChild(textArea);
    textArea.select();
    document.execCommand('copy');
    document.body.removeChild(textArea);
    // 提示反馈
    message.success('日志内容复制成功！');
  }
};

onMounted(() => {
  updateIframeUrlAndLoadLogs();
});
</script>

<style lang='less' scoped>
// 基础样式：仅清理默认margin/padding，不设置全屏
html,
body {
  height: 100vh;
  margin: 0;
  padding: 0;
}

// 表单容器：正常宽度，保留少量间距
:deep(.ant-form) {
  width: 100%;
  padding: 0 8px;
  box-sizing: border-box;
  margin: 0;
}

// 日志容器：正常宽度（继承父元素），高度适配
.container {
  width: 100%; // 继承父元素宽度，非全屏
  height: calc(100vh - 50px); // 留出顶部表单高度，固定高度不溢出
  padding: 0 8px; // 保留少量左右间距，视觉更友好
  box-sizing: border-box;
  overflow: hidden; // 新增：隐藏容器外溢出内容，避免挤压footer
}
// pre样式：继承card-body宽度（核心）
.card-width-pre {
  width: 100% !important; // 完全继承父元素（card-body）宽度
  min-width: 100%;
  padding: 8px; // 合理内边距，避免内容贴边
  margin: 0 !important;
  white-space: pre-wrap;
  word-break: break-all;
  height: 100%; // 继承card-body高度
  max-height: calc(100vh - 66px); // 新增：严格限制最大高度，避开表单+卡片内边距
  box-sizing: border-box;
  overflow-y: auto; // 保留：纵向滚动（优先显示垂直滚动条）
  overflow-x: auto; // 保留：横向滚动（日志行过长时）
  font-size: 14px;
  line-height: 1.5;
  // 可选：美化滚动条（适配现代浏览器）
  &::-webkit-scrollbar {
    width: 6px; // 垂直滚动条宽度
    height: 6px; // 水平滚动条高度
  }
  &::-webkit-scrollbar-thumb {
    border-radius: 3px;
    background-color: rgba(0, 0, 0, 0.2);
  }
}

.date-control-group {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;

  .date-btn {
    width: 36px;
    height: 32px;
    padding: 0;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .current-date-text {
    font-size: 14px;
    color: #1f2329;
    font-weight: 500;
    min-width: 80px;
    text-align: center;
  }
}

// 复制按钮样式
.copy-btn {
  // height: 32px; /* 和日期按钮/单选按钮高度一致 */
  // padding: 0 16px;
  color: #722ed1;
}

// card-body样式：恢复默认内边距，作为pre的宽度基准
:deep(.ant-card-body) {
  padding: 4px; // 恢复合理内边距，作为pre的宽度容器
  height: 100%; // 全屏继承容器高度
  width: 100%;
  box-sizing: border-box;
  overflow: hidden; // 新增：隐藏卡片内溢出，确保pre独占滚动区域
}

// error状态下pre文字颜色
.log-error {
  .card-width-pre {
    color: #f5222d !important;
  }
}

// radio-button样式（保留之前的修正版）
:deep(.log-type-radio-group) {
  .ant-radio-button-wrapper:first-child {
    &.ant-radio-button-wrapper-checked {
      background-color: #52c41a !important;
      border-color: #52c41a !important;
      color: #fff !important;

      &:hover,
      &:active {
        background-color: #389e0d !important;
        border-color: #389e0d !important;
      }
    }

    &:not(.ant-radio-button-wrapper-checked):hover {
      border-color: #73d13d !important;
      color: #52c41a !important;
    }
  }

  .ant-radio-button-wrapper:last-child {
    &.ant-radio-button-wrapper-checked {
      background-color: #f5222d !important;
      border-color: #f5222d !important;
      color: #fff !important;

      &:hover,
      &:active {
        background-color: #cf1322 !important;
        border-color: #cf1322 !important;
      }
    }

    &:not(.ant-radio-button-wrapper-checked):hover {
      border-color: #ff4d4f !important;
      color: #f5222d !important;
    }
  }

  .ant-radio-button-wrapper {
    &:first-child {
      border-radius: 6px 0 0 6px !important;
    }
    &:last-child {
      border-radius: 0 6px 6px 0 !important;
    }

    &:not(:first-child) {
      margin-left: -1px !important;
    }
  }

  .ant-radio-button-wrapper-checked {
    &:first-child {
      &::before {
        background-color: transparent !important;
      }
    }
  }
}

// 调整form布局，让复制按钮靠右
:deep(.ant-form-inline) {
  display: flex;
  align-items: center;
  width: 100%;
}
</style>