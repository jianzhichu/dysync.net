<script lang="ts" setup>
import { LogoutOutlined } from '@ant-design/icons-vue';
import { onMounted, onUnmounted, computed } from 'vue';
import { ThemeProvider, alert } from 'stepin';
import http from '@/store/http';
import { useRouter, useRoute } from 'vue-router';

const router = useRouter();
const route = useRoute();

// 核心：精准判断是否为手机浏览器（多重校验）
const isMobileBrowser = computed(() => {
  if (typeof navigator === 'undefined' || typeof window === 'undefined') {
    return false;
  }

  const userAgent = navigator.userAgent.toLowerCase();
  // 1. 匹配手机UA特征（排除平板/桌面端）
  const mobileUA = /android|iphone|ipod|blackberry|windows phone|iemobile|opera mini/i.test(userAgent);
  const isTablet = /ipad|tablet|playbook|kindle|android 3\.|android 4\.[0-3]/.test(userAgent); // 排除平板
  // 2. 触摸设备校验（手机核心特征）
  const isTouchDevice = 'ontouchstart' in window || navigator.maxTouchPoints > 0;
  // 3. 屏幕尺寸兜底（适配响应式/模拟器）
  const isMobileScreen = window.innerWidth <= 768 && window.innerHeight <= 1024;

  // 最终判定：UA是手机 + 触摸设备 或 小屏触摸设备（覆盖所有手机场景）
  return (mobileUA && !isTablet && isTouchDevice) || (isMobileScreen && isTouchDevice);
});

// 路由守卫：手机端强制拦截所有路由，仅允许/mobile
const enforceMobileRoute = () => {
  const targetMobilePath = '/mobile';
  const currentPath = route.path.toLowerCase().trim();

  // 核心规则：手机浏览器 → 强制跳转到/mobile（无论当前路由是什么）
  if (isMobileBrowser.value) {
    if (currentPath !== targetMobilePath) {
      // 替换路由（禁止返回上一页，避免用户回退到其他路由）
      router.replace({ path: targetMobilePath, replace: true });
    }
  } else {
    // PC端：禁止访问mobile路由，强制跳转到dashboard
    if (currentPath === targetMobilePath) {
      router.replace({ path: '/dashboard', replace: true });
    }
  }
};

// 监听路由变化：确保跳转后仍拦截（防止手动输入URL）
const routerGuard = router.afterEach(() => {
  if (http.checkAuthorization()) {
    // 仅登录后生效
    enforceMobileRoute();
  }
});

onMounted(() => {
  // 1. 登录状态校验
  if (http.checkAuthorization()) {
    // 2. 初始化立即拦截路由
    enforceMobileRoute();

    // 3. 监听窗口大小变化（适配手机横屏/竖屏切换、模拟器调整尺寸）
    window.addEventListener('resize', enforceMobileRoute);
  } else {
    // 未登录先跳登录页，登录后再拦截
    router.push('/login');
  }
});

// 组件卸载：清理监听，防止内存泄漏
onUnmounted(() => {
  window.removeEventListener('resize', enforceMobileRoute);
  // 移除路由守卫
  routerGuard();
});
</script>

<template>
  <ThemeProvider :color="{ 
      middle: { 'bg-base': '#fff','bg-container':'#fff','bg-container-light':'#fff' }, 
      primary: { DEFAULT: '#1896ff' } 
    }" :autoAdapt="true">
    <div class="front-view flex flex-col" style="background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%)">
      <div class="front-content">
        <router-view />
      </div>
    </div>
  </ThemeProvider>
</template>

<style lang="less" scoped>
.front-view {
  height: 100vh;
  width: 100vw;
  overflow: hidden;

  .front-content {
    height: 100vh;
    overflow: auto; // 防止mobile页面内容溢出
  }

  // 原样式保留（无冲突）
  .front-header {
    .front-nav-item {
      &.with-list .front-nav-item-content {
        &:after {
          content: '';
          @apply ~"h-[8px]" ~"w-[8px]" transition-transform ml-2 inline-block border-text border-l-0 border-t-0 border-r-2 border-b-2 border-solid ~"rotate-[-135deg]" translate-y-1/4;
        }
        &:hover {
          &:after {
            @apply ~"rotate-[45deg]" translate-y-0;
          }
        }
      }
    }
  }
}
</style>