<template>
  <div class="login flex items-center justify-center">
    <login-box class="shadow-lg" @success="onLoginSuccess" @failure="onLoginFail" />
  </div>
</template>

<script lang="ts" setup>
import LoginBox from './LoginBox.vue';
import { useRouter } from 'vue-router';
import { message } from 'ant-design-vue';

const router = useRouter();

function onLoginSuccess() {
  if (isMobileBrowser()) router.push('/mobile');
  else router.push('/dashboard');
}
// 精准判断移动端浏览器（复用之前的核心逻辑，简化版）
const isMobileBrowser = (): boolean => {
  if (typeof navigator === 'undefined' || typeof window === 'undefined') {
    return false;
  }
  const userAgent = navigator.userAgent.toLowerCase();
  // 匹配手机UA（排除平板）
  const mobileUA = /android|iphone|ipod|blackberry|windows phone|iemobile|opera mini/i.test(userAgent);
  const isTablet = /ipad|tablet|playbook|kindle|android 3\.|android 4\.[0-3]/.test(userAgent);
  // 触摸设备+小屏兜底
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
  min-height: -webkit-fill-available; /* 适配iOS安全区域 */
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  position: relative;
  overflow: hidden; /* 防止装饰元素溢出 */
  padding: 20px 0; /* 移动端上下内边距，避免卡片贴边 */

  // 装饰性背景元素 - 移动端适配尺寸
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

/* 适配iOS安全区域 */
@supports (bottom: env(safe-area-inset-bottom)) {
  .login {
    padding-bottom: env(safe-area-inset-bottom);
    padding-top: env(safe-area-inset-top);
  }
}
</style>