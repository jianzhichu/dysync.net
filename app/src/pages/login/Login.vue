<template>
  <div class="login flex items-center justify-center">
    <login-box class="shadow-lg" @success="onLoginSuccess" @failure="onLoginFail" />
  </div>
</template>
<script lang="ts" setup>
import LoginBox from './LoginBox.vue';
import { useRouter } from 'vue-router';
// import http from '@/store/http';
const router = useRouter();

function onLoginSuccess() {
  router.push('/dashboard');
}
import { message } from 'ant-design-vue';

function onLoginFail(status, res) {
  console.log(res);
  if (res.code === 0) {
  } else {
    message.error(res.erro, 5);
  }
}
</script>
<style scoped lang="less">
.login {
  height: 100vh;
  // 与深色登录框搭配的渐变背景
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);

  // 可以添加一些装饰性背景元素
  &::before {
    content: '';
    position: absolute;
    width: 400px;
    height: 400px;
    border-radius: 50%;
    background: rgba(54, 191, 250, 0.1);
    top: 20%;
    left: 15%;
    filter: blur(80px);
  }

  &::after {
    content: '';
    position: absolute;
    width: 300px;
    height: 300px;
    border-radius: 50%;
    background: rgba(54, 191, 250, 0.08);
    bottom: 10%;
    right: 10%;
    filter: blur(60px);
  }
}
</style>
