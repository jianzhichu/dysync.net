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
const copyVersion = (version: string) => {
  const pureVersion = version.replace('（当前版本）', '').trim(); // 过滤标记
  copyLoading.value[version] = true;
  navigator.clipboard
    .writeText(pureVersion)
    .then(() => {
      message.success(`已复制版本: ${pureVersion}`);
    })
    .catch(() => {
      message.error('复制失败，请手动复制');
    })
    .finally(() => {
      copyLoading.value[version] = false;
    });
};

// 定义版本列表组件（移除所有额外当前版本标记）
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
            const isFirstWithCurrent = tag.includes('（当前版本）'); // 只通过文本判断是否为当前版本
            return h(
              'div',
              {
                class: ['custom-version-item', isFirstWithCurrent ? 'custom-current-version' : ''],
                key: index,
                title: isFirstWithCurrent ? '当前使用版本 - 点击右侧按钮复制版本号' : '点击右侧按钮复制版本号',
                style: {
                  width: '100%',
                  padding: '8px 12px',
                  borderBottom: '1px solid #f0f0f0',
                  borderRadius: '4px',
                  transition: 'all 0.2s',
                  boxSizing: 'border-box',
                  // 核心：块级Flex，强制内部（文字+按钮）一行
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  whiteSpace: 'nowrap',
                  // 只保留轻微背景色（可选，可删除）
                  backgroundColor: isFirstWithCurrent ? 'rgba(24, 144, 255, 0.05)' : 'transparent',
                },
              },
              [
                // 文本容器（单行+溢出省略）
                h(
                  'div',
                  {
                    style: {
                      display: 'flex',
                      alignItems: 'center',
                      // 给按钮留固定空间，文本超长时省略
                      maxWidth: 'calc(100% - 40px)',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap',
                    },
                  },
                  [
                    h(
                      'span',
                      {
                        style: {
                          fontSize: '14px',
                          color: isFirstWithCurrent ? '#1890ff' : '#1f2937', // 当前版本文字变色（可选，可删除）
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                          whiteSpace: 'nowrap',
                        },
                      },
                      tag // 直接显示带“（当前版本）”的文本
                    ),
                  ]
                ),
                // 复制按钮（固定大小，不挤压）
                h(
                  'button',
                  {
                    style: {
                      width: '24px',
                      height: '24px',
                      border: 'none',
                      borderRadius: '4px',
                      backgroundColor: 'transparent',
                      color: copyLoading.value[tag] ? '#d1d5db' : isFirstWithCurrent ? '#1890ff' : '#9ca3af',
                      cursor: copyLoading.value[tag] ? 'not-allowed' : 'pointer',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      padding: '0',
                      margin: '0',
                      flexShrink: '0',
                      transition: 'all 0.2s',
                    },
                    onClick: (e: Event) => {
                      e.stopPropagation();
                      copyVersion(tag);
                    },
                    disabled: copyLoading.value[tag],
                    title: copyLoading.value[tag] ? '复制中...' : '复制版本号',
                  },
                  [
                    h(CopyOutlined, {
                      style: {
                        fontSize: '14px',
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