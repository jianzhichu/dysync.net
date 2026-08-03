<template>
  <div class="login-page">
    <div class="room-scene" aria-hidden="true">
      <div class="ambient ambient-left"></div>
      <div class="ambient ambient-right"></div>

      <div class="nas-corner">
        <div class="shelf-line"></div>
        <div class="nas-device">
          <span class="nas-led nas-led-green"></span>
          <span class="nas-led nas-led-blue"></span>
          <span class="nas-slot nas-slot-1"></span>
          <span class="nas-slot nas-slot-2"></span>
          <span class="nas-slot nas-slot-3"></span>
          <span class="nas-slot nas-slot-4"></span>
        </div>
      </div>

      <div class="video-corner">
        <div class="video-mark">♪</div>
        <div class="shelf-line"></div>
      </div>
    </div>

    <login-box class="login-panel" @success="onLoginSuccess" @failure="onLoginFail" />

    <div class="login-page-tip">NAS · 抖音同步助手</div>
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
.login-page {
  position: fixed;
  inset: 0;
  z-index: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  box-sizing: border-box;
  width: 100%;
  height: 100vh;
  height: 100dvh;
  padding: 28px;
  overflow: hidden;
  color: #f8fafc;
  background: radial-gradient(circle at 50% 18%, rgba(41, 98, 171, 0.2), transparent 34%),
    radial-gradient(circle at 50% 88%, rgba(23, 77, 139, 0.22), transparent 38%),
    linear-gradient(180deg, #07111f 0%, #081426 54%, #091426 100%);

  &::before {
    content: '';
    position: absolute;
    inset: 0;
    z-index: -3;
    pointer-events: none;
    opacity: 0.62;
    background-image: radial-gradient(circle at 6% 12%, rgba(255, 255, 255, 0.62) 0 1px, transparent 1.8px),
      radial-gradient(circle at 16% 34%, rgba(74, 162, 255, 0.82) 0 1px, transparent 2.2px),
      radial-gradient(circle at 26% 8%, rgba(255, 255, 255, 0.44) 0 1px, transparent 1.7px),
      radial-gradient(circle at 38% 26%, rgba(255, 255, 255, 0.38) 0 1px, transparent 1.8px),
      radial-gradient(circle at 63% 12%, rgba(255, 255, 255, 0.54) 0 1px, transparent 1.8px),
      radial-gradient(circle at 76% 31%, rgba(58, 153, 255, 0.7) 0 1.2px, transparent 2.3px),
      radial-gradient(circle at 88% 10%, rgba(255, 255, 255, 0.5) 0 1px, transparent 1.8px),
      radial-gradient(circle at 94% 42%, rgba(255, 255, 255, 0.4) 0 1px, transparent 1.8px);
  }

  &::after {
    content: '';
    position: absolute;
    left: 50%;
    bottom: -190px;
    z-index: -2;
    width: min(860px, 78vw);
    height: 330px;
    pointer-events: none;
    border-radius: 50%;
    transform: translateX(-50%);
    background: radial-gradient(ellipse at center, rgba(28, 103, 183, 0.23), rgba(8, 20, 38, 0) 68%);
    filter: blur(10px);
  }
}

.room-scene {
  position: absolute;
  inset: 0;
  z-index: -1;
  pointer-events: none;
  overflow: hidden;
}

.ambient {
  position: absolute;
  border-radius: 999px;
  filter: blur(90px);
  opacity: 0.18;
}

.ambient-left {
  left: -120px;
  bottom: 70px;
  width: 420px;
  height: 250px;
  background: #d08b3f;
}

.ambient-right {
  right: -120px;
  bottom: 50px;
  width: 440px;
  height: 260px;
  background: #1b64ba;
}

.nas-corner,
.video-corner {
  position: absolute;
  bottom: 9vh;
  width: 270px;
  height: 180px;
  opacity: 0.48;
  filter: saturate(0.88);
}

.nas-corner {
  left: 7vw;
}

.video-corner {
  right: 7vw;
}

.shelf-line {
  position: absolute;
  left: 0;
  right: 0;
  bottom: 22px;
  height: 2px;
  background: linear-gradient(90deg, transparent, rgba(118, 146, 180, 0.22), transparent);
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.32);
}

.nas-device {
  position: absolute;
  left: 46px;
  bottom: 24px;
  width: 148px;
  height: 102px;
  border: 1px solid rgba(137, 165, 196, 0.2);
  border-radius: 12px;
  background: linear-gradient(145deg, rgba(29, 44, 64, 0.86), rgba(10, 18, 31, 0.9));
  box-shadow: 0 20px 48px rgba(0, 0, 0, 0.44);
}

.nas-slot {
  position: absolute;
  top: 14px;
  bottom: 14px;
  width: 23px;
  border: 1px solid rgba(117, 146, 178, 0.14);
  border-radius: 4px;
  background: linear-gradient(180deg, rgba(49, 67, 89, 0.68), rgba(15, 25, 39, 0.9));
}

.nas-slot-1 {
  left: 18px;
}
.nas-slot-2 {
  left: 47px;
}
.nas-slot-3 {
  left: 76px;
}
.nas-slot-4 {
  left: 105px;
}

.nas-led {
  position: absolute;
  z-index: 2;
  right: 8px;
  width: 3px;
  height: 3px;
  border-radius: 50%;
  box-shadow: 0 0 7px currentColor;
}

.nas-led-green {
  top: 18px;
  color: #4ade80;
  background: currentColor;
}

.nas-led-blue {
  top: 29px;
  color: #3b82f6;
  background: currentColor;
}

.video-mark {
  position: absolute;
  right: 44px;
  bottom: 38px;
  display: grid;
  place-items: center;
  width: 66px;
  height: 66px;
  border: 1px solid rgba(245, 178, 86, 0.2);
  border-radius: 12px;
  color: rgba(255, 190, 103, 0.74);
  font-size: 38px;
  font-weight: 700;
  background: rgba(21, 28, 41, 0.64);
  box-shadow: 0 14px 42px rgba(0, 0, 0, 0.4);
}

.login-panel {
  position: relative;
  z-index: 2;
}

.login-page-tip {
  position: absolute;
  left: 50%;
  bottom: 18px;
  z-index: 2;
  transform: translateX(-50%);
  color: rgba(148, 163, 184, 0.52);
  font-size: 12px;
  letter-spacing: 0.14em;
  white-space: nowrap;
}

@media (max-width: 980px) {
  .nas-corner,
  .video-corner {
    display: none;
  }
}

@media (max-width: 767.98px) {
  .login-page {
    padding: 16px;
    background: radial-gradient(circle at 50% 18%, rgba(42, 112, 190, 0.2), transparent 38%),
      linear-gradient(180deg, #07111f 0%, #09172a 100%);
  }

  .login-page-tip {
    bottom: 10px;
    font-size: 10px;
  }
}

@media (max-height: 700px) and (min-width: 768px) {
  .login-page {
    padding: 12px;
  }

  .login-page-tip {
    display: none;
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
