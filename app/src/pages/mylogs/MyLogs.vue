<template>
  <a-form layout="inline" style="margin-top:5px;margin-bottom:5px;">
    <a-form-item>
      <a-date-picker v-model:value="dateValue" format="YYYYMMDD" :locale="locale" @change="datePickChange" />
    </a-form-item>
    <a-form-item>
      <a-radio-group v-model:value="typeValue" button-style="solid" @change="typeChange">
        <a-radio-button value="debug">debug</a-radio-button>
        <a-radio-button value="error">error</a-radio-button>
      </a-radio-group>
    </a-form-item>
  </a-form>
  <div class="container">
    <a-card title="" :bordered="true">
      <pre>{{ logs }}</pre>
    </a-card>
  </div>
</template>
<script lang="ts" setup>
import { defineComponent, reactive, ref, watch, onMounted } from 'vue';
import { useApiStore } from '@/store';
import type { UnwrapRef } from 'vue';
import dayjs, { Dayjs } from 'dayjs';
import locale from 'ant-design-vue/es/date-picker/locale/zh_CN';
type RangeValue = [Dayjs, Dayjs];
import 'dayjs/locale/zh-cn';
dayjs.locale('zh-cn');
const dateValue = ref<Dayjs>(dayjs(Date()));
const typeValue = ref<string>('debug');
const iframeUrl = ref<string>('');
const dateValue1 = ref<string>();
const logs = ref<string>('');
dateValue1.value = dayjs(Date()).format('YYYYMMDD');
iframeUrl.value = `type=${typeValue.value}&date=${dateValue1.value}`;
const datePickChange = (e, dateStr) => {
  dateValue1.value = dateStr;
  iframeUrl.value = `type=${typeValue.value}&date=${dateValue1.value}`;
  console.log(iframeUrl);
  loadLogs();
};

const typeChange = (e) => {
  console.log(e.target);
  iframeUrl.value = `type=${e.target.value}&date=${dateValue1.value}`;
  loadLogs();
};
const mIfrm = ref<any>(null);

// 核心：处理日志时间戳，移除 .xxx +08:00 部分
const formatLogTime = (logContent: string) => {
  // 正则匹配：2025-12-11 18:41:39.624 +08:00 格式，捕获前面的日期时间部分
  // 正则解释：
  // (\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) 捕获 年-月-日 时:分:秒
  // \.\d+ \+08:00 匹配 .毫秒 +时区 部分
  const timeRegex = /(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\.\d+ \+08:00/g;

  // 替换匹配到的内容，只保留捕获的日期时间部分
  return logContent.replace(timeRegex, '$1');
};
onMounted(() => {
  // console.log(mIfrm.value);
  loadLogs();
});
const loadLogs = () => {
  useApiStore()
    .apiGetLogs(iframeUrl.value)
    .then((log) => {
      // console.log(log);
      log = formatLogTime(log);
      const lines = log.split('\n'); // 将文本按换行符分割为行数组
      const reversedLines = lines.reverse(); // 对行数组进行倒序操作
      const reversedText = reversedLines.join('\n'); // 将行数组重新连接为一个字符串

      logs.value = reversedText;
    });
};
</script>
<style lang='less'  scoped>
html {
  height: 100vh;
}
.container {
  width: 100%;
  height: 100%;
  // max-height: 400px;
  iframe {
    .word-wrap {
      color: white !important;
    }
  }
}
</style>
