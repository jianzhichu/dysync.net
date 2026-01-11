<template>
  <div class="login flex items-center justify-center">
    <login-box class="shadow-lg" @success="onLoginSuccess" @failure="onLoginFail" />
  </div>
</template>

<script lang="ts" setup>
import LoginBox from './LoginBox.vue';
import { useRouter } from 'vue-router';
import { message, notification, Modal } from 'ant-design-vue'; // 引入 Modal 用于确认提示
import { onMounted, ref, h } from 'vue';
import { useApiStore } from '@/store';
import { CopyOutlined } from '@ant-design/icons-vue';

const router = useRouter();

// 版本数据存储
const currentTag = ref<string>('');
const latestTag = ref<string>('未知版本');

// 复制版本号到剪贴板核心逻辑
const copyToClipboard = async (content: string, type: string) => {
  if (!content || content === '未知版本') {
    message.warning(`${type}无有效内容可复制`);
    return;
  }

  try {
    if (navigator.clipboard && window.isSecureContext) {
      await navigator.clipboard.writeText(content);
      message.success(`${type}已复制到剪贴板 ✅`);
    } else {
      const textarea = document.createElement('textarea');
      textarea.value = content;
      textarea.style.position = 'fixed';
      textarea.style.left = '-9999px';
      textarea.style.top = '-9999px';
      document.body.appendChild(textarea);
      textarea.select();
      document.execCommand('copy');
      document.body.removeChild(textarea);
      message.success(`${type}已复制到剪贴板 ✅`);
    }
  } catch (err) {
    console.error('复制失败：', err);
    message.error('复制失败，请手动选中复制');
  }
};

// 统一的关闭处理：标记缓存 + 关闭通知
const handleNoticeClose = (noticeKey: string) => {
  console.log('版本提醒通知已关闭，后续不再提醒');
  // 标记为已提醒，存入缓存（确保版本通知和确认弹窗都不再出现）
  localStorage.setItem('maintain_notice_shown', 'true');
  // 关闭版本通知
  notification.close(noticeKey);
};

// 关闭前的确认提示弹窗
const showCloseConfirm = (noticeKey: string) => {
  Modal.confirm({
    title: '确认关闭',
    content: '关闭后该提醒将不再弹出，确定要关闭吗？',
    okText: '确定',
    cancelText: '取消',
    onOk: () => {
      // 确认关闭：执行统一处理
      handleNoticeClose(noticeKey);
      message.success('提醒已关闭，后续不再弹出');
    },
    onCancel: () => {
      // 取消关闭：不执行任何操作，保留版本通知
      message.info('已取消关闭');
    },
  });
};

// 打开右上角 notification 通知
const openVersionNotice = () => {
  const noticeKey = `version_notice_${Date.now()}`;
  notification.open({
    message: '温馨提示一下',
    key: noticeKey,
    duration: 0,
    placement: 'topRight',
    // 通知描述内容
    description: h('div', { class: 'notice-content' }, [
      h('p', { class: 'notice-desc' }, '当前您使用的docker镜像为阿里云镜像，已停止维护。'),
      h('p', { class: 'notice-version-item' }, [
        h('strong', { class: 'notice-version-label' }, '当前版本：'),
        h('span', { style: { color: '#ff4d4f', fontWeight: '500' } }, currentTag.value.replace('[不再维护]', '')),
        h(CopyOutlined, {
          class: 'notice-copy-icon',
          title: '复制当前版本',
          onClick: () => copyToClipboard(currentTag.value.replace('[不再维护]', ''), '当前版本'),
        }),
      ]),
      h('p', { class: 'notice-version-item' }, [
        h('strong', { class: 'notice-version-label' }, '最新版本：'),
        h('span', { style: { color: '#52c41a', fontWeight: '500' } }, latestTag.value),
        h(CopyOutlined, {
          class: 'notice-copy-icon',
          title: '复制最新版本',
          onClick: () => copyToClipboard(latestTag.value, '最新版本'),
        }),
      ]),
      h('p', { class: 'notice-tip' }, '建议升级到最新版本！'),
    ]),
    // 「我已知晓」按钮：点击触发确认弹窗
    btn: () =>
      h(
        'button',
        {
          class: 'notice-confirm-btn',
          onClick: () => {
            // 不直接关闭，先弹出确认提示
            showCloseConfirm(noticeKey);
          },
        },
        '我已知晓'
      ),
  });
};

onMounted(() => {
  useApiStore()
    .AppisInit()
    .then((res) => {
      if (res.code == 0 && res.data) {
      } else {
        router.push('/init');
      }
    });

  useApiStore()
    .CheckTag()
    .then((res) => {
      if (res.code === 0) {
        if (res.data.length > 0) {
          const tag = res.data[0];
          if (tag.indexOf('不再维护') !== -1) {
            const hasShown = localStorage.getItem('maintain_notice_shown');
            if (!hasShown) {
              currentTag.value = tag;
              latestTag.value = res.data.length >= 2 ? res.data[1] : '未知版本';
              openVersionNotice();
            }
          }
        }
      } else {
        message.error(res.message);
      }
    })
    .catch((err) => {
      console.error(err);
    });
});

function onLoginSuccess() {
  if (isMobileBrowser()) router.push('/mobile');
  else router.push('/dashboard');
}

const isMobileBrowser = (): boolean => {
  if (typeof navigator === 'undefined' || typeof window === 'undefined') {
    return false;
  }
  const userAgent = navigator.userAgent.toLowerCase();
  const mobileUA = /android|iphone|ipod|blackberry|windows phone|iemobile|opera mini/i.test(userAgent);
  const isTablet = /ipad|tablet|playbook|kindle|android 3\.|android 4\.[0-3]/.test(userAgent);
  const isTouchDevice = 'ontouchstart' in window || navigator.maxTouchPoints > 0;
  const isMobileScreen = window.innerWidth <= 768 && window.innerHeight <= 1024;

  return (mobileUA && !isTablet && isTouchDevice) || (isMobileScreen && isTouchDevice);
};

function onLoginFail(reason: string, fields: any) {
  console.log('登录失败:', reason, fields);
  message.error(reason || '登录失败，请重试', 5);
}
</script>

<style scoped lang="less">
.login {
  height: 100vh;
  min-height: -webkit-fill-available;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  position: relative;
  overflow: hidden;
  padding: 20px 0;

  &::before {
    content: '';
    position: absolute;
    width: 280px;
    height: 280px;
    border-radius: 50%;
    background: rgba(54, 191, 250, 0.1);
    top: 15%;
    left: 5%;
    filter: blur(60px);
    @media (min-width: 768px) {
      width: 400px;
      height: 400px;
      top: 20%;
      left: 15%;
      filter: blur(80px);
    }
  }

  &::after {
    content: '';
    position: absolute;
    width: 220px;
    height: 220px;
    border-radius: 50%;
    background: rgba(54, 191, 250, 0.08);
    bottom: 5%;
    right: 5%;
    filter: blur(40px);
    @media (min-width: 768px) {
      width: 300px;
      height: 300px;
      bottom: 10%;
      right: 10%;
      filter: blur(60px);
    }
  }
}

@supports (bottom: env(safe-area-inset-bottom)) {
  .login {
    padding-bottom: env(safe-area-inset-bottom);
    padding-top: env(safe-area-inset-top);
  }
}
</style>

<style lang="less">
.ant-notification {
  .ant-notification-notice {
    width: 500px !important;
    padding: 16px !important;
  }

  .notice-content {
    font-size: 14px !important;
    line-height: 1.8 !important;
    color: #334155 !important;
    width: 480px !important;

    .notice-desc {
      margin-bottom: 12px !important;
      padding-left: 2px !important;
      margin: 0 !important;
    }

    .notice-version-item {
      display: flex !important;
      align-items: center !important;
      margin: 8px 0 !important;
      padding: 6px 10px !important;
      background-color: #f8fafc !important;
      border-radius: 6px !important;
      transition: background-color 0.2s ease !important;

      &:hover {
        background-color: #f1f5f9 !important;
      }
    }

    .notice-version-label {
      color: #1e293b !important;
      width: 70px !important;
      flex-shrink: 0 !important;
      font-size: 13px !important;
    }

    .notice-copy-icon {
      flex-shrink: 0 !important;
      color: #165dff !important;
      font-size: 16px !important;
      cursor: pointer !important;
      transition: all 0.2s ease !important;
      margin-left: 8px !important;

      &:hover {
        color: #0d47a1 !important;
        transform: scale(1.1) !important;
      }

      &:active {
        transform: scale(0.95) !important;
      }
    }

    .notice-tip {
      margin-top: 12px !important;
      color: #64748b !important;
      padding-left: 2px !important;
      font-style: italic !important;
      font-size: 13px !important;
      margin: 0 !important;
    }
  }

  .notice-confirm-btn {
    background-color: #165dff !important;
    color: #ffffff !important;
    border: none !important;
    border-radius: 4px !important;
    padding: 4px 12px !important;
    font-size: 13px !important;
    cursor: pointer !important;
    transition: background-color 0.2s ease !important;
    margin-top: 12px !important;

    &:hover {
      background-color: #0d47a1 !important;
    }
  }
}
</style>