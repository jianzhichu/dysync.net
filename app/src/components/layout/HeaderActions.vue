<script lang="ts" setup>
import { ref, h, VNode, onMounted } from 'vue';
import { StepinHeaderAction } from 'stepin';
import DayNightSwitch from '@/components/switch/DayNightSwitch.vue';
import { TagsOutlined, CopyOutlined } from '@ant-design/icons-vue'; // 移除不需要的图标
import Fullscreen from '../fullscreen/Fullscreen.vue';
import { useApiStore } from '@/store';
import { notification, message } from 'ant-design-vue';

// 版本数据和当前版本状态
const dyVersions = ref<string[]>([]);
const copyLoading = ref<Record<string, boolean>>({}); // 复制按钮加载状态
const notificationKey = ref<string>('version-notification');

// 复制版本号方法（优化：去除“（当前版本）”标记，只复制纯版本号）
// 复制版本号方法（优化：兼容所有浏览器，修复 navigator.clipboard 不存在的问题）
const copyVersion = (version: string) => {
  const pureVersion = version.replace('（当前版本）', '').replace('（最新版）', '').trim(); // 过滤所有标记
  copyLoading.value[version] = true;

  // 兼容方案：优先使用现代 API，降级使用传统方法
  const doCopy = async () => {
    try {
      // 方案1：现代浏览器 + HTTPS 环境（优先）
      if (navigator.clipboard && typeof navigator.clipboard.writeText === 'function') {
        await navigator.clipboard.writeText(pureVersion);
        message.success(`已复制版本: ${pureVersion}`);
        return;
      }

      // 方案2：降级使用 document.execCommand（兼容 HTTP/旧浏览器）
      const textarea = document.createElement('textarea');
      // 隐藏文本域（避免影响页面）
      textarea.style.position = 'absolute';
      textarea.style.top = '-9999px';
      textarea.style.left = '-9999px';
      textarea.value = pureVersion;
      document.body.appendChild(textarea);

      // 选中并复制
      textarea.select();
      const success = document.execCommand('copy');
      document.body.removeChild(textarea); // 清理 DOM

      if (success) {
        message.success(`已复制版本: ${pureVersion}`);
      } else {
        throw new Error('execCommand 复制失败');
      }
    } catch (error) {
      // 方案3：最终降级 - 提示手动复制
      message.warning(`复制失败，请手动复制：${pureVersion}`);
      console.warn('复制版本号失败：', error);
    } finally {
      copyLoading.value[version] = false;
    }
  };

  doCopy();
};

// 定义版本列表组件（移除所有额外当前版本标记）
// 定义版本列表组件（完整版本：保留所有原有逻辑+样式优化+单行显示）
const renderVersionList = (): VNode => {
  return h(
    'div',
    {
      class: 'custom-version-list',
      style: {
        width: '100%',
        padding: '10px 0',
        boxSizing: 'border-box',
        display: 'block',
      },
    },
    [
      dyVersions.value.length > 0
        ? dyVersions.value.map((tag, index) => {
            // 判断是否包含当前版本/最新版标记
            const isCurrentVersion = tag.includes('（当前版本）');
            const isLatestVersion = tag.includes('（最新版）');

            return h(
              'div',
              {
                class: [
                  'custom-version-item',
                  isCurrentVersion ? 'custom-current-version' : '',
                  isLatestVersion ? 'custom-latest-version' : '',
                ],
                key: index,
                title: isCurrentVersion
                  ? '当前使用版本 - 点击右侧按钮复制版本号'
                  : isLatestVersion
                  ? '最新版本 - 点击右侧按钮复制版本号'
                  : '点击右侧按钮复制版本号',
                style: {
                  width: '100%',
                  padding: '8px 12px',
                  borderBottom: '1px solid #f0f0f0',
                  borderRadius: '4px',
                  transition: 'all 0.2s ease',
                  boxSizing: 'border-box',
                  // 核心：Flex布局确保文字+按钮一行显示
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  whiteSpace: 'nowrap', // 禁止整行换行
                  // 版本标记背景色（区分当前版和最新版）
                  backgroundColor: isCurrentVersion
                    ? 'rgba(24, 144, 255, 0.05)'
                    : isLatestVersion
                    ? 'rgba(46, 125, 50, 0.05)'
                    : 'transparent',
                },
              },
              [
                // 文本容器（单行溢出省略+标记颜色区分）
                h(
                  'div',
                  {
                    style: {
                      display: 'flex',
                      alignItems: 'center',
                      // 给复制按钮留固定宽度，避免文本挤压
                      maxWidth: 'calc(100% - 40px)',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap', // 文本单行显示
                    },
                  },
                  [
                    h(
                      'span',
                      {
                        style: {
                          fontSize: '14px',
                          color: isCurrentVersion
                            ? '#1890ff' // 当前版本文字色
                            : isLatestVersion
                            ? '#2e7d32' // 最新版本文字色
                            : '#1f2937', // 普通版本文字色
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                          whiteSpace: 'nowrap',
                        },
                      },
                      tag // 显示带标记的完整文本（如：v1.0.0（当前版本））
                    ),
                  ]
                ),
                // 复制按钮（固定大小+加载动画+hover效果）
                h(
                  'button',
                  {
                    style: {
                      width: '24px',
                      height: '24px',
                      border: 'none',
                      borderRadius: '4px',
                      backgroundColor: 'transparent',
                      color: copyLoading.value[tag]
                        ? '#d1d5db' // 加载中颜色
                        : isCurrentVersion
                        ? '#1890ff' // 当前版本按钮色
                        : isLatestVersion
                        ? '#2e7d32' // 最新版本按钮色
                        : '#9ca3af', // 普通版本按钮色
                      cursor: copyLoading.value[tag] ? 'not-allowed' : 'pointer',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      padding: '0',
                      margin: '0',
                      flexShrink: '0', // 禁止按钮收缩
                      transition: 'all 0.2s ease',
                    },
                    onClick: (e: Event) => {
                      e.stopPropagation(); // 阻止事件冒泡
                      copyVersion(tag);
                    },
                    disabled: copyLoading.value[tag],
                    title: copyLoading.value[tag] ? '复制中...' : '复制版本号',
                  },
                  [
                    h(CopyOutlined, {
                      style: {
                        fontSize: '14px',
                        // 加载时旋转动画
                        animation: copyLoading.value[tag] ? 'custom-spin 1s linear infinite' : 'none',
                      },
                    }),
                  ]
                ),
              ]
            );
          })
        : h(
            'div',
            {
              style: {
                textAlign: 'center',
                color: '#999',
                padding: '20px 0',
                fontSize: '14px',
                backgroundColor: '#f9fafb',
                borderRadius: '8px',
                margin: '0 12px',
              },
            },
            '暂无版本数据'
          ),
    ]
  );
};

// 获取版本列表并显示Notification（核心修改这里）
const showVersionNotification = () => {
  useApiStore()
    .CheckTag()
    .then((res) => {
      if (res.code === 1) {
        dyVersions.value = res.data;
        const versionLen = dyVersions.value.length;

        if (versionLen > 0) {
          const newVersions = [...dyVersions.value]; // 深拷贝避免修改原数据

          if (versionLen === 1) {
            // 只有1个版本：标记为“最新版”
            newVersions[0] = `${newVersions[0]}（最新版）`;
          } else {
            // 大于1个版本：第一个加“当前版本”，最后一个加“最新版”
            newVersions[0] = `${newVersions[0]}（当前版本）`;
            const lastIndex = versionLen - 1;
            newVersions[lastIndex] = `${newVersions[lastIndex]}（最新版）`;
          }

          dyVersions.value = newVersions;
        }
        openVersionNotification();
      } else {
        message.error(res.msg);
      }
    })
    .catch((err) => {
      message.error('获取版本列表失败');
      console.error(err);
    });
};

// 打开版本通知
const openVersionNotification = () => {
  notification.open({
    key: notificationKey.value,
    message: '', // 空消息标题
    duration: 5, // 5秒自动关闭（可改为0不自动关闭）
    placement: 'topRight',
    description: renderVersionList(),
    style: {
      width: '520px',
      minWidth: '520px',
      marginTop: '50px',
      height: 'auto',
      boxShadow: '0 4px 12px rgba(0, 0, 0, 0.08)',
      borderRadius: '8px',
      boxSizing: 'border-box',
    },
  });

  // 强制覆盖AntD默认换行样式
  setTimeout(() => {
    const notificationEl = document.querySelector('.custom-version-notification');
    if (notificationEl) {
      const descriptionEls = notificationEl.querySelectorAll('.ant-notification-notice-description');
      descriptionEls.forEach((el) => {
        const htmlEl = el as HTMLElement;
        htmlEl.style.whiteSpace = 'normal';
        htmlEl.style.width = '100%';
        htmlEl.style.height = 'auto';
        htmlEl.style.overflow = 'visible';
      });
    }
  }, 0);
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
</script>

<template>
  <StepinHeaderAction>
    <DayNightSwitch />
  </StepinHeaderAction>
  <!-- 版本按钮 -->
  <StepinHeaderAction>
    <div @click="showVersionNotification" class="action-item">
      <TagsOutlined class="action-icon" />
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

/* 样式兜底：确保每个版本项内部单行，外部纵向排列 */
:deep(.custom-version-notification) {
  .ant-notification-notice-description {
    white-space: normal !important; /* 允许版本项纵向换行 */
    width: 100% !important;
    height: auto !important;
    overflow: visible !important;
  }

  .custom-version-item {
    display: flex !important;
    align-items: center !important;
    justify-content: space-between !important;
    white-space: nowrap !important; /* 禁止版本项内部换行 */
    padding: 8px 12px !important; /* 重置内边距，去掉左侧竖线空间 */

    &:hover {
      background-color: #f5fafe !important;
    }

    &:last-child {
      border-bottom: none !important;
    }
  }

  /* 当前版本轻微高亮（可选，可删除） */
  .custom-current-version {
    background-color: rgba(24, 144, 255, 0.05) !important;

    &:hover {
      background-color: rgba(24, 144, 255, 0.1) !important;
    }
  }

  /* 按钮hover样式 */
  button:hover {
    background-color: rgba(24, 144, 255, 0.1) !important;
    color: #1890ff !important;
  }
}
</style>