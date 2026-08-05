<template>
  <div class="stats-dashboard-mobile">
    <div class="dashboard-container">
      <!-- 看板 Tab 内容 -->
      <div v-if="activeTab === 'dashboard'" class="tab-content">
        <section class="stats-overview">
          <!-- 合并后的总视频数和空间总计卡片 -->
          <div class="stat-card primary-card main-card">
            <div class="stat-header">
              <div class="header-left">
                <span class="stat-meta">视频统计</span>
                <div class="stat-value-container">
                  <div class="summary-stat-item">
                    <span class="summary-stat-label">视频总数</span>
                    <div class="stat-value video-count">
                      {{ totalVideos }}
                      <span class="value-label">个</span>
                    </div>
                  </div>

                  <div class="summary-stat-divider"></div>

                  <div class="summary-stat-item">
                    <span class="summary-stat-label">存储总计</span>
                    <div class="stat-value size-count">
                      {{ fileSizeTotal }}
                      <span class="value-label">G</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="stat-subitems">
              <div class="subitem" :title="`我喜欢的视频数: ${favoriteCount} (占用: ${favoriteSize}G)`">
                <div class="subitem-icon like-icon">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
                  </svg>
                </div>
                <span class="subitem-meta">我喜欢的 <span class="subitem-count">({{ favoriteCount }})</span></span>
                <span class="subitem-size">{{ favoriteSize }}G</span>
              </div>
              <div class="subitem" :title="`我收藏的视频数: ${collectCount} (占用: ${collectSize}G)`">
                <div class="subitem-icon collect-icon">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M19 21l-7-5-7 5V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2z"></path>
                  </svg>
                </div>
                <span class="subitem-meta">我收藏的 <span class="subitem-count">({{ collectCount }})</span></span>
                <span class="subitem-size">{{ collectSize }}G</span>
              </div>
              <div class="subitem" :title="`我关注的视频数: ${followCount} (占用: ${followSize}G)`">
                <div class="subitem-icon follow-icon">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                    <circle cx="8.5" cy="7" r="4"></circle>
                    <line x1="20" y1="8" x2="20" y2="14"></line>
                    <line x1="23" y1="11" x2="17" y2="11"></line>
                  </svg>
                </div>
                <span class="subitem-meta">我关注的 <span class="subitem-count">({{ followCount }})</span></span>
                <span class="subitem-size">{{ followSize }}G</span>
              </div>
              <div class="subitem" :title="`图文视频数: ${graphicVideoCount} (占用: ${graphicVideoSize}G)`">
                <div class="subitem-icon graphic-icon">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
                    <circle cx="8.5" cy="8.5" r="1.5"></circle>
                    <polyline points="21 15 16 10 5 21"></polyline>
                  </svg>
                </div>
                <span class="subitem-meta">图文视频 <span class="subitem-count">({{ graphicVideoCount }})</span></span>
                <span class="subitem-size">{{ graphicVideoSize }}G</span>
              </div>

              <!-- 新增：合集视频统计项 -->
              <div class="subitem" :title="`合集视频数: ${mixCount || 0} (占用: ${videoMixSize}G)`">
                <div class="subitem-icon mix-icon">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M8 3H5a2 2 0 0 0-2 2v3m18 0V5a2 2 0 0 0-2-2h-3m0 18h3a2 2 0 0 0 2-2v-3M3 16v3a2 2 0 0 0 2 2h3"></path>
                  </svg>
                </div>
                <span class="subitem-meta">合集视频 <span class="subitem-count">({{ mixCount || 0 }})</span></span>
                <span class="subitem-size">{{ videoMixSize }}G</span>
              </div>
              <!-- 新增：短剧视频统计项 -->
              <div class="subitem" :title="`短剧视频数: ${seriesCount || 0} (占用: ${videoSeriesSize}G)`">
                <div class="subitem-icon series-icon">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <rect x="2" y="2" width="20" height="20" rx="2.18" ry="2.18"></rect>
                    <line x1="7" y1="2" x2="7" y2="22"></line>
                    <line x1="17" y1="2" x2="17" y2="22"></line>
                    <line x1="2" y1="12" x2="22" y2="12"></line>
                    <line x1="2" y1="7" x2="7" y2="7"></line>
                    <line x1="2" y1="17" x2="7" y2="17"></line>
                    <line x1="17" y1="17" x2="22" y2="17"></line>
                    <line x1="17" y1="7" x2="22" y2="7"></line>
                  </svg>
                </div>
                <span class="subitem-meta">短剧视频 <span class="subitem-count">({{ seriesCount || 0 }})</span></span>
                <span class="subitem-size">{{ videoSeriesSize }}G</span>
              </div>
            </div>
          </div>

          <!-- 最新同步视频列表 -->
          <div class="stat-card secondary-card main-card">
            <div class="stat-header">
              <div class="header-left">
                <span class="stat-meta">最新同步</span>
              </div>
            </div>
            <div class="stat-subitems video-list-container">
              <div v-for="(video, index) in topVideos" :key="index" class="subitem video-item" :title="video.title">
                <div class="video-info">
                  <span class="subitem-meta video-title" @click="openVideoPlayer(video)">{{ video.title }}</span>
                  <span class="subitem-value video-time">{{ video.time }}</span>
                </div>
              </div>
              <div v-if="topVideos.length === 0" class="empty-video-list">
                暂无热门视频数据
              </div>
            </div>
          </div>
        </section>
      </div>

      <!-- 日志 Tab 内容 -->
      <div v-if="activeTab === 'log'" class="tab-content log-tab">
        <div class="log-hero">
          <div class="log-hero-copy">
            <h2 class="log-page-title">系统日志</h2>
            <p class="log-page-subtitle">查看同步任务运行记录与异常信息</p>
          </div>
          <div class="log-total-badge">
            <strong>{{ logs.length }}</strong>
            <span>条记录</span>
          </div>
        </div>

        <div class="log-filter-shell">
          <div class="log-filter">
            <button class="filter-btn" :class="{ active: logFilter === 'all' }" @click="logFilter = 'all'">
              全部 <span>{{ logs.length }}</span>
            </button>
            <button class="filter-btn" :class="{ active: logFilter === 'debug' }" @click="logFilter = 'debug'">
              DEBUG <span>{{ debugLogCount ?? 0 }}</span>
            </button>
            <button class="filter-btn" :class="{ active: logFilter === 'error' }" @click="logFilter = 'error'">
              ERROR <span>{{ errorLogCount ?? 0 }}</span>
            </button>
          </div>
        </div>

        <div class="log-list">
          <div v-for="(log, index) in filteredLogs" :key="log.id || index" class="log-item" :class="getLogType(log).toLowerCase()" @click="openLogDetail(log)">
            <div class="log-item-icon">
              <svg v-if="getLogType(log) === 'ERROR'" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path>
                <line x1="12" y1="9" x2="12" y2="13"></line>
                <line x1="12" y1="17" x2="12.01" y2="17"></line>
              </svg>
              <svg v-else xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="12" cy="12" r="9"></circle>
                <line x1="12" y1="8" x2="12" y2="12"></line>
                <line x1="12" y1="16" x2="12.01" y2="16"></line>
              </svg>
            </div>

            <div class="log-item-main">
              <div class="log-item-header">
                <span class="log-type" :class="getLogType(log).toLowerCase()">{{ getLogType(log) || 'UNKNOWN' }}</span>
                <span class="log-time">{{ formatShowDate(log?.date) }}</span>
              </div>
              <div class="log-file">{{ log.fileName }}</div>
              <div v-if="log.message" class="log-message-preview">{{ log.message }}</div>
            </div>

            <div class="log-item-arrow">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
                <polyline points="9 18 15 12 9 6"></polyline>
              </svg>
            </div>
          </div>

          <div v-if="filteredLogs.length === 0" class="empty-log">
            <div class="empty-log-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                <polyline points="14 2 14 8 20 8"></polyline>
                <line x1="9" y1="13" x2="15" y2="13"></line>
                <line x1="9" y1="17" x2="15" y2="17"></line>
              </svg>
            </div>
            <strong>暂无日志</strong>
            <span>当前没有{{ logFilter === 'all' ? '' : logFilter.toUpperCase() }}类型记录</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 新增：关注列表 Tab 内容（移动端专属重构） -->
    <div v-if="activeTab === 'follow'" class="tab-content follow-tab">
      <div class="follow-hero">
        <div class="follow-hero-copy">
          <h2 class="follow-page-title">关注管理</h2>
          <p class="follow-page-subtitle">管理博主同步状态与保存策略</p>
        </div>
        <div class="follow-count-badge">
          <strong>{{ currentTabData.length }}</strong>
          <span>位博主</span>
        </div>
      </div>

      <!-- 搜索+操作栏：移动端单行布局，适配小屏 -->
      <div class="follow-header">
        <!-- <a-select v-model:value="filterStatus" class="follow-filter-select" placeholder="筛选同步状态" @change="handleFilterChange">
          <a-option value="all">全部</a-option>
          <a-option value="openSync">已开启同步</a-option>
          <a-option value="fullSync">全量同步</a-option>
          <a-option value="closeSync">未开启同步</a-option>
        </a-select> -->
        <div class="search-wrapper">
          <a-input v-model:value="quaryData.followUserName" placeholder="搜索博主名称或抖音号" allow-clear @pressEnter="handleSearch" @search="handleSearch" class="follow-search-input" />
        </div>
        <div class="follow-actions">

          <a-button type="primary" class="follow-btn sync-btn" @click="handleSyncAll" :disabled="isSyncDisabled" title="立即同步">
            <SyncOutlined />
          </a-button>
        </div>
      </div>

      <!-- Tab导航：移动端横向滚动，适配多Tab -->
      <div class="follow-tab-wrapper">
        <div class="follow-tab-scroll">
          <a-tabs v-model:value="activeFollowTabKey" type="line" class="follow-custom-tabs" @change="handleTabChange">
            <a-tab-pane v-for="tab in tabList" :key="tab.key" :tab="`${tab.name}(${tab.total || 0})`" />
          </a-tabs>
        </div>
      </div>

      <!-- 关注列表：移动端单列卡片，滚动加载 -->
      <div class="follow-list-container" @scroll="handleScroll">
        <!-- 博主卡片：移动端专属布局【修复DOM结构】 -->
        <a-card v-for="(item, index) in currentTabData" :key="item.id" :data-key="item.id" class="follow-card" :bordered="false" :hoverable="true" :class="{ 'no-followed-card': item.isNoFollowed }">
          <div class="follow-card-inner">
            <!-- 顶部：头像+名称+操作【修复flex布局】 -->
            <div class="follow-card-top">
              <div class="avatar-wrapper">
                <a-avatar shape="circle" size="large" :src="item.uperAvatar" v-if="item.uperAvatar" />
                <a-avatar shape="circle" size="large" v-else class="avatar-placeholder">
                  {{ item.uperName?.charAt(0) || '#' }} <!-- 增加空值处理 -->
                </a-avatar>
              </div>
              <div class="name-actions">
                <div class="name-wrapper">
                  <div class="uper-title-line">
                    <h3 class="uper-name">{{ truncateText(item.uperName, 10) || '未知博主' }}</h3>
                    <span v-if="item.douyinNo" class="douyin-no">{{ item.douyinNo }}</span>
                  </div>
                  <span v-if="item.isNoFollowed" class="no-followed-badge">非关注</span>

                </div>
                <div class="top-actions">
                  <span class="sync-state-text" :class="{ enabled: item.openSync }">
                    <i></i>{{ item.openSync ? '同步中' : '未同步' }}
                  </span>
                  <!-- 删除按钮：仅非关注显示 -->
                  <a-button type="text" class="delete-btn" v-if="item.isNoFollowed" @click="(e) => { e.stopPropagation(); handleDeleteItem(item); }" :disabled="item.isSaving" title="删除">
                    <DeleteOutlined />
                  </a-button>
                </div>
              </div>
            </div>

            <!-- 签名：移动端单行截断【增加强制显示】 -->
            <div class="follow-card-desc" v-if="item.signature && item.signature.trim()">
              <a-tooltip placement="top" :title="item.signature">
                <span class="signature-text">{{ truncateText(item.signature, 25) }}</span>
              </a-tooltip>
            </div>

            <!-- 路径、开启和全量同步保持同一行 -->
            <div class="follow-card-bottom">
              <div class="path-area">
                <template v-if="item.isEditing">
                  <div class="edit-input-group">
                    <a-input v-model:value="item.savePath" placeholder="默认用博主名" @keypress.enter="() => handleSavePath(item)" maxlength="20" :disabled="item.isSaving" class="path-input" />
                    <a-button type="text" @click="() => handleSavePath(item)" :disabled="item.isSaving">
                      <SaveOutlined />
                    </a-button>
                  </div>
                </template>
                <template v-else>
                  <span class="path-text" :class="{ 'path-empty': !item.savePath }">
                    {{ item.savePath || '默认用博主名字' }}
                  </span>
                  <a-button type="text" @click="() => handleEditPath(item)" :disabled="item.isSaving" title="编辑保存路径" class="edit-btn">
                    <EditOutlined />
                  </a-button>
                </template>
              </div>

              <div class="follow-switches">
                <div class="enable-sync-area">
                  <span class="enable-sync-label">开启</span>
                  <a-switch v-model:checked="item.openSync" size="small" @change="(checked) => handleSwitchChange(item, checked)" :disabled="item.isSaving" />
                </div>

                <div v-if="item.openSync" class="full-sync-area">
                  <span class="full-sync-label">全量</span>
                  <a-switch v-model:checked="item.fullSync" size="small" @change="(checked) => handleSyncChange(item, checked)" :disabled="item.isSaving" />
                </div>
              </div>
            </div>
          </div>
        </a-card>

        <!-- 原有加载/空状态代码不变 -->
        <div v-if="loading" class="loading-container">
          <a-spin size="middle" />
          <span class="loading-text">加载中...</span>
        </div>
        <div v-if="noMoreData && followData.length > 0" class="no-more-container">暂无更多</div>
        <div v-if="followData.length === 0 && !loading" class="empty-container">
          <Empty description="暂无关注博主" />
        </div>
      </div>
    </div>

    <!-- 底部导航：新增【关注】Tab，改为三分栏 -->
    <div class="bottom-nav">
      <div class="nav-item" :class="{ active: activeTab === 'dashboard' }" @click="activeTab = 'dashboard'">
        <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polygon points="3 11 12 2 21 11 12 20 3 11"></polygon>
          <line x1="12" y1="2" x2="12" y2="20"></line>
        </svg>
        <span>看板</span>
      </div>
      <div class="nav-item" :class="{ active: activeTab === 'follow' }" @click="handleSwitchToFollow">
        <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
          <circle cx="8.5" cy="7" r="4"></circle>
          <line x1="20" y1="8" x2="20" y2="14"></line>
          <line x1="23" y1="11" x2="17" y2="11"></line>
        </svg>
        <span>关注</span>
      </div>
      <div class="nav-item" :class="{ active: activeTab === 'log' }" @click="activeTab = 'log'">
        <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
          <polyline points="14 2 14 8 20 8"></polyline>
          <line x1="16" y1="13" x2="8" y2="13"></line>
          <line x1="16" y1="17" x2="8" y2="17"></line>
          <polyline points="10 9 9 9 8 9"></polyline>
        </svg>
        <span>日志</span>
      </div>

    </div>

    <!-- 日志详情弹窗 -->
    <div v-if="showLogModal" class="log-modal-mask" @click="closeLogModal">
      <div class="log-modal-content" @click.stop>
        <div class="log-modal-header">
          <div class="header-left">
            <span class="modal-type-tag" :class="getLogType(currentLog ?? undefined).toLowerCase()">
              {{ getLogType(currentLog ?? undefined) || 'UNKNOWN' }}
            </span>
            <span class="modal-date">{{ formatShowDate(currentLog?.date) }}</span>
          </div>
          <button class="close-btn" @click="closeLogModal">
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"></line>
              <line x1="6" y1="6" x2="18" y2="18"></line>
            </svg>
          </button>
        </div>

        <div class="log-modal-body">
          <pre class="log-content">{{ logContent || '加载日志内容中...' }}</pre>
        </div>

        <div class="log-modal-footer">
          <button class="copy-btn" @click="copyLogContent" :disabled="!logContent">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="margin-right: 6px;">
              <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
              <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
            </svg>
            复制
          </button>
        </div>
      </div>
    </div>

    <!-- 视频播放弹窗 -->
    <div v-if="showVideoPlayer" class="video-modal-mask" @click="closeVideoPlayer">
      <div class="video-modal-content" @click.stop>
        <button class="video-close-btn floating-close-btn" @click="closeVideoPlayer">
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#ffffff" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18"></line>
            <line x1="6" y1="6" x2="18" y2="18"></line>
          </svg>
        </button>

        <div class="video-modal-body">
          <div v-if="videoLoading" class="video-loading">
            <div class="loading-spinner"></div>
            <p>加载视频中...</p>
          </div>
          <div v-else-if="videoError" class="video-error">
            <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#f44336" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="10"></circle>
              <line x1="12" y1="8" x2="12" y2="12"></line>
              <line x1="12" y1="16" x2="12.01" y2="16"></line>
            </svg>
            <p>视频加载失败</p>
            <button class="retry-btn" @click="loadVideo(currentVideo)">重试</button>
          </div>
          <div v-else class="video-player-wrapper">
            <video ref="videoPlayerRef" class="video-player" controls autoplay playsinline :src="videoPlayUrl" @error="handleVideoError" @pause="onVideoPause" @play="onVideoPlay" width="100%" height="100%" preload="auto" @loadedmetadata="handleVideoLoadedMetadata">
              您的浏览器不支持HTML5视频播放
            </video>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted, onUnmounted, UnwrapRef, reactive, nextTick, watch } from 'vue';
import { message, Spin, Empty, Tooltip, Modal, Form, FormInstance } from 'ant-design-vue';
import { useApiStore } from '@/store';
import {
  CloseOutlined,
  SearchOutlined,
  PlusOutlined,
  SyncOutlined,
  SaveOutlined,
  EditOutlined,
  DeleteOutlined,
} from '@ant-design/icons-vue';

// ========== 原有仪表盘类型定义 ==========
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
interface LogItem {
  id: string;
  type: 'DEBUG' | 'ERROR';
  time: string;
  message: string;
  file: string;
  date: string;
  fileName: string;
}
interface TopVideoItem {
  title: string;
  time: string;
  id: string;
}

// ========== 关注列表新增类型定义 ==========
interface TabItem {
  key: string;
  name: string;
  total?: number;
}
interface FollowItem {
  id: string;
  mySelfId: string;
  uperName: string;
  enterprise: string;
  signature: string;
  uperAvatar: string;
  fullSync: boolean;
  openSync: boolean;
  savePath?: string;
  isEditing: boolean;
  isSaving?: boolean;
  uperId?: string;
  douyinNo?: string;
  isNoFollowed: boolean;
}
interface QuaryParam {
  pageIndex: number;
  pageSize: number;
  followUserName: string | null;
  mySelfId?: string;
}
// ========== 原有仪表盘状态 ==========
const totalVideos = ref<number>(0);
const fileSizeTotal = ref<string>('0.00');
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
const activeTab = ref<string>('dashboard');
const logFilter = ref<string>('all');
const logs = ref<LogItem[]>([]);
const showLogModal = ref<boolean>(false);
const currentLog = ref<LogItem | null>(null);
const logContent = ref<string>('');
const topVideos = ref<TopVideoItem[]>([]);
const showVideoPlayer = ref<boolean>(false);
const currentVideo = ref<TopVideoItem | null>(null);
const videoPlayUrl = ref<string>('');
const videoLoading = ref<boolean>(false);
const videoError = ref<boolean>(false);
const videoPlayerRef = ref<HTMLVideoElement | null>(null);
const isVideoPaused = ref<boolean>(false);
const normalizeLogType = (type?: string): string => {
  const value = String(type ?? '')
    .trim()
    .toUpperCase();

  if (value === 'ERR' || value.includes('ERROR') || value.includes('错误') || value.includes('异常')) {
    return 'ERROR';
  }

  if (value === 'DBG' || value.includes('DEBUG') || value.includes('调试')) {
    return 'DEBUG';
  }

  return value;
};

const getLogType = (log?: Partial<LogItem> & Record<string, any>): string => {
  const directType = normalizeLogType(log?.type ?? log?.logType ?? log?.level ?? log?.logLevel);

  if (directType === 'DEBUG' || directType === 'ERROR') {
    return directType;
  }

  // 有些接口类型字段为空或带异常格式，文件名仍可准确区分日志类型。
  const fileIdentity = String(log?.fileName ?? log?.file ?? '')
    .trim()
    .toUpperCase();

  if (fileIdentity.includes('ERROR')) {
    return 'ERROR';
  }

  if (fileIdentity.includes('DEBUG')) {
    return 'DEBUG';
  }

  return directType;
};

const debugLogCount = computed(() =>
  logs.value.reduce((count, log) => count + (getLogType(log) === 'DEBUG' ? 1 : 0), 0)
);

const errorLogCount = computed(() =>
  logs.value.reduce((count, log) => count + (getLogType(log) === 'ERROR' ? 1 : 0), 0)
);

const filteredLogs = computed(() => {
  if (logFilter.value === 'all') {
    return logs.value;
  }

  const targetType = logFilter.value.toUpperCase();

  return logs.value.filter((log) => getLogType(log) === targetType);
});

// ========== 关注列表状态 ==========
const tabList = ref<TabItem[]>([]);
const activeFollowTabKey = ref('');
const followData = ref<FollowItem[]>([]);
const loading = ref(false);
const noMoreData = ref(false);
const hasMore = ref(true);
const isSyncDisabled = ref(false);
const isAddDisabled = ref(false);

// 搜索参数
const quaryData: UnwrapRef<QuaryParam> = reactive({
  pageIndex: 0,
  pageSize: 10, // 移动端每页加载10条，更友好
  followUserName: null,
  mySelfId: '',
});
// 计算当前Tab数据
const currentTabData = computed(() => {
  return followData.value.filter((item) => item.mySelfId === activeFollowTabKey.value);
});

// 组件名称
defineOptions({ name: 'StatsDashboardMobile' });

// ========== 生命周期 ==========
onMounted(() => {
  // 加载仪表盘数据
  loadDashboardData();
  loadLogData();
  TopVideo();
  // 初始化关注列表参数
  quaryData.pageIndex = 0;
  // 监听滚动（关注列表）
  const followContainer = document.querySelector('.follow-list-container');
  if (followContainer) {
    followContainer.addEventListener('scroll', handleScroll);
  }
});

onUnmounted(() => {
  // 移除关注列表滚动监听
  const followContainer = document.querySelector('.follow-list-container');
  if (followContainer) {
    followContainer.removeEventListener('scroll', handleScroll);
  }
});

// 监听关注Tab切换，初始化数据
watch(
  () => activeTab.value,
  (newVal) => {
    if (newVal === 'follow' && tabList.value.length === 0) {
      GetCookies().then(() => {
        if (activeFollowTabKey.value) {
          quaryData.mySelfId = activeFollowTabKey.value;
          GetFollows(true);
        }
      });
    }
  }
);

const loadDashboardData = async () => {
  try {
    const res = await useApiStore().VideoStatics();
    totalVideos.value = res.data.videoCount;
    fileSizeTotal.value = res.data.videoSizeTotal || '0.00';

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
  } catch (err) {
    console.error('加载仪表盘数据失败：', err);
  }
};
const loadLogData = async () => {
  try {
    useApiStore()
      .MobileLogs()
      .then((res) => {
        if (res.code !== 0) {
          logs.value = [];
          return;
        }

        const sourceLogs = Array.isArray(res.data) ? res.data : [];

        // 数据进入页面时统一规范日志类型，避免显示为 ERROR，
        // 但数量统计因大小写、空格或字段名不同而得到 0。
        logs.value = sourceLogs.map((item: Record<string, any>) => {
          const normalizedItem = {
            ...item,
            type: getLogType(item),
          };

          return normalizedItem as LogItem;
        });

        if (import.meta.env.DEV) {
          console.debug(
            '[MobileLogs] 类型统计',
            logs.value.map((item) => ({
              fileName: item.fileName,
              rawType:
                (item as Record<string, any>).logType ??
                (item as Record<string, any>).level ??
                (item as Record<string, any>).logLevel ??
                item.type,
              normalizedType: getLogType(item),
            }))
          );
        }
      })
      .catch((err) => {
        logs.value = [];
        console.error('加载日志数据失败：', err);
      });
  } catch (err) {
    logs.value = [];
    console.error('加载日志数据失败：', err);
  }
};
const TopVideo = () => {
  useApiStore()
    .TopVideo(10)
    .then((res) => {
      if (res.code == 0) topVideos.value = res.data;
    });
};
const openLogDetail = (log: LogItem) => {
  currentLog.value = log;
  showLogModal.value = true;
  loadLogDetailContent(log);
};
const closeLogModal = () => {
  showLogModal.value = false;
  currentLog.value = null;
  logContent.value = '';
};
const loadLogDetailContent = async (log: LogItem) => {
  try {
    const requestParams = `${getLogType(log).toLowerCase()}/${log.date}`;
    const logContentStr = await useApiStore().apiGetLogs(requestParams);
    const formattedLog = formatLogTime(String(logContentStr ?? ''));
    logContent.value = formattedLog.split('\n').reverse().join('\n');
  } catch (err) {
    console.error('加载日志详情失败：', err);
    logContent.value = '日志内容加载失败，请重试';
  }
};
const formatShowDate = (dateStr?: string) => {
  if (!dateStr || dateStr.length !== 8) return dateStr || '';
  return `${dateStr.slice(0, 4)}-${dateStr.slice(4, 6)}-${dateStr.slice(6, 8)}`;
};
const formatLogTime = (logContent: string) => {
  const timeRegex = /\d{4}-\d{2}-\d{2} (\d{2}:\d{2}:\d{2})\.\d+ \+08:00/g;
  return logContent.replace(timeRegex, '$1');
};
const copyLogContent = () => {
  if (!logContent.value) return message.warn('暂无日志内容可复制');
  if (navigator.clipboard) {
    navigator.clipboard
      .writeText(logContent.value)
      .then(() => message.success('复制成功'))
      .catch(() => fallbackCopyTextToClipboard(logContent.value));
  } else {
    fallbackCopyTextToClipboard(logContent.value);
  }
};
const fallbackCopyTextToClipboard = (text: string) => {
  const textArea = document.createElement('textarea');
  textArea.value = text;
  textArea.style.position = 'fixed';
  textArea.style.opacity = '0';
  document.body.appendChild(textArea);
  textArea.select();
  try {
    document.execCommand('copy') ? message.success('复制成功') : message.error('复制失败');
  } catch (err) {
    message.error('复制失败，请手动复制');
  }
  document.body.removeChild(textArea);
};
const openVideoPlayer = (video: TopVideoItem) => {
  if (!video.id) return message.warn('视频ID不存在');
  currentVideo.value = video;
  showVideoPlayer.value = true;
  loadVideo(video).then(() => {
    videoPlayerRef.value?.play().catch((err) => console.log('自动播放被拦截：', err));
  });
};
const loadVideo = async (video: TopVideoItem) => {
  try {
    videoLoading.value = true;
    videoError.value = false;
    const timestamp = new Date().getTime();
    videoPlayUrl.value = `${import.meta.env.VITE_API_URL}api/Video/play/${video.id}?t=${timestamp}`;
  } catch (err) {
    videoError.value = true;
    message.error('视频加载失败');
  } finally {
    videoLoading.value = false;
  }
};
const closeVideoPlayer = () => {
  showVideoPlayer.value = false;
  currentVideo.value = null;
  videoPlayUrl.value = '';
  videoLoading.value = false;
  videoError.value = false;
  isVideoPaused.value = false;
  videoPlayerRef.value?.pause();
};
const handleVideoError = () => {
  videoError.value = true;
};
const onVideoPause = () => {
  isVideoPaused.value = true;
};
const onVideoPlay = () => {
  isVideoPaused.value = false;
};
const deleteCurrentVideo = async () => {
  closeVideoPlayer();
  TopVideo();
  loadDashboardData();
};
const handleVideoLoadedMetadata = () => {
  if (!videoPlayerRef.value) return;
  const video = videoPlayerRef.value;
  const wrapper = document.querySelector('.video-player-wrapper');
  if (!wrapper) return;
  const videoRatio = video.videoWidth / video.videoHeight;
  if (videoRatio <= 1) {
    video.style.width = '100%';
    video.style.height = '100%';
    video.style.objectFit = 'cover';
    video.style.position = 'static';
  } else {
    video.style.objectFit = 'contain';
    video.style.position = 'absolute';
    video.style.top = '50%';
    video.style.left = '50%';
    video.style.transform = 'translate(-50%, -50%)';
    video.style.maxHeight = '100%';
    video.style.maxWidth = '100%';
  }
};

// ========== 关注列表核心方法 ==========
// 切换到关注Tab时初始化
const handleSwitchToFollow = () => {
  activeTab.value = 'follow';
  nextTick(() => {
    if (tabList.value.length === 0) {
      GetCookies();
    }
  });
};
// 获取Cookie/Tab列表
const GetCookies = (): Promise<void> => {
  return new Promise((resolve) => {
    useApiStore()
      .CookieList()
      .then((res) => {
        if (res.code === 0) {
          tabList.value = res.data;
          if (tabList.value.length > 0 && !activeFollowTabKey.value) {
            activeFollowTabKey.value = tabList.value[0].key;
          }
        }
        resolve();
      })
      .catch((err) => {
        console.error('获取Tab数据失败：', err);
        message.error('获取Tab数据失败，请刷新重试');
        resolve();
      });
  });
};
// 获取关注列表
const GetFollows = (isReset = false) => {
  if (loading.value || (noMoreData.value && !isReset)) return;
  loading.value = true;
  useApiStore()
    .FollowList(quaryData)
    .then((res) => {
      if (res.code === 0) {
        const newData = res.data.data || [];
        const total = res.data.total || 0;
        // 格式化数据
        const formattedData = newData.map((item) => ({
          ...item,
          isSaving: false,
          isEditing: item.isEditing ?? false,
          uperId: item.uperId || item.id,
          isNoFollowed: item.isNoFollowed ?? false,
        }));
        // 更新Tab总数
        if (isReset) {
          const tabIndex = tabList.value.findIndex((tab) => tab.key === activeFollowTabKey.value);
          if (tabIndex !== -1) tabList.value[tabIndex].total = total;
        }
        // 判断是否有更多
        if (formattedData.length < quaryData.pageSize) {
          noMoreData.value = true;
          hasMore.value = false;
        } else {
          noMoreData.value = false;
          hasMore.value = true;
        }
        // 合并数据
        if (isReset) {
          followData.value = formattedData;
        } else {
          const existingKeys = followData.value.map((item) => item.id);
          const uniqueNewData = formattedData.filter((item) => !existingKeys.includes(item.id));
          followData.value = [...followData.value, ...uniqueNewData];
        }
        quaryData.pageIndex += 1;
      } else {
        message.error('获取关注列表失败');
        noMoreData.value = true;
        hasMore.value = false;
      }
    })
    .catch((err) => {
      console.error('获取关注列表异常：', err);
      message.error('网络异常，请重试');
      noMoreData.value = true;
      hasMore.value = false;
    })
    .finally(() => {
      loading.value = false;
    });
};
// 清空搜索
const clearSearch = () => {
  quaryData.followUserName = null;
  quaryData.pageIndex = 0;
  GetFollows(true);
};
// 执行搜索
const handleSearch = () => {
  quaryData.pageIndex = 0;
  noMoreData.value = false;
  hasMore.value = true;
  GetFollows(true);
};
// Tab切换
const handleTabChange = (key: string) => {
  if (activeFollowTabKey.value === key) return;
  activeFollowTabKey.value = key;
  quaryData.mySelfId = key;
  quaryData.pageIndex = 0;
  noMoreData.value = false;
  hasMore.value = true;
  quaryData.followUserName = null;
  GetFollows(true);
};
// 滚动加载更多
const handleScroll = () => {
  const container = document.querySelector('.follow-list-container');
  if (!container || loading.value || !hasMore.value || noMoreData.value) return;
  const { scrollTop, scrollHeight, clientHeight } = container as HTMLDivElement;
  if (scrollTop + clientHeight >= scrollHeight - 50) {
    // 移动端提前50px加载
    GetFollows(false);
  }
};
// 同步总开关变更
const handleSwitchChange = (item: FollowItem, checked: boolean) => {
  item.openSync = checked;
  uploadSyncStatus(item);
};
// 全量同步开关变更
const handleSyncChange = (item: FollowItem, checked: boolean) => {
  item.fullSync = checked;
  uploadSyncStatus(item);
};
// 编辑路径
const handleEditPath = (item: FollowItem) => {
  if (item.isSaving) return;
  item.isEditing = true;
  nextTick(() => {
    const input = document.querySelector(`.follow-card[data-key="${item.id}"] .path-input`) as HTMLInputElement | null;
    input?.focus();
  });
};
// 保存路径
const handleSavePath = (item: FollowItem) => {
  if (item.isSaving) return;
  uploadSyncStatus(item);
};
// 更新同步状态
const uploadSyncStatus = (item: FollowItem) => {
  item.isSaving = true;
  useApiStore()
    .OpenOrCloseSync({
      Id: item.id,
      OpenSync: item.openSync,
      FullSync: item.fullSync,
      SavePath: item.savePath,
      uperId: item.uperId,
    })
    .then((res) => {
      if (res.code === 0) {
        message.success(`保存成功，下次任务生效`);
        item.isEditing = false;
      } else {
        message.error('保存失败' + (res.message || '未知错误'));
      }
    })
    .catch((err) => {
      console.error('保存失败', err);
      message.error('保存失败，请重试');
    })
    .finally(() => {
      item.isSaving = false;
    });
};
// 批量同步
const handleSyncAll = () => {
  if (isSyncDisabled.value) return;
  isSyncDisabled.value = true;
  loading.value = true;
  useApiStore()
    .StartJobNow()
    .then((res) => {
      if (res.code === 0) {
        message.success('后台开始同步，请注意查收');
      } else {
        message.error('同步失败：' + (res.message || '未知错误'));
      }
    })
    .catch((err) => {
      console.error('同步异常：', err);
      message.error('同步失败，请重试');
    })
    .finally(() => {
      isSyncDisabled.value = false;
      loading.value = false;
    });
};
// 文本截断
const truncateText = (text: string, maxLength: number): string => {
  if (!text || text.length <= maxLength) return text;
  return text.slice(0, maxLength) + '...';
};

// 删除关注博主
const handleDeleteItem = (item: FollowItem) => {
  Modal.confirm({
    title: '确认删除',
    content: `确定删除「${item.uperName}」吗？删除后无法恢复`,
    okText: '删除',
    cancelText: '取消',
    okType: 'danger',
    maskClosable: false,
    onOk: () => {
      return new Promise((resolve, reject) => {
        useApiStore()
          .DelFollow({
            id: item.id,
            mySelfId: item.mySelfId,
            uperId: item.uperId,
          })
          .then((res) => {
            if (res.code === 0) {
              message.success('删除成功！');
              // 重新加载数据
              quaryData.pageIndex = 0;
              GetFollows(true);
              // 更新Tab总数
              const tabIndex = tabList.value.findIndex((tab) => tab.key === activeFollowTabKey.value);
              if (tabIndex !== -1) {
                tabList.value[tabIndex].total = (tabList.value[tabIndex].total || 1) - 1;
              }
              resolve(true);
            } else {
              message.error('删除失败：' + (res.message || '未知错误'));
              reject(false);
            }
          })
          .catch((err) => {
            console.error('删除异常：', err);
            message.error('网络异常，请重试');
            reject(false);
          });
      });
    },
  });
};
</script>

<style>
html,
body {
  margin: 0;
  padding: 0;
  width: 100%;
  height: 100%;
  overflow-x: hidden;
}
#app {
  margin: 0;
  padding: 0;
  width: 100%;
  height: 100%;
}
.ant-message {
  z-index: 10000 !important;
}
/* 强制清除antd卡片默认边距和内边距 */
.ant-card {
  margin: 0 !important;
  border-radius: 12px !important;
}
.ant-card-body {
  padding: 0 !important;
}

/* ========== 移动端隐藏所有滚动条 - 核心代码 ========== */
/* 针对webkit内核浏览器（iOS/安卓Chrome/Edge等） */
*::-webkit-scrollbar {
  width: 0px !important; /* 垂直滚动条宽度 */
  height: 0px !important; /* 水平滚动条高度 */
  display: none !important; /* 直接隐藏滚动条元素 */
}
/* 滚动条轨道/滑块也隐藏，防止残留 */
*::-webkit-scrollbar-track,
*::-webkit-scrollbar-thumb {
  background: transparent !important;
  border-radius: 0 !important;
}
/* 火狐浏览器适配（可选，移动端火狐占比极低） */
* {
  scrollbar-width: none !important; /* 火狐隐藏滚动条 */
  -ms-overflow-style: none !important; /* IE/Edge 隐藏滚动条 */
}

/* ===== 关注 Tab：路径与开启同一行 + 紧凑卡片 ===== */
.follow-list-container {
  gap: 8px !important;
  padding-bottom: 14px !important;
}

.follow-card {
  border-radius: 13px !important;
  box-shadow: 0 3px 12px rgba(31, 45, 61, 0.05) !important;
}

.follow-card:hover {
  transform: none;
  box-shadow: 0 4px 14px rgba(31, 45, 61, 0.07) !important;
}

.follow-card-inner {
  min-height: 0 !important;
  gap: 7px !important;
  padding: 10px 10px 9px 14px !important;
}

.follow-card-top {
  align-items: center;
  gap: 9px;
}

.avatar-wrapper {
  width: 44px;
  height: 44px;
  padding: 1px;
}

:deep(.follow-card .ant-avatar),
:deep(.follow-card .ant-avatar-lg) {
  width: 40px !important;
  height: 40px !important;
  font-size: 16px !important;
}

.name-actions {
  width: calc(100% - 53px) !important;
  gap: 4px;
}

.name-wrapper {
  align-items: center;
}

.uper-title-line {
  gap: 1px;
}

.uper-name {
  font-size: 14px;
  line-height: 1.2;
}

.douyin-no {
  padding: 1px 6px;
  font-size: 9px;
}

.no-followed-badge {
  height: 18px;
  padding: 0 6px;
  font-size: 9px;
}

.top-actions {
  gap: 5px;
}

.sync-state-text {
  gap: 4px;
  font-size: 9px;
}

.sync-state-text i {
  width: 5px;
  height: 5px;
}

.delete-btn {
  width: 25px !important;
  height: 25px !important;
  border-radius: 7px !important;
}

.follow-card-desc {
  padding: 4px 7px;
  border-radius: 7px;
  font-size: 10px;
  line-height: 1.35;
}

/* 路径与开启操作强制同一行 */
.follow-card-bottom {
  display: grid !important;
  grid-template-columns: minmax(0, 1fr) auto !important;
  align-items: center !important;
  gap: 6px !important;
  padding-top: 6px !important;
}

.path-area {
  min-width: 0;
  gap: 4px;
  padding: 4px 6px;
  border-radius: 7px;
}

.path-text {
  min-width: 0;
  font-size: 10px;
  line-height: 1.25;
}

.edit-btn {
  width: 24px !important;
  height: 24px !important;
  border-radius: 7px !important;
}

.edit-input-group {
  gap: 4px;
}

.path-input {
  height: 26px !important;
  font-size: 11px !important;
}

.follow-switches {
  display: flex;
  align-items: center;
  gap: 4px;
  flex-shrink: 0;
}

.enable-sync-area,
.full-sync-area {
  width: auto !important;
  min-width: 0 !important;
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px 5px;
  border-radius: 7px;
  box-sizing: border-box;
}

.enable-sync-area {
  background: rgba(64, 150, 255, 0.075);
}

.full-sync-area {
  background: rgba(76, 175, 80, 0.075);
}

.enable-sync-label,
.full-sync-label {
  font-size: 9px !important;
  line-height: 1;
  white-space: nowrap;
}

.enable-sync-label {
  color: #5f7894;
}

.full-sync-label {
  color: #64806c;
}

:deep(.follow-switches .ant-switch-small) {
  min-width: 28px !important;
}

/* 小屏也不换行 */
@media screen and (max-width: 375px) {
  .follow-card-inner {
    gap: 6px !important;
    padding: 9px 9px 8px 13px !important;
  }

  .follow-card-bottom {
    grid-template-columns: minmax(0, 1fr) auto !important;
  }

  .follow-switches {
    gap: 3px;
  }

  .enable-sync-area,
  .full-sync-area {
    gap: 3px;
    padding-left: 4px;
    padding-right: 4px;
  }

  .enable-sync-label,
  .full-sync-label {
    font-size: 8px !important;
  }
}

html.dark-mode .enable-sync-area {
  background: rgba(64, 150, 255, 0.12);
}

html.dark-mode .enable-sync-label {
  color: #9fc4ec;
}

</style>

<style scoped>
/* ========== 原有仪表盘样式（保留，无需修改） ========== */
.stats-dashboard-mobile {
  min-height: 100vh;
  background-color: #ffffff;
  color: #333333;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  padding: 5px 0;
  display: flex;
  flex-direction: column;
  padding-bottom: 80px;
  position: relative;
}
.dashboard-container {
  width: 100%;
  padding: 0 5px;
  box-sizing: border-box;
  flex: 1;
  overflow-y: auto;
  -webkit-overflow-scrolling: touch;
}
.tab-content {
  width: 100%;
  height: 100%;
}
.stats-overview {
  display: grid;
  grid-template-columns: 1fr;
  gap: 20px;
  margin-bottom: 5px;
}
.main-card {
  padding: 0px 10px;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.06);
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
  border: 1px solid #f0f0f0;
  background: #fff;
}
.main-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 25px rgba(0, 0, 0, 0.08);
}
.stat-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 16px;
}
.header-left {
  display: flex;
  flex-direction: column;
  gap: 4px;
  width: 100%;
}
.stat-meta {
  font-size: 14px;
  color: #666666;
  text-transform: uppercase;
  letter-spacing: 0.4px;
  font-weight: 500;
}
.stat-value-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
  margin-top: 8px;
}
.stat-value {
  font-size: 26px;
  font-weight: 700;
  color: #1a1a1a;
  line-height: 1.1;
}
.value-label {
  font-size: 14px;
  color: #666;
  font-weight: 500;
  margin-left: 4px;
}
.video-count {
  font-size: 22px;
}
.size-count {
  font-size: 22px;
  color: #666;
}
.unit {
  font-size: 18px;
  color: #444444;
  font-weight: 500;
}
.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background-color: rgba(76, 175, 80, 0.15);
  color: #4caf50;
}
.secondary-card .stat-icon {
  background-color: rgba(33, 150, 243, 0.15);
  color: #2196f3;
}
.stat-subitems {
  display: grid;
  grid-template-columns: 1fr;
  gap: 12px;
  padding-top: 16px;
  border-top: 1px solid #f0f0f0;
}
.subitem {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 14px;
  background: #ffffff;
  border: 1px solid #f0f0f0;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  transition: all 0.2s ease;
  cursor: default;
  justify-content: space-between;
}
.subitem:hover {
  transform: translateY(-1px);
  box-shadow: 0 3px 10px rgba(0, 0, 0, 0.06);
  border-color: #e0e0e0;
}
.subitem-icon {
  width: 28px;
  height: 28px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background-color: rgba(233, 30, 99, 0.1);
  color: #e91e63;
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
.video-list-icon {
  background-color: rgba(33, 150, 243, 0.15);
  color: #2196f3;
}
.video-index {
  font-size: 12px;
  font-weight: 600;
}
.subitem-meta {
  font-size: 13px;
  color: #666666;
  font-weight: 500;
  flex: 1;
}
.subitem-count {
  color: #222;
  font-weight: 600;
  margin-left: 4px;
}
.subitem-size {
  font-size: 12px;
  color: #999;
  font-weight: 400;
}
.video-title {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
  line-height: 1.4;
  max-height: 2.8em;
  width: 100%;
  padding-right: 4px;
  cursor: pointer;
  transition: color 0.2s;
}
.video-title:hover {
  color: #2196f3;
}
.video-time {
  font-size: 11px !important;
  color: #999;
  margin-top: 2px;
  font-weight: 300 !important;
  display: flex;
  align-items: center;
  justify-content: flex-end;
}
.video-info {
  flex: 1;
  overflow: hidden;
}
.subitem-value {
  font-size: 15px;
  font-weight: 600;
  color: #222222;
  display: flex;
  align-items: baseline;
  gap: 3px;
}
.subitem-value .unit {
  font-size: 12px;
  color: #555555;
  font-weight: 500;
}
.primary-card {
  border-top: 3px solid #4caf50;
}
.secondary-card {
  border-top: 3px solid #2196f3;
}
.empty-video-list {
  text-align: center;
  padding: 10px 0;
  color: #999;
  font-size: 14px;
}

/* 底部导航样式：改为三分栏 */
.bottom-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  height: 60px;
  background-color: #ffffff;
  border-top: 1px solid #f0f0f0;
  display: flex;
  justify-content: space-around;
  align-items: center;
  z-index: 100;
  box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.05);
}
.nav-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
  height: 100%;
  color: #666666;
  transition: all 0.2s ease;
  cursor: pointer;
}
.nav-item.active {
  color: #4caf50;
}
.nav-item span {
  font-size: 12px;
  margin-top: 4px;
  font-weight: 500;
}
.nav-item svg {
  transition: all 0.2s ease;
}
.nav-item.active svg {
  transform: scale(1.1);
}

/* 日志 Tab 样式 */
.log-tab {
  padding: 5px 0;
}
.log-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding: 0 5px;
}
.log-title {
  font-size: 18px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0;
}
.log-filter {
  display: flex;
  gap: 8px;
}
.filter-btn {
  padding: 6px 12px;
  border-radius: 20px;
  border: 1px solid #e0e0e0;
  background-color: #ffffff;
  color: #666666;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}
.filter-btn.active {
  background-color: #4caf50;
  color: #ffffff;
  border-color: #4caf50;
}
.filter-btn:hover:not(.active) {
  border-color: #cccccc;
  color: #333333;
}

/* 日志列表样式 */
.log-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.log-item {
  padding: 10px;
  border-radius: 5px;
  border: 1px solid #f0f0f0;
  background-color: #ffffff;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  cursor: pointer;
  transition: all 0.2s ease;
}
.log-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}
.log-item.debug {
  border-left: 4px solid #2196f3;
}
.log-item.error {
  border-left: 4px solid #f44336;
}
.log-item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}
.log-type {
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
}
.log-type.debug {
  background-color: rgba(33, 150, 243, 0.1);
  color: #2196f3;
}
.log-type.error {
  background-color: rgba(244, 67, 54, 0.1);
  color: #f44336;
}
.log-time {
  font-size: 11px;
  color: #999999;
}
.log-file {
  font-size: 12px;
  color: #666666;
}
.empty-log {
  text-align: center;
  padding: 40px 20px;
  color: #999999;
  font-size: 14px;
}

/* 日志详情弹窗样式 */
.log-modal-mask {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 10px;
  box-sizing: border-box;
}
.log-modal-content {
  width: 100%;
  max-width: 500px;
  background-color: #ffffff;
  border-radius: 12px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
  max-height: 80vh;
  display: flex;
  flex-direction: column;
}
.log-modal-header {
  padding: 5px;
  border-bottom: 1px solid #f0f0f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.header-left {
  display: flex;
  align-items: center;
  gap: 8px;
}
.modal-type-tag {
  padding: 3px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
}
.modal-type-tag.debug {
  background-color: rgba(33, 150, 243, 0.1);
  color: #2196f3;
}
.modal-type-tag.error {
  background-color: rgba(244, 67, 54, 0.1);
  color: #f44336;
}
.modal-date {
  font-size: 14px;
  color: #666;
  font-weight: 500;
}
.close-btn {
  background: transparent;
  border: none;
  cursor: pointer;
  color: #666;
  padding: 4px;
  border-radius: 4px;
  transition: all 0.2s ease;
}
.close-btn:hover {
  background-color: #f5f5f5;
  color: #333;
}
.log-modal-body {
  padding: 10px;
  flex: 1;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}
.file-name {
  font-size: 14px;
  color: #333;
  font-weight: 600;
  margin-bottom: 5px;
  border-bottom: 1px solid #f0f0f0;
}
.log-content {
  flex: 1;
  margin: 0;
  padding: 5px;
  background-color: #f9f9f9;
  border-radius: 8px;
  overflow-y: auto;
  overflow-x: auto;
  font-family: Consolas, Monaco, monospace;
  font-size: 13px;
  line-height: 1.6;
  white-space: pre-wrap;
  word-break: break-all;
  -webkit-overflow-scrolling: touch;
  touch-action: pan-y;
  color: #333;
}
.log-modal-footer {
  padding: 5px 20px;
  border-top: 1px solid #f0f0f0;
  display: flex;
  justify-content: flex-end;
}
.copy-btn {
  padding: 8px 16px;
  background-color: #4caf50;
  color: #ffffff;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: all 0.2s ease;
}
.copy-btn:disabled {
  background-color: #cccccc;
  cursor: not-allowed;
}
.copy-btn:hover:not(:disabled) {
  background-color: #43a047;
}
/* 视频弹窗样式 */
.video-modal-mask {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #000;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
  padding: 0;
  margin: 0;
  box-sizing: border-box;
  overflow: hidden;
}
.video-modal-content {
  width: 100vw;
  height: 100vh;
  max-width: none;
  max-height: none;
  background-color: #000;
  border-radius: 0;
  box-shadow: none;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  position: absolute;
  top: 0;
  left: 0;
  margin: 0;
  padding: 0;
  box-sizing: border-box;
  padding-top: env(safe-area-inset-top);
  height: calc(100vh - env(safe-area-inset-top));
}
.video-player-wrapper {
  position: relative;
  width: 100%;
  height: 100%;
  margin: 0;
  padding: 0;
  overflow: hidden;
  height: calc(100% - env(safe-area-inset-top));
  display: flex;
  align-items: center;
  justify-content: center;
}
.floating-close-btn {
  position: absolute;
  top: env(safe-area-inset-top);
  right: 0px;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background-color: rgba(0, 0, 0, 0.5);
  border: none;
  cursor: pointer;
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  z-index: 2010;
  backdrop-filter: blur(2px);
}
.floating-close-btn:hover {
  background-color: rgba(255, 255, 255, 0.2);
  transform: scale(1.1);
}
.video-modal-body {
  padding: 0;
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #000;
  position: relative;
  width: 100%;
  height: 100%;
  margin: 0;
  overflow: hidden;
}
.video-player {
  width: 100%;
  height: 100%;
  object-fit: cover;
  margin: 0;
  padding: 0;
  border: none;
  outline: none;
  display: block;
}
.video-delete-btn {
  position: fixed;
  top: 50%;
  right: 20px;
  transform: translateY(-50%);
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background-color: rgba(0, 0, 0, 0.3);
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #ffffff;
  transition: all 0.2s ease;
  z-index: 2010;
  backdrop-filter: blur(2px);
}
.video-delete-btn:hover,
.video-delete-btn:active {
  background-color: rgba(244, 67, 54, 0.5);
  transform: translateY(-50%) scale(1.1);
}
.video-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #fff;
  padding: 40px 20px;
  width: 100%;
  height: 100%;
}
.loading-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid rgba(255, 255, 255, 0.2);
  border-radius: 50%;
  border-top-color: #fff;
  animation: spin 1s ease-in-out infinite;
  margin-bottom: 16px;
}
@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
.video-error {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #fff;
  padding: 40px 20px;
  text-align: center;
  width: 100%;
  height: 100%;
}
.video-error p {
  margin: 16px 0 24px;
  font-size: 14px;
}
.retry-btn {
  padding: 8px 16px;
  background-color: #2196f3;
  color: #fff;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s;
}
.retry-btn:hover {
  background-color: #1976d2;
}

/* ========== 关注列表样式【彻底修复：无数据不居中，页面布局正常】 ========== */
.follow-tab {
  padding: 0 5px;
  width: 100%;
  box-sizing: border-box;
  /* 关键：让tab容器占满父级高度 */
  display: flex;
  flex-direction: column;
  height: 100vh;
}
/* 搜索+操作栏 */
.follow-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 10px 0;
  width: 100%;
  z-index: 10;
  position: relative;
  /* 固定在顶部，不随滚动移动 */
  flex-shrink: 0;
}
.search-wrapper {
  flex: 1;
  position: relative;
}
.follow-search-input {
  height: 40px !important;
  border-radius: 20px !important;
  padding: 0 16px !important;
  font-size: 14px !important;
  width: 100% !important;
}
.follow-actions {
  display: flex;
  gap: 8px;
  flex-shrink: 0;
}
.follow-btn {
  width: 40px !important;
  height: 40px !important;
  border-radius: 50% !important;
  padding: 0 !important;
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
}
/* Tab导航：横向滚动 */
.follow-tab-wrapper {
  width: 100%;
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
  margin-bottom: 10px;
  z-index: 10;
  position: relative;
  /* 固定在搜索栏下方，不随滚动移动 */
  flex-shrink: 0;
}
.follow-tab-scroll {
  width: max-content;
  padding-right: 10px;
}
.follow-custom-tabs {
  --ant-tabs-nav-item-spacing: 16px !important;
}
:deep(.follow-custom-tabs .ant-tabs-tab) {
  padding: 8px 12px !important;
  font-size: 14px !important;
  white-space: nowrap;
}
:deep(.follow-custom-tabs .ant-tabs-ink-bar) {
  background-color: #4caf50 !important;
}
/* 列表容器：核心修复★★★ 固定高度+flex布局+滚动，无数据时不居中 */
.follow-list-container {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 12px !important;
  /* 适配底部导航，预留高度，固定计算方式 */
  height: calc(100vh - 180px) !important;
  max-height: calc(100vh - 180px) !important;
  overflow-y: auto !important;
  padding: 0 2px 20px 2px !important;
  box-sizing: border-box;
  -webkit-overflow-scrolling: touch;
  /* 关键：让容器占满剩余高度，子元素按flex布局排列 */
  flex: 1;
  flex-grow: 1;
  /* 关键：确保容器高度不超过视口高度 */
  position: relative;
}
/* 博主卡片【核心修复：完整显示所有内容】 */
.follow-card {
  border-radius: 12px !important;
  border: 1px solid #f0f0f0 !important;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04) !important;
  background: #fff !important;
  overflow: visible !important;
  margin: 0 !important;
  width: 100% !important;
  box-sizing: border-box !important;
  position: relative !important;
  z-index: 1 !important;
  /* 卡片不拉伸，固定高度 */
  flex-shrink: 0;
}
/* 非关注卡片样式 */
.follow-card.no-followed-card {
  border-color: #fee2e2 !important;
  background-color: #fef2f2 !important;
}
/* 卡片内层容器【强制显示所有子元素】 */
.follow-card-inner {
  padding: 16px 14px !important;
  display: flex;
  flex-direction: column;
  gap: 12px !important;
  width: 100% !important;
  box-sizing: border-box !important;
  position: relative !important;
  z-index: 2 !important;
  height: auto !important;
  min-height: 120px !important; /* 最小高度，防止内容过少塌陷 */
}
/* 卡片顶部：头像+名称+操作 */
.follow-card-top {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  box-sizing: border-box;
  flex-shrink: 0;
}
.avatar-wrapper {
  width: 52px;
  height: 52px;
  flex-shrink: 0;
  cursor: pointer;
  border-radius: 50%;
  overflow: hidden;
}
/* 头像样式覆盖 */
:deep(.follow-card .ant-avatar) {
  width: 52px !important;
  height: 52px !important;
  font-size: 20px !important;
  border-radius: 50% !important;
}
:deep(.follow-card .ant-avatar-lg) {
  width: 52px !important;
  height: 52px !important;
}
.avatar-placeholder {
  background: linear-gradient(135deg, #4096ff, #69b1ff) !important;
  color: #fff !important;
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
}
/* 名称+操作容器：自动占满剩余宽度 */
.name-actions {
  flex: 1 !important;
  display: flex;
  flex-direction: column;
  gap: 8px;
  width: calc(100% - 62px) !important;
  box-sizing: border-box;
}
.name-wrapper {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
  width: 100%;
}
.uper-name {
  font-size: 16px;
  font-weight: 600;
  color: #1d2129;
  margin: 0;
  line-height: 1.4;
  flex: 1;
}
.no-followed-badge {
  background-color: #fee2e2;
  color: #dc2626;
  font-size: 11px;
  padding: 1px 6px;
  border-radius: 4px;
  flex-shrink: 0;
  height: 20px;
  display: flex;
  align-items: center;
}
/* 顶部操作按钮：删除+开关 */
.top-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  justify-content: flex-end;
  width: 100%;
}
.delete-btn {
  color: #ef4444 !important;
  font-size: 16px !important;
  width: 32px !important;
  height: 32px !important;
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
}
/* 签名样式：强制显示 */
.follow-card-desc {
  font-size: 13px;
  color: #86909c;
  line-height: 1.5;
  width: 100%;
  box-sizing: border-box;
  padding: 0 2px;
  flex-shrink: 0;
}
.signature-text {
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
  width: 100%;
  display: block !important;
}
/* 卡片底部：路径+全量同步【仅同步开启显示】 */
.follow-card-bottom {
  display: flex;
  flex-direction: column;
  gap: 10px !important;
  padding-top: 5px !important;
  border-top: 1px solid #f5f5f5;
  width: 100%;
  box-sizing: border-box;
  flex-shrink: 0;
}
.path-area {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100%;
}
.path-text {
  font-size: 13px;
  color: #333;
  flex: 1;
  line-height: 1.4;
}
.path-text.path-empty {
  color: #999;
  font-style: italic;
}
.edit-btn {
  color: #4096ff !important;
  font-size: 14px !important;
  width: 32px !important;
  height: 32px !important;
}
/* 编辑输入框 */
.edit-input-group {
  display: flex;
  align-items: center;
  gap: 6px;
  flex: 1;
  width: 100%;
}
.path-input {
  height: 32px !important;
  font-size: 13px !important;
  flex: 1 !important;
}
/* 全量同步 */
.full-sync-area {
  display: flex;
  align-items: center;
  gap: 8px;
  justify-content: space-between;
  width: 100%;
}
.full-sync-label {
  font-size: 13px;
  color: #666;
}

/* 状态容器：核心修复★★★ 加载/无数据/无更多 统一样式，占满剩余高度且内容居中 */
.loading-container,
.empty-container,
.no-more-container {
  width: 100%;
  box-sizing: border-box;
  /* 关键：占满列表容器剩余高度，内容垂直水平居中 */
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 !important;
  padding: 20px 0 !important;
}
/* 加载状态 */
.loading-container {
  gap: 12px;
  color: #6b7280;
  font-size: 14px;
}
/* 无更多状态（取消居中，靠上显示） */
.no-more-container {
  flex: none !important; /* 不占满高度，仅自身高度 */
  padding: 10px 0 !important;
  color: #9ca3af;
  font-size: 14px;
}
/* 无数据状态 */
.empty-container {
  padding: 40px 0 !important;
}
/* 黑暗模式下无数据文字颜色 */
html.dark-mode .empty-container :deep(.ant-empty-description) {
  color: #808099 !important;
}

/* ========== 小屏手机适配（375px以下，如SE/小屏安卓） ========== */
@media screen and (max-width: 375px) {
  .follow-list-container {
    /* 小屏减少预留高度，适配更友好 */
    height: calc(100vh - 160px) !important;
    max-height: calc(100vh - 160px) !important;
    gap: 10px !important;
  }
  .follow-card-inner {
    padding: 14px 12px !important;
    gap: 10px !important;
    min-height: 100px !important;
  }
  .avatar-wrapper {
    width: 48px !important;
    height: 48px !important;
  }
  :deep(.follow-card .ant-avatar) {
    width: 48px !important;
    height: 48px !important;
    font-size: 18px !important;
  }
  .uper-name {
    font-size: 15px !important;
  }
  .follow-card-desc {
    font-size: 12px !important;
  }
  .full-sync-label {
    font-size: 12px !important;
  }
}
/* 黑暗模式下无数据文字颜色 */
html.dark-mode .empty-container :deep(.ant-empty-description) {
  color: #808099 !important;
}

/* ========== 黑暗模式适配【完整适配follow-card】 ========== */
html.dark-mode .stats-dashboard-mobile {
  background-color: #1a1a2e;
  color: #eaeaea;
  padding-bottom: 80px;
}
html.dark-mode .main-card {
  border-color: rgba(255, 255, 255, 0.1);
  background-color: rgba(30, 30, 50, 0.9);
}
html.dark-mode .stat-subitems {
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}
html.dark-mode .subitem {
  background: rgba(40, 40, 65, 0.7);
  border: 1px solid rgba(255, 255, 255, 0.05);
}
html.dark-mode .subitem:hover {
  background: rgba(40, 40, 65, 0.9);
  border-color: rgba(255, 255, 255, 0.1);
}
html.dark-mode .stat-meta {
  color: #b0b0c3;
}
html.dark-mode .stat-value {
  color: #ffffff;
}
html.dark-mode .value-label {
  color: #d0d0d0;
}
html.dark-mode .size-count {
  color: #d0d0d0;
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
html.dark-mode .subitem-count {
  color: #ffffff;
}
html.dark-mode .subitem-size {
  color: #a0a0b3;
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
html.dark-mode .video-list-icon {
  background-color: rgba(33, 150, 243, 0.25);
}
html.dark-mode .video-time {
  color: #a0a0b3;
}
html.dark-mode .empty-video-list {
  color: #808099;
}
/* 黑暗模式 - 底部导航 */
html.dark-mode .bottom-nav {
  background-color: #16213e;
  border-top-color: rgba(255, 255, 255, 0.1);
}
html.dark-mode .nav-item {
  color: #b0b0c3;
}
html.dark-mode .nav-item.active {
  color: #4caf50;
}
/* 黑暗模式 - 日志样式 */
html.dark-mode .log-title {
  color: #ffffff;
}
html.dark-mode .filter-btn {
  background-color: #1f4068;
  border-color: rgba(255, 255, 255, 0.1);
  color: #e0e0e0;
}
html.dark-mode .filter-btn.active {
  background-color: #4caf50;
  color: #ffffff;
  border-color: #4caf50;
}
html.dark-mode .log-item {
  background-color: rgba(30, 30, 50, 0.9);
  border-color: rgba(255, 255, 255, 0.05);
}
html.dark-mode .log-file {
  color: #c0c0d3;
}
html.dark-mode .empty-log {
  color: #808099;
}
/* 黑暗模式 - 弹窗样式 */
html.dark-mode .log-modal-content {
  background-color: #1e1e3f;
  border-color: rgba(255, 255, 255, 0.1);
}
html.dark-mode .log-modal-header,
html.dark-mode .log-modal-footer {
  border-color: rgba(255, 255, 255, 0.1);
}
html.dark-mode .modal-date {
  color: #c0c0d3;
}
html.dark-mode .file-name {
  color: #ffffff;
  border-color: rgba(255, 255, 255, 0.1);
}
html.dark-mode .log-content {
  background-color: #2a2a4a;
  color: #eaeaea;
}
html.dark-mode .close-btn {
  color: #c0c0d3;
}
html.dark-mode .close-btn:hover {
  background-color: #2a2a4a;
  color: #ffffff;
}
/* 黑暗模式 - 视频弹窗 */
html.dark-mode .video-delete-btn {
  background-color: rgba(0, 0, 0, 0.4);
}
html.dark-mode .video-delete-btn:hover,
html.dark-mode .video-delete-btn:active {
  background-color: rgba(244, 67, 54, 0.6);
}
/* 黑暗模式 - 关注列表【完整适配】 */
html.dark-mode .follow-card {
  border-color: rgba(255, 255, 255, 0.1) !important;
  background-color: rgba(30, 30, 50, 0.9) !important;
}
html.dark-mode .follow-card.no-followed-card {
  border-color: #7f1d1d !important;
  background-color: rgba(127, 29, 29, 0.2) !important;
}
html.dark-mode .uper-name {
  color: #f3f4f6 !important;
}
html.dark-mode .no-followed-badge {
  background-color: #7f1d1d !important;
  color: #fecaca !important;
}
html.dark-mode .follow-card-desc {
  color: #9ca3af !important;
}
html.dark-mode .follow-card-bottom {
  border-top-color: rgba(255, 255, 255, 0.05) !important;
}
html.dark-mode .path-text {
  color: #d1d5db !important;
}
html.dark-mode .path-text.path-empty {
  color: #6b7280 !important;
}
html.dark-mode .full-sync-label {
  color: #9ca3af !important;
}
html.dark-mode .delete-btn {
  color: #fecaca !important;
}
html.dark-mode .edit-btn {
  color: #60a5fa !important;
}
html.dark-mode .avatar-placeholder {
  background: linear-gradient(135deg, #3b82f6, #60a5fa) !important;
}

/* ========== 小屏手机适配（375px以下，如SE/小屏安卓） ========== */
@media screen and (max-width: 375px) {
  .follow-list-container {
    max-height: calc(100vh - 160px) !important;
    gap: 10px !important;
  }
  .follow-card-inner {
    padding: 14px 12px !important;
    gap: 10px !important;
    min-height: 100px !important;
  }
  .avatar-wrapper {
    width: 48px !important;
    height: 48px !important;
  }
  :deep(.follow-card .ant-avatar) {
    width: 48px !important;
    height: 48px !important;
    font-size: 18px !important;
  }
  .uper-name {
    font-size: 15px !important;
  }
  .follow-card-desc {
    font-size: 12px !important;
  }
  .full-sync-label {
    font-size: 12px !important;
  }
}

/* 筛选下拉选择器：和搜索框、同步按钮统一风格，移动端适配 */
.follow-filter-select {
  width: 120px !important;
  height: 40px !important;
  flex-shrink: 0;
}
/* 覆盖antd select默认样式，和布局匹配 */
:deep(.follow-filter-select .ant-select-selector) {
  height: 100% !important;
  border-radius: 20px !important; /* 和搜索框同圆角，视觉统一 */
  display: flex !important;
  align-items: center !important;
  padding: 0 12px !important;
  font-size: 13px !important;
}
/* 隐藏select默认边框高亮（可选，更简洁） */
:deep(.follow-filter-select .ant-select-focused .ant-select-selector) {
  box-shadow: none !important;
  border-color: #d9d9d9 !important;
}
/* 下拉选项弹窗：适配移动端，调大宽度 */
:deep(.follow-filter-select .ant-select-dropdown) {
  min-width: 120px !important;
  font-size: 13px !important;
}

/* 黑暗模式适配：筛选选择器样式统一 */
html.dark-mode :deep(.follow-filter-select .ant-select-selector) {
  background-color: #1f4068 !important;
  border-color: rgba(255, 255, 255, 0.1) !important;
  color: #e0e0e0 !important;
}
html.dark-mode :deep(.follow-filter-select .ant-select-dropdown) {
  background-color: #1e1e3f !important;
  border-color: rgba(255, 255, 255, 0.1) !important;
}
html.dark-mode :deep(.follow-filter-select .ant-select-item) {
  color: #e0e0e0 !important;
}
html.dark-mode :deep(.follow-filter-select .ant-select-item-selected) {
  background-color: #4caf50 !important;
  color: #fff !important;
}

/* ===== 顶部视频统计：移动端协调版，一行 2 个 ===== */

/* 两张主卡片之间稍微收紧 */
.stats-overview {
  gap: 14px;
}

/* 顶部统计卡片整体留白统一 */
.primary-card {
  padding: 0 12px 12px;
}

.primary-card .stat-header {
  margin-bottom: 10px;
}

.primary-card .header-left {
  gap: 0;
}

/* 标题居中，层级更清晰 */
.primary-card .stat-meta {
  display: block;
  width: 100%;
  padding-top: 5px;
  text-align: center;
  font-size: 15px;
  line-height: 24px;
  color: #555;
  letter-spacing: 0;
  text-transform: none;
}

/* 总数和容量保持同一基线 */
.primary-card .stat-value-container {
  margin-top: 10px;
  padding: 0 2px 8px;
  align-items: baseline;
}

.primary-card .video-count {
  font-size: 27px;
  line-height: 1;
  color: #202020;
}

.primary-card .size-count {
  font-size: 25px;
  line-height: 1;
  color: #666;
}

.primary-card .value-label {
  font-size: 13px;
  margin-left: 3px;
}

/* 六个统计项固定两列 */
.primary-card .stat-subitems {
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 8px;
  padding-top: 10px;
}

/* 小卡片改为轻量化，避免一层大卡里再堆六个重卡 */
.primary-card .stat-subitems .subitem {
  box-sizing: border-box;
  min-width: 0;
  min-height: 66px;
  margin: 0;
  padding: 9px 8px;
  display: grid;
  grid-template-columns: 28px minmax(0, 1fr);
  grid-template-areas:
    'icon meta'
    'icon size';
  column-gap: 7px;
  row-gap: 4px;
  align-items: center;
  justify-content: initial;
  border: 1px solid #edf0f2;
  border-radius: 10px;
  background: #fafbfc;
  box-shadow: none;
}

.primary-card .stat-subitems .subitem:hover {
  transform: none;
  border-color: #e3e7ea;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.035);
}

/* 图标尺寸统一，视觉重心更稳 */
.primary-card .stat-subitems .subitem-icon {
  grid-area: icon;
  width: 28px;
  height: 28px;
  border-radius: 8px;
}

.primary-card .stat-subitems .subitem-icon svg {
  width: 14px;
  height: 14px;
}

/* 名称与数量保持在同一行 */
.primary-card .stat-subitems .subitem-meta {
  grid-area: meta;
  min-width: 0;
  margin: 0;
  font-size: 12px;
  line-height: 1.25;
  color: #555;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.primary-card .stat-subitems .subitem-count {
  margin-left: 2px;
  color: #222;
  font-weight: 600;
}

/* 容量放在名称下方，统一左对齐 */
.primary-card .stat-subitems .subitem-size {
  grid-area: size;
  min-width: 0;
  margin: 0;
  font-size: 11px;
  line-height: 1.2;
  color: #999;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* 最新同步继续保持单列，不受顶部两列规则影响 */
.secondary-card .video-list-container {
  grid-template-columns: 1fr;
}

/* 暗黑模式 */
html.dark-mode .primary-card .stat-meta {
  color: #d7d7e2;
}

html.dark-mode .primary-card .stat-subitems .subitem {
  background: rgba(40, 40, 65, 0.62);
  border-color: rgba(255, 255, 255, 0.06);
}

html.dark-mode .primary-card .stat-subitems .subitem:hover {
  border-color: rgba(255, 255, 255, 0.1);
}

html.dark-mode .primary-card .stat-subitems .subitem-meta {
  color: #d0d0dc;
}

/* 极窄屏仍保持两列 */
@media screen and (max-width: 360px) {
  .primary-card {
    padding-left: 9px;
    padding-right: 9px;
  }

  .primary-card .stat-subitems {
    gap: 6px;
  }

  .primary-card .stat-subitems .subitem {
    min-height: 62px;
    padding: 7px 6px;
    grid-template-columns: 24px minmax(0, 1fr);
    column-gap: 5px;
  }

  .primary-card .stat-subitems .subitem-icon {
    width: 24px;
    height: 24px;
    border-radius: 7px;
  }

  .primary-card .stat-subitems .subitem-meta {
    font-size: 11px;
  }

  .primary-card .stat-subitems .subitem-size {
    font-size: 10px;
  }

  .primary-card .video-count {
    font-size: 25px;
  }

  .primary-card .size-count {
    font-size: 23px;
  }
}

/* ===== 顶部总数与总大小：对称协调版 ===== */
.primary-card .stat-value-container {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 1px minmax(0, 1fr);
  align-items: stretch;
  gap: 0;
  width: 100%;
  margin-top: 8px;
  padding: 4px 0 10px;
}

.primary-card .summary-stat-item {
  min-width: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 5px;
  padding: 7px 8px;
}

.primary-card .summary-stat-divider {
  width: 1px;
  align-self: stretch;
  margin: 5px 0;
  background: #eceff1;
}

.primary-card .summary-stat-label {
  font-size: 11px;
  line-height: 1.2;
  color: #999;
  font-weight: 500;
  letter-spacing: 0.2px;
  white-space: nowrap;
}

.primary-card .summary-stat-item .stat-value {
  display: flex;
  align-items: baseline;
  justify-content: center;
  min-width: 0;
  font-size: 26px;
  line-height: 1;
  font-weight: 700;
  color: #252525;
  white-space: nowrap;
}

.primary-card .summary-stat-item .video-count,
.primary-card .summary-stat-item .size-count {
  font-size: 26px;
  line-height: 1;
  color: #252525;
}

.primary-card .summary-stat-item .value-label {
  margin-left: 4px;
  font-size: 12px;
  line-height: 1;
  font-weight: 500;
  color: #8a8a8a;
}

/* 暗黑模式 */
html.dark-mode .primary-card .summary-stat-divider {
  background: rgba(255, 255, 255, 0.1);
}

html.dark-mode .primary-card .summary-stat-label {
  color: #9f9fb3;
}

html.dark-mode .primary-card .summary-stat-item .stat-value,
html.dark-mode .primary-card .summary-stat-item .video-count,
html.dark-mode .primary-card .summary-stat-item .size-count {
  color: #ffffff;
}

html.dark-mode .primary-card .summary-stat-item .value-label {
  color: #b4b4c5;
}

/* 小屏进一步压缩，但仍保持完全对称 */
@media screen and (max-width: 360px) {
  .primary-card .summary-stat-item {
    padding: 6px 4px;
    gap: 4px;
  }

  .primary-card .summary-stat-item .stat-value,
  .primary-card .summary-stat-item .video-count,
  .primary-card .summary-stat-item .size-count {
    font-size: 23px;
  }

  .primary-card .summary-stat-label {
    font-size: 10px;
  }

  .primary-card .summary-stat-item .value-label {
    font-size: 11px;
  }
}

/* ===== 关注 Tab 视觉美化版 ===== */
.follow-tab {
  height: calc(100vh - 65px);
  padding: 10px 10px 0;
  background: radial-gradient(circle at 100% 0, rgba(82, 196, 26, 0.09), transparent 34%),
    linear-gradient(180deg, #f8faf9 0%, #f5f7f8 100%);
}

/* 顶部标题区 */
.follow-hero {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 13px 14px;
  margin-bottom: 10px;
  border: 1px solid rgba(76, 175, 80, 0.12);
  border-radius: 16px;
  background: linear-gradient(135deg, rgba(76, 175, 80, 0.12), rgba(255, 255, 255, 0.96) 52%), #fff;
  box-shadow: 0 6px 20px rgba(31, 45, 61, 0.06);
  flex-shrink: 0;
}

.follow-hero-copy {
  min-width: 0;
}

.follow-page-title {
  margin: 0;
  color: #1f2d24;
  font-size: 19px;
  line-height: 1.25;
  font-weight: 700;
  letter-spacing: 0.2px;
}

.follow-page-subtitle {
  margin: 4px 0 0;
  color: #8a9690;
  font-size: 11px;
  line-height: 1.35;
}

.follow-count-badge {
  min-width: 62px;
  padding: 7px 9px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border-radius: 13px;
  color: #329348;
  background: rgba(76, 175, 80, 0.1);
  flex-shrink: 0;
}

.follow-count-badge strong {
  font-size: 18px;
  line-height: 1;
}

.follow-count-badge span {
  margin-top: 3px;
  font-size: 10px;
  color: #6c8b74;
}

/* 搜索工具栏 */
.follow-header {
  gap: 9px;
  margin: 0 0 10px;
  padding: 8px;
  border: 1px solid #edf0ee;
  border-radius: 15px;
  background: rgba(255, 255, 255, 0.92);
  box-shadow: 0 4px 14px rgba(31, 45, 61, 0.045);
}

.search-wrapper {
  min-width: 0;
}

:deep(.follow-search-input.ant-input-affix-wrapper) {
  height: 42px !important;
  padding: 0 14px !important;
  border: 1px solid transparent !important;
  border-radius: 12px !important;
  background: #f3f6f4 !important;
  box-shadow: none !important;
}

:deep(.follow-search-input.ant-input-affix-wrapper-focused),
:deep(.follow-search-input.ant-input-affix-wrapper:hover) {
  border-color: rgba(76, 175, 80, 0.35) !important;
  background: #fff !important;
}

:deep(.follow-search-input input) {
  font-size: 13px !important;
  background: transparent !important;
}

.follow-btn.sync-btn {
  width: 42px !important;
  height: 42px !important;
  border: 0 !important;
  border-radius: 12px !important;
  background: linear-gradient(135deg, #45b85b, #2f9d4a) !important;
  box-shadow: 0 5px 13px rgba(47, 157, 74, 0.25) !important;
}

.follow-btn.sync-btn:not(:disabled):active {
  transform: scale(0.96);
}

.follow-btn.sync-btn:disabled {
  opacity: 0.55;
  box-shadow: none !important;
}

/* 账号 Tab 改成圆角胶囊 */
.follow-tab-wrapper {
  margin-bottom: 10px;
  padding: 4px;
  overflow: hidden;
  border: 1px solid #edf0ee;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.9);
  box-shadow: 0 3px 12px rgba(31, 45, 61, 0.04);
}

.follow-tab-scroll {
  width: 100%;
  padding-right: 0;
  overflow-x: auto;
}

.follow-custom-tabs {
  min-width: max-content;
}

:deep(.follow-custom-tabs .ant-tabs-nav) {
  margin: 0 !important;
}

:deep(.follow-custom-tabs .ant-tabs-nav::before) {
  display: none !important;
}

:deep(.follow-custom-tabs .ant-tabs-tab) {
  margin: 0 3px !important;
  padding: 7px 12px !important;
  border-radius: 10px !important;
  color: #7a8780 !important;
  font-size: 12px !important;
  transition: all 0.2s ease;
}

:deep(.follow-custom-tabs .ant-tabs-tab-active) {
  background: rgba(76, 175, 80, 0.11) !important;
}

:deep(.follow-custom-tabs .ant-tabs-tab-active .ant-tabs-tab-btn) {
  color: #319447 !important;
  font-weight: 600 !important;
}

:deep(.follow-custom-tabs .ant-tabs-ink-bar) {
  display: none !important;
}

/* 列表 */
.follow-list-container {
  height: auto !important;
  max-height: none !important;
  min-height: 0;
  gap: 10px !important;
  padding: 0 1px 18px !important;
}

/* 博主卡片 */
.follow-card {
  overflow: hidden !important;
  border: 1px solid #e9eeeb !important;
  border-radius: 16px !important;
  background: linear-gradient(180deg, #ffffff 0%, #fbfcfb 100%) !important;
  box-shadow: 0 5px 16px rgba(31, 45, 61, 0.055) !important;
  transition: transform 0.2s ease, box-shadow 0.2s ease, border-color 0.2s ease !important;
}

.follow-card::before {
  content: '';
  position: absolute;
  inset: 0 auto 0 0;
  width: 3px;
  background: #d9dedb;
  transition: background 0.2s ease;
}

.follow-card:has(.sync-state-text.enabled)::before {
  background: linear-gradient(180deg, #52c41a, #2f9d4a);
}

.follow-card:hover {
  transform: translateY(-1px);
  border-color: rgba(76, 175, 80, 0.24) !important;
  box-shadow: 0 8px 22px rgba(31, 45, 61, 0.08) !important;
}

.follow-card.no-followed-card {
  border-color: #f4dede !important;
  background: linear-gradient(180deg, #fffafa 0%, #fffefe 100%) !important;
}

.follow-card.no-followed-card::before {
  background: #ef8b8b;
}

.follow-card-inner {
  min-height: 0 !important;
  gap: 10px !important;
  padding: 13px 13px 12px 16px !important;
}

.follow-card-top {
  align-items: flex-start;
  gap: 11px;
}

/* 头像 */
.avatar-wrapper {
  width: 50px;
  height: 50px;
  padding: 2px;
  border: 1px solid rgba(76, 175, 80, 0.2);
  background: #fff;
  box-shadow: 0 3px 10px rgba(31, 45, 61, 0.08);
}

:deep(.follow-card .ant-avatar),
:deep(.follow-card .ant-avatar-lg) {
  width: 44px !important;
  height: 44px !important;
}

.name-actions {
  width: calc(100% - 61px) !important;
  gap: 7px;
}

.name-wrapper {
  align-items: flex-start;
  justify-content: space-between;
  gap: 6px;
  flex-wrap: nowrap;
}

.uper-title-line {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.uper-name {
  max-width: 100%;
  color: #26332b;
  font-size: 15px;
  line-height: 1.25;
  font-weight: 700;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.douyin-no {
  width: fit-content;
  max-width: 100%;
  padding: 2px 7px;
  border-radius: 999px;
  color: #8b9690;
  background: #f2f5f3;
  font-size: 10px;
  line-height: 1.4;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.no-followed-badge {
  height: 20px;
  padding: 0 7px;
  border-radius: 999px;
  font-size: 10px;
}

/* 同步状态与操作 */
.top-actions {
  justify-content: space-between;
  gap: 8px;
}

.sync-state-text {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  color: #9aa49f;
  font-size: 10px;
  white-space: nowrap;
}

.sync-state-text i {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: #c5ccc8;
  box-shadow: 0 0 0 3px rgba(197, 204, 200, 0.17);
}

.sync-state-text.enabled {
  color: #309348;
}

.sync-state-text.enabled i {
  background: #45b85b;
  box-shadow: 0 0 0 3px rgba(69, 184, 91, 0.14);
}

.delete-btn {
  width: 28px !important;
  height: 28px !important;
  border-radius: 8px !important;
  background: rgba(239, 68, 68, 0.07) !important;
}

/* 签名 */
.follow-card-desc {
  padding: 7px 9px;
  border-radius: 9px;
  color: #7b8781;
  background: #f6f8f7;
  font-size: 11px;
  line-height: 1.4;
}

.signature-text {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* 底部设置区 */
.follow-card-bottom {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 8px !important;
  padding-top: 9px !important;
  border-top-color: #edf1ef;
  align-items: center;
}

.path-area {
  min-width: 0;
  padding: 6px 8px;
  border-radius: 9px;
  background: #f5f7f6;
}

.path-text {
  min-width: 0;
  color: #59645e;
  font-size: 11px;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.edit-btn {
  width: 27px !important;
  height: 27px !important;
  border-radius: 8px !important;
}

.full-sync-area {
  width: auto;
  min-width: 88px;
  padding: 6px 8px;
  border-radius: 9px;
  background: rgba(76, 175, 80, 0.075);
}

.full-sync-label {
  color: #64806c;
  font-size: 10px;
  white-space: nowrap;
}

.edit-input-group {
  min-width: 0;
}

.path-input {
  min-width: 0;
  height: 29px !important;
  border-radius: 8px !important;
}

/* Ant Design 开关统一绿色 */
:deep(.follow-card .ant-switch-checked) {
  background: #43ad58 !important;
}

/* 空状态 */
.empty-container {
  min-height: 260px;
  margin-top: 6px !important;
  border: 1px dashed #dce3df;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.65);
}

/* 暗黑模式 */
html.dark-mode .follow-tab {
  background: radial-gradient(circle at 100% 0, rgba(76, 175, 80, 0.13), transparent 34%), #17172b;
}

html.dark-mode .follow-hero,
html.dark-mode .follow-header,
html.dark-mode .follow-tab-wrapper {
  border-color: rgba(255, 255, 255, 0.07);
  background: rgba(30, 30, 50, 0.88);
}

html.dark-mode .follow-page-title {
  color: #f1f4f2;
}

html.dark-mode .follow-page-subtitle,
html.dark-mode .follow-count-badge span {
  color: #9299a5;
}

html.dark-mode .follow-count-badge {
  color: #7fd38f;
  background: rgba(76, 175, 80, 0.15);
}

html.dark-mode :deep(.follow-search-input.ant-input-affix-wrapper) {
  background: rgba(255, 255, 255, 0.055) !important;
}

html.dark-mode .follow-card {
  border-color: rgba(255, 255, 255, 0.07) !important;
  background: linear-gradient(180deg, rgba(38, 38, 61, 0.96), rgba(31, 31, 52, 0.96)) !important;
}

html.dark-mode .uper-name {
  color: #f0f2f1 !important;
}

html.dark-mode .douyin-no,
html.dark-mode .follow-card-desc,
html.dark-mode .path-area {
  background: rgba(255, 255, 255, 0.055);
}

html.dark-mode .douyin-no,
html.dark-mode .follow-card-desc,
html.dark-mode .path-text {
  color: #a8aeb5 !important;
}

html.dark-mode .full-sync-area {
  background: rgba(76, 175, 80, 0.12);
}

html.dark-mode .empty-container {
  border-color: rgba(255, 255, 255, 0.08);
  background: rgba(30, 30, 50, 0.45);
}

/* 小屏适配 */
@media screen and (max-width: 375px) {
  .follow-tab {
    padding-left: 7px;
    padding-right: 7px;
  }

  .follow-hero {
    padding: 11px 12px;
  }

  .follow-page-title {
    font-size: 17px;
  }

  .follow-page-subtitle {
    font-size: 10px;
  }

  .follow-card-inner {
    padding: 12px 11px 11px 14px !important;
  }

  .follow-card-bottom {
    grid-template-columns: 1fr;
  }

  .full-sync-area {
    width: 100%;
    box-sizing: border-box;
  }
}

/* ===== 日志 Tab 视觉美化版 ===== */
.log-tab {
  min-height: calc(100vh - 65px);
  padding: 10px 10px 18px;
  box-sizing: border-box;
  background: radial-gradient(circle at 100% 0, rgba(33, 150, 243, 0.09), transparent 34%),
    linear-gradient(180deg, #f8fafc 0%, #f5f7f9 100%);
}

.log-hero {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 13px 14px;
  margin-bottom: 10px;
  border: 1px solid rgba(33, 150, 243, 0.12);
  border-radius: 16px;
  background: linear-gradient(135deg, rgba(33, 150, 243, 0.11), rgba(255, 255, 255, 0.96) 54%), #fff;
  box-shadow: 0 6px 20px rgba(31, 45, 61, 0.06);
}

.log-hero-copy {
  min-width: 0;
}

.log-page-title {
  margin: 0;
  color: #24313a;
  font-size: 19px;
  line-height: 1.25;
  font-weight: 700;
  letter-spacing: 0.2px;
}

.log-page-subtitle {
  margin: 4px 0 0;
  color: #89959e;
  font-size: 11px;
  line-height: 1.35;
}

.log-total-badge {
  min-width: 62px;
  padding: 7px 9px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border-radius: 13px;
  color: #1976d2;
  background: rgba(33, 150, 243, 0.1);
  flex-shrink: 0;
}

.log-total-badge strong {
  font-size: 18px;
  line-height: 1;
}

.log-total-badge span {
  margin-top: 3px;
  color: #6685a0;
  font-size: 10px;
}

/* 日志数量摘要 */
.log-summary {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 9px;
  margin-bottom: 10px;
}

.log-summary-card {
  min-width: 0;
  min-height: 72px;
  padding: 10px 11px;
  display: flex;
  align-items: center;
  gap: 10px;
  border: 1px solid #e8edf1;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.94);
  box-shadow: 0 4px 14px rgba(31, 45, 61, 0.045);
  text-align: left;
  cursor: pointer;
  transition: all 0.2s ease;
}

.log-summary-card:active {
  transform: scale(0.985);
}

.log-summary-card.active {
  transform: translateY(-1px);
}

.log-summary-card .summary-icon {
  width: 34px;
  height: 34px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 11px;
  flex-shrink: 0;
}

.debug-summary .summary-icon {
  color: #1976d2;
  background: rgba(33, 150, 243, 0.11);
}

.error-summary .summary-icon {
  color: #e53935;
  background: rgba(244, 67, 54, 0.1);
}

.debug-summary.active {
  border-color: rgba(33, 150, 243, 0.32);
  box-shadow: 0 6px 18px rgba(33, 150, 243, 0.1);
}

.error-summary.active {
  border-color: rgba(244, 67, 54, 0.28);
  box-shadow: 0 6px 18px rgba(244, 67, 54, 0.09);
}

.summary-copy {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.summary-copy .summary-label {
  color: #7d8991;
  font-size: 11px;
  white-space: nowrap;
}

.summary-copy strong {
  color: #27333b;
  font-size: 20px;
  line-height: 1;
}

/* 筛选器 */
.log-filter-shell {
  padding: 4px;
  margin-bottom: 10px;
  border: 1px solid #e9eef1;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.92);
  box-shadow: 0 3px 12px rgba(31, 45, 61, 0.04);
}

.log-filter {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 4px;
}

.log-filter .filter-btn {
  min-width: 0;
  padding: 8px 5px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 5px;
  border: 0;
  border-radius: 10px;
  color: #7a878f;
  background: transparent;
  font-size: 11px;
  font-weight: 600;
}

.log-filter .filter-btn span {
  min-width: 18px;
  padding: 1px 5px;
  border-radius: 999px;
  color: #929ca2;
  background: #eef2f4;
  font-size: 9px;
  line-height: 1.5;
}

.log-filter .filter-btn.active {
  color: #1976d2;
  background: rgba(33, 150, 243, 0.1);
}

.log-filter .filter-btn.active span {
  color: #fff;
  background: #3b94df;
}

/* 日志卡片列表 */
.log-list {
  display: flex;
  flex-direction: column;
  gap: 9px;
}

.log-item {
  position: relative;
  min-height: 76px;
  padding: 11px 10px 11px 12px;
  display: grid;
  grid-template-columns: 36px minmax(0, 1fr) 18px;
  gap: 9px;
  align-items: center;
  border: 1px solid #e9eef1;
  border-left: 1px solid #e9eef1;
  border-radius: 15px;
  background: linear-gradient(180deg, #ffffff 0%, #fbfcfd 100%);
  box-shadow: 0 5px 16px rgba(31, 45, 61, 0.05);
  overflow: hidden;
}

.log-item::before {
  content: '';
  position: absolute;
  inset: 0 auto 0 0;
  width: 3px;
}

.log-item.debug::before {
  background: linear-gradient(180deg, #42a5f5, #1976d2);
}

.log-item.error::before {
  background: linear-gradient(180deg, #ef5350, #d32f2f);
}

.log-item:hover {
  transform: translateY(-1px);
  box-shadow: 0 8px 20px rgba(31, 45, 61, 0.075);
}

.log-item-icon {
  width: 34px;
  height: 34px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 11px;
}

.log-item.debug .log-item-icon {
  color: #1976d2;
  background: rgba(33, 150, 243, 0.1);
}

.log-item.error .log-item-icon {
  color: #e53935;
  background: rgba(244, 67, 54, 0.1);
}

.log-item-main {
  min-width: 0;
}

.log-item-header {
  margin-bottom: 5px;
}

.log-type {
  padding: 2px 7px;
  border-radius: 999px;
  font-size: 9px;
  line-height: 1.45;
  letter-spacing: 0.3px;
}

.log-type.debug {
  color: #1976d2;
  background: rgba(33, 150, 243, 0.1);
}

.log-type.error {
  color: #d32f2f;
  background: rgba(244, 67, 54, 0.1);
}

.log-time {
  color: #98a2a8;
  font-size: 10px;
}

.log-file {
  min-width: 0;
  color: #34414a;
  font-size: 12px;
  line-height: 1.35;
  font-weight: 600;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.log-message-preview {
  margin-top: 4px;
  color: #8b969d;
  font-size: 10px;
  line-height: 1.35;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.log-item-arrow {
  color: #bcc4c9;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* 空状态 */
.empty-log {
  min-height: 280px;
  padding: 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border: 1px dashed #dbe3e8;
  border-radius: 16px;
  color: #9aa4aa;
  background: rgba(255, 255, 255, 0.62);
}

.empty-log-icon {
  width: 60px;
  height: 60px;
  margin-bottom: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 18px;
  color: #8fa2ae;
  background: #eef3f6;
}

.empty-log strong {
  margin-bottom: 5px;
  color: #65727a;
  font-size: 14px;
}

.empty-log span {
  color: #a1aaaf;
  font-size: 11px;
}

/* 暗黑模式 */
html.dark-mode .log-tab {
  background: radial-gradient(circle at 100% 0, rgba(33, 150, 243, 0.14), transparent 34%), #17172b;
}

html.dark-mode .log-hero,
html.dark-mode .log-summary-card,
html.dark-mode .log-filter-shell {
  border-color: rgba(255, 255, 255, 0.07);
  background: rgba(30, 30, 50, 0.88);
}

html.dark-mode .log-page-title {
  color: #f1f4f6;
}

html.dark-mode .log-page-subtitle,
html.dark-mode .log-total-badge span {
  color: #9299a5;
}

html.dark-mode .log-total-badge {
  color: #7dbbea;
  background: rgba(33, 150, 243, 0.15);
}

html.dark-mode .summary-copy .summary-label {
  color: #9da4ae;
}

html.dark-mode .summary-copy strong {
  color: #f2f4f5;
}

html.dark-mode .log-filter .filter-btn {
  color: #a8afb7;
}

html.dark-mode .log-filter .filter-btn span {
  color: #adb4bc;
  background: rgba(255, 255, 255, 0.07);
}

html.dark-mode .log-item {
  border-color: rgba(255, 255, 255, 0.07);
  background: linear-gradient(180deg, rgba(38, 38, 61, 0.96), rgba(31, 31, 52, 0.96));
}

html.dark-mode .log-file {
  color: #e5e8ea;
}

html.dark-mode .log-message-preview,
html.dark-mode .log-time {
  color: #9ca4ad;
}

html.dark-mode .empty-log {
  border-color: rgba(255, 255, 255, 0.08);
  background: rgba(30, 30, 50, 0.45);
}

html.dark-mode .empty-log-icon {
  color: #8ca4b5;
  background: rgba(255, 255, 255, 0.06);
}

html.dark-mode .empty-log strong {
  color: #d8dde0;
}

/* 小屏适配 */
@media screen and (max-width: 375px) {
  .log-tab {
    padding-left: 7px;
    padding-right: 7px;
  }

  .log-hero {
    padding: 11px 12px;
  }

  .log-page-title {
    font-size: 17px;
  }

  .log-page-subtitle {
    font-size: 10px;
  }

  .log-summary {
    gap: 6px;
  }

  .log-summary-card {
    min-height: 66px;
    padding: 8px;
    gap: 7px;
  }

  .log-summary-card .summary-icon {
    width: 30px;
    height: 30px;
  }

  .summary-copy strong {
    font-size: 18px;
  }

  .log-item {
    grid-template-columns: 32px minmax(0, 1fr) 15px;
    gap: 7px;
    padding: 9px 8px 9px 10px;
  }

  .log-item-icon {
    width: 30px;
    height: 30px;
  }
}

/* ===== 看板 Tab：高级视觉优化 ===== */

/* 仅优化第一个看板 Tab，不修改其他 Tab 和业务逻辑 */
.tab-content:has(.stats-overview) {
  min-height: calc(100vh - 70px);
  padding: 7px 7px 14px;
  box-sizing: border-box;
  background: radial-gradient(circle at 100% 0, rgba(76, 175, 80, 0.09), transparent 30%),
    radial-gradient(circle at 0 42%, rgba(33, 150, 243, 0.055), transparent 28%),
    linear-gradient(180deg, #f8faf9 0%, #f4f7f6 100%);
}

.stats-overview {
  gap: 12px !important;
  margin-bottom: 0;
}

/* 主卡片统一 */
.stats-overview .main-card {
  position: relative;
  overflow: hidden;
  padding: 0 12px 13px;
  border: 1px solid rgba(222, 230, 225, 0.95);
  border-radius: 18px;
  background: linear-gradient(180deg, rgba(255, 255, 255, 0.99), rgba(251, 253, 252, 0.98));
  box-shadow: 0 8px 26px rgba(31, 45, 61, 0.065), 0 1px 2px rgba(31, 45, 61, 0.025);
}

.stats-overview .main-card:hover {
  transform: none;
  box-shadow: 0 9px 28px rgba(31, 45, 61, 0.075), 0 1px 2px rgba(31, 45, 61, 0.03);
}

.stats-overview .main-card::after {
  content: '';
  position: absolute;
  top: -48px;
  right: -48px;
  width: 128px;
  height: 128px;
  border-radius: 50%;
  pointer-events: none;
}

.stats-overview .primary-card {
  border-top: 0;
}

.stats-overview .primary-card::before {
  content: '';
  position: absolute;
  inset: 0 0 auto;
  height: 4px;
  background: linear-gradient(90deg, #2fa84f, #72d38a);
}

.stats-overview .primary-card::after {
  background: radial-gradient(circle, rgba(76, 175, 80, 0.12), transparent 66%);
}

.stats-overview .secondary-card {
  border-top: 0;
}

.stats-overview .secondary-card::before {
  content: '';
  position: absolute;
  inset: 0 0 auto;
  height: 4px;
  background: linear-gradient(90deg, #1688e8, #63b8f4);
}

.stats-overview .secondary-card::after {
  background: radial-gradient(circle, rgba(33, 150, 243, 0.11), transparent 66%);
}

/* 卡片标题 */
.stats-overview .stat-header {
  position: relative;
  z-index: 1;
  min-height: 42px;
  margin: 0 0 9px;
  padding-top: 9px;
  align-items: center;
}

.stats-overview .stat-meta {
  position: relative;
  width: fit-content;
  margin: 0 auto;
  padding: 4px 12px 4px 25px;
  border-radius: 999px;
  color: #34423a;
  background: rgba(245, 248, 246, 0.92);
  font-size: 13px;
  line-height: 1.35;
  font-weight: 700;
  letter-spacing: 0.2px;
  text-align: center;
  text-transform: none;
}

.stats-overview .primary-card .stat-meta::before,
.stats-overview .secondary-card .stat-meta::before {
  content: '';
  position: absolute;
  left: 10px;
  top: 50%;
  width: 7px;
  height: 7px;
  border-radius: 50%;
  transform: translateY(-50%);
}

.stats-overview .primary-card .stat-meta::before {
  background: #38ae56;
  box-shadow: 0 0 0 4px rgba(56, 174, 86, 0.12);
}

.stats-overview .secondary-card .stat-meta::before {
  background: #2196f3;
  box-shadow: 0 0 0 4px rgba(33, 150, 243, 0.11);
}

/* 总视频数与存储总计 */
.primary-card .stat-value-container {
  position: relative;
  z-index: 1;
  margin: 0 0 10px;
  padding: 6px;
  border: 1px solid #e9eeeb;
  border-radius: 15px;
  background: linear-gradient(135deg, rgba(76, 175, 80, 0.075), rgba(255, 255, 255, 0.94) 52%);
}

.primary-card .summary-stat-item {
  min-height: 72px;
  gap: 5px;
  padding: 8px 6px;
  border-radius: 11px;
}

.primary-card .summary-stat-divider {
  margin: 10px 0;
  background: linear-gradient(180deg, transparent, #dfe6e2 20%, #dfe6e2 80%, transparent);
}

.primary-card .summary-stat-label {
  color: #8a958f;
  font-size: 10px;
  font-weight: 600;
  letter-spacing: 0.35px;
}

.primary-card .summary-stat-item .stat-value,
.primary-card .summary-stat-item .video-count,
.primary-card .summary-stat-item .size-count {
  color: #243029;
  font-size: 28px;
  font-weight: 800;
  letter-spacing: -0.5px;
}

.primary-card .summary-stat-item .value-label {
  margin-left: 3px;
  color: #849089;
  font-size: 11px;
  font-weight: 600;
}

/* 六项统计卡片 */
.primary-card .stat-subitems {
  position: relative;
  z-index: 1;
  gap: 8px;
  padding-top: 0;
  border-top: 0;
}

.primary-card .stat-subitems .subitem {
  position: relative;
  min-height: 64px;
  padding: 8px 8px;
  overflow: hidden;
  border: 1px solid #ebefed;
  border-radius: 12px;
  background: rgba(250, 252, 251, 0.96);
  box-shadow: none;
}

.primary-card .stat-subitems .subitem::after {
  content: '';
  position: absolute;
  inset: auto 0 0;
  height: 2px;
  opacity: 0.45;
}

.primary-card .stat-subitems .subitem:nth-child(1)::after {
  background: #ec407a;
}

.primary-card .stat-subitems .subitem:nth-child(2)::after {
  background: #ffa726;
}

.primary-card .stat-subitems .subitem:nth-child(3)::after {
  background: #ab47bc;
}

.primary-card .stat-subitems .subitem:nth-child(4)::after {
  background: #ef5350;
}

.primary-card .stat-subitems .subitem:nth-child(5)::after {
  background: #7e57c2;
}

.primary-card .stat-subitems .subitem:nth-child(6)::after {
  background: #5c6bc0;
}

.primary-card .stat-subitems .subitem:hover {
  border-color: #dde5e0;
  background: #ffffff;
  box-shadow: 0 5px 14px rgba(31, 45, 61, 0.055);
}

.primary-card .stat-subitems .subitem-icon {
  width: 30px;
  height: 30px;
  border-radius: 9px;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.45);
}

.primary-card .stat-subitems .subitem-meta {
  color: #4e5b54;
  font-size: 11px;
  font-weight: 600;
}

.primary-card .stat-subitems .subitem-count {
  color: #26322b;
  font-weight: 700;
}

.primary-card .stat-subitems .subitem-size {
  color: #929d97;
  font-size: 10px;
  font-weight: 500;
}

/* 最新同步 */
.secondary-card .stat-header {
  margin-bottom: 7px;
}

.secondary-card .video-list-container {
  position: relative;
  z-index: 1;
  padding-top: 0;
  border-top: 0;
  gap: 8px;
  counter-reset: latest-video;
}

.secondary-card .video-item {
  position: relative;
  min-height: 56px;
  padding: 9px 10px 9px 42px;
  border: 1px solid #e9eef2;
  border-radius: 12px;
  background: linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(249, 252, 254, 0.98));
  box-shadow: none;
  counter-increment: latest-video;
}

.secondary-card .video-item::before {
  content: counter(latest-video);
  position: absolute;
  left: 10px;
  top: 50%;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  transform: translateY(-50%);
  color: #237fca;
  background: rgba(33, 150, 243, 0.1);
  font-size: 10px;
  line-height: 1;
  font-weight: 800;
}

.secondary-card .video-item:hover {
  transform: none;
  border-color: rgba(33, 150, 243, 0.22);
  background: #ffffff;
  box-shadow: 0 5px 14px rgba(31, 45, 61, 0.055);
}

.secondary-card .video-info {
  min-width: 0;
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 7px;
  align-items: end;
}

.secondary-card .video-title {
  display: block;
  max-height: none;
  padding: 0;
  color: #3a4750;
  font-size: 11px;
  line-height: 1.45;
  font-weight: 600;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.secondary-card .video-time {
  margin: 0;
  color: #9ba5ab;
  font-size: 9px !important;
  line-height: 1.4;
  white-space: nowrap;
}

.secondary-card .empty-video-list {
  min-height: 150px;
  padding: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px dashed #dce5ea;
  border-radius: 13px;
  color: #9aa5ab;
  background: rgba(248, 251, 253, 0.8);
  font-size: 12px;
}

/* 底部导航前预留更自然 */
.stats-overview .secondary-card {
  margin-bottom: 3px;
}

/* 暗黑模式 */
html.dark-mode .tab-content:has(.stats-overview) {
  background: radial-gradient(circle at 100% 0, rgba(76, 175, 80, 0.12), transparent 30%),
    radial-gradient(circle at 0 42%, rgba(33, 150, 243, 0.09), transparent 28%), #17172b;
}

html.dark-mode .stats-overview .main-card {
  border-color: rgba(255, 255, 255, 0.075);
  background: linear-gradient(180deg, rgba(37, 37, 59, 0.98), rgba(29, 29, 49, 0.98));
  box-shadow: none;
}

html.dark-mode .stats-overview .stat-meta {
  color: #e4e7e5;
  background: rgba(255, 255, 255, 0.055);
}

html.dark-mode .primary-card .stat-value-container {
  border-color: rgba(255, 255, 255, 0.07);
  background: linear-gradient(135deg, rgba(76, 175, 80, 0.12), rgba(255, 255, 255, 0.025) 56%);
}

html.dark-mode .primary-card .summary-stat-divider {
  background: linear-gradient(
    180deg,
    transparent,
    rgba(255, 255, 255, 0.1) 20%,
    rgba(255, 255, 255, 0.1) 80%,
    transparent
  );
}

html.dark-mode .primary-card .summary-stat-label {
  color: #9ca6a0;
}

html.dark-mode .primary-card .summary-stat-item .stat-value,
html.dark-mode .primary-card .summary-stat-item .video-count,
html.dark-mode .primary-card .summary-stat-item .size-count {
  color: #ffffff;
}

html.dark-mode .primary-card .stat-subitems .subitem,
html.dark-mode .secondary-card .video-item {
  border-color: rgba(255, 255, 255, 0.06);
  background: rgba(255, 255, 255, 0.035);
}

html.dark-mode .primary-card .stat-subitems .subitem:hover,
html.dark-mode .secondary-card .video-item:hover {
  border-color: rgba(255, 255, 255, 0.11);
  background: rgba(255, 255, 255, 0.055);
  box-shadow: none;
}

html.dark-mode .primary-card .stat-subitems .subitem-meta,
html.dark-mode .secondary-card .video-title {
  color: #d7dcd9;
}

html.dark-mode .primary-card .stat-subitems .subitem-count {
  color: #ffffff;
}

html.dark-mode .primary-card .stat-subitems .subitem-size,
html.dark-mode .secondary-card .video-time {
  color: #969e9a;
}

html.dark-mode .secondary-card .video-item::before {
  color: #7fc3f5;
  background: rgba(33, 150, 243, 0.14);
}

html.dark-mode .secondary-card .empty-video-list {
  border-color: rgba(255, 255, 255, 0.08);
  color: #8f98a0;
  background: rgba(255, 255, 255, 0.025);
}

/* 极窄屏 */
@media screen and (max-width: 360px) {
  .tab-content:has(.stats-overview) {
    padding-left: 5px;
    padding-right: 5px;
  }

  .stats-overview .main-card {
    padding-left: 9px;
    padding-right: 9px;
    border-radius: 15px;
  }

  .primary-card .summary-stat-item {
    min-height: 66px;
    padding-left: 4px;
    padding-right: 4px;
  }

  .primary-card .summary-stat-item .stat-value,
  .primary-card .summary-stat-item .video-count,
  .primary-card .summary-stat-item .size-count {
    font-size: 25px;
  }

  .primary-card .stat-subitems {
    gap: 6px;
  }

  .primary-card .stat-subitems .subitem {
    min-height: 60px;
    padding: 7px 6px;
  }

  .primary-card .stat-subitems .subitem-icon {
    width: 27px;
    height: 27px;
  }

  .secondary-card .video-item {
    padding-left: 38px;
  }

  .secondary-card .video-info {
    grid-template-columns: 1fr;
    gap: 2px;
  }

  .secondary-card .video-time {
    justify-content: flex-start;
  }
}
</style>