<template>
  <div class="stats-dashboard">
    <div class="dashboard-container">
      <!-- 核心统计概览 - 美化版 -->
      <section class="stats-overview">
        <!-- 总视频数卡片 -->
        <div class="stat-card primary-card main-card">
          <!-- 卡片头部 -->
          <div class="stat-header">
            <div class="header-left">
              <span class="stat-meta">视频总数</span>
              <div class="stat-value">{{ totalVideos }}</div>
            </div>
            <div class="stat-icon video-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <polygon points="23 7 16 12 23 17 23 7"></polygon>
                <rect x="1" y="5" width="15" height="14" rx="2" ry="2"></rect>
              </svg>
            </div>
          </div>

          <!-- 细分项区域 - 新增合集、短剧统计项 -->
          <div class="stat-subitems">
            <div class="subitem" :title="`我喜欢的视频数: ${favoriteCount}`">
              <div class="subitem-icon like-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
                </svg>
              </div>
              <span class="subitem-meta">我喜欢的</span>
              <span class="subitem-value">{{ favoriteCount }}</span>
            </div>

            <div class="subitem" :title="`我收藏的视频数: ${collectCount}`">
              <div class="subitem-icon collect-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M19 21l-7-5-7 5V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2z"></path>
                </svg>
              </div>
              <span class="subitem-meta">我收藏的</span>
              <span class="subitem-value">{{ collectCount }}</span>
            </div>

            <div class="subitem" :title="`我关注的视频数: ${followCount}`">
              <div class="subitem-icon follow-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                  <circle cx="8.5" cy="7" r="4"></circle>
                  <line x1="20" y1="8" x2="20" y2="14"></line>
                  <line x1="23" y1="11" x2="17" y2="11"></line>
                </svg>
              </div>
              <span class="subitem-meta">我关注的</span>
              <span class="subitem-value">{{ followCount }}</span>
            </div>

            <div class="subitem" :title="`图文视频数: ${graphicVideoCount}`">
              <div class="subitem-icon graphic-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
                  <circle cx="8.5" cy="8.5" r="1.5"></circle>
                  <polyline points="21 15 16 10 5 21"></polyline>
                </svg>
              </div>
              <span class="subitem-meta">图文视频</span>
              <span class="subitem-value">{{ graphicVideoCount }}</span>
            </div>

            <!-- 新增：合集数量 -->
            <div class="subitem" :title="`合集数量: ${mixCount}`">
              <div class="subitem-icon mix-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
                  <line x1="16" y1="2" x2="16" y2="6"></line>
                  <line x1="8" y1="2" x2="8" y2="6"></line>
                  <line x1="3" y1="10" x2="21" y2="10"></line>
                </svg>
              </div>
              <span class="subitem-meta">合集数量</span>
              <span class="subitem-value">{{ mixCount }}</span>
            </div>

            <!-- 新增：短剧数量 -->
            <div class="subitem" :title="`短剧数量: ${seriesCount}`">
              <div class="subitem-icon series-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path>
                  <polyline points="3.27 6.96 12 12.01 20.73 6.96"></polyline>
                  <line x1="12" y1="22.08" x2="12" y2="12"></line>
                </svg>
              </div>
              <span class="subitem-meta">短剧数量</span>
              <span class="subitem-value">{{ seriesCount }}</span>
            </div>
          </div>
        </div>

        <!-- 总占用空间卡片 -->
        <div class="stat-card secondary-card main-card">
          <!-- 卡片头部 -->
          <div class="stat-header">
            <div class="header-left">
              <span class="stat-meta">空间总计</span>
              <div class="stat-value">{{ fileSizeTotal }} <span class="unit">G</span></div>
            </div>
            <div class="stat-icon size-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M22 12H2v8h20v-8z" />
                <path d="M6 18h.01" />
                <path d="M10 18h.01" />
              </svg>
            </div>
          </div>

          <!-- 细分项区域 - 新增合集、短剧空间占用 -->
          <div class="stat-subitems">
            <div class="subitem" :title="`喜欢的视频占用: ${favoriteSize}G`">
              <div class="subitem-icon like-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
                </svg>
              </div>
              <span class="subitem-meta">喜欢占用</span>
              <span class="subitem-value">{{ favoriteSize }} <span class="unit">G</span></span>
            </div>

            <div class="subitem" :title="`收藏的视频占用: ${collectSize}G`">
              <div class="subitem-icon collect-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M19 21l-7-5-7 5V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2z"></path>
                </svg>
              </div>
              <span class="subitem-meta">收藏占用</span>
              <span class="subitem-value">{{ collectSize }} <span class="unit">G</span></span>
            </div>

            <div class="subitem" :title="`关注的视频占用: ${followSize}G`">
              <div class="subitem-icon follow-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                  <circle cx="8.5" cy="7" r="4"></circle>
                  <line x1="20" y1="8" x2="20" y2="14"></line>
                  <line x1="23" y1="11" x2="17" y2="11"></line>
                </svg>
              </div>
              <span class="subitem-meta">关注占用</span>
              <span class="subitem-value">{{ followSize }} <span class="unit">G</span></span>
            </div>

            <div class="subitem" :title="`图文视频占用: ${graphicVideoSize}G`">
              <div class="subitem-icon graphic-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
                  <circle cx="8.5" cy="8.5" r="1.5"></circle>
                  <polyline points="21 15 16 10 5 21"></polyline>
                </svg>
              </div>
              <span class="subitem-meta">图文占用</span>
              <span class="subitem-value">{{ graphicVideoSize }} <span class="unit">G</span></span>
            </div>

            <!-- 新增：合集空间占用 -->
            <div class="subitem" :title="`合集占用: ${videoMixSize}G`">
              <div class="subitem-icon mix-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
                  <line x1="16" y1="2" x2="16" y2="6"></line>
                  <line x1="8" y1="2" x2="8" y2="6"></line>
                  <line x1="3" y1="10" x2="21" y2="10"></line>
                </svg>
              </div>
              <span class="subitem-meta">合集占用</span>
              <span class="subitem-value">{{ videoMixSize }} <span class="unit">G</span></span>
            </div>

            <!-- 新增：短剧空间占用 -->
            <div class="subitem" :title="`短剧占用: ${videoSeriesSize}G`">
              <div class="subitem-icon series-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path>
                  <polyline points="3.27 6.96 12 12.01 20.73 6.96"></polyline>
                  <line x1="12" y1="22.08" x2="12" y2="12"></line>
                </svg>
              </div>
              <span class="subitem-meta">短剧占用</span>
              <span class="subitem-value">{{ videoSeriesSize }} <span class="unit">G</span></span>
            </div>
          </div>
        </div>
      </section>

      <!-- 详细分类统计（保持不变） -->
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
          <!-- 作者统计 -->
          <div v-if="currentTab === 'author'" key="author-view" class="stats-content">
            <div class="authors-grid">
              <div class="author-card" v-for="(author, index) in authors" :key="index" @dblclick="handleDeleteItem(author)">
                <!-- 新增横向容器：包裹头像和作者信息 -->
                <div class="author-info-row">
                  <div class="author-avatar">
                    <img :src="author.icon" alt="作者头像" />
                  </div>
                  <div class="author-info">
                    <h3 class="author-name">{{ author.name }}</h3>
                    <p class="author-stats">同步数量: {{ author.count }}</p>
                  </div>
                </div>
                <!-- 进度条独立在横向容器下方，不与头像同行 -->
                <div class="author-progress">
                  <div class="progress-bar" :style="{ width: `${(author.count / totalVideos) * 100}%` }"></div>
                </div>
              </div>
            </div>
          </div>
          <!-- 分类统计 -->
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

    <!-- SVG图标定义 -->
    <svg style="display: none;">
      <symbol id="cup" viewBox="0 0 12 12">
        <path d="M18 4h2v16h-2zM4 4h14v2H4zM4 8h10v2H4zM4 12h10v2H4zM4 16h6v2H4zM4 20h6v2H4z" />
      </symbol>
    </svg>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from 'vue';
import { useApiStore } from '@/store';
import { message, Spin, Empty, Tooltip, Modal, Form, FormInstance, Popconfirm } from 'ant-design-vue';
// 类型接口
interface Author {
  name: string;
  count: number;
  icon: string;
}

interface Category {
  name: string;
  count: number;
  color: string;
  icon: string;
}

// 状态管理
const totalVideos = ref<number>(0);
const totalAuthors = ref<number>(0);
const categoryTotal = ref<number>(0);
const fileSizeTotal = ref<string>('0.00');
const totalDiskSize = ref<string>('0.00');

const favoriteCount = ref<number>(0);
const collectCount = ref<number>(0);
const followCount = ref<number>(0);
const graphicVideoCount = ref<number>(0);
// 新增：合集数量、短剧数量
const mixCount = ref<number>(0);
const seriesCount = ref<number>(0);

const favoriteSize = ref<string>('0.00');
const collectSize = ref<string>('0.00');
const followSize = ref<string>('0.00');
const graphicVideoSize = ref<string>('0.00');
// 新增：合集占用空间、短剧占用空间
const videoMixSize = ref<string>('0.00');
const videoSeriesSize = ref<string>('0.00');

const categories = ref<Category[]>([]);
const authors = ref<Author[]>([]);
const currentTab = ref<string>('author');
const tabCount = ref<number>(0);

// 组件名称
defineOptions({
  name: 'StatsDashboard',
});

// 生成随机十六进制颜色的工具函数
const generateRandomColor = () => {
  // 生成0-255的随机RGB值，转换为十六进制并补零
  const randomHex = () =>
    Math.floor(Math.random() * 256)
      .toString(16)
      .padStart(2, '0');
  return `#${randomHex()}${randomHex()}${randomHex()}`;
};

// 加载数据和切换标签逻辑
onMounted(() => {
  loadDashboardData();
});

const changeTab = (e: any) => {
  currentTab.value = e;
  if (e == 'author') {
    tabCount.value = totalAuthors.value;
  } else {
    tabCount.value = categoryTotal.value;
  }
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
    // 新增：从接口获取合集、短剧数量
    mixCount.value = res.data.mixCount || 0;
    seriesCount.value = res.data.seriesCount || 0;

    favoriteSize.value = res.data.videoFavoriteSize || '0.00';
    collectSize.value = res.data.videoCollectSize || '0.00';
    followSize.value = res.data.videoFollowSize || '0.00';
    graphicVideoSize.value = res.data.graphicVideoSize || '0.00';
    // 新增：从接口获取合集、短剧空间占用
    videoMixSize.value = res.data.videoMixSize || '0.00';
    videoSeriesSize.value = res.data.videoSeriesSize || '0.00';

    categories.value = res.data.categories;
    authors.value = res.data.authors;

    // 移除categoriessss相关逻辑，直接给分类设置固定图标cup（保证显示）
    // 动态为每个分类生成随机颜色，替代原有的colorArray
    categories.value.forEach((item) => {
      item.icon = 'cup'; // 固定使用已定义的cup图标，确保显示正常
      item.color = generateRandomColor(); // 随机生成颜色
    });
  } catch (err) {
    console.error('加载仪表盘数据失败：', err);
  }
};

const getRandomElements = (arr: any[], n: number) => {
  if (n <= 0) return [];
  if (n >= arr.length) return [...arr];
  return [...arr].sort(() => Math.random() - 0.5).slice(0, n);
};

const handleDeleteItem = (item: any) => {
  Modal.confirm({
    title: '确认删除',
    content: `确定要删除博主「${item.name}」所有视频吗？删除后将无法恢复。`,
    okText: '确认删除',
    cancelText: '取消',
    okType: 'danger',
    maskClosable: false,
    onOk: () => {
      return new Promise((resolve, reject) => {
        useApiStore()
          .DeleteByAuthor(item.uperId)
          .then((res) => {
            if (res.code === 0) {
              message.success('根据视频数量，需要时常不确定，可以稍后去日志查看...');
              resolve(true);
            } else {
              message.error('删除博主视频失败' + (res.message || '未知错误'));
              reject(false);
            }
          })
          .catch((err) => {
            console.error('删除博主视频异常', err);
            message.error('删除博主视频异常' + err);
            reject(false);
          });
      });
    },
  });
};
</script>
<style scoped>
/* 基础样式 */
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

/* 核心概览区域 - 调整网格布局适配新增的2个统计项 */
.stats-overview {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 25px;
  margin-bottom: 30px;
}

/* 主卡片样式 - 增强边框可见性 */
.main-card {
  padding: 28px;
  border-radius: 16px;
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
  /* 白天模式添加明显边框 */
  border: 1px solid #e0e0e0;
}

/* 卡片hover效果 */
.main-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 35px rgba(0, 0, 0, 0.12);
  /* hover时边框颜色加深 */
  border-color: #d0d0d0;
}

/* 卡片头部 */
.stat-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 22px;
}

.header-left {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

/* 卡片元数据 */
.stat-meta {
  font-size: 15px;
  color: #666666;
  text-transform: uppercase;
  letter-spacing: 0.6px;
  font-weight: 500;
}

/* 卡片主数值 */
.stat-value {
  font-size: 42px;
  font-weight: 700;
  color: #1a1a1a;
  line-height: 1.1;
  display: flex;
  align-items: baseline;
  gap: 8px;
}

/* 单位样式 */
.unit {
  font-size: 22px;
  color: #444444;
  font-weight: 500;
}

/* 卡片图标 */
.stat-icon {
  width: 60px;
  height: 60px;
  border-radius: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background-color: rgba(76, 175, 80, 0.15);
  color: #4caf50;
  transition: all 0.3s ease;
}

/* 空间卡片图标颜色 */
.secondary-card .stat-icon {
  background-color: rgba(33, 150, 243, 0.15);
  color: #2196f3;
}

/* 卡片hover时图标缩放 */
.main-card:hover .stat-icon {
  transform: scale(1.08);
}

/* 细分项容器 - 增强分隔线，调整网格布局为3列适配6个统计项 */
.stat-subitems {
  display: grid;
  grid-template-columns: repeat(3, 1fr); /* 从2列改为3列，适配新增的2个统计项 */
  gap: 15px;
  padding-top: 20px;
  /* 白天模式使用明显的分隔线 */
  border-top: 1px solid #d0d0d0;
}

/* 细分项样式 - 增强边框和背景 */
.subitem {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px 16px;
  /* 白天模式添加白色背景和明显边框 */
  background: #ffffff;
  border: 1px solid #e0e0e0;
  border-radius: 12px;
  box-shadow: 0 3px 12px rgba(0, 0, 0, 0.04);
  transition: all 0.2s ease;
  cursor: default;
}

/* 细分项hover效果 - 增强边框和阴影 */
.subitem:hover {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.07);
  border-color: #c0c0c0;
  background: #fafafa;
}

/* 细分项图标 */
.subitem-icon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background-color: rgba(233, 30, 99, 0.1);
  color: #e91e63;
}

/* 收藏图标颜色 */
.collect-icon {
  background-color: rgba(255, 152, 0, 0.1);
  color: #ff9800;
}

/* 关注图标颜色 */
.follow-icon {
  background-color: rgba(156, 39, 176, 0.1);
  color: #9c27b0;
}

/* 图文图标颜色 */
.graphic-icon {
  background-color: rgba(255, 159, 64, 0.1);
  color: #d9091a;
}

/* 新增：合集图标样式 */
.mix-icon {
  background-color: rgba(63, 81, 181, 0.1);
  color: #3f51b5;
}

/* 新增：短剧图标样式 */
.series-icon {
  background-color: rgba(0, 188, 212, 0.1);
  color: #00bcd4;
}

/* 细分项元数据 */
.subitem-meta {
  font-size: 13px;
  color: #666666;
  font-weight: 500;
  letter-spacing: 0.3px;
  flex: 1;
}

/* 细分项数值 */
.subitem-value {
  font-size: 16px;
  font-weight: 600;
  color: #222222;
  display: flex;
  align-items: baseline;
  gap: 4px;
}

/* 细分项单位 */
.subitem-value .unit {
  font-size: 12px;
  color: #555555;
  font-weight: 500;
}

/* 卡片顶部主题边框 */
.primary-card {
  border-top: 4px solid #4caf50;
}

.secondary-card {
  border-top: 4px solid #2196f3;
}

/* 详细分类统计区域 */
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

/* 作者卡片样式 - 核心修改：头像+文字横向，进度条独立在下 */
.author-card {
  display: flex;
  flex-direction: column; /* 整体纵向布局，容纳「头像+文字行」和「进度条行」 */
  gap: 10px; /* 头像文字行 与 进度条 之间的间距，可微调 */
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

/* 头像+文字 横向容器 */
.author-info-row {
  display: flex;
  align-items: center; /* 头像与文字垂直居中 */
  gap: 12px; /* 头像与文字的间距，减少空白 */
}

.author-avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  overflow: hidden;
  flex-shrink: 0; /* 防止头像被压缩 */
}

.author-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

/* 作者文字信息（名字+作品数） */
.author-info {
  flex: 1; /* 占据头像右侧剩余空间 */
  display: flex;
  flex-direction: column; /* 名字在上，作品数在下，紧凑排列 */
  gap: 3px; /* 名字与作品数的间距，减少空白 */
}

.author-name,
.category-name {
  margin: 0; /* 移除默认外边距，消除多余空白 */
  font-size: 16px;
  color: #333333;
}

.author-stats,
.category-stats {
  margin: 5px 0px; /* 移除默认外边距 */
  font-size: 12px;
  color: #666666;
  line-height: 1; /* 紧凑行高，减少垂直空白 */
}

/* 进度条 - 独立成行，不与头像同行 */
.author-progress {
  height: 6px;
  background: #e0e0e0;
  border-radius: 3px;
  overflow: hidden;
  width: 100%; /* 占满作者卡片宽度 */
}

.progress-bar {
  height: 100%;
  background: #4caf50;
  border-radius: 3px;
  transition: width 0.5s ease;
}

/* 分类卡片样式 */
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

/* 夜间模式样式 - 保持原有效果 */
html.dark-mode .main-card {
  border-color: rgba(255, 255, 255, 0.1);
  background-color: rgba(30, 30, 50, 0.9);
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.2);
}

html.dark-mode .main-card:hover {
  border-color: rgba(255, 255, 255, 0.15);
  box-shadow: 0 12px 35px rgba(0, 0, 0, 0.25);
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

/* 夜间模式其他样式保持不变 */
html.dark-mode .stats-dashboard {
  background-color: #1a1a2e;
  color: #eaeaea;
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

html.dark-mode .secondary-card .stat-icon {
  background-color: rgba(33, 150, 243, 0.25);
}

html.dark-mode .subitem-meta {
  color: #c0c0d3;
}

html.dark-mode .subitem-value {
  color: #ffffff;
}

html.dark-mode .subitem-value .unit {
  color: #b0b0c3;
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

/* 新增：夜间模式下合集、短剧图标样式 */
html.dark-mode .mix-icon {
  background-color: rgba(63, 81, 181, 0.2);
}

html.dark-mode .series-icon {
  background-color: rgba(0, 188, 212, 0.2);
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
</style>