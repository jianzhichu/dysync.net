<template>
  <div class="stats-dashboard">
    <div class="dashboard-container">
      <!-- 核心统计概览 + 7天同步趋势图 - 互换位置：左40%统计 右60%图表 -->
      <section class="stats-overview">
        <!-- 左侧：统计卡片（40%） -->
        <div class="stat-card primary-card main-card stats-left">
          <div class="stat-header">
            <div class="main-stat-item total-videos-item">
              <span class="stat-meta">视频总数</span>
              <div class="stat-value">{{ totalVideos }}</div>
            </div>
            <div class="main-stat-item total-space-item">
              <span class="stat-meta">存储总计</span>
              <div class="stat-value">{{ fileSizeTotal }} <span class="unit">G</span></div>
            </div>
          </div>

          <div class="stat-subitems">
            <div class="subitem" :title="`我喜欢的视频数: ${favoriteCount} | 占用空间: ${favoriteSize}G`">
              <div class="subitem-icon like-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
                </svg>
              </div>
              <span class="subitem-meta">我喜欢的</span>
              <span class="subitem-value">
                {{ favoriteCount }}<span class="split" v-if="favoriteCount>0">/{{ favoriteSize }}G</span>
              </span>
            </div>
            <div class="subitem" :title="`我收藏的视频数: ${collectCount} | 占用空间: ${collectSize}G`">
              <div class="subitem-icon collect-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M19 21l-7-5-7 5V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2z"></path>
                </svg>
              </div>
              <span class="subitem-meta">我收藏的</span>
              <span class="subitem-value">
                {{ collectCount }}<span class="split" v-if="collectCount>0">/{{ collectSize }}G</span>
              </span>
            </div>
            <div class="subitem" :title="`我关注的视频数: ${followCount} | 占用空间: ${followSize}G`">
              <div class="subitem-icon follow-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                  <circle cx="8.5" cy="7" r="4"></circle>
                  <line x1="20" y1="8" x2="20" y2="14"></line>
                  <line x1="23" y1="11" x2="17" y2="11"></line>
                </svg>
              </div>
              <span class="subitem-meta">我关注的</span>
              <span class="subitem-value">
                {{ followCount }}<span class="split" v-if="followCount>0">/{{ followSize }}G</span>
              </span>
            </div>
            <div class="subitem" :title="`图文视频数: ${graphicVideoCount} | 占用空间: ${graphicVideoSize}G`">
              <div class="subitem-icon graphic-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
                  <circle cx="8.5" cy="8.5" r="1.5"></circle>
                  <polyline points="21 15 16 10 5 21"></polyline>
                </svg>
              </div>
              <span class="subitem-meta">图文视频</span>
              <span class="subitem-value">
                {{ graphicVideoCount }}<span class="split" v-if="graphicVideoCount>0">/{{ graphicVideoSize }}G</span>
              </span>
            </div>
            <div class="subitem" :title="`合集数量: ${mixCount} | 占用空间: ${videoMixSize}G`">
              <div class="subitem-icon mix-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
                  <line x1="16" y1="2" x2="16" y2="6"></line>
                  <line x1="8" y1="2" x2="8" y2="6"></line>
                  <line x1="3" y1="10" x2="21" y2="10"></line>
                </svg>
              </div>
              <span class="subitem-meta">合集</span>
              <span class="subitem-value">
                {{ mixCount }}<span class="split" v-if="mixCount>0">/{{ videoMixSize }}G</span>
              </span>
            </div>
            <div class="subitem" :title="`合集数量: ${seriesCount} | 占用空间: ${videoSeriesSize}G`">
              <div class="subitem-icon series-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path>
                  <polyline points="3.27 6.96 12 12.01 20.73 6.96"></polyline>
                  <line x1="12" y1="22.08" x2="12" y2="12"></line>
                </svg>
              </div>
              <span class="subitem-meta">短剧</span>
              <span class="subitem-value">
                {{ seriesCount }}<span class="split" v-if="seriesCount>0">/{{ videoSeriesSize }}G</span>
              </span>
            </div>
          </div>
        </div>

        <div class="chart-card secondary-card main-card stats-right">
          <div class="chart-container" ref="chartRef"></div>
          <a-button type="text" shape="circle" class="fullscreen-btn" @click="openFullChart">
            <fullscreen-outlined />
          </a-button>
        </div>
      </section>

      <!-- 详细分类统计 - 保持原有不变 -->
      <section class="detailed-stats">
        <div class="stats-header">
          <div class="tab-controls">
            <a-badge :count="totalAuthors">
              <button class="tab-btn" :class="{ active: currentTab === 'author' }" @click="changeTab('author')">
                视频作者
              </button>
            </a-badge>
            <a-badge :count="categoryTotal">
              <button class="tab-btn" :class="{ active: currentTab === 'type' }" @click="changeTab('type')">
                视频分类
              </button>
            </a-badge>
          </div>
        </div>

        <transition name="stats-fade" mode="out-in">
          <div v-if="currentTab === 'author'" key="author-view" class="stats-content">
            <div class="authors-grid">
              <div class="author-card" v-for="(author, index) in authors" :key="index" @dblclick="handleDeleteItem(author)">
                <div class="author-info-row">
                  <div class="author-avatar">
                    <img :src="author.icon" alt="作者头像" />
                  </div>
                  <div class="author-info">
                    <h3 class="author-name">{{ author.name }}</h3>
                    <p class="author-stats">同步数量: {{ author.count }}</p>
                  </div>
                </div>
                <div class="author-progress">
                  <div class="progress-bar" :style="{ width: `${(author.count / totalVideos) * 100}%` }"></div>
                </div>
              </div>
            </div>
          </div>
          <div v-else key="category-view" class="stats-content">
            <div class="categories-grid">
              <div class="category-card" v-for="(category, index) in categories" :key="index" :style="{ '--category-color': category.color }">
                <div class="category-icon">
                  <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 12 12" fill="white">
                    <use :xlink:href="`#${category.icon}`"></use>
                  </svg>
                </div>
                <div class="category-info">
                  <h3 class="category-name">{{ category.name }}</h3>
                  <p class="category-stats">作品数: {{ category.count }}</p>
                </div>
                <div class="category-percentage">
                  {{ Math.round((category.count / totalVideos) * 100) }}%
                </div>
              </div>
            </div>
          </div>
        </transition>
      </section>
    </div>

    <!-- 新增：全屏图表模态窗口 -->
    <a-modal v-model:visible="showFullChartModal" title="近30天同步曲线图" width="80%" destroyOnClose @resize="handleFullChartResize" @cancel="destroyFullChart">
      <!-- 新增内联样式，强制设置宽高 -->
      <div class="full-chart-container" ref="fullChartRef" style="width: 100%; height: 400px; min-height: 400px; background: #fff;"></div>
    </a-modal>

    <svg style="display: none;">
      <symbol id="cup" viewBox="0 0 12 12">
        <path d="M18 4h2v16h-2zM4 4h14v2H4zM4 8h10v2H4zM4 12h10v2H4zM4 16h6v2H4zM4 20h6v2H4z" />
      </symbol>
    </svg>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, onUnmounted, watch, nextTick } from 'vue';
import { useApiStore } from '@/store';
import { message, Modal } from 'ant-design-vue';
import * as echarts from 'echarts';
import { FullscreenOutlined } from '@ant-design/icons-vue'; // 确保导入图标

// 类型接口不变
interface Author {
  name: string;
  count: number;
  icon: string;
  uperId?: string;
}
interface Category {
  name: string;
  count: number;
  color: string;
  icon: string;
}
interface SyncDataItem {
  date: string;
  favorite: number;
  collect: number;
  follow: number;
  graphic: number;
  mix: number;
  series: number;
}

// 状态管理 - 保留所有原有变量
const totalVideos = ref<number>(0);
const totalAuthors = ref<number>(0);
const categoryTotal = ref<number>(0);
const fileSizeTotal = ref<string>('0.00');
const totalDiskSize = ref<string>('0.00');

const favoriteCount = ref<number>(0);
const collectCount = ref<number>(0);
const followCount = ref<number>(0);
const graphicVideoCount = ref<number>(0);
const mixCount = ref<number>(0);
const seriesCount = ref<number>(0);

const favoriteSize = ref<string>('0.00');
const collectSize = ref<string>('0.00');
const followSize = ref<string>('0.00');
const graphicVideoSize = ref<string>('0.00');
const videoMixSize = ref<string>('0.00');
const videoSeriesSize = ref<string>('0.00');

const categories = ref<Category[]>([]);
const authors = ref<Author[]>([]);
const currentTab = ref<string>('author');
const tabCount = ref<number>(0);

const chartRef = ref<HTMLDivElement | null>(null);
let chartInstance: echarts.ECharts | null = null;
const syncData = ref<SyncDataItem[]>([]);
const syncFullData = ref<SyncDataItem[]>([]);

// 新增：全屏图表相关变量
const showFullChartModal = ref<boolean>(false); // 控制模态窗口显示
const fullChartRef = ref<HTMLDivElement | null>(null); // 全屏图表容器
let fullChartInstance: echarts.ECharts | null = null; // 全屏图表实例

defineOptions({
  name: 'StatsDashboard',
});

// 生成随机十六进制颜色
const generateRandomColor = () => {
  const randomHex = () =>
    Math.floor(Math.random() * 256)
      .toString(16)
      .padStart(2, '0');
  return `#${randomHex()}${randomHex()}${randomHex()}`;
};

//获取chart数据
const generateSyncData = () => {
  useApiStore()
    .VideoChart(7)
    .then((res) => {
      if (res.code === 0) {
        syncData.value = res.data;
        initChart();
      }
    })
    .catch((r) => {
      console.log(r);
    });
};

//获取chart数据
const getFullSyncData = () => {
  useApiStore()
    .VideoChart(30)
    .then((res) => {
      if (res.code === 0) {
        syncFullData.value = res.data;
        initFullChart();
      }
    })
    .catch((r) => {
      console.log(r);
    });
};

// 原有方法：初始化普通图表（提取公共配置，方便复用）
const getChartOption = (data: SyncDataItem[]): echarts.EChartsOption => {
  const colors = ['#5470c6', '#91cc75', '#fac858', '#ee6666', '#73c0de', '#3ba272'];
  const dateList = data.map((item) => item.date);

  return {
    backgroundColor: 'transparent',
    tooltip: {
      trigger: 'axis',
      zlevel: 100,
      appendToBody: true,
      confine: false,
      axisPointer: { type: 'cross' },
      backgroundColor: 'rgba(0,0,0,0.6)',
      textStyle: { fontSize: 12, color: '#fff' },
      padding: [8, 12],
      className: 'custom-chart-tooltip',
    },
    legend: {
      data: ['我喜欢的', '我收藏的', '我关注的', '图文视频', '合集', '短剧'],
      top: 0,
      textStyle: {
        color: document.documentElement.classList.contains('dark-mode') ? '#eaeaea' : '#333',
      },
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      top: '15%',
      containLabel: true,
    },
    xAxis: [
      {
        type: 'category',
        data: dateList,
        axisTick: { alignWithLabel: true },
        axisLine: {
          lineStyle: { color: document.documentElement.classList.contains('dark-mode') ? '#444' : '#ccc' },
        },
        axisLabel: {
          color: document.documentElement.classList.contains('dark-mode') ? '#b0b0c3' : '#666',
          fontSize: 11,
        },
      },
    ],
    yAxis: [
      {
        type: 'value',
        name: '同步数量',
        nameTextStyle: { color: document.documentElement.classList.contains('dark-mode') ? '#eaeaea' : '#333' },
        axisLine: { lineStyle: { color: document.documentElement.classList.contains('dark-mode') ? '#444' : '#ccc' } },
        axisLabel: { color: document.documentElement.classList.contains('dark-mode') ? '#b0b0c3' : '#666' },
        splitLine: {
          lineStyle: {
            color: document.documentElement.classList.contains('dark-mode')
              ? 'rgba(255,255,255,0.1)'
              : 'rgba(0,0,0,0.05)',
          },
        },
      },
    ],
    series: [
      {
        name: '我喜欢的',
        type: 'line',
        stack: 'Total',
        smooth: true,
        lineStyle: { width: 2, color: colors[0] },
        showSymbol: true,
        symbol: 'circle',
        symbolSize: 4,
        emphasis: {
          focus: 'series',
          symbolSize: 6,
        } as any,
        data: data.map((item) => item.favorite),
      },
      {
        name: '我收藏的',
        type: 'line',
        stack: 'Total',
        smooth: true,
        lineStyle: { width: 2, color: colors[1] },
        showSymbol: true,
        symbol: 'circle',
        symbolSize: 4,
        emphasis: { focus: 'series', symbolSize: 6 },
        data: data.map((item) => item.collect),
      },
      {
        name: '我关注的',
        type: 'line',
        stack: 'Total',
        smooth: true,
        lineStyle: { width: 2, color: colors[2] },
        showSymbol: true,
        symbol: 'circle',
        symbolSize: 4,
        emphasis: { focus: 'series', symbolSize: 6 },
        data: data.map((item) => item.follow),
      },
      {
        name: '图文视频',
        type: 'line',
        stack: 'Total',
        smooth: true,
        lineStyle: { width: 2, color: colors[3] },
        showSymbol: true,
        symbol: 'circle',
        symbolSize: 4,
        emphasis: { focus: 'series', symbolSize: 6 },
        data: data.map((item) => item.graphic),
      },
      {
        name: '合集',
        type: 'line',
        stack: 'Total',
        smooth: true,
        lineStyle: { width: 2, color: colors[4] },
        showSymbol: true,
        symbol: 'circle',
        symbolSize: 4,
        emphasis: { focus: 'series', symbolSize: 6 },
        data: data.map((item) => item.mix),
      },
      {
        name: '短剧',
        type: 'line',
        stack: 'Total',
        smooth: true,
        lineStyle: { width: 2, color: colors[5] },
        showSymbol: true,
        symbol: 'circle',
        symbolSize: 4,
        emphasis: { focus: 'series', symbolSize: 6 },
        data: data.map((item) => item.series),
      },
    ],
  };
};

// 原有方法：初始化普通图表
const initChart = () => {
  if (!chartRef.value) return;
  if (chartInstance) chartInstance.dispose();

  chartInstance = echarts.init(chartRef.value);
  const option = getChartOption(syncData.value);
  chartInstance.setOption(option);

  const resizeHandler = () => chartInstance?.resize();
  window.addEventListener('resize', resizeHandler);
  onUnmounted(() => {
    window.removeEventListener('resize', resizeHandler);
    chartInstance?.dispose();
    chartInstance = null;
  });
};
// 新增：打开全屏图表窗口
const openFullChart = () => {
  showFullChartModal.value = true;
};
// 新增：初始化全屏图表
// 重写 initFullChart 函数（增加更多日志和容错）
const initFullChart = async () => {
  console.log('进入initFullChart函数');
  await nextTick(); // 等待模态窗口DOM渲染完成

  // 增加日志排查
  console.log('fullChartRef是否存在：', !!fullChartRef.value);
  console.log('syncData是否有数据：', syncData.value.length);

  if (!fullChartRef.value) {
    console.error('全屏图表容器不存在');
    return;
  }

  if (syncData.value.length === 0) {
    console.error('同步数据为空');
    return;
  }

  console.log('开始初始化全屏图表');
  // 销毁已有实例
  if (fullChartInstance) {
    fullChartInstance.dispose();
    fullChartInstance = null;
  }

  try {
    // 初始化全屏图表
    fullChartInstance = echarts.init(fullChartRef.value);
    const option = getChartOption(syncData.value);
    // 调整全屏图表的网格和字号，优化显示效果
    option.grid = {
      left: '4%',
      right: '4%',
      bottom: '5%',
      top: '8%',
      containLabel: true,
    };
    if (option.xAxis && Array.isArray(option.xAxis) && option.xAxis[0]) {
      option.xAxis[0].axisLabel = {
        ...option.xAxis[0].axisLabel,
        fontSize: 12,
      };
    }
    if (option.yAxis && Array.isArray(option.yAxis) && option.yAxis[0]) {
      option.yAxis[0].axisLabel = {
        ...option.yAxis[0].axisLabel,
        fontSize: 12,
      };
    }

    fullChartInstance.setOption(option);
    fullChartInstance.resize(); // 强制调整大小
    console.log('全屏图表初始化成功');
  } catch (error) {
    console.error('全屏图表初始化失败：', error);
  }
};

// 新增：处理全屏图表窗口大小变化
const handleFullChartResize = () => {
  fullChartInstance?.resize();
};

// 新增：销毁全屏图表实例
const destroyFullChart = () => {
  fullChartInstance?.dispose();
  fullChartInstance = null;
};

// 监听暗黑模式切换
// watch(
//   () => document.documentElement.classList.contains('dark-mode'),
//   () => initChart(),
//   { immediate: true }
// );

// 在脚本中添加（放在onMounted之前）
watch(
  () => showFullChartModal.value,
  async (isOpen) => {
    if (isOpen) {
      console.log('弹窗已打开，准备初始化全屏图表');
      getFullSyncData();
    } else {
      destroyFullChart(); // 弹窗关闭时销毁图表
    }
  },
  { immediate: false }
);

// 加载数据 - 调用7天方法
onMounted(() => {
  generateSyncData();
  loadDashboardData();
});

// 新增：窗口关闭时销毁全屏图表
onUnmounted(() => {
  destroyFullChart();
});
const changeTab = (e: string) => {
  currentTab.value = e;
  tabCount.value = e === 'author' ? totalAuthors.value : categoryTotal.value;
};

const loadDashboardData = async () => {
  try {
    const res = await useApiStore().VideoStatics();
    totalAuthors.value = res.data.authorCount;
    categoryTotal.value = res.data.categoryCount;
    totalVideos.value = res.data.videoCount;
    fileSizeTotal.value = res.data.videoSizeTotal || '0.00';
    totalDiskSize.value = res.data.totalDiskSize || '0.00';

    favoriteCount.value = res.data.favoriteCount;
    collectCount.value = res.data.collectCount;
    followCount.value = res.data.followCount || 0;
    graphicVideoCount.value = res.data.graphicVideoCount || 0;
    mixCount.value = res.data.mixCount || 0;
    seriesCount.value = res.data.seriesCount || 0;

    favoriteSize.value = res.data.videoFavoriteSize || '0.00';
    collectSize.value = res.data.videoCollectSize || '0.00';
    followSize.value = res.data.videoFollowSize || '0.00';
    graphicVideoSize.value = res.data.graphicVideoSize || '0.00';
    videoMixSize.value = res.data.videoMixSize || '0.00';
    videoSeriesSize.value = res.data.videoSeriesSize || '0.00';

    categories.value = res.data.categories;
    authors.value = res.data.authors;
    categories.value.forEach((item) => {
      item.icon = 'cup';
      item.color = generateRandomColor();
    });
  } catch (err) {
    console.error('加载仪表盘数据失败：', err);
  }
};

const handleDeleteItem = (item: Author) => {
  Modal.confirm({
    title: '确认删除',
    content: `确定要删除博主「${item.name}」所有视频吗？删除后将无法恢复。`,
    okText: '确认删除',
    cancelText: '取消',
    okType: 'danger',
    maskClosable: false,
    onOk: () =>
      new Promise((resolve, reject) => {
        useApiStore()
          .DeleteByAuthor(item.uperId!)
          .then((res) => {
            res.code === 0
              ? (message.success('操作已提交，可稍后去日志查看结果'), resolve(true))
              : (message.error('删除失败：' + (res.message || '未知错误')), reject(false));
          })
          .catch((err) => {
            console.error('删除博主视频异常', err);
            message.error('删除异常：' + err);
            reject(false);
          });
      }),
  });
};
</script>

<style scoped>
/* 核心修改：互换网格列比例 + 适配样式 */
.stats-overview {
  display: grid;
  grid-template-columns: 4fr 6fr; /* 原6fr 4fr → 改为4fr 6fr */
  gap: 8px;
  margin-bottom: 20px;
}
.stats-left {
  padding: 10px 25px !important; /* 复用原统计卡片的padding */
  height: 100%;
  display: flex;
  flex-direction: column;
}
.stats-right {
  padding: 10px 20px; /* 复用原图表卡片的padding */
  height: 100%;
  display: flex;
  flex-direction: column;
  position: relative; /* 新增：为全屏按钮提供定位容器 */
}
.chart-container {
  flex: 1;
  width: 100%;
  min-height: 220px;
  padding-right: 40px; /* 新增：给右侧留出按钮空间，避免覆盖 */
}
.stat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
  width: 100%;
}
.stat-subitems {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 6px;
  padding-top: 8px;
  border-top: 1px solid #d0d0d0;
  flex: 1;
}
.subitem {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 8px;
  background: #ffffff;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.03);
  transition: all 0.2s ease;
  cursor: default;
}
.chart-header {
  margin-bottom: 4px;
}
.stat-value {
  font-size: 32px;
  font-weight: 700;
  color: #1a1a1a;
  line-height: 1.1;
  display: flex;
  align-items: baseline;
  gap: 6px;
}
.subitem-icon {
  width: 22px;
  height: 22px;
  border-radius: 5px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background-color: rgba(233, 30, 99, 0.1);
  color: #e91e63;
}
.stats-dashboard {
  min-height: 100vh;
  background-color: #ffffff;
  color: #333333;
  font-family: 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  padding: 20px 0;
}
.dashboard-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 15px;
}
@media (max-width: 1700px) {
  .dashboard-container {
    max-width: 95%;
  }
}
@media (max-width: 992px) {
  .stats-overview {
    grid-template-columns: 1fr;
  }
}
.main-card {
  border-radius: 16px;
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
  position: relative;
  /* overflow: hidden; */
  border: 1px solid #e0e0e0;
}
.primary-card {
  border-top: 4px solid #4caf50;
}
.main-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 35px rgba(0, 0, 0, 0.12);
  border-color: #d0d0d0;
}
.stat-meta {
  font-size: 12px;
  color: #666666;
  text-transform: uppercase;
  letter-spacing: 0.6px;
  font-weight: 500;
}
.unit {
  font-size: 16px;
  color: #444444;
  font-weight: 500;
}
.stat-icon {
  width: 50px;
  height: 50px;
  border-radius: 15px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background-color: rgba(76, 175, 80, 0.15);
  color: #4caf50;
  transition: all 0.3s ease;
}
.main-card:hover .stat-icon {
  transform: scale(1.08);
}
.chart-title {
  font-size: 15px;
  font-weight: 600;
  color: #333;
  letter-spacing: 0.3px;
}
.collect-icon {
  background-color: rgba(255, 152, 0, 0.1);
  color: #ff9800;
}
.follow-icon {
  background-color: rgba(156, 39, 176, 0.1);
  color: #9c27b0;
}
.graphic-icon {
  background-color: rgba(255, 159, 64, 0.1);
  color: #d9091a;
}
.mix-icon {
  background-color: rgba(63, 81, 181, 0.1);
  color: #3f51b5;
}
.series-icon {
  background-color: rgba(0, 188, 212, 0.1);
  color: #00bcd4;
}
.subitem-meta {
  font-size: 11px;
  color: #666666;
  font-weight: 500;
  letter-spacing: 0.3px;
  flex: 1;
}
.subitem-value {
  font-size: 12px;
  font-weight: 600;
  color: #222222;
  display: flex;
  align-items: baseline;
  gap: 3px;
}
.split {
  font-size: 11px;
  color: #999999;
  margin: 0 1px;
}
.detailed-stats {
  background: #f5f5f5;
  border-radius: 12px;
  padding: 25px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
}
.stats-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
}
.tab-controls {
  display: flex;
  gap: 10px;
}
.tab-btn {
  background: transparent;
  border: none;
  color: #666666;
  padding: 8px 16px;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
}
.tab-btn.active {
  background: rgba(76, 175, 80, 0.2);
  color: #4caf50;
  font-weight: 500;
}
.stats-content {
  animation: fadeIn 0.5s ease;
}
.authors-grid,
.categories-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 15px;
}
@media (min-width: 576px) {
  .authors-grid,
  .categories-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}
@media (min-width: 992px) {
  .authors-grid,
  .categories-grid {
    grid-template-columns: repeat(5, 1fr);
  }
}
.author-card {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: 15px;
  background: #eeeeee;
  border-radius: 8px;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
}
.author-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.12);
}
.author-info-row {
  display: flex;
  align-items: center;
  gap: 12px;
}
.author-avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  overflow: hidden;
  flex-shrink: 0;
}
.author-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.author-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 3px;
}
.author-name,
.category-name {
  margin: 0;
  font-size: 16px;
  color: #333333;
}
.author-stats,
.category-stats {
  margin: 5px 0px;
  font-size: 12px;
  color: #666666;
  line-height: 1;
}
.author-progress {
  height: 6px;
  background: #e0e0e0;
  border-radius: 3px;
  overflow: hidden;
  width: 100%;
}
.progress-bar {
  height: 100%;
  background: #4caf50;
  border-radius: 3px;
  transition: width 0.5s ease;
}
.category-card {
  display: flex;
  align-items: center;
  gap: 15px;
  padding: 15px;
  background: #eeeeee;
  border-radius: 8px;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
}
.category-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.12);
}
.category-icon {
  width: 40px;
  height: 40px;
  border-radius: 8px;
  background-color: var(--category-color);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.category-percentage {
  font-size: 14px;
  font-weight: 500;
  color: var(--category-color);
}
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
.stats-fade-enter-from,
.stats-fade-leave-to {
  opacity: 0;
  transform: translateY(10px);
}
.stats-fade-enter-active,
.stats-fade-leave-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}

.secondary-card {
  border-top: 4px solid #2196f3;
}
/* 空间卡片图标颜色 */
.secondary-card .stat-icon {
  background-color: rgba(33, 150, 243, 0.15);
  color: #2196f3;
}

html.dark-mode .secondary-card .stat-icon {
  background-color: rgba(33, 150, 243, 0.25);
}

html.dark-mode .stats-dashboard {
  background-color: #1a1a2e;
  color: #eaeaea;
}
html.dark-mode .main-card {
  border-color: rgba(255, 255, 255, 0.1);
  background-color: rgba(30, 30, 50, 0.9);
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.2);
}
html.dark-mode .main-card:hover {
  border-color: rgba(255, 255, 255, 0.15);
  box-shadow: 0 12px 35px rgba(0, 0, 0, 0.25);
}
html.dark-mode .stat-meta {
  color: #b0b0c3;
}
html.dark-mode .stat-value {
  color: #ffffff;
}
html.dark-mode .unit {
  color: #d0d0d0;
}
html.dark-mode .stat-icon {
  background-color: rgba(76, 175, 80, 0.25);
}
html.dark-mode .stat-subitems {
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}
html.dark-mode .subitem {
  background: rgba(40, 40, 65, 0.7);
  border: 1px solid rgba(255, 255, 255, 0.05);
  box-shadow: 0 3px 12px rgba(0, 0, 0, 0.15);
}
html.dark-mode .subitem:hover {
  background: rgba(40, 40, 65, 0.9);
  border-color: rgba(255, 255, 255, 0.1);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
}
html.dark-mode .subitem-meta {
  color: #c0c0d3;
}
html.dark-mode .subitem-value {
  color: #ffffff;
}
html.dark-mode .split {
  color: #8888a8;
}
html.dark-mode .subitem-icon {
  background-color: rgba(233, 30, 99, 0.2);
}
html.dark-mode .collect-icon {
  background-color: rgba(255, 152, 0, 0.2);
}
html.dark-mode .follow-icon {
  background-color: rgba(156, 39, 176, 0.2);
}
html.dark-mode .graphic-icon {
  background-color: rgba(255, 159, 64, 0.2);
}
html.dark-mode .mix-icon {
  background-color: rgba(63, 81, 181, 0.2);
}
html.dark-mode .series-icon {
  background-color: rgba(0, 188, 212, 0.2);
}
html.dark-mode .chart-title {
  color: #eaeaea;
}
html.dark-mode .detailed-stats {
  background: rgba(30, 30, 50, 0.8);
}
html.dark-mode .author-card,
html.dark-mode .category-card {
  background: rgba(40, 40, 65, 0.6);
  box-shadow: none;
}
html.dark-mode .author-name,
html.dark-mode .category-name {
  color: #ffffff;
}
html.dark-mode .author-stats,
html.dark-mode .category-stats {
  color: #b0b0c3;
}
html.dark-mode .author-progress {
  background: rgba(255, 255, 255, 0.1);
}

/* 新增：全屏按钮样式 */
.fullscreen-btn {
  position: absolute;
  top: 5px;
  right: 5px;
  z-index: 10; /* 确保在图表上方显示 */
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background-color: #ffffff;
  color: #666;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
  transition: all 0.2s ease;
}

.fullscreen-btn:hover {
  background-color: #f5f5f5;
  color: #2196f3;
  border-color: #2196f3;
}

/* 暗黑模式下的全屏按钮样式 */
html.dark-mode .fullscreen-btn {
  background-color: rgba(40, 40, 65, 0.8);
  border-color: rgba(255, 255, 255, 0.1);
  color: #b0b0c3;
}

html.dark-mode .fullscreen-btn:hover {
  background-color: rgba(60, 60, 85, 0.8);
  color: #42a5f5;
  border-color: #42a5f5;
}
</style>