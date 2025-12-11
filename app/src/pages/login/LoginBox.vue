<template>
  <ThemeProvider :color="{ 
    middle: { 'bg-base': '#fff' },
    primary: { DEFAULT: '#4f46e5', hover: '#4338ca' }
  }">
    <!-- 登录卡片 - 移动端全屏 + PC端保留原宽度 -->
    <div class="login-box rounded-2xl shadow-lg p-6 md:p-8 lg:p-10 max-w-md w-full border border-gray-200 transition-all duration-500 hover:shadow-xl transform hover:-translate-y-1 mx-auto" style="box-sizing: border-box; max-width: 90vw;">
      <!-- 顶部Logo区域 - 边框圈精准贴合Logo内容 -->
      <div class="flex flex-col items-center mb-6 md:mb-8">
        <div class="w-30 h-30 md:w-36 md:h-36 bg-primary/10 rounded-full flex items-center justify-center mb-4 md:mb-5">
          <!-- 缩小边框容器尺寸，适配img实际内容 style="border: 1.5px solid rgba(79, 70, 229, 0.2); border-radius: 50%;"-->
          <div class="w-24 h-24 md:w-28 h-28 rounded-full flex items-center justify-center">
            <img src="/logo.png" alt="抖小云logo" class="w-18 h-18 md:w-20 h-20 object-contain" />
          </div>
        </div>
        <h2 class="third-title text-xl md:text-2xl font-bold text-gray-800 tracking-tight">抖小云</h2>
      </div>

      <a-form :model="form" :wrapperCol="{ span: 24 }" @finish="login" class="login-form w-full text-gray-700">
        <!-- 用户名输入框 -->
        <a-form-item :required="true" name="username" class="mb-4 md:mb-5" :validate-status="usernameStatus">
          <a-input v-model:value="form.username" autocomplete="new-username" placeholder="请输入用户名" class="login-input h-[46px] md:h-[50px] rounded-lg bg-gray-50 border-gray-200 text-gray-800 placeholder:text-gray-400 focus:border-primary focus:bg-white text-base transition-all duration-300 focus:ring-2 focus:ring-primary/20" style="padding: 0 12px;">
            <template #prefix>
              <svg class="w-4 h-4 md:w-5 md:h-5 text-gray-400 mr-2" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                <circle cx="12" cy="7" r="4"></circle>
              </svg>
            </template>
          </a-input>
        </a-form-item>

        <!-- 密码输入框 -->
        <a-form-item :required="true" name="password" class="mb-2" :validate-status="passwordStatus">
          <a-input v-model:value="form.password" autocomplete="new-password" placeholder="请输入密码" class="login-input h-[46px] md:h-[50px] rounded-lg bg-gray-50 border-gray-200 text-gray-800 placeholder:text-gray-400 focus:border-primary focus:bg-white text-base transition-all duration-300 focus:ring-2 focus:ring-primary/20" :type="showPassword ? 'text' : 'password'" style="padding: 0 12px;">
            <template #prefix>
              <svg class="w-4 h-4 md:w-5 md:h-5 text-gray-400 mr-2" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
                <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
              </svg>
            </template>
            <!-- 密码显示/隐藏图标 -->
            <template #suffix>
              <div class="w-8 h-8 flex items-center justify-center" @click="showPassword = !showPassword">
                <EyeOutlined v-if="showPassword" class="text-gray-400 hover:text-primary cursor-pointer transition-colors duration-300 text-lg" aria-label="隐藏密码" />
                <EyeInvisibleOutlined v-else class="text-gray-400 hover:text-primary cursor-pointer transition-colors duration-300 text-lg" aria-label="显示密码" />
              </div>
            </template>
          </a-input>
        </a-form-item>

        <!-- 记住密码 -->
        <div class="flex items-center justify-between mb-5 md:mb-6 px-1">
          <a-form-item class="mb-0">
            <a-checkbox v-model:checked="rememberPassword" class="text-gray-600 hover:text-primary transition-colors duration-300 cursor-pointer">
              <span class="text-xs md:text-sm">记住密码</span>
            </a-checkbox>
          </a-form-item>
        </div>

        <!-- 登录按钮 -->
        <a-button htmlType="submit" class="h-[48px] md:h-[52px] w-full rounded-lg transition-all duration-300 hover:bg-primary/90 bg-primary border-primary text-white text-base font-medium shadow-md hover:shadow-lg transform hover:-translate-y-0.5 active:translate-y-0" type="primary" :loading="loading" style="touch-action: manipulation; -webkit-tap-highlight-color: transparent;">
          <span v-if="!loading">登录</span>
          <span v-else>登录中...</span>
        </a-button>
      </a-form>
    </div>
  </ThemeProvider>
</template>

<script lang="ts" setup>
import { reactive, ref, onMounted, computed } from 'vue';
import { useAccountStore } from '@/store';
import { ThemeProvider } from 'stepin';
import { EyeOutlined, EyeInvisibleOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';

// 控制密码显示/隐藏的状态
const showPassword = ref(false);
// 记住密码状态
const rememberPassword = ref(false);
// 加载状态
const loading = ref(false);

// 表单状态（用于输入框验证反馈）
const form = reactive({
  username: '',
  password: '',
});
const usernameStatus = computed(() => (form.username ? 'success' : ''));
const passwordStatus = computed(() => (form.password ? 'success' : ''));

export interface LoginFormProps {
  username: string;
  password: string;
}

const emit = defineEmits<{
  (e: 'success', fields: LoginFormProps): void;
  (e: 'failure', reason: string, fields: LoginFormProps): void;
  (e: 'register'): void;
  (e: 'forgot-password'): void;
}>();

const accountStore = useAccountStore();

// 页面加载时读取本地存储的记住密码信息
onMounted(() => {
  const savedUser = localStorage.getItem('rememberedUser');
  if (savedUser) {
    try {
      const { username, password } = JSON.parse(savedUser);
      form.username = username;
      form.password = password;
      rememberPassword.value = true;
    } catch (e) {
      console.error('读取保存的用户信息失败', e);
      localStorage.removeItem('rememberedUser');
    }
  }
});

// 登录处理
async function login(params: LoginFormProps) {
  // 简单表单验证
  if (!params.username) {
    message.warning('请输入用户名');
    return;
  }
  if (!params.password) {
    message.warning('请输入密码');
    return;
  }

  loading.value = true;

  // 根据记住密码状态保存/清除用户信息
  if (rememberPassword.value) {
    localStorage.setItem(
      'rememberedUser',
      JSON.stringify({
        username: params.username,
        password: params.password,
      })
    );
  } else {
    localStorage.removeItem('rememberedUser');
  }

  try {
    const res = await accountStore.login(params.username, params.password);
    emit('success', params);
    console.log(res);
    message.success('登录成功！');
  } catch (e: any) {
    emit('failure', e.message || '登录失败', params);
    message.error(e.data?.erro || '登录失败，请重试');
  } finally {
    loading.value = false;
  }
}

// 忘记密码处理
function handleForgotPassword() {
  emit('forgot-password');
  message.info('忘记密码功能即将上线');
}

// 注册处理
function handleRegister() {
  emit('register');
}
</script>

<style scoped>
/* 全局布局适配 - 替代原外层容器的样式 */
:root {
  -webkit-text-size-adjust: 100%; /* 禁止iOS文本缩放 */
  -webkit-font-smoothing: antialiased; /* 优化字体渲染 */
}

body {
  margin: 0;
  padding: 0;
  overflow-x: hidden;
  min-height: 100vh;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  /* 全局居中布局 */
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 4px;
}

/* 登录卡片核心样式 */
.login-box {
  box-sizing: border-box;
  touch-action: manipulation; /* 优化触摸性能 */
  /* 移动端全屏，PC端保留原max-w-md（默认28rem/448px）+ 90vw限制 */
}

/* 移动端适配（<768px） */
@media (max-width: 767.98px) {
  body {
    padding: 0;
  }
  .login-box {
    max-width: 100vw !important; /* 移动端全屏 */
    min-height: 100vh;
    border: none !important;
    border-radius: 0 !important;
    box-shadow: none !important;
    padding: 20px 16px !important;
    padding-top: 20vh !important; /* 移动端垂直居中 */
  }
  .third-title {
    font-size: 1.25rem !important;
  }
  ::v-deep(.ant-input) {
    font-size: 14px; /* 移动端字体适配 */
  }
}

/* 小屏手机额外适配 */
@media (max-width: 480px) {
  .login-box {
    padding-top: 15vh !important;
  }
}

/* PC端样式（保留原有宽度，仅调小最大宽度） */
@media (min-width: 768px) {
  body {
    padding: 8px;
  }
  .login-box {
    /* 原宽度是 min(448px, 90vw)，现调整为更小的数值，比如 380px（可根据需求修改） */
    max-width: min(340px, 90vw) !important;
    min-height: auto !important;
    border: 1px solid #e5e7eb !important;
    border-radius: 1rem !important;
    box-shadow: 0 10px 15px -3px rgb(0 0 0 / 0.1) !important;
    padding-top: 0 !important;
  }
}

/* 输入框聚焦动画优化 */
::v-deep(.ant-input) {
  box-sizing: border-box;
}
::v-deep(.ant-input:focus) {
  box-shadow: 0 0 0 2px rgba(79, 70, 229, 0.2) !important;
  border-color: #4f46e5 !important;
  outline: none; /* 移除默认轮廓 */
}

/* 复选框样式优化 */
::v-deep(.ant-checkbox-checked .ant-checkbox-inner) {
  background-color: #4f46e5 !important;
  border-color: #4f46e5 !important;
}
::v-deep(.ant-checkbox:hover .ant-checkbox-inner) {
  border-color: #4f46e5 !important;
}
::v-deep(.ant-checkbox) {
  transform: scale(0.9); /* 移动端复选框适当缩小 */
}

/* 按钮样式优化 */
::v-deep(.ant-btn-primary) {
  background-color: #4f46e5 !important;
  border-color: #4f46e5 !important;
  box-sizing: border-box;
}
::v-deep(.ant-btn-primary:hover) {
  background-color: #4338ca !important;
  border-color: #4338ca !important;
}
::v-deep(.ant-btn-primary:focus) {
  box-shadow: 0 0 0 2px rgba(79, 70, 229, 0.3) !important;
  outline: none;
}

/* 加载状态动画优化 */
::v-deep(.ant-btn-loading .ant-btn-loading-icon) {
  margin-right: 8px !important;
}

/* 防止点击闪烁和触摸高亮 */
* {
  -webkit-tap-highlight-color: transparent;
  tap-highlight-color: transparent;
}

/* 平滑滚动 */
html {
  scroll-behavior: smooth;
  overflow-x: hidden; /* 防止移动端横向滚动 */
}
</style>