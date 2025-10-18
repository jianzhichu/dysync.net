<script lang="ts" setup>
import { LogoutOutlined } from '@ant-design/icons-vue';
import { onMounted } from 'vue';
import { ThemeProvider, alert } from 'stepin';
import http from '@/store/http';
import { useRouter } from 'vue-router';

const router = useRouter();
// console.log(router.getRoutes());

onMounted(() => {
  // alert.info(
  //   `<div class="text-text">
  //     Stepin is a fast, light framework to Vue3 – try it out today with the
  //     <span class="underline">Stepin Template Beta</span>.
  //   </div>`,
  //   { renderRaw: true, duration: -1 }
  // );
  // console.log(router.getRoutes());
  if (http.checkAuthorization()) {
    // console.log(22222);
    router.push('/dashboard');
  } else {
    router.push('/login');
  }
});
</script>
<template>
  <ThemeProvider :color="{ middle: { 'bg-base': '#fff','bg-container':'#fff','bg-container-light':'#fff' }, primary: { DEFAULT: '#1896ff' } }" :autoAdapt="true">
    <div class="front-view flex flex-col" style="background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%)">
      <div class="text-xxl text-text hover:text-text" style="margin-left:20px;">
        <img src="/logo1.png" />
        dy.sync.net
      </div>
      <div class="front-content ">
        <router-view />
      </div>
    </div>
  </ThemeProvider>
</template>
<style lang="less" scoped>
.front-view {
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
  .front-content {
    height: 100vh;
  }
  .front-view {
    height: 100vh;
  }
}
</style>
