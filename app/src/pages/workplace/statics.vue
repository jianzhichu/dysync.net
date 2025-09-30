<template>
  <div class="stats-dashboard">
    <main class="main-content">
      <!-- 上方统计卡片（修改为四列布局） -->
      <section class="top-stats">
        <!-- 视频总数-->
        <div class="stat-card video-card">
          <div class="stat-info">
            <p class="stat-label">视频总数</p>
            <h3 class="stat-value">{{ totalVideos }}</h3>
            <p class="stat-trend positive"></p>
          </div>
          <div class="stat-icon">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polygon points="23 7 16 12 23 17 23 7"></polygon>
              <rect x="1" y="5" width="15" height="14" rx="2" ry="2"></rect>
            </svg>
          </div>
        </div>
        <!-- 我喜欢的数量卡片 -->
        <div class="stat-card like-card">
          <div class="stat-info">
            <p class="stat-label">我喜欢的</p>
            <h3 class="stat-value">{{ favoriteCount }}</h3>
            <p class="stat-trend positive"></p>
          </div>
          <div class="stat-icon">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
            </svg>
          </div>
        </div>
        <!-- 我收藏的数量卡片 -->
        <div class="stat-card collect-card">
          <div class="stat-info">
            <p class="stat-label">我收藏的</p>
            <h3 class="stat-value">{{ collectCount }}</h3>
            <p class="stat-trend positive"></p>
          </div>
          <div class="stat-icon">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M19 21l-7-5-7 5V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2z"></path>
            </svg>
          </div>
        </div>

        <!-- 作者总数卡片 -->
        <div class="stat-card author-card">
          <div class="stat-info">
            <p class="stat-label">作者总数</p>
            <h3 class="stat-value">{{ totalAuthors }}</h3>
            <p class="stat-trend positive"></p>
          </div>
          <div class="stat-icon">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
              <circle cx="12" cy="7" r="4"></circle>
            </svg>
          </div>
        </div>

        <!-- 分类总数卡片 -->
        <div class="stat-card cate-card">
          <div class="stat-info">
            <p class="stat-label">分类总数</p>
            <h3 class="stat-value">{{ categoryTotal }}</h3>
            <p class="stat-trend positive"></p>
          </div>
          <div class="stat-icon">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <rect x="2" y="5" width="20" height="14" rx="2" />
              <path d="M17 5v4" />
              <path d="M7 5v4" />
            </svg>
          </div>
        </div>

        <!-- 视频占用空间卡片 -->
        <div class="stat-card size-card">
          <div class="stat-info">
            <p class="stat-label">占用空间</p>
            <h3 class="stat-value">{{ fileSizeTotal }} G</h3>
            <p class="stat-trend positive"></p>
          </div>
          <div class="stat-icon">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <!-- 补充硬盘图标定义（原代码缺失，避免引用错误） -->
              <path d="M22 12H2v8h20v-8z" />
              <path d="M6 18h.01" />
              <path d="M10 18h.01" />
            </svg>
          </div>
        </div>
      </section>

      <!-- 下方统计：调换Tab顺序（视频作者在前，视频分类在后） -->
      <section class="category-stats">
        <!-- Tab切换控制器（调换Tab顺序 + 调整下划线位置） -->
        <div class="tab-switcher" :class="{'currentTab-type': currentTab === 'type'}">
          <!-- 1. 调换Tab项顺序：视频作者 -> 视频分类 -->
          <div class="tab-item" :class="{ active: currentTab === 'author' }" @click="currentTab = 'author'">
            视频作者
          </div>
          <div class="tab-item" :class="{ active: currentTab === 'type' }" @click="currentTab = 'type'">
            视频分类
          </div>
          <div class="tab-underline"></div>
        </div>

        <!-- 内容区域（调换显示顺序，默认显示视频作者） -->
        <transition name="content-fade" mode="out-in">
          <!-- 2. 调换内容顺序：先显示作者统计，再显示分类统计 -->
          <div v-if="currentTab === 'author'" key="author-content" class="tab-content">
            <div class="category-section">
              <div class="grid-container authors-grid">
                <div class="grid-item" v-for="(author, index) in authors" :key="index">
                  <div class="category-icon" style="background-color: #722ed1; display: flex; align-items: center; justify-content: center;">
                    <img :src="author.icon" alt="作者头像" width="60" height="60" style="object-fit: cover; border-radius: 50%;">
                  </div>
                  <h3 class="item-name">{{ author.name }}</h3>
                  <p class="item-count">作品数: {{ author.count }}</p>
                </div>
              </div>
            </div>
          </div>

          <div v-else key="type-content" class="tab-content">
            <div class="category-section">
              <div class="grid-container categories-grid">
                <div class="grid-item" v-for="(category, index) in categories" :key="index" :style="{ '--category-color': category.color }">
                  <div class="category-icon">
                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 12 12" fill="white" stroke="white" stroke-width="2">
                      <use :xlink:href="`#${category.icon}`"></use>
                    </svg>
                  </div>
                  <h3 class="item-name">{{ category.name }}</h3>
                  <p class="item-count">作品数: {{ category.count }}</p>
                </div>
              </div>
            </div>
          </div>
        </transition>
      </section>
    </main>

    <!-- 定义SVG图标 -->
    <svg style="display: none;">
      <!-- 日常用品类 -->
      <symbol id="cup" viewBox="0 0 12 12">
        <path d="M18 4h2v16h-2zM4 4h14v2H4zM4 8h10v2H4zM4 12h10v2H4zM4 16h6v2H4zM4 20h6v2H4z" />
      </symbol>
      <symbol id="spoon" viewBox="0 0 12 12">
        <path d="M18 2c-.55 0-1 .45-1 1v5.59L6.12 20.88c-.39.39-1.02.39-1.41 0-.39-.39-.39-1.02 0-1.41L15.58 8H10c-.55 0-1-.45-1-1s.45-1 1-1h8c.55 0 1 .45 1 1s-.45 1-1 1z" />
      </symbol>
      <symbol id="fork" viewBox="0 0 12 12">
        <path d="M8 3v11h4v-5h4v5h4V3M6 3v16h12V3H6z" />
      </symbol>
      <symbol id="knife" viewBox="0 0 12 12">
        <path d="M4 7h16v2H4zM4 11h13v2H4zM4 15h8v2H4zM7 3v2h10V3z" />
      </symbol>
      <symbol id="plate" viewBox="0 0 12 12">
        <circle cx="12" cy="12" r="8" />
        <circle cx="12" cy="12" r="6" />
      </symbol>
      <symbol id="bottle" viewBox="0 0 12 12">
        <path d="M10 2h4v5h-4zM8 7v15c0 .55.45 1 1 1h6c.55 0 1-.45 1-1V7H8z" />
      </symbol>
      <symbol id="toothbrush" viewBox="0 0 12 12">
        <path d="M11 3h2v11h-2zM5 3h2v15H5zM17 3h2v11h-2z" />
      </symbol>
      <symbol id="comb" viewBox="0 0 12 12">
        <path d="M4 5h16v2H4zM4 9h13v2H4zM4 13h10v2H4zM4 17h8v2H4z" />
      </symbol>
      <symbol id="mirror" viewBox="0 0 12 12">
        <rect x="3" y="3" width="18" height="18" rx="2" />
        <circle cx="12" cy="10" r="3" />
        <path d="M12 15c-2.2 0-4 1.8-4 4h8c0-2.2-1.8-4-4-4z" />
      </symbol>
      <symbol id="soap" viewBox="0 0 12 12">
        <rect x="6" y="6" width="12" height="12" rx="3" />
        <path d="M9 9h6v6H9z" />
      </symbol>

      <!-- 电子设备类 -->
      <symbol id="mobile" viewBox="0 0 12 12">
        <rect x="5" y="2" width="14" height="20" rx="2" ry="2" />
        <line x1="12" y1="18" x2="12.01" y2="18" />
      </symbol>
      <symbol id="tablet" viewBox="0 0 12 12">
        <rect x="4" y="2" width="16" height="20" rx="2" ry="2" />
        <line x1="12" y1="20" x2="12.01" y2="20" />
      </symbol>
      <symbol id="camera" viewBox="0 0 12 12">
        <circle cx="12" cy="12" r="6" />
        <circle cx="12" cy="12" r="3" />
        <rect x="18" y="18" width="4" height="4" />
      </symbol>
      <symbol id="headphones" viewBox="0 0 12 12">
        <path d="M3 10v4c0 2.21 1.79 4 4 4h2c2.21 0 4-1.79 4-4v-4H3zm14 4c0 2.21 1.79 4 4 4v-4h-4z" />
        <path d="M15 10v4c0 2.21-1.79 4-4 4H7c-2.21 0-4-1.79-4-4v-4h12z" />
      </symbol>
      <symbol id="speaker" viewBox="0 0 12 12">
        <path d="M3 9v6h4l5 5V4L7 9H3zm13.5 3c0-1.77-1.02-3.29-2.5-4.03v8.05c1.48-.73 2.5-2.25 2.5-4.02zM14 3.23v2.06c2.89.86 5 3.54 5 6.71s-2.11 5.85-5 6.71v2.06c4.01-.91 7-4.49 7-8.77s-2.99-7.86-7-8.77z" />
      </symbol>
      <symbol id="tv" viewBox="0 0 12 12">
        <rect x="2" y="4" width="20" height="15" rx="2" ry="2" />
        <path d="M20 20H4L2 22h20z" />
      </symbol>
      <symbol id="watch" viewBox="0 0 12 12">
        <circle cx="12" cy="12" r="8" />
        <circle cx="12" cy="12" r="1" />
        <line x1="12" y1="8" x2="12" y2="12" />
        <line x1="12" y1="12" x2="15" y2="14" />
      </symbol>
      <symbol id="charger" viewBox="0 0 12 12">
        <path d="M10 16v-2h4v2h5v2h-5v4h-2v-4H5v-2h5zm11-9h-6V3c0-.55-.45-1-1-1H4c-.55 0-1 .45-1 1v4H1v2h22v-2z" />
      </symbol>
      <symbol id="router" viewBox="0 0 12 12">
        <rect x="4" y="4" width="16" height="16" rx="2" />
        <circle cx="8" cy="8" r="1" />
        <circle cx="16" cy="8" r="1" />
        <circle cx="8" cy="16" r="1" />
        <circle cx="16" cy="16" r="1" />
        <circle cx="12" cy="12" r="1" />
      </symbol>
      <symbol id="printer" viewBox="0 0 12 12">
        <path d="M6 18v2h12v-2H6zM6 14h12v2H6zM19 8H5c-1.1 0-2 .9-2 2v4c0 1.1.9 2 2 2h4v2h-4c-2.21 0-4-1.79-4-4V10c0-2.21 1.79-4 4-4h14c2.21 0 4 1.79 4 4v4c0 2.21-1.79 4-4 4h-4v-2h4c1.1 0 2-.9 2-2V10c0-1.1-.9-2-2-2z" />
      </symbol>

      <!-- 工具类 -->
      <symbol id="axe" viewBox="0 0 12 12">
        <path d="M13 12h7v2h-7zM5 19h14v2H5zM19 3h-4.18C14.4 1.84 13.3 1 12 1c-1.3 0-2.4.84-2.82 2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-7-.25c.41 0 .75.34.75.75s-.34.75-.75.75-.75-.34-.75-.75.34-.75.75-.75zM10 12H3v-2h7v2zm5-6h-2v2H7V6H5v2h7v2h2V6z" />
      </symbol>
      <symbol id="saw" viewBox="0 0 12 12">
        <path d="M3 5h18v2H3zM3 9h16v2H3zM3 13h14v2H3zM3 17h12v2H3z" />
        <path d="M21 19H3v2h18z" />
      </symbol>
      <symbol id="screwdriver" viewBox="0 0 12 12">
        <path d="M7 19h10V5H7v14zm2-8h6v2H9v-2zm0 4h6v2H9v-2zm0-8h6v2H9V7z" />
      </symbol>
      <symbol id="ladder" viewBox="0 0 12 12">
        <path d="M4 3v18h2V5h14V3H4z" />
        <path d="M8 7v10M12 7v10M16 7v10" />
      </symbol>
      <symbol id="flashlight" viewBox="0 0 12 12">
        <rect x="11" y="3" width="2" height="12" />
        <path d="M9 15h6l3 6V9l-3 6z" />
      </symbol>
      <symbol id="tape-measure" viewBox="0 0 12 12">
        <path d="M16 13h-3V3H5v18h6v-3h3v3h6V13zM7 5h5v8H7zm10 14h-5v-8h5z" />
        <path d="M10 18h2v2h-2z" />
      </symbol>
      <symbol id="compass" viewBox="0 0 12 12">
        <circle cx="12" cy="12" r="9" />
        <circle cx="12" cy="12" r="2" />
        <line x1="12" y1="4" x2="12" y2="8" />
        <line x1="12" y1="16" x2="12" y2="20" />
        <line x1="4" y1="12" x2="8" y2="12" />
        <line x1="16" y1="12" x2="20" y2="12" />
      </symbol>
      <symbol id="binoculars" viewBox="0 0 12 12">
        <path d="M11 7h2v2h-2zm0 4h2v6h-2zm1-9C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8z" />
        <path d="M14.85 12.65l-3.79-3.79-1.41 1.42 3.79 3.79z" />
      </symbol>
      <symbol id="magnifying-glass" viewBox="0 0 12 12">
        <circle cx="11" cy="11" r="7" />
        <line x1="21" y1="21" x2="16.65" y2="16.65" />
      </symbol>
      <symbol id="multimeter" viewBox="0 0 12 12">
        <rect x="5" y="5" width="14" height="14" rx="2" />
        <circle cx="12" cy="12" r="3" />
        <line x1="8" y1="8" x2="10" y2="10" />
        <line x1="14" y1="14" x2="16" y2="16" />
      </symbol>

      <!-- 交通类 -->
      <symbol id="car" viewBox="0 0 12 12">
        <path d="M18.92 6.01C18.72 5.42 18.16 5 17.5 5H6.5C5.84 5 5.29 5.42 5.08 6.01L3 12v8c0 .55.45 1 1 1h1c.55 0 1-.45 1-1v-1h12v1c0 .55.45 1 1 1h1c.55 0 1-.45 1-1v-8l-2.08-5.99zM6.5 16c-.83 0-1.5-.67-1.5-1.5S5.67 13 6.5 13s1.5.67 1.5 1.5S7.33 16 6.5 16zm11 0c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5zM5 11l1.5-4.5h11L19 11H5z" />
      </symbol>
      <symbol id="bicycle" viewBox="0 0 12 12">
        <circle cx="5" cy="18" r="3" />
        <circle cx="19" cy="18" r="3" />
        <path d="M10 18c0-1.1-.9-2-2-2H5.83l.69-4.17h2.67l-.5 3h2.75l-.13 1zm5.17-13c-.77 0-1.41.5-1.63 1.22l-3.76 9.03c-.2.48.07 1.01.56 1.22.49.22 1.07-.08 1.27-.56l3.76-9.03C17.64 5.5 16.99 5 16.17 5z" />
      </symbol>
      <symbol id="bus" viewBox="0 0 12 12">
        <path d="M18 10.5V6c0-1.1-.9-2-2-2H4c-1.1 0-1.99.9-1.99 2v12c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2v-4.5l-2-2zm-2-1.5v-2c1.1 0 2 .9 2 2v2h-2zM4 6h12v2H4V6zm14 14H4v-2h14v2zm0-4H4v-2h14v2zm-6-4h2v4h-2z" />
      </symbol>
      <symbol id="train" viewBox="0 0 12 12">
        <rect x="4" y="3" width="16" height="12" rx="2" />
        <rect x="2" y="11" width="2" height="2" />
        <rect x="20" y="11" width="2" height="2" />
        <circle cx="8" cy="19" r="2" />
        <circle cx="16" cy="19" r="2" />
        <path d="M12 15v3" />
      </symbol>
      <symbol id="boat" viewBox="0 0 12 12">
        <path d="M21 14c0 4.97-4.03 9-9 9s-9-4.03-9-9c0-.62.08-1.21.21-1.79L8 16h8l7.79-3.79C20.92 12.79 21 13.38 21 14zM1 13c0 3.87 3.13 7 7 7s7-3.13 7-7H1z" />
      </symbol>
      <symbol id="airplane" viewBox="0 0 12 12">
        <path d="M20.5 19h-17c-.83 0-1.5-.67-1.5-1.5v-11c0-.83.67-1.5 1.5-1.5h17c.83 0 1.5.67 1.5 1.5v11c0 .83-.67 1.5-1.5 1.5zm-16-10c-.28 0-.5-.22-.5-.5s.22-.5.5-.5h16c.28 0 .5.22.5.5s-.22.5-.5.5h-16zm0 3c-.28 0-.5-.22-.5-.5s.22-.5.5-.5h16c.28 0 .5.22.5.5s-.22.5-.5.5h-16zm0 3c-.28 0-.5-.22-.5-.5s.22-.5.5-.5h10c.28 0 .5.22.5.5s-.22.5-.5.5h-10z" />
        <path d="M4.5 16.5l6-3.5 6 3.5" />
      </symbol>
      <symbol id="helicopter" viewBox="0 0 12 12">
        <path d="M18 10c0-1.1-.9-2-2-2h-6c-1.1 0-2 .9-2 2v2H4c-1.1 0-2 .9-2 2v4c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2v-4c0-1.1-.9-2-2-2h-4v-2zM7 18H4v-2h3v2zm13 0h-3v-2h3v2zm0-4H4v-2h16v2z" />
        <path d="M10 6h4v6h-4z" />
      </symbol>
      <symbol id="rocket" viewBox="0 0 12 12">
        <path d="M12 2L4 7l8 5 8-5-8-5zM4 15l8 5 8-5M4 11l8 5 8-5" />
      </symbol>
      <symbol id="ship" viewBox="0 0 12 12">
        <path d="M21 14c0 4.97-4.03 9-9 9s-9-4.03-9-9c0-.62.08-1.21.21-1.79L8 16h8l7.79-3.79C20.92 12.79 21 13.38 21 14zM1 13c0 3.87 3.13 7 7 7s7-3.13 7-7H1z" />
        <path d="M12 10V7c0-1.66-1.34-3-3-3S6 5.34 6 7v3H5v2h14v-2h-1V7c0-1.66-1.34-3-3-3s-3 1.34-3 3v3z" />
      </symbol>
      <symbol id="motorcycle" viewBox="0 0 12 12">
        <circle cx="6" cy="17" r="3" />
        <circle cx="17" cy="16" r="3" />
        <path d="M17 13c-2.76 0-5 2.24-5 5h-2c0-3.87 3.13-7 7-7s7 3.13 7 7-3.13 7-7 7v-2c1.66 0 3-1.34 3-3s-1.34-3-3-3z" />
        <path d="M6 14c-1.66 0-3-1.34-3-3s1.34-3 3-3h2l3 4H6z" />
      </symbol>

      <!-- 食物类 -->
      <symbol id="apple" viewBox="0 0 12 12">
        <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z" />
        <path d="M12 5c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2z" />
      </symbol>
      <symbol id="banana" viewBox="0 0 12 12">
        <path d="M17.5 5c-1.93 0-3.71.78-4.97 2.03l-3.5 3.5C7.78 11.29 7 13.07 7 15c0 1.93 1.57 3.5 3.5 3.5.53 0 1.03-.11 1.5-.31l1.5 1.5c-.63.45-1.37.71-2.16.79v1.5c0 .83.67 1.5 1.5 1.5.92 0 1.73-.47 2.21-1.16l4.43-6.65C20.47 13.23 21 11.42 21 9.5 21 7.01 19.49 5 17.5 5z" />
      </symbol>
      <symbol id="bread" viewBox="0 0 12 12">
        <rect x="4" y="6" width="16" height="12" rx="2" />
        <path d="M6 6v-.5c0-.83.67-1.5 1.5-1.5h9c.83 0 1.5.67 1.5 1.5V6" />
        <path d="M6 18v2h12v-2" />
      </symbol>
      <symbol id="cheese" viewBox="0 0 12 12">
        <path d="M20 3H9v2h11v14h-4v2h6V3zM4 3H3v18h1v-9h3.5c.8 0 1.5-.7 1.5-1.5v-5c0-.8-.7-1.5-1.5-1.5H4V3z" />
        <path d="M6.5 9H4v1h2.5c.3 0 .5.2.5.5v3c0 .3-.2.5-.5.5H4v1h2.5c.8 0 1.5-.7 1.5-1.5v-3c0-.8-.7-1.5-1.5-1.5z" />
      </symbol>
      <symbol id="coffee" viewBox="0 0 12 12">
        <path d="M18 8h1v8h-1zM2 8h16v2H2zM6 14h12v2H6z" />
        <path d="M4.25 5h15.5c.69 0 1.25.56 1.25 1.25v.25c0 .69-.56 1.25-1.25 1.25H4.25C3.56 7.5 3 6.94 3 6.25v-.25C3 5.56 3.56 5 4.25 5z" />
      </symbol>
      <symbol id="pizza" viewBox="0 0 12 12">
        <circle cx="12" cy="12" r="8" />
        <path d="M12 4v16M4 12h16" />
      </symbol>
      <symbol id="ice-cream" viewBox="0 0 12 12">
        <path d="M17 11c.34 0 .67.03 1 .08V6c0-1.1-.9-2-2-2H8c-1.1 0-2 .9-2 2v5.08c.33-.05.66-.08 1-.08 1.66 0 3 1.34 3 3s-1.34 3-3 3c-1.66 0-3-1.34-3-3H4c0 2.76 2.24 5 5 5s5-2.24 5-5c0-1.66-1.34-3-3-3z" />
        <path d="M7 11l5 5 5-5" />
      </symbol>
      <symbol id="hamburger" viewBox="0 0 12 12">
        <rect x="4" y="5" width="16" height="2" />
        <rect x="4" y="11" width="16" height="2" />
        <rect x="4" y="17" width="16" height="2" />
      </symbol>
      <symbol id="carrot" viewBox="0 0 12 12">
        <path d="M12.01 4.05L10.6 3.24c-.4-.15-.84.16-.97.56L7.6 10.23c-.14.41.16.84.57.97l1.41.47 5.29 1.76c.4.13.85-.15.98-.55l2.2-5.06c.13-.39-.16-.83-.56-.96l-4.76-1.6z" />
        <path d="M5.21 12.04l9.05 3.02-2.7 5.88c-.24.52.22 1.11.79 1.11.32 0 .61-.16.79-.41l6.22-13.35c.24-.52-.22-1.11-.79-1.11h-15c-.56 0-1.03.59-.79 1.11l2.71 5.89z" />
      </symbol>
      <symbol id="cake" viewBox="0 0 12 12">
        <path d="M12 3L2 12h3v8h6v-6h2v6h6v-8h3L12 3z" />
        <circle cx="12" cy="8" r="2" />
      </symbol>

      <!-- 运动类 -->
      <symbol id="football" viewBox="0 0 12 12">
        <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8z" />
        <path d="M12 6.5c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2zm0 9c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2zm-4.95-1.6L12 9.05l4.95 6.85-9.9-6.85zm9.9 5.75L12 14.95l-4.95 3.15 9.9-3.15z" />
      </symbol>
      <symbol id="basketball" viewBox="0 0 12 12">
        <circle cx="12" cy="12" r="10" />
        <path d="M2 12h20M12 2v20" />
        <path d="M5.64 5.64l12.72 12.72M5.64 18.36l12.72-12.72" />
      </symbol>
      <symbol id="tennis" viewBox="0 0 12 12">
        <circle cx="12" cy="12" r="6" />
        <circle cx="12" cy="12" r="2" />
        <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8z" />
      </symbol>
      <symbol id="swim" viewBox="0 0 12 12">
        <path d="M20.57 14.86L22 13.43 20.57 12 17 15.57 13.43 12 12 13.43 15.57 17 12 20.57 13.43 22 17 18.43 20.57 22 22 20.57 18.43 17 20.57 14.86zM10 15c0-1.66-1.34-3-3-3-.35 0-.69.07-1 .18V6c0-1.1.9-2 2-2h4c.73 0 1.39.45 1.73 1.03l1.46 3.4L15.5 12l.68 1.53c.01-.05.02-.1.02-.16 0-1.66-1.34-3-3-3zm-1.15-8h-2.9v2.9l2.9-2.9z" />
      </symbol>
      <symbol id="run" viewBox="0 0 12 12">
        <path d="M13.5 5.5c1.1 0 2-.9 2-2s-.9-2-2-2-2 .9-2 2 .9 2 2 2zM9.8 8.9L7 23h2.1l1.8-8 2.1 2v6h2v-7.5l-2.1-2 .6-3C14.8 12 16.8 13 19 13v-2c-1.9 0-3.5-1-4.3-2.4l-1-1.6c-.4-.6-1-1-1.7-1-.3 0-.5.1-.8.1L6 8.3V13h2V9.6l1.8-.7" />
      </symbol>
      <symbol id="bike" viewBox="0 0 12 12">
        <circle cx="5" cy="18" r="3" />
        <circle cx="19" cy="18" r="3" />
        <path d="M10 18c0-1.1-.9-2-2-2H5.83l.69-4.17h2.67l-.5 3h2.75l-.13 1zm5.17-13c-.77 0-1.41.5-1.63 1.22l-3.76 9.03c-.2.48.07 1.01.56 1.22.49.22 1.07-.08 1.27-.56l3.76-9.03C17.64 5.5 16.99 5 16.17 5z" />
      </symbol>
      <symbol id="boxing" viewBox="0 0 12 12">
        <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8z" />
        <path d="M15 8H9v2h6zm-2 4v6h-2v-6z" />
      </symbol>
      <symbol id="golf" viewBox="0 0 12 12">
        <path d="M12 10.9c-1.7 0-3.24.81-4.16 2.14-.42.61-.28 1.45.26 1.87.55.42 1.39.28 1.81-.26.37-.47.74-.96 1.08-1.46.34-.5.7-1 .99-1.37H13c.73 0 1.41-.21 2-.58.59-.37 1.02-.96 1.02-1.64 0-1.11-.9-2.01-2.01-2.01zM5.1 19.1c-.88-.22-1.66-.61-2.3-.98-.19-.11-.4-.03-.51.16-.11.19-.03.4.16.51.72.41 1.56.67 2.45.75.36.03.68-.25.71-.61.03-.36-.25-.68-.61-.71zm15.58-.98c-.71.38-1.5.77-2.38.99-.36.03-.68-.25-.71-.61.03-.36.25-.68.61-.71.88-.22 1.66-.61 2.3-.98.19-.11.4-.03.51.16.11.19.03.4-.16.51z" />
        <circle cx="12" cy="12" r="2" />
      </symbol>
      <symbol id="yoga" viewBox="0 0 12 12">
        <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-1.41 16.09V20h2.67v-1.93c1.71-.36 3.16-1.46 3.27-3.4h1.73c-.1 2.43-1.96 4.42-4.44 4.93V20h2.67v-2h-3.91c-.31-.01-.55-.3-.55-.63v-4.15c0-.31.23-.57.53-.6l3.87-.53c.48-.07.83.39.72.86l-.85 5.27c1.81-.42 3.21-2.08 3.21-4.07 0-2.31-1.91-4.19-4.25-4.19-1.72 0-3.15.85-4.09 2.15l-1.7-1.13c1.2-1.66 3.02-2.68 5.04-2.68 3.41 0 6.16 2.82 6.16 6.33 0 3.52-2.75 6.33-6.16 6.33-1.55 0-2.95-.64-3.9-1.66l-1.4 1.4A8.89 8.89 0 0 0 12 20c4.96 0 9-4.04 9-9s-4.04-9-9-9z" />
      </symbol>
      <symbol id="weight" viewBox="0 0 12 12">
        <circle cx="8" cy="12" r="5" />
        <circle cx="16" cy="12" r="5" />
        <line x1="13" y1="12" x2="11" y2="12" />
      </symbol>

      <!-- 天气类 -->
      <symbol id="sun" viewBox="0 0 12 12">
        <circle cx="12" cy="12" r="4" />
        <line x1="12" y1="1" x2="12" y2="3" />
        <line x1="12" y1="21" x2="12" y2="23" />
        <line x1="4.22" y1="4.22" x2="5.64" y2="5.64" />
        <line x1="18.36" y1="18.36" x2="19.78" y2="19.78" />
        <line x1="1" y1="12" x2="3" y2="12" />
        <line x1="21" y1="12" x2="23" y2="12" />
        <line x1="4.22" y1="19.78" x2="5.64" y2="18.36" />
        <line x1="18.36" y1="5.64" x2="19.78" y2="4.22" />
      </symbol>
      <symbol id="moon" viewBox="0 0 12 12">
        <path d="M12 3c-4.97 0-9 4.03-9 9s4.03 9 9 9c.83 0 1.5-.67 1.5-1.5 0-.39-.15-.74-.39-1.01-.23-.26-.38-.61-.38-.99 0-.83.67-1.5 1.5-1.5H16c2.76 0 5-2.24 5-5 0-4.42-4.03-8-9-8z" />
      </symbol>
      <symbol id="cloud" viewBox="0 0 12 12">
        <path d="M19.35 10.04C18.67 6.59 15.64 4 12 4 9.11 4 6.6 5.64 5.35 8.04 2.34 8.36 0 10.91 0 14c0 3.31 2.69 6 6 6h13c2.76 0 5-2.24 5-5 0-2.64-2.05-4.78-4.65-4.96zM19 18H6c-2.21 0-4-1.79-4-4 0-2.05 1.53-3.76 3.56-3.97l1.07-.11.5-.95C8.08 7.14 9.94 6 12 6c2.62 0 4.88 1.86 5.39 4.43l.3 1.5 1.53.11c1.56.1 2.78 1.41 2.78 2.96 0 1.65-1.35 3-3 3z" />
      </symbol>
      <symbol id="rain" viewBox="0 0 12 12">
        <path d="M16 18v-2h1v2h-1zM8 18v-2h1v2H8zM12 18v-2h1v2h-1z" />
        <path d="M19.07 4.93l-1.41-1.41c-.39-.39-1.02-.39-1.41 0L14 5.59l-1.65-1.66c-.39-.39-1.02-.39-1.41 0l-1.41 1.41c-.39.39-.39 1.02 0 1.41L10.59 9 5.41 3.83c-.39-.39-1.02-.39-1.41 0L2.29 5.25c-.39.39-.39 1.02 0 1.41L8 11.59l-1.65 1.65c-.39.39-.39 1.02 0 1.41l1.41 1.41c.39.39 1.02.39 1.41 0L12 14.41l1.65 1.66c.39.39 1.02.39 1.41 0l1.41-1.41c.39-.39.39-1.02 0-1.41L15.41 9l5.17 5.17c.39.39 1.02.39 1.41 0l1.41-1.41c.39-.39.39-1.02 0-1.41L20.41 7l1.65-1.66c.39-.39.39-1.02 0-1.41zM13 13h-2v-2h2v2z" />
      </symbol>
      <symbol id="snow" viewBox="0 0 12 12">
        <path d="M12 18c-3.31 0-6-2.69-6-6s2.69-6 6-6 6 2.69 6 6-2.69 6-6 6zm0-10c-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4-1.79-4-4-4z" />
        <path d="M7 19h2v2H7zM15 19h2v2h-2zM7 5h2v2H7zM15 5h2v2h-2z" />
      </symbol>
      <symbol id="wind" viewBox="0 0 12 12">
        <path d="M20 12H4M12 20l-8-8 8-8M16 16l-4-4 4-4" />
      </symbol>
      <symbol id="storm" viewBox="0 0 12 12">
        <path d="M12 7c2.76 0 5 2.24 5 5h2c0-3.87-3.13-7-7-7s-7 3.13-7 7h2c0-2.76 2.24-5 5-5zM12 17h-2v-2h2v2zm0-4h-2V7h2v6zm8 4h-2v-2h2v2zm0-4h-2V7h2v6z" />
        <path d="M19.07 4.93l-1.41-1.41c-.39-.39-1.02-.39-1.41 0L14 5.59l-1.65-1.66c-.39-.39-1.02-.39-1.41 0l-1.41 1.41c-.39.39-.39 1.02 0 1.41L10.59 9 5.41 3.83c-.39-.39-1.02-.39-1.41 0L2.29 5.25c-.39.39-.39 1.02 0 1.41L8 11.59l-1.65 1.65c-.39.39-.39 1.02 0 1.41l1.41 1.41c.39.39 1.02.39 1.41 0L12 14.41l1.65 1.66c.39.39 1.02.39 1.41 0l1.41-1.41c.39-.39.39-1.02 0-1.41L15.41 9l5.17 5.17c.39.39 1.02.39 1.41 0l1.41-1.41c.39-.39.39-1.02 0-1.41L20.41 7l1.65-1.66c.39-.39.39-1.02 0-1.41z" />
      </symbol>
      <symbol id="fog" viewBox="0 0 12 12">
        <path d="M20 18h2v2h-2zM1 18h2v2H1zm13 0h2v2h-2zm6-8h2v2h-2zM1 10h2v2H1zm20-8H3c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h18c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 16H3V4h18v14zM6 15c0-3.87 3.13-7 7-7s7 3.13 7 7H6z" />
      </symbol>
      <symbol id="thermometer" viewBox="0 0 12 12">
        <path d="M15 13V5c0-1.66-1.34-3-3-3S9 3.34 9 5v8c-1.21.91-2 2.37-2 4 0 2.76 2.24 5 5 5s5-2.24 5-5c0-1.63-.79-3.09-2-4zm-3-9c.55 0 1 .45 1 1v6h-2V5c0-.55.45-1 1-1z" />
        <path d="M12 19c-1.66 0-3-1.34-3-3 0-2 3-5.4 3-5.4s3 3.4 3 5.4c0 1.66-1.34 3-3 3z" />
      </symbol>
      <symbol id="umbrella" viewBox="0 0 12 12">
        <path d="M9 16c0 1.1.9 2 2 2s2-.9 2-2H9zm3-14C8.14 2 5 5.14 5 9c0 2.38 1.19 4.47 3 5.74V17c0 .55.45 1 1 1h6c.55 0 1-.45 1-1v-2.26c1.81-1.27 3-3.36 3-5.74 0-3.86-3.14-7-7-7z" />
      </symbol>

      <!-- 教育类 -->
      <symbol id="pencil" viewBox="0 0 12 12">
        <path d="M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z" />
      </symbol>
      <symbol id="notebook" viewBox="0 0 12 12">
        <path d="M18 2H6c-1.1 0-2 .9-2 2v16c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zM6 4h5v8l-2.5-1.5L6 12V4z" />
      </symbol>
      <symbol id="calculator" viewBox="0 0 12 12">
        <path d="M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-5 14H7v-2h7v2zm3-4H7v-2h10v2zm0-4H7V7h10v2z" />
      </symbol>
      <symbol id="ruler" viewBox="0 0 12 12">
        <path d="M3 3v18h18V3H3zm16 16H5V5h14v14zM7 7h2v2H7zm0 4h2v2H7zm0 4h2v2H7zm4-8h2v2h-2zm0 4h2v2h-2zm0 4h2v2h-2zm4-8h2v2h-2zm0 4h2v2h-2z" />
      </symbol>
      <symbol id="glasses" viewBox="0 0 12 12">
        <path d="M6.5 20c.83 0 1.5-.67 1.5-1.5v-11c0-.83-.67-1.5-1.5-1.5S5 6.67 5 7.5v11c0 .83.67 1.5 1.5 1.5zm11 0c.83 0 1.5-.67 1.5-1.5v-11c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v11c0 .83.67 1.5 1.5 1.5zM16 16c0-1.11-.9-2-2-2-.29 0-.62.02-.97.05 1.16-.84 1.97-1.97 1.97-3.39V7c0-2.76-2.24-5-5-5S7 4.24 7 7v3.66c0 1.42.81 2.55 1.97 3.39-.35-.03-.68-.05-.97-.05-1.1 0-2 .89-2 2s.9 2 2 2c1.1 0 2-.89 2-2h4c0 1.11.9 2 2 2s2-.89 2-2z" />
      </symbol>
      <symbol id="graduation-cap" viewBox="0 0 12 12">
        <path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5" />
      </symbol>
      <symbol id="bookmark" viewBox="0 0 12 12">
        <path d="M19 21l-7-3-7 3V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v16z" />
      </symbol>
      <symbol id="microscope" viewBox="0 0 12 12">
        <path d="M18 6v6c0 1.1-.9 2-2 2h-2v4l-4-4H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2h12c1.1 0 2 .9 2 2zM7 14l4 4 4-4H7z" />
        <circle cx="12" cy="8" r="2" />
        <circle cx="12" cy="8" r="1" />
      </symbol>
      <symbol id="telescope" viewBox="0 0 12 12">
        <path d="M21 5H3c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h18c1.1 0 2-.9 2-2V7c0-1.1-.9-2-2-2zM3 7h2v10H3V7zm18 10h-2V7h2v10zM12 12c1.1 0 2-.9 2-2s-.9-2-2-2-2 .9-2 2 .9 2 2 2zm0-3c.55 0 1 .45 1 1s-.45 1-1 1-1-.45-1-1 .45-1 1-1z" />
        <path d="M16.5 8.5l2.5 2.5-2.5 2.5M7.5 8.5l-2.5 2.5 2.5 2.5" />
      </symbol>
      <symbol id="globe" viewBox="0 0 12 12">
        <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8z" />
        <path d="M12 4c-4.41 0-8 3.59-8 8s3.59 8 8 8v-2c-3.31 0-6-2.69-6-6s2.69-6 6-6z" />
        <path d="M12 6v6l4 2.34" />
      </symbol>

      <!-- 医疗类 -->
      <symbol id="heart" viewBox="0 0 12 12">
        <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z" />
      </symbol>
      <symbol id="stethoscope" viewBox="0 0 12 12">
        <circle cx="19" cy="10" r="2" />
        <path d="M21 8c0-2.76-2.24-5-5-5s-5 2.24-5 5H5c-1.1 0-2 .9-2 2v6c0 1.1.9 2 2 2h1v3c0 .55.45 1 1 1h6c.55 0 1-.45 1-1v-3h5c1.1 0 2-.9 2-2v-6c0-1.1-.9-2-2-2zM7 15H5v-4h2v4zm4 0H9v-4h2v4zm4 0h-2v-4h2v4zm4 0h-5v-4h5v4z" />
      </symbol>
      <symbol id="pill" viewBox="0 0 12 12">
        <path d="M2.5 17c.83 0 1.5-.67 1.5-1.5v-9c0-.83-.67-1.5-1.5-1.5S1 5.67 1 6.5v9c0 .83.67 1.5 1.5 1.5zm4-13c.83 0 1.5.67 1.5 1.5v13c0 .83-.67 1.5-1.5 1.5S6 18.33 6 17.5v-13C6 4.67 6.67 4 7.5 4zm14 13c.83 0 1.5-.67 1.5-1.5v-9c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v9c0 .83.67 1.5 1.5 1.5zm-4-13c.83 0 1.5.67 1.5 1.5v13c0 .83-.67 1.5-1.5 1.5s-1.5-.67-1.5-1.5v-13c0-.83.67-1.5 1.5-1.5z" />
      </symbol>
      <symbol id="bandage" viewBox="0 0 12 12">
        <rect x="5" y="5" width="14" height="14" rx="2" />
        <rect x="9" y="7" width="2" height="10" />
        <rect x="13" y="7" width="2" height="10" />
        <rect x="7" y="9" width="10" height="2" />
        <rect x="7" y="13" width="10" height="2" />
      </symbol>
      <symbol id="syringe" viewBox="0 0 12 12">
        <path d="M20 7h-4V3c0-.55-.45-1-1-1H9c-.55 0-1 .45-1 1v4H4c-.55 0-1 .45-1 1v11c0 .55.45 1 1 1h16c.55 0 1-.45 1-1V8c0-.55-.45-1-1-1zM18 18H6V8h5V5h2v3h5v10z" />
        <path d="M8 15h8v2H8z" />
      </symbol>
      <symbol id="thermometer-medical" viewBox="0 0 12 12">
        <path d="M13 5c0-1.66-1.34-3-3-3S7 3.34 7 5v8c-1.21.91-2 2.37-2 4 0 2.76 2.24 5 5 5s5-2.24 5-5c0-1.63-.79-3.09-2-4V5zm-1 16c-1.66 0-3-1.34-3-3 0-1.31-.69-2.5-1.76-3.24l.04-.06 4.95-4.95.06.04c.73 1.05 1.93 1.75 3.22 1.76 1.66 0 3 1.34 3 3s-1.34 3-3 3h-2z" />
      </symbol>
      <symbol id="first-aid" viewBox="0 0 12 12">
        <path d="M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-5 14H7v-2h7v2zm3-4H7v-2h10v2zm0-4H7V7h10v2z" />
      </symbol>
      <symbol id="eyeglasses" viewBox="0 0 12 12">
        <path d="M6.5 20c.83 0 1.5-.67 1.5-1.5v-11c0-.83-.67-1.5-1.5-1.5S5 6.67 5 7.5v11c0 .83.67 1.5 1.5 1.5zm11 0c.83 0 1.5-.67 1.5-1.5v-11c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v11c0 .83.67 1.5 1.5 1.5zM16 16c0-1.11-.9-2-2-2-.29 0-.62.02-.97.05 1.16-.84 1.97-1.97 1.97-3.39V7c0-2.76-2.24-5-5-5S7 4.24 7 7v3.66c0 1.42.81 2.55 1.97 3.39-.35-.03-.68-.05-.97-.05-1.1 0-2 .89-2 2s.9 2 2 2c1.1 0 2-.89 2-2h4c0 1.11.9 2 2 2s2-.89 2-2z" />
      </symbol>
      <symbol id="heartbeat" viewBox="0 0 12 12">
        <path d="M17.5 12c0 2.48-2.02 4.5-4.5 4.5S8.5 14.48 8.5 12H10c0 1.38 1.12 2.5 2.5 2.5s2.5-1.12 2.5-2.5h1.5zM12 22c1.52 0 2.73-.47 3.69-1.24.96-.77 1.58-1.89 1.79-3.16.13-.76-.31-1.49-.99-1.62-.67-.13-1.29.39-1.62.99-.25.5-.57.97-1.01 1.38L12 18l-.66-.66c-.44-.41-.76-.88-1.01-1.38-.33-.6-.95-1.12-1.62-.99-.68.13-1.12.86-.99 1.62.21 1.27.83 2.39 1.79 3.16C9.27 21.53 10.48 22 12 22zM12 2C8.14 2 5 5.14 5 9c0 .88.24 1.72.66 2.45.15.25.13.61-.06.81l-1.43 1.79c-.25.31-.68.36-.96.11-.28-.25-.29-.66-.02-.96l1.2-1.54c.4-.5.92-.93 1.5-1.26.58-.33 1.2-.53 1.84-.61V9c0-2.76 2.24-5 5-5s5 2.24 5 5v.28c.64.08 1.26.28 1.84.61.58.33 1.1.76 1.5 1.26l1.2 1.54c.27.3.26.71-.02.96-.28.25-.71.2-1.01-.11l-1.43-1.79c-.19-.2-.21-.56-.06-.81.42-.73.66-1.57.66-2.45 0-3.86-3.14-7-7-7z" />
      </symbol>
      <symbol id="wheelchair" viewBox="0 0 12 12">
        <circle cx="5" cy="19" r="2" />
        <circle cx="17" cy="19" r="2" />
        <path d="M18 13c0-3.87-3.13-7-7-7S4 9.13 4 13v-2c0-3.31 2.69-6 6-6s6 2.69 6 6v2h2v-2z" />
      </symbol>

      <!-- 自然类 -->
      <symbol id="tree" viewBox="0 0 12 12">
        <path d="M17 12h-5v5h5v-5zM16 1v2H8V1H6v2H5c-1.11 0-1.99.9-1.99 2L3 19c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2h-1V1h-2zm3 18H5V8h14v11z" />
      </symbol>
      <symbol id="flower" viewBox="0 0 12 12">
        <circle cx="12" cy="12" r="3" />
        <path d="M12 2c-2.21 0-4 1.79-4 4 0 .89.29 1.71.78 2.38l-2.47 2.47c-1.12.37-1.81 1.47-1.81 2.71 0 1.93 1.57 3.5 3.5 3.5.59 0 1.17-.1 1.7-.31l2.47 2.47c.67.49 1.49.78 2.38.78 2.21 0 4-1.79 4-4 0-.89-.29-1.71-.78-2.38l2.47-2.47c1.12-.37 1.81-1.47 1.81-2.71 0-1.93-1.57-3.5-3.5-3.5-.59 0-1.17.1-1.7.31l-2.47-2.47C13.71 2.29 12.89 2 12 2zm0 1.5c1.38 0 2.5 1.12 2.5 2.5 0 .52-.16.99-.44 1.38l.44.44c.74.23 1.42.57 2.01 1.06l1.06-1.06c.49-.59.83-1.27 1.06-2.01l.44-.44c.39-.28.86-.44 1.38-.44 1.38 0 2.5 1.12 2.5 2.5 0 .73-.2 1.42-.54 2.01l-2.64 2.64c.34.59.54 1.28.54 2.01 0 1.38-1.12 2.5-2.5 2.5-.73 0-1.42-.2-2.01-.54l-2.64 2.64c-.59-.34-1.28-.54-2.01-.54-1.38 0-2.5-1.12-2.5-2.5 0-.73.2-1.42.54-2.01L3.46 11.5c-.34-.59-.54-1.28-.54-2.01 0-1.38 1.12-2.5 2.5-2.5.73 0 1.42.2 2.01.54l2.64-2.64c.59.34 1.28.54 2.01.54z" />
      </symbol>
      <symbol id="leaf" viewBox="0 0 12 12">
        <path d="M17 7h-4v4H7v4h4v4h4v-4h4v-4h-4V7z" />
        <path d="M3 11h2v2H3zm0 4h2v2H3zm14-4h2v2h-2zm0 4h2v2h-2z" />
      </symbol>
      <symbol id="mountain" viewBox="0 0 12 12">
        <path d="M13 2v8h8c0-4.42-3.58-8-8-8zm6.32 13.89C20.37 14.54 21 12.84 21 11H6.44l-.95-2H2v2h2.22s1.89 4.07 2.12 4.42c-1.1.59-1.84 1.75-1.84 3.08C4.5 21.43 6.07 23 8 23c1.76 0 3.22-1.3 3.46-3h2.08c.24 1.7 1.7 3 3.46 3 1.93 0 3.5-1.57 3.5-3.5 0-1.04-.46-1.97-1.18-2.61z" />
      </symbol>
      <symbol id="river" viewBox="0 0 12 12">
        <path d="M20 20c-1.39 0-2.76-.57-3.68-1.56C16.56 17.76 17 16.39 17 15c0-3.87-3.13-7-7-7s-7 3.13-7 7c0 1.39.44 2.76 1.44 3.68C5.76 20.43 7.11 21 8.5 21s2.74-.57 3.65-1.56c.91.99 2.28 1.56 3.65 1.56 1.39 0 2.74-.57 3.66-1.56.9.99 2.27 1.56 3.66 1.56 1.38 0 2.73-.57 3.64-1.56-.91-.99-2.27-1.56-3.64-1.56zM4 15c0-2.76 2.24-5 5-5s5 2.24 5 5-2.24 5-5 5-5-2.24-5-5zm16 0c0-2.76-2.24-5-5-5s-5 2.24-5 5 2.24 5 5 5 5-2.24 5-5z" />
      </symbol>
      <symbol id="star" viewBox="0 0 12 12">
        <path d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z" />
      </symbol>
      <symbol id="cactus" viewBox="0 0 12 12">
        <rect x="11" y="3" width="2" height="10" />
        <rect x="7" y="7" width="2" height="4" />
        <rect x="15" y="7" width="2" height="4" />
        <rect x="9" y="13" width="6" height="8" rx="2" />
      </symbol>
      <symbol id="fire" viewBox="0 0 12 12">
        <path d="M12.5 4.2c.88.7 1.5 1.76 1.5 2.95 0 2.21-1.79 4-4 4-.34 0-.67-.03-1-.08.41 1.21 1.52 2.16 2.89 2.54.27.08.45.3.45.59v1.66c0 .38-.31.7-.7.7-1.22-.07-2.38-.57-3.28-1.37-.43-.41-.68-.99-.68-1.61 0-1.78 1.46-3.22 3.25-3.22.88 0 1.67.39 2.16 1 .5.61.75 1.39.75 2.21 0 .16-.01.31-.03.46-.39-.25-.81-.4-1.26-.4-.91 0-1.67.7-1.67 1.58 0 .87.76 1.58 1.67 1.58 1.38 0 2.5-.93 2.63-2.23.24-2.6-.99-4.97-3.03-6.36zM5.21 12c.41 1.21 1.52 2.16 2.89 2.54.27.08.45.3.45.59v1.66c0 .38-.31.7-.7.7-1.22-.07-2.38-.57-3.28-1.37-.43-.41-.68-.99-.68-1.61 0-1.78 1.46-3.22 3.25-3.22.88 0 1.67.39 2.16 1 .5.61.75 1.39.75 2.21 0 .16-.01.31-.03.46-.39-.25-.81-.4-1.26-.4-.91 0-1.67.7-1.67 1.58 0 .87.76 1.58 1.67 1.58 1.38 0 2.5-.93 2.63-2.23.24-2.6-.99-4.97-3.03-6.36 0 0-.53-.42-1.27-.42-.89 0-1.35.67-1.35 1.49 0 .43.14.84.41 1.19-.15.03-.31.04-.46.04-2.03 0-3.68-1.65-3.68-3.68 0-.21.02-.41.05-.61.37.41.86.66 1.39.66.59 0 1.11-.29 1.45-.76-.1-.04-.19-.08-.29-.12C1.46 3.93 1 5.81 1 7.81c0 2.63 2.1 4.77 4.73 4.81h.48zM18.79 12c.41 1.21 1.52 2.16 2.89 2.54.27.08.45.3.45.59v1.66c0 .38-.31.7-.7.7-1.22-.07-2.38-.57-3.28-1.37-.43-.41-.68-.99-.68-1.61 0-1.78 1.46-3.22 3.25-3.22.88 0 1.67.39 2.16 1 .5.61.75 1.39.75 2.21 0 .16-.01.31-.03.46-.39-.25-.81-.4-1.26-.4-.91 0-1.67.7-1.67 1.58 0 .87.76 1.58 1.67 1.58 1.38 0 2.5-.93 2.63-2.23.24-2.6-.99-4.97-3.03-6.36 0 0-.53-.42-1.27-.42-.89 0-1.35.67-1.35 1.49 0 .43.14.84.41 1.19-.15.03-.31.04-.46.04-2.03 0-3.68-1.65-3.68-3.68 0-.21.02-.41.05-.61.37.41.86.66 1.39.66.59 0 1.11-.29 1.45-.76-.1-.04-.19-.08-.29-.12-1.09-.53-1.55-1.7-1.55-2.91 0-2.63 2.1-4.77 4.73-4.81h.48z" />
      </symbol>
      <symbol id="moon-star" viewBox="0 0 12 12">
        <path d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z" />
        <path d="M12 3c-4.97 0-9 4.03-9 9s4.03 9 9 9c.83 0 1.5-.67 1.5-1.5 0-.39-.15-.74-.39-1.01-.23-.26-.38-.61-.38-.99 0-.83.67-1.5 1.5-1.5H16c2.76 0 5-2.24 5-5 0-4.42-4.03-8-9-8z" />
      </symbol>
      <symbol id="wave" viewBox="0 0 12 12">
        <path d="M2 12c0 2.76 2.24 5 5 5h10c2.76 0 5-2.24 5-5s-2.24-5-5-5H7c-2.76 0-5 2.24-5 5zm19-3h-5c-1.66 0-3 1.34-3 3s1.34 3 3 3h5c1.66 0 3-1.34 3-3s-1.34-3-3-3zm0 5h-5c-.55 0-1-.45-1-1s.45-1 1-1h5c.55 0 1 .45 1 1s-.45 1-1 1zM7 10c.55 0 1 .45 1 1s-.45 1-1 1H2c-.55 0-1-.45-1-1s.45-1 1-1h5zm12 2c0-.55-.45-1-1-1H8c-.55 0-1 .45-1 1s.45 1 1 1h10c.55 0 1-.45 1-1z" />
      </symbol>
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

// 3. 初始Tab改为"视频作者"（currentTab默认值从'type'改为'author'）
const totalVideos = ref<number>(0);
const totalAuthors = ref<number>(0);
const categoryTotal = ref<number>(0);
const fileSizeTotal = ref<number>(0);
const favoriteCount = ref<number>(0);
const collectCount = ref<number>(0);
const categories = ref<Category[]>([]);
const authors = ref<Author[]>([]);
const currentTab = ref<string>('author'); // 默认显示"视频作者"Tab

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
    fileSizeTotal.value = res.data.viedoSizeTotal; // 注意：原代码"video"拼写错误（viedo），建议修正为"videoSizeTotal"
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
  'router',
  'printer',
]);

const colorArray = [
  '#3A7CA5',
  '#D9A566',
  '#6B4226',
  '#829399',
  '#A63446',
  '#4F6367',
  '#EEF4ED',
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
.stats-dashboard {
  min-height: 100vh;
  background-color: #361f68;
  padding: 0 20px;
  box-sizing: border-box;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans',
    'Helvetica Neue', sans-serif;
  color: #ffffff;
}

.main-content {
  margin: 0 auto;
  padding: 20px 0;
  /* 增加最大宽度，避免四列在大屏下过宽 max-width: 1400px; */
}

/* 1. 顶部统计卡片：修改为四列布局（核心修改） */
.top-stats {
  display: grid;
  grid-template-columns: 1fr;
  gap: 20px;
  margin-bottom: 30px;
}

/* 平板及以上屏幕：四列布局（原3列改为4列） */
@media (min-width: 768px) {
  .top-stats {
    grid-template-columns: repeat(6, 1fr); /* 关键修改：3 -> 4 */
  }
}

/* 小屏适配：保持1列，避免拥挤 */
@media (max-width: 767px) {
  .stat-card .stat-value {
    font-size: 24px; /* 小屏缩小数值字体 */
  }
}

/* 统计卡片基础样式（不变） */
.stat-card {
  background-color: rgba(255, 255, 255, 0.05);
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  display: flex;
  justify-content: space-between;
  align-items: center;
  transition: transform 0.2s, box-shadow 0.2s, background-color 0.2s;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);
  background-color: rgba(255, 255, 255, 0.1);
}

/* 卡片图标样式（不变） */
.video-card .stat-icon {
  background-color: rgba(24, 144, 255, 0.15);
  color: #1890ff;
}

.author-card .stat-icon {
  background-color: rgba(114, 46, 209, 0.15);
  color: #722ed1;
}

.cate-card .stat-icon {
  background-color: rgba(194, 238, 51, 0.15);
  color: hsl(66, 91%, 49%);
}

.size-card .stat-icon {
  background-color: rgba(34, 211, 238, 0.15);
  color: #22d3ee;
}
.like-card .stat-icon {
  background-color: rgba(227, 19, 234, 0.15);
  color: #ef09c5;
}

.collect-card .stat-icon {
  background-color: rgba(237, 103, 6, 0.15);
  color: #b95406;
}
.stat-info .stat-label {
  font-size: 14px;
  color: rgba(255, 255, 255, 0.8);
  margin: 0 0 8px 0;
}

.stat-info .stat-value {
  font-size: 28px;
  font-weight: 600;
  margin: 0 0 8px 0;
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* 下方Tab区域（核心修改：调整下划线位置） */
.category-stats {
  display: grid;
  grid-template-columns: 1fr;
  gap: 30px;
}

.tab-switcher {
  position: relative;
  display: flex;
  width: 240px;
  margin: 0 auto 20px;
  border-radius: 4px;
  overflow: hidden;
}

.tab-item {
  flex: 1;
  padding: 12px 0;
  text-align: center;
  font-size: 16px;
  font-weight: 500;
  color: rgba(255, 255, 255, 0.7);
  cursor: pointer;
  transition: color 0.3s ease;
}

.tab-item.active {
  color: #ffffff; /* 激活态文字变白，增强辨识度 */
}

/* 2. Tab下划线：调整初始位置（对应"视频作者"Tab） */
.tab-underline {
  position: absolute;
  bottom: 0;
  height: 3px;
  width: 60px; /* 与文字宽度匹配 */
  background-color: #722ed1;
  border-radius: 3px 3px 0 0;
  transition: all 0.3s ease;
  left: 0;
  transform: translateX(30px); /* 初始位置：对应第一个Tab（视频作者） */
}

/* 切换到"视频分类"Tab时，下划线位置调整 */
.currentTab-type .tab-underline {
  transform: translateX(150px); /* 对应第二个Tab（视频分类） */
}

/* 内容切换动画（不变） */
.content-fade-enter-from,
.content-fade-leave-to {
  opacity: 0;
  transform: translateY(10px);
}

.content-fade-enter-active,
.content-fade-leave-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}

/* 分类/作者列表样式（不变） */
.category-section {
  background-color: rgba(255, 255, 255, 0.05);
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.grid-container {
  display: grid;
  grid-template-columns: repeat(1, 1fr);
  gap: 16px;
}

@media (min-width: 576px) {
  .grid-container {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (min-width: 768px) {
  .grid-container {
    grid-template-columns: repeat(4, 1fr);
  }
}

@media (min-width: 1200px) {
  .grid-container {
    grid-template-columns: repeat(6, 1fr);
  }
}

.grid-item {
  background-color: rgba(255, 255, 255, 0.02);
  border-radius: 6px;
  padding: 16px;
  text-align: center;
  transition: transform 0.2s, box-shadow 0.2s, background-color 0.2s;
  cursor: pointer;
}

.grid-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
  background-color: rgba(255, 255, 255, 0.05);
}

.category-icon {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  margin: 0 auto 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.categories-grid .category-icon {
  background-color: var(--category-color);
}

.item-name {
  font-size: 16px;
  font-weight: 500;
  margin: 0 0 8px 0;
}

.item-count {
  font-size: 14px;
  color: rgba(255, 255, 255, 0.8);
  margin: 0;
}
</style>