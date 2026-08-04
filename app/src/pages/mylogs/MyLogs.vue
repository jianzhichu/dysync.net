<template>
  <div class="log-page">
    <a-form layout="inline" class="log-toolbar" style="margin-top:5px;margin-bottom:5px;align-items: center;">
      <!-- 日期控制 -->
      <a-form-item class="toolbar-item">
        <div class="date-control-group">
          <a-button type="text" @click="handleDateMinus" class="date-btn">
            <left-outlined />
          </a-button>
          <span class="current-date-text">{{ currentDateText }}</span>
          <a-button type="text" @click="handleDatePlus" class="date-btn" :disabled="isToday">
            <right-outlined />
          </a-button>
        </div>
      </a-form-item>

      <!-- debug/error单选按钮 -->
      <a-form-item class="toolbar-item">
        <a-radio-group class="log-type-radio-group" v-model:value="typeValue" @change="typeChange" button-style="solid">
          <a-radio-button value="debug">DEBUG</a-radio-button>
          <a-radio-button value="error">ERROR</a-radio-button>
        </a-radio-group>
      </a-form-item>

      <!-- 复制按钮 - 同一行最右侧 -->
      <a-form-item class="toolbar-item toolbar-copy-item" style="margin-left: auto;">
        <a-button type="text" size="small" @click="copyLogs" class="copy-btn" title="复制日志">
          <copy-outlined />
          <span class="copy-btn-text">复制日志</span>
        </a-button>
      </a-form-item>
    </a-form>

    <div class="container">
      <a-card title="" :bordered="true" class="log-card" :class="{ 'log-error': typeValue === 'error' }">
        <div class="log-card-header">
          <div class="log-card-title">
            <span class="log-status-dot"></span>
            <span>{{ typeValue === 'error' ? '错误日志' : '调试日志' }}</span>
          </div>
          <span class="log-date-badge">{{ currentDateText }}</span>
        </div>
        <pre class="card-width-pre">{{ logs }}</pre>
      </a-card>
    </div>
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

<style lang="less" scoped>
.log-page {
  width: 100%;
  min-height: calc(100vh - 112px);
  padding: 14px;
  box-sizing: border-box;
  background: radial-gradient(circle at 100% 0, rgba(124, 58, 237, 0.06), transparent 30%),
    linear-gradient(180deg, #f8fafc 0%, #f5f7fa 100%);
}

/* 工具栏 */
.log-toolbar {
  width: 100%;
  min-height: 58px;
  margin: 0 0 12px !important;
  padding: 10px 12px !important;
  display: flex;
  align-items: center;
  gap: 10px;
  box-sizing: border-box;
  border: 1px solid #e7ecf2;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.95);
  box-shadow: 0 6px 20px rgba(31, 45, 61, 0.055);
  backdrop-filter: blur(10px);
}

:deep(.log-toolbar .ant-form-item) {
  margin: 0 !important;
}

.toolbar-copy-item {
  margin-left: auto !important;
}

/* 日期切换 */
.date-control-group {
  min-width: 210px;
  height: 36px;
  padding: 3px 5px;
  display: grid;
  grid-template-columns: 34px minmax(110px, 1fr) 34px;
  align-items: center;
  gap: 4px;
  box-sizing: border-box;
  border: 1px solid #e2e7ee;
  border-radius: 10px;
  background: #f8fafc;
}

.date-btn {
  width: 30px !important;
  height: 30px !important;
  padding: 0 !important;
  display: inline-flex !important;
  align-items: center;
  justify-content: center;
  border-radius: 8px !important;
  color: #6f7a86 !important;
}

.date-btn:not(:disabled):hover {
  color: #7c3aed !important;
  background: rgba(124, 58, 237, 0.08) !important;
}

.date-btn:disabled {
  color: #c2c8cf !important;
  background: transparent !important;
}

.current-date-text {
  min-width: 0;
  color: #27333d;
  font-size: 14px;
  line-height: 1;
  font-weight: 700;
  text-align: center;
  letter-spacing: 0.5px;
  font-variant-numeric: tabular-nums;
}

/* 日志类型切换 */
:deep(.log-type-radio-group) {
  display: inline-flex;
}

:deep(.log-type-radio-group .ant-radio-button-wrapper) {
  min-width: 82px;
  height: 36px;
  padding: 0 16px;
  color: #64707d;
  border-color: #e2e7ee;
  background: #ffffff;
  font-size: 12px;
  line-height: 34px;
  font-weight: 700;
  text-align: center;
  letter-spacing: 0.4px;
  box-shadow: none;
}

:deep(.log-type-radio-group .ant-radio-button-wrapper:first-child) {
  border-radius: 10px 0 0 10px !important;
}

:deep(.log-type-radio-group .ant-radio-button-wrapper:last-child) {
  border-radius: 0 10px 10px 0 !important;
}

:deep(.log-type-radio-group .ant-radio-button-wrapper:first-child.ant-radio-button-wrapper-checked) {
  color: #ffffff !important;
  border-color: #22a45a !important;
  background: #22a45a !important;
  box-shadow: -1px 0 0 0 #22a45a !important;
}

:deep(.log-type-radio-group .ant-radio-button-wrapper:first-child.ant-radio-button-wrapper-checked:hover) {
  border-color: #178844 !important;
  background: #178844 !important;
}

:deep(.log-type-radio-group .ant-radio-button-wrapper:last-child.ant-radio-button-wrapper-checked) {
  color: #ffffff !important;
  border-color: #ef4444 !important;
  background: #ef4444 !important;
  box-shadow: -1px 0 0 0 #ef4444 !important;
}

:deep(.log-type-radio-group .ant-radio-button-wrapper:last-child.ant-radio-button-wrapper-checked:hover) {
  border-color: #dc2626 !important;
  background: #dc2626 !important;
}

:deep(.log-type-radio-group .ant-radio-button-wrapper:not(.ant-radio-button-wrapper-checked):hover) {
  color: #7c3aed !important;
  border-color: #b8a0f3 !important;
}

/* 复制按钮 */
.copy-btn {
  height: 36px !important;
  padding: 0 13px !important;
  display: inline-flex !important;
  align-items: center;
  justify-content: center;
  gap: 6px;
  border-radius: 9px !important;
  color: #7c3aed !important;
  background: rgba(124, 58, 237, 0.07) !important;
  font-size: 12px;
  font-weight: 600;
}

.copy-btn:hover {
  color: #ffffff !important;
  background: #7c3aed !important;
}

.copy-btn-text {
  line-height: 1;
}

/* 日志主体 */
.container {
  width: 100%;
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

:deep(.log-card.ant-card) {
  overflow: hidden;
  border: 1px solid #e4e9ef !important;
  border-radius: 14px !important;
  background: #ffffff;
  box-shadow: 0 8px 26px rgba(31, 45, 61, 0.06);
}

:deep(.log-card .ant-card-body) {
  width: 100%;
  height: auto;
  padding: 0 !important;
  overflow: hidden;
  box-sizing: border-box;
}

.log-card-header {
  min-height: 48px;
  padding: 0 14px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  border-bottom: 1px solid #e8edf2;
  background: #f8fafc;
}

.log-card-title {
  min-width: 0;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  color: #33404b;
  font-size: 13px;
  font-weight: 700;
}

.log-status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #22a45a;
  box-shadow: 0 0 0 4px rgba(34, 164, 90, 0.12);
}

.log-error .log-status-dot {
  background: #ef4444;
  box-shadow: 0 0 0 4px rgba(239, 68, 68, 0.12);
}

.log-date-badge {
  padding: 4px 8px;
  border-radius: 999px;
  color: #7d8894;
  background: #edf1f5;
  font-size: 10px;
  line-height: 1.2;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
}

.card-width-pre {
  width: 100% !important;
  min-width: 100%;
  min-height: 300px;
  max-height: calc(100vh - 260px);
  margin: 0 !important;
  padding: 16px 18px;
  box-sizing: border-box;
  overflow: auto;
  color: #33404b;
  background: linear-gradient(180deg, #ffffff 0%, #fbfcfd 100%);
  font-family: 'JetBrains Mono', 'Cascadia Code', Consolas, 'Microsoft YaHei Mono', monospace;
  font-size: 12px;
  line-height: 1.72;
  white-space: pre-wrap;
  word-break: break-word;
  tab-size: 2;
  font-variant-numeric: tabular-nums;

  &::selection {
    color: #ffffff;
    background: rgba(124, 58, 237, 0.75);
  }

  &::-webkit-scrollbar {
    width: 8px;
    height: 8px;
  }

  &::-webkit-scrollbar-track {
    background: transparent;
  }

  &::-webkit-scrollbar-thumb {
    border: 2px solid transparent;
    border-radius: 999px;
    background: rgba(100, 111, 123, 0.28);
    background-clip: padding-box;
  }

  &::-webkit-scrollbar-thumb:hover {
    background: rgba(100, 111, 123, 0.46);
    background-clip: padding-box;
  }
}

.log-error .card-width-pre {
  color: #c9343c !important;
  background: linear-gradient(180deg, #fffdfd 0%, #fffafa 100%);
}

/* 暗色主题 */
:global(html.dark-mode) .log-page {
  background: radial-gradient(circle at 100% 0, rgba(124, 58, 237, 0.12), transparent 30%), #141426;
}

:global(html.dark-mode) .log-toolbar {
  border-color: #303247;
  background: rgba(27, 27, 49, 0.94);
  box-shadow: none;
}

:global(html.dark-mode) .date-control-group {
  border-color: #34364c;
  background: #202037;
}

:global(html.dark-mode) .date-btn {
  color: #aeb1bd !important;
}

:global(html.dark-mode) .date-btn:not(:disabled):hover {
  color: #c7a6ff !important;
  background: rgba(169, 112, 255, 0.12) !important;
}

:global(html.dark-mode) .date-btn:disabled {
  color: #5f6172 !important;
}

:global(html.dark-mode) .current-date-text {
  color: #eceef3;
}

:global(html.dark-mode) :deep(.log-type-radio-group .ant-radio-button-wrapper) {
  color: #bfc1cd;
  border-color: #34364c;
  background: #202037;
}

:global(html.dark-mode) .copy-btn {
  color: #c7a6ff !important;
  background: rgba(169, 112, 255, 0.13) !important;
}

:global(html.dark-mode) .copy-btn:hover {
  color: #ffffff !important;
  background: #7c3aed !important;
}

:global(html.dark-mode) :deep(.log-card.ant-card) {
  border-color: #303247 !important;
  background: #1b1b31 !important;
  box-shadow: none;
}

:global(html.dark-mode) .log-card-header {
  border-bottom-color: #303247;
  background: #202037;
}

:global(html.dark-mode) .log-card-title {
  color: #e6e7ed;
}

:global(html.dark-mode) .log-date-badge {
  color: #a7a9b5;
  background: #2a2b43;
}

:global(html.dark-mode) .card-width-pre {
  color: #cfd1da;
  background: linear-gradient(180deg, #1b1b31 0%, #18182c 100%);
}

:global(html.dark-mode) .log-error .card-width-pre {
  color: #ff8a8f !important;
  background: linear-gradient(180deg, #211b2d 0%, #1c1929 100%);
}

:global(html.dark-mode) .card-width-pre::-webkit-scrollbar-thumb {
  background: rgba(210, 212, 225, 0.25);
  background-clip: padding-box;
}

/* 响应式 */
@media (max-width: 768px) {
  .log-page {
    padding: 9px;
  }

  .log-toolbar {
    padding: 9px !important;
    flex-wrap: wrap;
    gap: 8px;
  }

  .date-control-group {
    min-width: 190px;
  }

  .toolbar-copy-item {
    margin-left: 0 !important;
  }

  .copy-btn-text {
    display: none;
  }

  .copy-btn {
    width: 36px !important;
    padding: 0 !important;
  }

  .card-width-pre {
    min-height: 260px;
    max-height: calc(100vh - 280px);
    padding: 13px;
    font-size: 11px;
  }
}

@media (max-width: 480px) {
  .log-toolbar {
    display: grid !important;
    grid-template-columns: 1fr auto;
  }

  .toolbar-item:first-child {
    grid-column: 1 / -1;
  }

  .date-control-group {
    width: 100%;
    min-width: 0;
  }

  :deep(.log-type-radio-group .ant-radio-button-wrapper) {
    min-width: 72px;
    padding: 0 10px;
  }
}
</style>
