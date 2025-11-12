<template>
  <div class="login-page min-h-screen flex items-center justify-center  p-4 md:p-8">
    <ThemeProvider :color="{ 
      middle: { 'bg-base': '#f8fafc' },
      primary: { DEFAULT: '#4f46e5', hover: '#4338ca' }
    }">
      <!-- 登录卡片 -->
      <div class="login-box rounded-2xl  shadow-lg p-8 md:p-10 max-w-md w-full border border-gray-200 transition-all duration-500 hover:shadow-xl transform hover:-translate-y-1">
        <!-- 顶部Logo区域 -->
        <div class="flex flex-col items-center mb-8">
          <div class="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mb-4">
            <img src="/public/logo.png">
          </div>
          <h2 class="third-title text-2xl font-bold text-gray-800 tracking-tight">抖音同步小帮手</h2>
        </div>

        <a-form :model="form" :wrapperCol="{ span: 24 }" @finish="login" class="login-form w-full text-gray-700">
          <!-- 用户名输入框 -->
          <a-form-item :required="true" name="username" class="mb-5" :validate-status="usernameStatus">
            <a-input v-model:value="form.username" autocomplete="new-username" placeholder="请输入用户名" class="login-input h-[50px] rounded-lg bg-gray-50 border-gray-200 text-gray-800 placeholder:text-gray-400 focus:border-primary focus:bg-white text-base transition-all duration-300 focus:ring-2 focus:ring-primary/20">
              <template #prefix>
                <svg class="w-5 h-5 text-gray-400 mr-2" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                  <circle cx="12" cy="7" r="4"></circle>
                </svg>
              </template>
            </a-input>
          </a-form-item>

          <!-- 密码输入框 -->
          <a-form-item :required="true" name="password" class="mb-2" :validate-status="passwordStatus">
            <a-input v-model:value="form.password" autocomplete="new-password" placeholder="请输入密码" class="login-input h-[50px] rounded-lg bg-gray-50 border-gray-200 text-gray-800 placeholder:text-gray-400 focus:border-primary focus:bg-white text-base transition-all duration-300 focus:ring-2 focus:ring-primary/20" :type="showPassword ? 'text' : 'password'">
              <template #prefix>
                <svg class="w-5 h-5 text-gray-400 mr-2" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
                  <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
                </svg>
              </template>
              <!-- 密码显示/隐藏图标 -->
              <template #suffix>
                <EyeOutlined v-if="showPassword" @click="showPassword = !showPassword" class="text-gray-400 hover:text-primary cursor-pointer transition-colors duration-300 text-lg" aria-label="隐藏密码" />
                <EyeInvisibleOutlined v-else @click="showPassword = !showPassword" class="text-gray-400 hover:text-primary cursor-pointer transition-colors duration-300 text-lg" aria-label="显示密码" />
              </template>
            </a-input>
          </a-form-item>

          <!-- 记住密码与忘记密码 -->
          <div class="flex items-center justify-between mb-6 px-1">
            <a-form-item class="mb-0">
              <a-checkbox v-model:checked="rememberPassword" class="text-gray-600 hover:text-primary transition-colors duration-300 cursor-pointer">
                <span class="text-sm">记住密码</span>
              </a-checkbox>
            </a-form-item>
            <!-- <a href="#" class="text-primary hover:text-primary/80 text-sm font-medium transition-colors duration-300" @click.prevent="handleForgotPassword">
              忘记密码？
            </a> -->
          </div>

          <!-- 登录按钮 -->
          <a-button htmlType="submit" class="h-[52px] w-full rounded-lg transition-all duration-300 hover:bg-primary/90 bg-primary border-primary text-white text-base font-medium shadow-md hover:shadow-lg transform hover:-translate-y-0.5 active:translate-y-0" type="primary" :loading="loading">
            <span v-if="!loading">登录</span>
            <span v-else>登录中...</span>
          </a-button>

          <!-- 注册入口 -->
          <!-- <div class="text-center mt-6 text-gray-500 text-sm">
            还没有账号？
            <a href="#" class="text-primary hover:text-primary/80 font-medium transition-colors duration-300 ml-1" @click.prevent="handleRegister">
              立即注册
            </a>
          </div> -->
        </a-form>
      </div>
    </ThemeProvider>
  </div>
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
const usernameStatus = computed(() => (form.username ? 'success' : ''));
const passwordStatus = computed(() => (form.password ? 'success' : ''));

export interface LoginFormProps {
  username: string;
  password: string;
}

const form = reactive<LoginFormProps>({
  username: '',
  password: '',
});

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
    message.success('登录成功！');
  } catch (e: any) {
    // emit('failure', e.message, e.data);
    message.error(e.data.erro || '登录失败，请重试');
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
/* 页面基础样式 */
.login-page {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

/* 输入框聚焦动画优化 */
::v-deep(.ant-input:focus) {
  box-shadow: 0 0 0 2px rgba(79, 70, 229, 0.2) !important;
  border-color: #4f46e5 !important;
}

/* 复选框样式优化 */
::v-deep(.ant-checkbox-checked .ant-checkbox-inner) {
  background-color: #4f46e5 !important;
  border-color: #4f46e5 !important;
}

::v-deep(.ant-checkbox:hover .ant-checkbox-inner) {
  border-color: #4f46e5 !important;
}

/* 按钮样式优化 */
::v-deep(.ant-btn-primary) {
  background-color: #4f46e5 !important;
  border-color: #4f46e5 !important;
}

::v-deep(.ant-btn-primary:hover) {
  background-color: #4338ca !important;
  border-color: #4338ca !important;
}

::v-deep(.ant-btn-primary:focus) {
  box-shadow: 0 0 0 2px rgba(79, 70, 229, 0.3) !important;
}

/* 分割线样式优化 */
::v-deep(.ant-divider) {
  background-color: #e5e7eb !important;
}

/* 加载状态动画优化 */
::v-deep(.ant-btn-loading .ant-btn-loading-icon) {
  margin-right: 8px !important;
}

/* 响应式调整 */
@media (max-width: 375px) {
  .login-box {
    padding: 60px 20px !important;
  }

  .third-title {
    font-size: 1.5rem !important;
  }
}

/* 平滑滚动 */
html {
  scroll-behavior: smooth;
}

/* 防止点击闪烁 */
* {
  -webkit-tap-highlight-color: transparent;
}
</style>