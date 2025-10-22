<template>
  <div class="stats-dashboard">
    <div class="dashboard-container">
      <!-- 页面标题 -->

      <!-- 核心统计概览 -->
      <section class="stats-overview">
        <div class="stat-card primary-card">
          <div class="stat-header">
            <span class="stat-meta">总视频数</span>
            <div class="stat-icon video-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <polygon points="23 7 16 12 23 17 23 7"></polygon>
                <rect x="1" y="5" width="15" height="14" rx="2" ry="2"></rect>
              </svg>
            </div>
          </div>
          <div class="stat-value">{{ totalVideos }}</div>
          <div class="stat-trend">
            <!-- <span class="trend-up">较上月 +5.2%</span> -->
          </div>
        </div>

        <div class="stat-card secondary-card">
          <div class="stat-header">
            <span class="stat-meta">占用空间</span>
            <div class="stat-icon size-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M22 12H2v8h20v-8z" />
                <path d="M6 18h.01" />
                <path d="M10 18h.01" />
              </svg>
            </div>
          </div>
          <div class="stat-value">{{ fileSizeTotal }} G</div>
          <div class="stat-trend">
            <!-- <span class="trend-up">较上月 +12.8%</span> -->
          </div>
        </div>
      </section>

      <!-- 次级统计卡片组 -->
      <section class="stats-grid">
        <!-- 我喜欢的数量卡片 -->
        <div class="stat-card mini-card">
          <div class="stat-header">
            <span class="stat-meta">我喜欢的</span>
            <div class="stat-icon like-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
              </svg>
            </div>
          </div>
          <div class="stat-value">{{ favoriteCount }}</div>
        </div>

        <!-- 我收藏的数量卡片 -->
        <div class="stat-card mini-card">
          <div class="stat-header">
            <span class="stat-meta">我收藏的</span>
            <div class="stat-icon collect-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M19 21l-7-5-7 5V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2z"></path>
              </svg>
            </div>
          </div>
          <div class="stat-value">{{ collectCount }}</div>
        </div>

        <!-- 作者总数卡片 -->
        <div class="stat-card mini-card">
          <div class="stat-header">
            <span class="stat-meta">作者总数</span>
            <div class="stat-icon author-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                <circle cx="12" cy="7" r="4"></circle>
              </svg>
            </div>
          </div>
          <div class="stat-value">{{ totalAuthors }}</div>
        </div>

        <!-- 分类总数卡片 -->
        <div class="stat-card mini-card">
          <div class="stat-header">
            <span class="stat-meta">分类总数</span>
            <div class="stat-icon cate-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <rect x="2" y="5" width="20" height="14" rx="2" />
                <path d="M17 5v4" />
                <path d="M7 5v4" />
              </svg>
            </div>
          </div>
          <div class="stat-value">{{ categoryTotal }}</div>
        </div>
      </section>

      <!-- 详细分类统计 -->
      <section class="detailed-stats">
        <div class="stats-header">
          <h2>详细统计</h2>
          <div class="tab-controls">
            <button class="tab-btn" :class="{ active: currentTab === 'author' }" @click="currentTab = 'author'">
              视频作者
            </button>
            <button class="tab-btn" :class="{ active: currentTab === 'type' }" @click="currentTab = 'type'">
              视频分类
            </button>
          </div>
        </div>

        <transition name="stats-fade" mode="out-in">
          <!-- 作者统计 -->
          <div v-if="currentTab === 'author'" key="author-view" class="stats-content">
            <div class="authors-grid">
              <div class="author-card" v-for="(author, index) in authors" :key="index">
                <div class="author-avatar">
                  <img :src="author.icon" alt="作者头像" />
                </div>
                <div class="author-info">
                  <h3 class="author-name">{{ author.name }}</h3>
                  <p class="author-stats">作品数: {{ author.count }}</p>
                </div>
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
      <!-- 所有图标定义保持不变 -->
      <symbol id="cup" viewBox="0 0 12 12">
        <path d="M18 4h2v16h-2zM4 4h14v2H4zM4 8h10v2H4zM4 12h10v2H4zM4 16h6v2H4zM4 20h6v2H4z" />
      </symbol>
      <!-- 其他图标省略，保持原样 -->
    </svg>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from 'vue';
import { useApiStore } from '@/store';

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
const fileSizeTotal = ref<number>(0);
const favoriteCount = ref<number>(0);
const collectCount = ref<number>(0);
const categories = ref<Category[]>([]);
const authors = ref<Author[]>([]);
const currentTab = ref<string>('author');

// 组件名称
defineOptions({
  name: 'StatsDashboard',
});

// 加载数据
onMounted(() => {
  loadDashboardData();
});

const loadDashboardData = async () => {
  try {
    const res = await useApiStore().VideoStatics();
    totalVideos.value = res.data.videoCount;
    totalAuthors.value = res.data.authorCount;
    categoryTotal.value = res.data.categoryCount;
    favoriteCount.value = res.data.favoriteCount;
    collectCount.value = res.data.collectCount;
    categories.value = res.data.categories;
    fileSizeTotal.value = res.data.viedoSizeTotal; // 修复拼写错误
    authors.value = res.data.authors;

    // 分类图标和颜色随机分配
    const cates = getRandomElements(categoriessss.value, categories.value.length);
    const colors = getRandomElements(colorArray, categories.value.length);
    categories.value.forEach((item, index) => {
      item.icon = cates[index];
      item.color = colors[index];
    });
  } catch (err) {
    console.error('加载仪表盘数据失败：', err);
  }
};

// 随机获取数组元素方法
const getRandomElements = (arr: any[], n: number) => {
  if (n <= 0) return [];
  if (n >= arr.length) return [...arr];
  return [...arr].sort(() => Math.random() - 0.5).slice(0, n);
};

// 图标列表和颜色数组
const categoriessss = ref<any[]>([
  'cup',
  'spoon',
  'fork',
  'knife',
  'plate',
  'bottle',
  'toothbrush',
  'comb',
  'mirror',
  'soap',
  'mobile',
  'tablet',
  'camera',
  'headphones',
  'speaker',
  'tv',
  'watch',
  'charger',
]);

const colorArray = [
  '#3A7CA5',
  '#D9A566',
  '#6B4226',
  '#829399',
  '#A63446',
  '#4F6367',
  '#FE5F55',
  '#C7EFCF',
  '#78C0E0',
  '#49BEAA',
  '#49DCB1',
  '#FF9999',
  '#66B2FF',
  '#99FF99',
];
</script>

<style scoped>
/* 白天模式默认样式（无dark-mode类时生效） */
.stats-dashboard {
  min-height: 100vh;
  background-color: #ffffff; /* 白天背景：白色 */
  color: #333333; /* 白天文本：深灰 */
  font-family: 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  padding: 20px 0;
}

/* 加宽整体宽度并减少两侧留白 */
.dashboard-container {
  max-width: 1400px; /* 从1200px加宽至1600px */
  margin: 0 auto;
  padding: 0 15px; /* 两侧留白从20px减至15px */
}

/* 响应式适配：避免宽屏溢出 */
@media (max-width: 1700px) {
  .dashboard-container {
    max-width: 95%; /* 中等屏幕用百分比控制宽度 */
  }
}

/* 头部样式 */
.dashboard-header {
  margin-bottom: 30px;
  padding-bottom: 15px;
  border-bottom: 1px solid #e0e0e0; /* 白天边框：浅灰 */
}

.dashboard-header h1 {
  font-size: 28px;
  margin: 0 0 10px 0;
  color: #333333; /* 白天标题：深灰 */
}

.header-subtitle {
  font-size: 16px;
  color: #666666; /* 白天副标题：中灰 */
  margin: 0;
}

/* 统计卡片样式（增强白天阴影） */
.stat-card {
  background: #f5f5f5; /* 白天卡片背景：浅灰 */
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); /* 增强阴影 */
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.15); /* 增强hover阴影 */
}

.stat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.stat-meta {
  font-size: 14px;
  color: #666666; /* 白天元数据：中灰 */
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.stat-icon {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.video-icon {
  background-color: rgba(76, 175, 80, 0.2);
  color: #4caf50;
}

.size-icon {
  background-color: rgba(33, 150, 243, 0.2);
  color: #2196f3;
}

.like-icon {
  background-color: rgba(233, 30, 99, 0.2);
  color: #e91e63;
}

.collect-icon {
  background-color: rgba(255, 152, 0, 0.2);
  color: #ff9800;
}

.author-icon {
  background-color: rgba(156, 39, 176, 0.2);
  color: #9c27b0;
}

.cate-icon {
  background-color: rgba(0, 188, 212, 0.2);
  color: #00bcd4;
}

.stat-value {
  font-size: 32px;
  font-weight: 700;
  margin: 0 0 10px 0;
  color: #333333;
  line-height: 1;
}

.stat-trend {
  font-size: 13px;
}

.trend-up {
  color: #4caf50;
  display: flex;
  align-items: center;
}

.trend-up::before {
  content: '↑';
  margin-right: 4px;
}

/* 概览区域布局 */
.stats-overview {
  display: grid;
  grid-template-columns: 1fr;
  gap: 20px;
  margin-bottom: 30px;
}

@media (min-width: 768px) {
  .stats-overview {
    grid-template-columns: 2fr 1fr;
  }
}

.primary-card {
  border-top: 4px solid #4caf50;
}

.secondary-card {
  border-top: 4px solid #2196f3;
}

/* 次级统计网格布局 */
.stats-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 15px;
  margin-bottom: 30px;
}

@media (min-width: 576px) {
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (min-width: 768px) {
  .stats-grid {
    grid-template-columns: repeat(4, 1fr);
  }
}

.mini-card .stat-value {
  font-size: 24px;
}

/* 详细统计区域 */
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

.stats-header h2 {
  margin: 0;
  font-size: 20px;
  color: #333333;
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
  border-radius: 20px;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.3s ease;
}

.tab-btn.active {
  background: rgba(76, 175, 80, 0.2);
  color: #4caf50;
  font-weight: 500;
}

.tab-btn:hover:not(.active) {
  background: rgba(0, 0, 0, 0.05);
  color: #333333;
}

/* 统计内容区域 */
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
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (min-width: 992px) {
  .authors-grid,
  .categories-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

/* 作者卡片（增强白天阴影） */
.author-card {
  background: #eeeeee;
  border-radius: 8px;
  padding: 15px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08); /* 新增阴影 */
}

.author-card:hover {
  transform: translateX(5px);
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.12); /* 增强hover阴影 */
}

.author-avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  overflow: hidden;
}

.author-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.author-info {
  flex: 1;
}

.author-name {
  margin: 0 0 5px 0;
  font-size: 16px;
  color: #333333;
}

.author-stats {
  margin: 0;
  font-size: 13px;
  color: #666666;
}

.author-progress {
  height: 6px;
  background: #e0e0e0;
  border-radius: 3px;
  overflow: hidden;
}

.progress-bar {
  height: 100%;
  background: #4caf50;
  border-radius: 3px;
  transition: width 0.5s ease;
}

/* 分类卡片（增强白天阴影） */
.category-card {
  background: #eeeeee;
  border-radius: 8px;
  padding: 15px;
  display: flex;
  align-items: center;
  gap: 15px;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08); /* 新增阴影 */
}

.category-card:hover {
  transform: translateX(5px);
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.12); /* 增强hover阴影 */
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

.category-info {
  flex: 1;
}

.category-name {
  margin: 0 0 3px 0;
  font-size: 16px;
  color: #333333;
}

.category-stats {
  margin: 0;
  font-size: 13px;
  color: #666666;
}

.category-percentage {
  font-size: 14px;
  font-weight: 500;
  color: var(--category-color);
}

/* 动画效果 */
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

/* 晚上模式样式 */
html.dark-mode .stats-dashboard {
  background-color: #1a1a2e;
  color: #eaeaea;
}

html.dark-mode .dashboard-header {
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

html.dark-mode .dashboard-header h1 {
  color: #ffffff;
}

html.dark-mode .header-subtitle {
  color: #b0b0c3;
}

html.dark-mode .stat-card {
  background: rgba(30, 30, 50, 0.8);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.2);
}

html.dark-mode .stat-card:hover {
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.3);
}

html.dark-mode .stat-meta {
  color: #b0b0c3;
}

html.dark-mode .stat-value {
  color: #ffffff;
}

html.dark-mode .detailed-stats {
  background: rgba(30, 30, 50, 0.8);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.2);
}

html.dark-mode .stats-header h2 {
  color: #ffffff;
}

html.dark-mode .tab-btn {
  color: #b0b0c3;
}

html.dark-mode .tab-btn:hover:not(.active) {
  background: rgba(255, 255, 255, 0.05);
  color: #ffffff;
}

html.dark-mode .author-card {
  background: rgba(40, 40, 65, 0.6);
  box-shadow: none;
}

html.dark-mode .author-name {
  color: #ffffff;
}

html.dark-mode .author-stats {
  color: #b0b0c3;
}

html.dark-mode .author-progress {
  background: rgba(255, 255, 255, 0.1);
}

html.dark-mode .category-card {
  background: rgba(40, 40, 65, 0.6);
  box-shadow: none;
}

html.dark-mode .category-name {
  color: #ffffff;
}

html.dark-mode .category-stats {
  color: #b0b0c3;
}
</style>