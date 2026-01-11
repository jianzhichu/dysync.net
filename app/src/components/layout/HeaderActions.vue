<script lang="ts" setup>
// 步骤2：声明父组件传递的 showSetting 事件（关键！）
const emits = defineEmits(['showSetting']);
import { ref, onMounted } from 'vue'; // 移除 h、VNode 导入
import { StepinHeaderAction } from 'stepin';
import DayNightSwitch from '@/components/switch/DayNightSwitch.vue';
import { TagsOutlined, CopyOutlined, GithubOutlined } from '@ant-design/icons-vue';
import Fullscreen from '../fullscreen/Fullscreen.vue';
import { useApiStore } from '@/store';
import { message, Popover } from 'ant-design-vue'; // 移除 notification

// 版本数据和当前版本状态
const dyVersions = ref<string[]>([]);
const copyLoading = ref<Record<string, boolean>>({});
// 版本 popover 显隐控制
const versionPopoverVisible = ref<boolean>(false);

// 仓库地址常量
const gitRepos = ref([
  {
    name: 'Gitee',
    url: 'https://gitee.com/deathvicky/dysync.net',
    color: '#FF6600',
  },
  {
    name: 'GitHub',
    url: 'https://github.com/jianzhichu/dysync.net',
    color: '#6c35de',
  },
]);
const gitCopyLoading = ref<Record<string, boolean>>({});
const popoverVisible = ref<boolean>(false);

// 复制版本号方法（保留原有兼容逻辑，无改动）
const copyVersion = (version: string) => {
  const pureVersion = version.replace('（当前版本）', '').replace('（最新版）', '').trim();
  copyLoading.value[version] = true;

  const doCopy = async () => {
    try {
      if (navigator.clipboard && typeof navigator.clipboard.writeText === 'function') {
        await navigator.clipboard.writeText(pureVersion);
        message.success(`已复制版本: ${pureVersion}`);
        return;
      }

      const textarea = document.createElement('textarea');
      textarea.style.position = 'absolute';
      textarea.style.top = '-9999px';
      textarea.style.left = '-9999px';
      textarea.value = pureVersion;
      document.body.appendChild(textarea);

      textarea.select();
      const success = document.execCommand('copy');
      document.body.removeChild(textarea);

      if (success) {
        message.success(`已复制版本: ${pureVersion}`);
      } else {
        throw new Error('execCommand 复制失败');
      }
    } catch (error) {
      message.warning(`复制失败，请手动复制：${pureVersion}`);
      console.warn('复制版本号失败：', error);
    } finally {
      copyLoading.value[version] = false;
    }
  };

  doCopy();
};

// 辅助方法：判断是否为当前版本（模板中使用）
const isCurrentVersion = (version: string) => {
  return version.includes('（当前版本）');
};

// 辅助方法：判断是否为最新版本（模板中使用）
const isLatestVersion = (version: string) => {
  return version.includes('（最新版）');
};

// 复制仓库地址方法（保留原有逻辑，无改动）
const copyGitUrl = (repo: { name: string; url: string; color: string }) => {
  gitCopyLoading.value[repo.url] = true;
  const doCopy = async () => {
    try {
      if (navigator.clipboard && typeof navigator.clipboard.writeText === 'function') {
        await navigator.clipboard.writeText(repo.url);
        message.success(`已复制 ${repo.name} 地址`);
        return;
      }

      const textarea = document.createElement('textarea');
      textarea.style.position = 'absolute';
      textarea.style.top = '-9999px';
      textarea.style.left = '-9999px';
      textarea.value = repo.url;
      document.body.appendChild(textarea);
      textarea.select();
      const success = document.execCommand('copy');
      document.body.removeChild(textarea);

      if (success) {
        message.success(`已复制 ${repo.name} 地址`);
      } else {
        throw new Error('execCommand 复制失败');
      }
    } catch (error) {
      message.warning(`复制 ${repo.name} 地址失败，请手动复制`);
      console.warn(`复制 ${repo.name} 地址失败：`, error);
    } finally {
      gitCopyLoading.value[repo.url] = false;
    }
  };

  doCopy();
};

// 版本查看方法（获取数据后控制 popover 显隐）
const showVersionNotification = () => {
  useApiStore()
    .CheckTag()
    .then((res) => {
      if (res.code === 0) {
        dyVersions.value = res.data;
        const versionLen = dyVersions.value.length;

        if (versionLen > 0) {
          const newVersions = [...dyVersions.value];
          if (versionLen === 1) {
            newVersions[0] = `${newVersions[0]}（最新版）`;
          } else {
            newVersions[0] = `${newVersions[0]}（当前版本）`;
            const lastIndex = versionLen - 1;
            newVersions[lastIndex] = `${newVersions[lastIndex]}（最新版）`;
          }
          dyVersions.value = newVersions;
        }

        // 打开版本 popover
        versionPopoverVisible.value = true;
      } else {
        message.error(res.message);
      }
    })
    .catch((err) => {
      message.error('获取版本列表失败');
      console.error(err);
    });
};

// 全局注入加载动画样式
onMounted(() => {
  if (!document.querySelector('#custom-spin-style')) {
    const style = document.createElement('style');
    style.id = 'custom-spin-style';
    style.textContent = `
      @keyframes custom-spin {
        from { transform: rotate(0deg); }
        to { transform: rotate(360deg); }
      }
    `;
    document.head.appendChild(style);
  }
});

// Git 弹窗控制方法
const showGit = () => {
  popoverVisible.value = !popoverVisible.value;
  console.log('打开开源地址弹窗');
};
</script>

<template>
  <StepinHeaderAction>
    <DayNightSwitch />
  </StepinHeaderAction>

  <!-- 版本查看：纯模板实现，移除 renderVersionList -->
  <StepinHeaderAction>
    <div class="action-item">
      <a-popover v-model:visible="versionPopoverVisible" placement="bottom" trigger="click" overlay-class="version-popover-overlay" @visible-change="(visible) => versionPopoverVisible = visible">
        <template #content>
          <div class="version-popover-content">
            <!-- 版本列表：纯模板 v-for 实现，替代 VNode 渲染 -->
            <div class="custom-version-list" v-if="dyVersions.length > 0">
              <div class="custom-version-item" :class="{
                  'custom-current-version': isCurrentVersion(version),
                  'custom-latest-version': isLatestVersion(version)
                }" v-for="(version, index) in dyVersions" :key="index" :title="isCurrentVersion(version)
                  ? '当前使用版本 - 点击右侧按钮复制版本号'
                  : isLatestVersion(version)
                  ? '最新版本 - 点击右侧按钮复制版本号'
                  : '点击右侧按钮复制版本号'">
                <!-- 版本文本 -->
                <div class="version-text">
                  <span :style="{
                      color: isCurrentVersion(version) ? '#1890ff' : isLatestVersion(version) ? '#2e7d32' : '#1f2937'
                    }">
                    {{ version }}
                  </span>
                </div>
                <!-- 复制按钮 -->
                <button class="version-copy-btn" :disabled="copyLoading[version]" @click.stop="copyVersion(version)" :title="copyLoading[version] ? '复制中...' : '复制版本号'">
                  <CopyOutlined :style="{
                      fontSize: '14px',
                      color: copyLoading[version]
                        ? '#d1d5db'
                        : isCurrentVersion(version)
                        ? '#1890ff'
                        : isLatestVersion(version)
                        ? '#2e7d32'
                        : '#9ca3af',
                      animation: copyLoading[version] ? 'custom-spin 1s linear infinite' : 'none'
                    }" />
                </button>
              </div>
            </div>
          </div>
        </template>
        <a-tooltip placement="bottom">
          <template #title>
            <span>版本查看</span>
          </template>
          <TagsOutlined class="action-icon" @click="showVersionNotification" />
        </a-tooltip>
      </a-popover>
    </div>
  </StepinHeaderAction>

  <!-- Git 开源地址弹窗（保留原有实现） -->
  <StepinHeaderAction>
    <div class="action-item">
      <a-popover v-model:visible="popoverVisible" placement="bottom" trigger="click" overlay-class="git-popover-overlay" @visible-change="(visible) => popoverVisible = visible">
        <template #content>
          <div class="git-popover-content">
            <div class="git-repo-item" v-for="repo in gitRepos" :key="repo.url">
              <div class="git-repo-name" :style="{ color: repo.color }">
                <span class="git-repo-tag" :style="{ backgroundColor: repo.color }"></span>
                {{ repo.name }}
              </div>
              <div class="git-repo-url-wrapper">
                <a :href="repo.url" target="_blank" class="git-repo-url" :style="{ color: repo.color }" title="点击打开仓库地址">
                  {{ repo.url }}
                </a>
                <button class="git-copy-btn" :disabled="gitCopyLoading[repo.url]" @click.stop="copyGitUrl(repo)" title="复制仓库地址">
                  <CopyOutlined :style="{
                      fontSize: '12px',
                      color: gitCopyLoading[repo.url] ? '#d1d5db' : repo.color,
                      animation: gitCopyLoading[repo.url] ? 'custom-spin 1s linear infinite' : 'none'
                    }" />
                </button>
              </div>
            </div>
          </div>
        </template>
        <a-tooltip placement="bottom">
          <template #title>
            <span>项目开源地址</span>
          </template>
          <GithubOutlined class="action-icon" @click="showGit" />
        </a-tooltip>
      </a-popover>
    </div>
  </StepinHeaderAction>

  <StepinHeaderAction>
    <Fullscreen class="-mx-xs -my-sm h-[56px] px-xs py-sm flex items-center" target=".stepin-layout" />
  </StepinHeaderAction>
</template>

<style scoped lang="less">
/* 顶部按钮样式 */
.action-item {
  font-size: 20px;
  height: 100%;
  margin: 0 -8px;
  padding: 0 4px;
  line-height: 40px;
  display: flex;
  align-items: center;
  cursor: pointer;

  &:hover {
    color: #1890ff;
  }
}

.action-icon {
  font-size: 20px;
}

/* 版本 Popover 样式（纯模板适配） */
:deep(.version-popover-overlay) {
  .ant-popover-inner {
    padding: 16px;
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
    width: 520px;
  }

  .ant-popover-arrow-content {
    background-color: #fff;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  }
}

.version-popover-content {
  width: 100%;
  box-sizing: border-box;
}

/* 版本列表（模板对应样式） */
.custom-version-list {
  width: 100%;
  padding: 8px 0;
  box-sizing: border-box;
}

.custom-version-item {
  width: 100%;
  padding: 8px 12px;
  border-bottom: 1px solid #f0f0f0;
  border-radius: 4px;
  transition: all 0.2s ease;
  box-sizing: border-box;
  display: flex;
  align-items: center;
  justify-content: space-between;
  white-space: nowrap;
  background-color: transparent;

  &:hover {
    background-color: #f5fafe;
  }

  &:last-child {
    border-bottom: none;
  }
}

/* 当前版本/最新版本高亮 */
.custom-current-version {
  background-color: rgba(24, 144, 255, 0.05) !important;

  &:hover {
    background-color: rgba(24, 144, 255, 0.1) !important;
  }
}

.custom-latest-version {
  background-color: rgba(46, 125, 50, 0.05) !important;

  &:hover {
    background-color: rgba(46, 125, 50, 0.1) !important;
  }
}

/* 版本文本容器 */
.version-text {
  display: flex;
  align-items: center;
  maxwidth: calc(100% - 40px);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* 版本复制按钮 */
.version-copy-btn {
  width: 24px;
  height: 24px;
  border: none;
  border-radius: 4px;
  background-color: transparent;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
  margin: 0;
  flex-shrink: 0;
  transition: all 0.2s ease;

  &:hover {
    background-color: rgba(24, 144, 255, 0.1);
    color: #1890ff !important;
  }

  &:disabled {
    cursor: not-allowed;
    opacity: 0.5;
  }
}

/* 暂无版本数据 */
.no-version-data {
  text-align: center;
  color: #999;
  padding: 20px 0;
  font-size: 14px;
  background-color: #f9fafb;
  border-radius: 8px;
  margin: 0 12px;
}

/* Git Popover 样式 */
:deep(.git-popover-overlay) {
  .ant-popover-inner {
    padding: 16px;
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
    width: 360px;
  }

  .ant-popover-arrow-content {
    background-color: #fff;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  }
}

.git-popover-content {
  width: 100%;
  box-sizing: border-box;
}

.git-repo-item {
  margin-bottom: 12px;

  &:last-child {
    margin-bottom: 0;
  }
}

.git-repo-name {
  display: flex;
  align-items: center;
  font-size: 14px;
  font-weight: 500;
  margin-bottom: 6px;
}

.git-repo-tag {
  display: inline-block;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  margin-right: 6px;
}

.git-repo-url-wrapper {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 12px;
  background-color: #f9fafb;
  border-radius: 4px;
  font-size: 13px;
}

.git-repo-url {
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  text-decoration: none;

  &:hover {
    text-decoration: underline;
    opacity: 0.8;
  }
}

.git-copy-btn {
  width: 24px;
  height: 24px;
  border: none;
  border-radius: 4px;
  background-color: transparent;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-left: 8px;
  flex-shrink: 0;

  &:hover {
    background-color: rgba(0, 0, 0, 0.05);
  }

  &:disabled {
    cursor: not-allowed;
    opacity: 0.5;
  }
}

/* 根容器样式 */
.header-actions-root {
  display: flex;
  align-items: center;
  gap: 8px;
}
</style>