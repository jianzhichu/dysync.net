<script lang="ts" setup>
import { LogoutOutlined } from '@ant-design/icons-vue';
import { onMounted, onUnmounted, computed } from 'vue';
import { ThemeProvider, alert } from 'stepin';
import http from '@/store/http';
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